namespace MahanShop.Application.Features.Admin.Categories;

/// <summary>سطر لیست دسته در پنل ادمین (با نام والد و عمق برای نمایش درختی).</summary>
public class CategoryListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public int Depth { get; set; }
    public bool IsActive { get; set; }
    public bool ShowInMenu { get; set; }
    public bool ShowOnHome { get; set; }
    public int DisplayOrder { get; set; }
    public int ProductCount { get; set; }
}

/// <summary>داده دسته برای فرم ویرایش/ایجاد.</summary>
public class CategoryEditDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Slug { get; set; }
    public string? ImageUrl { get; set; }
    public int? ParentId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool ShowInMenu { get; set; } = true;
    public bool ShowOnHome { get; set; }
}

/// <summary>گزینه انتخاب والد در فرم.</summary>
public class CategoryOptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int Depth { get; set; }
}
