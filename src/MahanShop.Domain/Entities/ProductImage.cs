using MahanShop.Domain.Common;

namespace MahanShop.Domain.Entities;

/// <summary>تصویر گالری محصول. Alt برای SEO/دسترسی‌پذیری.</summary>
public class ProductImage : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string Url { get; set; } = null!;
    public string? Alt { get; set; }
    public bool IsMain { get; set; }
    public int DisplayOrder { get; set; }
}
