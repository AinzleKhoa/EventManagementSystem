namespace WebClient.DTOs.Response
{
    public class ErrorResponse
    {
        public string? Message { get; set; }
        public string? Error { get; set; }
    }

    public class ToggleResponse
    {
        public string? Message { get; set; }
        public bool IsActive { get; set; }
    }
}
