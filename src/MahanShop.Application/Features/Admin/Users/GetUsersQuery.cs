using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Users;

/// <summary>لیست کاربران برای ادمین: جستجو (نام/موبایل/ایمیل) + فیلتر ادمین/فعال + صفحه‌بندی.</summary>
public record GetUsersQuery(
    string? Search = null, bool? OnlyAdmins = null, bool? OnlyInactive = null,
    int Page = 1, int PageSize = 20) : IRequest<AdminUserListResult>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, AdminUserListResult>
{
    private readonly IApplicationDbContext _db;
    public GetUsersQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<AdminUserListResult> Handle(GetUsersQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var size = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

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

        return new AdminUserListResult { Items = items, TotalCount = total, Page = page, PageSize = size };
    }
}
