using WebClient.DTOs.Response;

namespace WebClient.ViewModel
{
    public class SearchViewModel
    {
        public List<ReadEventDTO> Events { get; set; } = new List<ReadEventDTO>();

        public List<ReadCategoryDTO> Categories { get; set; } = new List<ReadCategoryDTO>();

        public string? Query { get; set; }

        public List<int> SelectedCategoryIds { get; set; } = new List<int>();

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 9; 
        public int TotalItems { get; set; }

        public List<int> WishlistedEventIds { get; set; } = new List<int>();
    }
}
