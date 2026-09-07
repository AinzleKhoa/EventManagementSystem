using System.Net.Http.Headers;

namespace BookingAPI.Helpers
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthHeaderHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var authHeader = _httpContextAccessor.HttpContext?
                .Request.Headers["Authorization"]
                .FirstOrDefault();
            Console.WriteLine("🔥 TOKEN FORWARD: " + authHeader);
            if (!string.IsNullOrEmpty(authHeader))
            {
                request.Headers.Authorization =
                    AuthenticationHeaderValue.Parse(authHeader);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
