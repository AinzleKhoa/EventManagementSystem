namespace WebClient.DTOs.Request
{
    public abstract class OrderDetailsValidationDTO
    {
        public int OrderId { get; set; }

        public int TicketTypeId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
    }
    public class CreateOrderDetailsDTO : OrderDetailsValidationDTO
    {
    }
    public class UpdateOrderDetailsDTO : OrderDetailsValidationDTO
    {
    }
}
