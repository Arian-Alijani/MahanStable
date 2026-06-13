using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Dashboard;

/// <summary>آمار داشبورد ادمین: فروش امروز/کل، سفارش‌های جدید، موجودی کم، شمارنده‌ها + سفارش‌های اخیر.</summary>
public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;

public class DashboardStatsDto
{
    public long TodaySales { get; set; }
    public long TotalSales { get; set; }
    public int TodayOrderCount { get; set; }
    public int TotalOrderCount { get; set; }          // F10: کل سفارش‌های واقعی (غیر از Pending/AwaitingPayment)
    public int PendingProcessingCount { get; set; }   // پرداخت‌شده‌های منتظر پردازش/ارسال
    public int LowStockCount { get; set; }
    public int ProductCount { get; set; }
    public int UserCount { get; set; }
    public List<DashboardRecentOrderDto> RecentOrders { get; set; } = new();
    public List<DashboardLowStockDto> LowStockProducts { get; set; } = new();

    /// <summary>F10: سری ۷ روز اخیر (شامل امروز) — برای نمودار SVG داشبورد.</summary>
    public List<DashboardDailyPointDto> Last7Days { get; set; } = new();
}

public class DashboardDailyPointDto
{
    public DateTime Date { get; set; }            // تاریخ روز (UTC date)
    public int OrderCount { get; set; }            // تعداد سفارش‌های sold همان روز
    public long Sales { get; set; }                // مجموع فروش sold همان روز
}

public class DashboardRecentOrderDto
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = null!;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public long FinalAmount { get; set; }
}

public class DashboardLowStockDto
{
    public int ProductId { get; set; }
    public string Title { get; set; } = null!;
    public string? VariantTitle { get; set; }
    public int Stock { get; set; }
}

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private const int LowStockThreshold = 5;

    /// <summary>سفارش‌هایی که «فروش» حساب می‌شوند (پرداخت‌شده و مرجوع/لغونشده).</summary>
    private static readonly OrderStatus[] SoldStatuses =
    {
        OrderStatus.Paid, OrderStatus.Processing, OrderStatus.Shipped, OrderStatus.Delivered
    };

    private readonly IApplicationDbContext _db;
    public GetDashboardStatsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var dto = new DashboardStatsDto();

        var sold = _db.Orders.AsNoTracking().Where(o => SoldStatuses.Contains(o.Status));

        dto.TodaySales = await sold
            .Where(o => o.PaidAt >= today)
            .SumAsync(o => (long?)o.FinalAmount, ct) ?? 0;
        dto.TotalSales = await sold.SumAsync(o => (long?)o.FinalAmount, ct) ?? 0;

        dto.TodayOrderCount = await sold.CountAsync(o => o.PaidAt >= today, ct);
        // F10: کل سفارش‌ها (به‌غیر از سفارش‌های هنوز پرداخت‌نشده) — برای کارت آماری «کل سفارش‌ها».
        dto.TotalOrderCount = await _db.Orders.AsNoTracking()
            .CountAsync(o => o.Status != OrderStatus.Pending && o.Status != OrderStatus.AwaitingPayment, ct);
        dto.PendingProcessingCount = await _db.Orders.AsNoTracking()
            .CountAsync(o => o.Status == OrderStatus.Paid || o.Status == OrderStatus.Processing, ct);

        dto.ProductCount = await _db.Products.AsNoTracking().CountAsync(o => o.IsActive, ct);
        dto.UserCount = await _db.Users.AsNoTracking().CountAsync(ct);

        // موجودی کم: محصولات بدون variant روی Stock خودشان، با variant روی موجودی هر variant فعال
        var lowSimple = await _db.Products.AsNoTracking()
            .Where(p => p.IsActive && !p.HasVariants && p.Stock <= LowStockThreshold)
            .OrderBy(p => p.Stock)
            .Select(p => new DashboardLowStockDto { ProductId = p.Id, Title = p.Title, Stock = p.Stock })
            .Take(10)
            .ToListAsync(ct);

        var lowVariants = await _db.ProductVariants.AsNoTracking()
            .Where(v => v.IsActive && v.Product.IsActive && v.Product.HasVariants && v.Stock <= LowStockThreshold)
            .OrderBy(v => v.Stock)
            .Select(v => new DashboardLowStockDto
            {
                ProductId = v.ProductId,
                Title = v.Product.Title,
                VariantTitle = v.Sku,
                Stock = v.Stock
            })
            .Take(10)
            .ToListAsync(ct);

        dto.LowStockProducts = lowSimple.Concat(lowVariants)
            .OrderBy(x => x.Stock).Take(10).ToList();
        dto.LowStockCount = await _db.Products.AsNoTracking()
                .CountAsync(p => p.IsActive && !p.HasVariants && p.Stock <= LowStockThreshold, ct)
            + await _db.ProductVariants.AsNoTracking()
                .CountAsync(v => v.IsActive && v.Product.IsActive && v.Product.HasVariants && v.Stock <= LowStockThreshold, ct);

        dto.RecentOrders = await _db.Orders.AsNoTracking()
            .Where(o => o.Status != OrderStatus.Pending && o.Status != OrderStatus.AwaitingPayment)
            .OrderByDescending(o => o.CreatedAt)
            .Take(8)
            .Select(o => new DashboardRecentOrderDto
            {
                Id = o.Id,
                OrderCode = o.OrderCode,
                CustomerName = o.User.FullName,
                CreatedAt = o.CreatedAt,
                Status = o.Status,
                FinalAmount = o.FinalAmount
            })
            .ToListAsync(ct);

        // F10: سری ۷ روز اخیر (شامل امروز) — برای نمودار SVG داشبورد.
        // معیار «روز» = PaidAt (روز پرداخت). فقط sold statuses شمرده می‌شود تا با کارت‌های فروش هم‌خوان باشد.
        var weekStart = today.AddDays(-6);   // ۷ روز شامل امروز
        var dailyRaw = await sold
            .Where(o => o.PaidAt >= weekStart)
            .GroupBy(o => o.PaidAt!.Value.Date)
            .Select(g => new
            {
                Date = g.Key,
                OrderCount = g.Count(),
                Sales = g.Sum(o => (long?)o.FinalAmount) ?? 0
            })
            .ToListAsync(ct);

        var dailyMap = dailyRaw.ToDictionary(x => x.Date, x => x);
        dto.Last7Days = Enumerable.Range(0, 7)
            .Select(i => weekStart.AddDays(i))
            .Select(d => dailyMap.TryGetValue(d, out var v)
                ? new DashboardDailyPointDto { Date = d, OrderCount = v.OrderCount, Sales = v.Sales }
                : new DashboardDailyPointDto { Date = d, OrderCount = 0, Sales = 0 })
            .ToList();

        return dto;
    }
}
