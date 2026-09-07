using System.Net.Http.Headers;
using System.Security.Claims;
using WebClient.Extensions;
using WebClient.Services;
using System.Net;

namespace WebClient.Handlers
{
    public class AuthTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;

        public AuthTokenHandler(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var context = _httpContextAccessor.HttpContext;
            string? token = null;

            if (context != null && context.User.Identity?.IsAuthenticated == true)
            {
                token = context.User.FindFirst("AccessToken")?.Value 
                     ?? context.User.FindFirst("token")?.Value 
                     ?? context.User.FindFirst("jwt")?.Value;
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized && context != null)
            {
                // Prevent infinite loop if the refresh token request itself fails with 401
                if (request.RequestUri != null && request.RequestUri.AbsolutePath.Contains("refresh-token"))
                {
                    return response;
                }

                var refreshToken = context.User.FindFirst("RefreshToken")?.Value;

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var authService = _serviceProvider.GetRequiredService<IAuthService>();
                    var result = await authService.RefreshToken(refreshToken);

                    if (result.Data != null)
                    {
                        await context.SignInUserAsync(result.Data);

                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.Data.Token);

                        response.Dispose();
                        return await base.SendAsync(request, cancellationToken);
                    }
                }
            }

            return response;
        }
    }
}