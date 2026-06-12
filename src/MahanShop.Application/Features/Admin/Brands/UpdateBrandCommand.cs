using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Application.Features.Admin.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Brands;

/// <summary>ویرایش برند موجود.</summary>
public record UpdateBrandCommand(
    int Id, string Name, string? Slug, string? LogoUrl, bool IsActive, int DisplayOrder) : IRequest<bool>;

public class UpdateBrandCommandValidator : AbstractValidator<UpdateBrandCommand>
{
    public UpdateBrandCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().WithMessage("نام برند را وارد کنید.").MaximumLength(150);
        RuleFor(x => x.LogoUrl).MaximumLength(500);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateBrandCommandHandler : IRequestHandler<UpdateBrandCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public UpdateBrandCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(UpdateBrandCommand request, CancellationToken ct)
    {
        var brand = await _db.Brands.FirstOrDefaultAsync(b => b.Id == request.Id, ct)
            ?? throw new ValidationException("برند یافت نشد.");

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.Make(request.Name)
            : SlugHelper.Make(request.Slug);
        if (string.IsNullOrEmpty(slug)) slug = SlugHelper.Make(request.Name);

        if (await _db.Brands.AnyAsync(b => b.Slug == slug && b.Id != request.Id, ct))
            throw new ValidationException("نامک (slug) تکراری است.");

        brand.Name = request.Name.Trim();
        brand.Slug = slug;
        brand.LogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl) ? null : request.LogoUrl.Trim();
        brand.IsActive = request.IsActive;
        brand.DisplayOrder = request.DisplayOrder;
        brand.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
