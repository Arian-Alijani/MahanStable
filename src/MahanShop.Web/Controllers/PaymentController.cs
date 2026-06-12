using System.Security.Claims;
using MahanShop.Application.Features.Payment.Commands.StartPayment;
using MahanShop.Application.Features.Payment.Commands.VerifyPayment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MahanShop.Web.Controllers;

/// <summary>پرداخت Zarinpal: شروع (ریدایرکت درگاه) + verify (callback). UserId از claim. anti-forgery گلوبال روی POST.</summary>
[Authorize]
public class PaymentController : Controller
{
    private readonly IMediator _mediator;

    public PaymentController(IMediator mediator) => _mediator = mediator;

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // POST /payment/start — شروع پرداخت سفارش (مالکیت سرور تایید می‌کند)
    [HttpPost]
    public async Task<IActionResult> Start(int orderId)
    {
        var result = await _mediator.Send(new StartPaymentCommand(orderId, UserId));
        if (!result.Success || string.IsNullOrEmpty(result.RedirectUrl))
        {
            TempData["PaymentError"] = result.Error ?? "شروع پرداخت ناموفق بود.";
            return RedirectToAction("Placed", "Checkout", new { id = orderId });
        }

        // ریدایرکت خارجی به درگاه Zarinpal — LocalRedirect نه
        return Redirect(result.RedirectUrl);
    }

    // GET /payment/verify — callback درگاه (Authority + Status)
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Verify(string Authority, string Status)
    {
        var result = await _mediator.Send(new VerifyPaymentCommand(Authority ?? "", Status ?? ""));

        ViewBag.Success = result.Success;
        ViewBag.OrderCode = result.OrderCode;
        ViewBag.RefId = result.RefId;
        ViewBag.Error = result.Error;
        return View();
    }
}
