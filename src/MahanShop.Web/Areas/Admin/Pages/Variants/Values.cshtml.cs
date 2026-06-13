using FluentValidation;
using MahanShop.Application.Features.Admin.Variants;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Variants;

/// <summary>مدیریت مقادیر pool یک ویژگی: افزودن/ویرایش/حذف.</summary>
public class ValuesModel : PageModel
{
    private readonly IMediator _mediator;
    public ValuesModel(IMediator mediator) => _mediator = mediator;

    [BindProperty] public int Id { get; set; }
    public string AttributeName { get; private set; } = "";
    public bool IsColor { get; private set; }
    public VariantAttributeKind Kind { get; private set; }
    public bool IsBrand => Kind == VariantAttributeKind.Brand;
    public List<VariantAttributeValueDto> Values { get; private set; } = new();

    [BindProperty] public string NewValue { get; set; } = "";
    [BindProperty] public string? NewColorHex { get; set; }
    [BindProperty] public string? NewLogoUrl { get; set; }
    [BindProperty] public int NewDisplayOrder { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Id = id;
        if (!await LoadAsync(id)) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        try
        {
            await _mediator.Send(new CreateVariantAttributeValueCommand(Id, NewValue, NewColorHex, NewLogoUrl, NewDisplayOrder));
            TempData["AdminOk"] = "مقدار اضافه شد.";
        }
        catch (ValidationException ex)
        {
            TempData["AdminErr"] = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "خطا.";
        }
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostUpdateAsync(int valueId, string value, string? colorHex, string? logoUrl, int displayOrder)
    {
        try
        {
            await _mediator.Send(new UpdateVariantAttributeValueCommand(valueId, value, colorHex, logoUrl, displayOrder));
            TempData["AdminOk"] = "مقدار به‌روزرسانی شد.";
        }
        catch (ValidationException ex)
        {
            TempData["AdminErr"] = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "خطا.";
        }
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int valueId)
    {
        try
        {
            await _mediator.Send(new DeleteVariantAttributeValueCommand(valueId));
            TempData["AdminOk"] = "مقدار حذف شد.";
        }
        catch (ValidationException ex)
        {
            TempData["AdminErr"] = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "خطا در حذف.";
        }
        return RedirectToPage(new { id = Id });
    }

    private async Task<bool> LoadAsync(int id)
    {
        var attr = await _mediator.Send(new GetVariantAttributeForEditQuery(id));
        if (attr is null) return false;
        AttributeName = attr.Name;
        IsColor = attr.IsColor;
        Kind = attr.Kind;
        Values = await _mediator.Send(new GetVariantAttributeValuesQuery(id));
        return true;
    }
}
