using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Inventory;

/// <summary>تغییر سریع موجودی یک واریانت با مقدار دلتا (+۱/−۱/−۲ …). نتیجه = موجودی جدید.</summary>
public record AdjustVariantStockCommand(int VariantId, int Delta) : IRequest<int>;

public class AdjustVariantStockCommandHandler : IRequestHandler<AdjustVariantStockCommand, int>
{
    private readonly IApplicationDbContext _db;
    public AdjustVariantStockCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<int> Handle(AdjustVariantStockCommand request, CancellationToken ct)
    {
        var variant = await _db.ProductVariants.FirstOrDefaultAsync(v => v.Id == request.VariantId, ct)
            ?? throw new ValidationException("واریانت یافت نشد.");

        var next = variant.Stock + request.Delta;
        if (next < 0) next = 0;
        variant.Stock = next;
        variant.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return variant.Stock;
    }
}

/// <summary>تنظیم مستقیم موجودی یک واریانت (ویرایش inline). نتیجه = موجودی جدید.</summary>
public record SetVariantStockCommand(int VariantId, int Stock) : IRequest<int>;

public class SetVariantStockCommandHandler : IRequestHandler<SetVariantStockCommand, int>
{
    private readonly IApplicationDbContext _db;
    public SetVariantStockCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<int> Handle(SetVariantStockCommand request, CancellationToken ct)
    {
        if (request.Stock < 0) throw new ValidationException("موجودی نمی‌تواند منفی باشد.");
        var variant = await _db.ProductVariants.FirstOrDefaultAsync(v => v.Id == request.VariantId, ct)
            ?? throw new ValidationException("واریانت یافت نشد.");
        variant.Stock = request.Stock;
        variant.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return variant.Stock;
    }
}

/// <summary>نوع عملیات دسته‌ای روی موجودی واریانت‌های انتخاب‌شده.</summary>
public enum BulkStockOperation
{
    /// <summary>تنظیم موجودی همه روی یک عدد ثابت.</summary>
    Set = 0,
    /// <summary>افزایش موجودی همه به اندازهٔ یک عدد.</summary>
    Increase = 1,
    /// <summary>کاهش موجودی همه به اندازهٔ یک عدد (کف صفر).</summary>
    Decrease = 2,
    /// <summary>افزایش درصدی موجودی همه (مثلاً ۱۰٪).</summary>
    IncreasePercent = 3
}

/// <summary>اعمال دسته‌ای روی موجودی چند واریانت. نتیجه = تعداد ردیف‌های تغییرکرده.</summary>
public record BulkUpdateStockCommand(List<int> VariantIds, BulkStockOperation Operation, int Amount) : IRequest<int>;

public class BulkUpdateStockCommandValidator : AbstractValidator<BulkUpdateStockCommand>
{
    public BulkUpdateStockCommandValidator()
    {
        RuleFor(x => x.VariantIds).NotEmpty().WithMessage("هیچ واریانتی انتخاب نشده است.");
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0).WithMessage("مقدار نامعتبر است.");
    }
}

public class BulkUpdateStockCommandHandler : IRequestHandler<BulkUpdateStockCommand, int>
{
    private readonly IApplicationDbContext _db;
    public BulkUpdateStockCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<int> Handle(BulkUpdateStockCommand request, CancellationToken ct)
    {
        var ids = request.VariantIds.Distinct().ToList();
        if (ids.Count == 0) return 0;

        var variants = await _db.ProductVariants.Where(v => ids.Contains(v.Id)).ToListAsync(ct);
        var now = DateTime.UtcNow;

        foreach (var v in variants)
        {
            var next = request.Operation switch
            {
                BulkStockOperation.Set => request.Amount,
                BulkStockOperation.Increase => v.Stock + request.Amount,
                BulkStockOperation.Decrease => v.Stock - request.Amount,
                BulkStockOperation.IncreasePercent => (int)Math.Round(v.Stock * (1 + request.Amount / 100.0)),
                _ => v.Stock
            };
            v.Stock = next < 0 ? 0 : next;
            v.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(ct);
        return variants.Count;
    }
}
