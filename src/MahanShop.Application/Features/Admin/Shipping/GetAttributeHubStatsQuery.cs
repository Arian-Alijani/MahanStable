using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Shipping;

/// <summary>
/// شمارش‌های لازم برای hub «کنترل ویژگی» — یک رفت‌وبرگشت به DB.
/// شامل دسته‌بندی‌ها، برندها، ویژگی‌های متغیر، مشخصات فنی، برچسب‌ها و نوع‌های پست.
/// </summary>
public record GetAttributeHubStatsQuery : IRequest<AttributeHubStatsDto>;

public class AttributeHubStatsDto
{
    public int CategoryCount { get; set; }
    public int BrandCount { get; set; }
    public int VariantAttributeCount { get; set; }
    public int FeatureCount { get; set; }
    public int TagCount { get; set; }
    public int ShippingMethodCount { get; set; }
}

public class GetAttributeHubStatsQueryHandler : IRequestHandler<GetAttributeHubStatsQuery, AttributeHubStatsDto>
{
    private readonly IApplicationDbContext _db;
    public GetAttributeHubStatsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<AttributeHubStatsDto> Handle(GetAttributeHubStatsQuery request, CancellationToken ct)
    {
        var categoryCount      = await _db.Categories.CountAsync(ct);
        var brandCount         = await _db.Brands.CountAsync(ct);
        var variantAttrCount   = await _db.VariantAttributes.CountAsync(ct);
        var featureCount       = await _db.Features.CountAsync(ct);
        var tagCount           = await _db.Tags.CountAsync(ct);
        var shippingCount      = await _db.ShippingMethods.CountAsync(ct);

        return new AttributeHubStatsDto
        {
            CategoryCount        = categoryCount,
            BrandCount           = brandCount,
            VariantAttributeCount = variantAttrCount,
            FeatureCount         = featureCount,
            TagCount             = tagCount,
            ShippingMethodCount  = shippingCount
        };
    }
}
