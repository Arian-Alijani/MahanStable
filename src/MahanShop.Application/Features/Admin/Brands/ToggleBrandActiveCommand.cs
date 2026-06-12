using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Brands;

/// <summary>فعال/غیرفعال‌کردن برند (حذف نرم).</summary>
public record ToggleBrandActiveCommand(int Id) : IRequest<bool>;

public class ToggleBrandActiveCommandHandler : IRequestHandler<ToggleBrandActiveCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public ToggleBrandActiveCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(ToggleBrandActiveCommand request, CancellationToken ct)
    {
        var brand = await _db.Brands.FirstOrDefaultAsync(b => b.Id == request.Id, ct)
            ?? throw new ValidationException("برند یافت نشد.");
        brand.IsActive = !brand.IsActive;
        brand.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return brand.IsActive;
    }
}
