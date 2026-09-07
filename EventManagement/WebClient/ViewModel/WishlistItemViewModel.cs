using WebClient.DTOs.Response;

namespace WebClient.ViewModel
{
    public class WishlistItemViewModel
    {
        public int WishlistId { get; set; }
        public int EventId { get; set; }
        public DateTime AddedAt { get; set; }
        public ReadEventDTO Event { get; set; }
    }
}