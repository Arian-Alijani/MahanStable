using MahanShop.Domain.Enums;

namespace MahanShop.Application.Features.Admin.Home;

/// <summary>سطر یک دسته در پنل مدیریت «گرید دسته‌بندی صفحهٔ اصلی».</summary>
public class HomeCategoryItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? ImageUrl { get; set; }
    public bool ShowOnHome { get; set; }
    public int DisplayOrder { get; set; }
    public int ProductCount { get; set; }
}

/// <summary>وضعیت کامل بخش «گرید دسته‌بندی صفحهٔ اصلی» برای ادمین.</summary>
public class HomeCategoryBoardDto
{
    /// <summary>دسته‌هایی که روی صفحهٔ اصلی نمایش داده می‌شوند (مرتب بر اساس DisplayOrder).</summary>
    public List<HomeCategoryItemDto> Selected { get; set; } = new();

    /// <summary>دسته‌های فعالی که فعلاً روی صفحهٔ اصلی نیستند (برای افزودن).</summary>
    public List<HomeCategoryItemDto> Available { get; set; } = new();

    /// <summary>سبک نمایش فعلی گرید.</summary>
    public HomeCategoryStyle Style { get; set; } = HomeCategoryStyle.Bento;

    /// <summary>آیا کل بخش گرید دسته‌بندی روی صفحهٔ اصلی فعال است؟</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>عدد ترتیب بخش گرید در بدنهٔ صفحهٔ اصلی (نسبت به نوارها/بنرها).</summary>
    public int DisplayOrder { get; set; }

    /// <summary>عنوان نمایشی بخش (سرتیتر).</summary>
    public string Title { get; set; } = "خرید بر اساس دسته‌بندی";
}
