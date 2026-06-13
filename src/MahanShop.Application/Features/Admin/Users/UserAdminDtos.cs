namespace MahanShop.Application.Features.Admin.Users;

/// <summary>سطر لیست کاربر در پنل ادمین.</summary>
public class AdminUserListItemDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = null!;
    public string? Email { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int OrderCount { get; set; }
}

/// <summary>آمار خلاصه کاربران برای کارت‌های بالای صفحه.</summary>
public class AdminUserStatsDto
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }
    public int Admins { get; set; }
    /// <summary>کاربرانی که حداقل یک سفارش دارند.</summary>
    public int WithOrders { get; set; }
}

/// <summary>نتیجه صفحه‌بندی‌شده لیست کاربران + آمار.</summary>
public class AdminUserListResult
{
    public List<AdminUserListItemDto> Items { get; set; } = new();
    public AdminUserStatsDto Stats { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

/// <summary>جزییات کاربر برای صفحه ادمین: پروفایل + آدرس‌ها + سفارش‌های اخیر.</summary>
public class AdminUserDetailDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = null!;
    public string? Email { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalOrderCount { get; set; }
    public List<AdminUserAddressDto> Addresses { get; set; } = new();
    public List<AdminUserOrderDto> RecentOrders { get; set; } = new();
}

public class AdminUserAddressDto
{
    public int Id { get; set; }
    public string Province { get; set; } = null!;
    public string City { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string FullAddress { get; set; } = null!;
    public string ReceiverName { get; set; } = null!;
    public string ReceiverPhone { get; set; } = null!;
}

public class AdminUserOrderDto
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public Domain.Enums.OrderStatus Status { get; set; }
    public long FinalAmount { get; set; }
}
