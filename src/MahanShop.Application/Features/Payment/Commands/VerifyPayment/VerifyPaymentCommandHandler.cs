using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Payment.Commands.VerifyPayment;

/// <summary>تایید پرداخت: یافتن Payment با Authority، تایید مبلغ از درگاه، علامت Success/Failed، سفارش → Paid + کاهش موجودی (idempotent).</summary>
public class VerifyPaymentCommandHandler : IRequestHandler<VerifyPaymentCommand, VerifyPaymentResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IPaymentGateway _gateway;

    public VerifyPaymentCommandHandler(IApplicationDbContext db, IPaymentGateway gateway)
    {
        _db = db;
        _gateway = gateway;
    }

    public async Task<VerifyPaymentResult> Handle(VerifyPaymentCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Authority))
            return new VerifyPaymentResult(false, null, null, "اطلاعات پرداخت ناقص است.");

        var payment = await _db.Payments
            .Include(p => p.Order)
                .ThenInclude(o => o.Items)
            .FirstOrDefaultAsync(p => p.Authority == request.Authority, ct);

        if (payment is null)
            return new VerifyPaymentResult(false, null, null, "تراکنش یافت نشد.");

        var order = payment.Order;

        // idempotent: این تراکنش قبلاً موفق شده — همان نتیجه بدون کاهش مجدد موجودی
        if (payment.Status == PaymentStatus.Success)
            return new VerifyPaymentResult(true, order.OrderCode, payment.RefId, null);

        // سفارش قبلاً (با تراکنش دیگری) پرداخت شده — این Authority تکراری را علامت Failed بزن، موجودی دوباره کم نشود
        if (order.Status == OrderStatus.Paid)
        {
            payment.Status = PaymentStatus.Failed;
            await _db.SaveChangesAsync(ct);
            return new VerifyPaymentResult(true, order.OrderCode, order.TrackingCode, null);
        }

        // کاربر پرداخت را لغو کرد (Status != OK) — سفارش Pending می‌ماند برای تلاش مجدد
        if (!string.Equals(request.Status, "OK", StringComparison.OrdinalIgnoreCase))
        {
            payment.Status = PaymentStatus.Failed;
            if (order.Status == OrderStatus.AwaitingPayment)
                order.Status = OrderStatus.Pending;
            await _db.SaveChangesAsync(ct);
            return new VerifyPaymentResult(false, order.OrderCode, null, "پرداخت توسط کاربر لغو شد.");
        }

        var verify = await _gateway.VerifyAsync(payment.Amount, request.Authority, ct);

        if (!verify.Success)
        {
            payment.Status = PaymentStatus.Failed;
            if (order.Status == OrderStatus.AwaitingPayment)
                order.Status = OrderStatus.Pending;
            await _db.SaveChangesAsync(ct);
            return new VerifyPaymentResult(false, order.OrderCode, null, verify.Error ?? "تایید پرداخت ناموفق بود.");
        }

        // موفق
        payment.Status = PaymentStatus.Success;
        payment.RefId = verify.RefId;
        order.Status = OrderStatus.Paid;
        order.PaidAt = DateTime.UtcNow;
        order.TrackingCode = verify.RefId;

        await DecrementStockAsync(order.Items, ct);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // verify همزمان دیگری سفارش را نهایی کرد — این کسر موجودی نباید اعمال شود.
            // RowVersion ناسازگار = commit رد شد؛ نتیجه موفق همان تراکنش برنده را برگردان.
            return new VerifyPaymentResult(true, order.OrderCode, verify.RefId, null);
        }

        return new VerifyPaymentResult(true, order.OrderCode, verify.RefId, null);
    }

    /// <summary>کاهش موجودی پس از پرداخت موفق. variant → ProductVariant.Stock (با RowVersion، idempotent) + کاهش rollup روی Product.Stock؛ محصول ساده → Product.Stock.</summary>
    private async Task DecrementStockAsync(ICollection<Domain.Entities.OrderItem> items, CancellationToken ct)
    {
        var productIds = items.Select(i => i.ProductId).Distinct().ToList();
        var variantIds = items.Where(i => i.VariantId is not null).Select(i => i.VariantId!.Value).Distinct().ToList();

        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(ct);
        var variants = await _db.ProductVariants
            .Where(v => variantIds.Contains(v.Id))
            .ToListAsync(ct);

        foreach (var item in items)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId);

            if (item.VariantId is not null)
            {
                var v = variants.FirstOrDefault(x => x.Id == item.VariantId.Value);
                if (v is not null)
                    v.Stock = Math.Max(0, v.Stock - item.Quantity);
                if (product is not null)  // rollup: Product.Stock = Σ variant.Stock
                    product.Stock = Math.Max(0, product.Stock - item.Quantity);
                continue;
            }

            if (product is not null)
                product.Stock = Math.Max(0, product.Stock - item.Quantity);
        }
    }
}
