using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Variants;

/// <summary>یک attribute برای فرم ویرایش. null اگر یافت نشد.</summary>
public record GetVariantAttributeForEditQuery(int Id) : IRequest<VariantAttributeEditDto?>;

public class GetVariantAttributeForEditQueryHandler : IRequestHandler<GetVariantAttributeForEditQuery, VariantAttributeEditDto?>
{
    private readonly IApplicationDbContext _db;
    public GetVariantAttributeForEditQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<VariantAttributeEditDto?> Handle(GetVariantAttributeForEditQuery request, CancellationToken ct) =>
        await _db.VariantAttributes.AsNoTracking()
            .Where(a => a.Id == request.Id)
            .Select(a => new VariantAttributeEditDto
            {
                Id = a.Id,
                Name = a.Name,
                IsColor = a.IsColor,
                Kind = a.Kind,
                DisplayOrder = a.DisplayOrder
            })
            .FirstOrDefaultAsync(ct);
}

/// <summary>مقادیر یک attribute (pool) برای صفحه مدیریت مقادیر.</summary>
public record GetVariantAttributeValuesQuery(int AttributeId) : IRequest<List<VariantAttributeValueDto>>;

public class GetVariantAttributeValuesQueryHandler : IRequestHandler<GetVariantAttributeValuesQuery, List<VariantAttributeValueDto>>
{
    private readonly IApplicationDbContext _db;
    public GetVariantAttributeValuesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<VariantAttributeValueDto>> Handle(GetVariantAttributeValuesQuery request, CancellationToken ct) =>
        await _db.VariantAttributeValues.AsNoTracking()
            .Where(v => v.AttributeId == request.AttributeId)
            .OrderBy(v => v.DisplayOrder).ThenBy(v => v.Value)
            .Select(v => new VariantAttributeValueDto
            {
                Id = v.Id,
                AttributeId = v.AttributeId,
                Value = v.Value,
                ColorHex = v.ColorHex,
                LogoUrl = v.LogoUrl,
                DisplayOrder = v.DisplayOrder
            })
            .ToListAsync(ct);
}
