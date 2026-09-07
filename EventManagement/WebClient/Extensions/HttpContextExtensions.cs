using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebClient.DTOs.Response;
using WebClient.DTOs.Request;

namespace WebClient.Extensions
{
    public static class HttpContextExtensions
    {
        public static async Task SignInUserAsync(this HttpContext httpContext, AuthResponseDTO authResponse)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(authResponse.Token);

            var claims = jwtToken.Claims.ToList();

            claims.Add(new Claim("AccessToken", authResponse.Token));
            if (!string.IsNullOrEmpty(authResponse.RefreshToken))
            {
                claims.Add(new Claim("RefreshToken", authResponse.RefreshToken));
            }

            var roleVal = authResponse.User?.Role?.RoleName 
                        ?? claims.FirstOrDefault(c => c.Type == "role" || c.Type == ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(roleVal))
            {
                if (!claims.Any(c => c.Type == ClaimTypes.Role && c.Value == roleVal))
                    claims.Add(new Claim(ClaimTypes.Role, roleVal));
                if (!claims.Any(c => c.Type == "role" && c.Value == roleVal))
                    claims.Add(new Claim("role", roleVal));
            }

            UpdateBasicInfoClaims(claims, authResponse.User.FullName, authResponse.User.AvatarUrl);

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme,
                ClaimTypes.Name,
                ClaimTypes.Role
            );

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = jwtToken.ValidTo
            };

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        public static async Task UpdateUserClaimsAsync(this HttpContext httpContext, string newToken, ProfileDTO profile)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(newToken);
            var claims = jwtToken.Claims.ToList();

            var currentRefreshToken = httpContext.User.FindFirstValue("RefreshToken");

            claims.Add(new Claim("AccessToken", newToken));

            if (!string.IsNullOrEmpty(currentRefreshToken))
            {
                claims.Add(new Claim("RefreshToken", currentRefreshToken));
            }

            UpdateBasicInfoClaims(claims, profile.FullName, profile.AvatarUrl);

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme,
                ClaimTypes.Name,
                "role"
            );

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = jwtToken.ValidTo
            };

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        private static void UpdateBasicInfoClaims(List<Claim> claims, string? fullName, string? avatarUrl)
        {
            if (!string.IsNullOrEmpty(fullName))
            {
                var oldName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                if (oldName != null) claims.Remove(oldName);
                claims.Add(new Claim(ClaimTypes.Name, fullName));
            }

            if (!string.IsNullOrEmpty(avatarUrl))
            {
                var oldAvatar = claims.FirstOrDefault(c => c.Type == "AvatarUrl");
                if (oldAvatar != null) claims.Remove(oldAvatar);
                claims.Add(new Claim("AvatarUrl", avatarUrl));
            }
        }
    }
}