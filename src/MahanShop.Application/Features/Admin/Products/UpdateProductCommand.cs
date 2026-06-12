using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Application.Features.Admin.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Products;

/// <summary>ویرایش اطلاعات پایهٔ محصول (نه گالری/واریانت).</summary>
public record UpdateProductCommand(
    int Id, string Title, string? Slug, string? ShortDescription, string? Description,
    long Price, long? DiscountPrice, int Stock, bool HasVariants,
    bool IsActive, bool IsFeatured, string? MetaTitle, string? MetaDescription,
    int BrandId, int CategoryId) : IRequest<bool>;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
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

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public UpdateProductCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new ValidationException("محصول یافت نشد.");

        var slug = SlugHelper.Make(string.IsNullOrWhiteSpace(request.Slug) ? request.Title : request.Slug);
        if (string.IsNullOrEmpty(slug)) slug = SlugHelper.Make(request.Title);

        if (await _db.Products.AnyAsync(p => p.Slug == slug && p.Id != request.Id, ct))
            throw new ValidationException("نامک (slug) تکراری است.");
        if (!await _db.Brands.AnyAsync(b => b.Id == request.BrandId, ct))
            throw new ValidationException("برند یافت نشد.");
        if (!await _db.Categories.AnyAsync(c => c.Id == request.CategoryId, ct))
            throw new ValidationException("دسته یافت نشد.");

        product.Title = request.Title.Trim();
        product.Slug = slug;
        product.ShortDescription = request.ShortDescription?.Trim();
        product.Description = request.Description;
        product.Price = request.Price;
        product.DiscountPrice = request.DiscountPrice;
        product.Stock = request.Stock;
        product.HasVariants = request.HasVariants;
        product.IsActive = request.IsActive;
        product.IsFeatured = request.IsFeatured;
        product.MetaTitle = request.MetaTitle?.Trim();
        product.MetaDescription = request.MetaDescription?.Trim();
        product.BrandId = request.BrandId;
        product.CategoryId = request.CategoryId;
        product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
