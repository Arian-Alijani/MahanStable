using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Catalog.Queries.GetProductDetail;

public class GetProductDetailQueryHandler : IRequestHandler<GetProductDetailQuery, ProductDetailDto?>
{
    private readonly IApplicationDbContext _db;

    public GetProductDetailQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ProductDetailDto?> Handle(GetProductDetailQuery request, CancellationToken ct)
    {
        var dto = await _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.Slug == request.Slug)
            .Select(p => new ProductDetailDto
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                ShortDescription = p.ShortDescription,
                Description = p.Description,
                HasVariants = p.HasVariants,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                DiscountPercent = p.DiscountPrice != null && p.DiscountPrice < p.Price && p.Price > 0
                    ? (int)Math.Round((p.Price - p.DiscountPrice.Value) * 100.0 / p.Price)
                    : 0,
                Stock = p.Stock,
                BrandName = p.Brand.Name,
                BrandSlug = p.Brand.Slug,
                CategoryName = p.Category.Name,
                CategorySlug = p.Category.Slug,
                MetaTitle = p.MetaTitle,
                MetaDescription = p.MetaDescription,
                Images = p.Images
                    .OrderByDescending(i => i.IsMain).ThenBy(i => i.DisplayOrder)
                    .Select(i => new ProductImageDto { Url = i.Url, Alt = i.Alt, IsMain = i.IsMain })
                    .ToList(),
                Attributes = p.Variants
                    .SelectMany(v => v.Values)
                    .Select(x => x.AttributeValue.Attribute)
                    .Distinct()
                    .OrderBy(a => a.DisplayOrder)
                    .Select(a => new ProductAttributeDto
                    {
                        Id = a.Id,
                        Name = a.Name,
                        IsColor = a.IsColor,
                        Kind = a.Kind,
                        Values = a.Values
                            .Where(av => av.VariantValues.Any(vv => vv.ProductVariant.ProductId == p.Id))
                            .OrderBy(av => av.DisplayOrder).ThenBy(av => av.Value)
                            .Select(av => new ProductAttributeValueDto { Id = av.Id, Value = av.Value, ColorHex = av.ColorHex, LogoUrl = av.LogoUrl })
                            .ToList()
                    })
                    .ToList(),
                Variants = p.Variants
                    .Where(v => v.IsActive)
                    .OrderBy(v => v.DisplayOrder)
                    .Select(v => new ProductVariantDto
                    {
                        Id = v.Id,
                        Price = v.Price,
                        DiscountPrice = v.DiscountPrice,
                        Stock = v.Stock,
                        ValueIds = v.Values.Select(x => x.AttributeValueId).ToList()
                    })
                    .ToList(),
                Features = p.Features
                    .OrderBy(f => f.Feature.DisplayOrder)
                    .Select(f => new ProductFeatureDto { Name = f.Feature.Name, Value = f.Value })
                    .ToList(),
                Tags = p.Tags
                    .Select(t => new ProductTagDto { Name = t.Tag.Name, Slug = t.Tag.Slug })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        return dto;
    }
}
