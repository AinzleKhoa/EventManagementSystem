using System.ComponentModel.DataAnnotations;

namespace EventAPI.DTOs
{
    public class AddValidatorDTO
    {
        [Required(ErrorMessage = "Event ID is required")]
        public int EventId { get; set; }

        [Required(ErrorMessage = "Validator email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string ValidatorEmail { get; set; } = null!;
    }

    public class ReadEventValidatorDTO
    {
        public int EventStaffId { get; set; }
        public int EventId { get; set; }
        public int StaffId { get; set; }
        public string? StaffEmail { get; set; }
        public string? ValidatorRole { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public bool? IsActive { get; set; }
    }

    public class RemoveValidatorDTO
    {
        [Required(ErrorMessage = "Event Staff ID is required")]
        public int EventStaffId { get; set; }
    }
}
