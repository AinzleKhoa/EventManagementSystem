using System.ComponentModel.DataAnnotations;

namespace WebClient.DTOs.Request
{
    public class CreateEventDTO
    {
        public int OrganizerId { get; set; } = 1;
        public int CategoryId { get; set; } = 1;

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
        public string Title { get; set; }

        [StringLength(5000, ErrorMessage = "Description cannot exceed 5000 characters.")]
        public string? Description { get; set; }
        public string? PosterUrl { get; set; }

        [StringLength(500, ErrorMessage = "Location cannot exceed 500 characters.")]
        public string? Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [Required(ErrorMessage = "Start Date is required.")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "End Date is required.")]
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(1);

        public List<CreateTicketTypeDTO> TicketTypes { get; set; } = new List<CreateTicketTypeDTO>();
    }

    public class CreateTicketTypeDTO
    {
        [Required(ErrorMessage = "Ticket name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Ticket name must be between 1 and 100 characters.")]
        public string TypeName { get; set; } = "Vé thường";

        [Range(0, 100000000, ErrorMessage = "Price must be between 0 and 100,000,000.")]
        public decimal Price { get; set; } = 0;

        [Range(1, 100000, ErrorMessage = "Quantity must be between 1 and 100,000.")]
        public int Quantity { get; set; } = 100;
        public string? ColorCode { get; set; } = "#000000";
        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }
    }

    public class UpdateEventDTO
    {
        public int EventId { get; set; }
        public int OrganizerId { get; set; }
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
        public string Title { get; set; }

        [StringLength(5000, ErrorMessage = "Description cannot exceed 5000 characters.")]
        public string? Description { get; set; }
        public string? PosterUrl { get; set; }

        [StringLength(500, ErrorMessage = "Location cannot exceed 500 characters.")]
        public string? Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [Required(ErrorMessage = "Start Date is required.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End Date is required.")]
        public DateTime EndDate { get; set; }

        public List<UpdateTicketTypeDTO> TicketTypes { get; set; } = new List<UpdateTicketTypeDTO>();
    }

    public class UpdateTicketTypeDTO
    {
        public int TicketTypeId { get; set; }

        [Required(ErrorMessage = "Ticket name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Ticket name must be between 1 and 100 characters.")]
        public string TypeName { get; set; }

        [Range(0, 100000000, ErrorMessage = "Price must be between 0 and 100,000,000.")]
        public decimal Price { get; set; }

        [Range(1, 100000, ErrorMessage = "Quantity must be between 1 and 100,000.")]
        public int Quantity { get; set; }
        public string? ColorCode { get; set; }
        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }
    }
}
