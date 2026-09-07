using BookingAPI.Models;
using BookingAPI.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http.Json; // BẮT BUỘC ĐỂ GỌI SANG NOTIFICATION API

namespace BookingAPI.Services.Implements
{
    public class VnPayService : IVnPayService
    {
        private readonly VnPayConfig _config;
        private readonly IOrderRepository _repository;
        private readonly IOrderService _orderService;
        private readonly HttpClient _http; // THÊM HTTP CLIENT ĐỂ GỌI CHÉO API

        public VnPayService(
            IOptions<VnPayConfig> config,
            IOrderRepository repository,
            IOrderService orderService,
            IHttpClientFactory factory, // Tiêm IHttpClientFactory vào đây
            IConfiguration appConfig) // Tiêm Configuration để lấy URL Gateway
        {
            _config = config.Value;
            _repository = repository;
            _orderService = orderService;
            _http = factory.CreateClient();

            // Cấu hình BaseAddress chỉ thẳng vào Gateway (hoặc NotificationAPI)
            _http.BaseAddress = new Uri(appConfig["GatewayAPI:Url"] ?? "https://localhost:7158");
        }

        public string CreatePaymentUrl(long orderId, int amount, string ipAddress)
        {
            var vnpUrl = _config.BaseUrl;
            var tmnCode = _config.TmnCode;
            var hashSecret = _config.HashSecret;
            var returnUrl = _config.ReturnUrl;

            var vnpayData = new SortedDictionary<string, string>
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", tmnCode },
                { "vnp_Amount", (amount * 100).ToString() }, // 🔥 bắt buộc nhân 100
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
                { "vnp_CurrCode", "VND" },
                { "vnp_IpAddr", ipAddress ?? "127.0.0.1" },
                { "vnp_Locale", "en" },
                { "vnp_OrderInfo", $"Thanh toan don hang {orderId}" },
                { "vnp_OrderType", "other" },
                { "vnp_ReturnUrl", returnUrl },
                { "vnp_TxnRef", orderId.ToString() }
            };

            // ====== Build query đã URL encode ======
            var query = string.Join("&",
                vnpayData.Select(kvp =>
                    $"{kvp.Key}={System.Net.WebUtility.UrlEncode(kvp.Value)}"
                )
            );

            // ====== Ký trên chuỗi đã encode ======
            var secureHash = HmacSHA512(hashSecret, query);

            return $"{vnpUrl}?{query}&vnp_SecureHash={secureHash}";
        }

        public bool ValidateSignature(IQueryCollection query, string rawQuery)
        {
            var hashSecret = _config.HashSecret;
            var vnp_SecureHash = query["vnp_SecureHash"].ToString();

            var queryString = rawQuery.TrimStart('?');

            var filtered = string.Join("&",
                queryString.Split('&')
                    .Where(x => !x.StartsWith("vnp_SecureHash"))
                    .OrderBy(x => x)
            );

            var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(hashSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(filtered));

            var computedHash = BitConverter.ToString(hash)
                .Replace("-", "")
                .ToLower();

            return computedHash.Equals(vnp_SecureHash, StringComparison.OrdinalIgnoreCase);
        }

        private string HmacSHA512(string key, string inputData)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);

            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                return BitConverter.ToString(hashValue)
                    .Replace("-", "")
                    .ToLower();
            }
        }

        public async Task<string> HandleVnPayReturn(IQueryCollection query, string rawQuery)
        {
            if (!ValidateSignature(query, rawQuery))
                return "Invalid signature";

            var responseCode = query["vnp_ResponseCode"].ToString();
            var transactionStatus = query["vnp_TransactionStatus"].ToString();
            var orderIdStr = query["vnp_TxnRef"].ToString();

            if (!int.TryParse(orderIdStr, out int orderId))
                return "Invalid order id";

            var order = await _repository.GetById(orderId);

            if (order == null)
                return "Order not found";

            // ======= XỬ LÝ THANH TOÁN THÀNH CÔNG =======
            if (responseCode == "00" && transactionStatus == "00")
            {
    
                await SendPaymentSuccessNotification(order.CustomerId, order.EventName, order.FinalAmount);

                return await _orderService.HandleOrderReturn(order, "VNPay", query["vnp_TransactionNo"]);
            }

            if (order.Status == "Pending")
            {
                await _orderService.CancelOrderAsync(order.OrderId);

                await SendPaymentFailedNotification(order.CustomerId, order.EventName);

                return "Cancelled";
            }

            return "Payment failed or cancelled";
        }

        private async Task SendPaymentSuccessNotification(int userId, string eventName, long amount)
        {
            try
            {
                var notifPayload = new
                {
                    UserId = userId,
                    Title = "Payment Successful!",
                    Message = $"Your ticket order for the event {eventName} worth {amount:N0} VND has been successfully paid. Your tickets are now ready!",
                    Type = "Order"
                };

                var response = await _http.PostAsJsonAsync("notification-gateway/Notifications", notifPayload);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error sending payment success notification: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while sending notification: {ex.Message}");
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
                    Message = $"Your ticket order for the event {eventName} was cancelled because the payment was not successful.",
                    Type = "System"
                };

                await _http.PostAsJsonAsync("notification-gateway/Notifications", notifPayload);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while sending cancellation notification: {ex.Message}");
            }
        }
    }
}