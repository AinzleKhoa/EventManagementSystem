using System;
using System.Collections.Generic;

namespace WebClient.Models;

public partial class Ticket
{
    public int TicketId { get; set; }

    public string TicketCode { get; set; } = null!;

    public int OrderId { get; set; }

    public int TicketTypeId { get; set; }

    public string? OwnerName { get; set; }

    public string? Status { get; set; }

    public DateTime? CheckedInAt { get; set; }

    public int? CheckedInBy { get; set; }

    public virtual User? CheckedInByNavigation { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual TicketType TicketType { get; set; } = null!;
}
