using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Application.Features.Admin.Common;
using MahanShop.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Brands;

/// <summary>ایجاد برند جدید. Slug اگر خالی باشد از Name ساخته می‌شود.</summary>
public record CreateBrandCommand(
    string Name, string? Slug, string? LogoUrl, bool IsActive, int DisplayOrder) : IRequest<int>;

public class CreateBrandCommandValidator : AbstractValidator<CreateBrandCommand>
{
    public CreateBrandCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("نام برند را وارد کنید.").MaximumLength(150);
        RuleFor(x => x.LogoUrl).MaximumLength(500);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class CreateBrandCommandHandler : IRequestHandler<CreateBrandCommand, int>
{
    private readonly IApplicationDbContext _db;
    public CreateBrandCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<int> Handle(CreateBrandCommand request, CancellationToken ct)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.Make(request.Name)
            : SlugHelper.Make(request.Slug);
        if (string.IsNullOrEmpty(slug)) slug = SlugHelper.Make(request.Name);

        if (await _db.Brands.AnyAsync(b => b.Slug == slug, ct))
            throw new ValidationException("نامک (slug) تکراری است.");

        var brand = new Brand
        {
            Name = request.Name.Trim(),
            Slug = slug,
            LogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl) ? null : request.LogoUrl.Trim(),
            IsActive = request.IsActive,
            DisplayOrder = request.DisplayOrder
        };
        _db.Brands.Add(brand);
        await _db.SaveChangesAsync(ct);
        return brand.Id;
    }
}
