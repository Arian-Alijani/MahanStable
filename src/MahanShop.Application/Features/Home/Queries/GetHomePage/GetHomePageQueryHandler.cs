using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Entities;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Home.Queries.GetHomePage;

public class GetHomePageQueryHandler : IRequestHandler<GetHomePageQuery, HomePageViewModel>
{
    private readonly IApplicationDbContext _db;

    public GetHomePageQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<HomePageViewModel> Handle(GetHomePageQuery request, CancellationToken ct)
    {
        var vm = new HomePageViewModel
        {
            Banners = await _db.Banners
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new HomeBannerDto
                {
                    ImageUrl = x.ImageUrl,
                    MobileImageUrl = x.MobileImageUrl,
                    LinkUrl = x.LinkUrl,
                    AltText = x.AltText,
                    Title = x.Title
                })
                .ToListAsync(ct),

            FeaturedCategories = await _db.Categories
                .AsNoTracking()
                .Where(x => x.IsActive && x.ShowOnHome)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new FeaturedCategoryDto { Name = x.Name, Slug = x.Slug, ImageUrl = x.ImageUrl })
                .ToListAsync(ct)
        };

        // فروش هر محصول برای BestSelling (fallback ViewCount)
        var soldCounts = await _db.OrderItems
            .AsNoTracking()
            .GroupBy(oi => oi.ProductId)
            .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
            .ToDictionaryAsync(x => x.ProductId, x => x.Qty, ct);

        var sections = await _db.HomeSections
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(ct);

        foreach (var s in sections)
        {
            if (s.SectionType == HomeSectionType.PromoBanner)
            {
                vm.Sections.Add(new HomeSectionViewModel
                {
                    Title = s.Title,
                    SectionType = HomeSectionType.PromoBanner,
                    ImageUrl = s.ImageUrl,
                    MobileImageUrl = s.MobileImageUrl,
                    LinkUrl = s.LinkUrl,
                    Subtitle = s.Subtitle,
                    IsHalfWidth = s.IsHalfWidth
                });
                continue;
            }

            var products = await LoadProductsAsync(s, soldCounts, ct);
            if (products.Count == 0) continue;  // نوار خالی رندر نشه

            vm.Sections.Add(new HomeSectionViewModel
            {
                Title = s.Title,
                SectionType = HomeSectionType.ProductRow,
                Products = products,
                ViewAllUrl = s.ProductSource == HomeProductSource.ByCategory && s.Category != null
                    ? "/" + s.Category.Slug
                    : null
            });
        }

        return vm;
    }

    private async Task<List<ProductCardDto>> LoadProductsAsync(
        HomeSection s, IReadOnlyDictionary<int, int> soldCounts, CancellationToken ct)
    {
        var q = _db.Products.AsNoTracking().Where(p => p.IsActive);

        switch (s.ProductSource)
        {
            case HomeProductSource.Featured:
                q = q.Where(p => p.IsFeatured).OrderByDescending(p => p.ViewCount);
                break;
            case HomeProductSource.Discounted:
                q = q.Where(p => p.DiscountPrice != null && p.DiscountPrice < p.Price)
                     .OrderByDescending(p => p.CreatedAt);
                break;
            case HomeProductSource.Newest:
                q = q.OrderByDescending(p => p.CreatedAt);
                break;
            case HomeProductSource.ByCategory:
                q = q.Where(p => p.CategoryId == s.CategoryId).OrderByDescending(p => p.CreatedAt);
                break;
            case HomeProductSource.BestSelling:
                // مرتب‌سازی فروش در حافظه (fallback ViewCount) — کاندیدها رو می‌گیریم
                q = q.OrderByDescending(p => p.ViewCount);
                break;
            default:
                q = q.OrderByDescending(p => p.CreatedAt);
                break;
        }

        var rows = await q
            .Take(s.SectionType == HomeSectionType.ProductRow ? s.MaxItems * 2 : s.MaxItems)
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

        if (s.ProductSource == HomeProductSource.BestSelling)
            rows = rows.OrderByDescending(r => soldCounts.TryGetValue(r.Id, out var n) ? n : 0).ToList();

        return rows.Take(s.MaxItems).ToList();
    }
}
