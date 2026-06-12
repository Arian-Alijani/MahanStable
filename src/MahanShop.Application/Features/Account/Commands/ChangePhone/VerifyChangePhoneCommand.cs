using FluentValidation;
using MahanShop.Application.Common;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Application.Common.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MahanShop.Application.Features.Account.Commands.ChangePhone;

/// <summary>مرحله ۲ تغییر شماره: تایید OTP شماره جدید و اعمال تغییر روی کاربر جاری.</summary>
public record VerifyChangePhoneCommand(int UserId, string NewPhone, string Code) : IRequest<VerifyChangePhoneResult>;

public record VerifyChangePhoneResult(bool Success, string? NewPhone, string? Error);

public class VerifyChangePhoneCommandValidator : AbstractValidator<VerifyChangePhoneCommand>
{
    public VerifyChangePhoneCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.NewPhone).Must(p => PhoneNumberHelper.Normalize(p) is not null).WithMessage("شماره موبایل معتبر نیست.");
        RuleFor(x => x.Code).NotEmpty().WithMessage("کد را وارد کنید.");
    }
}

public class VerifyChangePhoneCommandHandler : IRequestHandler<VerifyChangePhoneCommand, VerifyChangePhoneResult>
{
    private const string GenericError = "کد وارد شده صحیح نیست یا منقضی شده است.";

    private readonly IApplicationDbContext _db;
    private readonly IOtpHasher _hasher;
    private readonly OtpSettings _otp;

    public VerifyChangePhoneCommandHandler(IApplicationDbContext db, IOtpHasher hasher, IOptions<OtpSettings> otp)
    {
        _db = db;
        _hasher = hasher;
        _otp = otp.Value;
    }

    public async Task<VerifyChangePhoneResult> Handle(VerifyChangePhoneCommand request, CancellationToken ct)
    {
        var phone = PhoneNumberHelper.Normalize(request.NewPhone)!;
        var now = DateTime.UtcNow;
        var hourAgo = now.AddHours(-1);

        // قفل brute-force سطح شماره
        var attemptsLastHour = await _db.OtpCodes.AsNoTracking()
            .Where(o => o.PhoneNumber == phone && o.CreatedAt >= hourAgo)
            .SumAsync(o => (int?)o.Attempts, ct) ?? 0;
        if (attemptsLastHour >= _otp.MaxVerifyAttemptsPerHour)
            return new VerifyChangePhoneResult(false, null, "تعداد تلاش بیش از حد. بعداً دوباره تلاش کنید.");

        var otp = await _db.OtpCodes
            .Where(o => o.PhoneNumber == phone && !o.IsUsed)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (otp is null) return new VerifyChangePhoneResult(false, null, GenericError);

        if (otp.ExpiresAt <= now)
        {
            otp.IsUsed = true;
            await _db.SaveChangesAsync(ct);
            return new VerifyChangePhoneResult(false, null, GenericError);
        }

        if (otp.Attempts >= _otp.MaxAttempts)
        {
            otp.IsUsed = true;
            await _db.SaveChangesAsync(ct);
            return new VerifyChangePhoneResult(false, null, "تعداد تلاش بیش از حد. کد جدید بگیرید.");
        }

        if (!_hasher.Verify(request.Code, otp.CodeHash))
        {
            otp.Attempts++;
            otp.UpdatedAt = now;
            await _db.SaveChangesAsync(ct);
            return new VerifyChangePhoneResult(false, null, GenericError);
        }

        otp.IsUsed = true;
        otp.UpdatedAt = now;

        // بازبررسی یکتایی شماره (race با ثبت‌نام همزمان)
        var taken = await _db.Users.AnyAsync(u => u.PhoneNumber == phone && u.Id != request.UserId, ct);
        if (taken)
        {
            await _db.SaveChangesAsync(ct);
            return new VerifyChangePhoneResult(false, null, "این شماره قبلاً برای حساب دیگری ثبت شده است.");
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
        if (user is null)
        {
            await _db.SaveChangesAsync(ct);
            return new VerifyChangePhoneResult(false, null, "کاربر یافت نشد.");
        }

        user.PhoneNumber = phone;
        user.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);

        return new VerifyChangePhoneResult(true, phone, null);
    }
}
