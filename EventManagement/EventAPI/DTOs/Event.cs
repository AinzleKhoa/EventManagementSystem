using System.ComponentModel.DataAnnotations;
using EventAPI.CustomValidation;

namespace EventAPI.DTOs
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
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? RejectedReason { get; set; }
        public string? CategoryName { get; set; }
        public int? ConversationId { get; set; }
        public List<ReadTicketTypeDTO> TicketTypes { get; set; }
    }

    [EventDateRangeValidation]
    public class CreateEventDTO
    {
        [Required(ErrorMessage = "Organizer ID is required.")]
        public int OrganizerId { get; set; }

        [Required(ErrorMessage = "Category ID is required.")]
        public int CategoryId { get; set; }

        [EventTitleValidation]
        public string Title { get; set; }

        [EventDescriptionValidation]
        public string? Description { get; set; }

        public string? PosterUrl { get; set; }

        [EventLocationValidation]
        public string? Location { get; set; }

        [LatitudeValidation]
        public double? Latitude { get; set; }

        [LongitudeValidation]
        public double? Longitude { get; set; }

        [FutureDateValidation(ErrorMessage = "Start Date must be a future date.")]
        public DateTime StartDate { get; set; }

        [FutureDateValidation(ErrorMessage = "End Date must be a future date.")]
        public DateTime EndDate { get; set; }

        public List<CreateTicketTypeDTO>? TicketTypes { get; set; }
    }

    [EventDateRangeValidation]
    public class UpdateEventDTO
    {
        [EventTitleValidation]
        public string Title { get; set; }

        [EventDescriptionValidation]
        public string? Description { get; set; }

        public string? PosterUrl { get; set; }

        [EventLocationValidation]
        public string? Location { get; set; }

        [LatitudeValidation]
        public double? Latitude { get; set; }

        [LongitudeValidation]
        public double? Longitude { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<UpdateTicketTypeDTO>? TicketTypes { get; set; }
    }
}
