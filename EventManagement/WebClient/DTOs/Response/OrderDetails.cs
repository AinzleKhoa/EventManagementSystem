namespace WebClient.DTOs.Response
{
    public class ReadOrderDetailsDTO
    {
        public int OrderDetailId { get; set; }

        public int OrderId { get; set; }

        public int TicketTypeId { get; set; }

        public int Quantity { get; set; }

        public long UnitPrice { get; set; }
        public ReadTicketTypeDTO? TicketType { get; set; }

    }
}
