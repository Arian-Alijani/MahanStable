using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Entities;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Home;

/// <summary>دادهٔ بنر میانی برای فرم ویرایش.</summary>
public record GetBannerForEditQuery(int Id) : IRequest<BannerEditDto?>;

public class GetBannerForEditQueryHandler : IRequestHandler<GetBannerForEditQuery, BannerEditDto?>
{
    private readonly IApplicationDbContext _db;
    public GetBannerForEditQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<BannerEditDto?> Handle(GetBannerForEditQuery request, CancellationToken ct) =>
        await _db.HomeSections.AsNoTracking()
            .Where(s => s.Id == request.Id && s.SectionType == HomeSectionType.PromoBanner)
            .Select(s => new BannerEditDto
            {
                Id = s.Id,
                Subtitle = s.Subtitle,
                ImageUrl = s.ImageUrl,
                MobileImageUrl = s.MobileImageUrl,
                LinkUrl = s.LinkUrl,
                DisplayOrder = s.DisplayOrder,
                IsActive = s.IsActive
            })
            .FirstOrDefaultAsync(ct);
}

/// <summary>ایجاد بنر میانی جدید. ترتیب با نوارهای محصول مشترک است؛ ترتیبِ یکسانِ دو بنر = یک ردیف کنار هم.</summary>
public record CreateBannerCommand(
    string? Subtitle, string ImageUrl, string? MobileImageUrl, string? LinkUrl,
    int DisplayOrder, bool IsActive) : IRequest<int>;

/// <summary>ویرایش بنر میانی.</summary>
public record UpdateBannerCommand(
    int Id, string? Subtitle, string ImageUrl, string? MobileImageUrl, string? LinkUrl,
    int DisplayOrder, bool IsActive) : IRequest<Unit>;

public class CreateBannerCommandValidator : AbstractValidator<CreateBannerCommand>
{
    public CreateBannerCommandValidator()
    {
        RuleFor(x => x.ImageUrl).NotEmpty().WithMessage("تصویر بنر را انتخاب کنید.").MaximumLength(500);
        RuleFor(x => x.MobileImageUrl).MaximumLength(500);
        RuleFor(x => x.LinkUrl).MaximumLength(500);
        RuleFor(x => x.Subtitle).MaximumLength(200);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateBannerCommandValidator : AbstractValidator<UpdateBannerCommand>
{
    public UpdateBannerCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.ImageUrl).NotEmpty().WithMessage("تصویر بنر را انتخاب کنید.").MaximumLength(500);
        RuleFor(x => x.MobileImageUrl).MaximumLength(500);
        RuleFor(x => x.LinkUrl).MaximumLength(500);
        RuleFor(x => x.Subtitle).MaximumLength(200);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class CreateBannerCommandHandler : IRequestHandler<CreateBannerCommand, int>
{
    private readonly IApplicationDbContext _db;
    public CreateBannerCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<int> Handle(CreateBannerCommand request, CancellationToken ct)
    {
        var section = new HomeSection
        {
            Title = "",
            SectionType = HomeSectionType.PromoBanner,
            Subtitle = Trim(request.Subtitle),
            ImageUrl = request.ImageUrl.Trim(),
            MobileImageUrl = Trim(request.MobileImageUrl),
            LinkUrl = Trim(request.LinkUrl),
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };
        _db.HomeSections.Add(section);
        await _db.SaveChangesAsync(ct);
        return section.Id;
    }

    private static string? Trim(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();
}

public class UpdateBannerCommandHandler : IRequestHandler<UpdateBannerCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    public UpdateBannerCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(UpdateBannerCommand request, CancellationToken ct)
    {
        var s = await _db.HomeSections
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.SectionType == HomeSectionType.PromoBanner, ct)
            ?? throw new ValidationException("بنر یافت نشد.");

        s.Subtitle = Trim(request.Subtitle);
        s.ImageUrl = request.ImageUrl.Trim();
        s.MobileImageUrl = Trim(request.MobileImageUrl);
        s.LinkUrl = Trim(request.LinkUrl);
        s.DisplayOrder = request.DisplayOrder;
        s.IsActive = request.IsActive;
        s.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }

    private static string? Trim(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();
}
