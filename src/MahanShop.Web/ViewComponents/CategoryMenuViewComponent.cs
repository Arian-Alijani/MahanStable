using MahanShop.Application.Features.Home.Queries.GetMenuCategories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MahanShop.Web.ViewComponents;

/// <summary>مگامنوی دسته‌بندی هدر — درخت دسته از DB (پویا).</summary>
public class CategoryMenuViewComponent : ViewComponent
{
    private readonly IMediator _mediator;

    public CategoryMenuViewComponent(IMediator mediator) => _mediator = mediator;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var categories = await _mediator.Send(new GetMenuCategoriesQuery());
        return View(categories);
    }
}
