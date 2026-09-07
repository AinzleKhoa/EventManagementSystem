namespace WebClient.DTOs.Request
{
    public class CreateReviewDTO
    {
        public int UserId { get; set; }
        public int EventId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }

    public class UpdateReviewDTO
    {
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
