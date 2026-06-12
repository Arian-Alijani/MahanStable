namespace MahanShop.Application.Common.Settings;

/// <summary>پارامترهای امنیتی OTP. مقادیر حساس (Pepper) فقط از env/secret؛ هیچ default واقعی در سورس.</summary>
public class OtpSettings
{
    public const string SectionName = "Otp";

    /// <summary>کلید مخفی سرور برای HMAC هش کد. باید از env بیاید و قوی باشد.</summary>
    public string Pepper { get; set; } = string.Empty;

    public int CodeLength { get; set; } = 6;
    public int ExpiryMinutes { get; set; } = 2;

    /// <summary>سقف تلاش غلط روی یک کد (قفل همان ردیف).</summary>
    public int MaxAttempts { get; set; } = 5;

    /// <summary>سقف تلاش غلط روی یک شماره در یک ساعت (مهار brute-force مستقل از resend).</summary>
    public int MaxVerifyAttemptsPerHour { get; set; } = 10;

    public int ResendCooldownSeconds { get; set; } = 90;
    public int MaxSendsPerHour { get; set; } = 5;
}
