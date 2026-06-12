using MahanShop.Domain.Common;

namespace MahanShop.Domain.Entities;

/// <summary>کاربر. احراز با شماره موبایل + OTP. IsAdmin برای دسترسی پنل.</summary>
public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = null!;  // یکتا
    public string? Email { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<ProductComment> Comments { get; set; } = new List<ProductComment>();
}
