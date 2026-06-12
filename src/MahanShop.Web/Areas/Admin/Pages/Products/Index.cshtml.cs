using MahanShop.Application.Features.Admin.Brands;
using MahanShop.Application.Features.Admin.Categories;
using MahanShop.Application.Features.Admin.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Products;

/// <summary>لیست محصولات + جستجو/فیلتر دسته‌برند + صفحه‌بندی + toggle.</summary>
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
    [BindProperty(SupportsGet = true)] public int Page { get; set; } = 1;

    public async Task OnGetAsync()
    {
        Result = await _mediator.Send(new GetProductsQuery(Search, CategoryId, BrandId, Page));
        Brands = await _mediator.Send(new GetBrandsQuery());
        Categories = await _mediator.Send(new GetCategoryOptionsQuery());
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        await _mediator.Send(new ToggleProductActiveCommand(id));
        TempData["AdminOk"] = "وضعیت محصول تغییر کرد.";
        return RedirectToPage(new { Search, CategoryId, BrandId, Page });
    }
}
