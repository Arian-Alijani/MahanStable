using MahanShop.Domain.Common;

namespace MahanShop.Domain.Entities;

/// <summary>رابط چند‌به‌چند محصول↔برچسب.</summary>
public class ProductTag : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
