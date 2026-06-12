using MahanShop.Domain.Common;

namespace MahanShop.Domain.Entities;

/// <summary>دسته‌بندی درختی (خودارجاع) — جایگزین Groups + SubmenuGroups نمونه.</summary>
public class Category : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool ShowInMenu { get; set; } = true;
    public bool ShowOnHome { get; set; }  // گرید دسته‌بندی منتخب زیر بنر

    // درخت دسته
    public int? ParentId { get; set; }
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
