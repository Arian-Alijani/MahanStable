using MahanShop.Application.Features.Admin.Orders;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Orders;

/// <summary>لیست سفارش‌ها + جستجو/فیلتر وضعیت/بازه تاریخ + صفحه‌بندی.</summary>
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    public IndexModel(IMediator mediator) => _mediator = mediator;

    public AdminOrderListResult Result { get; private set; } = new();

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public OrderStatus? Status { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? FromDate { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? ToDate { get; set; }
    [BindProperty(SupportsGet = true)] public int Page { get; set; } = 1;

    public async Task OnGetAsync() =>
        Result = await _mediator.Send(new GetOrdersQuery(Search, Status, FromDate, ToDate, Page));
}
