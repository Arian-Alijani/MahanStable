using System.Security.Claims;
using FluentValidation;
using MahanShop.Application.Features.Admin.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Users;

/// <summary>جزییات کاربر + toggle نقش ادمین/فعال + حذف آدرس. شناسهٔ ادمین جاری از claim (نه فرم).</summary>
public class DetailModel : PageModel
{
    private readonly IMediator _mediator;
    public DetailModel(IMediator mediator) => _mediator = mediator;

    public AdminUserDetailDto Detail { get; private set; } = null!;

    private int CurrentAdminId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var d = await _mediator.Send(new GetUserDetailQuery(id));
        if (d is null) return NotFound();
        Detail = d;
        return Page();
    }

    public async Task<IActionResult> OnPostToggleAdminAsync(int id)
    {
        try
        {
            await _mediator.Send(new ToggleUserAdminCommand(id, CurrentAdminId));
            TempData["AdminOk"] = "نقش مدیریت کاربر تغییر کرد.";
        }
        catch (ValidationException ex) { TempData["AdminError"] = ex.Message; }
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(int id)
    {
        try
        {
            await _mediator.Send(new ToggleUserActiveCommand(id, CurrentAdminId));
            TempData["AdminOk"] = "وضعیت کاربر تغییر کرد.";
        }
        catch (ValidationException ex) { TempData["AdminError"] = ex.Message; }
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteAddressAsync(int id, int addressId)
    {
        try
        {
            await _mediator.Send(new DeleteUserAddressCommand(id, addressId));
            TempData["AdminOk"] = "آدرس حذف شد.";
        }
        catch (ValidationException ex) { TempData["AdminError"] = ex.Message; }
        return RedirectToPage(new { id });
    }
}
