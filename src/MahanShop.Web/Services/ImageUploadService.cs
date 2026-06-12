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
        if (!await HasValidImageSignatureAsync(file, ct))
            return new(false, null, "محتوای فایل، عکس معتبر نیست.");

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

    /// <summary>بررسی magic bytes — جلوگیری از آپلود فایل غیرعکس با پسوند/Content-Type جعلی.</summary>
    private static async Task<bool> HasValidImageSignatureAsync(IFormFile file, CancellationToken ct)
    {
        var header = new byte[12];
        await using var s = file.OpenReadStream();
        var read = await s.ReadAsync(header, ct);
        if (read < 4) return false;
        // JPEG: FF D8 FF
        if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) return true;
        // PNG: 89 50 4E 47
        if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47) return true;
        // GIF: "GIF8"
        if (header[0] == (byte)'G' && header[1] == (byte)'I' && header[2] == (byte)'F' && header[3] == (byte)'8') return true;
        // WEBP: "RIFF" .... "WEBP"
        if (read >= 12 && header[0] == (byte)'R' && header[1] == (byte)'I' && header[2] == (byte)'F' && header[3] == (byte)'F'
            && header[8] == (byte)'W' && header[9] == (byte)'E' && header[10] == (byte)'B' && header[11] == (byte)'P') return true;
        return false;
    }
}
