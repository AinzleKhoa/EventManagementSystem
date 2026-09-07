using System.ComponentModel.DataAnnotations;

namespace WebClient.DTOs.Request
{
    public class UserSearchFilterDTO
    {
        public string? SearchTerm { get; set; }

        public bool? IsActive { get; set; }

        [Range(2, 4, ErrorMessage = "Role ID must be 2 (Customer), 3 (Orgerniger), or 4 (Staff).")]
        public int? RoleId { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string? SortBy { get; set; } = "FullName";

        public string? SortOrder { get; set; } = "asc";
    }
}
