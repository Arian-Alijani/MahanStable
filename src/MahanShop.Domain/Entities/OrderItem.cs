using MahanShop.Domain.Common;

namespace MahanShop.Domain.Entities;

/// <summary>قلم سفارش. عنوان و قیمت snapshot می‌شن تا تغییر بعدی محصول سفارش قدیم رو خراب نکنه.</summary>
public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int? VariantId { get; set; }
    public ProductVariant? Variant { get; set; }

    public string ProductTitle { get; set; } = null!;  // snapshot
    public string? VariantTitle { get; set; }          // snapshot ترکیب ویژگی (برند+مدل+کد)
    public long UnitPrice { get; set; }                // snapshot
    public int Quantity { get; set; }
}
