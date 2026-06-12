using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Categories;

/// <summary>دریافت یک دسته برای فرم ویرایش.</summary>
public record GetCategoryForEditQuery(int Id) : IRequest<CategoryEditDto?>;

public class GetCategoryForEditQueryHandler : IRequestHandler<GetCategoryForEditQuery, CategoryEditDto?>
{
    private readonly IApplicationDbContext _db;
    public GetCategoryForEditQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<CategoryEditDto?> Handle(GetCategoryForEditQuery request, CancellationToken ct) =>
        await _db.Categories.AsNoTracking()
            .Where(c => c.Id == request.Id)
            .Select(c => new CategoryEditDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                ImageUrl = c.ImageUrl,
                ParentId = c.ParentId,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                ShowInMenu = c.ShowInMenu,
                ShowOnHome = c.ShowOnHome
            })
            .FirstOrDefaultAsync(ct);
}
