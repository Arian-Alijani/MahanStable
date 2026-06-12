using MahanShop.Domain.Common;
using MahanShop.Domain.Enums;

namespace MahanShop.Domain.Entities;

/// <summary>تراکنش Zarinpal. Authority = توکن درخواست، RefId = کد پیگیری موفق.</summary>
public class Payment : BaseEntity
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public long Amount { get; set; }
    public string? Authority { get; set; }
    public string? RefId { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
}
