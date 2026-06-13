using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Products;

/// <summary>فیلتر وضعیت نمایش محصول.</summary>
public enum ProductActiveFilter { All = 0, Active = 1, Inactive = 2 }

/// <summary>فیلتر وضعیت موجودی محصول.</summary>
public enum ProductStockFilter { All = 0, InStock = 1, Low = 2, Out = 3 }

/// <summary>گزینه‌های مرتب‌سازی لیست محصولات.</summary>
public enum ProductSort { Newest = 0, Oldest = 1, PriceAsc = 2, PriceDesc = 3, StockAsc = 4, StockDesc = 5, TitleAsc = 6 }

/// <summary>
/// لیست محصولات ادمین با جست‌وجوی پیشرفته (عنوان/نامک/برند/دسته)،
/// فیلتر دسته/برند/وضعیت/موجودی/منتخب، مرتب‌سازی و صفحه‌بندی. شامل غیرفعال‌ها.
/// همراه آمار کلی (مستقل از فیلتر) برای کارت‌های بالای صفحه.
/// </summary>
public record GetProductsQuery(
    string? Search = null,
    int? CategoryId = null,
    int? BrandId = null,
    ProductActiveFilter Active = ProductActiveFilter.All,
    ProductStockFilter Stock = ProductStockFilter.All,
    bool? Featured = null,
    ProductSort Sort = ProductSort.Newest,
    int Page = 1,
    int PageSize = 20) : IRequest<ProductListResult>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, ProductListResult>
{
    private const int LowStockThreshold = 5;
    private readonly IApplicationDbContext _db;
    public GetProductsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ProductListResult> Handle(GetProductsQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var size = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        // موجودی واقعی: محصول ساده = Stock خودش، محصول دارای گزینه = جمع موجودی گزینه‌ها.
        // این عبارت در همهٔ مراحل (فیلتر/مرتب‌سازی/خروجی) دوباره استفاده می‌شود تا یکدست بماند.
        var baseQ = _db.Products.AsNoTracking();

        // ---- جست‌وجو ----
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            baseQ = baseQ.Where(p =>
                p.Title.Contains(term) ||
                p.Slug.Contains(term) ||
                p.Brand.Name.Contains(term) ||
                p.Category.Name.Contains(term) ||
                (p.ShortDescription != null && p.ShortDescription.Contains(term)));
        }

        // ---- فیلتر دسته/برند ----
        if (request.CategoryId is int cid) baseQ = baseQ.Where(p => p.CategoryId == cid);
        if (request.BrandId is int bid) baseQ = baseQ.Where(p => p.BrandId == bid);

        // ---- فیلتر وضعیت نمایش ----
        baseQ = request.Active switch
        {
            ProductActiveFilter.Active => baseQ.Where(p => p.IsActive),
            ProductActiveFilter.Inactive => baseQ.Where(p => !p.IsActive),
            _ => baseQ
        };

        // ---- فیلتر منتخب ----
        if (request.Featured is bool f) baseQ = baseQ.Where(p => p.IsFeatured == f);

        // ---- فیلتر موجودی (روی موجودی واقعی) ----
        baseQ = request.Stock switch
        {
            ProductStockFilter.InStock => baseQ.Where(p => (p.HasVariants ? p.Variants.Sum(v => v.Stock) : p.Stock) > LowStockThreshold),
            ProductStockFilter.Low => baseQ.Where(p =>
                (p.HasVariants ? p.Variants.Sum(v => v.Stock) : p.Stock) > 0 &&
                (p.HasVariants ? p.Variants.Sum(v => v.Stock) : p.Stock) <= LowStockThreshold),
            ProductStockFilter.Out => baseQ.Where(p => (p.HasVariants ? p.Variants.Sum(v => v.Stock) : p.Stock) == 0),
            _ => baseQ
        };

        var total = await baseQ.CountAsync(ct);

        // ---- مرتب‌سازی ----
        IQueryable<Product> ordered = request.Sort switch
        {
            ProductSort.Oldest => baseQ.OrderBy(p => p.CreatedAt),
            ProductSort.PriceAsc => baseQ.OrderBy(p => p.DiscountPrice ?? p.Price),
            ProductSort.PriceDesc => baseQ.OrderByDescending(p => p.DiscountPrice ?? p.Price),
            ProductSort.StockAsc => baseQ.OrderBy(p => p.HasVariants ? p.Variants.Sum(v => v.Stock) : p.Stock),
            ProductSort.StockDesc => baseQ.OrderByDescending(p => p.HasVariants ? p.Variants.Sum(v => v.Stock) : p.Stock),
            ProductSort.TitleAsc => baseQ.OrderBy(p => p.Title),
            _ => baseQ.OrderByDescending(p => p.CreatedAt)
        };

        var items = await ordered
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
                VariantCount = p.Variants.Count,
                EffectiveStock = p.HasVariants ? p.Variants.Sum(v => v.Stock) : p.Stock,
                DisplayPrice = p.DiscountPrice ?? p.Price,
                PrimaryImageUrl = p.Images
                    .OrderByDescending(i => i.IsMain).ThenBy(i => i.DisplayOrder)
                    .Select(i => i.Url).FirstOrDefault()
            })
            .ToListAsync(ct);

        // ---- آمار کلی (مستقل از فیلتر صفحه) ----
        var stats = await BuildStatsAsync(ct);

        return new ProductListResult
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = size,
            Stats = stats
        };
    }

    /// <summary>آمار کلی روی همهٔ محصولات (نه فیلترشده) — برای کارت‌های بالای صفحه.</summary>
    private async Task<ProductStatsDto> BuildStatsAsync(CancellationToken ct)
    {
        var all = _db.Products.AsNoTracking();

        var total = await all.CountAsync(ct);
        var active = await all.CountAsync(p => p.IsActive, ct);

        var lowStock = await all.CountAsync(p =>
            (p.HasVariants ? p.Variants.Sum(v => v.Stock) : p.Stock) > 0 &&
            (p.HasVariants ? p.Variants.Sum(v => v.Stock) : p.Stock) <= LowStockThreshold, ct);

        var outOfStock = await all.CountAsync(p =>
            (p.HasVariants ? p.Variants.Sum(v => v.Stock) : p.Stock) == 0, ct);

        return new ProductStatsDto
        {
            Total = total,
            Active = active,
            Inactive = total - active,
            LowStock = lowStock,
            OutOfStock = outOfStock
        };
    }
}
