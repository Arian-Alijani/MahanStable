using System.Text;
using FluentValidation;
using MahanShop.Application.Features.Admin.Inventory;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Inventory;

/// <summary>صفحهٔ مرکزی «مدیریت موجودی»: جدول همهٔ واریانت‌ها + فیلتر + ویرایش سریع + دسته‌ای + CSV.</summary>
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    public IndexModel(IMediator mediator) => _mediator = mediator;

    public InventoryOverviewDto Data { get; private set; } = new();

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int? BrandValueId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Model { get; set; }
    [BindProperty(SupportsGet = true)] public StockStatusFilter Status { get; set; } = StockStatusFilter.All;
    [BindProperty(SupportsGet = true)] public int Page { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public string Sort { get; set; } = "product";
    [BindProperty(SupportsGet = true)] public bool Desc { get; set; }

    private const int PageSize = 50;

    public async Task OnGetAsync() => await LoadAsync();

    // ویرایش مستقیم موجودی (inline) — فراخوانی AJAX
    public async Task<IActionResult> OnPostSetStockAsync(int variantId, int stock)
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

    // تغییر سریع (+/-) — فراخوانی AJAX
    public async Task<IActionResult> OnPostAdjustStockAsync(int variantId, int delta)
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

    // اعمال دسته‌ای روی ردیف‌های انتخاب‌شده
    public async Task<IActionResult> OnPostBulkAsync(List<int> ids, BulkStockOperation operation, int amount)
    {
        try
        {
            var count = await _mediator.Send(new BulkUpdateStockCommand(ids ?? new(), operation, amount));
            TempData["AdminOk"] = $"موجودی {count} واریانت به‌روزرسانی شد.";
        }
        catch (ValidationException ex)
        {
            TempData["AdminErr"] = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "خطا در عملیات دسته‌ای.";
        }
        return RedirectToPage(new { Search, BrandValueId, Model, Status, Page, Sort, Desc });
    }

    // خروجی CSV از نتایج فیلترشده
    public async Task<IActionResult> OnGetExportAsync()
    {
        var bytes = await _mediator.Send(new ExportInventoryCsvQuery(Search, BrandValueId, Model, Status));
        return File(bytes, "text/csv", $"inventory_{DateTime.Now:yyyy-MM-dd}.csv");
    }

    // بارگذاری CSV برای به‌روزرسانی انبوه موجودی
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
        Data = await _mediator.Send(new GetInventoryOverviewQuery(
            Search, BrandValueId, Model, Status, Page, PageSize, Sort, Desc));
    }
}
