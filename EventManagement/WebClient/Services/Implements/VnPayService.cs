using WebClient.Api;
using WebClient.Helper;

namespace WebClient.Services.Implements
{
    public class VnPayService : IVnPayService
    {
        private readonly HttpClient _http;
        public VnPayService(IHttpClientFactory factory, IConfiguration config)
        {
            _http = factory.CreateClient("ApiClient");
            var apiUrl = config["Api:Url"];
            _http.BaseAddress = new Uri(apiUrl);
        }
        public async Task<string> CreatePaymentUrl(int orderId)
        {
            var response = await _http.GetAsync(BookingAPI.CreateVNPayByOrderId.WithId(orderId));

            var body = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Payment url: " + body);

            return body;
        }
    }
}
