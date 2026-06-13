using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Entities;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Home;

/* =========================================================================
 * مدیریت بخش «گرید دسته‌بندی صفحهٔ اصلی»
 * - افزودن/حذف دسته از صفحهٔ اصلی  (Category.ShowOnHome)
 * - مرتب‌سازی هوشمند با لیست مرتب‌شدهٔ ID (drag & drop)  (Category.DisplayOrder)
 * - تنظیم سبک نمایش / فعال‌بودن / ترتیب بخش / عنوان  (HomeSection نوع CategoryGrid)
 * هیچ‌کدام نیاز به مهاجرت دیتابیس ندارد.
 * ========================================================================= */

/// <summary>افزودن یک دسته به گرید صفحهٔ اصلی (در انتهای ترتیب).</summary>
public record AddHomeCategoryCommand(int CategoryId) : IRequest<Unit>;

public class AddHomeCategoryCommandHandler : IRequestHandler<AddHomeCategoryCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    public AddHomeCategoryCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(AddHomeCategoryCommand request, CancellationToken ct)
    {
        var cat = await _db.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, ct)
            ?? throw new ValidationException("دسته یافت نشد.");

        if (!cat.ShowOnHome)
        {
            var maxOrder = await _db.Categories.Where(c => c.ShowOnHome)
                .Select(c => (int?)c.DisplayOrder).MaxAsync(ct) ?? 0;
            cat.ShowOnHome = true;
            cat.DisplayOrder = maxOrder + 1;
            cat.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
        return Unit.Value;
    }
}

/// <summary>حذف یک دسته از گرید صفحهٔ اصلی (در منو/کاتالوگ باقی می‌ماند).</summary>
public record RemoveHomeCategoryCommand(int CategoryId) : IRequest<Unit>;

public class RemoveHomeCategoryCommandHandler : IRequestHandler<RemoveHomeCategoryCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    public RemoveHomeCategoryCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(RemoveHomeCategoryCommand request, CancellationToken ct)
    {
        var cat = await _db.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, ct)
            ?? throw new ValidationException("دسته یافت نشد.");
        if (cat.ShowOnHome)
        {
            cat.ShowOnHome = false;
            cat.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
        return Unit.Value;
    }
}

/// <summary>
/// مرتب‌سازی هوشمند: ترتیب نهایی دسته‌های منتخب با لیست مرتب‌شدهٔ ID.
/// (اندیس ۰ = اولین تایل). فقط روی دسته‌های موجود در لیست اعمال می‌شود.
/// </summary>
public record ReorderHomeCategoriesCommand(IReadOnlyList<int> OrderedIds) : IRequest<Unit>;

public class ReorderHomeCategoriesCommandHandler : IRequestHandler<ReorderHomeCategoriesCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    public ReorderHomeCategoriesCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(ReorderHomeCategoriesCommand request, CancellationToken ct)
    {
        if (request.OrderedIds is null || request.OrderedIds.Count == 0) return Unit.Value;

        var ids = request.OrderedIds.Distinct().ToList();
        var cats = await _db.Categories.Where(c => ids.Contains(c.Id)).ToListAsync(ct);

        for (var i = 0; i < request.OrderedIds.Count; i++)
        {
            var cat = cats.FirstOrDefault(c => c.Id == request.OrderedIds[i]);
            if (cat is null) continue;
            cat.DisplayOrder = i + 1;
            cat.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

/// <summary>جابه‌جایی یک دسته یک پله بالا/پایین (دکمه‌ای — برای محیط بدون درگ).</summary>
public record MoveHomeCategoryCommand(int CategoryId, bool Up) : IRequest<Unit>;

public class MoveHomeCategoryCommandHandler : IRequestHandler<MoveHomeCategoryCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    public MoveHomeCategoryCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(MoveHomeCategoryCommand request, CancellationToken ct)
    {
        var selected = await _db.Categories
            .Where(c => c.ShowOnHome)
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .ToListAsync(ct);

        // نرمال‌سازی ترتیب‌ها به ۱..n (در صورت وجود ناهماهنگی)
        for (var i = 0; i < selected.Count; i++) selected[i].DisplayOrder = i + 1;

        var idx = selected.FindIndex(c => c.Id == request.CategoryId);
        if (idx < 0) return Unit.Value;

        var swap = request.Up ? idx - 1 : idx + 1;
        if (swap < 0 || swap >= selected.Count) { await _db.SaveChangesAsync(ct); return Unit.Value; }

        (selected[idx].DisplayOrder, selected[swap].DisplayOrder) =
            (selected[swap].DisplayOrder, selected[idx].DisplayOrder);

        selected[idx].UpdatedAt = selected[swap].UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

/// <summary>تنظیم سبک نمایش، فعال‌بودن، عنوان و عدد ترتیب بخش گرید دسته‌بندی.</summary>
public record UpdateHomeCategorySettingsCommand(
    HomeCategoryStyle Style, bool IsActive, int DisplayOrder, string? Title) : IRequest<Unit>;

public class UpdateHomeCategorySettingsCommandValidator : AbstractValidator<UpdateHomeCategorySettingsCommand>
{
    public UpdateHomeCategorySettingsCommandValidator()
    {
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0).WithMessage("عدد ترتیب نمی‌تواند منفی باشد.");
        RuleFor(x => x.Title).MaximumLength(150).WithMessage("عنوان حداکثر ۱۵۰ نویسه.");
    }
}

public class UpdateHomeCategorySettingsCommandHandler : IRequestHandler<UpdateHomeCategorySettingsCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    public UpdateHomeCategorySettingsCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(UpdateHomeCategorySettingsCommand request, CancellationToken ct)
    {
        var row = await _db.HomeSections
            .FirstOrDefaultAsync(x => x.SectionType == HomeSectionType.CategoryGrid, ct);

        var title = string.IsNullOrWhiteSpace(request.Title)
            ? HomeCategorySettings.DefaultTitle : request.Title!.Trim();

        if (row is null)
        {
            row = new HomeSection
            {
                SectionType = HomeSectionType.CategoryGrid,
                Title = title,
                Subtitle = request.Style.ToString(),
                IsActive = request.IsActive,
                DisplayOrder = request.DisplayOrder,
                CreatedAt = DateTime.UtcNow
            };
            _db.HomeSections.Add(row);
        }
        else
        {
            row.Title = title;
            row.Subtitle = request.Style.ToString();
            row.IsActive = request.IsActive;
            row.DisplayOrder = request.DisplayOrder;
            row.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
