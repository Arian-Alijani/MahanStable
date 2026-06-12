using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Pages.Panel;

/// <summary>پایه صفحات پنل — UserId از claim (هرگز از فرم/کوئری).</summary>
public abstract class PanelPageModel : PageModel
{
    protected int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
