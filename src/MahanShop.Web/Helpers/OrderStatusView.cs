using MahanShop.Domain.Enums;

namespace MahanShop.Web.Helpers;

/// <summary>کمک‌نمایش وضعیت سفارش در پنل کاربر — برچسب فارسی + رنگ + آیکن (مشابه Plus Version).</summary>
public static class OrderStatusView
{
    public static string Label(OrderStatus s) => s switch
    {
        OrderStatus.Pending => "در انتظار پرداخت",
        OrderStatus.AwaitingPayment => "در حال پرداخت",
        OrderStatus.Paid => "پرداخت شد",
        OrderStatus.Processing => "در حال پردازش",
        OrderStatus.Shipped => "ارسال شد",
        OrderStatus.Delivered => "تحویل شد",
        OrderStatus.Canceled => "لغو شد",
        OrderStatus.Refunded => "مرجوع شد",
        _ => "نامشخص"
    };

    /// <summary>کلاس رنگ بج (در panel.css تعریف شده): is-warn/is-info/is-success/is-danger/is-muted.</summary>
    public static string Tone(OrderStatus s) => s switch
    {
        OrderStatus.Paid => "is-success",
        OrderStatus.Delivered => "is-success",
        OrderStatus.Processing => "is-warn",
        OrderStatus.Shipped => "is-info",
        OrderStatus.Canceled => "is-danger",
        OrderStatus.Refunded => "is-danger",
        _ => "is-muted"
    };
}
