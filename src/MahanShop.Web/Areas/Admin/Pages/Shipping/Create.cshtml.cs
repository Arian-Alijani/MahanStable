using FluentValidation;
using MahanShop.Application.Features.Admin.Shipping;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Shipping;

/// <summary>ایجاد روش ارسال جدید.</summary>
public class CreateModel : PageModel
{
    private readonly IMediator _mediator;
    public CreateModel(IMediator mediator) => _mediator = mediator;

    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public long Cost { get; set; }
    [BindProperty] public bool IsActive { get; set; } = true;
    [BindProperty] public int DisplayOrder { get; set; }
    [BindProperty] public string? Description { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            await _mediator.Send(new CreateShippingMethodCommand(Name, Cost, IsActive, DisplayOrder, Description));
            TempData["AdminOk"] = "روش ارسال ساخته شد.";
            return RedirectToPage("Index");
        }
        catch (ValidationException ex)
        {
            foreach (var e in ex.Errors)
                ModelState.AddModelError(string.Empty, e.ErrorMessage);
            return Page();
        }
    }
}
