using MahanShop.Domain.Enums;

namespace MahanShop.Application.Features.Admin.Variants;

/// <summary>سطر لیست ویژگی متغیر (attribute) در پنل ادمین.</summary>
public class VariantAttributeListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsColor { get; set; }
    public VariantAttributeKind Kind { get; set; }
    public int DisplayOrder { get; set; }
    public int ValueCount { get; set; }
}

/// <summary>داده attribute برای فرم ویرایش/ایجاد.</summary>
public class VariantAttributeEditDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsColor { get; set; }
    public VariantAttributeKind Kind { get; set; }
    public int DisplayOrder { get; set; }
}

/// <summary>سطر مقدار یک attribute در pool.</summary>
public class VariantAttributeValueDto
{
    public int Id { get; set; }
    public int AttributeId { get; set; }
    public string Value { get; set; } = null!;
    public string? ColorHex { get; set; }
    public string? LogoUrl { get; set; }
    public int DisplayOrder { get; set; }

    /// <summary>برای مقادیرِ «مدل» = id مقدارِ برندِ والد (در ویژگیِ برند). null برای سایر.</summary>
    public int? ParentValueId { get; set; }
    /// <summary>نام برندِ والد (برای نمایش).</summary>
    public string? ParentValueName { get; set; }
}

/// <summary>یک مقدارِ برند برای انتخاب به‌عنوان والدِ مدل (در صفحهٔ مقادیرِ مدل).</summary>
public class BrandValueOptionDto
{
    public int Id { get; set; }
    public string Value { get; set; } = null!;
}
