using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Orders;

/// <summary>تغییر وضعیت سفارش توسط ادمین + ثبت اختیاری کد رهگیری پستی. گذار از/به Pending/AwaitingPayment ممنوع (چرخه پرداخت دست درگاه است).</summary>
public record ChangeOrderStatusCommand(int OrderId, OrderStatus NewStatus, string? TrackingCode = null) : IRequest;

public class ChangeOrderStatusCommandHandler : IRequestHandler<ChangeOrderStatusCommand>
{
    /// <summary>وضعیت‌هایی که ادمین مجاز است ست کند (نه Pending/AwaitingPayment که متعلق به چرخه پرداخت‌اند).</summary>
    private static readonly OrderStatus[] AllowedTargets =
    {
        OrderStatus.Processing, OrderStatus.Shipped, OrderStatus.Delivered,
        OrderStatus.Canceled, OrderStatus.Refunded
    };

    private readonly IApplicationDbContext _db;
    public ChangeOrderStatusCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(ChangeOrderStatusCommand request, CancellationToken ct)
    {
        if (!AllowedTargets.Contains(request.NewStatus))
            throw new ValidationException("این وضعیت قابل انتخاب نیست.");

        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
            ?? throw new ValidationException("سفارش یافت نشد.");

        // سفارش پرداخت‌نشده را نمی‌توان وارد چرخه پردازش/ارسال کرد (لغو مجاز است).
        if (order.PaidAt is null && request.NewStatus is not OrderStatus.Canceled)
            throw new ValidationException("سفارش پرداخت‌نشده فقط قابل لغو است.");

        var tracking = request.TrackingCode?.Trim();
        if (tracking is { Length: > 100 })
            throw new ValidationException("کد رهگیری حداکثر ۱۰۰ کاراکتر است.");

        order.Status = request.NewStatus;
        if (!string.IsNullOrEmpty(tracking)) order.TrackingCode = tracking;
        order.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }
}
