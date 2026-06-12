using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Brands;

/// <summary>لیست همه برندها برای ادمین (با جستجوی اختیاری روی نام).</summary>
public record GetBrandsQuery(string? Search = null) : IRequest<List<BrandListItemDto>>;

public class GetBrandsQueryHandler : IRequestHandler<GetBrandsQuery, List<BrandListItemDto>>
{
    private readonly IApplicationDbContext _db;
    public GetBrandsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<BrandListItemDto>> Handle(GetBrandsQuery request, CancellationToken ct)
    {
        var q = _db.Brands.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            q = q.Where(b => b.Name.Contains(term) || b.Slug.Contains(term));
        }

        return await q
            .OrderBy(b => b.DisplayOrder).ThenBy(b => b.Name)
            .Select(b => new BrandListItemDto
            {
                Id = b.Id,
                Name = b.Name,
                Slug = b.Slug,
                LogoUrl = b.LogoUrl,
                IsActive = b.IsActive,
                DisplayOrder = b.DisplayOrder,
                ProductCount = b.Products.Count
            })
            .ToListAsync(ct);
    }
}
