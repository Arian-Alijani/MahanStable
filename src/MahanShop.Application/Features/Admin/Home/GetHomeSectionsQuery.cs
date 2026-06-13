using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Home;

/// <summary>لیست همهٔ نوارهای صفحهٔ اصلی (نوار محصول + بنر میانی) به‌ترتیب DisplayOrder.</summary>
public record GetHomeSectionsQuery : IRequest<List<HomeSectionAdminDto>>;

public class GetHomeSectionsQueryHandler : IRequestHandler<GetHomeSectionsQuery, List<HomeSectionAdminDto>>
{
    private readonly IApplicationDbContext _db;
    public GetHomeSectionsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<HomeSectionAdminDto>> Handle(GetHomeSectionsQuery request, CancellationToken ct) =>
        await _db.HomeSections.AsNoTracking()
            // رکورد تنظیماتِ گرید دسته‌بندی جدا مدیریت می‌شود؛ در این لیست نمایش داده نشود.
            .Where(s => s.SectionType != HomeSectionType.CategoryGrid)
            .OrderBy(s => s.DisplayOrder).ThenBy(s => s.Id)
            .Select(s => new HomeSectionAdminDto
            {
                Id = s.Id,
                Title = s.Title,
                SectionType = s.SectionType,
                DisplayOrder = s.DisplayOrder,
                IsActive = s.IsActive,
                ProductSource = s.ProductSource,
                CategoryId = s.CategoryId,
                CategoryName = s.Category != null ? s.Category.Name : null,
                MaxItems = s.MaxItems,
                ImageUrl = s.ImageUrl,
                MobileImageUrl = s.MobileImageUrl,
                LinkUrl = s.LinkUrl,
                Subtitle = s.Subtitle
            })
            .ToListAsync(ct);
}
