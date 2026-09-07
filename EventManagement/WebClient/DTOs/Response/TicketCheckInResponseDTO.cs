namespace WebClient.DTOs.Response
{
    public class TicketCheckInResponseDTO
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string OwnerName { get; set; }
        public string TicketType { get; set; } // Nếu em muốn join bảng TicketTypes để lấy tên vé
    }
}
