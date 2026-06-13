using FluentValidation;
using MahanShop.Application.Features.Admin.Shipping;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Shipping;

/// <summary>ویرایش روش ارسال موجود.</summary>
public class EditModel : PageModel
{
    private readonly IMediator _mediator;
    public EditModel(IMediator mediator) => _mediator = mediator;

    [BindProperty] public int Id { get; set; }
    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public long Cost { get; set; }
    [BindProperty] public bool IsActive { get; set; }
    [BindProperty] public int DisplayOrder { get; set; }
    [BindProperty] public string? Description { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var m = await _mediator.Send(new GetShippingMethodForEditQuery(id));
        if (m is null) return NotFound();
        Id = m.Id;
        Name = m.Name;
        Cost = m.Cost;
        IsActive = m.IsActive;
        DisplayOrder = m.DisplayOrder;
        Description = m.Description;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            await _mediator.Send(new UpdateShippingMethodCommand(Id, Name, Cost, IsActive, DisplayOrder, Description));
            TempData["AdminOk"] = "روش ارسال به‌روزرسانی شد.";
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
