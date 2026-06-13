using MahanShop.Application.Features.Admin.Orders;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Orders;

/// <summary>
/// لیست سفارش‌ها — سورت پیش‌فرض جدیدترین + سورت‌های دیگر + پنل جست‌وجو/فیلتر + کارت آمار.
/// </summary>
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    public IndexModel(IMediator mediator) => _mediator = mediator;

    public AdminOrderListResult Result { get; private set; } = new();

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public OrderStatus? Status { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? FromDate { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? ToDate { get; set; }
    [BindProperty(SupportsGet = true)] public OrderSortOption Sort { get; set; } = OrderSortOption.Newest;
    [BindProperty(SupportsGet = true)] public int Page { get; set; } = 1;

    public async Task OnGetAsync() =>
        Result = await _mediator.Send(new GetOrdersQuery(Search, Status, FromDate, ToDate, Sort, Page));

    /// <summary>آیا حداقل یک فیلتر فعال است.</summary>
    public bool HasFilter =>
        !string.IsNullOrWhiteSpace(Search) || Status is not null
        || FromDate is not null || ToDate is not null;
}
