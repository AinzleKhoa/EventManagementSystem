using System;
using System.Collections.Generic;

namespace WebClient.Models;

public partial class Wishlist
{
    public int WishlistId { get; set; }

    public int UserId { get; set; }

    public int EventId { get; set; }

    public DateTime? AddedAt { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
