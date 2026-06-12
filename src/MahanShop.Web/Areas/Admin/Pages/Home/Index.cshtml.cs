using MahanShop.Application.Features.Admin.Home;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Home;

/// <summary>
/// مدیریت «صفحهٔ اصلی» — نوارهای محصول و بنرهای میانی در یک لیستِ مرتب با ترتیب مشترک.
/// تغییر عدد ترتیب، فعال/غیرفعال، حذف، و افزودن نوار/بنر جدید.
/// دسترسی از policy AdminOnly روی کل Area تأمین می‌شود.
/// </summary>
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    public IndexModel(IMediator mediator) => _mediator = mediator;

    public List<HomeSectionAdminDto> Sections { get; private set; } = new();

    public async Task OnGetAsync() =>
        Sections = await _mediator.Send(new GetHomeSectionsQuery());

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
