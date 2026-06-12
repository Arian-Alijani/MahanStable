using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Categories;

/// <summary>فعال/غیرفعال‌کردن دسته (حذف نرم).</summary>
public record ToggleCategoryActiveCommand(int Id) : IRequest<bool>;

public class ToggleCategoryActiveCommandHandler : IRequestHandler<ToggleCategoryActiveCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public ToggleCategoryActiveCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(ToggleCategoryActiveCommand request, CancellationToken ct)
    {
        var cat = await _db.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, ct)
            ?? throw new ValidationException("دسته یافت نشد.");
        cat.IsActive = !cat.IsActive;
        cat.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return cat.IsActive;
    }
}
