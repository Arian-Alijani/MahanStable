using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Application.Features.Admin.Common;
using MahanShop.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Categories;

/// <summary>ایجاد دستهٔ جدید (اختیاری زیر یک والد).</summary>
public record CreateCategoryCommand(
    string Name, string? Slug, string? ImageUrl, int? ParentId,
    int DisplayOrder, bool IsActive, bool ShowInMenu, bool ShowOnHome) : IRequest<int>;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("نام دسته را وارد کنید.").MaximumLength(150);
        RuleFor(x => x.ImageUrl).MaximumLength(500);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, int>
{
    private readonly IApplicationDbContext _db;
    public CreateCategoryCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<int> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var slug = SlugHelper.Make(string.IsNullOrWhiteSpace(request.Slug) ? request.Name : request.Slug);
        if (string.IsNullOrEmpty(slug)) slug = SlugHelper.Make(request.Name);

        if (await _db.Categories.AnyAsync(c => c.Slug == slug, ct))
            throw new ValidationException("نامک (slug) تکراری است.");

        if (request.ParentId is int pid && !await _db.Categories.AnyAsync(c => c.Id == pid, ct))
            throw new ValidationException("دستهٔ والد یافت نشد.");

        var cat = new Category
        {
            Name = request.Name.Trim(),
            Slug = slug,
            ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
            ParentId = request.ParentId,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            ShowInMenu = request.ShowInMenu,
            ShowOnHome = request.ShowOnHome
        };
        _db.Categories.Add(cat);
        await _db.SaveChangesAsync(ct);
        return cat.Id;
    }
}
