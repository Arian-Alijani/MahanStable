using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Products;

/// <summary>لیست محصولات ادمین با جستجو/فیلتر دسته‌برند و صفحه‌بندی. شامل غیرفعال‌ها.</summary>
public record GetProductsQuery(
    string? Search = null, int? CategoryId = null, int? BrandId = null,
    int Page = 1, int PageSize = 20) : IRequest<ProductListResult>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, ProductListResult>
{
    private readonly IApplicationDbContext _db;
    public GetProductsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ProductListResult> Handle(GetProductsQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var size = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var q = _db.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            q = q.Where(p => p.Title.Contains(term) || p.Slug.Contains(term) || p.Brand.Name.Contains(term));
        }
        if (request.CategoryId is int cid) q = q.Where(p => p.CategoryId == cid);
        if (request.BrandId is int bid) q = q.Where(p => p.BrandId == bid);

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * size).Take(size)
            .Select(p => new ProductListItemDto
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                BrandName = p.Brand.Name,
                CategoryName = p.Category.Name,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                Stock = p.Stock,
                HasVariants = p.HasVariants,
                IsActive = p.IsActive,
                IsFeatured = p.IsFeatured,
                PrimaryImageUrl = p.Images
                    .OrderByDescending(i => i.IsMain).ThenBy(i => i.DisplayOrder)
                    .Select(i => i.Url).FirstOrDefault()
            })
            .ToListAsync(ct);

        return new ProductListResult { Items = items, TotalCount = total, Page = page, PageSize = size };
    }
}
