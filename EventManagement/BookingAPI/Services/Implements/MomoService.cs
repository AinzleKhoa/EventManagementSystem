using BookingAPI.Models;
using BookingAPI.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json; // BẮT BUỘC ĐỂ GỌI SANG NOTIFICATION API
using Microsoft.Extensions.Configuration; // BẮT BUỘC ĐỂ ĐỌC CONFIG

namespace BookingAPI.Services.Implements
{
    public class MomoService : IMomoService
    {
        private readonly MomoConfig _config;
        private readonly IOrderRepository _repository;
        private readonly IOrderService _orderService;
        private readonly HttpClient _httpClient;
        private readonly string _gatewayUrl; 

        public MomoService(
            IOptions<MomoConfig> config,
            IOrderRepository repository,
            IOrderService orderService,
            HttpClient httpClient,
            IConfiguration appConfig) 
        {
            _config = config.Value;
            _repository = repository;
            _orderService = orderService;
            _httpClient = httpClient;
            _gatewayUrl = appConfig["GatewayAPI:Url"] ?? "https://localhost:7158";
        }

        public bool ValidateSignature(IQueryCollection query)
        {
            var partnerCode = query["partnerCode"].ToString();
            var accessKey = _config.AccessKey;
            var requestId = query["requestId"].ToString();
            var amount = query["amount"].ToString();
            var orderId = query["orderId"].ToString();
            var orderInfo = query["orderInfo"].ToString();
            var orderType = query["orderType"].ToString();
            var transId = query["transId"].ToString();
            var message = query["message"].ToString();
            var localMessage = query["localMessage"].ToString();
            var responseTime = query["responseTime"].ToString();
            var resultCode = query["resultCode"].ToString();
            if (string.IsNullOrEmpty(resultCode)) resultCode = query["errorCode"].ToString();
            var payType = query["payType"].ToString();
            var extraData = query["extraData"].ToString();
            var momoSignature = query["signature"].ToString();

            var rawHash =
                $"partnerCode={partnerCode}" +
                $"&accessKey={accessKey}" +
                $"&requestId={requestId}" +
                $"&amount={amount}" +
                $"&orderId={orderId}" +
                $"&orderInfo={orderInfo}" +
                $"&orderType={orderType}" +
                $"&transId={transId}" +
                $"&message={message}" +
                $"&localMessage={localMessage}" +
                $"&responseTime={responseTime}" +
                $"&errorCode={resultCode}" +
                $"&payType={payType}" +
                $"&extraData={extraData}";

            var computedSignature = HmacSHA256(_config.SecretKey, rawHash);

            Console.WriteLine("IPN RAW HASH: " + rawHash);
            Console.WriteLine("IPN SIGNATURE MOMO: " + momoSignature);
            Console.WriteLine("IPN SIGNATURE LOCAL: " + computedSignature);

            return computedSignature.Equals(momoSignature, StringComparison.OrdinalIgnoreCase);
        }

        public async Task<string> CreatePaymentUrl(long orderId, int amount)
        {
            var requestId = DateTime.UtcNow.Ticks.ToString();
            var momoOrderId = requestId;

            var orderInfo = $"Payment_for_order_{orderId}";
            var extraData = orderId.ToString(); 

            var rawHash =
                $"partnerCode={_config.PartnerCode}" +
                $"&accessKey={_config.AccessKey}" +
                $"&requestId={requestId}" +
                $"&amount={amount}" +
                $"&orderId={momoOrderId}" +
                $"&orderInfo={orderInfo}" +
                $"&returnUrl={_config.ReturnUrl}" +
                $"&notifyUrl={_config.NotifyUrl}" +
                $"&extraData={extraData}";

            var signature = HmacSHA256(_config.SecretKey, rawHash);

            var requestBody = new
            {
                accessKey = _config.AccessKey,
                partnerCode = _config.PartnerCode,
                requestType = _config.RequestType,
                notifyUrl = _config.NotifyUrl,
                returnUrl = _config.ReturnUrl,
                orderId = momoOrderId,
                amount = amount.ToString(),
                orderInfo = orderInfo,
                requestId = requestId,
                extraData = extraData,
                signature = signature
            };

            var json = JsonSerializer.Serialize(requestBody);

            Console.WriteLine("MoMo RAW HASH: " + rawHash);
            Console.WriteLine("MoMo REQUEST: " + json);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_config.MomoApiUrl, content);

            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine("MoMo RESPONSE: " + responseContent);

            var momoResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (momoResponse.TryGetProperty("payUrl", out var payUrl))
            {
                return payUrl.GetString();
            }

            return null;
        }

        public async Task<string> HandleMomoReturn(IQueryCollection query)
        {
            if (!ValidateSignature(query))
                return "Invalid signature";

            var resultCode = query["resultCode"].ToString();
            if (string.IsNullOrEmpty(resultCode)) resultCode = query["errorCode"].ToString();
            var extraData = query["extraData"].ToString();
            var transId = query["transId"].ToString();

            if (!long.TryParse(extraData, out long orderId))
                return "Invalid order id";

            var order = await _repository.GetById((int)orderId);

            if (order == null)
                return "Order not found";

            if (order.Status == "Paid")
                return "Order already paid";
            if (order.Status == "Cancelled")
                return "Cancelled";

            if (resultCode == "0")
            {
              
                await SendPaymentSuccessNotification(order.CustomerId, order.EventName, order.FinalAmount);

                return await _orderService.HandleOrderReturn(order, "Momo", transId);
            }

            if (resultCode == "1006" || resultCode == "42")
            {
                if (order.Status == "Pending")
                {
                    await _orderService.CancelOrderAsync(order.OrderId);

                    await SendPaymentFailedNotification(order.CustomerId, order.EventName);
                }
                return "Cancelled";
            }

            return "Payment failed";
        }

        private string HmacSHA256(string key, string rawData)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var rawBytes = Encoding.UTF8.GetBytes(rawData);

            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(rawBytes);

            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

      
        private async Task SendPaymentSuccessNotification(int userId, string eventName, long amount)
        {
            try
            {
                var notifPayload = new
                {
                    UserId = userId,
                    Title = "Payment Successful!",
                    Message = $"Your order for {eventName} ({amount:N0} VND) has been paid successfully via MoMo. Your tickets are ready!",
                    Type = "Order"
                };

                var url = $"{_gatewayUrl.TrimEnd('/')}/notification-gateway/Notifications";
                var response = await _httpClient.PostAsJsonAsync(url, notifPayload);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error sending MoMo payment success notification: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception sending notification: {ex.Message}");
            }
        }

        private async Task SendPaymentFailedNotification(int userId, string eventName)
        {
            try
            {
                var notifPayload = new
                {
                    UserId = userId,
                    Title = "Payment Failed",
                    Message = $"Your ticket order for {eventName} has been cancelled due to an unsuccessful MoMo payment.",
                    Type = "System"
                };

                var url = $"{_gatewayUrl.TrimEnd('/')}/notification-gateway/Notifications";
                await _httpClient.PostAsJsonAsync(url, notifPayload);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception sending cancellation notification: {ex.Message}");
            }
        }
    }
}