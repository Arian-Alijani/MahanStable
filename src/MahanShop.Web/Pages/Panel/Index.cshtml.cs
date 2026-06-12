using MahanShop.Application.Features.Account.Queries.GetUserOrders;
using MahanShop.Application.Features.Account.Queries.GetUserProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Pages.Panel;

/// <summary>داشبورد پنل: خلاصه پروفایل + تاریخچه سفارش‌ها. UserId از claim.</summary>
[Authorize]
public class IndexModel : PanelPageModel
{
    private readonly IMediator _mediator;
    public IndexModel(IMediator mediator) => _mediator = mediator;

    public UserProfileDto? Profile { get; private set; }
    public List<UserOrderDto> Orders { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Profile = await _mediator.Send(new GetUserProfileQuery(UserId));
        Orders = await _mediator.Send(new GetUserOrdersQuery(UserId));
    }
}
