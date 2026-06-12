namespace MahanShop.Application.Features.Admin.Brands;

/// <summary>سطر لیست برند در پنل ادمین.</summary>
public class BrandListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public int ProductCount { get; set; }
}

/// <summary>داده برند برای فرم ویرایش/ایجاد.</summary>
public class BrandEditDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Slug { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}
