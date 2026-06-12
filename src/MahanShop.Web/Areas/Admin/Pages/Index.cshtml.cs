using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages;

/// <summary>داشبورد ادمین (مرحله ۱): فقط placeholder آمار. دسترسی از policy AdminOnly روی کل Area.</summary>
public class IndexModel : PageModel
{
    public void OnGet() { }
}
