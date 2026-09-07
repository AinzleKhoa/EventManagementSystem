using System;
using System.Collections.Generic;

namespace EventAPI.Models;

public partial class TicketType
{
    public int TicketTypeId { get; set; }

    public int EventId { get; set; }

    public string TypeName { get; set; } = null!;

    public long Price { get; set; }

    public int Quantity { get; set; }

    public int? SoldQuantity { get; set; }

    public string? ColorCode { get; set; }

    public DateTime? SaleStartDate { get; set; }

    public DateTime? SaleEndDate { get; set; }

    public virtual Event Event { get; set; } = null!;
}

