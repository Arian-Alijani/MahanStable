using MahanShop.Domain.Enums;

namespace MahanShop.Application.Features.Admin.Home;

/// <summary>سطر مدیریت یک نوار صفحهٔ اصلی (نوار محصول یا بنر میانی) برای ادمین.</summary>
public class HomeSectionAdminDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public HomeSectionType SectionType { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }

    // ProductRow
    public HomeProductSource? ProductSource { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int MaxItems { get; set; }

    // PromoBanner
    public string? ImageUrl { get; set; }
    public string? MobileImageUrl { get; set; }
    public string? LinkUrl { get; set; }
    public string? Subtitle { get; set; }

    /// <summary>عنوان فارسی منبع محصولات برای نمایش.</summary>
    public string SourceLabel => ProductSource switch
    {
        HomeProductSource.Featured => "پیشنهادی",
        HomeProductSource.BestSelling => "پرفروش‌ترین",
        HomeProductSource.Newest => "جدیدترین",
        HomeProductSource.Discounted => "تخفیف‌دار",
        HomeProductSource.ByCategory => "بر اساس دسته" + (CategoryName != null ? $" ({CategoryName})" : ""),
        _ => "—"
    };
}

/// <summary>دادهٔ نوار محصول برای فرم ویرایش/ایجاد.</summary>
public class ProductRowEditDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public HomeProductSource ProductSource { get; set; } = HomeProductSource.Featured;
    public int? CategoryId { get; set; }
    public int MaxItems { get; set; } = 10;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>دادهٔ بنر میانی برای فرم ویرایش/ایجاد.</summary>
public class BannerEditDto
{
    public int Id { get; set; }
    public string? Subtitle { get; set; }
    public string? ImageUrl { get; set; }
    public string? MobileImageUrl { get; set; }
    public string? LinkUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
