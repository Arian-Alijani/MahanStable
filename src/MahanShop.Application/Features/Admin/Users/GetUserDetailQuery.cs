using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Users;

/// <summary>جزییات کاربر برای ادمین: پروفایل + آدرس‌ها + ۱۰ سفارش اخیر.</summary>
public record GetUserDetailQuery(int UserId) : IRequest<AdminUserDetailDto?>;

public class GetUserDetailQueryHandler : IRequestHandler<GetUserDetailQuery, AdminUserDetailDto?>
{
    private readonly IApplicationDbContext _db;
    public GetUserDetailQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<AdminUserDetailDto?> Handle(GetUserDetailQuery request, CancellationToken ct) =>
        await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == request.UserId)
            .Select(u => new AdminUserDetailDto
            {
                Id = u.Id,
                FullName = u.FullName,
                PhoneNumber = u.PhoneNumber,
                Email = u.Email,
                IsAdmin = u.IsAdmin,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                Addresses = u.Addresses.Select(a => new AdminUserAddressDto
                {
                    Id = a.Id,
                    Province = a.Province,
                    City = a.City,
                    PostalCode = a.PostalCode,
                    FullAddress = a.FullAddress,
                    ReceiverName = a.ReceiverName,
                    ReceiverPhone = a.ReceiverPhone
                }).ToList(),
                TotalOrderCount = u.Orders.Count,
                RecentOrders = u.Orders
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(20)
                    .Select(o => new AdminUserOrderDto
                    {
                        Id = o.Id,
                        OrderCode = o.OrderCode,
                        CreatedAt = o.CreatedAt,
                        Status = o.Status,
                        FinalAmount = o.FinalAmount
                    }).ToList()
            })
            .FirstOrDefaultAsync(ct);
}
