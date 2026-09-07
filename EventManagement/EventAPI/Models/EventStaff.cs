using System;
using System.Collections.Generic;

namespace EventAPI.Models;

public partial class EventStaff
{
    public int EventStaffId { get; set; }

    public int EventId { get; set; }

    public int StaffId { get; set; }

    public string? StaffEmail { get; set; }

    public string? ValidatorRole { get; set; }

    public DateTime? AssignedAt { get; set; }

    public DateTime? ExpiredAt { get; set; }

    public bool? IsActive { get; set; }

    public virtual Event Event { get; set; } = null!;
}
