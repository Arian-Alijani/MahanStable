using MahanShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Cart.Queries.GetCart;

/// <summary>سبد را از DB می‌سازد: قیمت نهایی = (DiscountPrice ?? Price) + PriceModifier رنگ. موجودی = رنگ (اگر انتخاب شد) وگرنه محصول. Quantity به موجودی clamp. اقلام غیرفعال/صفرموجودی حذف.</summary>
public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartViewModel>
{
    private readonly IApplicationDbContext _db;

    public GetCartQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<CartViewModel> Handle(GetCartQuery request, CancellationToken ct)
    {
        var vm = new CartViewModel();
        if (request.Items.Count == 0) return vm;

        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();

        var products = await _db.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Slug,
                p.HasVariants,
                p.Price,
                p.DiscountPrice,
                p.Stock,
                ImageUrl = p.Images
                    .OrderByDescending(im => im.IsMain).ThenBy(im => im.DisplayOrder)
                    .Select(im => im.Url).FirstOrDefault(),
                Variants = p.Variants.Where(v => v.IsActive).Select(v => new
                {
                    v.Id,
                    v.Price,
                    v.DiscountPrice,
                    v.Stock,
                    Parts = v.Values
                        .OrderBy(x => x.AttributeValue.Attribute.DisplayOrder)
                        .ThenBy(x => x.AttributeValue.DisplayOrder)
                        .Select(x => x.AttributeValue.Value).ToList()
                }).ToList()
            })
            .ToListAsync(ct);

        var byId = products.ToDictionary(p => p.Id);

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0) continue;
            if (!byId.TryGetValue(item.ProductId, out var p)) { vm.HasUnavailable = true; continue; }

            long unitList, unitSell, stock;
            int? variantId = null;
            string? variantTitle = null;

            if (p.HasVariants)
            {
                if (item.VariantId is null) { vm.HasUnavailable = true; continue; }
                var v = p.Variants.FirstOrDefault(x => x.Id == item.VariantId.Value);
                if (v is null) { vm.HasUnavailable = true; continue; }

                stock = v.Stock;
                unitList = v.Price;
                unitSell = v.DiscountPrice is > 0 && v.DiscountPrice < v.Price ? v.DiscountPrice.Value : v.Price;
                variantId = v.Id;
                variantTitle = string.Join("، ", v.Parts);
            }
            else
            {
                stock = p.Stock;
                unitList = p.Price;
                unitSell = p.DiscountPrice is > 0 && p.DiscountPrice < p.Price ? p.DiscountPrice.Value : p.Price;
            }

            if (stock <= 0) { vm.HasUnavailable = true; continue; }
            if (unitList < 0) unitList = 0;
            if (unitSell < 0) unitSell = 0;

            var qty = item.Quantity;
            if (qty > stock) { qty = (int)stock; vm.HasUnavailable = true; }

            var line = new CartLineDto
            {
                ProductId = p.Id,
                VariantId = variantId,
                Title = p.Title,
                Slug = p.Slug,
                ImageUrl = p.ImageUrl,
                VariantTitle = variantTitle,
                UnitPrice = unitSell,
                UnitBasePrice = unitList,
                Quantity = qty,
                AvailableStock = (int)stock,
                LineTotal = unitSell * qty
            };
            vm.Lines.Add(line);

            vm.TotalAmount += unitList * qty;
            vm.PayableAmount += line.LineTotal;
            vm.TotalQuantity += qty;
        }

        vm.DiscountAmount = vm.TotalAmount - vm.PayableAmount;
        return vm;
    }
}
