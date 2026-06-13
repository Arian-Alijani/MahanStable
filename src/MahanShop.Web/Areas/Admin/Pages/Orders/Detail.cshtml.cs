using FluentValidation;
using MahanShop.Application.Features.Admin.Orders;
using MahanShop.Domain.Enums;
using MahanShop.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Orders;

/// <summary>
/// جزییات سفارش برای ادمین:
///   • تغییر وضعیت (ChangeOrderStatusCommand)
///   • ورود/به‌روزرسانی کد رهگیری بدون تغییر وضعیت (UpdateOrderTrackingCommand)
///   • دانلود فاکتور PDF
/// </summary>
public class DetailModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly InvoicePdfService _pdf;

    public DetailModel(IMediator mediator, InvoicePdfService pdf)
    {
        _mediator = mediator;
        _pdf = pdf;
    }

    public AdminOrderDetailDto Detail { get; private set; } = null!;

    // ── تغییر وضعیت
    [BindProperty] public OrderStatus NewStatus { get; set; }
    [BindProperty] public string? StatusTrackingCode { get; set; }

    // ── ورود کد رهگیری مستقل
    [BindProperty] public string TrackingCodeInput { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var d = await _mediator.Send(new GetAdminOrderDetailQuery(id));
        if (d is null) return NotFound();
        Detail = d;
        NewStatus = d.Order.Status;
        TrackingCodeInput = d.Order.TrackingCode ?? string.Empty;
        StatusTrackingCode = d.Order.TrackingCode;
        return Page();
    }

    /// <summary>تغییر وضعیت سفارش (و اختیاری: کد رهگیری همزمان)</summary>
    public async Task<IActionResult> OnPostStatusAsync(int id)
    {
        try
        {
            await _mediator.Send(new ChangeOrderStatusCommand(id, NewStatus, StatusTrackingCode));
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

    /// <summary>ثبت/به‌روزرسانی کد رهگیری بدون تغییر وضعیت</summary>
    public async Task<IActionResult> OnPostTrackingAsync(int id)
    {
        if (string.IsNullOrWhiteSpace(TrackingCodeInput))
        {
            TempData["AdminError"] = "کد رهگیری نمی‌تواند خالی باشد.";
            return RedirectToPage(new { id });
        }
        try
        {
            await _mediator.Send(new UpdateOrderTrackingCommand(id, TrackingCodeInput));
            TempData["AdminOk"] = "کد رهگیری ثبت شد.";
        }
        catch (ValidationException ex)
        {
            TempData["AdminError"] = ex.Errors.Any()
                ? string.Join(" ", ex.Errors.Select(e => e.ErrorMessage))
                : ex.Message;
        }
        return RedirectToPage(new { id });
    }

    /// <summary>دانلود فاکتور PDF</summary>
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
