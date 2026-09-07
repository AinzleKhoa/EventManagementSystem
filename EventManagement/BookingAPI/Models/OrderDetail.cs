using System;
using System.Collections.Generic;

namespace BookingAPI.Models;

public partial class OrderDetail
{
    public int OrderDetailId { get; set; }

    public int OrderId { get; set; }

    public int TicketTypeId { get; set; }

    public int Quantity { get; set; }

    public long UnitPrice { get; set; }

    public virtual Order Order { get; set; } = null!;
}
