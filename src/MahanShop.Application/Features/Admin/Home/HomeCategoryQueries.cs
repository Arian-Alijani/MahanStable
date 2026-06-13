using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Home;

/// <summary>
/// خواندن وضعیت بخش «گرید دسته‌بندی صفحهٔ اصلی»:
/// دسته‌های منتخب (روی صفحهٔ اصلی)، دسته‌های قابل افزودن، و تنظیمات بخش (سبک/فعال/ترتیب).
/// تنظیمات بخش در یک رکورد HomeSection از نوع CategoryGrid نگه‌داری می‌شود (بدون مهاجرت).
/// </summary>
public record GetHomeCategoryBoardQuery : IRequest<HomeCategoryBoardDto>;

public class GetHomeCategoryBoardQueryHandler : IRequestHandler<GetHomeCategoryBoardQuery, HomeCategoryBoardDto>
{
    private readonly IApplicationDbContext _db;
    public GetHomeCategoryBoardQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<HomeCategoryBoardDto> Handle(GetHomeCategoryBoardQuery request, CancellationToken ct)
    {
        var cats = await _db.Categories.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .Select(c => new HomeCategoryItemDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                ImageUrl = c.ImageUrl,
                ShowOnHome = c.ShowOnHome,
                DisplayOrder = c.DisplayOrder,
                ProductCount = c.Products.Count(p => p.IsActive)
            })
            .ToListAsync(ct);

        var board = new HomeCategoryBoardDto
        {
            Selected = cats.Where(c => c.ShowOnHome)
                           .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name).ToList(),
            Available = cats.Where(c => !c.ShowOnHome)
                            .OrderBy(c => c.Name).ToList()
        };

        var setting = await HomeCategorySettings.GetAsync(_db, ct);
        board.Style = setting.Style;
        board.IsActive = setting.IsActive;
        board.DisplayOrder = setting.DisplayOrder;
        board.Title = setting.Title;

        return board;
    }
}

/// <summary>
/// کمک‌متدهای مشترک برای خواندن/ساختِ رکورد تنظیماتِ گرید دسته‌بندی (HomeSection از نوع CategoryGrid).
/// سبک نمایش در Subtitle (نام enum) و عنوان در Title نگه‌داری می‌شود.
/// </summary>
public static class HomeCategorySettings
{
    public const string DefaultTitle = "خرید بر اساس دسته‌بندی";

    public sealed record Snapshot(HomeCategoryStyle Style, bool IsActive, int DisplayOrder, string Title);

    public static async Task<Snapshot> GetAsync(IApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.HomeSections.AsNoTracking()
            .FirstOrDefaultAsync(x => x.SectionType == HomeSectionType.CategoryGrid, ct);

        if (row is null)
            return new Snapshot(HomeCategoryStyle.Bento, true, 0, DefaultTitle);

        var style = Enum.TryParse<HomeCategoryStyle>(row.Subtitle, out var s) ? s : HomeCategoryStyle.Bento;
        var title = string.IsNullOrWhiteSpace(row.Title) ? DefaultTitle : row.Title;
        return new Snapshot(style, row.IsActive, row.DisplayOrder, title);
    }
}
