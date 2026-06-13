namespace MahanShop.Application.Features.Admin.Shipping;

/// <summary>سطر لیست نوع پست در پنل ادمین.</summary>
public class ShippingMethodListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public long Cost { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public string? Description { get; set; }
}

/// <summary>داده نوع پست برای فرم ویرایش/ایجاد.</summary>
public class ShippingMethodEditDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public long Cost { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    public string? Description { get; set; }
}
