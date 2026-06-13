using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Account.Queries.GetOrderDetail;

/// <summary>جزییات یک سفارش کاربر جاری (برای مشاهده + فاکتور PDF). IDOR guard: Id + UserId.</summary>
public record GetOrderDetailQuery(int OrderId, int UserId) : IRequest<OrderDetailDto?>;

public class OrderDetailDto
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public OrderStatus Status { get; set; }
    public long TotalAmount { get; set; }
    public long DiscountAmount { get; set; }
    public long ShippingCost { get; set; }
    public string? ShippingMethodName { get; set; }  // snapshot نام روش ارسال (F4)
    public long FinalAmount { get; set; }
    public string? TrackingCode { get; set; }

    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;

    public string? ReceiverName { get; set; }
    public string? ReceiverPhone { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? FullAddress { get; set; }

    public List<OrderDetailItemDto> Items { get; set; } = new();
}

public class OrderDetailItemDto
{
    public string ProductTitle { get; set; } = null!;
    public long UnitPrice { get; set; }
    public int Quantity { get; set; }
    public long LineTotal => UnitPrice * Quantity;
}

public class GetOrderDetailQueryHandler : IRequestHandler<GetOrderDetailQuery, OrderDetailDto?>
{
    private readonly IApplicationDbContext _db;
    public GetOrderDetailQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<OrderDetailDto?> Handle(GetOrderDetailQuery request, CancellationToken ct) =>
        await _db.Orders
            .AsNoTracking()
            .Where(o => o.Id == request.OrderId && o.UserId == request.UserId)
            .Select(o => new OrderDetailDto
            {
                Id = o.Id,
                OrderCode = o.OrderCode,
                CreatedAt = o.CreatedAt,
                PaidAt = o.PaidAt,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                DiscountAmount = o.DiscountAmount,
                ShippingCost = o.ShippingCost,
                ShippingMethodName = o.ShippingMethodName,
                FinalAmount = o.FinalAmount,
                TrackingCode = o.TrackingCode,
                CustomerName = o.User.FullName,
                CustomerPhone = o.User.PhoneNumber,
                ReceiverName = o.Address != null ? o.Address.ReceiverName : null,
                ReceiverPhone = o.Address != null ? o.Address.ReceiverPhone : null,
                Province = o.Address != null ? o.Address.Province : null,
                City = o.Address != null ? o.Address.City : null,
                PostalCode = o.Address != null ? o.Address.PostalCode : null,
                FullAddress = o.Address != null ? o.Address.FullAddress : null,
                Items = o.Items.Select(i => new OrderDetailItemDto
                {
                    ProductTitle = i.ProductTitle,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity
                }).ToList()
            })
            .FirstOrDefaultAsync(ct);
}
