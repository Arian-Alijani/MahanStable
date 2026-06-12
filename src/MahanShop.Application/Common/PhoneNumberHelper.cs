using System.Text;

namespace MahanShop.Application.Common;

/// <summary>نرمال‌سازی شماره موبایل ایران به فرم استاندارد 09xxxxxxxxx.</summary>
public static class PhoneNumberHelper
{
    /// <summary>ارقام فارسی/عربی→لاتین، حذف فاصله/خط‌تیره، تبدیل +98/98/۰۰98 به 0. خروجی null اگر معتبر نباشد.</summary>
    public static string? Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        var sb = new StringBuilder(input.Length);
        foreach (var ch in input.Trim())
        {
            var c = ch switch
            {
                >= '۰' and <= '۹' => (char)('0' + (ch - '۰')),  // فارسی
                >= '٠' and <= '٩' => (char)('0' + (ch - '٠')),  // عربی
                _ => ch
            };
            if (c is >= '0' and <= '9') sb.Append(c);
            // بقیه (فاصله، -، (، )، +) دور ریخته می‌شود
        }

        var d = sb.ToString();

        // 0098xxxxxxxxxx → 98xxxxxxxxxx
        if (d.StartsWith("0098")) d = d[2..];
        // 98xxxxxxxxxx (12 رقم) → 0xxxxxxxxxx
        if (d.Length == 12 && d.StartsWith("98")) d = "0" + d[2..];
        // 9xxxxxxxxx (10 رقم بدون صفر) → 09xxxxxxxxx
        if (d.Length == 10 && d.StartsWith("9")) d = "0" + d;

        // اعتبارسنجی نهایی: دقیقا 09 + 9 رقم
        if (d.Length == 11 && d[0] == '0' && d[1] == '9')
            return d;

        return null;
    }
}
