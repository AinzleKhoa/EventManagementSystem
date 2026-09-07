using AuthenticationAPI.Helpers;
using AuthenticationAPI.Models;

namespace AuthenticationAPI.Helpers
{
    public interface IJwtHelper
    {
        string GenerateToken(User user);
        string GenerateRefreshTokenString();
    }
}
