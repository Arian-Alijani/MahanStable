using MahanShop.Application.Features.Admin.Home;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Home;

/// <summary>
/// مدیریت «صفحهٔ اصلی» — بنرهای هیرو (بالای صفحه)، نوارهای محصول، بنرهای میانی،
/// و بخش «گرید دسته‌بندی منتخب» (انتخاب دسته‌ها + مرتب‌سازی هوشمند + سبک نمایش).
/// دسترسی از policy AdminOnly روی کل Area تأمین می‌شود.
/// </summary>
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    public IndexModel(IMediator mediator) => _mediator = mediator;

    public List<HeroBannerAdminDto> HeroBanners { get; private set; } = new();
    public List<HomeSectionAdminDto> Sections { get; private set; } = new();
    public HomeCategoryBoardDto CategoryBoard { get; private set; } = new();

    public async Task OnGetAsync()
    {
        HeroBanners = await _mediator.Send(new GetHeroBannersQuery());
        Sections = await _mediator.Send(new GetHomeSectionsQuery());
        CategoryBoard = await _mediator.Send(new GetHomeCategoryBoardQuery());
    }

    /* ---------- بنر هیرو (بالای صفحه) ---------- */

    public async Task<IActionResult> OnPostHeroOrderAsync(int id, int displayOrder)
    {
        await _mediator.Send(new SetHeroBannerOrderCommand(id, displayOrder));
        TempData["AdminOk"] = "ترتیب بنر بالای صفحه به‌روزرسانی شد.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostHeroToggleAsync(int id)
    {
        await _mediator.Send(new ToggleHeroBannerActiveCommand(id));
        TempData["AdminOk"] = "وضعیت بنر بالای صفحه تغییر کرد.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostHeroDeleteAsync(int id)
    {
        await _mediator.Send(new DeleteHeroBannerCommand(id));
        TempData["AdminOk"] = "بنر بالای صفحه حذف شد.";
        return RedirectToPage();
    }

    /* ---------- نوارهای محصول و بنرهای میانی ---------- */

    public async Task<IActionResult> OnPostOrderAsync(int id, int displayOrder)
    {
        await _mediator.Send(new SetHomeSectionOrderCommand(id, displayOrder));
        TempData["AdminOk"] = "ترتیب به‌روزرسانی شد.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        await _mediator.Send(new ToggleHomeSectionActiveCommand(id));
        TempData["AdminOk"] = "وضعیت نوار تغییر کرد.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _mediator.Send(new DeleteHomeSectionCommand(id));
        TempData["AdminOk"] = "نوار حذف شد.";
        return RedirectToPage();
    }

    /* ---------- گرید دسته‌بندی صفحهٔ اصلی ---------- */

    public async Task<IActionResult> OnPostCatAddAsync(int categoryId)
    {
        await _mediator.Send(new AddHomeCategoryCommand(categoryId));
        TempData["AdminOk"] = "دسته به صفحهٔ اصلی افزوده شد.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCatRemoveAsync(int categoryId)
    {
        await _mediator.Send(new RemoveHomeCategoryCommand(categoryId));
        TempData["AdminOk"] = "دسته از صفحهٔ اصلی حذف شد.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCatMoveAsync(int categoryId, bool up)
    {
        await _mediator.Send(new MoveHomeCategoryCommand(categoryId, up));
        TempData["AdminOk"] = "ترتیب دسته‌ها به‌روزرسانی شد.";
        return RedirectToPage();
    }

    /// <summary>مرتب‌سازی هوشمند با درگ‌ودراپ — لیست مرتب‌شدهٔ ID از فرم.</summary>
    public async Task<IActionResult> OnPostCatReorderAsync(string orderedIds)
    {
        var ids = (orderedIds ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out var n) ? n : 0)
            .Where(n => n > 0).ToList();

        if (ids.Count > 0)
        {
            await _mediator.Send(new ReorderHomeCategoriesCommand(ids));
            TempData["AdminOk"] = "ترتیب دسته‌ها ذخیره شد.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCatSettingsAsync(
        HomeCategoryStyle style, bool isActive, int displayOrder, string? title)
    {
        await _mediator.Send(new UpdateHomeCategorySettingsCommand(style, isActive, displayOrder, title));
        TempData["AdminOk"] = "تنظیمات گرید دسته‌بندی ذخیره شد.";
        return RedirectToPage();
    }
}
