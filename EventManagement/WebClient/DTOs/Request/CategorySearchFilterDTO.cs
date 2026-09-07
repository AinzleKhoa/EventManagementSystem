namespace WebClient.DTOs.Request
{
    public class CategorySearchFilterDTO
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "CategoryName";
        public string? SortOrder { get; set; } = "asc";
    }
}
