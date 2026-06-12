using FluentValidation;
using MahanShop.Application.Features.Admin.Variants;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Variants;

/// <summary>لیست ویژگی‌های متغیر + حذف.</summary>
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    public IndexModel(IMediator mediator) => _mediator = mediator;

    public List<VariantAttributeListItemDto> Attributes { get; private set; } = new();
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }

    public async Task OnGetAsync() =>
        Attributes = await _mediator.Send(new GetVariantAttributesQuery(Search));

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            await _mediator.Send(new DeleteVariantAttributeCommand(id));
            TempData["AdminOk"] = "ویژگی حذف شد.";
        }
        catch (ValidationException ex)
        {
            TempData["AdminErr"] = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "خطا در حذف.";
        }
        return RedirectToPage(new { Search });
    }
}
