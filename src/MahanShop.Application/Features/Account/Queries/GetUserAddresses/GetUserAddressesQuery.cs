using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Account.Queries.GetUserAddresses;

/// <summary>آدرس‌های یک کاربر برای انتخاب در checkout.</summary>
public record GetUserAddressesQuery(int UserId) : IRequest<List<AddressDto>>;

public class AddressDto
{
    public int Id { get; set; }
    public string Province { get; set; } = null!;
    public string City { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string FullAddress { get; set; } = null!;
    public string ReceiverName { get; set; } = null!;
    public string ReceiverPhone { get; set; } = null!;
}

public class GetUserAddressesQueryHandler : IRequestHandler<GetUserAddressesQuery, List<AddressDto>>
{
    private readonly IApplicationDbContext _db;
    public GetUserAddressesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<AddressDto>> Handle(GetUserAddressesQuery request, CancellationToken ct) =>
        await _db.Addresses
            .AsNoTracking()
            .Where(a => a.UserId == request.UserId)
            .OrderByDescending(a => a.Id)
            .Select(a => new AddressDto
            {
                Id = a.Id,
                Province = a.Province,
                City = a.City,
                PostalCode = a.PostalCode,
                FullAddress = a.FullAddress,
                ReceiverName = a.ReceiverName,
                ReceiverPhone = a.ReceiverPhone
            })
            .ToListAsync(ct);
}
