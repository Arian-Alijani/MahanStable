using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Categories;

/// <summary>گزینه‌های والد برای فرم دسته (می‌توان دستهٔ excludeId و فرزندانش را کنار گذاشت تا حلقه نسازد).</summary>
public record GetCategoryOptionsQuery(int? ExcludeId = null) : IRequest<List<CategoryOptionDto>>;

public class GetCategoryOptionsQueryHandler : IRequestHandler<GetCategoryOptionsQuery, List<CategoryOptionDto>>
{
    private readonly IApplicationDbContext _db;
    public GetCategoryOptionsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<CategoryOptionDto>> Handle(GetCategoryOptionsQuery request, CancellationToken ct)
    {
        var all = await _db.Categories.AsNoTracking()
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .Select(c => new { c.Id, c.Name, c.ParentId })
            .ToListAsync(ct);

        // شناسه‌های ممنوع = خودِ دسته + همه زیرشاخه‌ها (جلوگیری از حلقه)
        var excluded = new HashSet<int>();
        if (request.ExcludeId is int ex)
        {
            excluded.Add(ex);
            var byParent = all.ToLookup(c => c.ParentId);
            var queue = new Queue<int>();
            queue.Enqueue(ex);
            while (queue.Count > 0)
            {
                var pid = queue.Dequeue();
                foreach (var child in byParent[pid])
                {
                    excluded.Add(child.Id);
                    queue.Enqueue(child.Id);
                }
            }
        }

        var byParent2 = all.ToLookup(c => c.ParentId);
        var result = new List<CategoryOptionDto>();
        void Walk(int? parentId, int depth)
        {
            foreach (var c in byParent2[parentId])
            {
                if (excluded.Contains(c.Id)) continue;
                result.Add(new CategoryOptionDto { Id = c.Id, Name = c.Name, Depth = depth });
                Walk(c.Id, depth + 1);
            }
        }
        Walk(null, 0);
        return result;
    }
}
