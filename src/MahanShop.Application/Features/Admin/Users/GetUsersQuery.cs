using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Users;

/// <summary>لیست کاربران برای ادمین: جستجو (نام/موبایل/ایمیل) + فیلتر ادمین/فعال/دارای‌سفارش + صفحه‌بندی + آمار کلی.</summary>
public record GetUsersQuery(
    string? Search = null,
    bool? OnlyAdmins = null,
    bool? OnlyInactive = null,
    bool? OnlyWithOrders = null,
    int Page = 1,
    int PageSize = 20) : IRequest<AdminUserListResult>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, AdminUserListResult>
{
    private readonly IApplicationDbContext _db;
    public GetUsersQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<AdminUserListResult> Handle(GetUsersQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var size = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        // آمار کلی — همیشه روی همه کاربران (بدون فیلتر) محاسبه می‌شود
        var stats = await BuildStatsAsync(ct);

        var q = _db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            q = q.Where(u => u.FullName.Contains(term)
                          || u.PhoneNumber.Contains(term)
                          || (u.Email != null && u.Email.Contains(term)));
        }
        if (request.OnlyAdmins == true) q = q.Where(u => u.IsAdmin);
        if (request.OnlyInactive == true) q = q.Where(u => !u.IsActive);
        if (request.OnlyWithOrders == true) q = q.Where(u => u.Orders.Any());

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * size).Take(size)
            .Select(u => new AdminUserListItemDto
            {
                Id = u.Id,
                FullName = u.FullName,
                PhoneNumber = u.PhoneNumber,
                Email = u.Email,
                IsAdmin = u.IsAdmin,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                OrderCount = u.Orders.Count
            })
            .ToListAsync(ct);

        return new AdminUserListResult
        {
            Items = items,
            Stats = stats,
            TotalCount = total,
            Page = page,
            PageSize = size
        };
    }

    private async Task<AdminUserStatsDto> BuildStatsAsync(CancellationToken ct)
    {
        var total     = await _db.Users.CountAsync(ct);
        var active    = await _db.Users.CountAsync(u => u.IsActive, ct);
        var admins    = await _db.Users.CountAsync(u => u.IsAdmin, ct);
        var withOrders = await _db.Users.CountAsync(u => u.Orders.Any(), ct);

        return new AdminUserStatsDto
        {
            Total      = total,
            Active     = active,
            Inactive   = total - active,
            Admins     = admins,
            WithOrders = withOrders
        };
    }
}
