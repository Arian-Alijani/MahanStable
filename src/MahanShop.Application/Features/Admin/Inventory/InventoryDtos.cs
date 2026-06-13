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

/// <summary>یک سطر واریانت در جدول مرکزی «مدیریت موجودی».</summary>
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

/// <summary>گزینهٔ برند برای دراپ‌داون فیلتر (از مقادیر ویژگیِ نوع برند).</summary>
public class InventoryBrandOptionDto
{
    public int ValueId { get; set; }
    public string Name { get; set; } = "";
}

/// <summary>کل دادهٔ صفحهٔ مرکزی مدیریت موجودی (با صفحه‌بندی + شمارش‌ها).</summary>
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
