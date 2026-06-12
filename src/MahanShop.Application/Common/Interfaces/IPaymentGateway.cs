namespace MahanShop.Application.Common.Interfaces;

/// <summary>درگاه پرداخت (Zarinpal). HTTP در لایه Infra. مبالغ ورودی به تومان؛ تبدیل به ریال داخل پیاده‌سازی.</summary>
public interface IPaymentGateway
{
    /// <summary>درخواست پرداخت → Authority + URL درگاه برای ریدایرکت.</summary>
    Task<PaymentRequestResult> RequestAsync(long amountToman, string callbackUrl, string description, CancellationToken ct = default);

    /// <summary>تایید پرداخت پس از بازگشت از درگاه. مبلغ باید با درخواست یکی باشد.</summary>
    Task<PaymentVerifyResult> VerifyAsync(long amountToman, string authority, CancellationToken ct = default);
}

/// <summary>نتیجه درخواست پرداخت.</summary>
public record PaymentRequestResult(bool Success, string? Authority, string? GatewayUrl, string? Error);

/// <summary>نتیجه تایید پرداخت. AlreadyVerified = کد 101 (قبلاً تایید شده).</summary>
public record PaymentVerifyResult(bool Success, string? RefId, bool AlreadyVerified, string? Error);
