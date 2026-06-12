using FluentValidation;
using MahanShop.Application.Features.Admin.Variants;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Variants;

/// <summary>ویرایش ویژگی متغیر موجود.</summary>
public class EditModel : PageModel
{
    private readonly IMediator _mediator;
    public EditModel(IMediator mediator) => _mediator = mediator;

    [BindProperty] public int Id { get; set; }
    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public bool IsColor { get; set; }
    [BindProperty] public int DisplayOrder { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var a = await _mediator.Send(new GetVariantAttributeForEditQuery(id));
        if (a is null) return NotFound();
        Id = a.Id; Name = a.Name; IsColor = a.IsColor; DisplayOrder = a.DisplayOrder;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            await _mediator.Send(new UpdateVariantAttributeCommand(Id, Name, IsColor, DisplayOrder));
            TempData["AdminOk"] = "ویژگی به‌روزرسانی شد.";
            return RedirectToPage("Index");
        }
        catch (ValidationException ex)
        {
            foreach (var e in ex.Errors) ModelState.AddModelError(string.Empty, e.ErrorMessage);
            return Page();
        }
    }
}
