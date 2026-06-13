using System.Security.Claims;
using FluentValidation;
using MahanShop.Application.Features.Admin.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Users;

/// <summary>لیست کاربران + جستجو/فیلتر + آمار + صفحه‌بندی + حذف از همین صفحه.</summary>
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    public IndexModel(IMediator mediator) => _mediator = mediator;

    public AdminUserListResult Result { get; private set; } = new();

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public bool? OnlyAdmins { get; set; }
    [BindProperty(SupportsGet = true)] public bool? OnlyInactive { get; set; }
    [BindProperty(SupportsGet = true)] public bool? OnlyWithOrders { get; set; }
    [BindProperty(SupportsGet = true)] public int Page { get; set; } = 1;

    public bool HasFilter => !string.IsNullOrWhiteSpace(Search)
        || OnlyAdmins == true || OnlyInactive == true || OnlyWithOrders == true;

    private int CurrentAdminId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task OnGetAsync() =>
        Result = await _mediator.Send(new GetUsersQuery(Search, OnlyAdmins, OnlyInactive, OnlyWithOrders, Page));

    public async Task<IActionResult> OnPostDeleteAsync(int userId)
    {
        try
        {
            await _mediator.Send(new DeleteUserCommand(userId, CurrentAdminId));
            TempData["AdminOk"] = "کاربر با موفقیت حذف شد.";
        }
        catch (ValidationException ex) { TempData["AdminError"] = ex.Message; }
        return RedirectToPage(new { Search, OnlyAdmins, OnlyInactive, OnlyWithOrders, Page });
    }
}
