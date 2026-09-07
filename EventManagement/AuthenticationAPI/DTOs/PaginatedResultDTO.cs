using System.ComponentModel.DataAnnotations;

namespace AuthenticationAPI.DTOs
{
    public class PaginatedResultDTO<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
    public class UserSearchFilterDTO
    {
        public string? SearchTerm { get; set; }

        public bool? IsActive { get; set; }

        [Range(2, 4, ErrorMessage = "Role ID must be 2 (Admin), 3 (Staff), or 4 (User).")]
        public int? RoleId { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string? SortBy { get; set; } = "FullName";

        public string? SortOrder { get; set; } = "asc";
    }
}
