using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Attributes;

/// <summary>
/// hub «کنترل ویژگی» (F1): صفحهٔ یکپارچهٔ تنظیمات پایه.
/// در این فاز فقط کارت‌های لینک به زیربخش‌های موجود (دسته/برند/مدل/مشخصات‌فنی/تگ) + نوع پست (F3).
/// شمارش واقعی و CRUD نوع پست در فاز ۳ افزوده می‌شود (Boundary این فاز: فقط Areas/Admin + wwwroot/admin).
/// </summary>
public class IndexModel : PageModel
{
    public void OnGet() { }
}
