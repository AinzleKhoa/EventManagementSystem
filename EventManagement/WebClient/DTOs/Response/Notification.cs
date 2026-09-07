namespace WebClient.DTOs.Response
{
    public class NotificationResponseDTO
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public string? Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UnreadCountResponseDTO
    {
        public int Count { get; set; }
    }
}
