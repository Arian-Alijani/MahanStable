using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Application.Features.Admin.Common;
using MahanShop.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Products;

/// <summary>ایجاد محصول جدید. قیمت/موجودی همیشه سمت سرور اعتبارسنجی می‌شود.</summary>
public record CreateProductCommand(
    string Title, string? Slug, string? ShortDescription, string? Description,
    long Price, long? DiscountPrice, int Stock, bool HasVariants,
    bool IsActive, bool IsFeatured, string? MetaTitle, string? MetaDescription,
    int BrandId, int CategoryId) : IRequest<int>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("عنوان محصول را وارد کنید.").MaximumLength(250);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("قیمت نامعتبر است.");
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0).WithMessage("موجودی نامعتبر است.");
        RuleFor(x => x.BrandId).GreaterThan(0).WithMessage("برند را انتخاب کنید.");
        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("دسته را انتخاب کنید.");
        RuleFor(x => x.DiscountPrice)
            .Must((cmd, d) => d == null || d < cmd.Price)
            .WithMessage("قیمت با تخفیف باید کمتر از قیمت اصلی باشد.");
    }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IApplicationDbContext _db;
    public CreateProductCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<int> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var slug = SlugHelper.Make(string.IsNullOrWhiteSpace(request.Slug) ? request.Title : request.Slug);
        if (string.IsNullOrEmpty(slug)) slug = SlugHelper.Make(request.Title);

        if (await _db.Products.AnyAsync(p => p.Slug == slug, ct))
            throw new ValidationException("نامک (slug) تکراری است.");
        if (!await _db.Brands.AnyAsync(b => b.Id == request.BrandId, ct))
            throw new ValidationException("برند یافت نشد.");
        if (!await _db.Categories.AnyAsync(c => c.Id == request.CategoryId, ct))
            throw new ValidationException("دسته یافت نشد.");

        var product = new Product
        {
            Title = request.Title.Trim(),
            Slug = slug,
            ShortDescription = request.ShortDescription?.Trim(),
            Description = request.Description,
            Price = request.Price,
            DiscountPrice = request.DiscountPrice,
            Stock = request.Stock,
            HasVariants = request.HasVariants,
            IsActive = request.IsActive,
            IsFeatured = request.IsFeatured,
            MetaTitle = request.MetaTitle?.Trim(),
            MetaDescription = request.MetaDescription?.Trim(),
            BrandId = request.BrandId,
            CategoryId = request.CategoryId
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);
        return product.Id;
    }
}
