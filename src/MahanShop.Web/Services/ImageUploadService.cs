namespace MahanShop.Web.Services;

/// <summary>
/// آپلود امن عکس برای پنل ادمین → wwwroot/AdminPanel/Photo/...
/// whitelist پسوند + content-type + حد اندازه. self-host (domestic-only).
/// (بخشی از ADMIN-PANEL Phase 9 — حذف این فایل + پوشهٔ AdminPanel = rollback)
/// </summary>
public class ImageUploadService
{
    private const long MaxBytes = 3 * 1024 * 1024; // 3MB
    private static readonly HashSet<string> AllowedExt = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        { "image/jpeg", "image/png", "image/webp", "image/gif" };

    private readonly IWebHostEnvironment _env;
    public ImageUploadService(IWebHostEnvironment env) => _env = env;

    /// <summary>نتیجهٔ آپلود: موفقیت + مسیر وب یا پیام خطا.</summary>
    public record UploadResult(bool Success, string? WebPath, string? Error);

    /// <summary>آپلود یک فایل در زیرپوشهٔ مشخص (مثلاً "Photo"). مسیر وب برمی‌گرداند.</summary>
    public async Task<UploadResult> UploadAsync(IFormFile? file, string subFolder, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            return new(false, null, "فایلی انتخاب نشده است.");
        if (file.Length > MaxBytes)
            return new(false, null, "حجم فایل بیش از حد مجاز است (حداکثر ۳ مگابایت).");

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || !AllowedExt.Contains(ext))
            return new(false, null, "فرمت فایل مجاز نیست (jpg/png/webp/gif).");
        if (!AllowedContentTypes.Contains(file.ContentType))
            return new(false, null, "نوع محتوای فایل مجاز نیست.");

        var safeSub = string.Concat(subFolder.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'));
        if (string.IsNullOrEmpty(safeSub)) safeSub = "Photo";

        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var relDir = Path.Combine("AdminPanel", safeSub);
        var absDir = Path.Combine(webRoot, relDir);
        Directory.CreateDirectory(absDir);

        var fileName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var absPath = Path.Combine(absDir, fileName);

        await using (var stream = new FileStream(absPath, FileMode.Create))
            await file.CopyToAsync(stream, ct);

        var webPath = "/" + Path.Combine(relDir, fileName).Replace('\\', '/');
        return new(true, webPath, null);
    }
}
