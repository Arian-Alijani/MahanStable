using FluentValidation;
using MahanShop.Application.Features.Admin.Home;
using MahanShop.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Home;

/// <summary>ایجاد بنر هیرو (بالای صفحه) جدید + آپلود تصویر دسکتاپ و اختیاری موبایل.</summary>
public class HeroBannerCreateModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ImageUploadService _upload;
    public HeroBannerCreateModel(IMediator mediator, ImageUploadService upload)
    {
        _mediator = mediator; _upload = upload;
    }

    [BindProperty] public string? Title { get; set; }
    [BindProperty] public string AltText { get; set; } = string.Empty;
    [BindProperty] public string? LinkUrl { get; set; }
    [BindProperty] public int DisplayOrder { get; set; }
    [BindProperty] public bool IsActive { get; set; } = true;
    [BindProperty] public IFormFile? ImageFile { get; set; }
    [BindProperty] public IFormFile? MobileImageFile { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var img = await _upload.UploadAsync(ImageFile, "Banner");
        if (!img.Success)
        {
            ModelState.AddModelError(string.Empty, img.Error!);
            return Page();
        }

        string? mobileUrl = null;
        if (MobileImageFile is not null)
        {
            var m = await _upload.UploadAsync(MobileImageFile, "Banner");
            if (!m.Success) { ModelState.AddModelError(string.Empty, m.Error!); return Page(); }
            mobileUrl = m.WebPath;
        }

        try
        {
            await _mediator.Send(new CreateHeroBannerCommand(
                Title, img.WebPath!, mobileUrl, LinkUrl, AltText, DisplayOrder, IsActive));
            TempData["AdminOk"] = "بنر بالای صفحه ساخته شد.";
            return RedirectToPage("Index");
        }
        catch (ValidationException ex)
        {
            foreach (var e in ex.Errors) ModelState.AddModelError(string.Empty, e.ErrorMessage);
            return Page();
        }
    }
}
