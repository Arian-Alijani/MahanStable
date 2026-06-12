using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.ProductVariants;

/// <summary>ایجاد یک گزینه فروش (variant) برای محصول = ترکیب مقادیر ویژگی + قیمت/موجودی.</summary>
public record CreateProductVariantCommand(
    int ProductId, string? Sku, long Price, long? DiscountPrice, int Stock,
    bool IsActive, int DisplayOrder, List<int> ValueIds) : IRequest<int>;

public class CreateProductVariantCommandValidator : AbstractValidator<CreateProductVariantCommand>
{
    public CreateProductVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Sku).MaximumLength(100);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("قیمت نامعتبر است.");
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ValueIds).NotEmpty().WithMessage("حداقل یک مقدار ویژگی انتخاب کنید.");
    }
}

public class CreateProductVariantCommandHandler : IRequestHandler<CreateProductVariantCommand, int>
{
    private readonly IApplicationDbContext _db;
    public CreateProductVariantCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<int> Handle(CreateProductVariantCommand request, CancellationToken ct)
    {
        if (!await _db.Products.AnyAsync(p => p.Id == request.ProductId, ct))
            throw new ValidationException("محصول یافت نشد.");

        var valueIds = request.ValueIds.Distinct().ToList();
        var validCount = await _db.VariantAttributeValues.CountAsync(v => valueIds.Contains(v.Id), ct);
        if (validCount != valueIds.Count)
            throw new ValidationException("یکی از مقادیر ویژگی نامعتبر است.");

        if (request.DiscountPrice is long dp && dp >= request.Price)
            throw new ValidationException("قیمت با تخفیف باید کمتر از قیمت اصلی باشد.");

        var variant = new ProductVariant
        {
            ProductId = request.ProductId,
            Sku = string.IsNullOrWhiteSpace(request.Sku) ? null : request.Sku.Trim(),
            Price = request.Price,
            DiscountPrice = request.DiscountPrice,
            Stock = request.Stock,
            IsActive = request.IsActive,
            DisplayOrder = request.DisplayOrder,
            Values = valueIds.Select(id => new ProductVariantValue { AttributeValueId = id }).ToList()
        };
        _db.ProductVariants.Add(variant);
        await _db.SaveChangesAsync(ct);
        return variant.Id;
    }
}

/// <summary>ویرایش یک گزینه فروش (شامل بازنویسی مقادیر ویژگی).</summary>
public record UpdateProductVariantCommand(
    int Id, string? Sku, long Price, long? DiscountPrice, int Stock,
    bool IsActive, int DisplayOrder, List<int> ValueIds) : IRequest<bool>;

public class UpdateProductVariantCommandValidator : AbstractValidator<UpdateProductVariantCommand>
{
    public UpdateProductVariantCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Sku).MaximumLength(100);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ValueIds).NotEmpty().WithMessage("حداقل یک مقدار ویژگی انتخاب کنید.");
    }
}

public class UpdateProductVariantCommandHandler : IRequestHandler<UpdateProductVariantCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public UpdateProductVariantCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(UpdateProductVariantCommand request, CancellationToken ct)
    {
        var variant = await _db.ProductVariants
            .Include(v => v.Values)
            .FirstOrDefaultAsync(v => v.Id == request.Id, ct)
            ?? throw new ValidationException("گزینه یافت نشد.");

        var valueIds = request.ValueIds.Distinct().ToList();
        var validCount = await _db.VariantAttributeValues.CountAsync(v => valueIds.Contains(v.Id), ct);
        if (validCount != valueIds.Count)
            throw new ValidationException("یکی از مقادیر ویژگی نامعتبر است.");

        if (request.DiscountPrice is long dp && dp >= request.Price)
            throw new ValidationException("قیمت با تخفیف باید کمتر از قیمت اصلی باشد.");

        variant.Sku = string.IsNullOrWhiteSpace(request.Sku) ? null : request.Sku.Trim();
        variant.Price = request.Price;
        variant.DiscountPrice = request.DiscountPrice;
        variant.Stock = request.Stock;
        variant.IsActive = request.IsActive;
        variant.DisplayOrder = request.DisplayOrder;
        variant.UpdatedAt = DateTime.UtcNow;

        _db.ProductVariantValues.RemoveRange(variant.Values);
        variant.Values = valueIds.Select(id => new ProductVariantValue { ProductVariantId = variant.Id, AttributeValueId = id }).ToList();

        await _db.SaveChangesAsync(ct);
        return true;
    }
}

/// <summary>ویرایش سریع موجودی یک گزینه (inline).</summary>
public record UpdateVariantStockCommand(int Id, int Stock) : IRequest<bool>;

public class UpdateVariantStockCommandHandler : IRequestHandler<UpdateVariantStockCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public UpdateVariantStockCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(UpdateVariantStockCommand request, CancellationToken ct)
    {
        if (request.Stock < 0) throw new ValidationException("موجودی نمی‌تواند منفی باشد.");
        var variant = await _db.ProductVariants.FirstOrDefaultAsync(v => v.Id == request.Id, ct)
            ?? throw new ValidationException("گزینه یافت نشد.");
        variant.Stock = request.Stock;
        variant.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}

/// <summary>فعال/غیرفعال‌کردن یک گزینه.</summary>
public record ToggleVariantActiveCommand(int Id) : IRequest<bool>;

public class ToggleVariantActiveCommandHandler : IRequestHandler<ToggleVariantActiveCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public ToggleVariantActiveCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(ToggleVariantActiveCommand request, CancellationToken ct)
    {
        var variant = await _db.ProductVariants.FirstOrDefaultAsync(v => v.Id == request.Id, ct)
            ?? throw new ValidationException("گزینه یافت نشد.");
        variant.IsActive = !variant.IsActive;
        variant.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return variant.IsActive;
    }
}

/// <summary>حذف یک گزینه فروش + مقادیرش.</summary>
public record DeleteProductVariantCommand(int Id) : IRequest<bool>;

public class DeleteProductVariantCommandHandler : IRequestHandler<DeleteProductVariantCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public DeleteProductVariantCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(DeleteProductVariantCommand request, CancellationToken ct)
    {
        var variant = await _db.ProductVariants
            .Include(v => v.Values)
            .FirstOrDefaultAsync(v => v.Id == request.Id, ct)
            ?? throw new ValidationException("گزینه یافت نشد.");

        _db.ProductVariantValues.RemoveRange(variant.Values);
        _db.ProductVariants.Remove(variant);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
