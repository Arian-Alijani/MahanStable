using FluentValidation;
using MahanShop.Application.Features.Admin.Brands;
using MahanShop.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Brands;

/// <summary>ویرایش برند موجود + جایگزینی اختیاری لوگو.</summary>
public class EditModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ImageUploadService _upload;
    public EditModel(IMediator mediator, ImageUploadService upload)
    {
        _mediator = mediator; _upload = upload;
    }

    [BindProperty] public int Id { get; set; }
    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public string? Slug { get; set; }
    [BindProperty] public string? LogoUrl { get; set; }
    [BindProperty] public bool IsActive { get; set; }
    [BindProperty] public int DisplayOrder { get; set; }
    [BindProperty] public IFormFile? LogoFile { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var b = await _mediator.Send(new GetBrandForEditQuery(id));
        if (b is null) return NotFound();
        Id = b.Id; Name = b.Name; Slug = b.Slug; LogoUrl = b.LogoUrl;
        IsActive = b.IsActive; DisplayOrder = b.DisplayOrder;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (LogoFile is not null)
        {
            var r = await _upload.UploadAsync(LogoFile, "Photo");
            if (!r.Success) { ModelState.AddModelError(string.Empty, r.Error!); return Page(); }
            LogoUrl = r.WebPath;
        }
        try
        {
            await _mediator.Send(new UpdateBrandCommand(Id, Name, Slug, LogoUrl, IsActive, DisplayOrder));
            TempData["AdminOk"] = "برند به‌روزرسانی شد.";
            return RedirectToPage("Index");
        }
        catch (ValidationException ex)
        {
            foreach (var e in ex.Errors) ModelState.AddModelError(string.Empty, e.ErrorMessage);
            return Page();
        }
    }
}
