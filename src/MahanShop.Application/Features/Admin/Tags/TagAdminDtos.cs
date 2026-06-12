namespace MahanShop.Application.Features.Admin.Tags;

/// <summary>سطر لیست برچسب در پنل ادمین.</summary>
public class TagListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public int UsageCount { get; set; }
}

/// <summary>داده برچسب برای فرم ویرایش/ایجاد.</summary>
public class TagEditDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Slug { get; set; }
}
