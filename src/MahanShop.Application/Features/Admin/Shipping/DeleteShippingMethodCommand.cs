using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Shipping;

/// <summary>
/// حذف نوع پست. اگر این روش در سفارش‌هایی استفاده شده، FK با SetNull تنظیم‌شده
/// پس سفارش‌های قدیمی snapshot نام/نرخ دارند و بدون خطا حذف می‌شود.
/// </summary>
public record DeleteShippingMethodCommand(int Id) : IRequest<bool>;

public class DeleteShippingMethodCommandHandler : IRequestHandler<DeleteShippingMethodCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public DeleteShippingMethodCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(DeleteShippingMethodCommand request, CancellationToken ct)
    {
        var method = await _db.ShippingMethods.FirstOrDefaultAsync(s => s.Id == request.Id, ct)
            ?? throw new ValidationException("روش ارسال یافت نشد.");
        _db.ShippingMethods.Remove(method);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
