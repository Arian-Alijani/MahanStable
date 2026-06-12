using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Variants;

/// <summary>ایجاد ویژگی متغیر جدید (برند/مدل/رنگ/...).</summary>
public record CreateVariantAttributeCommand(string Name, bool IsColor, int DisplayOrder) : IRequest<int>;

public class CreateVariantAttributeCommandValidator : AbstractValidator<CreateVariantAttributeCommand>
{
    public CreateVariantAttributeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("نام ویژگی را وارد کنید.").MaximumLength(100);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class CreateVariantAttributeCommandHandler : IRequestHandler<CreateVariantAttributeCommand, int>
{
    private readonly IApplicationDbContext _db;
    public CreateVariantAttributeCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<int> Handle(CreateVariantAttributeCommand request, CancellationToken ct)
    {
        var name = request.Name.Trim();
        if (await _db.VariantAttributes.AnyAsync(a => a.Name == name, ct))
            throw new ValidationException("ویژگی با این نام قبلاً ثبت شده است.");

        var attr = new VariantAttribute
        {
            Name = name,
            IsColor = request.IsColor,
            DisplayOrder = request.DisplayOrder
        };
        _db.VariantAttributes.Add(attr);
        await _db.SaveChangesAsync(ct);
        return attr.Id;
    }
}

/// <summary>ویرایش ویژگی متغیر موجود.</summary>
public record UpdateVariantAttributeCommand(int Id, string Name, bool IsColor, int DisplayOrder) : IRequest<bool>;

public class UpdateVariantAttributeCommandValidator : AbstractValidator<UpdateVariantAttributeCommand>
{
    public UpdateVariantAttributeCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().WithMessage("نام ویژگی را وارد کنید.").MaximumLength(100);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateVariantAttributeCommandHandler : IRequestHandler<UpdateVariantAttributeCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public UpdateVariantAttributeCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(UpdateVariantAttributeCommand request, CancellationToken ct)
    {
        var attr = await _db.VariantAttributes.FirstOrDefaultAsync(a => a.Id == request.Id, ct)
            ?? throw new ValidationException("ویژگی یافت نشد.");

        var name = request.Name.Trim();
        if (await _db.VariantAttributes.AnyAsync(a => a.Name == name && a.Id != request.Id, ct))
            throw new ValidationException("ویژگی با این نام قبلاً ثبت شده است.");

        attr.Name = name;
        attr.IsColor = request.IsColor;
        attr.DisplayOrder = request.DisplayOrder;
        attr.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}

/// <summary>حذف ویژگی متغیر (فقط اگر در هیچ variant استفاده نشده باشد).</summary>
public record DeleteVariantAttributeCommand(int Id) : IRequest<bool>;

public class DeleteVariantAttributeCommandHandler : IRequestHandler<DeleteVariantAttributeCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public DeleteVariantAttributeCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(DeleteVariantAttributeCommand request, CancellationToken ct)
    {
        var attr = await _db.VariantAttributes
            .Include(a => a.Values)
            .FirstOrDefaultAsync(a => a.Id == request.Id, ct)
            ?? throw new ValidationException("ویژگی یافت نشد.");

        var valueIds = attr.Values.Select(v => v.Id).ToList();
        if (valueIds.Count > 0 &&
            await _db.ProductVariantValues.AnyAsync(pvv => valueIds.Contains(pvv.AttributeValueId), ct))
            throw new ValidationException("این ویژگی در محصولات استفاده شده و قابل حذف نیست.");

        _db.VariantAttributeValues.RemoveRange(attr.Values);
        _db.VariantAttributes.Remove(attr);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
