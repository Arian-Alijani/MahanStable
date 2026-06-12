using MahanShop.Domain.Common;

namespace MahanShop.Domain.Entities;

/// <summary>برچسب محصول (پرفروش، جدید، ...). Slug برای SEO.</summary>
public class Tag : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;

    public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
}
