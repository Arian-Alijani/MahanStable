using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using DomainFeature = MahanShop.Domain.Entities.Feature;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Features;

/// <summary>ایجاد مشخصه فنی جدید (RAM، حافظه، ...).</summary>
public record CreateFeatureCommand(string Name, int DisplayOrder) : IRequest<int>;

public class CreateFeatureCommandValidator : AbstractValidator<CreateFeatureCommand>
{
    public CreateFeatureCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("نام مشخصه را وارد کنید.").MaximumLength(100);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class CreateFeatureCommandHandler : IRequestHandler<CreateFeatureCommand, int>
{
    private readonly IApplicationDbContext _db;
    public CreateFeatureCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<int> Handle(CreateFeatureCommand request, CancellationToken ct)
    {
        var name = request.Name.Trim();
        if (await _db.Features.AnyAsync(f => f.Name == name, ct))
            throw new ValidationException("مشخصه با این نام قبلاً ثبت شده است.");

        var feature = new DomainFeature { Name = name, DisplayOrder = request.DisplayOrder };
        _db.Features.Add(feature);
        await _db.SaveChangesAsync(ct);
        return feature.Id;
    }
}

/// <summary>ویرایش مشخصه فنی موجود.</summary>
public record UpdateFeatureCommand(int Id, string Name, int DisplayOrder) : IRequest<bool>;

public class UpdateFeatureCommandValidator : AbstractValidator<UpdateFeatureCommand>
{
    public UpdateFeatureCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().WithMessage("نام مشخصه را وارد کنید.").MaximumLength(100);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateFeatureCommandHandler : IRequestHandler<UpdateFeatureCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public UpdateFeatureCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(UpdateFeatureCommand request, CancellationToken ct)
    {
        var feature = await _db.Features.FirstOrDefaultAsync(f => f.Id == request.Id, ct)
            ?? throw new ValidationException("مشخصه یافت نشد.");

        var name = request.Name.Trim();
        if (await _db.Features.AnyAsync(f => f.Name == name && f.Id != request.Id, ct))
            throw new ValidationException("مشخصه با این نام قبلاً ثبت شده است.");

        feature.Name = name;
        feature.DisplayOrder = request.DisplayOrder;
        feature.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}

/// <summary>حذف مشخصه فنی + مقادیر محصولی آن.</summary>
public record DeleteFeatureCommand(int Id) : IRequest<bool>;

public class DeleteFeatureCommandHandler : IRequestHandler<DeleteFeatureCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public DeleteFeatureCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(DeleteFeatureCommand request, CancellationToken ct)
    {
        var feature = await _db.Features
            .Include(f => f.ProductFeatures)
            .FirstOrDefaultAsync(f => f.Id == request.Id, ct)
            ?? throw new ValidationException("مشخصه یافت نشد.");

        _db.ProductFeatures.RemoveRange(feature.ProductFeatures);
        _db.Features.Remove(feature);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
