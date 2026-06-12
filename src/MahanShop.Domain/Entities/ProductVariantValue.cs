using MahanShop.Domain.Common;

namespace MahanShop.Domain.Entities;

/// <summary>join: یک variant ⟷ یک مقدار ویژگی. مثلا variant فلان → (مدل=A10).</summary>
public class ProductVariantValue : BaseEntity
{
    public int ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

    public int AttributeValueId { get; set; }
    public VariantAttributeValue AttributeValue { get; set; } = null!;
}
