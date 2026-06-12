using MediatR;

namespace MahanShop.Application.Features.Auth.Commands.VerifyLoginCode;

/// <summary>تایید کد OTP و ورود. در صورت نبود کاربر → ثبت‌نام خودکار (نام اختیاری). خروجی = نتیجه + اطلاعات کاربر برای signin.</summary>
public record VerifyLoginCodeCommand(string PhoneNumber, string Code, string? FullName) : IRequest<VerifyLoginCodeResult>;

/// <summary>نتیجه تایید. Success=true → User پر است (برای ساخت claim). در خطا پیام عمومی.</summary>
public record VerifyLoginCodeResult(bool Success, string? Error, AuthenticatedUser? User);

/// <summary>اطلاعات حداقلی کاربر احرازشده برای ساخت کوکی auth.</summary>
public record AuthenticatedUser(int Id, string FullName, string PhoneNumber, bool IsAdmin);
