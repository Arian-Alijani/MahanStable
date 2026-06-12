using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Brands;

/// <summary>دریافت یک برند برای فرم ویرایش. null اگر یافت نشد.</summary>
public record GetBrandForEditQuery(int Id) : IRequest<BrandEditDto?>;

public class GetBrandForEditQueryHandler : IRequestHandler<GetBrandForEditQuery, BrandEditDto?>
{
    private readonly IApplicationDbContext _db;
    public GetBrandForEditQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<BrandEditDto?> Handle(GetBrandForEditQuery request, CancellationToken ct) =>
        await _db.Brands.AsNoTracking()
            .Where(b => b.Id == request.Id)
            .Select(b => new BrandEditDto
            {
                Id = b.Id,
                Name = b.Name,
                Slug = b.Slug,
                LogoUrl = b.LogoUrl,
                IsActive = b.IsActive,
                DisplayOrder = b.DisplayOrder
            })
            .FirstOrDefaultAsync(ct);
}
