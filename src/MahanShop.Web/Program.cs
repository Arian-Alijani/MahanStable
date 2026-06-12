using System.Security.Claims;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Infra.IoC;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC + Razor Pages (server-render) — anti-forgery گلوبال روی همه POST
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Panel");
    // === ADMIN-PANEL START (Phase 9) — remove this whole block to rollback ===
    options.Conventions.AuthorizeAreaFolder("Admin", "/", "AdminOnly");
    // === ADMIN-PANEL END ===
});

// === ADMIN-PANEL START (Phase 9) — remove this whole block to rollback ===
// policy دسترسی فقط ادمین (claim نقش "Admin" که در VerifyLoginCode/Account ست می‌شود)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});
// سرویس آپلود عکس ادمین (whitelist پسوند/نوع/حجم → wwwroot/AdminPanel)
builder.Services.AddScoped<MahanShop.Web.Services.ImageUploadService>();
// === ADMIN-PANEL END ===

// فاکتور PDF (QuestPDF) — سرویس Web
builder.Services.AddSingleton<MahanShop.Web.Services.InvoicePdfService>();

// HSTS — یک سال، شامل ساب‌دامین‌ها
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
});

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// سبد خرید session-based
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<MahanShop.Web.Services.CartStore>();

// لایه‌ها (DbContext, IApplicationDbContext, ...)
builder.Services.RegisterServices(builder.Configuration);

// Cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/"; // عدم افشای مسیر ادمین برای کاربر غیرمجاز
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(10);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        // === ADMIN-PANEL START (Phase 9) — remove this whole block to rollback ===
        // revoke فوری نقش Admin: اگر IsAdmin/IsActive در DB برداشته شد، کوکی ۱۰روزه دیگر معتبر نیست
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = async ctx =>
            {
                if (ctx.Principal?.IsInRole("Admin") != true) return; // فقط ادمین‌ها re-check می‌شوند
                var idStr = ctx.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
                var db = ctx.HttpContext.RequestServices.GetRequiredService<IApplicationDbContext>();
                var stillAdmin = int.TryParse(idStr, out var id)
                    && await db.Users.AnyAsync(u => u.Id == id && u.IsAdmin && u.IsActive);
                if (!stillAdmin)
                {
                    ctx.RejectPrincipal();
                    await ctx.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
            }
        };
        // === ADMIN-PANEL END ===
    });

var app = builder.Build();

// Seed داده‌ی نمونه فقط در Development (migrate + کاتالوگ + محتوای صفحه اصلی)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MahanShop.Infra.Data.Context.MyDbContext>();
    await MahanShop.Infra.Data.Seed.DataSeeder.SeedAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// هدرهای امنیتی پایه (ضد clickjacking / MIME-sniffing / نشت referrer)
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.UseStaticFiles();

app.UseRouting();

// Session باید قبل از auth و endpointها باشد (باگ نمونه قدیم: بعد از map صدا می‌زد)
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// مرور محصولات (Phase 4A) — URLهای تمیز SEO
app.MapControllerRoute(
    name: "catalog",
    pattern: "products",
    defaults: new { controller = "Catalog", action = "Index" });
app.MapControllerRoute(
    name: "search",
    pattern: "search",
    defaults: new { controller = "Catalog", action = "Search" });
app.MapControllerRoute(
    name: "category",
    pattern: "category/{slug}",
    defaults: new { controller = "Catalog", action = "Category" });
app.MapControllerRoute(
    name: "product",
    pattern: "product/{slug}",
    defaults: new { controller = "Catalog", action = "Detail" });

// سبد و تسویه (Phase 6)
app.MapControllerRoute(
    name: "cart",
    pattern: "cart",
    defaults: new { controller = "Cart", action = "Index" });
app.MapControllerRoute(
    name: "checkout",
    pattern: "checkout",
    defaults: new { controller = "Checkout", action = "Index" });

// پرداخت (Phase 7)
app.MapControllerRoute(
    name: "payment-start",
    pattern: "payment/start",
    defaults: new { controller = "Payment", action = "Start" });
app.MapControllerRoute(
    name: "payment-verify",
    pattern: "payment/verify",
    defaults: new { controller = "Payment", action = "Verify" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
