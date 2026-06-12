using System.Security.Cryptography;
using System.Text;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Application.Common.Settings;
using Microsoft.Extensions.Options;

namespace MahanShop.Infra.Data.Services;

/// <summary>هش OTP با HMAC-SHA256 (کلید = Pepper سرور). مقایسه constant-time برای جلوگیری از timing attack. کد خام هرگز ذخیره/لاگ نمی‌شود.</summary>
public class OtpHasher : IOtpHasher
{
    private readonly byte[] _pepper;

    public OtpHasher(IOptions<OtpSettings> options)
    {
        var pepper = options.Value.Pepper;
        if (string.IsNullOrWhiteSpace(pepper))
            throw new InvalidOperationException("OtpSettings.Pepper تنظیم نشده — باید از env بیاید (Otp__Pepper).");
        _pepper = Encoding.UTF8.GetBytes(pepper);
    }

    public string Hash(string code)
    {
        using var hmac = new HMACSHA256(_pepper);
        var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(bytes);  // 64 char hex
    }

    public bool Verify(string code, string hash)
    {
        var computed = Hash(code);
        // constant-time — طول برابر، مقایسه بایتی ثابت‌زمان
        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(computed),
            Encoding.ASCII.GetBytes(hash));
    }
}
