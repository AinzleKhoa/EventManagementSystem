using System.Text.Json.Serialization;

namespace WebClient.DTOs.Response
{
    public class ReadReviewDTO
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public int EventId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? UserName { get; set; }
        public string? AvatarUrl { get; set; }
    }

    
}
