using MahanShop.Domain.Common;

namespace MahanShop.Domain.Entities;

/// <summary>کلید مشخصه فنی (مثل RAM، حافظه، باتری).</summary>
public class Feature : BaseEntity
{
    public string Name { get; set; } = null!;
    public int DisplayOrder { get; set; }

    public ICollection<ProductFeature> ProductFeatures { get; set; } = new List<ProductFeature>();
}
