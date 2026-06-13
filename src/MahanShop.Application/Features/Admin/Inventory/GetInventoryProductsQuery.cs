using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Inventory;

/// <summary>
/// نمای product-group برای صفحهٔ مدیریت موجودی (F7).
/// هر ردیف = یک محصول. محصول ساده → فیلدهای Price/Stock inline.
/// محصول واریانتی → لیست کامل واریانت‌هایش برای expand و کنترل تفکیکی.
/// </summary>
public record GetInventoryProductsQuery(
    string? Search = null,
    int? BrandId = null,
    StockStatusFilter Status = StockStatusFilter.All,
    InventoryProductTypeFilter ProductType = InventoryProductTypeFilter.All,
    int Page = 1,
    int PageSize = 40,
    string Sort = "title",
    bool Desc = false) : IRequest<InventoryProductsDto>;

public class GetInventoryProductsQueryHandler : IRequestHandler<GetInventoryProductsQuery, InventoryProductsDto>
{
    private readonly IApplicationDbContext _db;
    public GetInventoryProductsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<InventoryProductsDto> Handle(GetInventoryProductsQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 200 ? 40 : request.PageSize;

        var q = _db.Products.AsNoTracking();

        // فیلتر جستجو
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            q = q.Where(p => p.Title.Contains(term) || p.Slug.Contains(term));
        }

        // فیلتر برند
        if (request.BrandId is int brandId && brandId > 0)
            q = q.Where(p => p.BrandId == brandId);

        // فیلتر نوع
        q = request.ProductType switch
        {
            InventoryProductTypeFilter.Simple => q.Where(p => !p.HasVariants),
            InventoryProductTypeFilter.Variant => q.Where(p => p.HasVariants),
            _ => q
        };

        // فیلتر وضعیت موجودی
        q = request.Status switch
        {
            StockStatusFilter.InStock => q.Where(p =>
                (!p.HasVariants && p.Stock > 5) ||
                (p.HasVariants && p.Variants.Any(v => v.Stock > 5))),
            StockStatusFilter.Low => q.Where(p =>
                (!p.HasVariants && p.Stock >= 1 && p.Stock <= 5) ||
                (p.HasVariants && p.Variants.Any(v => v.Stock >= 1 && v.Stock <= 5))),
            StockStatusFilter.Out => q.Where(p =>
                (!p.HasVariants && p.Stock == 0) ||
                (p.HasVariants && p.Variants.All(v => v.Stock == 0))),
            _ => q
        };

        var totalCount = await q.CountAsync(ct);

        // مرتب‌سازی
        q = (request.Sort, request.Desc) switch
        {
            ("title", true) => q.OrderByDescending(p => p.Title).ThenByDescending(p => p.Id),
            ("price", false) => q.OrderBy(p => p.Price).ThenBy(p => p.Id),
            ("price", true) => q.OrderByDescending(p => p.Price).ThenByDescending(p => p.Id),
            ("stock", false) => q.OrderBy(p => p.Stock).ThenBy(p => p.Id),
            ("stock", true) => q.OrderByDescending(p => p.Stock).ThenByDescending(p => p.Id),
            _ => q.OrderBy(p => p.Title).ThenBy(p => p.Id)
        };

        // واکشی محصولات صفحه‌بندی‌شده با واریانت‌هایشان
        var products = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new InventoryProductRowDto
            {
                ProductId = p.Id,
                ProductTitle = p.Title,
                CategoryName = p.Category != null ? p.Category.Name : null,
                BrandName = p.Brand != null ? p.Brand.Name : null,
                HasVariants = p.HasVariants,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                Stock = p.Stock,
                Variants = p.HasVariants
                    ? p.Variants
                        .OrderBy(v => v.DisplayOrder).ThenBy(v => v.Id)
                        .Select(v => new InventoryVariantRowDto
                        {
                            VariantId = v.Id,
                            Sku = v.Sku,
                            Price = v.Price,
                            DiscountPrice = v.DiscountPrice,
                            Stock = v.Stock,
                            IsActive = v.IsActive,
                            Title = string.Join(" / ", v.Values
                                .OrderBy(x => x.AttributeValue.Attribute.DisplayOrder)
                                .Select(x => x.AttributeValue.Value))
                        }).ToList()
                    : new List<InventoryVariantRowDto>()
            })
            .ToListAsync(ct);

        // شمارش موجودی کم (برای نوار هشدار) — روی کل DB نه فقط صفحهٔ جاری
        var lowVariant = await _db.ProductVariants.AsNoTracking()
            .CountAsync(v => v.Stock >= 1 && v.Stock <= 5, ct);
        var lowSimple = await _db.Products.AsNoTracking()
            .CountAsync(p => !p.HasVariants && p.Stock >= 1 && p.Stock <= 5, ct);

        // گزینه‌های برند برای دراپ‌داون (از جدول Brands — نه VariantAttributeValues)
        var brandOptions = await _db.Brands.AsNoTracking()
            .Where(b => b.IsActive && b.Slug != "no-brand")
            .OrderBy(b => b.DisplayOrder).ThenBy(b => b.Name)
            .Select(b => new InventoryBrandOptionDto { BrandId = b.Id, Name = b.Name })
            .ToListAsync(ct);

        return new InventoryProductsDto
        {
            Products = products,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            LowStockVariantCount = lowVariant,
            LowStockSimpleCount = lowSimple,
            BrandOptions = brandOptions
        };
    }
}
