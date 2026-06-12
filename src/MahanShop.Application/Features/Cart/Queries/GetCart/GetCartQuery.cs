using MahanShop.Application.Features.Cart.Models;
using MediatR;

namespace MahanShop.Application.Features.Cart.Queries.GetCart;

/// <summary>تبدیل اقلام خام session به سبد محاسبه‌شده. قیمت/موجودی از DB. مقدار از موجودی بیشتر → clamp.</summary>
public record GetCartQuery(IReadOnlyList<CartItemInput> Items) : IRequest<CartViewModel>;
