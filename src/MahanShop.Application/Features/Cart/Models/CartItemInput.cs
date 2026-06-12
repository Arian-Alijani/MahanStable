namespace MahanShop.Application.Features.Cart.Models;

/// <summary>قلم خام سبد از session (client). قیمت اینجا نیست — سرور همیشه از DB می‌خواند تا دستکاری نشود.</summary>
public class CartItemInput
{
    public int ProductId { get; set; }
    public int? VariantId { get; set; }   // ProductVariant.Id (اگر محصول HasVariants)
    public int Quantity { get; set; }
}
