using AuthenticationAPI.DTOs;
using System.Threading.Tasks;

namespace AuthenticationAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> Login(LoginDTO dto);
        Task<AuthResponseDTO> GoogleLogin(GoogleLoginDTO dto);
        Task Register(RegisterDTO dto);
        Task ForgotPassword(string email);
        Task ResetPassword(ResetPasswordDTO dto);
        Task Logout(int userId);
        Task<AuthResponseDTO> RefreshToken(string refreshToken);
    }
}
