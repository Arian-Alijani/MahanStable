using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Entities;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Home;

/// <summary>دادهٔ نوار محصول برای فرم ویرایش.</summary>
public record GetProductRowForEditQuery(int Id) : IRequest<ProductRowEditDto?>;

public class GetProductRowForEditQueryHandler : IRequestHandler<GetProductRowForEditQuery, ProductRowEditDto?>
{
    private readonly IApplicationDbContext _db;
    public GetProductRowForEditQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ProductRowEditDto?> Handle(GetProductRowForEditQuery request, CancellationToken ct) =>
        await _db.HomeSections.AsNoTracking()
            .Where(s => s.Id == request.Id && s.SectionType == HomeSectionType.ProductRow)
            .Select(s => new ProductRowEditDto
            {
                Id = s.Id,
                Title = s.Title,
                ProductSource = s.ProductSource ?? HomeProductSource.Featured,
                CategoryId = s.CategoryId,
                MaxItems = s.MaxItems,
                DisplayOrder = s.DisplayOrder,
                IsActive = s.IsActive
            })
            .FirstOrDefaultAsync(ct);
}

/// <summary>ایجاد نوار محصول جدید (بر اساس منبع: پیشنهادی/پرفروش/جدید/تخفیف/دسته).</summary>
public record CreateProductRowCommand(
    string Title, HomeProductSource ProductSource, int? CategoryId,
    int MaxItems, int DisplayOrder, bool IsActive) : IRequest<int>;

/// <summary>ویرایش نوار محصول.</summary>
public record UpdateProductRowCommand(
    int Id, string Title, HomeProductSource ProductSource, int? CategoryId,
    int MaxItems, int DisplayOrder, bool IsActive) : IRequest<Unit>;

public class CreateProductRowCommandValidator : AbstractValidator<CreateProductRowCommand>
{
    public CreateProductRowCommandValidator() => ProductRowRules.Apply(this);
}

public class UpdateProductRowCommandValidator : AbstractValidator<UpdateProductRowCommand>
{
    public UpdateProductRowCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().WithMessage("عنوان نوار را وارد کنید.").MaximumLength(150);
        RuleFor(x => x.MaxItems).InclusiveBetween(1, 30).WithMessage("تعداد نمایش باید بین ۱ تا ۳۰ باشد.");
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryId).NotNull()
            .When(x => x.ProductSource == HomeProductSource.ByCategory)
            .WithMessage("برای نوار «بر اساس دسته» انتخاب دسته الزامی است.");
    }
}

/// <summary>قواعد مشترک ایجاد نوار محصول.</summary>
internal static class ProductRowRules
{
    public static void Apply(AbstractValidator<CreateProductRowCommand> v)
    {
        v.RuleFor(x => x.Title).NotEmpty().WithMessage("عنوان نوار را وارد کنید.").MaximumLength(150);
        v.RuleFor(x => x.MaxItems).InclusiveBetween(1, 30).WithMessage("تعداد نمایش باید بین ۱ تا ۳۰ باشد.");
        v.RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
        v.RuleFor(x => x.CategoryId).NotNull()
            .When(x => x.ProductSource == HomeProductSource.ByCategory)
            .WithMessage("برای نوار «بر اساس دسته» انتخاب دسته الزامی است.");
    }
}

public class CreateProductRowCommandHandler : IRequestHandler<CreateProductRowCommand, int>
{
    private readonly IApplicationDbContext _db;
    public CreateProductRowCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<int> Handle(CreateProductRowCommand request, CancellationToken ct)
    {
        var categoryId = await ResolveCategoryAsync(_db, request.ProductSource, request.CategoryId, ct);

        var section = new HomeSection
        {
            Title = request.Title.Trim(),
            SectionType = HomeSectionType.ProductRow,
            ProductSource = request.ProductSource,
            CategoryId = categoryId,
            MaxItems = request.MaxItems,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };
        _db.HomeSections.Add(section);
        await _db.SaveChangesAsync(ct);
        return section.Id;
    }

    internal static async Task<int?> ResolveCategoryAsync(
        IApplicationDbContext db, HomeProductSource source, int? categoryId, CancellationToken ct)
    {
        if (source != HomeProductSource.ByCategory) return null;
        if (categoryId is not int cid || !await db.Categories.AnyAsync(c => c.Id == cid, ct))
            throw new ValidationException("دستهٔ انتخاب‌شده یافت نشد.");
        return cid;
    }
}

public class UpdateProductRowCommandHandler : IRequestHandler<UpdateProductRowCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    public UpdateProductRowCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(UpdateProductRowCommand request, CancellationToken ct)
    {
        var s = await _db.HomeSections
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.SectionType == HomeSectionType.ProductRow, ct)
            ?? throw new ValidationException("نوار محصول یافت نشد.");

        s.Title = request.Title.Trim();
        s.ProductSource = request.ProductSource;
        s.CategoryId = await CreateProductRowCommandHandler.ResolveCategoryAsync(_db, request.ProductSource, request.CategoryId, ct);
        s.MaxItems = request.MaxItems;
        s.DisplayOrder = request.DisplayOrder;
        s.IsActive = request.IsActive;
        s.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
