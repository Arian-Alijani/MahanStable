using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Home.Queries.GetMenuCategories;

public class GetMenuCategoriesQueryHandler : IRequestHandler<GetMenuCategoriesQuery, List<MenuCategoryDto>>
{
    private readonly IApplicationDbContext _db;

    public GetMenuCategoriesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<MenuCategoryDto>> Handle(GetMenuCategoriesQuery request, CancellationToken ct)
    {
        // همه دسته‌های منو رو یکجا می‌گیریم و درخت رو در حافظه می‌سازیم (تعداد کم)
        var all = await _db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive && c.ShowInMenu)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new { c.Id, c.Name, c.Slug, c.ImageUrl, c.ParentId })
            .ToListAsync(ct);

        var nodes = all.ToDictionary(
            c => c.Id,
            c => new MenuCategoryDto { Id = c.Id, Name = c.Name, Slug = c.Slug, ImageUrl = c.ImageUrl });

        var roots = new List<MenuCategoryDto>();
        foreach (var c in all)
        {
            var node = nodes[c.Id];
            if (c.ParentId is int pid && nodes.TryGetValue(pid, out var parent))
                parent.Children.Add(node);
            else
                roots.Add(node);
        }

        return roots;
    }
}
