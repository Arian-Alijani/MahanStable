namespace MahanShop.Domain.Enums;

/// <summary>منبع محصولات یک نوار ProductRow در صفحه اصلی.</summary>
public enum HomeProductSource
{
    Featured = 0,     // IsFeatured
    BestSelling = 1,  // جمع OrderItem.Quantity desc — fallback ViewCount
    Newest = 2,       // CreatedAt desc
    Discounted = 3,   // DiscountPrice != null && < Price
    ByCategory = 4    // بر اساس CategoryId نوار
}
