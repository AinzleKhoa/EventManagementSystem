using System.ComponentModel.DataAnnotations;

namespace BookingAPI.DTOs.TicketTypeDTO
{
    public class ReadTicketTypeDTO
    {
        public int TicketTypeId { get; set; }
        public string TypeName { get; set; }
        public long Price { get; set; }
        public int Quantity { get; set; }
        public int SoldQuantity { get; set; }
        public int AvailableQty { get; set; }
        public string ColorCode { get; set; }
    }

    public class UpdateTicketTypeQuantityDTO
    {
        public int Quantity { get; set; }
    }
}
