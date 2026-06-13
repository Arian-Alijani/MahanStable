using MahanShop.Application.Features.Admin.Home;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Home;

/// <summary>
/// مدیریت «صفحهٔ اصلی» — بنرهای هیرو (بالای صفحه) و نوارهای محصول و بنرهای میانی.
/// تغییر عدد ترتیب، فعال/غیرفعال، حذف، و افزودن نوار/بنر جدید.
/// دسترسی از policy AdminOnly روی کل Area تأمین می‌شود.
/// </summary>
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    public IndexModel(IMediator mediator) => _mediator = mediator;

    public List<HeroBannerAdminDto> HeroBanners { get; private set; } = new();
    public List<HomeSectionAdminDto> Sections { get; private set; } = new();

    public async Task OnGetAsync()
    {
        HeroBanners = await _mediator.Send(new GetHeroBannersQuery());
        Sections = await _mediator.Send(new GetHomeSectionsQuery());
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
}
