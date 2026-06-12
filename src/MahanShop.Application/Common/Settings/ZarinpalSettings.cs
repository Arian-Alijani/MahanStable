namespace MahanShop.Application.Common.Settings;

/// <summary>تنظیمات درگاه پرداخت Zarinpal. مقدار از env: Zarinpal__MerchantId و Zarinpal__CallbackUrl. Zarinpal__Sandbox=true برای تست.</summary>
public class ZarinpalSettings
{
    public const string SectionName = "Zarinpal";

    public string MerchantId { get; set; } = string.Empty;

    /// <summary>URL مطلق callback (مثل https://host/payment/verify). Zarinpal بعد پرداخت کاربر را به اینجا برمی‌گرداند.</summary>
    public string CallbackUrl { get; set; } = string.Empty;

    /// <summary>true → محیط تست sandbox.zarinpal.com.</summary>
    public bool Sandbox { get; set; }

    private string Host => Sandbox ? "sandbox.zarinpal.com" : "payment.zarinpal.com";

    public string RequestUrl => $"https://{Host}/pg/v4/payment/request.json";
    public string VerifyUrl => $"https://{Host}/pg/v4/payment/verify.json";
    public string StartPayUrl(string authority) => $"https://{Host}/pg/StartPay/{authority}";
}
