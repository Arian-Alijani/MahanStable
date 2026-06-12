using MahanShop.Domain.Common;

namespace MahanShop.Domain.Entities;

/// <summary>اسلاید بنر هیرو صفحه اصلی. عکس موبایل جدا برای ریسپانسیو. قابل کنترل از پنل ادمین (P9).</summary>
public class Banner : BaseEntity
{
    public string? Title { get; set; }
    public string ImageUrl { get; set; } = null!;
    public string? MobileImageUrl { get; set; }
    public string? LinkUrl { get; set; }
    public string AltText { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
