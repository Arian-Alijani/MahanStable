using MahanShop.Domain.Common;

namespace MahanShop.Domain.Entities;

/// <summary>
/// نوع پست / روش ارسال (پست پیشتاز، تیپاکس، ...). نرخ ثابت per-method (تومان).
/// نرخ فقط توسط ادمین تنظیم می‌شود و در چک‌اوت از DB خوانده می‌شود (هرگز از فرم client) → غیرقابل دستکاری.
/// هنگام ثبت سفارش، نام و نرخ به‌صورت snapshot در Order ذخیره می‌شوند تا تغییر بعدیِ نرخ، سفارش‌های قدیمی را نشکند.
/// </summary>
public class ShippingMethod : BaseEntity
{
    /// <summary>نام نمایشی روش ارسال («پست پیشتاز»، «تیپاکس»).</summary>
    public string Name { get; set; } = null!;

    /// <summary>هزینهٔ ثابت ارسال به تومان.</summary>
    public long Cost { get; set; }

    /// <summary>فعال بودن (فقط روش‌های فعال در چک‌اوت نمایش داده می‌شوند).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>ترتیب نمایش در لیست انتخاب.</summary>
    public int DisplayOrder { get; set; }

    /// <summary>توضیح اختیاری (مثلاً «۲ تا ۳ روز کاری»).</summary>
    public string? Description { get; set; }
}
