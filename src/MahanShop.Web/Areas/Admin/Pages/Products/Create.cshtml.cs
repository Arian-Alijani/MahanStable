using FluentValidation;
using MahanShop.Application.Features.Admin.Brands;
using MahanShop.Application.Features.Admin.Categories;
using MahanShop.Application.Features.Admin.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Products;

/// <summary>
/// ویزارد افزودن محصول. دو مسیر:
///  • ساده  → یک محصول با قیمت/موجودیِ خودش.
///  • چندبرندی → انتخاب برندها → گوشی‌های زیرمجموعه → یک قیمت اولیه → تولید خودکار گزینه‌ها.
/// </summary>
public class CreateModel : PageModel
{
    private readonly IMediator _mediator;
    public CreateModel(IMediator mediator) => _mediator = mediator;

    // ---- مشترک ----
    [BindProperty] public string Title { get; set; } = "";
    [BindProperty] public string? Slug { get; set; }
    [BindProperty] public string? ShortDescription { get; set; }
    [BindProperty] public string? Description { get; set; }
    [BindProperty] public bool IsActive { get; set; } = true;
    [BindProperty] public bool IsFeatured { get; set; }
    [BindProperty] public string? MetaTitle { get; set; }
    [BindProperty] public string? MetaDescription { get; set; }
    [BindProperty] public int BrandId { get; set; }
    [BindProperty] public int CategoryId { get; set; }

    // ---- ساده ----
    [BindProperty] public long Price { get; set; }
    [BindProperty] public long? DiscountPrice { get; set; }
    [BindProperty] public int Stock { get; set; }

    // ---- چندبرندی ----
    [BindProperty] public List<int> ModelValueIds { get; set; } = new();
    [BindProperty] public List<int> ColorValueIds { get; set; } = new();
    [BindProperty] public long BasePrice { get; set; }
    [BindProperty] public long? BaseDiscountPrice { get; set; }
    [BindProperty] public int BaseStock { get; set; }

    public List<BrandListItemDto> Brands { get; private set; } = new();
    public List<CategoryOptionDto> Categories { get; private set; } = new();
    public ProductWizardDataDto Wizard { get; private set; } = new();

    public async Task OnGetAsync() => await LoadOptionsAsync();

    // مسیر «ساده»
    public async Task<IActionResult> OnPostSimpleAsync()
    {
        try
        {
            var id = await _mediator.Send(new CreateProductCommand(
                Title, Slug, ShortDescription, Description, Price, DiscountPrice, Stock,
                false, IsActive, IsFeatured, MetaTitle, MetaDescription, BrandId, CategoryId));
            TempData["AdminOk"] = "محصول ساده ساخته شد. اکنون عکس‌ها را اضافه کنید.";
            return RedirectToPage("Edit", new { id });
        }
        catch (ValidationException ex)
        {
            foreach (var e in ex.Errors) ModelState.AddModelError(string.Empty, e.ErrorMessage);
            await LoadOptionsAsync();
            ViewData["WizardMode"] = "simple";
            return Page();
        }
    }

    // مسیر «چندبرندی»
    public async Task<IActionResult> OnPostMultiAsync()
    {
        try
        {
            var id = await _mediator.Send(new CreateMultiBrandProductCommand(
                Title, Slug, ShortDescription, Description, IsActive, IsFeatured,
                MetaTitle, MetaDescription, BrandId, CategoryId,
                ModelValueIds, ColorValueIds, BasePrice, BaseDiscountPrice, BaseStock));
            TempData["AdminOk"] = "محصول چندبرندی و همهٔ گزینه‌هایش ساخته شد. اکنون قیمت/موجودی هر گوشی را در «مدیریت موجودی محصول» تنظیم کنید.";
            return RedirectToPage("Inventory", new { id });
        }
        catch (ValidationException ex)
        {
            foreach (var e in ex.Errors) ModelState.AddModelError(string.Empty, e.ErrorMessage);
            await LoadOptionsAsync();
            ViewData["WizardMode"] = "multi";
            return Page();
        }
    }

    private async Task LoadOptionsAsync()
    {
        Brands = await _mediator.Send(new GetBrandsQuery());
        Categories = await _mediator.Send(new GetCategoryOptionsQuery());
        Wizard = await _mediator.Send(new GetProductWizardDataQuery());
    }
}
