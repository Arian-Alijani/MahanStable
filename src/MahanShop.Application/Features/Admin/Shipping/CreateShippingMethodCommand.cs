using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Entities;
using MediatR;

namespace MahanShop.Application.Features.Admin.Shipping;

/// <summary>ایجاد نوع پست جدید.</summary>
public record CreateShippingMethodCommand(
    string Name, long Cost, bool IsActive, int DisplayOrder, string? Description) : IRequest<int>;

public class CreateShippingMethodCommandValidator : AbstractValidator<CreateShippingMethodCommand>
{
    public CreateShippingMethodCommandValidator()
    {
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

public class CreateShippingMethodCommandHandler : IRequestHandler<CreateShippingMethodCommand, int>
{
    private readonly IApplicationDbContext _db;
    public CreateShippingMethodCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<int> Handle(CreateShippingMethodCommand request, CancellationToken ct)
    {
        var method = new ShippingMethod
        {
            Name = request.Name.Trim(),
            Cost = request.Cost,
            IsActive = request.IsActive,
            DisplayOrder = request.DisplayOrder,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim()
        };
        _db.ShippingMethods.Add(method);
        await _db.SaveChangesAsync(ct);
        return method.Id;
    }
}
