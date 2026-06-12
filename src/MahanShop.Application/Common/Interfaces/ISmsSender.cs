namespace MahanShop.Application.Common.Interfaces;

/// <summary>ارسال پیامک کد تایید (OTP). پیاده‌سازی واقعی = SMS.ir؛ در dev بدون کلید = Fake.</summary>
public interface ISmsSender
{
    Task SendVerificationAsync(string phone, string code, CancellationToken ct = default);
}
