using FluentValidation;
using MahanShop.Application.Features.Admin.Brands;
using MahanShop.Application.Features.Admin.Categories;
using MahanShop.Application.Features.Admin.Products;
using MahanShop.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Products;

/// <summary>ویرایش محصول + مدیریت گالری عکس (افزودن/حذف/تعیین اصلی).</summary>
public class EditModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ImageUploadService _upload;
    public EditModel(IMediator mediator, ImageUploadService upload)
    {
        _mediator = mediator; _upload = upload;
    }

    [BindProperty] public int Id { get; set; }
    [BindProperty] public string Title { get; set; } = "";
    [BindProperty] public string? Slug { get; set; }
    [BindProperty] public string? ShortDescription { get; set; }
    [BindProperty] public string? Description { get; set; }
    [BindProperty] public long Price { get; set; }
    [BindProperty] public long? DiscountPrice { get; set; }
    [BindProperty] public int Stock { get; set; }
    [BindProperty] public bool HasVariants { get; set; }
    [BindProperty] public bool IsActive { get; set; }
    [BindProperty] public bool IsFeatured { get; set; }
    [BindProperty] public string? MetaTitle { get; set; }
    [BindProperty] public string? MetaDescription { get; set; }
    [BindProperty] public int BrandId { get; set; }
    [BindProperty] public int CategoryId { get; set; }

    [BindProperty] public IFormFile? ImageFile { get; set; }
    [BindProperty] public string? ImageAlt { get; set; }

    public List<ProductImageDto> Images { get; private set; } = new();
    public List<BrandListItemDto> Brands { get; private set; } = new();
    public List<CategoryOptionDto> Categories { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var p = await _mediator.Send(new GetProductForEditQuery(id));
        if (p is null) return NotFound();
        MapFrom(p);
        await LoadOptionsAsync();
        return Page();
    }

    // ذخیرهٔ اطلاعات پایه
    public async Task<IActionResult> OnPostSaveAsync()
    {
        try
        {
            await _mediator.Send(new UpdateProductCommand(
                Id, Title, Slug, ShortDescription, Description, Price, DiscountPrice, Stock,
                HasVariants, IsActive, IsFeatured, MetaTitle, MetaDescription, BrandId, CategoryId));
            TempData["AdminOk"] = "محصول به‌روزرسانی شد.";
            return RedirectToPage(new { id = Id });
        }
        catch (ValidationException ex)
        {
            foreach (var e in ex.Errors) ModelState.AddModelError(string.Empty, e.ErrorMessage);
            await ReloadAsync();
            return Page();
        }
    }

    // افزودن عکس به گالری
    public async Task<IActionResult> OnPostAddImageAsync()
    {
        var r = await _upload.UploadAsync(ImageFile, "Photo");
        if (!r.Success)
        {
            TempData["AdminErr"] = r.Error;
            return RedirectToPage(new { id = Id });
        }
        await _mediator.Send(new AddProductImageCommand(Id, r.WebPath!, ImageAlt));
        TempData["AdminOk"] = "عکس اضافه شد.";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostDeleteImageAsync(int imageId)
    {
        await _mediator.Send(new DeleteProductImageCommand(imageId));
        TempData["AdminOk"] = "عکس حذف شد.";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostSetMainImageAsync(int imageId)
    {
        await _mediator.Send(new SetMainProductImageCommand(imageId));
        TempData["AdminOk"] = "عکس اصلی تعیین شد.";
        return RedirectToPage(new { id = Id });
    }

    private void MapFrom(ProductEditDto p)
    {
        Id = p.Id; Title = p.Title; Slug = p.Slug; ShortDescription = p.ShortDescription;
        Description = p.Description; Price = p.Price; DiscountPrice = p.DiscountPrice; Stock = p.Stock;
        HasVariants = p.HasVariants; IsActive = p.IsActive; IsFeatured = p.IsFeatured;
        MetaTitle = p.MetaTitle; MetaDescription = p.MetaDescription;
        BrandId = p.BrandId; CategoryId = p.CategoryId; Images = p.Images;
    }

    private async Task ReloadAsync()
    {
        var p = await _mediator.Send(new GetProductForEditQuery(Id));
        if (p is not null) Images = p.Images;
        await LoadOptionsAsync();
    }

    private async Task LoadOptionsAsync()
    {
        Brands = await _mediator.Send(new GetBrandsQuery());
        Categories = await _mediator.Send(new GetCategoryOptionsQuery());
    }
}
