using System.Security.Claims;
using FluentValidation;
using MahanShop.Application.Features.Account.Commands.AddAddress;
using MahanShop.Application.Features.Account.Queries.GetUserAddresses;
using MahanShop.Application.Features.Cart.Commands.PlaceOrder;
using MahanShop.Application.Features.Cart.Queries.GetCart;
using MahanShop.Web.Models.Checkout;
using MahanShop.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MahanShop.Web.Controllers;

/// <summary>تسویه: انتخاب/افزودن آدرس + ثبت سفارش (Pending). فقط کاربر لاگین. UserId از claim — هرگز از فرم. anti-forgery گلوبال.</summary>
[Authorize]
public class CheckoutController : Controller
{
    private readonly IMediator _mediator;
    private readonly CartStore _cart;

    public CheckoutController(IMediator mediator, CartStore cart)
    {
        _mediator = mediator;
        _cart = cart;
    }

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET /checkout
    public async Task<IActionResult> Index()
    {
        var cart = await _mediator.Send(new GetCartQuery(_cart.GetItems()));
        if (cart.Lines.Count == 0)
            return RedirectToAction("Index", "Cart");

        var vm = await BuildVm(cart);
        return View(vm);
    }

    // POST /checkout/add-address
    [HttpPost]
    public async Task<IActionResult> AddAddress(CheckoutViewModel form)
    {
        var n = form.NewAddress;
        try
        {
            var id = await _mediator.Send(new AddAddressCommand(
                UserId, n.Province ?? "", n.City ?? "", n.PostalCode ?? "",
                n.FullAddress ?? "", n.ReceiverName ?? "", n.ReceiverPhone ?? ""));
            return RedirectToAction(nameof(Index), new { addressId = id });
        }
        catch (ValidationException ex)
        {
            var cart = await _mediator.Send(new GetCartQuery(_cart.GetItems()));
            if (cart.Lines.Count == 0) return RedirectToAction("Index", "Cart");
            var vm = await BuildVm(cart);
            vm.NewAddress = n;
            foreach (var e in ex.Errors) ModelState.AddModelError(string.Empty, e.ErrorMessage);
            return View(nameof(Index), vm);
        }
    }

    // POST /checkout/place — ثبت سفارش
    [HttpPost]
    public async Task<IActionResult> Place(int addressId)
    {
        PlaceOrderResult result;
        try
        {
            result = await _mediator.Send(new PlaceOrderCommand(UserId, addressId, _cart.GetItems()));
        }
        catch (ValidationException ex)
        {
            var cart = await _mediator.Send(new GetCartQuery(_cart.GetItems()));
            if (cart.Lines.Count == 0) return RedirectToAction("Index", "Cart");
            var vm = await BuildVm(cart);
            foreach (var e in ex.Errors) ModelState.AddModelError(string.Empty, e.ErrorMessage);
            return View(nameof(Index), vm);
        }

        if (!result.Success)
        {
            TempData["CheckoutError"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        // سبد خالی شود — سفارش Pending ثبت شد (پرداخت در مرحله بعد)
        _cart.Clear();
        return RedirectToAction(nameof(Placed), new { code = result.OrderCode, id = result.OrderId });
    }

    // GET /checkout/placed?code=...&id=...
    public IActionResult Placed(string code, int id)
    {
        if (string.IsNullOrEmpty(code)) return RedirectToAction("Index", "Home");
        ViewBag.OrderCode = code;
        ViewBag.OrderId = id;
        return View();
    }

    private async Task<CheckoutViewModel> BuildVm(CartViewModel cart)
    {
        var addresses = await _mediator.Send(new GetUserAddressesQuery(UserId));
        return new CheckoutViewModel
        {
            Cart = cart,
            Addresses = addresses,
            SelectedAddressId = addresses.FirstOrDefault()?.Id
        };
    }
}
