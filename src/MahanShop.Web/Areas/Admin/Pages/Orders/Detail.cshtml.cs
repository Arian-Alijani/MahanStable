using FluentValidation;
using MahanShop.Application.Features.Admin.Orders;
using MahanShop.Domain.Enums;
using MahanShop.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Orders;

/// <summary>جزییات سفارش برای ادمین + تغییر وضعیت/کد رهگیری + دانلود فاکتور PDF.</summary>
public class DetailModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly InvoicePdfService _pdf;
    public DetailModel(IMediator mediator, InvoicePdfService pdf)
    {
        _mediator = mediator; _pdf = pdf;
    }

    public AdminOrderDetailDto Detail { get; private set; } = null!;

    [BindProperty] public OrderStatus NewStatus { get; set; }
    [BindProperty] public string? TrackingCode { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var d = await _mediator.Send(new GetAdminOrderDetailQuery(id));
        if (d is null) return NotFound();
        Detail = d;
        NewStatus = d.Order.Status;
        TrackingCode = d.Order.TrackingCode;
        return Page();
    }

    public async Task<IActionResult> OnPostStatusAsync(int id)
    {
        try
        {
            await _mediator.Send(new ChangeOrderStatusCommand(id, NewStatus, TrackingCode));
            TempData["AdminOk"] = "وضعیت سفارش به‌روزرسانی شد.";
        }
        catch (ValidationException ex)
        {
            TempData["AdminError"] = ex.Errors.Any()
                ? string.Join(" ", ex.Errors.Select(e => e.ErrorMessage))
                : ex.Message;
        }
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnGetInvoiceAsync(int id)
    {
        var d = await _mediator.Send(new GetAdminOrderDetailQuery(id));
        if (d is null) return NotFound();
        if (d.Order.PaidAt is null && d.Order.Status == OrderStatus.Pending)
            return BadRequest("این سفارش هنوز پرداخت نشده است.");

        var bytes = _pdf.Build(d.Order);
        return File(bytes, "application/pdf", $"invoice-{d.Order.OrderCode}.pdf");
    }
}
