using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Variants;

/// <summary>افزودن یک مقدار به pool یک ویژگی (مثلا سامسونگ به ویژگی برند).</summary>
public record CreateVariantAttributeValueCommand(int AttributeId, string Value, string? ColorHex, string? LogoUrl, int DisplayOrder) : IRequest<int>;

public class CreateVariantAttributeValueCommandValidator : AbstractValidator<CreateVariantAttributeValueCommand>
{
    public CreateVariantAttributeValueCommandValidator()
    {
        RuleFor(x => x.AttributeId).GreaterThan(0);
        RuleFor(x => x.Value).NotEmpty().WithMessage("مقدار را وارد کنید.").MaximumLength(150);
        RuleFor(x => x.ColorHex).MaximumLength(20);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class CreateVariantAttributeValueCommandHandler : IRequestHandler<CreateVariantAttributeValueCommand, int>
{
    private readonly IApplicationDbContext _db;
    public CreateVariantAttributeValueCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<int> Handle(CreateVariantAttributeValueCommand request, CancellationToken ct)
    {
        if (!await _db.VariantAttributes.AnyAsync(a => a.Id == request.AttributeId, ct))
            throw new ValidationException("ویژگی یافت نشد.");

        var value = request.Value.Trim();
        if (await _db.VariantAttributeValues.AnyAsync(v => v.AttributeId == request.AttributeId && v.Value == value, ct))
            throw new ValidationException("این مقدار قبلاً ثبت شده است.");

        var entity = new VariantAttributeValue
        {
            AttributeId = request.AttributeId,
            Value = value,
            ColorHex = string.IsNullOrWhiteSpace(request.ColorHex) ? null : request.ColorHex.Trim(),
            LogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl) ? null : request.LogoUrl.Trim(),
            DisplayOrder = request.DisplayOrder
        };
        _db.VariantAttributeValues.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Id;
    }
}

/// <summary>ویرایش یک مقدار در pool.</summary>
public record UpdateVariantAttributeValueCommand(int Id, string Value, string? ColorHex, string? LogoUrl, int DisplayOrder) : IRequest<bool>;

public class UpdateVariantAttributeValueCommandValidator : AbstractValidator<UpdateVariantAttributeValueCommand>
{
    public UpdateVariantAttributeValueCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Value).NotEmpty().WithMessage("مقدار را وارد کنید.").MaximumLength(150);
        RuleFor(x => x.ColorHex).MaximumLength(20);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateVariantAttributeValueCommandHandler : IRequestHandler<UpdateVariantAttributeValueCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public UpdateVariantAttributeValueCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(UpdateVariantAttributeValueCommand request, CancellationToken ct)
    {
        var entity = await _db.VariantAttributeValues.FirstOrDefaultAsync(v => v.Id == request.Id, ct)
            ?? throw new ValidationException("مقدار یافت نشد.");

        var value = request.Value.Trim();
        if (await _db.VariantAttributeValues.AnyAsync(v => v.AttributeId == entity.AttributeId && v.Value == value && v.Id != request.Id, ct))
            throw new ValidationException("این مقدار قبلاً ثبت شده است.");

        entity.Value = value;
        entity.ColorHex = string.IsNullOrWhiteSpace(request.ColorHex) ? null : request.ColorHex.Trim();
        entity.LogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl) ? null : request.LogoUrl.Trim();
        entity.DisplayOrder = request.DisplayOrder;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}

/// <summary>حذف یک مقدار از pool (فقط اگر در هیچ variant استفاده نشده باشد).</summary>
public record DeleteVariantAttributeValueCommand(int Id) : IRequest<bool>;

public class DeleteVariantAttributeValueCommandHandler : IRequestHandler<DeleteVariantAttributeValueCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public DeleteVariantAttributeValueCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(DeleteVariantAttributeValueCommand request, CancellationToken ct)
    {
        var entity = await _db.VariantAttributeValues.FirstOrDefaultAsync(v => v.Id == request.Id, ct)
            ?? throw new ValidationException("مقدار یافت نشد.");

        if (await _db.ProductVariantValues.AnyAsync(pvv => pvv.AttributeValueId == request.Id, ct))
            throw new ValidationException("این مقدار در محصولات استفاده شده و قابل حذف نیست.");

        _db.VariantAttributeValues.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
