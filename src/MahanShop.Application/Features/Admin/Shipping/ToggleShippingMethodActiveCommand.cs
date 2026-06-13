using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Shipping;

/// <summary>فعال/غیرفعال‌کردن نوع پست.</summary>
public record ToggleShippingMethodActiveCommand(int Id) : IRequest<bool>;

public class ToggleShippingMethodActiveCommandHandler : IRequestHandler<ToggleShippingMethodActiveCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public ToggleShippingMethodActiveCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(ToggleShippingMethodActiveCommand request, CancellationToken ct)
    {
        var method = await _db.ShippingMethods.FirstOrDefaultAsync(s => s.Id == request.Id, ct)
            ?? throw new ValidationException("روش ارسال یافت نشد.");
        method.IsActive = !method.IsActive;
        method.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return method.IsActive;
    }
}
