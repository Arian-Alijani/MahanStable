using MahanShop.Application.Common.Interfaces;
using MahanShop.Application.Features.Account.Queries.GetOrderDetail;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Orders;

/// <summary>جزییات سفارش برای ادمین (بدون قید UserId). DTO پایه را reuse می‌کند تا فاکتور PDF همان بماند.</summary>
public record GetAdminOrderDetailQuery(int OrderId) : IRequest<AdminOrderDetailDto?>;

/// <summary>جزییات سفارش + تراکنش‌های پرداخت برای پنل ادمین.</summary>
public class AdminOrderDetailDto
{
    public OrderDetailDto Order { get; set; } = null!;
    public int UserId { get; set; }
    public List<AdminOrderPaymentDto> Payments { get; set; } = new();
}

public class AdminOrderPaymentDto
{
    public long Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public string? RefId { get; set; }
    public string? Authority { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GetAdminOrderDetailQueryHandler : IRequestHandler<GetAdminOrderDetailQuery, AdminOrderDetailDto?>
{
    private readonly IApplicationDbContext _db;
    public GetAdminOrderDetailQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<AdminOrderDetailDto?> Handle(GetAdminOrderDetailQuery request, CancellationToken ct) =>
        await _db.Orders
            .AsNoTracking()
            .Where(o => o.Id == request.OrderId)
            .Select(o => new AdminOrderDetailDto
            {
                UserId = o.UserId,
                Order = new OrderDetailDto
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
                        ProductTitle = i.VariantTitle != null
                            ? i.ProductTitle + " — " + i.VariantTitle
                            : i.ProductTitle,
                        UnitPrice = i.UnitPrice,
                        Quantity = i.Quantity
                    }).ToList()
                },
                Payments = o.Payments
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new AdminOrderPaymentDto
                    {
                        Amount = p.Amount,
                        Status = p.Status,
                        RefId = p.RefId,
                        Authority = p.Authority,
                        CreatedAt = p.CreatedAt
                    }).ToList()
            })
            .FirstOrDefaultAsync(ct);
}
