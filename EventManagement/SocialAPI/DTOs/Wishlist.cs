using System.ComponentModel.DataAnnotations;

namespace SocialAPI.DTOs
{
    public class CreateWishlistDTO
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Event ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Event ID.")]
        public int EventId { get; set; }
    }

    public class ReadWishlistDTO
    {
        public int WishlistId { get; set; }
        public int UserId { get; set; }
        public int EventId { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
