using MahanShop.Domain.Enums;

namespace MahanShop.Application.Features.Admin.Orders;

/// <summary>سطر لیست سفارش در پنل ادمین.</summary>
public class AdminOrderListItemDto
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public OrderStatus Status { get; set; }
    public long FinalAmount { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public string? TrackingCode { get; set; }
}

/// <summary>نتیجه صفحه‌بندی‌شده لیست سفارش‌ها.</summary>
public class AdminOrderListResult
{
    public List<AdminOrderListItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
