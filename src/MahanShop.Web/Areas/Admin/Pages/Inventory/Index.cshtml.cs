using System.Text;
using FluentValidation;
using MahanShop.Application.Features.Admin.Inventory;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Inventory;

/// <summary>
/// صفحهٔ مدیریت موجودی (F7) — نمای product-group:
///   • محصولات ساده: ویرایش قیمت/تخفیف/موجودی inline (AJAX)
///   • محصولات واریانتی: expand ردیف → جدول واریانت‌ها با ویرایش inline هر زیرشاخه
///   • CSV export/import برای واریانت‌ها (حفظ‌شده از قبل)
/// </summary>
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    public IndexModel(IMediator mediator) => _mediator = mediator;

    public InventoryProductsDto Data { get; private set; } = new();

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int? BrandId { get; set; }
    [BindProperty(SupportsGet = true)] public StockStatusFilter Status { get; set; } = StockStatusFilter.All;
    [BindProperty(SupportsGet = true)] public InventoryProductTypeFilter ProductType { get; set; } = InventoryProductTypeFilter.All;
    [BindProperty(SupportsGet = true)] public int Page { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public string Sort { get; set; } = "title";
    [BindProperty(SupportsGet = true)] public bool Desc { get; set; }

    private const int PageSize = 40;

    public async Task OnGetAsync() => await LoadAsync();

    // ── ویرایش قیمت/تخفیف محصول ساده (AJAX) ───────────────────────────────
    public async Task<IActionResult> OnPostSetSimplePriceAsync(int productId, long price, long? discountPrice)
    {
        try
        {
            await _mediator.Send(new SetSimpleProductPriceCommand(productId, price, discountPrice <= 0 ? null : discountPrice));
            return new JsonResult(new { ok = true });
        }
        catch (ValidationException ex)
        {
            return new JsonResult(new { ok = false, error = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "خطا." });
        }
    }

    // ── ویرایش موجودی محصول ساده (AJAX) ─────────────────────────────────────
    public async Task<IActionResult> OnPostSetSimpleStockAsync(int productId, int stock)
    {
        try
        {
            var newStock = await _mediator.Send(new SetSimpleProductStockCommand(productId, stock));
            return new JsonResult(new { ok = true, stock = newStock });
        }
        catch (ValidationException ex)
        {
            return new JsonResult(new { ok = false, error = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "خطا." });
        }
    }

    // ── ویرایش قیمت+تخفیف+موجودی یک واریانت (AJAX) ─────────────────────────
    public async Task<IActionResult> OnPostSetVariantAsync(int variantId, long price, long? discountPrice, int stock)
    {
        try
        {
            await _mediator.Send(new SetVariantPriceAndStockCommand(
                variantId, price,
                discountPrice is long dp && dp > 0 ? dp : null,
                stock));
            return new JsonResult(new { ok = true });
        }
        catch (ValidationException ex)
        {
            return new JsonResult(new { ok = false, error = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "خطا." });
        }
    }

    // ── ویرایش سریع موجودی واریانت +/- (AJAX) ───────────────────────────────
    public async Task<IActionResult> OnPostAdjustVariantStockAsync(int variantId, int delta)
    {
        try
        {
            var newStock = await _mediator.Send(new AdjustVariantStockCommand(variantId, delta));
            return new JsonResult(new { ok = true, stock = newStock });
        }
        catch (ValidationException ex)
        {
            return new JsonResult(new { ok = false, error = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "خطا." });
        }
    }

    // ── ویرایش مستقیم موجودی یک واریانت (AJAX) ──────────────────────────────
    public async Task<IActionResult> OnPostSetVariantStockAsync(int variantId, int stock)
    {
        try
        {
            var newStock = await _mediator.Send(new SetVariantStockCommand(variantId, stock));
            return new JsonResult(new { ok = true, stock = newStock });
        }
        catch (ValidationException ex)
        {
            return new JsonResult(new { ok = false, error = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "خطا." });
        }
    }

    // ── خروجی CSV از واریانت‌ها ─────────────────────────────────────────────
    public async Task<IActionResult> OnGetExportAsync()
    {
        // فیلتر CSV بر اساس BrandValueId از VariantAttributeValues (نمای قدیمی)
        var bytes = await _mediator.Send(new ExportInventoryCsvQuery(Search, null, null, Status));
        return File(bytes, "text/csv", $"inventory_{DateTime.Now:yyyy-MM-dd}.csv");
    }

    // ── بارگذاری CSV ────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostImportAsync(IFormFile? csvFile)
    {
        if (csvFile is null || csvFile.Length == 0)
        {
            TempData["AdminErr"] = "فایلی انتخاب نشده است.";
            return RedirectToPage();
        }
        if (csvFile.Length > 5 * 1024 * 1024)
        {
            TempData["AdminErr"] = "حجم فایل بیش از حد مجاز (۵ مگابایت) است.";
            return RedirectToPage();
        }

        string content;
        using (var reader = new StreamReader(csvFile.OpenReadStream(), Encoding.UTF8))
            content = await reader.ReadToEndAsync();

        var result = await _mediator.Send(new ImportInventoryCsvCommand(content));
        var msg = $"به‌روزرسانی: {result.Updated} | یافت‌نشده (SKU): {result.NotFound} | نامعتبر: {result.Invalid}";
        if (result.Errors.Count > 0) msg += " — " + string.Join("؛ ", result.Errors.Take(3));
        TempData[result.Updated > 0 ? "AdminOk" : "AdminErr"] = msg;
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        Data = await _mediator.Send(new GetInventoryProductsQuery(
            Search, BrandId, Status, ProductType, Page, PageSize, Sort, Desc));
    }
}
