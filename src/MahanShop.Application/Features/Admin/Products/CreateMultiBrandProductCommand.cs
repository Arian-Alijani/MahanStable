using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Application.Features.Admin.Common;
using MahanShop.Domain.Entities;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Products;

/// <summary>
/// ایجاد محصولِ «چندبرندی» + تولید خودکار همهٔ گزینه‌ها (variant).
/// ادمین: مدل‌های موردنظر را از بین برندها انتخاب می‌کند (هر مدل برندش را از ParentValueId می‌گیرد)،
/// رنگ‌ها را (اختیاری) انتخاب می‌کند و یک «قیمت اولیه» می‌دهد؛ سیستم برای هر ترکیب
/// (برند × مدل [× رنگ]) یک variant با همین قیمت و موجودیِ اولیه می‌سازد.
/// بعداً قیمت/موجودی هر گزینه در «مدیریت موجودی محصول» جداگانه قابل تغییر است.
/// </summary>
public record CreateMultiBrandProductCommand(
    string Title, string? Slug, string? ShortDescription, string? Description,
    bool IsActive, bool IsFeatured, string? MetaTitle, string? MetaDescription,
    int BrandId, int CategoryId,
    List<int> ModelValueIds, List<int> ColorValueIds,
    long BasePrice, long? BaseDiscountPrice, int BaseStock) : IRequest<int>;

public class CreateMultiBrandProductCommandValidator : AbstractValidator<CreateMultiBrandProductCommand>
{
    public CreateMultiBrandProductCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("عنوان محصول را وارد کنید.").MaximumLength(250);
        RuleFor(x => x.BrandId).GreaterThan(0).WithMessage("برندِ اصلی را انتخاب کنید.");
        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("دسته را انتخاب کنید.");
        RuleFor(x => x.BasePrice).GreaterThanOrEqualTo(0).WithMessage("قیمت اولیه نامعتبر است.");
        RuleFor(x => x.BaseStock).GreaterThanOrEqualTo(0).WithMessage("موجودی اولیه نامعتبر است.");
        RuleFor(x => x.ModelValueIds).NotEmpty().WithMessage("حداقل یک گوشی (مدل) انتخاب کنید.");
        RuleFor(x => x.BaseDiscountPrice)
            .Must((cmd, d) => d == null || d < cmd.BasePrice)
            .WithMessage("قیمت با تخفیف باید کمتر از قیمت اصلی باشد.");
    }
}

public class CreateMultiBrandProductCommandHandler : IRequestHandler<CreateMultiBrandProductCommand, int>
{
    private readonly IApplicationDbContext _db;
    public CreateMultiBrandProductCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<int> Handle(CreateMultiBrandProductCommand request, CancellationToken ct)
    {
        var slug = SlugHelper.Make(string.IsNullOrWhiteSpace(request.Slug) ? request.Title : request.Slug);
        if (string.IsNullOrEmpty(slug)) slug = SlugHelper.Make(request.Title);

        if (await _db.Products.AnyAsync(p => p.Slug == slug, ct))
            throw new ValidationException("نامک (slug) تکراری است.");
        if (!await _db.Brands.AnyAsync(b => b.Id == request.BrandId, ct))
            throw new ValidationException("برند یافت نشد.");
        if (!await _db.Categories.AnyAsync(c => c.Id == request.CategoryId, ct))
            throw new ValidationException("دسته یافت نشد.");

        var modelIds = request.ModelValueIds.Where(x => x > 0).Distinct().ToList();
        var colorIds = request.ColorValueIds.Where(x => x > 0).Distinct().ToList();

        // مدل‌های انتخاب‌شده + برندِ والدشان (باید همگی Kind=Model و دارای برندِ والد باشند).
        var modelRows = await _db.VariantAttributeValues.AsNoTracking()
            .Where(v => modelIds.Contains(v.Id) && v.Attribute.Kind == VariantAttributeKind.Model)
            .Select(v => new { v.Id, v.ParentValueId })
            .ToListAsync(ct);

        if (modelRows.Count != modelIds.Count)
            throw new ValidationException("یکی از گوشی‌های انتخاب‌شده نامعتبر است.");
        if (modelRows.Any(m => m.ParentValueId is null))
            throw new ValidationException("یکی از گوشی‌ها به هیچ برندی وصل نیست. ابتدا در «ویژگی‌های متغیر» برندِ آن را تعیین کنید.");

        // رنگ‌ها (در صورت انتخاب) باید معتبر و از نوع رنگ باشند.
        if (colorIds.Count > 0)
        {
            var validColors = await _db.VariantAttributeValues
                .CountAsync(v => colorIds.Contains(v.Id) && v.Attribute.Kind == VariantAttributeKind.Color, ct);
            if (validColors != colorIds.Count)
                throw new ValidationException("یکی از رنگ‌های انتخاب‌شده نامعتبر است.");
        }

        var product = new Product
        {
            Title = request.Title.Trim(),
            Slug = slug,
            ShortDescription = request.ShortDescription?.Trim(),
            Description = request.Description,
            Price = request.BasePrice,
            DiscountPrice = request.BaseDiscountPrice,
            Stock = 0,
            HasVariants = true,
            IsActive = request.IsActive,
            IsFeatured = request.IsFeatured,
            MetaTitle = request.MetaTitle?.Trim(),
            MetaDescription = request.MetaDescription?.Trim(),
            BrandId = request.BrandId,
            CategoryId = request.CategoryId
        };

        // تولید گزینه‌ها: برای هر مدل (برند والد + مدل) [× هر رنگ].
        var variants = new List<ProductVariant>();
        var order = 0;
        foreach (var m in modelRows)
        {
            var brandValueId = m.ParentValueId!.Value;
            if (colorIds.Count == 0)
            {
                variants.Add(BuildVariant(request, order++, new[] { brandValueId, m.Id }));
            }
            else
            {
                foreach (var cId in colorIds)
                    variants.Add(BuildVariant(request, order++, new[] { brandValueId, m.Id, cId }));
            }
        }
        product.Variants = variants;

        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);
        return product.Id;
    }

    private static ProductVariant BuildVariant(CreateMultiBrandProductCommand req, int order, int[] valueIds) => new()
    {
        Price = req.BasePrice,
        DiscountPrice = req.BaseDiscountPrice,
        Stock = req.BaseStock,
        IsActive = true,
        DisplayOrder = order,
        Values = valueIds.Select(id => new ProductVariantValue { AttributeValueId = id }).ToList()
    };
}
