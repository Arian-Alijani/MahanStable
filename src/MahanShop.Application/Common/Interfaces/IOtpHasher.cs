namespace MahanShop.Application.Common.Interfaces;

/// <summary>هش/تایید کد OTP با HMAC + مقایسه constant-time. کد خام هرگز ذخیره نمی‌شود.</summary>
public interface IOtpHasher
{
    string Hash(string code);
    bool Verify(string code, string hash);
}
