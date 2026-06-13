using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Orders;

/// <summary>
/// لیست سفارش‌ها برای ادمین: جستجو + فیلتر وضعیت + بازه تاریخ + سورت + صفحه‌بندی.
/// پیش‌فرض سورت = جدیدترین.
/// </summary>
public record GetOrdersQuery(
    string? Search = null,
    OrderStatus? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    OrderSortOption Sort = OrderSortOption.Newest,
    int Page = 1,
    int PageSize = 20) : IRequest<AdminOrderListResult>;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, AdminOrderListResult>
{
    private readonly IApplicationDbContext _db;
    public GetOrdersQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<AdminOrderListResult> Handle(GetOrdersQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var size = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var q = _db.Orders.AsNoTracking();

        // ── جست‌وجو
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            q = q.Where(o => o.OrderCode.Contains(term)
                          || o.User.FullName.Contains(term)
                          || o.User.PhoneNumber.Contains(term)
                          || (o.TrackingCode != null && o.TrackingCode.Contains(term)));
        }

        // ── فیلتر وضعیت
        if (request.Status is OrderStatus st) q = q.Where(o => o.Status == st);

        // ── بازه تاریخ
        if (request.FromDate is DateTime from) q = q.Where(o => o.CreatedAt >= from);
        if (request.ToDate is DateTime to) q = q.Where(o => o.CreatedAt < to.AddDays(1));

        // ── آمار (روی query فیلترشده — یعنی آمار را هم با فیلتر‌های فعال محاسبه می‌کنیم)
        var stats = await BuildStatsAsync(q, ct);

        var total = await q.CountAsync(ct);

        // ── سورت
        IQueryable<Domain.Entities.Order> sorted = request.Sort switch
        {
            OrderSortOption.Oldest    => q.OrderBy(o => o.CreatedAt),
            OrderSortOption.AmountDesc => q.OrderByDescending(o => o.FinalAmount),
            OrderSortOption.AmountAsc  => q.OrderBy(o => o.FinalAmount),
            _                          => q.OrderByDescending(o => o.CreatedAt)   // Newest (default)
        };

        var items = await sorted
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
                TrackingCode = o.TrackingCode,
                ShippingMethodName = o.ShippingMethodName
            })
            .ToListAsync(ct);

        return new AdminOrderListResult
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = size,
            Stats = stats
        };
    }

    private static async Task<AdminOrderStatsDto> BuildStatsAsync(
        IQueryable<Domain.Entities.Order> q, CancellationToken ct)
    {
        // یک query گروه‌بندی‌شده برای همه وضعیت‌ها
        var groups = await q
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var dict = groups.ToDictionary(x => x.Status, x => x.Count);

        int Get(OrderStatus s) => dict.TryGetValue(s, out var c) ? c : 0;

        return new AdminOrderStatsDto
        {
            Total      = dict.Values.Sum(),
            Pending    = Get(OrderStatus.Pending) + Get(OrderStatus.AwaitingPayment),
            Paid       = Get(OrderStatus.Paid),
            Processing = Get(OrderStatus.Processing),
            Shipped    = Get(OrderStatus.Shipped),
            Delivered  = Get(OrderStatus.Delivered),
            Canceled   = Get(OrderStatus.Canceled) + Get(OrderStatus.Refunded)
        };
    }
}
