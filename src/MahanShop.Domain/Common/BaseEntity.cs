namespace MahanShop.Domain.Common;

/// <summary>پایه همه موجودیت‌ها — کلید و زمان‌های ساخت/ویرایش.</summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
