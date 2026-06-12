using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Categories;

/// <summary>لیست همه دسته‌ها به‌صورت مرتب‌شدهٔ درختی (والد بعد فرزند) برای ادمین.</summary>
public record GetCategoriesQuery : IRequest<List<CategoryListItemDto>>;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryListItemDto>>
{
    private readonly IApplicationDbContext _db;
    public GetCategoriesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<CategoryListItemDto>> Handle(GetCategoriesQuery request, CancellationToken ct)
    {
        var all = await _db.Categories.AsNoTracking()
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .Select(c => new CategoryListItemDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                ParentId = c.ParentId,
                IsActive = c.IsActive,
                ShowInMenu = c.ShowInMenu,
                ShowOnHome = c.ShowOnHome,
                DisplayOrder = c.DisplayOrder,
                ProductCount = c.Products.Count
            })
            .ToListAsync(ct);

        var byParent = all.ToLookup(c => c.ParentId);
        var nameById = all.ToDictionary(c => c.Id, c => c.Name);
        var ordered = new List<CategoryListItemDto>();

        void Walk(int? parentId, int depth)
        {
            foreach (var c in byParent[parentId])
            {
                c.Depth = depth;
                c.ParentName = c.ParentId is int p && nameById.TryGetValue(p, out var n) ? n : null;
                ordered.Add(c);
                Walk(c.Id, depth + 1);
            }
        }
        Walk(null, 0);
        return ordered;
    }
}
