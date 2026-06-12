using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Orders;

/// <summary>لیست سفارش‌ها برای ادمین: جستجو (کد سفارش/نام/موبایل) + فیلتر وضعیت + بازه تاریخ + صفحه‌بندی.</summary>
public record GetOrdersQuery(
    string? Search = null, OrderStatus? Status = null,
    DateTime? FromDate = null, DateTime? ToDate = null,
    int Page = 1, int PageSize = 20) : IRequest<AdminOrderListResult>;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, AdminOrderListResult>
{
    private readonly IApplicationDbContext _db;
    public GetOrdersQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<AdminOrderListResult> Handle(GetOrdersQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var size = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var q = _db.Orders.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            q = q.Where(o => o.OrderCode.Contains(term)
                          || o.User.FullName.Contains(term)
                          || o.User.PhoneNumber.Contains(term)
                          || (o.TrackingCode != null && o.TrackingCode.Contains(term)));
        }
        if (request.Status is OrderStatus st) q = q.Where(o => o.Status == st);
        if (request.FromDate is DateTime from) q = q.Where(o => o.CreatedAt >= from);
        if (request.ToDate is DateTime to) q = q.Where(o => o.CreatedAt < to.AddDays(1));

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * size).Take(size)
            .Select(o => new AdminOrderListItemDto
            {
                Id = o.Id,
                OrderCode = o.OrderCode,
                CreatedAt = o.CreatedAt,
                PaidAt = o.PaidAt,
                Status = o.Status,
                FinalAmount = o.FinalAmount,
                CustomerName = o.User.FullName,
                CustomerPhone = o.User.PhoneNumber,
                ItemCount = o.Items.Count,
                TrackingCode = o.TrackingCode
            })
            .ToListAsync(ct);

        return new AdminOrderListResult { Items = items, TotalCount = total, Page = page, PageSize = size };
    }
}
