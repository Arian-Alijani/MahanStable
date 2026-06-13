namespace MahanShop.Domain.Enums;

/// <summary>
/// سبک نمایش گرید دسته‌بندی‌های منتخب در صفحهٔ اصلی.
/// مقدار به‌صورت رشته (نام enum) در ستون Subtitle رکوردِ HomeSection از نوع CategoryGrid ذخیره می‌شود
/// تا نیازی به مهاجرت دیتابیس نباشد.
/// </summary>
public enum HomeCategoryStyle
{
    Bento = 0,    // بنتو — سلول اول بزرگ (پیش‌فرض)
    Grid = 1,     // گرید یکنواخت — همهٔ سلول‌ها هم‌اندازه
    Scroll = 2,   // اسلایدر افقی — تایل‌های گرد قابل اسکرول
    Pills = 3     // چیپس/قرص — فشرده و سبک، مناسب تعداد زیاد
}
