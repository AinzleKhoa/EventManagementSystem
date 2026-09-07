namespace BookingAPI.DTOs.TicketDTO
{
    public class TicketCheckInRequestDTO
    {
        // Chuỗi GUID quét được từ mã QR
        public Guid TicketCode { get; set; }

        // Rất quan trọng: Staff đang gác cổng ở Event nào thì truyền EventId đó lên
        // Tránh việc vé sự kiện A lại đem check-in thành công ở sự kiện B
        public int EventId { get; set; }
    }
}
