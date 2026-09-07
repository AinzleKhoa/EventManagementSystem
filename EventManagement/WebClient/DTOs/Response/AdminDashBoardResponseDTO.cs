using System.Collections.Generic;

namespace WebClient.DTOs.Response
{
    public class AdminDashBoardResponseDTO
    {
        public long TotalCustomers { get; set; }

        public long TotalEvents { get; set; }

        public long TotalPublicEvents { get; set; }

        public decimal TotalWithdrawal { get; set; }

        public List<ReadEventDTO> Top5SellingEvents { get; set; } = new List<ReadEventDTO>();
    }
}