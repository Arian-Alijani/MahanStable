using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Tags;

/// <summary>لیست همه برچسب‌ها برای ادمین.</summary>
public record GetTagsQuery(string? Search = null) : IRequest<List<TagListItemDto>>;

public class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, List<TagListItemDto>>
{
    private readonly IApplicationDbContext _db;
    public GetTagsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<TagListItemDto>> Handle(GetTagsQuery request, CancellationToken ct)
    {
        var q = _db.Tags.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            q = q.Where(t => t.Name.Contains(term) || t.Slug.Contains(term));
        }

        return await q
            .OrderBy(t => t.Name)
            .Select(t => new TagListItemDto
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                UsageCount = t.ProductTags.Count
            })
            .ToListAsync(ct);
    }
}

/// <summary>دریافت یک برچسب برای فرم ویرایش.</summary>
public record GetTagForEditQuery(int Id) : IRequest<TagEditDto?>;

public class GetTagForEditQueryHandler : IRequestHandler<GetTagForEditQuery, TagEditDto?>
{
    private readonly IApplicationDbContext _db;
    public GetTagForEditQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<TagEditDto?> Handle(GetTagForEditQuery request, CancellationToken ct) =>
        await _db.Tags.AsNoTracking()
            .Where(t => t.Id == request.Id)
            .Select(t => new TagEditDto { Id = t.Id, Name = t.Name, Slug = t.Slug })
            .FirstOrDefaultAsync(ct);
}
