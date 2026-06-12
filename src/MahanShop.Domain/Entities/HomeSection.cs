using MahanShop.Domain.Common;
using MahanShop.Domain.Enums;

namespace MahanShop.Domain.Entities;

/// <summary>
/// نوار بدنه‌ی صفحه اصلی — لیست مرتب‌شونده با DisplayOrder (ادمین جابه‌جا می‌کنه، P9).
/// SectionType مشخص می‌کنه کدوم ستون‌ها معنی دارن: ProductRow → Product*، PromoBanner → Image*.
/// </summary>
public class HomeSection : BaseEntity
{
    public string Title { get; set; } = null!;
    public HomeSectionType SectionType { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // --- ProductRow ---
    public HomeProductSource? ProductSource { get; set; }
    public int? CategoryId { get; set; }          // فقط برای ByCategory
    public Category? Category { get; set; }
    public int MaxItems { get; set; } = 10;

    // --- PromoBanner ---
    public string? ImageUrl { get; set; }
    public string? MobileImageUrl { get; set; }
    public string? LinkUrl { get; set; }
    public string? Subtitle { get; set; }
    public bool IsHalfWidth { get; set; }         // دو بنر half کنار هم
}
