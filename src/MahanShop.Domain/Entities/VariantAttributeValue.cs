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

    /// <summary>
    /// والد سلسله‌مراتبی این مقدار در همان pool. کاربرد اصلی: یک مقدارِ «مدل»
    /// (مثل A10) به یک مقدارِ «برند» (مثل سامسونگ) وصل می‌شود تا در ویزارد
    /// محصولِ چندبرندی، گوشی‌های هر برند به‌تفکیک نمایش داده شوند. برای برند/رنگ null.
    /// </summary>
    public int? ParentValueId { get; set; }
    public VariantAttributeValue? Parent { get; set; }
    public ICollection<VariantAttributeValue> Children { get; set; } = new List<VariantAttributeValue>();

    public ICollection<ProductVariantValue> VariantValues { get; set; } = new List<ProductVariantValue>();
}
