using System.ComponentModel.DataAnnotations;
using MahanShop.Application.Features.Account.Queries.GetUserAddresses;
using MahanShop.Application.Features.Cart.Queries.GetCart;

namespace MahanShop.Web.Models.Checkout;

/// <summary>صفحه checkout: سبد محاسبه‌شده + آدرس‌های کاربر + فرم آدرس جدید.</summary>
public class CheckoutViewModel
{
    public CartViewModel Cart { get; set; } = new();
    public List<AddressDto> Addresses { get; set; } = new();
    public int? SelectedAddressId { get; set; }
    public NewAddressInput NewAddress { get; set; } = new();
}

/// <summary>فرم آدرس جدید درون checkout.</summary>
public class NewAddressInput
{
    [Display(Name = "استان")] public string? Province { get; set; }
    [Display(Name = "شهر")] public string? City { get; set; }
    [Display(Name = "کد پستی")] public string? PostalCode { get; set; }
    [Display(Name = "نشانی کامل")] public string? FullAddress { get; set; }
    [Display(Name = "نام گیرنده")] public string? ReceiverName { get; set; }
    [Display(Name = "تماس گیرنده")] public string? ReceiverPhone { get; set; }
}
