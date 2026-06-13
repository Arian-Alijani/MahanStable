using FluentValidation;
using MahanShop.Application.Features.Admin.Variants;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Variants;

/// <summary>ایجاد ویژگی متغیر جدید.</summary>
public class CreateModel : PageModel
{
    private readonly IMediator _mediator;
    public CreateModel(IMediator mediator) => _mediator = mediator;

    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public bool IsColor { get; set; }
    [BindProperty] public VariantAttributeKind Kind { get; set; } = VariantAttributeKind.Other;
    [BindProperty] public int DisplayOrder { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            await _mediator.Send(new CreateVariantAttributeCommand(Name, IsColor, Kind, DisplayOrder));
            TempData["AdminOk"] = "ویژگی ساخته شد.";
            return RedirectToPage("Index");
        }
        catch (ValidationException ex)
        {
            foreach (var e in ex.Errors) ModelState.AddModelError(string.Empty, e.ErrorMessage);
            return Page();
        }
    }
}
