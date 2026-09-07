using System.ComponentModel.DataAnnotations;

namespace ChatbotAPI.DTOs
{
    public class ReadEventDTO
    {
        public int EventId { get; set; }
        public int CategoryId { get; set; }
        public int OrganizerId { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public string PosterUrl { get; set; }
        public string Location { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ReadTicketTypeDTO> TicketTypes { get; set; }
    }

    public class CreateEventDTO
    {
        [Required]
        public int OrganizerId { get; set; }
        [Required]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? PosterUrl { get; set; }
        public string? Location { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<CreateTicketTypeDTO>? TicketTypes { get; set; }
    }

    public class UpdateEventDTO
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? PosterUrl { get; set; }
        public string? Location { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

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

    public class CreateTicketTypeDTO
    {
        [Required]
        public string TypeName { get; set; }
        public long Price { get; set; }
        public int Quantity { get; set; }
        public string? ColorCode { get; set; }
    }
}
