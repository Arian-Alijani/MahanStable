using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using MahanShop.Application.Common;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Application.Common.Settings;
using MahanShop.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MahanShop.Application.Features.Account.Commands.ChangePhone;

/// <summary>مرحله ۱ تغییر شماره: ارسال OTP به شماره جدید. اگر شماره متعلق به کاربر دیگری باشد رد می‌شود (شماره یکتا).</summary>
public record SendChangePhoneCodeCommand(int UserId, string NewPhone) : IRequest<SendChangePhoneCodeResult>;

public record SendChangePhoneCodeResult(bool Success, string? NormalizedPhone, string? Error, int CooldownSeconds);

public class SendChangePhoneCodeCommandValidator : AbstractValidator<SendChangePhoneCodeCommand>
{
    public SendChangePhoneCodeCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.NewPhone)
            .Must(p => PhoneNumberHelper.Normalize(p) is not null)
            .WithMessage("شماره موبایل معتبر نیست.");
    }
}

public class SendChangePhoneCodeCommandHandler : IRequestHandler<SendChangePhoneCodeCommand, SendChangePhoneCodeResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IOtpHasher _hasher;
    private readonly ISmsSender _sms;
    private readonly OtpSettings _otp;

    public SendChangePhoneCodeCommandHandler(
        IApplicationDbContext db, IOtpHasher hasher, ISmsSender sms, IOptions<OtpSettings> otp)
    {
        _db = db;
        _hasher = hasher;
        _sms = sms;
        _otp = otp.Value;
    }

    public async Task<SendChangePhoneCodeResult> Handle(SendChangePhoneCodeCommand request, CancellationToken ct)
    {
        var phone = PhoneNumberHelper.Normalize(request.NewPhone)!;
        var now = DateTime.UtcNow;
        var hourAgo = now.AddHours(-1);

        // شماره جدید نباید شماره فعلی همین کاربر باشد، و نباید متعلق به کاربر دیگری باشد.
        var owner = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.PhoneNumber == phone, ct);
        if (owner is not null)
        {
            var msg = owner.Id == request.UserId
                ? "این شماره هم‌اکنون شماره فعلی شماست."
                : "این شماره قبلاً برای حساب دیگری ثبت شده است.";
            return new SendChangePhoneCodeResult(false, null, msg, _otp.ResendCooldownSeconds);
        }

        // محدودیت نرخ (مثل ورود) — اینجا چون شماره مالک ندارد oracle مهم نیست، پیام واقعی برمی‌گردد.
        var sendsLastHour = await _db.OtpCodes.AsNoTracking()
            .CountAsync(o => o.PhoneNumber == phone && o.CreatedAt >= hourAgo, ct);
        if (sendsLastHour >= _otp.MaxSendsPerHour)
            return new SendChangePhoneCodeResult(false, phone, "تعداد ارسال بیش از حد. بعداً تلاش کنید.", _otp.ResendCooldownSeconds);

        var last = await _db.OtpCodes.AsNoTracking()
            .Where(o => o.PhoneNumber == phone)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(ct);
        if (last is not null && (now - last.CreatedAt).TotalSeconds < _otp.ResendCooldownSeconds)
            return new SendChangePhoneCodeResult(true, phone, null, _otp.ResendCooldownSeconds);

        var live = await _db.OtpCodes
            .Where(o => o.PhoneNumber == phone && !o.IsUsed && o.ExpiresAt > now)
            .ToListAsync(ct);
        foreach (var c in live) c.IsUsed = true;

        var code = GenerateNumericCode(_otp.CodeLength);
        _db.OtpCodes.Add(new OtpCode
        {
            PhoneNumber = phone,
            CodeHash = _hasher.Hash(code),
            ExpiresAt = now.AddMinutes(_otp.ExpiryMinutes),
            IsUsed = false,
            Attempts = 0,
            CreatedAt = now
        });
        await _db.SaveChangesAsync(ct);

        await _sms.SendVerificationAsync(phone, code, ct);
        return new SendChangePhoneCodeResult(true, phone, null, _otp.ResendCooldownSeconds);
    }

    private static string GenerateNumericCode(int length)
    {
        var sb = new StringBuilder(length);
        for (var i = 0; i < length; i++)
            sb.Append((char)('0' + RandomNumberGenerator.GetInt32(0, 10)));
        return sb.ToString();
    }
}
