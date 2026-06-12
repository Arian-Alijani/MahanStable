using MahanShop.Application.Features.Admin.Brands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Brands;

/// <summary>لیست برندها + جستجو + toggle فعال/غیرفعال.</summary>
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    public IndexModel(IMediator mediator) => _mediator = mediator;

    public List<BrandListItemDto> Brands { get; private set; } = new();
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }

    public async Task OnGetAsync() =>
        Brands = await _mediator.Send(new GetBrandsQuery(Search));

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        await _mediator.Send(new ToggleBrandActiveCommand(id));
        TempData["AdminOk"] = "وضعیت برند تغییر کرد.";
        return RedirectToPage(new { Search });
    }
}
