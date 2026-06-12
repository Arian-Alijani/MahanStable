using FluentValidation;
using MahanShop.Application.Features.Account.Commands.ChangePhone;
using MahanShop.Application.Features.Account.Commands.UpdateProfile;
using MahanShop.Application.Features.Account.Queries.GetUserAddresses;
using MahanShop.Application.Features.Account.Queries.GetUserProfile;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MahanShop.Web.Pages.Panel;

/// <summary>مشخصات حساب: نمایش/ویرایش نام و ایمیل + تغییر شماره با OTP دو مرحله‌ای + آدرس‌ها.</summary>
[Authorize]
public class ProfileModel : PanelPageModel
{
    private readonly IMediator _mediator;
    public ProfileModel(IMediator mediator) => _mediator = mediator;

    public UserProfileDto? Profile { get; private set; }
    public List<AddressDto> Addresses { get; private set; } = new();

    [BindProperty] public string? FullName { get; set; }
    [BindProperty] public string? Email { get; set; }
    [BindProperty] public string? NewPhone { get; set; }
    [BindProperty] public string? OtpCode { get; set; }

    // مرحله تایید کد تغییر شماره
    public bool ShowOtpStep { get; private set; }
    public string? PendingPhone { get; private set; }

    public async Task OnGetAsync() => await LoadAsync();

    // ذخیره نام/ایمیل
    public async Task<IActionResult> OnPostUpdateAsync()
    {
        try
        {
            await _mediator.Send(new UpdateProfileCommand(UserId, FullName ?? "", Email));
            TempData["PanelOk"] = "اطلاعات حساب به‌روزرسانی شد.";
            return RedirectToPage();
        }
        catch (ValidationException ex)
        {
            foreach (var e in ex.Errors) ModelState.AddModelError(string.Empty, e.ErrorMessage);
            await LoadAsync();
            return Page();
        }
    }

    // مرحله ۱ تغییر شماره — ارسال کد
    public async Task<IActionResult> OnPostSendPhoneCodeAsync()
    {
        SendChangePhoneCodeResult result;
        try
        {
            result = await _mediator.Send(new SendChangePhoneCodeCommand(UserId, NewPhone ?? ""));
        }
        catch (ValidationException ex)
        {
            foreach (var e in ex.Errors) ModelState.AddModelError(string.Empty, e.ErrorMessage);
            await LoadAsync();
            return Page();
        }

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "ارسال کد ناموفق بود.");
            await LoadAsync();
            return Page();
        }

        await LoadAsync();
        ShowOtpStep = true;
        PendingPhone = result.NormalizedPhone;
        return Page();
    }

    // مرحله ۲ تغییر شماره — تایید کد
    public async Task<IActionResult> OnPostVerifyPhoneAsync()
    {
        VerifyChangePhoneResult result;
        try
        {
            result = await _mediator.Send(new VerifyChangePhoneCommand(UserId, NewPhone ?? "", OtpCode ?? ""));
        }
        catch (ValidationException ex)
        {
            foreach (var e in ex.Errors) ModelState.AddModelError(string.Empty, e.ErrorMessage);
            await LoadAsync();
            ShowOtpStep = true;
            PendingPhone = NewPhone;
            return Page();
        }

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "تایید کد ناموفق بود.");
            await LoadAsync();
            ShowOtpStep = true;
            PendingPhone = NewPhone;
            return Page();
        }

        // شماره تغییر کرد → claim فعلی (MobilePhone) قدیمی است. خروج امن، ورود مجدد با شماره جدید.
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["LoginNotice"] = "شماره موبایل با موفقیت تغییر کرد. لطفاً با شماره جدید وارد شوید.";
        return RedirectToAction("Login", "Account");
    }

    private async Task LoadAsync()
    {
        Profile = await _mediator.Send(new GetUserProfileQuery(UserId));
        Addresses = await _mediator.Send(new GetUserAddressesQuery(UserId));
        FullName ??= Profile?.FullName;
        Email ??= Profile?.Email;
    }
}
