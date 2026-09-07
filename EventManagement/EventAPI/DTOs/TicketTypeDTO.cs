using System.ComponentModel.DataAnnotations;
using EventAPI.CustomValidation;

namespace EventAPI.DTOs
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
        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }
    }

    public class CreateTicketTypeDTO
    {
        [Required(ErrorMessage = "Event ID is required.")]
        public int EventId { get; set; }

        [TicketTypeNameValidation]
        public string TypeName { get; set; }

        [TicketPriceValidation]
        public long Price { get; set; }

        [TicketQuantityValidation]
        public int Quantity { get; set; }

        public string? ColorCode { get; set; }
        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }
    }

    public class UpdateTicketTypeDTO
    {
        [TicketTypeNameValidation]
        public string TypeName { get; set; }

        [TicketPriceValidation]
        public long Price { get; set; }

        [TicketQuantityValidation]
        public int Quantity { get; set; }

        public int? SoldQuantity { get; set; }

        public string? ColorCode { get; set; }
        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }
    }

    public class UpdateTicketTypeQuantityDTO
    {
        [TicketQuantityValidation]
        public int Quantity { get; set; }
    }
}
