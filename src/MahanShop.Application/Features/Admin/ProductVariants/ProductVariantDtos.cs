namespace MahanShop.Application.Features.Admin.ProductVariants;

/// <summary>سطر گرید گزینه/موجودی محصول در ویرایشگر.</summary>
public class ProductVariantRowDto
{
    public int Id { get; set; }
    public string? Sku { get; set; }
    public long Price { get; set; }
    public long? DiscountPrice { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    /// <summary>عنوان خوانا = ترکیب مقادیر (سامسونگ / A10).</summary>
    public string Title { get; set; } = "";
    /// <summary>شناسه مقادیر انتخاب‌شده این variant.</summary>
    public List<int> ValueIds { get; set; } = new();
}

/// <summary>یک گزینه مقدار برای انتخابگر (وابسته به attribute).</summary>
public class AttributeValueOptionDto
{
    public int Id { get; set; }
    public string Value { get; set; } = "";
    public string? ColorHex { get; set; }
}

/// <summary>یک گروه ویژگی + مقادیرش برای ساخت ردیف variant.</summary>
public class AttributeGroupDto
{
    public int AttributeId { get; set; }
    public string Name { get; set; } = "";
    public bool IsColor { get; set; }
    public List<AttributeValueOptionDto> Values { get; set; } = new();
}

/// <summary>کل داده تب «گزینه‌ها/موجودی» یک محصول.</summary>
public class ProductVariantsViewDto
{
    public int ProductId { get; set; }
    public bool HasVariants { get; set; }
    public List<ProductVariantRowDto> Variants { get; set; } = new();
    public List<AttributeGroupDto> Attributes { get; set; } = new();
}
