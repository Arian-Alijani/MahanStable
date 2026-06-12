using MahanShop.Application.Features.Catalog.Queries.GetCatalog;
using MahanShop.Application.Features.Catalog.Queries.GetProductDetail;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MahanShop.Web.Controllers;

/// <summary>مرور محصولات: لیست + فیلتر + سورت + جستجو + صفحه‌بندی (Phase 4A). فیلتر سمت سرور با query-string.</summary>
public class CatalogController : Controller
{
    private readonly IMediator _mediator;

    public CatalogController(IMediator mediator) => _mediator = mediator;

    // GET /products
    public async Task<IActionResult> Index([FromQuery] CatalogFilter f)
    {
        var vm = await _mediator.Send(new GetCatalogQuery(f));
        return View(vm);
    }

    // GET /category/{slug}
    public async Task<IActionResult> Category(string slug, [FromQuery] CatalogFilter f)
    {
        f.CategorySlug = slug;
        var vm = await _mediator.Send(new GetCatalogQuery(f));
        return View(nameof(Index), vm);
    }

    // GET /search?q=...
    public async Task<IActionResult> Search(string? q, [FromQuery] CatalogFilter f)
    {
        f.Search = q;
        var vm = await _mediator.Send(new GetCatalogQuery(f));
        return View(nameof(Index), vm);
    }

    // GET /product/{slug} — جزییات محصول (Phase 4B)
    public async Task<IActionResult> Detail(string slug)
    {
        var dto = await _mediator.Send(new GetProductDetailQuery(slug));
        if (dto is null) return NotFound();
        return View(dto);
    }
}
