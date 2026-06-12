using MahanShop.Application.Features.Cart.Models;
using MediatR;

namespace MahanShop.Application.Features.Cart.Commands.PlaceOrder;

/// <summary>ثبت سفارش از سبد. UserId از claim (نه client). آدرس متعلق به همان کاربر بررسی می‌شود. خروجی = نتیجه + OrderCode.</summary>
public record PlaceOrderCommand(int UserId, int AddressId, IReadOnlyList<CartItemInput> Items) : IRequest<PlaceOrderResult>;

/// <summary>نتیجه ثبت. Success=false → Error پیام عمومی. موفق → OrderId/OrderCode برای مرحله پرداخت (P7).</summary>
public record PlaceOrderResult(bool Success, string? Error, int OrderId, string? OrderCode);
