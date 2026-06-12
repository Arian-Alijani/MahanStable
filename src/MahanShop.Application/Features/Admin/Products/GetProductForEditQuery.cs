using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Products;

/// <summary>دریافت محصول برای فرم ویرایش (شامل گالری عکس).</summary>
public record GetProductForEditQuery(int Id) : IRequest<ProductEditDto?>;

public class GetProductForEditQueryHandler : IRequestHandler<GetProductForEditQuery, ProductEditDto?>
{
    private readonly IApplicationDbContext _db;
    public GetProductForEditQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ProductEditDto?> Handle(GetProductForEditQuery request, CancellationToken ct) =>
        await _db.Products.AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new ProductEditDto
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                ShortDescription = p.ShortDescription,
                Description = p.Description,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                Stock = p.Stock,
                HasVariants = p.HasVariants,
                IsActive = p.IsActive,
                IsFeatured = p.IsFeatured,
                MetaTitle = p.MetaTitle,
                MetaDescription = p.MetaDescription,
                BrandId = p.BrandId,
                CategoryId = p.CategoryId,
                Images = p.Images
                    .OrderByDescending(i => i.IsMain).ThenBy(i => i.DisplayOrder)
                    .Select(i => new ProductImageDto
                    {
                        Id = i.Id, Url = i.Url, Alt = i.Alt, IsMain = i.IsMain, DisplayOrder = i.DisplayOrder
                    }).ToList()
            })
            .FirstOrDefaultAsync(ct);
}
