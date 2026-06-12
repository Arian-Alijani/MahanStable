using MahanShop.Application.Features.Account.Queries.GetOrderDetail;
using MahanShop.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MahanShop.Web.Pages.Panel;

/// <summary>دانلود فاکتور PDF سفارش کاربر جاری. IDOR guard در query (Id+UserId). خروجی فایل، بدون View.</summary>
[Authorize]
public class InvoiceModel : PanelPageModel
{
    private readonly IMediator _mediator;
    private readonly InvoicePdfService _pdf;

    public InvoiceModel(IMediator mediator, InvoicePdfService pdf)
    {
        _mediator = mediator;
        _pdf = pdf;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var order = await _mediator.Send(new GetOrderDetailQuery(id, UserId));
        if (order is null) return NotFound();

        // فقط سفارش‌های پرداخت‌شده فاکتور دارند
        if (order.PaidAt is null && order.Status == Domain.Enums.OrderStatus.Pending)
            return BadRequest("این سفارش هنوز پرداخت نشده است.");

        var bytes = _pdf.Build(order);
        return File(bytes, "application/pdf", $"invoice-{order.OrderCode}.pdf");
    }
}
