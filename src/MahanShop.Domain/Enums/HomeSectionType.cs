namespace MahanShop.Domain.Enums;

/// <summary>نوع نوار صفحه اصلی — جداکننده‌ی ستون‌های nullable در HomeSection.</summary>
public enum HomeSectionType
{
    ProductRow = 0,    // نوار محصول (کاروسل)
    PromoBanner = 1,   // بنر تبلیغاتی میانی
    CategoryGrid = 2   // گرید دسته‌بندی‌های منتخب (سبک نمایش در Subtitle ذخیره می‌شود)
}
