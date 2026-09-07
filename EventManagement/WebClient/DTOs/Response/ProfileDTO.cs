namespace WebClient.DTOs.Response
{

    public class ProfileDTO
    {
        public int UserId { get; set; }

        public string? FullName { get; set; }

        public string Email { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public string? AvatarUrl { get; set; }

        public bool? IsEmailVerified { get; set; }

        public DateTime? CreatedAt { get; set; }
    }

    public class ProfileResponseDTO
    {
        public ProfileDTO Profile { get; set; } = null!;
        public string NewToken { get; set; } = string.Empty;
    }
}
