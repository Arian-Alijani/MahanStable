using FluentValidation;
using MahanShop.Application.Features.Admin.Tags;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Tags;

/// <summary>ویرایش برچسب موجود.</summary>
public class EditModel : PageModel
{
    private readonly IMediator _mediator;
    public EditModel(IMediator mediator) => _mediator = mediator;

    [BindProperty] public int Id { get; set; }
    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public string? Slug { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var t = await _mediator.Send(new GetTagForEditQuery(id));
        if (t is null) return NotFound();
        Id = t.Id; Name = t.Name; Slug = t.Slug;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            await _mediator.Send(new UpdateTagCommand(Id, Name, Slug));
            TempData["AdminOk"] = "برچسب به‌روزرسانی شد.";
            return RedirectToPage("Index");
        }
        catch (ValidationException ex)
        {
            foreach (var e in ex.Errors) ModelState.AddModelError(string.Empty, e.ErrorMessage);
            return Page();
        }
    }
}
