using MahanShop.Application.Features.Admin.Shipping;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Attributes;

/// <summary>
/// hub «کنترل ویژگی» — شمارش واقعی هر زیربخش از DB + لینک به صفحات مدیریت.
/// F3: شمارش‌های واقعی و کارت «نوع پست» فعال (CRUD آماده در /Admin/Shipping).
/// </summary>
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    public IndexModel(IMediator mediator) => _mediator = mediator;

    public AttributeHubStatsDto Stats { get; private set; } = new();

    public async Task OnGetAsync() =>
        Stats = await _mediator.Send(new GetAttributeHubStatsQuery());
}
