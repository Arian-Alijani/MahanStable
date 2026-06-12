using MahanShop.Domain.Common;
using MahanShop.Domain.Enums;

namespace MahanShop.Domain.Entities;

/// <summary>سفارش. مبالغ long (تومان). FinalAmount = Total - Discount + Shipping.</summary>
public class Order : BaseEntity
{
    public string OrderCode { get; set; } = null!;  // کد یکتا قابل پیگیری

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int? AddressId { get; set; }
    public Address? Address { get; set; }

    public long TotalAmount { get; set; }
    public long DiscountAmount { get; set; }
    public long ShippingCost { get; set; }
    public long FinalAmount { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? TrackingCode { get; set; }
    public DateTime? PaidAt { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    /// <summary>توکن همزمانی (rowversion) — جلوگیری از نهایی‌سازی/کسر موجودی دوباره در verifyهای همزمان.</summary>
    public byte[]? RowVersion { get; set; }
}
