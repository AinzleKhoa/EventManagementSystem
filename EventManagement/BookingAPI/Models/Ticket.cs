using System;
using System.Collections.Generic;

namespace BookingAPI.Models;

public partial class Ticket
{
    public int TicketId { get; set; }

    public Guid TicketCode { get; set; }

    public int OrderId { get; set; }

    public int EventId { get; set; }

    public int TicketTypeId { get; set; }

    public string? OwnerName { get; set; }

    public string? Status { get; set; }

    public DateTime? CheckedInAt { get; set; }

    public int? CheckedInBy { get; set; }

    public virtual Order Order { get; set; } = null!;
}
