using FluentValidation;
using MahanShop.Application.Features.Admin.Features;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Features;

/// <summary>لیست مشخصه‌های فنی + حذف.</summary>
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    public IndexModel(IMediator mediator) => _mediator = mediator;

    public List<FeatureListItemDto> Features { get; private set; } = new();
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }

    public async Task OnGetAsync() =>
        Features = await _mediator.Send(new GetFeaturesQuery(Search));

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            await _mediator.Send(new DeleteFeatureCommand(id));
            TempData["AdminOk"] = "مشخصه حذف شد.";
        }
        catch (ValidationException ex)
        {
            TempData["AdminErr"] = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "خطا.";
        }
        return RedirectToPage(new { Search });
    }
}
