using MahanShop.Domain.Common;

namespace MahanShop.Domain.Entities;

/// <summary>نظر کاربر روی محصول. IsApproved برای مدیریت ادمین.</summary>
public class ProductComment : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Text { get; set; } = null!;
    public byte Rating { get; set; }  // 1..5
    public bool IsApproved { get; set; }
}
