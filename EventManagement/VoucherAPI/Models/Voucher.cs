using System;
using System.Collections.Generic;

namespace VoucherAPI.Models;

public partial class Voucher
{
    public int VoucherId { get; set; }

    public int EventId { get; set; }

    public string Code { get; set; } = null!;

    public string? DiscountType { get; set; }

    public decimal DiscountValue { get; set; }

    public int Quantity { get; set; }

    public int? UsedCount { get; set; }

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<UserVoucher> UserVouchers { get; set; } = new List<UserVoucher>();
}
