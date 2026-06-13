using MahanShop.Domain.Enums;

namespace MahanShop.Application.Features.Admin.Inventory;

/// <summary>وضعیت موجودی برای فیلتر صفحهٔ مدیریت موجودی.</summary>
public enum StockStatusFilter
{
    All = 0,
    InStock = 1,    // > 5
    Low = 2,        // 1..5
    Out = 3         // 0
}

/// <summary>نوع محصول برای فیلتر صفحهٔ مدیریت موجودی.</summary>
public enum InventoryProductTypeFilter
{
    All = 0,
    Simple = 1,     // محصول ساده (HasVariants=false)
    Variant = 2     // محصول واریانتی (HasVariants=true)
}

// ─── DTOs مربوط به نمای product-group (F7) ───────────────────────────────────

/// <summary>یک واریانت در ردیف گسترش‌یافتهٔ محصول واریانتی — کنترل تفکیکی قیمت/تعداد.</summary>
public class InventoryVariantRowDto
{
    public int VariantId { get; set; }
    public string? Sku { get; set; }
    public long Price { get; set; }
    public long? DiscountPrice { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }

    /// <summary>عنوان خوانا از ترکیب مقادیر (مثلاً «سامسونگ / A10 / آبی»).</summary>
    public string Title { get; set; } = "";
}

/// <summary>یک ردیف محصول در جدول مدیریت موجودی (شامل واریانت‌ها اگر واریانتی باشد).</summary>
public class InventoryProductRowDto
{
    public int ProductId { get; set; }
    public string ProductTitle { get; set; } = "";
    public string? CategoryName { get; set; }
    public string? BrandName { get; set; }
    public bool HasVariants { get; set; }

    // --- فیلدهای محصول ساده ---
    public long Price { get; set; }
    public long? DiscountPrice { get; set; }
    public int Stock { get; set; }

    // --- واریانت‌های محصول واریانتی ---
    public List<InventoryVariantRowDto> Variants { get; set; } = new();

    /// <summary>تعداد کل واریانت‌ها (برای نمایش در برچسب).</summary>
    public int VariantCount => Variants.Count;

    /// <summary>وضعیت موجودی کلی (برای رنگ‌بندی).</summary>
    public bool IsOutOfStock => HasVariants
        ? Variants.All(v => v.Stock == 0)
        : Stock == 0;

    public bool IsLowStock => !IsOutOfStock && (HasVariants
        ? Variants.Any(v => v.Stock >= 1 && v.Stock <= 5)
        : Stock >= 1 && Stock <= 5);
}

/// <summary>گزینهٔ برند برای دراپ‌داون فیلتر.</summary>
public class InventoryBrandOptionDto
{
    public int BrandId { get; set; }
    public string Name { get; set; } = "";
}

/// <summary>کل دادهٔ صفحهٔ مدیریت موجودی (product-group view با صفحه‌بندی + شمارش‌ها).</summary>
public class InventoryProductsDto
{
    public List<InventoryProductRowDto> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    /// <summary>تعداد کل واریانت‌هایی که موجودی‌شان بین ۱ تا ۵ است (برای نوار هشدار).</summary>
    public int LowStockVariantCount { get; set; }

    /// <summary>تعداد کل محصولات ساده‌ای که موجودی‌شان کمتر از ۵ است.</summary>
    public int LowStockSimpleCount { get; set; }

    public int LowStockCount => LowStockVariantCount + LowStockSimpleCount;

    /// <summary>گزینه‌های برند برای دراپ‌داون فیلتر.</summary>
    public List<InventoryBrandOptionDto> BrandOptions { get; set; } = new();
}

// ─── DTOs قدیمی (variant-only view) — حفظ برای سازگاری با CSV ────────────────

/// <summary>یک سطر واریانت در جدول مرکزی «مدیریت موجودی» (نمای قدیمی — فقط برای CSV).</summary>
public class InventoryRowDto
{
    public int VariantId { get; set; }
    public int ProductId { get; set; }
    public string ProductTitle { get; set; } = "";
    public string? Sku { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }

    /// <summary>نام برند (نوع اول) — در صورت نبود، خالی.</summary>
    public string? Brand { get; set; }
    /// <summary>مدل (نوع اول) یا ویژگی اصلی (نوع دوم).</summary>
    public string? Model { get; set; }
    /// <summary>رنگ یا سایر ویژگی‌ها (با اسلش جدا می‌شوند).</summary>
    public string? OtherAttributes { get; set; }
}

/// <summary>کل دادهٔ صفحهٔ مرکزی مدیریت موجودی — نمای قدیمی واریانت‌محور (فقط برای CSV).</summary>
public class InventoryOverviewDto
{
    public List<InventoryRowDto> Rows { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    /// <summary>تعداد کل واریانت‌هایی که موجودی‌شان کمتر از ۵ است (برای نوار هشدار).</summary>
    public int LowStockCount { get; set; }

    /// <summary>گزینه‌های برند برای دراپ‌داون فیلتر.</summary>
    public List<InventoryBrandOptionDto> BrandOptions { get; set; } = new();
}
