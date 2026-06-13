using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Shipping;

/// <summary>دریافت یک نوع پست برای فرم ویرایش. null اگر یافت نشد.</summary>
public record GetShippingMethodForEditQuery(int Id) : IRequest<ShippingMethodEditDto?>;

public class GetShippingMethodForEditQueryHandler : IRequestHandler<GetShippingMethodForEditQuery, ShippingMethodEditDto?>
{
    private readonly IApplicationDbContext _db;
    public GetShippingMethodForEditQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ShippingMethodEditDto?> Handle(GetShippingMethodForEditQuery request, CancellationToken ct) =>
        await _db.ShippingMethods.AsNoTracking()
            .Where(s => s.Id == request.Id)
            .Select(s => new ShippingMethodEditDto
            {
                Id = s.Id,
                Name = s.Name,
                Cost = s.Cost,
                IsActive = s.IsActive,
                DisplayOrder = s.DisplayOrder,
                Description = s.Description
            })
            .FirstOrDefaultAsync(ct);
}
