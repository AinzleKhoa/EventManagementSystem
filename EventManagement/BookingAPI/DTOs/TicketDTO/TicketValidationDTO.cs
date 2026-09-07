namespace BookingAPI.DTOs.TicketDTO
{
    public class TicketValidationDTO
    {
        public int OrderId { get; set; }

        public int EventId { get; set; }

        public int TicketTypeId { get; set; }

        public string? OwnerName { get; set; }

        public string? Status { get; set; }

        public DateTime? CheckedInAt { get; set; }

        public int? CheckedInBy { get; set; }


    }
    public class CreateTicketDTO : TicketValidationDTO
    {
    }
    public class UpdateTicketDTO : TicketValidationDTO
    {
    }
}
