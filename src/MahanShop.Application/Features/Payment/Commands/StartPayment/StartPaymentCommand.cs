using MediatR;

namespace MahanShop.Application.Features.Payment.Commands.StartPayment;

/// <summary>شروع پرداخت سفارش. UserId از claim (نه فرم) — تایید مالکیت سفارش.</summary>
public record StartPaymentCommand(int OrderId, int UserId) : IRequest<StartPaymentResult>;

public record StartPaymentResult(bool Success, string? RedirectUrl, string? Error);
