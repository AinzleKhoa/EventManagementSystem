using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WebClient.Services.Implements
{

    public class AuthorizationService : IAuthorizationService
    {
        public string GetUserEmailFromClaims(ClaimsPrincipal user)
        {
            return user?.FindFirst(ClaimTypes.Email)?.Value 
                   ?? user?.FindFirst("email")?.Value
                   ?? user?.FindFirst(JwtRegisteredClaimNames.Email)?.Value 
                   ?? string.Empty;
        }

        public int GetUserIdFromClaims(ClaimsPrincipal user)
        {
            var userIdStr = user?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                         ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(userIdStr, out int uid) ? uid : 0;
        }
    }
}
