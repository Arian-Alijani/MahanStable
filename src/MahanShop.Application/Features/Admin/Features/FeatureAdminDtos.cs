namespace MahanShop.Application.Features.Admin.Features;

/// <summary>سطر لیست مشخصه فنی (کلید feature) در پنل ادمین.</summary>
public class FeatureListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public int UsageCount { get; set; }
}

/// <summary>داده مشخصه فنی برای فرم ویرایش/ایجاد.</summary>
public class FeatureEditDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int DisplayOrder { get; set; }
}
