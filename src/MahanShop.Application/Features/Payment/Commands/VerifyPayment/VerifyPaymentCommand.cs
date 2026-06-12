using MediatR;

namespace MahanShop.Application.Features.Payment.Commands.VerifyPayment;

/// <summary>تایید بازگشت از درگاه. Authority/Status از query درگاه. تایید مبلغ سمت سرور.</summary>
public record VerifyPaymentCommand(string Authority, string Status) : IRequest<VerifyPaymentResult>;

public record VerifyPaymentResult(bool Success, string? OrderCode, string? RefId, string? Error);
