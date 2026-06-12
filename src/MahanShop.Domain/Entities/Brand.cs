using MahanShop.Domain.Common;

namespace MahanShop.Domain.Entities;

/// <summary>برند موبایل (سامسونگ، اپل، ...). برای مرور بر اساس برند.</summary>
public class Brand : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
