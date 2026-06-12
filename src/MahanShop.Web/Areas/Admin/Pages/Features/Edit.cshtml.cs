using FluentValidation;
using MahanShop.Application.Features.Admin.Features;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Features;

/// <summary>ویرایش مشخصه فنی موجود.</summary>
public class EditModel : PageModel
{
    private readonly IMediator _mediator;
    public EditModel(IMediator mediator) => _mediator = mediator;

    [BindProperty] public int Id { get; set; }
    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public int DisplayOrder { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var f = await _mediator.Send(new GetFeatureForEditQuery(id));
        if (f is null) return NotFound();
        Id = f.Id; Name = f.Name; DisplayOrder = f.DisplayOrder;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            await _mediator.Send(new UpdateFeatureCommand(Id, Name, DisplayOrder));
            TempData["AdminOk"] = "مشخصه به‌روزرسانی شد.";
            return RedirectToPage("Index");
        }
        catch (ValidationException ex)
        {
            foreach (var e in ex.Errors) ModelState.AddModelError(string.Empty, e.ErrorMessage);
            return Page();
        }
    }
}
