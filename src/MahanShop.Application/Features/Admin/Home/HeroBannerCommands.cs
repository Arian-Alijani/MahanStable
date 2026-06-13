using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Home;

/// <summary>یک ردیف بنر هیرو (بنر بالای صفحه) برای جدول مدیریت.</summary>
public class HeroBannerAdminDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string ImageUrl { get; set; } = null!;
    public string? MobileImageUrl { get; set; }
    public string? LinkUrl { get; set; }
    public string AltText { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>دادهٔ بنر هیرو برای فرم ویرایش.</summary>
public class HeroBannerEditDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string ImageUrl { get; set; } = null!;
    public string? MobileImageUrl { get; set; }
    public string? LinkUrl { get; set; }
    public string AltText { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>لیست بنرهای هیرو، مرتب بر اساس عدد ترتیب.</summary>
public record GetHeroBannersQuery() : IRequest<List<HeroBannerAdminDto>>;

public class GetHeroBannersQueryHandler : IRequestHandler<GetHeroBannersQuery, List<HeroBannerAdminDto>>
{
    private readonly IApplicationDbContext _db;
    public GetHeroBannersQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<HeroBannerAdminDto>> Handle(GetHeroBannersQuery request, CancellationToken ct) =>
        await _db.Banners.AsNoTracking()
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id)
            .Select(x => new HeroBannerAdminDto
            {
                Id = x.Id,
                Title = x.Title,
                ImageUrl = x.ImageUrl,
                MobileImageUrl = x.MobileImageUrl,
                LinkUrl = x.LinkUrl,
                AltText = x.AltText,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive
            })
            .ToListAsync(ct);
}

/// <summary>دادهٔ بنر هیرو برای ویرایش.</summary>
public record GetHeroBannerForEditQuery(int Id) : IRequest<HeroBannerEditDto?>;

public class GetHeroBannerForEditQueryHandler : IRequestHandler<GetHeroBannerForEditQuery, HeroBannerEditDto?>
{
    private readonly IApplicationDbContext _db;
    public GetHeroBannerForEditQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<HeroBannerEditDto?> Handle(GetHeroBannerForEditQuery request, CancellationToken ct) =>
        await _db.Banners.AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new HeroBannerEditDto
            {
                Id = x.Id,
                Title = x.Title,
                ImageUrl = x.ImageUrl,
                MobileImageUrl = x.MobileImageUrl,
                LinkUrl = x.LinkUrl,
                AltText = x.AltText,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(ct);
}

/// <summary>ایجاد بنر هیرو جدید. عدد ترتیب کوچک‌تر = بالاتر/زودتر در اسلایدر.</summary>
public record CreateHeroBannerCommand(
    string? Title, string ImageUrl, string? MobileImageUrl, string? LinkUrl,
    string AltText, int DisplayOrder, bool IsActive) : IRequest<int>;

/// <summary>ویرایش بنر هیرو.</summary>
public record UpdateHeroBannerCommand(
    int Id, string? Title, string ImageUrl, string? MobileImageUrl, string? LinkUrl,
    string AltText, int DisplayOrder, bool IsActive) : IRequest<Unit>;

/// <summary>تغییر فقط عدد ترتیب بنر هیرو (از جدول مدیریت).</summary>
public record SetHeroBannerOrderCommand(int Id, int DisplayOrder) : IRequest<Unit>;

/// <summary>فعال/غیرفعال کردن بنر هیرو.</summary>
public record ToggleHeroBannerActiveCommand(int Id) : IRequest<Unit>;

/// <summary>حذف بنر هیرو.</summary>
public record DeleteHeroBannerCommand(int Id) : IRequest<Unit>;

public class CreateHeroBannerCommandValidator : AbstractValidator<CreateHeroBannerCommand>
{
    public CreateHeroBannerCommandValidator()
    {
        RuleFor(x => x.ImageUrl).NotEmpty().WithMessage("تصویر بنر را انتخاب کنید.").MaximumLength(500);
        RuleFor(x => x.AltText).NotEmpty().WithMessage("متن جایگزین تصویر را وارد کنید.").MaximumLength(200);
        RuleFor(x => x.Title).MaximumLength(200);
        RuleFor(x => x.MobileImageUrl).MaximumLength(500);
        RuleFor(x => x.LinkUrl).MaximumLength(500);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateHeroBannerCommandValidator : AbstractValidator<UpdateHeroBannerCommand>
{
    public UpdateHeroBannerCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.ImageUrl).NotEmpty().WithMessage("تصویر بنر را انتخاب کنید.").MaximumLength(500);
        RuleFor(x => x.AltText).NotEmpty().WithMessage("متن جایگزین تصویر را وارد کنید.").MaximumLength(200);
        RuleFor(x => x.Title).MaximumLength(200);
        RuleFor(x => x.MobileImageUrl).MaximumLength(500);
        RuleFor(x => x.LinkUrl).MaximumLength(500);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class CreateHeroBannerCommandHandler : IRequestHandler<CreateHeroBannerCommand, int>
{
    private readonly IApplicationDbContext _db;
    public CreateHeroBannerCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<int> Handle(CreateHeroBannerCommand request, CancellationToken ct)
    {
        var banner = new Banner
        {
            Title = Trim(request.Title),
            ImageUrl = request.ImageUrl.Trim(),
            MobileImageUrl = Trim(request.MobileImageUrl),
            LinkUrl = Trim(request.LinkUrl),
            AltText = request.AltText.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };
        _db.Banners.Add(banner);
        await _db.SaveChangesAsync(ct);
        return banner.Id;
    }

    private static string? Trim(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();
}

public class UpdateHeroBannerCommandHandler : IRequestHandler<UpdateHeroBannerCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    public UpdateHeroBannerCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(UpdateHeroBannerCommand request, CancellationToken ct)
    {
        var b = await _db.Banners.FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new ValidationException("بنر یافت نشد.");

        b.Title = Trim(request.Title);
        b.ImageUrl = request.ImageUrl.Trim();
        b.MobileImageUrl = Trim(request.MobileImageUrl);
        b.LinkUrl = Trim(request.LinkUrl);
        b.AltText = request.AltText.Trim();
        b.DisplayOrder = request.DisplayOrder;
        b.IsActive = request.IsActive;
        b.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }

    private static string? Trim(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();
}

public class SetHeroBannerOrderCommandHandler : IRequestHandler<SetHeroBannerOrderCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    public SetHeroBannerOrderCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(SetHeroBannerOrderCommand request, CancellationToken ct)
    {
        var b = await _db.Banners.FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new ValidationException("بنر یافت نشد.");
        b.DisplayOrder = request.DisplayOrder < 0 ? 0 : request.DisplayOrder;
        b.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public class ToggleHeroBannerActiveCommandHandler : IRequestHandler<ToggleHeroBannerActiveCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    public ToggleHeroBannerActiveCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(ToggleHeroBannerActiveCommand request, CancellationToken ct)
    {
        var b = await _db.Banners.FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new ValidationException("بنر یافت نشد.");
        b.IsActive = !b.IsActive;
        b.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public class DeleteHeroBannerCommandHandler : IRequestHandler<DeleteHeroBannerCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    public DeleteHeroBannerCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(DeleteHeroBannerCommand request, CancellationToken ct)
    {
        var b = await _db.Banners.FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new ValidationException("بنر یافت نشد.");
        _db.Banners.Remove(b);
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
