namespace WithdrawalAPI.DTOs
{
    public class PaginatedResultDTO<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    public class WithdrawalFilterDTO
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public int? OrganizerId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "RequestedAt";
        public string? SortOrder { get; set; } = "desc";
    }
}
