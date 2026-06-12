using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Account.Commands.UpdateProfile;

/// <summary>به‌روزرسانی نام و ایمیل کاربر جاری. UserId از claim (نه فرم). شماره موبایل اینجا تغییر نمی‌کند (نیاز OTP — جدا).</summary>
public record UpdateProfileCommand(int UserId, string FullName, string? Email) : IRequest<bool>;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.FullName).NotEmpty().WithMessage("نام را وارد کنید.").MaximumLength(200);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("ایمیل معتبر نیست.");
    }
}

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public UpdateProfileCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
        if (user is null) return false;

        user.FullName = request.FullName.Trim();
        user.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
