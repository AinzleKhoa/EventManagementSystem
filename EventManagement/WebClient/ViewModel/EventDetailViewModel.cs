using WebClient.DTOs.Response;

namespace WebClient.ViewModel
{
    public class EventDetailViewModel
    {
        public ReadEventDTO Event { get; set; }
        public List<ReadReviewDTO> Reviews { get; set; } = new List<ReadReviewDTO>();
        public ReadUserDTO? Organizer { get; set; }
        public double AverageRating => Reviews.Any() ? Reviews.Average(r => r.Rating) : 0;
        public int TotalReviews => Reviews.Count;
    }
}
