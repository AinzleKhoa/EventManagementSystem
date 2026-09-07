namespace WebClient.DTOs.Response
{
    public class ReadWishlistDTO
    {
        public int WishlistId { get; set; }
        public int UserId { get; set; }
        public int EventId { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
