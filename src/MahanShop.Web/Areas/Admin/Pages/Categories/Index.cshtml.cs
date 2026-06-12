using MahanShop.Application.Features.Admin.Categories;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Categories;

/// <summary>لیست درختی دسته‌ها + toggle.</summary>
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    public IndexModel(IMediator mediator) => _mediator = mediator;

    public List<CategoryListItemDto> Categories { get; private set; } = new();

    public async Task OnGetAsync() =>
        Categories = await _mediator.Send(new GetCategoriesQuery());

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        await _mediator.Send(new ToggleCategoryActiveCommand(id));
        TempData["AdminOk"] = "وضعیت دسته تغییر کرد.";
        return RedirectToPage();
    }
}
