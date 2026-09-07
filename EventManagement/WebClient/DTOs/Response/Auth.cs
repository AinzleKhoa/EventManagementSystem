namespace WebClient.DTOs.Response
{
    public class AuthResponseDTO
    {
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public ReadUserDTO User { get; set; } = null!;
    }

    public class ApiErrorResponse
    {
        public string? Message { get; set; }
    }
}
