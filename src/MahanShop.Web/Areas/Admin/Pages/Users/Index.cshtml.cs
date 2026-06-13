using MahanShop.Application.Features.Admin.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Users;

/// <summary>لیست کاربران + جستجو/فیلتر ادمین/غیرفعال + صفحه‌بندی.</summary>
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    public IndexModel(IMediator mediator) => _mediator = mediator;

    public AdminUserListResult Result { get; private set; } = new();

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public bool? OnlyAdmins { get; set; }
    [BindProperty(SupportsGet = true)] public bool? OnlyInactive { get; set; }
    [BindProperty(SupportsGet = true)] public int Page { get; set; } = 1;

    public async Task OnGetAsync() =>
        Result = await _mediator.Send(new GetUsersQuery(Search, OnlyAdmins, OnlyInactive, Page));
}
