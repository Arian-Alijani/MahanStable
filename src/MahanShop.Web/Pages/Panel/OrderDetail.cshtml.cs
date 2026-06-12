using MahanShop.Application.Features.Account.Queries.GetOrderDetail;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MahanShop.Web.Pages.Panel;

/// <summary>جزییات یک سفارش کاربر جاری. IDOR guard در query (Id+UserId).</summary>
[Authorize]
public class OrderDetailModel : PanelPageModel
{
    private readonly IMediator _mediator;
    public OrderDetailModel(IMediator mediator) => _mediator = mediator;

    public OrderDetailDto? Order { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Order = await _mediator.Send(new GetOrderDetailQuery(id, UserId));
        if (Order is null) return NotFound();
        return Page();
    }
}
