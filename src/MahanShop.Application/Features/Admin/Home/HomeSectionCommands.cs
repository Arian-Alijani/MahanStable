using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Home;

/// <summary>تغییر عدد ترتیب یک نوار صفحهٔ اصلی (۱ = اولین ردیف از بالا).</summary>
public record SetHomeSectionOrderCommand(int Id, int DisplayOrder) : IRequest<Unit>;

public class SetHomeSectionOrderCommandValidator : AbstractValidator<SetHomeSectionOrderCommand>
{
    public SetHomeSectionOrderCommandValidator() =>
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0).WithMessage("عدد ترتیب نمی‌تواند منفی باشد.");
}

public class SetHomeSectionOrderCommandHandler : IRequestHandler<SetHomeSectionOrderCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    public SetHomeSectionOrderCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(SetHomeSectionOrderCommand request, CancellationToken ct)
    {
        var s = await _db.HomeSections.FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new ValidationException("نوار یافت نشد.");
        s.DisplayOrder = request.DisplayOrder;
        s.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

/// <summary>فعال/غیرفعال‌کردن یک نوار صفحهٔ اصلی.</summary>
public record ToggleHomeSectionActiveCommand(int Id) : IRequest<bool>;

public class ToggleHomeSectionActiveCommandHandler : IRequestHandler<ToggleHomeSectionActiveCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public ToggleHomeSectionActiveCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(ToggleHomeSectionActiveCommand request, CancellationToken ct)
    {
        var s = await _db.HomeSections.FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new ValidationException("نوار یافت نشد.");
        s.IsActive = !s.IsActive;
        s.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return s.IsActive;
    }
}

/// <summary>حذف کامل یک نوار صفحهٔ اصلی (نوار محصول یا بنر میانی).</summary>
public record DeleteHomeSectionCommand(int Id) : IRequest<Unit>;

public class DeleteHomeSectionCommandHandler : IRequestHandler<DeleteHomeSectionCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    public DeleteHomeSectionCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(DeleteHomeSectionCommand request, CancellationToken ct)
    {
        var s = await _db.HomeSections.FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new ValidationException("نوار یافت نشد.");
        _db.HomeSections.Remove(s);
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
