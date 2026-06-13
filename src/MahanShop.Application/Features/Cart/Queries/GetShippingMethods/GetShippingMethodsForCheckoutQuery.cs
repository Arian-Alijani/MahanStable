using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Cart.Queries.GetShippingMethods;

/// <summary>لیست روش‌های ارسال فعال برای نمایش در صفحه checkout. نرخ از DB — هرگز از client.</summary>
public record GetShippingMethodsForCheckoutQuery : IRequest<List<ShippingMethodCheckoutDto>>;

public class ShippingMethodCheckoutDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public long Cost { get; set; }
    public string? Description { get; set; }
}

public class GetShippingMethodsForCheckoutQueryHandler
    : IRequestHandler<GetShippingMethodsForCheckoutQuery, List<ShippingMethodCheckoutDto>>
{
    private readonly IApplicationDbContext _db;
    public GetShippingMethodsForCheckoutQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<ShippingMethodCheckoutDto>> Handle(
        GetShippingMethodsForCheckoutQuery request, CancellationToken ct) =>
        await _db.ShippingMethods
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new ShippingMethodCheckoutDto
            {
                Id = s.Id,
                Name = s.Name,
                Cost = s.Cost,
                Description = s.Description
            })
            .ToListAsync(ct);
}
