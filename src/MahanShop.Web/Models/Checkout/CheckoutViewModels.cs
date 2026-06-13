using System.ComponentModel.DataAnnotations;
using MahanShop.Application.Features.Account.Queries.GetUserAddresses;
using MahanShop.Application.Features.Cart.Queries.GetCart;
using MahanShop.Application.Features.Cart.Queries.GetShippingMethods;

namespace MahanShop.Web.Models.Checkout;

/// <summary>صفحه checkout: سبد محاسبه‌شده + آدرس‌های کاربر + روش‌های ارسال + فرم آدرس جدید.</summary>
public class CheckoutViewModel
{
    public CartViewModel Cart { get; set; } = new();
    public List<AddressDto> Addresses { get; set; } = new();
    public int? SelectedAddressId { get; set; }
    public List<ShippingMethodCheckoutDto> ShippingMethods { get; set; } = new();
    public int? SelectedShippingMethodId { get; set; }
    public NewAddressInput NewAddress { get; set; } = new();

    /// <summary>هزینه ارسال روش انتخابی (برای نمایش مبلغ نهایی). سمت سرور محاسبه می‌شود — نه از client.</summary>
    public long SelectedShippingCost =>
        ShippingMethods.FirstOrDefault(s => s.Id == SelectedShippingMethodId)?.Cost ?? 0;

    /// <summary>مبلغ نهایی قابل پرداخت = اقلام + هزینه ارسال انتخابی.</summary>
    public long FinalPayableAmount => Cart.PayableAmount + SelectedShippingCost;
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
