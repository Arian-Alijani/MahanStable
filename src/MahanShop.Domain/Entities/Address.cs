using MahanShop.Domain.Common;

namespace MahanShop.Domain.Entities;

/// <summary>آدرس تحویل کاربر — جایگزین CodePostal نمونه.</summary>
public class Address : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Province { get; set; } = null!;
    public string City { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string FullAddress { get; set; } = null!;
    public string ReceiverName { get; set; } = null!;
    public string ReceiverPhone { get; set; } = null!;
}
