namespace WebClient.DTOs.Request
{
    public class VoucherSearchFilterDTO
    {
        public int? OrganizerId { get; set; }
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "Code"; 
        public string? SortOrder { get; set; } = "asc"; 
        public List<int>? EventIds { get; set; }
    }
}
