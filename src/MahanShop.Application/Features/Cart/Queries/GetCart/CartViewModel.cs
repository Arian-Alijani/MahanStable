namespace MahanShop.Application.Features.Cart.Queries.GetCart;

/// <summary>سبد محاسبه‌شده سمت سرور — قیمت/موجودی از DB، نه از client.</summary>
public class CartViewModel
{
    public List<CartLineDto> Lines { get; set; } = new();
    public long TotalAmount { get; set; }      // جمع قیمت پایه
    public long DiscountAmount { get; set; }    // جمع تخفیف
    public long PayableAmount { get; set; }     // قابل پرداخت (بدون هزینه ارسال)
    public int TotalQuantity { get; set; }
    public bool HasUnavailable { get; set; }    // قلمی ناموجود/کم‌موجود اصلاح شد
}

/// <summary>یک قلم سبد با اطلاعات نمایشی + قیمت snapshot لحظه‌ای از DB.</summary>
public class CartLineDto
{
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public string? VariantTitle { get; set; }   // ترکیب ویژگی نمایشی (برند، مدل، کد)

    public long UnitPrice { get; set; }        // قیمت نهایی واحد (با تخفیف)
    public long UnitBasePrice { get; set; }     // قیمت پایه واحد (بدون تخفیف)
    public int Quantity { get; set; }
    public int AvailableStock { get; set; }
    public long LineTotal { get; set; }         // UnitPrice * Quantity
}
