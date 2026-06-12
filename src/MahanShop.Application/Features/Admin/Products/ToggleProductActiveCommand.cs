using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Products;

/// <summary>فعال/غیرفعال‌کردن محصول (حذف نرم).</summary>
public record ToggleProductActiveCommand(int Id) : IRequest<bool>;

public class ToggleProductActiveCommandHandler : IRequestHandler<ToggleProductActiveCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public ToggleProductActiveCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(ToggleProductActiveCommand request, CancellationToken ct)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new ValidationException("محصول یافت نشد.");
        product.IsActive = !product.IsActive;
        product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return product.IsActive;
    }
}
