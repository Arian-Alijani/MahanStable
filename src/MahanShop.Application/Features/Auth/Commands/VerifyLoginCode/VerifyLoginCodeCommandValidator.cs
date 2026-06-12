using FluentValidation;
using MahanShop.Application.Common;

namespace MahanShop.Application.Features.Auth.Commands.VerifyLoginCode;

/// <summary>اعتبارسنجی ورودی تایید — شماره معتبر + کد فقط رقم با طول قابل قبول.</summary>
public class VerifyLoginCodeCommandValidator : AbstractValidator<VerifyLoginCodeCommand>
{
    public VerifyLoginCodeCommandValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .Must(p => PhoneNumberHelper.Normalize(p) is not null)
            .WithMessage("شماره موبایل معتبر نیست.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("کد را وارد کنید.")
            .Matches(@"^\d{4,8}$").WithMessage("کد نامعتبر است.");

        RuleFor(x => x.FullName)
            .MaximumLength(200).WithMessage("نام طولانی است.");
    }
}
