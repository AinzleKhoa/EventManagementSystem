using System;
using System.Collections.Generic;

namespace BookingAPI.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int CustomerId { get; set; }

    public int EventId { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? EventName { get; set; }

    public string? PosterUrl { get; set; }

    public DateTime? OrderDate { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public long TotalAmount { get; set; }

    public long? DiscountAmount { get; set; }

    public long FinalAmount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? TransactionId { get; set; }

    public string? Status { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public int? UserVoucherId { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
