using FluentValidation;
using MahanShop.Application.Features.Admin.Tags;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Tags;

/// <summary>ایجاد برچسب جدید.</summary>
public class CreateModel : PageModel
{
    private readonly IMediator _mediator;
    public CreateModel(IMediator mediator) => _mediator = mediator;

    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public string? Slug { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            await _mediator.Send(new CreateTagCommand(Name, Slug));
            TempData["AdminOk"] = "برچسب ساخته شد.";
            return RedirectToPage("Index");
        }
        catch (ValidationException ex)
        {
            foreach (var e in ex.Errors) ModelState.AddModelError(string.Empty, e.ErrorMessage);
            return Page();
        }
    }
}
