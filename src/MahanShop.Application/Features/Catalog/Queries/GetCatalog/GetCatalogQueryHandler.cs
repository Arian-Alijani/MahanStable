using MahanShop.Application.Common.Interfaces;
using MahanShop.Application.Features.Home.Queries.GetHomePage;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Catalog.Queries.GetCatalog;

public class GetCatalogQueryHandler : IRequestHandler<GetCatalogQuery, CatalogViewModel>
{
    private readonly IApplicationDbContext _db;

    public GetCatalogQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<CatalogViewModel> Handle(GetCatalogQuery request, CancellationToken ct)
    {
        var f = request.Filter;
        f.Page = f.Page < 1 ? 1 : f.Page;
        f.PageSize = f.PageSize is < 1 or > 60 ? 20 : f.PageSize;

        // ---- درخت دسته‌ها (یکجا، تعداد کم) برای sidebar + استخراج زیرشاخه‌ها ----
        var cats = await _db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive && c.ShowInMenu)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new { c.Id, c.Name, c.Slug, c.ParentId })
            .ToListAsync(ct);

        string? activeCategoryName = null;
        List<int>? categoryIds = null;
        if (!string.IsNullOrWhiteSpace(f.CategorySlug))
        {
            var root = cats.FirstOrDefault(c => c.Slug == f.CategorySlug);
            if (root != null)
            {
                activeCategoryName = root.Name;
                // دسته + همه زیرشاخه‌ها (پیمایش درخت در حافظه)
                categoryIds = new List<int> { root.Id };
                var frontier = new Queue<int>();
                frontier.Enqueue(root.Id);
                while (frontier.Count > 0)
                {
                    var pid = frontier.Dequeue();
                    foreach (var child in cats.Where(c => c.ParentId == pid))
                    {
                        categoryIds.Add(child.Id);
                        frontier.Enqueue(child.Id);
                    }
                }
            }
        }

        // ---- کوئری پایه + فیلترها (سمت سرور) ----
        var q = _db.Products.AsNoTracking().Where(p => p.IsActive);

        if (categoryIds != null)
            q = q.Where(p => categoryIds.Contains(p.CategoryId));

        if (!string.IsNullOrWhiteSpace(f.BrandSlug))
            q = q.Where(p => p.Brand.Slug == f.BrandSlug);

        // f.ColorId اکنون = VariantAttributeValue.Id (مقدار ویژگیِ رنگ)
        if (f.ColorId is int cid)
            q = q.Where(p => p.Variants.Any(v => v.Values.Any(x => x.AttributeValueId == cid)));

        if (f.InStockOnly)
            q = q.Where(p => p.Stock > 0);

        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var term = f.Search.Trim();
            q = q.Where(p => p.Title.Contains(term)
                || (p.ShortDescription != null && p.ShortDescription.Contains(term))
                || p.Brand.Name.Contains(term));
        }

        // قیمت نهایی = DiscountPrice ?? Price
        if (f.PriceMin is long pmin)
            q = q.Where(p => (p.DiscountPrice ?? p.Price) >= pmin);
        if (f.PriceMax is long pmax)
            q = q.Where(p => (p.DiscountPrice ?? p.Price) <= pmax);

        // ---- سورت ----
        q = f.Sort switch
        {
            "oldest" => q.OrderBy(p => p.CreatedAt),
            "price-asc" => q.OrderBy(p => p.DiscountPrice ?? p.Price),
            "price-desc" => q.OrderByDescending(p => p.DiscountPrice ?? p.Price),
            "popular" => q.OrderByDescending(p => p.ViewCount),
            _ => q.OrderByDescending(p => p.CreatedAt),
        };

        var total = await q.CountAsync(ct);

        var products = await q
            .Skip((f.Page - 1) * f.PageSize)
            .Take(f.PageSize)
            .Select(p => new ProductCardDto
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                BrandName = p.Brand.Name,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                DiscountPercent = p.DiscountPrice != null && p.DiscountPrice < p.Price && p.Price > 0
                    ? (int)Math.Round((p.Price - p.DiscountPrice.Value) * 100.0 / p.Price)
                    : 0,
                PrimaryImageUrl = p.Images
                    .OrderByDescending(i => i.IsMain).ThenBy(i => i.DisplayOrder)
                    .Select(i => i.Url).FirstOrDefault(),
                ColorHexes = p.Variants
                    .SelectMany(v => v.Values)
                    .Where(x => x.AttributeValue.ColorHex != null)
                    .Select(x => x.AttributeValue.ColorHex!)
                    .Distinct().ToList(),
                CategorySlug = p.Category.Slug
            })
            .ToListAsync(ct);

        // ---- facetها (روی همه محصولات فعال، مستقل از فیلتر فعلی) ----
        var activeProducts = _db.Products.AsNoTracking().Where(p => p.IsActive);

        var priceFloor = await activeProducts.Select(p => (long?)(p.DiscountPrice ?? p.Price)).MinAsync(ct) ?? 0;
        var priceCeil = await activeProducts.Select(p => (long?)(p.DiscountPrice ?? p.Price)).MaxAsync(ct) ?? 0;

        var brands = await _db.Brands.AsNoTracking()
            .Where(b => b.IsActive && b.Products.Any(p => p.IsActive))
            .OrderBy(b => b.DisplayOrder)
            .Select(b => new BrandFilterDto { Name = b.Name, Slug = b.Slug, Active = b.Slug == f.BrandSlug })
            .ToListAsync(ct);

        // facet رنگ از مقادیر ویژگیِ رنگ (Attribute.IsColor) ساخته می‌شود
        var colors = await _db.VariantAttributeValues.AsNoTracking()
            .Where(av => av.Attribute.IsColor && av.ColorHex != null
                && av.VariantValues.Any(vv => vv.ProductVariant.Product.IsActive))
            .OrderBy(av => av.DisplayOrder).ThenBy(av => av.Value)
            .Select(av => new ColorFilterDto { Id = av.Id, Name = av.Value, Hex = av.ColorHex!, Active = av.Id == f.ColorId })
            .ToListAsync(ct);

        // ---- درخت دسته‌ها برای sidebar ----
        var nodes = cats.ToDictionary(
            c => c.Id,
            c => new CategoryFilterDto { Name = c.Name, Slug = c.Slug, Active = c.Slug == f.CategorySlug });
        var roots = new List<CategoryFilterDto>();
        foreach (var c in cats)
        {
            var node = nodes[c.Id];
            if (c.ParentId is int pid && nodes.TryGetValue(pid, out var parent))
                parent.Children.Add(node);
            else
                roots.Add(node);
        }

        return new CatalogViewModel
        {
            Products = products,
            TotalCount = total,
            Page = f.Page,
            PageSize = f.PageSize,
            Categories = roots,
            Colors = colors,
            Brands = brands,
            PriceFloor = priceFloor,
            PriceCeil = priceCeil,
            Applied = f,
            ActiveCategoryName = activeCategoryName
        };
    }
}
