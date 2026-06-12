using FluentValidation;
using MahanShop.Application.Features.Admin.Tags;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Tags;

/// <summary>لیست برچسب‌ها + حذف.</summary>
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    public IndexModel(IMediator mediator) => _mediator = mediator;

    public List<TagListItemDto> Tags { get; private set; } = new();
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }

    public async Task OnGetAsync() =>
        Tags = await _mediator.Send(new GetTagsQuery(Search));

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            await _mediator.Send(new DeleteTagCommand(id));
            TempData["AdminOk"] = "برچسب حذف شد.";
        }
        catch (ValidationException ex)
        {
            TempData["AdminErr"] = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "خطا.";
        }
        return RedirectToPage(new { Search });
    }
}
