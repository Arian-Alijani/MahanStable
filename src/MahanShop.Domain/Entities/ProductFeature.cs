using MahanShop.Domain.Common;

namespace MahanShop.Domain.Entities;

/// <summary>مقدار مشخصه فنی برای یک محصول (RAM = 8GB).</summary>
public class ProductFeature : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int FeatureId { get; set; }
    public Feature Feature { get; set; } = null!;

    public string Value { get; set; } = null!;
}
