using MediatR;

namespace MahanShop.Application.Features.Catalog.Queries.GetCatalog;

/// <summary>کوئری مرور محصولات با فیلتر/سورت/جستجو/صفحه‌بندی (Phase 4A).</summary>
public record GetCatalogQuery(CatalogFilter Filter) : IRequest<CatalogViewModel>;
