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
    public long ShippingCost { get; set; }   // snapshot نرخ نوع پست در لحظهٔ ثبت سفارش (از DB، نه فرم client)
    public long FinalAmount { get; set; }

    // ── نوع پست (روش ارسال) — snapshot تغییرناپذیر بعد از ثبت (مثل ProductTitle در OrderItem) ──
    /// <summary>FK به روش ارسال انتخاب‌شده. nullable تا حذف ShippingMethod سفارش قدیمی را نشکند (SetNull + snapshotِ نام/نرخ).</summary>
    public int? ShippingMethodId { get; set; }
    public ShippingMethod? ShippingMethod { get; set; }

    /// <summary>snapshot نام روش ارسال در لحظهٔ ثبت — حتی اگر روش بعداً حذف/تغییر کند ثابت می‌ماند.</summary>
    public string? ShippingMethodName { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? TrackingCode { get; set; }
    public DateTime? PaidAt { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    /// <summary>توکن همزمانی (rowversion) — جلوگیری از نهایی‌سازی/کسر موجودی دوباره در verifyهای همزمان.</summary>
    public byte[]? RowVersion { get; set; }
}
