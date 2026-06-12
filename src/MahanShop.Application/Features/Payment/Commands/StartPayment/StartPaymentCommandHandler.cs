using MahanShop.Application.Common.Interfaces;
using MahanShop.Application.Common.Settings;
using MahanShop.Domain.Entities;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MahanShop.Application.Features.Payment.Commands.StartPayment;

/// <summary>تایید مالکیت+وضعیت سفارش، درخواست پرداخت از درگاه، ساخت Payment(Pending)، سفارش → AwaitingPayment، بازگشت URL درگاه.</summary>
public class StartPaymentCommandHandler : IRequestHandler<StartPaymentCommand, StartPaymentResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IPaymentGateway _gateway;
    private readonly ZarinpalSettings _settings;

    public StartPaymentCommandHandler(IApplicationDbContext db, IPaymentGateway gateway, IOptions<ZarinpalSettings> settings)
    {
        _db = db;
        _gateway = gateway;
        _settings = settings.Value;
    }

    public async Task<StartPaymentResult> Handle(StartPaymentCommand request, CancellationToken ct)
    {
        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == request.UserId, ct);

        if (order is null)
            return new StartPaymentResult(false, null, "سفارش یافت نشد.");

        if (order.Status is OrderStatus.Paid)
            return new StartPaymentResult(false, null, "این سفارش قبلاً پرداخت شده است.");

        if (order.Status is not (OrderStatus.Pending or OrderStatus.AwaitingPayment))
            return new StartPaymentResult(false, null, "این سفارش قابل پرداخت نیست.");

        if (order.FinalAmount <= 0)
            return new StartPaymentResult(false, null, "مبلغ سفارش نامعتبر است.");

        var result = await _gateway.RequestAsync(
            order.FinalAmount,
            _settings.CallbackUrl,
            $"پرداخت سفارش {order.OrderCode}",
            ct);

        if (!result.Success || string.IsNullOrEmpty(result.GatewayUrl))
            return new StartPaymentResult(false, null, result.Error ?? "درخواست پرداخت ناموفق بود.");

        _db.Payments.Add(new Domain.Entities.Payment
        {
            OrderId = order.Id,
            Amount = order.FinalAmount,
            Authority = result.Authority,
            Status = PaymentStatus.Pending
        });
        order.Status = OrderStatus.AwaitingPayment;
        await _db.SaveChangesAsync(ct);

        return new StartPaymentResult(true, result.GatewayUrl, null);
    }
}
