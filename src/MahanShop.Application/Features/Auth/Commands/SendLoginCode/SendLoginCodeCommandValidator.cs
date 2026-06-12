using FluentValidation;
using MahanShop.Application.Common;

namespace MahanShop.Application.Features.Auth.Commands.SendLoginCode;

/// <summary>اعتبارسنجی شماره ورودی — باید موبایل معتبر ایران باشد (پس از نرمال‌سازی).</summary>
public class SendLoginCodeCommandValidator : AbstractValidator<SendLoginCodeCommand>
{
    public SendLoginCodeCommandValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("شماره موبایل را وارد کنید.")
            .Must(p => PhoneNumberHelper.Normalize(p) is not null)
            .WithMessage("شماره موبایل معتبر نیست.");
    }
}
