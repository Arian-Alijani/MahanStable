using FluentValidation;

namespace MahanShop.Application.Features.Cart.Commands.PlaceOrder;

/// <summary>اعتبارسنجی پایه ورودی ثبت سفارش. منطق موجودی/قیمت/نوع‌پست در handler مقابل DB.</summary>
public class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.AddressId).GreaterThan(0).WithMessage("آدرس تحویل را انتخاب کنید.");
        RuleFor(x => x.ShippingMethodId).GreaterThan(0).WithMessage("روش ارسال را انتخاب کنید.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("سبد خرید خالی است.");
    }
}
