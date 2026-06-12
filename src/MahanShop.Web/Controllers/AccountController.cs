using System.Security.Claims;
using FluentValidation;
using MahanShop.Application.Features.Auth.Commands.SendLoginCode;
using MahanShop.Application.Features.Auth.Commands.VerifyLoginCode;
using MahanShop.Web.Models.Account;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MahanShop.Web.Controllers;

/// <summary>احراز هویت با OTP پیامکی: ورود دو مرحله‌ای (شماره → کد) + خروج. anti-forgery گلوبال روی همه POST.</summary>
public class AccountController : Controller
{
    private readonly IMediator _mediator;

    public AccountController(IMediator mediator) => _mediator = mediator;

    // GET /Account/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToLocalOrHome(returnUrl);
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // POST /Account/Login — ارسال کد
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        SendLoginCodeResult result;
        try
        {
            result = await _mediator.Send(new SendLoginCodeCommand(vm.Phone ?? string.Empty));
        }
        catch (ValidationException ex)
        {
            AddErrors(ex);
            return View(vm);
        }

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "ارسال کد ناموفق بود.");
            return View(vm);
        }

        return RedirectToAction(nameof(Verify), new { phone = result.NormalizedPhone, returnUrl = vm.ReturnUrl });
    }

    // GET /Account/Verify
    [HttpGet]
    public IActionResult Verify(string phone, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return RedirectToAction(nameof(Login));
        return View(new VerifyViewModel { Phone = phone, ReturnUrl = returnUrl });
    }

    // POST /Account/Verify — تایید کد و ورود
    [HttpPost]
    public async Task<IActionResult> Verify(VerifyViewModel vm)
    {
        VerifyLoginCodeResult result;
        try
        {
            result = await _mediator.Send(
                new VerifyLoginCodeCommand(vm.Phone, vm.Code ?? string.Empty, vm.FullName));
        }
        catch (ValidationException ex)
        {
            AddErrors(ex);
            return View(vm);
        }

        if (!result.Success || result.User is null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "کد نامعتبر است.");
            return View(vm);
        }

        await SignInAsync(result.User);
        return RedirectToLocalOrHome(vm.ReturnUrl);
    }

    // POST /Account/Logout
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    private async Task SignInAsync(AuthenticatedUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, string.IsNullOrWhiteSpace(user.FullName) ? user.PhoneNumber : user.FullName),
            new(ClaimTypes.MobilePhone, user.PhoneNumber),
        };
        if (user.IsAdmin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });
    }

    private void AddErrors(ValidationException ex)
    {
        foreach (var e in ex.Errors)
            ModelState.AddModelError(string.Empty, e.ErrorMessage);
    }

    private IActionResult RedirectToLocalOrHome(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }
}
