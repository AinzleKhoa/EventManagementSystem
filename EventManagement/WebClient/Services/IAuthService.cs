using WebClient.DTOs.Request;
using WebClient.DTOs.Response;

namespace WebClient.Services
{
    public interface IAuthService
    {
        Task<(AuthResponseDTO? Data, string? Error)> Login(LoginDTO loginDTO);

        Task<(AuthResponseDTO? Data, string? Error)> GoogleLogin(string idToken);

        Task<ReadUserDTO?> GetUserById(int id);

        Task<string?> Register(RegisterDTO dto);

        Task<string?> ForgotPassword(ForgotPasswordDTO dto);

        Task<string?> ResetPassword(ResetPasswordDTO dto);

        Task<(AuthResponseDTO? Data, string? Error)> RefreshToken(string refreshToken);
    }
}