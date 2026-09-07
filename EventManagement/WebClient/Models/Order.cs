using System;
using System.Collections.Generic;

namespace WebClient.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int CustomerId { get; set; }

    public int EventId { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal FinalAmount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? TransactionId { get; set; }

    public string? Status { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public virtual User Customer { get; set; } = null!;

    public virtual Event Event { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
