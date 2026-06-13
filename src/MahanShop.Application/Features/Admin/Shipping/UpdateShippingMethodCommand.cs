using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Shipping;

/// <summary>ویرایش نوع پست موجود.</summary>
public record UpdateShippingMethodCommand(
    int Id, string Name, long Cost, bool IsActive, int DisplayOrder, string? Description) : IRequest<bool>;

public class UpdateShippingMethodCommandValidator : AbstractValidator<UpdateShippingMethodCommand>
{
    public UpdateShippingMethodCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("نام روش ارسال را وارد کنید.")
            .MaximumLength(150).WithMessage("نام حداکثر ۱۵۰ حرف.");
        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0).WithMessage("هزینه نمی‌تواند منفی باشد.");
        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("ترتیب نمایش باید صفر یا بیشتر باشد.");
        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => x.Description != null)
            .WithMessage("توضیح حداکثر ۵۰۰ حرف.");
    }
}

public class UpdateShippingMethodCommandHandler : IRequestHandler<UpdateShippingMethodCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public UpdateShippingMethodCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(UpdateShippingMethodCommand request, CancellationToken ct)
    {
        var method = await _db.ShippingMethods.FirstOrDefaultAsync(s => s.Id == request.Id, ct)
            ?? throw new ValidationException("روش ارسال یافت نشد.");

        method.Name = request.Name.Trim();
        method.Cost = request.Cost;
        method.IsActive = request.IsActive;
        method.DisplayOrder = request.DisplayOrder;
        method.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        method.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
