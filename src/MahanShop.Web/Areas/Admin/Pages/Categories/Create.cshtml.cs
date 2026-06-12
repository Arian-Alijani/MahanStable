using FluentValidation;
using MahanShop.Application.Features.Admin.Categories;
using MahanShop.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Categories;

/// <summary>ایجاد دستهٔ جدید + آپلود اختیاری تصویر.</summary>
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
    [BindProperty] public string? ImageUrl { get; set; }
    [BindProperty] public int? ParentId { get; set; }
    [BindProperty] public int DisplayOrder { get; set; }
    [BindProperty] public bool IsActive { get; set; } = true;
    [BindProperty] public bool ShowInMenu { get; set; } = true;
    [BindProperty] public bool ShowOnHome { get; set; }
    [BindProperty] public IFormFile? ImageFile { get; set; }

    public List<CategoryOptionDto> ParentOptions { get; private set; } = new();

    public async Task OnGetAsync() => await LoadOptionsAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        if (ImageFile is not null)
        {
            var r = await _upload.UploadAsync(ImageFile, "Photo");
            if (!r.Success) { ModelState.AddModelError(string.Empty, r.Error!); await LoadOptionsAsync(); return Page(); }
            ImageUrl = r.WebPath;
        }
        try
        {
            await _mediator.Send(new CreateCategoryCommand(
                Name, Slug, ImageUrl, ParentId, DisplayOrder, IsActive, ShowInMenu, ShowOnHome));
            TempData["AdminOk"] = "دسته ساخته شد.";
            return RedirectToPage("Index");
        }
        catch (ValidationException ex)
        {
            foreach (var e in ex.Errors) ModelState.AddModelError(string.Empty, e.ErrorMessage);
            await LoadOptionsAsync();
            return Page();
        }
    }

    private async Task LoadOptionsAsync() =>
        ParentOptions = await _mediator.Send(new GetCategoryOptionsQuery());
}
