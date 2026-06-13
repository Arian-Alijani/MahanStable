using MahanShop.Application.Features.Admin.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages;

/// <summary>داشبورد ادمین (مرحله ۴): آمار واقعی فروش/سفارش/موجودی + سفارش‌های اخیر. دسترسی از policy AdminOnly روی کل Area.</summary>
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    public IndexModel(IMediator mediator) => _mediator = mediator;

    public DashboardStatsDto Stats { get; private set; } = new();

    public async Task OnGetAsync() => Stats = await _mediator.Send(new GetDashboardStatsQuery());
}
