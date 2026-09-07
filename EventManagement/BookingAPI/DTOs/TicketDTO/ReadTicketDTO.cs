using BookingAPI.DTOs.EventDTO;

namespace BookingAPI.DTOs.TicketDTO
{
    public class ReadTicketDTO
    {
        public int TicketId { get; set; }

        public Guid TicketCode { get; set; }

        public int OrderId { get; set; }

        public int TicketTypeId { get; set; }

        public string? OwnerName { get; set; }

        public string? Status { get; set; }

        public DateTime? CheckedInAt { get; set; }

        public int? CheckedInBy { get; set; }
        public ReadTicketTypeDTO? TicketType { get; set; }


    }
}
