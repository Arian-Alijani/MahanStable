using FluentValidation;
using MahanShop.Application.Features.Admin.Categories;
using MahanShop.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Categories;

/// <summary>ویرایش دسته. والد نمی‌تواند خود دسته/زیرشاخه‌اش باشد.</summary>
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
    [BindProperty] public string? ImageUrl { get; set; }
    [BindProperty] public int? ParentId { get; set; }
    [BindProperty] public int DisplayOrder { get; set; }
    [BindProperty] public bool IsActive { get; set; }
    [BindProperty] public bool ShowInMenu { get; set; }
    [BindProperty] public bool ShowOnHome { get; set; }
    [BindProperty] public IFormFile? ImageFile { get; set; }

    public List<CategoryOptionDto> ParentOptions { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var c = await _mediator.Send(new GetCategoryForEditQuery(id));
        if (c is null) return NotFound();
        Id = c.Id; Name = c.Name; Slug = c.Slug; ImageUrl = c.ImageUrl; ParentId = c.ParentId;
        DisplayOrder = c.DisplayOrder; IsActive = c.IsActive; ShowInMenu = c.ShowInMenu; ShowOnHome = c.ShowOnHome;
        await LoadOptionsAsync();
        return Page();
    }

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
            await _mediator.Send(new UpdateCategoryCommand(
                Id, Name, Slug, ImageUrl, ParentId, DisplayOrder, IsActive, ShowInMenu, ShowOnHome));
            TempData["AdminOk"] = "دسته به‌روزرسانی شد.";
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
        ParentOptions = await _mediator.Send(new GetCategoryOptionsQuery(Id));
}
