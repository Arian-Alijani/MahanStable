using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Shipping;

/// <summary>لیست همه نوع‌های پست برای پنل ادمین.</summary>
public record GetShippingMethodsQuery(string? Search = null) : IRequest<List<ShippingMethodListItemDto>>;

public class GetShippingMethodsQueryHandler : IRequestHandler<GetShippingMethodsQuery, List<ShippingMethodListItemDto>>
{
    private readonly IApplicationDbContext _db;
    public GetShippingMethodsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<ShippingMethodListItemDto>> Handle(GetShippingMethodsQuery request, CancellationToken ct)
    {
        var q = _db.ShippingMethods.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            q = q.Where(s => s.Name.Contains(term));
        }

        return await q
            .OrderBy(s => s.DisplayOrder).ThenBy(s => s.Name)
            .Select(s => new ShippingMethodListItemDto
            {
                Id = s.Id,
                Name = s.Name,
                Cost = s.Cost,
                IsActive = s.IsActive,
                DisplayOrder = s.DisplayOrder,
                Description = s.Description
            })
            .ToListAsync(ct);
    }
}
