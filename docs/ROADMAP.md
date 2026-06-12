# ROADMAP — فازبندی پیاده‌سازی

> ترتیب الزامی. هر فاز done-criteria داره — تا سبز نشده، بعدی شروع نکن. هر فاز = یک یا چند سشن. آخر هر فاز → `PROGRESS.md` آپدیت.

اصل: هر فاز **build سبز + قابل اجرا** بمونه. هیچ فاز سایت رو نشکنه.

---

## Phase 0 — Scaffold (پایه)
خروجی:
- `MahanShop.sln` + 5 پروژه (Domain, Application, Infra.Data, Infra.IoC, Web) همه net8.0.
- پکیج‌ها طبق `STACK.md`، reference بین لایه‌ها طبق `ARCHITECTURE.md`.
- `Program.cs` با ترتیب middleware درست (CONVENTIONS).
- `web.config` (in-process)، `appsettings.sample.json`، `.gitignore`.
- صفحه placeholder Home که بالا میاد.

Done: `dotnet build` سبز + `dotnet run` → صفحه خالی Home لود.

---

## Phase 1 — Domain + Data
خروجی:
- Entityها: `Products, Products_Groups, SubmenuGroups, Color, ProductsColor, Features, Products_Features, Products_Galleries, Products_Tags, Products_comment, Orders, OrderDetails, Users, CodePostal`.
- روابط EF (FK, navigation) + Fluent config.
- `MyDbContext : IApplicationDbContext` + `DbSet`ها.
- اولین Migration + (اختیاری) seed داده تست.

Done: `dotnet ef database update` رو SQL 2019 موفق + جدول‌ها ساخته شد.

---

## Phase 2 — Infra wiring + Config
خروجی:
- `DependencyContainer.RegisterServices` (IoC) — همه service/repository.
- MediatR ثبت، `IApplicationDbContext` به DI.
- خواندن config از env (ConnectionString, Sms, Zarinpal) — هیچ secret در سورس.
- baseline امنیت: HTTPS redirect, HSTS, anti-forgery global.

Done: app با env vars بالا میاد، DB وصل، DI بدون خطا resolve.

---

## Phase 3 — Layout + Home UI (شِل)
خروجی:
- `_Layout` RTL، Bootstrap 5 RTL، فونت Vazir، header/menu/footer — **همه asset local (بدون CDN، domestic-only)**.
- منوی دسته‌بندی پویا (Groups + SubmenuGroups) via ViewComponent.
- صفحه Home: بنر، محصولات منتخب، پرفروش‌ها (`BestSeller` ViewComponent).
- UI مشابه `Old UI` + ارتقاها.

Done: Home با داده واقعی DB رندر، ریسپانسیو موبایل، منو کار می‌کنه.

---

## Phase 4 — Catalog (مرور محصولات)
خروجی:
- لیست محصولات (`Home/AllProducts`, `Products`, `PageGroupsProducts`, `SubMenuProducts`).
- صفحه جزییات: گالری، رنگ‌ها، ویژگی‌ها، تگ‌ها، کامنت‌ها.
- `FilterProducts` (فیلتر دسته/قیمت/رنگ/...).
- `Search`.

Done: مرور + فیلتر + جستجو + جزییات با داده واقعی کار می‌کنن.

---

## Phase 5 — Auth (احراز هویت)
خروجی:
- ثبت‌نام (`Register`), ورود با OTP پیامکی (`Login` → `VerifyLoginCode`), `VerifyCode`.
- یکپارچه‌سازی SMS provider.
- Cookie auth (انقضا، LoginPath, Logout درست — نه typo قدیم).

Done: ثبت‌نام/ورود/خروج با OTP واقعی، session کاربر پایدار.

---

## Phase 6 — Cart + Checkout
خروجی:
- `ShopCart`: افزودن/حذف/تغییر تعداد (session-based).
- انتخاب آدرس + `CodePostal`.
- ساخت `Orders` + `OrderDetails` (وضعیت: pending).

Done: سبد → ثبت سفارش، رکورد Order در DB با وضعیت درست.

---

## Phase 7 — Payment (Zarinpal)
خروجی:
- درخواست پرداخت Zarinpal، ریدایرکت درگاه.
- callback verify (`/api/Shop/verify`)، `PaymentResult/success|failed`.
- آپدیت وضعیت سفارش + کاهش موجودی.

Done: پرداخت تست واقعی → verify → سفارش paid، حالت failed هم درست هندل.

---

## Phase 8 — User Panel
خروجی:
- پروفایل (`Index`, `Edit`).
- تغییر شماره + تایید OTP (`EditNumberUsers`, `VerifyNumber`).
- تاریخچه سفارش + دانلود فاکتور PDF (QuestPDF, فونت Vazir).

Done: کاربر پروفایل/شماره/سفارش‌ها رو می‌بینه و فاکتور PDF دانلود می‌شه.

---

## Phase 8.5 — Variant & Inventory System (سیستم ویژگی پویا + موجودی per-variant)
> طراحی کامل: `docs/VARIANTS.md` (LOCKED). تعمیم سیستم رنگ فعلی.

خروجی:
- **Domain:** `VariantAttribute`, `VariantAttributeValue`, `ProductVariant`, `ProductVariantValue` + `Product.HasVariants`. حذف `Color`/`ProductColor` (رنگ → attribute). `OrderItem.ColorId` → `VariantId?` + `VariantTitle?` snapshot.
- **EF + migration:** config + رابطه‌ها + RowVersion روی `ProductVariant`. اعمال روی LocalDB/SQL.
- **Wiring:** `GetProductDetail` (variantها + گروه attribute)، `GetCatalog` (قیمت=min، فیلتر موجودی)، `CartStore`/`GetCart`/`PlaceOrder` (VariantId)، `VerifyPayment.DecrementStock` (کسر variant).
- **Front:** انتخابگر variant در صفحه محصول + checkout؛ variant با `Stock<=0` یا غیرفعال → disabled/مخفی، قابل سفارش نیست.

Done: محصول قاب با چند مدل/کد/موجودی مستقل قابل ثبت و سفارش؛ مدل تمام‌شده انتخاب‌ناپذیر؛ قیمت سمت سرور؛ build سبز + smoke.

---

## Phase 9 — Admin Panel
> layout زیبا Bootstrap5 RTL + CSS دستی (نه Tailwind/CDN — domestic-only، بدون Node). دسترسی فقط ادمین (`IsAdmin`).

خروجی (Razor Pages، CRUD کامل):
- داشبورد (`Index`) — آمار فروش/سفارش/موجودی کم.
- **Variant/Inventory:** CRUD `VariantAttribute` + `VariantAttributeValue` (pool)؛ ویرایشگر محصول با تب «گزینه‌ها/موجودی» = گرید `ProductVariant` (انتخاب مقدار هر attribute + Sku + قیمت + تخفیف + موجودی + فعال)؛ ویرایش سریع موجودی inline؛ toggle `HasVariants`.
- Products (Add/Edit/Delete/Index) + آپلود عکس + Galleries.
- Groups/Categories, Brands, Features + Products_Features, Tags.
- Comments moderation (Index/Delete).
- Orders (Index + DownloadPdf فاکتور + تغییر وضعیت).
- Users (CRUD), Addresses.

Done: همه موجودیت‌ها (شامل variant/موجودی) از پنل قابل مدیریت، آپلود عکس کار، دسترسی فقط ادمین.

---

## Phase 10 — Security hardening
خروجی:
- Rate limiting (`AddRateLimiter`) رو login/OTP.
- NWebsec / CSP + security headers.
- audit anti-forgery رو همه POST.
- اعتبارسنجی آپلود فایل (پسوند/نوع/اندازه whitelist).
- تایید: هیچ secret در سورس، `Encrypt=True` رو DB.
- audit domestic-only: هیچ asset/سرویس خارجی (grep دامنه خارجی → صفر).

Done: چک‌لیست امنیت کامل، تست نفوذ پایه OK، سایت با قطع اینترنت بین‌الملل کامل لود.

---

## Phase 11 — Performance
خروجی:
- Response Compression + Output Caching.
- audit `AsNoTracking` رو queryهای خواندنی + async.
- index روی ستون‌های پرکوئری DB.
- bundle/minify asset، کش static.

Done: TTFB پایین، query بهینه، Lighthouse mobile قابل قبول.

---

## Phase 12 — Deploy + Go-live (Plesk)
⚠️ مرحله حساس — با دقت:
1. rotate همه creds لو‌رفته (DB, SMS, Zarinpal) — قبل هر چیز.
2. `dotnet publish -c Release`.
3. آپلود به Plesk، `web.config` in-process، .NET 8 runtime فعال.
4. env vars در Plesk ست (DEPLOYMENT.md).
5. migration رو DB هاست اجرا.
6. smoke test کامل: مرور، ثبت‌نام، سبد، پرداخت واقعی، پنل، فاکتور.
7. HTTPS/HSTS فعال، دامنه درست.

Done: سایت رو Plesk بالا، همه flowها رو production تست‌شده.

---

## SEO (cross-cutting — از P3 به بعد رعایت)
- هر صفحه: `<title>` + `meta description` پویا، canonical، Open Graph + Twitter card.
- URL تمیز با slug فارسی محصول و دسته (نه id خام).
- محصول: structured data **JSON-LD** (`Product`, `Offer`, `AggregateRating`).
- breadcrumb + JSON-LD `BreadcrumbList`.
- `robots.txt` + `sitemap.xml` پویا.
- `alt` رو همه عکس، heading سلسله‌مراتبی (یک h1)، lang/dir درست.
- سرعت (Core Web Vitals) → P11.

## Cross-cutting (هر فاز رعایت)
- build سبز پایان هر فاز.
- قواعد `ARCHITECTURE.md` + `CONVENTIONS.md`.
- `PROGRESS.md` آپدیت + changelog.
- UI تغییرات → تست مرورگر موبایل قبل claim اتمام.
- SEO طبق بخش بالا.
- **Domestic-only:** هیچ asset/سرویس خارجی اضافه نشه. asset جدید → local در `wwwroot`.
