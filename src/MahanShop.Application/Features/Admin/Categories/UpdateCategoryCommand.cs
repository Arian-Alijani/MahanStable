using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Application.Features.Admin.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Categories;

/// <summary>ویرایش دسته. والد نمی‌تواند خود دسته یا یکی از فرزندانش باشد.</summary>
public record UpdateCategoryCommand(
    int Id, string Name, string? Slug, string? ImageUrl, int? ParentId,
    int DisplayOrder, bool IsActive, bool ShowInMenu, bool ShowOnHome) : IRequest<bool>;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().WithMessage("نام دسته را وارد کنید.").MaximumLength(150);
        RuleFor(x => x.ImageUrl).MaximumLength(500);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public UpdateCategoryCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var cat = await _db.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, ct)
            ?? throw new ValidationException("دسته یافت نشد.");

        var slug = SlugHelper.Make(string.IsNullOrWhiteSpace(request.Slug) ? request.Name : request.Slug);
        if (string.IsNullOrEmpty(slug)) slug = SlugHelper.Make(request.Name);

        if (await _db.Categories.AnyAsync(c => c.Slug == slug && c.Id != request.Id, ct))
            throw new ValidationException("نامک (slug) تکراری است.");

        if (request.ParentId is int pid)
        {
            if (pid == request.Id)
                throw new ValidationException("دسته نمی‌تواند والد خودش باشد.");
            // جلوگیری از حلقه: والد نباید یکی از زیرشاخه‌ها باشد
            var all = await _db.Categories.AsNoTracking()
                .Select(c => new { c.Id, c.ParentId }).ToListAsync(ct);
            var byParent = all.ToLookup(c => c.ParentId);
            var descendants = new HashSet<int>();
            var queue = new Queue<int>();
            queue.Enqueue(request.Id);
            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                foreach (var child in byParent[cur]) { descendants.Add(child.Id); queue.Enqueue(child.Id); }
            }
            if (descendants.Contains(pid))
                throw new ValidationException("والد انتخابی یکی از زیرشاخه‌های همین دسته است.");
            if (!all.Any(c => c.Id == pid))
                throw new ValidationException("دستهٔ والد یافت نشد.");
        }

        cat.Name = request.Name.Trim();
        cat.Slug = slug;
        cat.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
        cat.ParentId = request.ParentId;
        cat.DisplayOrder = request.DisplayOrder;
        cat.IsActive = request.IsActive;
        cat.ShowInMenu = request.ShowInMenu;
        cat.ShowOnHome = request.ShowOnHome;
        cat.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
