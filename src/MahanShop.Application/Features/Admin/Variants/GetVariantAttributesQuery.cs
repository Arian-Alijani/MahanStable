using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Variants;

/// <summary>لیست همه ویژگی‌های متغیر (attribute pool) برای ادمین.</summary>
public record GetVariantAttributesQuery(string? Search = null) : IRequest<List<VariantAttributeListItemDto>>;

public class GetVariantAttributesQueryHandler : IRequestHandler<GetVariantAttributesQuery, List<VariantAttributeListItemDto>>
{
    private readonly IApplicationDbContext _db;
    public GetVariantAttributesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<VariantAttributeListItemDto>> Handle(GetVariantAttributesQuery request, CancellationToken ct)
    {
        var q = _db.VariantAttributes.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            q = q.Where(a => a.Name.Contains(term));
        }

        return await q
            .OrderBy(a => a.DisplayOrder).ThenBy(a => a.Name)
            .Select(a => new VariantAttributeListItemDto
            {
                Id = a.Id,
                Name = a.Name,
                IsColor = a.IsColor,
                DisplayOrder = a.DisplayOrder,
                ValueCount = a.Values.Count
            })
            .ToListAsync(ct);
    }
}
