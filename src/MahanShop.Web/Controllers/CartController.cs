using MahanShop.Application.Features.Cart.Queries.GetCart;
using MahanShop.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MahanShop.Web.Controllers;

/// <summary>سبد خرید session-based: نمایش + افزودن/ویرایش/حذف. anti-forgery گلوبال روی POSTها. قیمت همیشه از DB (GetCartQuery).</summary>
public class CartController : Controller
{
    private readonly IMediator _mediator;
    private readonly CartStore _cart;

    public CartController(IMediator mediator, CartStore cart)
    {
        _mediator = mediator;
        _cart = cart;
    }

    // GET /cart
    public async Task<IActionResult> Index()
    {
        var vm = await _mediator.Send(new GetCartQuery(_cart.GetItems()));
        return View(vm);
    }

    // POST /cart/add — ajax؛ خروجی شمارش جدید
    [HttpPost]
    public async Task<IActionResult> Add(int productId, int? variantId, int quantity = 1)
    {
        _cart.Add(productId, variantId, quantity);
        var vm = await _mediator.Send(new GetCartQuery(_cart.GetItems()));
        return Json(new { count = vm.TotalQuantity, payable = vm.PayableAmount });
    }

    // POST /cart/update — تنظیم مقدار دقیق
    [HttpPost]
    public async Task<IActionResult> Update(int productId, int? variantId, int quantity)
    {
        _cart.SetQuantity(productId, variantId, quantity);
        var vm = await _mediator.Send(new GetCartQuery(_cart.GetItems()));
        if (IsAjax())
            return Json(new { count = vm.TotalQuantity, payable = vm.PayableAmount });
        return RedirectToAction(nameof(Index));
    }

    // POST /cart/remove
    [HttpPost]
    public async Task<IActionResult> Remove(int productId, int? variantId)
    {
        _cart.Remove(productId, variantId);
        if (IsAjax())
        {
            var vm = await _mediator.Send(new GetCartQuery(_cart.GetItems()));
            return Json(new { count = vm.TotalQuantity, payable = vm.PayableAmount });
        }
        return RedirectToAction(nameof(Index));
    }

    private bool IsAjax() =>
        Request.Headers["X-Requested-With"] == "XMLHttpRequest";
}
