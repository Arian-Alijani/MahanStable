using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Account.Queries.GetUserProfile;

/// <summary>اطلاعات پروفایل کاربر جاری برای پنل (نام، شماره، ایمیل، شمارش سفارش).</summary>
public record GetUserProfileQuery(int UserId) : IRequest<UserProfileDto?>;

public class UserProfileDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int OrderCount { get; set; }
    public DateTime MemberSince { get; set; }
}

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto?>
{
    private readonly IApplicationDbContext _db;
    public GetUserProfileQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<UserProfileDto?> Handle(GetUserProfileQuery request, CancellationToken ct) =>
        await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == request.UserId)
            .Select(u => new UserProfileDto
            {
                Id = u.Id,
                FullName = u.FullName,
                PhoneNumber = u.PhoneNumber,
                Email = u.Email,
                OrderCount = u.Orders.Count,
                MemberSince = u.CreatedAt
            })
            .FirstOrDefaultAsync(ct);
}
