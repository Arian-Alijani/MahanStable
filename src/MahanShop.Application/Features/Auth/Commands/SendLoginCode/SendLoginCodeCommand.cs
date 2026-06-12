using MediatR;

namespace MahanShop.Application.Features.Auth.Commands.SendLoginCode;

/// <summary>درخواست ارسال کد ورود به شماره موبایل. خروجی = نتیجه با وضعیت محدودیت ارسال.</summary>
public record SendLoginCodeCommand(string PhoneNumber) : IRequest<SendLoginCodeResult>;

/// <summary>نتیجه ارسال. Success=false وقتی محدودیت نرخ خورد یا خطای ارسال. شماره نرمال‌شده برای مرحله تایید.</summary>
public record SendLoginCodeResult(bool Success, string NormalizedPhone, string? Error, int CooldownSeconds);
