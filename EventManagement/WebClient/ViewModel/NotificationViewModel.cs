namespace WebClient.ViewModel
{
    public class NotificationViewModel
    {
        public int NotificationId { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public string? Type { get; set; }
        public bool IsRead { get; set; }
        // Thêm trường hiển thị thời gian thân thiện (vd: "2 hours ago")
        public string? TimeAgo { get; set; }
    }
}