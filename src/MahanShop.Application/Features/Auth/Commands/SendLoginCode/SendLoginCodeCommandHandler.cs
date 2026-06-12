using System.Security.Cryptography;
using System.Text;
using MahanShop.Application.Common;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Application.Common.Settings;
using MahanShop.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MahanShop.Application.Features.Auth.Commands.SendLoginCode;

/// <summary>تولید کد OTP، هش و ذخیره، ارسال پیامک. مهار سوءاستفاده: cooldown بین ارسال‌ها + سقف ارسال در ساعت.</summary>
public class SendLoginCodeCommandHandler : IRequestHandler<SendLoginCodeCommand, SendLoginCodeResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IOtpHasher _hasher;
    private readonly ISmsSender _sms;
    private readonly OtpSettings _otp;

    public SendLoginCodeCommandHandler(
        IApplicationDbContext db, IOtpHasher hasher, ISmsSender sms, IOptions<OtpSettings> otp)
    {
        _db = db;
        _hasher = hasher;
        _sms = sms;
        _otp = otp.Value;
    }

    public async Task<SendLoginCodeResult> Handle(SendLoginCodeCommand request, CancellationToken ct)
    {
        var phone = PhoneNumberHelper.Normalize(request.PhoneNumber)!;  // validator تضمین کرده null نیست
        var now = DateTime.UtcNow;
        var hourAgo = now.AddHours(-1);

        // پاسخ همیشه یکسان است (موفق + شماره نرمال‌شده + cooldown ثابت) تا oracle شمارش/وجود شماره نشت نکند.
        // محدودیت‌ها (cooldown / سقف ساعتی) در صورت تخطی → ارسال بی‌صدا انجام نمی‌شود، ولی پاسخ تغییری نمی‌کند.
        var result = new SendLoginCodeResult(true, phone, null, _otp.ResendCooldownSeconds);

        // سقف ارسال در ساعت گذشته
        var sendsLastHour = await _db.OtpCodes
            .AsNoTracking()
            .CountAsync(o => o.PhoneNumber == phone && o.CreatedAt >= hourAgo, ct);
        if (sendsLastHour >= _otp.MaxSendsPerHour)
            return result;

        // cooldown — فاصله بین دو ارسال
        var last = await _db.OtpCodes
            .AsNoTracking()
            .Where(o => o.PhoneNumber == phone)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(ct);
        if (last is not null && (now - last.CreatedAt).TotalSeconds < _otp.ResendCooldownSeconds)
            return result;

        // باطل‌کردن کدهای استفاده‌نشده قبلی این شماره — فقط آخرین کد معتبر بماند
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

        return result;
    }

    /// <summary>کد عددی تصادفی امن (CSPRNG). هر رقم مستقل، بدون bias.</summary>
    private static string GenerateNumericCode(int length)
    {
        var sb = new StringBuilder(length);
        for (var i = 0; i < length; i++)
            sb.Append((char)('0' + RandomNumberGenerator.GetInt32(0, 10)));
        return sb.ToString();
    }
}
