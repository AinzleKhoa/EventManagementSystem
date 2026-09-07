using System;
using System.Collections.Generic;

namespace EventAPI.Models;

public partial class Event
{
    public int EventId { get; set; }

    public int OrganizerId { get; set; }

    public int CategoryId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? PosterUrl { get; set; }

    public string? Location { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string? Status { get; set; }

    public string? RejectedReason { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? ConversationId { get; set; }

    public virtual ICollection<EventStaff> EventStaffs { get; set; } = new List<EventStaff>();

    public virtual ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();
}
