namespace MahanShop.Domain.Enums;

/// <summary>وضعیت سفارش در چرخه خرید.</summary>
public enum OrderStatus
{
    Pending = 0,          // سبد ثبت شد، منتظر پرداخت
    AwaitingPayment = 1,  // به درگاه رفت
    Paid = 2,             // پرداخت موفق
    Processing = 3,       // در حال آماده‌سازی
    Shipped = 4,          // ارسال شد
    Delivered = 5,        // تحویل شد
    Canceled = 6,         // لغو
    Refunded = 7          // مرجوع
}
