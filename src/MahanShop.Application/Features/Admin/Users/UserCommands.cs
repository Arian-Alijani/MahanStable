using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Users;

/// <summary>تغییر نقش ادمین کاربر. گارد: ادمین جاری نمی‌تواند نقش خودش را بردارد (قفل‌شدن پنل).</summary>
public record ToggleUserAdminCommand(int UserId, int CurrentAdminId) : IRequest<bool>;

public class ToggleUserAdminCommandHandler : IRequestHandler<ToggleUserAdminCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public ToggleUserAdminCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(ToggleUserAdminCommand request, CancellationToken ct)
    {
        if (request.UserId == request.CurrentAdminId)
            throw new ValidationException("نمی‌توانید نقش ادمین خودتان را تغییر دهید.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
            ?? throw new ValidationException("کاربر یافت نشد.");

        user.IsAdmin = !user.IsAdmin;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return user.IsAdmin;
    }
}

/// <summary>فعال/غیرفعال‌کردن کاربر (مسدودسازی نرم). گارد: ادمین جاری نمی‌تواند خودش را غیرفعال کند.</summary>
public record ToggleUserActiveCommand(int UserId, int CurrentAdminId) : IRequest<bool>;

public class ToggleUserActiveCommandHandler : IRequestHandler<ToggleUserActiveCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public ToggleUserActiveCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(ToggleUserActiveCommand request, CancellationToken ct)
    {
        if (request.UserId == request.CurrentAdminId)
            throw new ValidationException("نمی‌توانید حساب خودتان را غیرفعال کنید.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
            ?? throw new ValidationException("کاربر یافت نشد.");

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return user.IsActive;
    }
}

/// <summary>حذف آدرس کاربر توسط ادمین (در صورت داده نامعتبر/درخواست کاربر). آدرس استفاده‌شده در سفارش حذف نمی‌شود.</summary>
public record DeleteUserAddressCommand(int UserId, int AddressId) : IRequest;

public class DeleteUserAddressCommandHandler : IRequestHandler<DeleteUserAddressCommand>
{
    private readonly IApplicationDbContext _db;
    public DeleteUserAddressCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteUserAddressCommand request, CancellationToken ct)
    {
        var address = await _db.Addresses
            .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == request.UserId, ct)
            ?? throw new ValidationException("آدرس یافت نشد.");

        var usedInOrder = await _db.Orders.AnyAsync(o => o.AddressId == address.Id, ct);
        if (usedInOrder)
            throw new ValidationException("این آدرس در سفارش(ها) استفاده شده و قابل حذف نیست.");

        _db.Addresses.Remove(address);
        await _db.SaveChangesAsync(ct);
    }
}
