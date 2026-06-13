using MahanShop.Domain.Common;

namespace MahanShop.Domain.Entities;

/// <summary>مقدار یک ویژگی در pool (سامسونگ، A10، #FF0000). برای رنگ ColorHex پر می‌شود.</summary>
public class VariantAttributeValue : BaseEntity
{
    public int AttributeId { get; set; }
    public VariantAttribute Attribute { get; set; } = null!;

    public string Value { get; set; } = null!;
    public string? ColorHex { get; set; }  // فقط برای attribute رنگ
    public string? LogoUrl { get; set; }   // فقط برای مقادیرِ برند (لوگوی برند)
    public int DisplayOrder { get; set; }

    public ICollection<ProductVariantValue> VariantValues { get; set; } = new List<ProductVariantValue>();
}
