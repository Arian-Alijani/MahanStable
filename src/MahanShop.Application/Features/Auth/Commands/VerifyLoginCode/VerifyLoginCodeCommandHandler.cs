using MahanShop.Application.Common;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Application.Common.Settings;
using MahanShop.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MahanShop.Application.Features.Auth.Commands.VerifyLoginCode;

/// <summary>تایید کد: بررسی انقضا/تلاش/تک‌مصرفی + مقایسه هش constant-time. موفق → find-or-create کاربر و بازگشت claim. مهار brute-force با MaxAttempts.</summary>
public class VerifyLoginCodeCommandHandler : IRequestHandler<VerifyLoginCodeCommand, VerifyLoginCodeResult>
{
    private const string GenericError = "کد وارد شده صحیح نیست یا منقضی شده است.";

    private readonly IApplicationDbContext _db;
    private readonly IOtpHasher _hasher;
    private readonly OtpSettings _otp;

    public VerifyLoginCodeCommandHandler(IApplicationDbContext db, IOtpHasher hasher, IOptions<OtpSettings> otp)
    {
        _db = db;
        _hasher = hasher;
        _otp = otp.Value;
    }

    public async Task<VerifyLoginCodeResult> Handle(VerifyLoginCodeCommand request, CancellationToken ct)
    {
        var phone = PhoneNumberHelper.Normalize(request.PhoneNumber)!;
        var now = DateTime.UtcNow;
        var hourAgo = now.AddHours(-1);

        // قفل brute-force سطح شماره (مستقل از resend): جمع تلاش‌های غلط یک ساعت گذشته
        var attemptsLastHour = await _db.OtpCodes
            .AsNoTracking()
            .Where(o => o.PhoneNumber == phone && o.CreatedAt >= hourAgo)
            .SumAsync(o => (int?)o.Attempts, ct) ?? 0;
        if (attemptsLastHour >= _otp.MaxVerifyAttemptsPerHour)
            return new VerifyLoginCodeResult(false, "تعداد تلاش بیش از حد. بعداً دوباره تلاش کنید.", null);

        // آخرین کد استفاده‌نشده‌ی این شماره (tracked — قرار است آپدیت شود)
        var otp = await _db.OtpCodes
            .Where(o => o.PhoneNumber == phone && !o.IsUsed)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (otp is null)
            return new VerifyLoginCodeResult(false, GenericError, null);

        // منقضی → باطل کن
        if (otp.ExpiresAt <= now)
        {
            otp.IsUsed = true;
            await _db.SaveChangesAsync(ct);
            return new VerifyLoginCodeResult(false, GenericError, null);
        }

        // سقف تلاش → قفل کد (مهار brute-force)
        if (otp.Attempts >= _otp.MaxAttempts)
        {
            otp.IsUsed = true;
            await _db.SaveChangesAsync(ct);
            return new VerifyLoginCodeResult(false, "تعداد تلاش بیش از حد. کد جدید بگیرید.", null);
        }

        // مقایسه constant-time
        if (!_hasher.Verify(request.Code, otp.CodeHash))
        {
            otp.Attempts++;
            otp.UpdatedAt = now;
            await _db.SaveChangesAsync(ct);
            return new VerifyLoginCodeResult(false, GenericError, null);
        }

        // درست — تک‌مصرف
        otp.IsUsed = true;
        otp.UpdatedAt = now;

        // find-or-create کاربر
        var user = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone, ct);
        if (user is null)
        {
            user = new User
            {
                PhoneNumber = phone,
                FullName = string.IsNullOrWhiteSpace(request.FullName) ? string.Empty : request.FullName!.Trim(),
                IsActive = true,
                IsAdmin = false,
                CreatedAt = now
            };
            _db.Users.Add(user);
        }
        else if (!user.IsActive)
        {
            await _db.SaveChangesAsync(ct);  // ذخیره IsUsed
            return new VerifyLoginCodeResult(false, "حساب کاربری غیرفعال است.", null);
        }
        else if (!string.IsNullOrWhiteSpace(request.FullName) && string.IsNullOrWhiteSpace(user.FullName))
        {
            user.FullName = request.FullName!.Trim();  // تکمیل نام در اولین فرصت
            user.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(ct);

        return new VerifyLoginCodeResult(true, null,
            new AuthenticatedUser(user.Id, user.FullName, user.PhoneNumber, user.IsAdmin));
    }
}
