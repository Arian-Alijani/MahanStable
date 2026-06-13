using MahanShop.Application.Features.Home.Queries.GetMenuCategories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MahanShop.Web.ViewComponents;

/// <summary>مگامنوی دسته‌بندی هدر — درخت دسته از DB (پویا).</summary>
public class CategoryMenuViewComponent : ViewComponent
{
    private readonly IMediator _mediator;

    public CategoryMenuViewComponent(IMediator mediator) => _mediator = mediator;

    /// <param name="variant">
    /// "header" → مگامنوی افقیِ دسکتاپ (پیش‌فرض)؛
    /// "drawer" → فهرست آکاردئونیِ منوی موبایل (هر دسته با کلیک باز/بسته می‌شود).
    /// </param>
    public async Task<IViewComponentResult> InvokeAsync(string variant = "header")
    {
        var categories = await _mediator.Send(new GetMenuCategoriesQuery());
        var view = string.Equals(variant, "drawer", System.StringComparison.OrdinalIgnoreCase)
            ? "Drawer"
            : "Default";
        return View(view, categories);
    }
}
