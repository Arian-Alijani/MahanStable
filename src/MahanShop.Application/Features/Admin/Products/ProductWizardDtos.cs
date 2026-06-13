namespace MahanShop.Application.Features.Admin.Products;

/// <summary>یک مقدارِ مدل (گوشی) زیر یک برند، برای انتخاب در ویزارد محصولِ چندبرندی.</summary>
public class WizardModelDto
{
    public int ValueId { get; set; }      // VariantAttributeValue.Id (مدل)
    public string Name { get; set; } = ""; // مثل «A10»، «iPhone 14»
}

/// <summary>یک برند + گوشی‌های زیرمجموعه‌اش (مقادیرِ مدلی که والدشان این برند است).</summary>
public class WizardBrandDto
{
    public int ValueId { get; set; }       // VariantAttributeValue.Id (برند)
    public string Name { get; set; } = ""; // مثل «سامسونگ»
    public string? LogoUrl { get; set; }
    public List<WizardModelDto> Models { get; set; } = new();
}

/// <summary>یک رنگ قابل‌انتخاب در ویزارد (اختیاری).</summary>
public class WizardColorDto
{
    public int ValueId { get; set; }
    public string Name { get; set; } = "";
    public string? ColorHex { get; set; }
}

/// <summary>کل دادهٔ موردنیاز ویزارد محصولِ چندبرندی.</summary>
public class ProductWizardDataDto
{
    /// <summary>آیا اصلاً ویژگیِ برند و مدل تعریف شده؟ (پیش‌نیاز جریان چندبرندی)</summary>
    public bool HasBrandModelAttributes { get; set; }
    public int BrandAttributeId { get; set; }
    public int ModelAttributeId { get; set; }
    public int? ColorAttributeId { get; set; }

    public List<WizardBrandDto> Brands { get; set; } = new();
    public List<WizardColorDto> Colors { get; set; } = new();

    /// <summary>مدل‌هایی که هنوز به هیچ برندی وصل نشده‌اند (راهنما برای ادمین).</summary>
    public int UnlinkedModelCount { get; set; }
}
