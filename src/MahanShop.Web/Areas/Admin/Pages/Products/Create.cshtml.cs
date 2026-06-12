using FluentValidation;
using MahanShop.Application.Features.Admin.Brands;
using MahanShop.Application.Features.Admin.Categories;
using MahanShop.Application.Features.Admin.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Products;

/// <summary>ایجاد محصول جدید (اطلاعات پایه). گالری عکس بعد از ساخت در صفحهٔ ویرایش.</summary>
public class CreateModel : PageModel
{
    private readonly IMediator _mediator;
    public CreateModel(IMediator mediator) => _mediator = mediator;

    [BindProperty] public string Title { get; set; } = "";
    [BindProperty] public string? Slug { get; set; }
    [BindProperty] public string? ShortDescription { get; set; }
    [BindProperty] public string? Description { get; set; }
    [BindProperty] public long Price { get; set; }
    [BindProperty] public long? DiscountPrice { get; set; }
    [BindProperty] public int Stock { get; set; }
    [BindProperty] public bool HasVariants { get; set; }
    [BindProperty] public bool IsActive { get; set; } = true;
    [BindProperty] public bool IsFeatured { get; set; }
    [BindProperty] public string? MetaTitle { get; set; }
    [BindProperty] public string? MetaDescription { get; set; }
    [BindProperty] public int BrandId { get; set; }
    [BindProperty] public int CategoryId { get; set; }

    public List<BrandListItemDto> Brands { get; private set; } = new();
    public List<CategoryOptionDto> Categories { get; private set; } = new();

    public async Task OnGetAsync() => await LoadOptionsAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var id = await _mediator.Send(new CreateProductCommand(
                Title, Slug, ShortDescription, Description, Price, DiscountPrice, Stock,
                HasVariants, IsActive, IsFeatured, MetaTitle, MetaDescription, BrandId, CategoryId));
            TempData["AdminOk"] = "محصول ساخته شد. اکنون عکس‌ها را اضافه کنید.";
            return RedirectToPage("Edit", new { id });
        }
        catch (ValidationException ex)
        {
            foreach (var e in ex.Errors) ModelState.AddModelError(string.Empty, e.ErrorMessage);
            await LoadOptionsAsync();
            return Page();
        }
    }

    private async Task LoadOptionsAsync()
    {
        Brands = await _mediator.Send(new GetBrandsQuery());
        Categories = await _mediator.Send(new GetCategoryOptionsQuery());
    }
}
