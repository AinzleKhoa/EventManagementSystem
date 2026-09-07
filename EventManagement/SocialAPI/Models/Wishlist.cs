using System;
using System.Collections.Generic;

namespace SocialAPI.Models;

public partial class Wishlist
{
    public int WishlistId { get; set; }

    public int UserId { get; set; }

    public int EventId { get; set; }

    public DateTime? AddedAt { get; set; }
}
