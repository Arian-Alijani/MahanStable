using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Features;

/// <summary>لیست همه مشخصه‌های فنی برای ادمین.</summary>
public record GetFeaturesQuery(string? Search = null) : IRequest<List<FeatureListItemDto>>;

public class GetFeaturesQueryHandler : IRequestHandler<GetFeaturesQuery, List<FeatureListItemDto>>
{
    private readonly IApplicationDbContext _db;
    public GetFeaturesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<FeatureListItemDto>> Handle(GetFeaturesQuery request, CancellationToken ct)
    {
        var q = _db.Features.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            q = q.Where(f => f.Name.Contains(term));
        }

        return await q
            .OrderBy(f => f.DisplayOrder).ThenBy(f => f.Name)
            .Select(f => new FeatureListItemDto
            {
                Id = f.Id,
                Name = f.Name,
                DisplayOrder = f.DisplayOrder,
                UsageCount = f.ProductFeatures.Count
            })
            .ToListAsync(ct);
    }
}

/// <summary>دریافت یک مشخصه فنی برای فرم ویرایش.</summary>
public record GetFeatureForEditQuery(int Id) : IRequest<FeatureEditDto?>;

public class GetFeatureForEditQueryHandler : IRequestHandler<GetFeatureForEditQuery, FeatureEditDto?>
{
    private readonly IApplicationDbContext _db;
    public GetFeatureForEditQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<FeatureEditDto?> Handle(GetFeatureForEditQuery request, CancellationToken ct) =>
        await _db.Features.AsNoTracking()
            .Where(f => f.Id == request.Id)
            .Select(f => new FeatureEditDto { Id = f.Id, Name = f.Name, DisplayOrder = f.DisplayOrder })
            .FirstOrDefaultAsync(ct);
}
