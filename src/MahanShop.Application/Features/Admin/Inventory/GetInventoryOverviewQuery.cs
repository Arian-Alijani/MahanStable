using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Inventory;

/// <summary>
/// دادهٔ صفحهٔ مرکزی «مدیریت موجودی»: همهٔ واریانت‌های همهٔ محصولات (نوع اول و دوم)
/// با فیلتر محصول/برند/مدل/وضعیت موجودی + صفحه‌بندی.
/// </summary>
public record GetInventoryOverviewQuery(
    string? Search = null,
    int? BrandValueId = null,
    string? Model = null,
    StockStatusFilter Status = StockStatusFilter.All,
    int Page = 1,
    int PageSize = 50,
    string Sort = "product",
    bool Desc = false) : IRequest<InventoryOverviewDto>;

public class GetInventoryOverviewQueryHandler : IRequestHandler<GetInventoryOverviewQuery, InventoryOverviewDto>
{
    private readonly IApplicationDbContext _db;
    public GetInventoryOverviewQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<InventoryOverviewDto> Handle(GetInventoryOverviewQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 500 ? 50 : request.PageSize;

        var q = _db.ProductVariants.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            q = q.Where(v => v.Product.Title.Contains(term) || (v.Sku != null && v.Sku.Contains(term)));
        }

        if (request.BrandValueId is int brandId && brandId > 0)
            q = q.Where(v => v.Values.Any(x => x.AttributeValueId == brandId));

        if (!string.IsNullOrWhiteSpace(request.Model))
        {
            var m = request.Model.Trim();
            q = q.Where(v => v.Values.Any(x =>
                x.AttributeValue.Attribute.Kind == VariantAttributeKind.Model &&
                x.AttributeValue.Value.Contains(m)));
        }

        q = request.Status switch
        {
            StockStatusFilter.InStock => q.Where(v => v.Stock > 5),
            StockStatusFilter.Low => q.Where(v => v.Stock >= 1 && v.Stock <= 5),
            StockStatusFilter.Out => q.Where(v => v.Stock == 0),
            _ => q
        };

        var totalCount = await q.CountAsync(ct);

        // پروجکشن میانی برای مرتب‌سازی روی نام محصول/موجودی/SKU
        var projected = q.Select(v => new
        {
            VariantId = v.Id,
            ProductId = v.ProductId,
            ProductTitle = v.Product.Title,
            v.Sku,
            v.Stock,
            v.IsActive,
            Values = v.Values
                .OrderBy(x => x.AttributeValue.Attribute.DisplayOrder)
                .Select(x => new
                {
                    x.AttributeValue.Attribute.Kind,
                    x.AttributeValue.Value
                }).ToList()
        });

        projected = (request.Sort, request.Desc) switch
        {
            ("stock", false) => projected.OrderBy(x => x.Stock).ThenBy(x => x.VariantId),
            ("stock", true) => projected.OrderByDescending(x => x.Stock).ThenBy(x => x.VariantId),
            ("sku", false) => projected.OrderBy(x => x.Sku).ThenBy(x => x.VariantId),
            ("sku", true) => projected.OrderByDescending(x => x.Sku).ThenBy(x => x.VariantId),
            (_, true) => projected.OrderByDescending(x => x.ProductTitle).ThenByDescending(x => x.VariantId),
            _ => projected.OrderBy(x => x.ProductTitle).ThenBy(x => x.VariantId)
        };

        var raw = await projected
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var rows = raw.Select(v =>
        {
            var brand = v.Values.FirstOrDefault(x => x.Kind == VariantAttributeKind.Brand)?.Value;
            var model = v.Values.FirstOrDefault(x => x.Kind == VariantAttributeKind.Model)?.Value;
            var others = v.Values
                .Where(x => x.Kind is VariantAttributeKind.Color or VariantAttributeKind.Other)
                .Select(x => x.Value)
                .ToList();

            // نوع دوم: اگر برند/مدل نبود، نخستین ویژگی به‌عنوان «مدل/ویژگی اصلی» نمایش داده می‌شود
            if (model is null && brand is null && others.Count > 0)
            {
                model = others[0];
                others = others.Skip(1).ToList();
            }

            return new InventoryRowDto
            {
                VariantId = v.VariantId,
                ProductId = v.ProductId,
                ProductTitle = v.ProductTitle,
                Sku = v.Sku,
                Stock = v.Stock,
                IsActive = v.IsActive,
                Brand = brand,
                Model = model,
                OtherAttributes = others.Count > 0 ? string.Join(" / ", others) : null
            };
        }).ToList();

        var lowStockCount = await _db.ProductVariants.AsNoTracking()
            .CountAsync(v => v.Stock >= 1 && v.Stock <= 5, ct);

        var brandOptions = await _db.VariantAttributeValues.AsNoTracking()
            .Where(x => x.Attribute.Kind == VariantAttributeKind.Brand)
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Value)
            .Select(x => new InventoryBrandOptionDto { BrandId = x.Id, Name = x.Value })
            .ToListAsync(ct);

        return new InventoryOverviewDto
        {
            Rows = rows,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            LowStockCount = lowStockCount,
            BrandOptions = brandOptions
        };
    }
}
