using FluentValidation;
using MahanShop.Application.Features.Admin.Home;
using MahanShop.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Home;

/// <summary>ویرایش بنر هیرو (بالای صفحه). تصویر فقط در صورت آپلود جدید جایگزین می‌شود.</summary>
public class HeroBannerEditModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ImageUploadService _upload;
    public HeroBannerEditModel(IMediator mediator, ImageUploadService upload)
    {
        _mediator = mediator; _upload = upload;
    }

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public string? Title { get; set; }
    [BindProperty] public string AltText { get; set; } = string.Empty;
    [BindProperty] public string? LinkUrl { get; set; }
    [BindProperty] public int DisplayOrder { get; set; }
    [BindProperty] public bool IsActive { get; set; } = true;
    [BindProperty] public string? ImageUrl { get; set; }
    [BindProperty] public string? MobileImageUrl { get; set; }
    [BindProperty] public IFormFile? ImageFile { get; set; }
    [BindProperty] public IFormFile? MobileImageFile { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var dto = await _mediator.Send(new GetHeroBannerForEditQuery(Id));
        if (dto is null) return NotFound();

        Title = dto.Title;
        AltText = dto.AltText;
        LinkUrl = dto.LinkUrl;
        DisplayOrder = dto.DisplayOrder;
        IsActive = dto.IsActive;
        ImageUrl = dto.ImageUrl;
        MobileImageUrl = dto.MobileImageUrl;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ImageFile is not null)
        {
            var img = await _upload.UploadAsync(ImageFile, "Banner");
            if (!img.Success) { ModelState.AddModelError(string.Empty, img.Error!); return Page(); }
            ImageUrl = img.WebPath;
        }
        if (MobileImageFile is not null)
        {
            var m = await _upload.UploadAsync(MobileImageFile, "Banner");
            if (!m.Success) { ModelState.AddModelError(string.Empty, m.Error!); return Page(); }
            MobileImageUrl = m.WebPath;
        }

        try
        {
            await _mediator.Send(new UpdateHeroBannerCommand(
                Id, Title, ImageUrl ?? "", MobileImageUrl, LinkUrl, AltText, DisplayOrder, IsActive));
            TempData["AdminOk"] = "بنر بالای صفحه به‌روزرسانی شد.";
            return RedirectToPage("Index");
        }
        catch (ValidationException ex)
        {
            foreach (var e in ex.Errors) ModelState.AddModelError(string.Empty, e.ErrorMessage);
            return Page();
        }
    }
}
