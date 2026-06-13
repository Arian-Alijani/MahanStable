using FluentValidation;
using MahanShop.Application.Features.Admin.Shipping;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Shipping;

/// <summary>لیست نوع‌های پست + جستجو + toggle فعال/غیرفعال + حذف.</summary>
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    public IndexModel(IMediator mediator) => _mediator = mediator;

    public List<ShippingMethodListItemDto> Methods { get; private set; } = new();
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }

    public async Task OnGetAsync() =>
        Methods = await _mediator.Send(new GetShippingMethodsQuery(Search));

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        try
        {
            await _mediator.Send(new ToggleShippingMethodActiveCommand(id));
            TempData["AdminOk"] = "وضعیت روش ارسال تغییر کرد.";
        }
        catch (ValidationException ex)
        {
            TempData["AdminErr"] = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "خطایی رخ داد.";
        }
        return RedirectToPage(new { Search });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            await _mediator.Send(new DeleteShippingMethodCommand(id));
            TempData["AdminOk"] = "روش ارسال حذف شد.";
        }
        catch (ValidationException ex)
        {
            TempData["AdminErr"] = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "خطایی رخ داد.";
        }
        return RedirectToPage(new { Search });
    }
}
