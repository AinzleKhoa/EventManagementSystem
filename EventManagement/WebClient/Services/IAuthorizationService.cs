using System.Security.Claims;

namespace WebClient.Services
{
    public interface IAuthorizationService
    {
        string GetUserEmailFromClaims(ClaimsPrincipal user);
        int GetUserIdFromClaims(ClaimsPrincipal user);
    }
}
