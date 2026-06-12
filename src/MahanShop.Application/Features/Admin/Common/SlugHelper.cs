using System.Text.RegularExpressions;

namespace MahanShop.Application.Features.Admin.Common;

/// <summary>تولید slug از متن فارسی/انگلیسی (فاصله→خط‌فاصله، حذف کاراکترهای غیرمجاز).</summary>
public static class SlugHelper
{
    public static string Make(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var s = input.Trim().ToLowerInvariant();
        s = s.Replace('ي', 'ی').Replace('ك', 'ک');
        s = Regex.Replace(s, @"[\s_]+", "-");
        // حذف هر چیزی جز حرف/رقم (شامل فارسی)/خط‌فاصله
        s = Regex.Replace(s, @"[^\p{L}\p{Nd}-]", "");
        s = Regex.Replace(s, @"-+", "-").Trim('-');
        return s;
    }
}
