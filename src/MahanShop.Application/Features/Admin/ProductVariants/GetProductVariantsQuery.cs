using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.ProductVariants;

/// <summary>داده تب گزینه‌ها/موجودی یک محصول: variantهای فعلی + pool ویژگی‌ها. null اگر محصول نبود.</summary>
public record GetProductVariantsQuery(int ProductId) : IRequest<ProductVariantsViewDto?>;

public class GetProductVariantsQueryHandler : IRequestHandler<GetProductVariantsQuery, ProductVariantsViewDto?>
{
    private readonly IApplicationDbContext _db;
    public GetProductVariantsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ProductVariantsViewDto?> Handle(GetProductVariantsQuery request, CancellationToken ct)
    {
        var product = await _db.Products.AsNoTracking()
            .Where(p => p.Id == request.ProductId)
            .Select(p => new { p.Id, p.HasVariants })
            .FirstOrDefaultAsync(ct);
        if (product is null) return null;

        var variants = await _db.ProductVariants.AsNoTracking()
            .Where(v => v.ProductId == request.ProductId)
            .OrderBy(v => v.DisplayOrder).ThenBy(v => v.Id)
            .Select(v => new ProductVariantRowDto
            {
                Id = v.Id,
                Sku = v.Sku,
                Price = v.Price,
                DiscountPrice = v.DiscountPrice,
                Stock = v.Stock,
                IsActive = v.IsActive,
                DisplayOrder = v.DisplayOrder,
                ValueIds = v.Values.Select(x => x.AttributeValueId).ToList(),
                Title = string.Join(" / ", v.Values
                    .OrderBy(x => x.AttributeValue.Attribute.DisplayOrder)
                    .Select(x => x.AttributeValue.Value))
            })
            .ToListAsync(ct);

        var attributes = await _db.VariantAttributes.AsNoTracking()
            .OrderBy(a => a.DisplayOrder).ThenBy(a => a.Name)
            .Select(a => new AttributeGroupDto
            {
                AttributeId = a.Id,
                Name = a.Name,
                IsColor = a.IsColor,
                Values = a.Values
                    .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Value)
                    .Select(x => new AttributeValueOptionDto { Id = x.Id, Value = x.Value, ColorHex = x.ColorHex })
                    .ToList()
            })
            .ToListAsync(ct);

        return new ProductVariantsViewDto
        {
            ProductId = product.Id,
            HasVariants = product.HasVariants,
            Variants = variants,
            Attributes = attributes
        };
    }
}
