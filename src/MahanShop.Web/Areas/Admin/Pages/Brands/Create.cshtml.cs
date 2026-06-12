using FluentValidation;
using MahanShop.Application.Features.Admin.Brands;
using MahanShop.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Brands;

/// <summary>ایجاد برند جدید + آپلود اختیاری لوگو.</summary>
public class CreateModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ImageUploadService _upload;
    public CreateModel(IMediator mediator, ImageUploadService upload)
    {
        _mediator = mediator; _upload = upload;
    }

    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public string? Slug { get; set; }
    [BindProperty] public string? LogoUrl { get; set; }
    [BindProperty] public bool IsActive { get; set; } = true;
    [BindProperty] public int DisplayOrder { get; set; }
    [BindProperty] public IFormFile? LogoFile { get; set; }

    public void OnGet() { }

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
            await _mediator.Send(new CreateBrandCommand(Name, Slug, LogoUrl, IsActive, DisplayOrder));
            TempData["AdminOk"] = "برند ساخته شد.";
            return RedirectToPage("Index");
        }
        catch (ValidationException ex)
        {
            foreach (var e in ex.Errors) ModelState.AddModelError(string.Empty, e.ErrorMessage);
            return Page();
        }
    }
}
