using MediatR;

namespace MahanShop.Application.Features.Catalog.Queries.GetProductDetail;

/// <summary>کوئری جزییات محصول بر اساس slug. null اگر یافت نشد/غیرفعال.</summary>
public record GetProductDetailQuery(string Slug) : IRequest<ProductDetailDto?>;
