using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Orders;

/// <summary>
/// ثبت/به‌روزرسانی کد رهگیری پستی سفارش بدون تغییر وضعیت.
/// این command کاملاً جدا از ChangeOrderStatusCommand است تا ادمین بتواند
/// فقط کد رهگیری را بدون تغییر وضعیت آپدیت کند.
/// </summary>
public record UpdateOrderTrackingCommand(int OrderId, string TrackingCode) : IRequest;

public class UpdateOrderTrackingCommandValidator : AbstractValidator<UpdateOrderTrackingCommand>
{
    public UpdateOrderTrackingCommandValidator()
    {
        RuleFor(x => x.OrderId).GreaterThan(0).WithMessage("شناسهٔ سفارش نامعتبر است.");
        RuleFor(x => x.TrackingCode)
            .NotEmpty().WithMessage("کد رهگیری نمی‌تواند خالی باشد.")
            .MaximumLength(100).WithMessage("کد رهگیری حداکثر ۱۰۰ کاراکتر است.");
    }
}

public class UpdateOrderTrackingCommandHandler : IRequestHandler<UpdateOrderTrackingCommand>
{
    private readonly IApplicationDbContext _db;
    public UpdateOrderTrackingCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(UpdateOrderTrackingCommand request, CancellationToken ct)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
            ?? throw new ValidationException("سفارش یافت نشد.");

        var tracking = request.TrackingCode.Trim();
        if (tracking.Length > 100)
            throw new ValidationException("کد رهگیری حداکثر ۱۰۰ کاراکتر است.");

        order.TrackingCode = tracking;
        order.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }
}
