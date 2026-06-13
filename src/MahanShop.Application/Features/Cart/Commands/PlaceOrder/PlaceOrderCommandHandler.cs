using System.Security.Cryptography;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Entities;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Cart.Commands.PlaceOrder;

/// <summary>اعتبارسنجی سبد مقابل DB (قیمت/موجودی)، تایید مالکیت آدرس، ساخت Order(Pending)+OrderItems با snapshot قیمت. موجودی اینجا کم نمی‌شود — بعد از پرداخت موفق (P7) کم می‌شود.</summary>
public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, PlaceOrderResult>
{
    private readonly IApplicationDbContext _db;

    public PlaceOrderCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<PlaceOrderResult> Handle(PlaceOrderCommand request, CancellationToken ct)
    {
        if (request.Items.Count == 0)
            return Fail("سبد خرید خالی است.");

        // آدرس باید متعلق به همین کاربر باشد (IDOR guard)
        var addressOwned = await _db.Addresses
            .AsNoTracking()
            .AnyAsync(a => a.Id == request.AddressId && a.UserId == request.UserId, ct);
        if (!addressOwned)
            return Fail("آدرس انتخابی معتبر نیست.");

        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .Include(p => p.Variants).ThenInclude(v => v.Values).ThenInclude(vv => vv.AttributeValue).ThenInclude(av => av.Attribute)
            .ToListAsync(ct);
        var byId = products.ToDictionary(p => p.Id);

        var orderItems = new List<OrderItem>();
        long total = 0, payable = 0;

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0) continue;
            if (!byId.TryGetValue(item.ProductId, out var p))
                return Fail("یکی از کالاهای سبد دیگر موجود نیست. سبد را بازبینی کنید.");

            long listPrice, sellPrice, stock;
            ProductVariant? variant = null;
            string? variantTitle = null;

            if (p.HasVariants)
            {
                if (item.VariantId is null)
                    return Fail("گزینه یکی از کالاها انتخاب نشده است. سبد را بازبینی کنید.");
                variant = p.Variants.FirstOrDefault(v => v.Id == item.VariantId.Value && v.IsActive);
                if (variant is null)
                    return Fail("گزینه انتخابی یکی از کالاها معتبر نیست.");

                stock = variant.Stock;
                listPrice = variant.Price;
                sellPrice = variant.DiscountPrice is > 0 && variant.DiscountPrice < variant.Price
                    ? variant.DiscountPrice.Value : variant.Price;
                variantTitle = BuildVariantTitle(variant);
            }
            else
            {
                stock = p.Stock;
                listPrice = p.Price;
                sellPrice = p.DiscountPrice is > 0 && p.DiscountPrice < p.Price
                    ? p.DiscountPrice.Value : p.Price;
            }

            if (stock < item.Quantity)
                return Fail("موجودی یکی از کالاها کافی نیست. سبد را بازبینی کنید.");

            total += listPrice * item.Quantity;
            payable += sellPrice * item.Quantity;

            orderItems.Add(new OrderItem
            {
                ProductId = p.Id,
                VariantId = variant?.Id,
                ProductTitle = p.Title,
                VariantTitle = variantTitle,
                UnitPrice = sellPrice,
                Quantity = item.Quantity
            });
        }

        if (orderItems.Count == 0)
            return Fail("سبد خرید خالی است.");

        // بارگیری و اعتبارسنجی روش ارسال از DB — نرخ از client قبول نمی‌شود (امن)
        var shippingMethod = await _db.ShippingMethods
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.ShippingMethodId && s.IsActive, ct);
        if (shippingMethod is null)
            return Fail("روش ارسال انتخابی معتبر نیست. لطفاً یک روش ارسال فعال انتخاب کنید.");

        long shippingCost = shippingMethod.Cost;
        long finalAmount = payable + shippingCost;

        var order = new Order
        {
            OrderCode = GenerateOrderCode(),
            UserId = request.UserId,
            AddressId = request.AddressId,
            TotalAmount = total,
            DiscountAmount = total - payable,
            ShippingMethodId = shippingMethod.Id,
            ShippingMethodName = shippingMethod.Name,  // snapshot نام (تغییرناپذیر بعد ثبت)
            ShippingCost = shippingCost,               // snapshot نرخ از DB (نه از client)
            FinalAmount = finalAmount,
            Status = OrderStatus.Pending,
            Items = orderItems
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        return new PlaceOrderResult(true, null, order.Id, order.OrderCode);
    }

    private static PlaceOrderResult Fail(string msg) => new(false, msg, 0, null);

    /// <summary>عنوان snapshot variant: مقادیر ویژگی به ترتیب نمایش، جدا با «، ».</summary>
    public static string BuildVariantTitle(ProductVariant v) =>
        string.Join("، ", v.Values
            .OrderBy(x => x.AttributeValue.Attribute.DisplayOrder)
            .ThenBy(x => x.AttributeValue.DisplayOrder)
            .Select(x => x.AttributeValue.Value));

    /// <summary>کد سفارش یکتا و غیرقابل‌حدس: تاریخ + بخش تصادفی امن.</summary>
    private static string GenerateOrderCode()
    {
        var rnd = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return $"MS-{DateTime.UtcNow:yyMMddHHmmss}-{rnd:D6}";
    }
}
