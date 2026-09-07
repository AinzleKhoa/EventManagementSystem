using System.ComponentModel.DataAnnotations;

namespace SocialAPI.DTOs
{
    public class CreateReviewDTO
    {
        [Required(ErrorMessage = "Event ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Event ID.")]
        public int EventId { get; set; }

        [Required(ErrorMessage = "Rating is required.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Review comment cannot be empty.")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Comment must be between 5 and 500 characters.")]
        public string Comment { get; set; } = null!;

        public int UserId { get; set; }
    }

    public class ReadReviewDTO
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public int EventId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateReviewDTO
    {
        [Required(ErrorMessage = "Rating is required.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Review comment cannot be empty.")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Comment must be between 5 and 500 characters.")]
        public string Comment { get; set; } = null!;
    }
}
