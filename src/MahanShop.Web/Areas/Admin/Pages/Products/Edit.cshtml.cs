using FluentValidation;
using MahanShop.Application.Features.Admin.Brands;
using MahanShop.Application.Features.Admin.Categories;
using MahanShop.Application.Features.Admin.Features;
using MahanShop.Application.Features.Admin.Products;
using MahanShop.Application.Features.Admin.ProductVariants;
using MahanShop.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Products;

/// <summary>ویرایش محصول: اطلاعات پایه + قیمت/موجودی + مشخصات‌فنی + گزینه‌ها + گالری عکس.</summary>
public class EditModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ImageUploadService _upload;
    public EditModel(IMediator mediator, ImageUploadService upload)
    {
        _mediator = mediator; _upload = upload;
    }

    [BindProperty] public int Id { get; set; }
    [BindProperty] public string Title { get; set; } = "";
    [BindProperty] public string? Slug { get; set; }
    [BindProperty] public string? ShortDescription { get; set; }
    [BindProperty] public string? Description { get; set; }
    [BindProperty] public long Price { get; set; }
    [BindProperty] public long? DiscountPrice { get; set; }
    [BindProperty] public int Stock { get; set; }
    [BindProperty] public bool HasVariants { get; set; }
    [BindProperty] public bool IsActive { get; set; }
    [BindProperty] public bool IsFeatured { get; set; }
    [BindProperty] public string? MetaTitle { get; set; }
    [BindProperty] public string? MetaDescription { get; set; }
    [BindProperty] public int BrandId { get; set; }
    [BindProperty] public int CategoryId { get; set; }

    // آپلود تصویر
    [BindProperty] public IFormFile? ImageFile { get; set; }
    [BindProperty] public string? ImageAlt { get; set; }

    // افزودن مشخصهٔ فنی
    [BindProperty] public int NewFeatureId { get; set; }
    [BindProperty] public string? NewFeatureValue { get; set; }

    public List<ProductImageDto> Images { get; private set; } = new();
    public List<BrandListItemDto> Brands { get; private set; } = new();
    public List<CategoryOptionDto> Categories { get; private set; } = new();
    public ProductVariantsViewDto? VariantsView { get; private set; }
    /// <summary>مشخصات فنی ثبت‌شده برای این محصول.</summary>
    public List<ProductFeatureItemDto> ProductFeatures { get; private set; } = new();
    /// <summary>همهٔ Feature موجود برای dropdown افزودن مشخصه.</summary>
    public List<FeatureListItemDto> AllFeatures { get; private set; } = new();

    // فرم گزینه/موجودی
    [BindProperty] public int VariantId { get; set; }
    [BindProperty] public string? VariantSku { get; set; }
    [BindProperty] public long VariantPrice { get; set; }
    [BindProperty] public long? VariantDiscountPrice { get; set; }
    [BindProperty] public int VariantStock { get; set; }
    [BindProperty] public bool VariantIsActive { get; set; } = true;
    [BindProperty] public int VariantDisplayOrder { get; set; }
    [BindProperty] public List<int> VariantValueIds { get; set; } = new();
    [BindProperty] public int QuickStock { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var p = await _mediator.Send(new GetProductForEditQuery(id));
        if (p is null) return NotFound();
        MapFrom(p);
        await LoadOptionsAsync();
        await LoadVariantsAsync();
        return Page();
    }

    // ذخیرهٔ اطلاعات پایه / قیمت / وضعیت
    public async Task<IActionResult> OnPostSaveAsync()
    {
        try
        {
            await _mediator.Send(new UpdateProductCommand(
                Id, Title, Slug, ShortDescription, Description, Price, DiscountPrice, Stock,
                HasVariants, IsActive, IsFeatured, MetaTitle, MetaDescription, BrandId, CategoryId));
            TempData["AdminOk"] = "محصول به‌روزرسانی شد.";
            return RedirectToPage(new { id = Id });
        }
        catch (ValidationException ex)
        {
            foreach (var e in ex.Errors) ModelState.AddModelError(string.Empty, e.ErrorMessage);
            await ReloadAsync();
            return Page();
        }
    }

    // افزودن مشخصهٔ فنی
    public async Task<IActionResult> OnPostAddFeatureAsync()
    {
        try
        {
            await _mediator.Send(new AddProductFeatureCommand(Id, NewFeatureId, NewFeatureValue ?? ""));
            TempData["AdminOk"] = "مشخصه ثبت شد.";
            TempData["ReturnTab"] = "ep-features";
        }
        catch (ValidationException ex)
        {
            TempData["AdminErr"] = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "خطا.";
            TempData["ReturnTab"] = "ep-features";
        }
        return RedirectToPage(new { id = Id });
    }

    // حذف مشخصهٔ فنی
    public async Task<IActionResult> OnPostDeleteFeatureAsync(int productFeatureId)
    {
        try
        {
            await _mediator.Send(new DeleteProductFeatureCommand(productFeatureId));
            TempData["AdminOk"] = "مشخصه حذف شد.";
            TempData["ReturnTab"] = "ep-features";
        }
        catch (ValidationException ex)
        {
            TempData["AdminErr"] = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "خطا.";
            TempData["ReturnTab"] = "ep-features";
        }
        return RedirectToPage(new { id = Id });
    }

    // افزودن عکس به گالری
    public async Task<IActionResult> OnPostAddImageAsync()
    {
        var r = await _upload.UploadAsync(ImageFile, "Photo");
        if (!r.Success)
        {
            TempData["AdminErr"] = r.Error;
            TempData["ReturnTab"] = "ep-images";
            return RedirectToPage(new { id = Id });
        }
        await _mediator.Send(new AddProductImageCommand(Id, r.WebPath!, ImageAlt));
        TempData["AdminOk"] = "تصویر اضافه شد.";
        TempData["ReturnTab"] = "ep-images";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostDeleteImageAsync(int imageId)
    {
        await _mediator.Send(new DeleteProductImageCommand(imageId));
        TempData["AdminOk"] = "تصویر حذف شد.";
        TempData["ReturnTab"] = "ep-images";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostSetMainImageAsync(int imageId)
    {
        await _mediator.Send(new SetMainProductImageCommand(imageId));
        TempData["AdminOk"] = "تصویر اصلی تعیین شد.";
        TempData["ReturnTab"] = "ep-images";
        return RedirectToPage(new { id = Id });
    }

    // افزودن گزینه فروش جدید
    public async Task<IActionResult> OnPostAddVariantAsync()
    {
        try
        {
            var valueIds = VariantValueIds.Where(x => x > 0).ToList();
            await _mediator.Send(new CreateProductVariantCommand(
                Id, VariantSku, VariantPrice, VariantDiscountPrice, VariantStock,
                VariantIsActive, VariantDisplayOrder, valueIds));
            TempData["AdminOk"] = "گزینه اضافه شد.";
        }
        catch (ValidationException ex)
        {
            TempData["AdminErr"] = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "خطا.";
        }
        TempData["ReturnTab"] = "ep-variants";
        return RedirectToPage(new { id = Id });
    }

    // ویرایش گزینه فروش
    public async Task<IActionResult> OnPostUpdateVariantAsync()
    {
        try
        {
            await _mediator.Send(new UpdateProductVariantCommand(
                VariantId, VariantSku, VariantPrice, VariantDiscountPrice, VariantStock,
                VariantIsActive, VariantDisplayOrder, VariantValueIds));
            TempData["AdminOk"] = "گزینه به‌روزرسانی شد.";
        }
        catch (ValidationException ex)
        {
            TempData["AdminErr"] = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "خطا.";
        }
        TempData["ReturnTab"] = "ep-variants";
        return RedirectToPage(new { id = Id });
    }

    // ویرایش سریع موجودی
    public async Task<IActionResult> OnPostQuickStockAsync(int variantId)
    {
        try
        {
            await _mediator.Send(new UpdateVariantStockCommand(variantId, QuickStock));
            TempData["AdminOk"] = "موجودی به‌روزرسانی شد.";
        }
        catch (ValidationException ex)
        {
            TempData["AdminErr"] = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "خطا.";
        }
        TempData["ReturnTab"] = "ep-variants";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostToggleVariantAsync(int variantId)
    {
        await _mediator.Send(new ToggleVariantActiveCommand(variantId));
        TempData["AdminOk"] = "وضعیت گزینه تغییر کرد.";
        TempData["ReturnTab"] = "ep-variants";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostDeleteVariantAsync(int variantId)
    {
        await _mediator.Send(new DeleteProductVariantCommand(variantId));
        TempData["AdminOk"] = "گزینه حذف شد.";
        TempData["ReturnTab"] = "ep-variants";
        return RedirectToPage(new { id = Id });
    }

    // خروجی CSV واریانت‌های محصول
    public async Task<IActionResult> OnGetExportVariantsAsync(int id)
    {
        var bytes = await _mediator.Send(new ExportProductVariantsCsvQuery(id));
        return File(bytes, "text/csv", $"variants_p{id}_{DateTime.Now:yyyy-MM-dd}.csv");
    }

    // بارگذاری CSV واریانت‌های محصول
    public async Task<IActionResult> OnPostImportVariantsAsync(IFormFile? csvFile)
    {
        if (csvFile is null || csvFile.Length == 0)
        {
            TempData["AdminErr"] = "فایلی انتخاب نشده است.";
            TempData["ReturnTab"] = "ep-variants";
            return RedirectToPage(new { id = Id });
        }
        if (csvFile.Length > 5 * 1024 * 1024)
        {
            TempData["AdminErr"] = "حجم فایل بیش از حد مجاز (۵ مگابایت) است.";
            TempData["ReturnTab"] = "ep-variants";
            return RedirectToPage(new { id = Id });
        }

        string content;
        using (var reader = new System.IO.StreamReader(csvFile.OpenReadStream(), System.Text.Encoding.UTF8))
            content = await reader.ReadToEndAsync();

        var result = await _mediator.Send(new ImportProductVariantsCsvCommand(Id, content));
        var msg = $"ساخته‌شده: {result.Created} | به‌روزرسانی: {result.Updated} | رد‌شده: {result.Skipped}";
        if (result.Errors.Count > 0) msg += " — " + string.Join("؛ ", result.Errors.Take(3));
        TempData[(result.Created + result.Updated) > 0 ? "AdminOk" : "AdminErr"] = msg;
        TempData["ReturnTab"] = "ep-variants";
        return RedirectToPage(new { id = Id });
    }

    private async Task LoadVariantsAsync() =>
        VariantsView = await _mediator.Send(new GetProductVariantsQuery(Id));

    private void MapFrom(ProductEditDto p)
    {
        Id = p.Id; Title = p.Title; Slug = p.Slug; ShortDescription = p.ShortDescription;
        Description = p.Description; Price = p.Price; DiscountPrice = p.DiscountPrice; Stock = p.Stock;
        HasVariants = p.HasVariants; IsActive = p.IsActive; IsFeatured = p.IsFeatured;
        MetaTitle = p.MetaTitle; MetaDescription = p.MetaDescription;
        BrandId = p.BrandId; CategoryId = p.CategoryId;
        Images = p.Images;
        ProductFeatures = p.Features;
    }

    private async Task ReloadAsync()
    {
        var p = await _mediator.Send(new GetProductForEditQuery(Id));
        if (p is not null) { Images = p.Images; ProductFeatures = p.Features; }
        await LoadOptionsAsync();
        await LoadVariantsAsync();
    }

    private async Task LoadOptionsAsync()
    {
        Brands = await _mediator.Send(new GetBrandsQuery());
        Categories = await _mediator.Send(new GetCategoryOptionsQuery());
        AllFeatures = await _mediator.Send(new GetFeaturesQuery());
    }
}
