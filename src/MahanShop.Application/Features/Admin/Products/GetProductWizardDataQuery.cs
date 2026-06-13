using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Products;

/// <summary>
/// دادهٔ ویزارد محصولِ چندبرندی: برندها (مقادیرِ ویژگیِ Brand) به‌همراه گوشی‌های زیرمجموعه
/// (مقادیرِ ویژگیِ Model که ParentValueId آن‌ها = این برند) + رنگ‌های موجود.
/// </summary>
public record GetProductWizardDataQuery() : IRequest<ProductWizardDataDto>;

public class GetProductWizardDataQueryHandler : IRequestHandler<GetProductWizardDataQuery, ProductWizardDataDto>
{
    private readonly IApplicationDbContext _db;
    public GetProductWizardDataQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ProductWizardDataDto> Handle(GetProductWizardDataQuery request, CancellationToken ct)
    {
        var brandAttr = await _db.VariantAttributes.AsNoTracking()
            .Where(a => a.Kind == VariantAttributeKind.Brand)
            .OrderBy(a => a.DisplayOrder).FirstOrDefaultAsync(ct);
        var modelAttr = await _db.VariantAttributes.AsNoTracking()
            .Where(a => a.Kind == VariantAttributeKind.Model)
            .OrderBy(a => a.DisplayOrder).FirstOrDefaultAsync(ct);
        var colorAttr = await _db.VariantAttributes.AsNoTracking()
            .Where(a => a.Kind == VariantAttributeKind.Color)
            .OrderBy(a => a.DisplayOrder).FirstOrDefaultAsync(ct);

        var dto = new ProductWizardDataDto
        {
            HasBrandModelAttributes = brandAttr is not null && modelAttr is not null,
            BrandAttributeId = brandAttr?.Id ?? 0,
            ModelAttributeId = modelAttr?.Id ?? 0,
            ColorAttributeId = colorAttr?.Id
        };

        if (!dto.HasBrandModelAttributes) return dto;

        // برندها
        var brands = await _db.VariantAttributeValues.AsNoTracking()
            .Where(v => v.AttributeId == brandAttr!.Id)
            .OrderBy(v => v.DisplayOrder).ThenBy(v => v.Value)
            .Select(v => new WizardBrandDto { ValueId = v.Id, Name = v.Value, LogoUrl = v.LogoUrl })
            .ToListAsync(ct);

        // مدل‌ها (به‌همراه برندِ والدشان)
        var models = await _db.VariantAttributeValues.AsNoTracking()
            .Where(v => v.AttributeId == modelAttr!.Id)
            .OrderBy(v => v.DisplayOrder).ThenBy(v => v.Value)
            .Select(v => new { v.Id, v.Value, v.ParentValueId })
            .ToListAsync(ct);

        var byBrand = models.Where(m => m.ParentValueId.HasValue)
            .GroupBy(m => m.ParentValueId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var b in brands)
        {
            if (byBrand.TryGetValue(b.ValueId, out var ms))
                b.Models = ms.Select(m => new WizardModelDto { ValueId = m.Id, Name = m.Value }).ToList();
        }

        dto.Brands = brands;
        dto.UnlinkedModelCount = models.Count(m => !m.ParentValueId.HasValue);

        if (colorAttr is not null)
        {
            dto.Colors = await _db.VariantAttributeValues.AsNoTracking()
                .Where(v => v.AttributeId == colorAttr.Id)
                .OrderBy(v => v.DisplayOrder).ThenBy(v => v.Value)
                .Select(v => new WizardColorDto { ValueId = v.Id, Name = v.Value, ColorHex = v.ColorHex })
                .ToListAsync(ct);
        }

        return dto;
    }
}
