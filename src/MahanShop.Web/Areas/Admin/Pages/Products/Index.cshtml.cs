using MahanShop.Application.Features.Admin.Brands;
using MahanShop.Application.Features.Admin.Categories;
using MahanShop.Application.Features.Admin.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Products;

/// <summary>
/// صفحهٔ مرکزی «محصولات»: جدول همهٔ محصولات + جست‌وجوی پیشرفته + فیلترها (وضعیت/موجودی/منتخب/برند/دسته)
/// + مرتب‌سازی + کارت‌های آمار + روشن/خاموش سریع (AJAX) + افزودن محصول.
/// </summary>
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    public IndexModel(IMediator mediator) => _mediator = mediator;

    public ProductListResult Result { get; private set; } = new();
    public List<BrandListItemDto> Brands { get; private set; } = new();
    public List<CategoryOptionDto> Categories { get; private set; } = new();

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int? CategoryId { get; set; }
    [BindProperty(SupportsGet = true)] public int? BrandId { get; set; }
    [BindProperty(SupportsGet = true)] public ProductActiveFilter Active { get; set; } = ProductActiveFilter.All;
    [BindProperty(SupportsGet = true)] public ProductStockFilter Stock { get; set; } = ProductStockFilter.All;
    [BindProperty(SupportsGet = true)] public bool? Featured { get; set; }
    [BindProperty(SupportsGet = true)] public ProductSort Sort { get; set; } = ProductSort.Newest;
    [BindProperty(SupportsGet = true)] public int Page { get; set; } = 1;

    private const int PageSize = 20;

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    /// <summary>روشن/خاموش‌کردن سریع محصول — فراخوانی AJAX، بازگشت وضعیت جدید.</summary>
    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        try
        {
            var isActive = await _mediator.Send(new ToggleProductActiveCommand(id));
            return new JsonResult(new { ok = true, isActive });
        }
        catch (FluentValidation.ValidationException ex)
        {
            return new JsonResult(new { ok = false, error = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "خطا." });
        }
    }

    private async Task LoadAsync()
    {
        Result = await _mediator.Send(new GetProductsQuery(
            Search, CategoryId, BrandId, Active, Stock, Featured, Sort, Page, PageSize));
        Brands = await _mediator.Send(new GetBrandsQuery());
        Categories = await _mediator.Send(new GetCategoryOptionsQuery());
    }
}
