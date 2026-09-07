namespace WebClient.DTOs.Response
{
    public class ReadEventDTO
    {
        public int EventId { get; set; }
        public int CategoryId { get; set; } 
        public int OrganizerId { get; set; }
        public string OrganizerName { get; set; } = "Unknown"; // ✅ THÊM ĐÂY
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? PosterUrl { get; set; }
        public string? Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? RejectedReason { get; set; }
        public string? CategoryName { get; set; }
        public int? ConversationId { get; set; }

        public List<ReadTicketTypeDTO>? TicketTypes { get; set; }
    }

    public class ReadTicketTypeDTO
    {
        public int TicketTypeId { get; set; }
        public string TypeName { get; set; }
        public long Price { get; set; }
        public int Quantity { get; set; }
        public int SoldQuantity { get; set; }
        public int AvailableQty { get; set; }
        public string ColorCode { get; set; }
        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }
    }
}
