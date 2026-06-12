using MahanShop.Domain.Common;

namespace MahanShop.Domain.Entities;

/// <summary>یک گزینه فروش محصول = یک ردیف موجودی مستقل. ترکیب چند مقدار ویژگی (برند+مدل+کد).</summary>
public class ProductVariant : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string? Sku { get; set; }          // کد داخلی/بارکد
    public long Price { get; set; }
    public long? DiscountPrice { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    /// <summary>کنترل همزمانی برای کسر موجودی idempotent.</summary>
    public byte[]? RowVersion { get; set; }

    public ICollection<ProductVariantValue> Values { get; set; } = new List<ProductVariantValue>();
}
