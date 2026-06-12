using MahanShop.Domain.Common;

namespace MahanShop.Domain.Entities;

/// <summary>کد یکبارمصرف پیامکی برای ورود/تایید شماره. کد به‌صورت هش (HMAC) ذخیره می‌شود — هرگز plaintext.</summary>
public class OtpCode : BaseEntity
{
    public string PhoneNumber { get; set; } = null!;
    public string CodeHash { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }

    /// <summary>تعداد تلاش‌های غلط برای این کد — مهار brute-force.</summary>
    public int Attempts { get; set; }
}
