using MahanShop.Domain.Common;
using MahanShop.Domain.Enums;

namespace MahanShop.Domain.Entities;

/// <summary>نوع ویژگی متغیر (برند، مدل، کد، رنگ، حافظه). global و قابل استفاده در هر محصول.</summary>
public class VariantAttribute : BaseEntity
{
    public string Name { get; set; } = null!;
    public int DisplayOrder { get; set; }

    /// <summary>اگر true، مقادیر این ویژگی رنگ‌اند و با swatch نمایش داده می‌شوند (ColorHex).</summary>
    public bool IsColor { get; set; }

    /// <summary>نقش ویژگی در UI: برند/مدل → جریان دستگاه، رنگ → swatch، سایر → دراپ‌داون.</summary>
    public VariantAttributeKind Kind { get; set; } = VariantAttributeKind.Other;

    public ICollection<VariantAttributeValue> Values { get; set; } = new List<VariantAttributeValue>();
}
