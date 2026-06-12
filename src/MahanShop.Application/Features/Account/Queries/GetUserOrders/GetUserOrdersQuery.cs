using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Account.Queries.GetUserOrders;

/// <summary>تاریخچه سفارش‌های کاربر جاری برای پنل. فیلتر اختیاری بر اساس وضعیت.</summary>
public record GetUserOrdersQuery(int UserId) : IRequest<List<UserOrderDto>>;

public class UserOrderDto
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public long FinalAmount { get; set; }
    public string? TrackingCode { get; set; }
    public int ItemCount { get; set; }
    public List<UserOrderItemDto> Items { get; set; } = new();
}

public class UserOrderItemDto
{
    public string ProductTitle { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public int Quantity { get; set; }
}

public class GetUserOrdersQueryHandler : IRequestHandler<GetUserOrdersQuery, List<UserOrderDto>>
{
    private readonly IApplicationDbContext _db;
    public GetUserOrdersQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<UserOrderDto>> Handle(GetUserOrdersQuery request, CancellationToken ct)
    {
        // فقط سفارش‌های پرداخت‌شده/فعال به کاربر نشان داده شود (Pending سبد رهاشده مخفی).
        var statuses = new[]
        {
            OrderStatus.Paid, OrderStatus.Processing, OrderStatus.Shipped,
            OrderStatus.Delivered, OrderStatus.Canceled, OrderStatus.Refunded
        };

        return await _db.Orders
            .AsNoTracking()
            .Where(o => o.UserId == request.UserId && statuses.Contains(o.Status))
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new UserOrderDto
            {
                Id = o.Id,
                OrderCode = o.OrderCode,
                CreatedAt = o.CreatedAt,
                Status = o.Status,
                FinalAmount = o.FinalAmount,
                TrackingCode = o.TrackingCode,
                ItemCount = o.Items.Count,
                Items = o.Items.Select(i => new UserOrderItemDto
                {
                    ProductTitle = i.ProductTitle,
                    Quantity = i.Quantity,
                    ImageUrl = i.Product.Images
                        .OrderByDescending(im => im.IsMain).ThenBy(im => im.DisplayOrder)
                        .Select(im => im.Url).FirstOrDefault()
                }).ToList()
            })
            .ToListAsync(ct);
    }
}
