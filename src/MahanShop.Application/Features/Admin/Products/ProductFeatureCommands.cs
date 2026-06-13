using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Products;

/// <summary>افزودن یک مشخصهٔ فنی (Feature + مقدار) به محصول.</summary>
public record AddProductFeatureCommand(int ProductId, int FeatureId, string Value) : IRequest<int>;

public class AddProductFeatureCommandValidator : AbstractValidator<AddProductFeatureCommand>
{
    public AddProductFeatureCommandValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.FeatureId).GreaterThan(0).WithMessage("مشخصه را انتخاب کنید.");
        RuleFor(x => x.Value).NotEmpty().WithMessage("مقدار مشخصه را وارد کنید.").MaximumLength(500);
    }
}

public class AddProductFeatureCommandHandler : IRequestHandler<AddProductFeatureCommand, int>
{
    private readonly IApplicationDbContext _db;
    public AddProductFeatureCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<int> Handle(AddProductFeatureCommand request, CancellationToken ct)
    {
        if (!await _db.Products.AnyAsync(p => p.Id == request.ProductId, ct))
            throw new ValidationException("محصول یافت نشد.");
        if (!await _db.Features.AnyAsync(f => f.Id == request.FeatureId, ct))
            throw new ValidationException("مشخصه یافت نشد.");

        // اگر همین Feature قبلاً برای این محصول ثبت شده، مقدارش را آپدیت می‌کنیم
        var existing = await _db.ProductFeatures
            .FirstOrDefaultAsync(pf => pf.ProductId == request.ProductId && pf.FeatureId == request.FeatureId, ct);

        if (existing is not null)
        {
            existing.Value = request.Value.Trim();
            existing.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return existing.Id;
        }

        var pf = new ProductFeature
        {
            ProductId = request.ProductId,
            FeatureId = request.FeatureId,
            Value = request.Value.Trim()
        };
        _db.ProductFeatures.Add(pf);
        await _db.SaveChangesAsync(ct);
        return pf.Id;
    }
}

/// <summary>حذف یک مشخصهٔ فنی از محصول.</summary>
public record DeleteProductFeatureCommand(int ProductFeatureId) : IRequest<bool>;

public class DeleteProductFeatureCommandHandler : IRequestHandler<DeleteProductFeatureCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public DeleteProductFeatureCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(DeleteProductFeatureCommand request, CancellationToken ct)
    {
        var pf = await _db.ProductFeatures.FirstOrDefaultAsync(x => x.Id == request.ProductFeatureId, ct)
            ?? throw new ValidationException("مشخصه یافت نشد.");
        _db.ProductFeatures.Remove(pf);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
