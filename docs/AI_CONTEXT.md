# AI_CONTEXT — نقشهٔ پروژه برای AI (CAVEMAN. کوتاه. دقیق. بخون → جهت‌یابی کن → کد بزن)

> هدف: کلاد این فایل رو بخونه و **بدون اسکن کل کدبیس** بدونه چی کجاست.
> این فایل = ایندکس. حقیقتِ تصمیم‌ها = CLAUDE.md. وضعیت زنده = PROGRESS.md.
> اگه چیزی با کد فرق داشت → **کد درسته**، این فایل رو fix کن.

---

## 1. پروژه در یک نگاه
- **چی:** MahanShop — فروشگاه آنلاین موبایل (RTL فارسی). بازنویسی تمیز از صفر.
- **استک:** ASP.NET Core 8 (net8.0) · Clean Arch · MediatR CQRS · EF Core 8 · SQL Server 2019 · Razor (MVC+Pages) · Bootstrap5 RTL + jQuery.
- **هاست:** Plesk Windows shared · IIS in-process · web.config. **بدون Docker/Node/SPA.**
- **قانون آهنین:** Domestic-only — صفر CDN/سرویس خارجی. همه asset self-host. (جزئیات: CLAUDE.md)
- **محیط فعلی sandbox = Linux، dotnet پیش‌فرض نیست ولی نصب‌شدنیه (~17s)** → build/publish اینجا **سبز می‌شه**. روش دقیق + اعداد: **docs/ENV_TESTING.md**. سریع: `source tools/setup-dotnet.sh && bash tools/build.sh`. تست سبک JS: `bash tools/check-js.sh`.

## 2. وضعیت (مرجع کامل: docs/PROGRESS.md)
- P0–P8 ✅ (Scaffold, Domain+Data, Infra, Layout+Home, Catalog, Auth-OTP, Cart+Checkout, Payment-Zarinpal, UserPanel).
- **P9 Admin Panel = پیاده‌سازی‌شده و زنده** (Areas/Admin، 32 صفحهٔ Razor، 61 فایل Feature/Admin). در حال توسعه.
- 🔴 **ADMIN REVAMP در جریان** — بازطراحی بزرگ چندفازی (۶ تب + ساختار محصول + نوع پست). نقشه: `docs/ADMIN_REVAMP_ROADMAP.md` · وضعیت: `docs/ADMIN_REVAMP_PROGRESS.md`. **هر فاز سشن جداست.**
- بعدی: P10 Security · P11 Perf · P12 Deploy.
- tag بازگشت قبل ادمین: `baseline-before-admin-panel`.

---

## 3. نقشهٔ راه‌حل (5 پروژه — وابستگی فقط رو به داخل)
```
Web → Infra.IoC → Infra.Data → Application → Domain
```
| پروژه | مسیر | چیه | وابسته‌به |
|------|------|-----|----------|
| Domain | `src/MahanShop.Domain` | Entity (21) + Enum (6) + BaseEntity. pure C#. | هیچی |
| Application | `src/MahanShop.Application` | CQRS (Command/Query+Handler+Validator)، Interfaceها، DTO، Settings. | Domain |
| Infra.Data | `src/MahanShop.Infra.Data` | DbContext، Fluent Config، Migration، Seed، پیاده‌سازی سرویس‌ها (SMS/OTP/Zarinpal). | Application+Domain |
| Infra.IoC | `src/MahanShop.Infra.IoC` | `RegisterServices()` — سیم‌کشی DI همه لایه‌ها. | همه |
| Web | `src/MahanShop.Web` | Controller/View (فروشگاه عمومی) + Razor Pages (Panel + Areas/Admin) + wwwroot. | از طریق IoC |

**قانون:** Application هیچوقت EF مستقیم نمی‌بینه → فقط `IApplicationDbContext`. View هیچوقت Entity نمی‌گیره → فقط DTO/ViewModel. Controller فقط `_mediator.Send()`.

---

## 4. الگوی CQRS (کپی کن از اینجا)
مسیر: `Application/Features/<Area>/{Commands|Queries}/<Name>/`
```csharp
// Query.cs            — record + IRequest<TResult>
public record GetCatalogQuery(CatalogFilter Filter) : IRequest<CatalogViewModel>;
// QueryHandler.cs     — IRequestHandler. inject IApplicationDbContext. read => AsNoTracking().
// Validator.cs        — FluentValidation (کنار Command؛ pipeline = ValidationBehavior)
```
Controller:
```csharp
var vm = await _mediator.Send(new GetCatalogQuery(filter));
return View(vm);
```

---

## 5. کجا چی پیدا کنم (پُرکاربردترین مسیرها)

### Domain entities (`src/MahanShop.Domain/Entities/`)
هسته: `User` (Phone+OTP، `IsAdmin`/`IsActive`) · `Product` (`HasVariants` تعیین‌کننده) · `Category` (درختی، self-ref، `ParentId`) · `Brand` · `Order`+`OrderItem` (`RowVersion` ضدّ کسر دوبارهٔ موجودی) · `Payment` · `Address` · `OtpCode` (`CodeHash`+`Attempts`).
صفحهٔ اصلی: `Banner` · `HomeSection` (نوع در `HomeSectionType`).
سیستم تنوع (EAV): `VariantAttribute` + `VariantAttributeValue` (pool) → `ProductVariant` (واحد موجودی، `RowVersion`) → `ProductVariantValue` (join). طراحی کامل: **docs/VARIANTS.md**.
join: `ProductFeature`/`Feature` · `ProductTag`/`Tag` · `ProductImage` · `ProductComment` (بلااستفاده — امتیاز/کامنت حذف شد).

### Enums (`src/MahanShop.Domain/Enums/`)
`OrderStatus` (Pending→AwaitingPayment→Paid→Processing→Shipped→Delivered / Canceled / Refunded) · `PaymentStatus` (Pending/Success/Failed) · `HomeSectionType` (ProductRow/PromoBanner/CategoryGrid) · `HomeProductSource` (Featured/BestSelling/Newest/Discounted/ByCategory) · `HomeCategoryStyle` (Bento/Grid/Scroll/Pills — در `Subtitle` ذخیره، بدون migration) · `VariantAttributeKind` (Other/Brand/Model/Color — UI صفحهٔ محصول رو سوییچ می‌کنه).

### Application Features (`src/MahanShop.Application/Features/`)
فروشگاه عمومی: `Home/` (GetHomePage, GetMenuCategories) · `Catalog/` (GetCatalog فیلتر/سورت/صفحه، GetProductDetail) · `Auth/` (SendLoginCode, VerifyLoginCode) · `Cart/` (GetCart, PlaceOrder) · `Payment/` (StartPayment, VerifyPayment) · `Account/` (پنل کاربر: پروفایل/آدرس/سفارش/تغییر شماره).
**Admin** (`Features/Admin/` — 61 فایل): `Products/` (+wizard، MultiBrand) · `ProductVariants/` (+CSV) · `Variants/` (attribute/value CRUD) · `Categories/` · `Brands/` · `Features/` · `Tags/` · `Inventory/` (+CSV) · `Orders/` (تغییر وضعیت) · `Users/` · `Home/` (بنر/Hero/گرید دسته/ProductRow) · `Dashboard/` · `Common/SlugHelper`.

### Application Common (`src/MahanShop.Application/Common/`)
`Interfaces/`: `IApplicationDbContext` (همه DbSetها) · `IOtpHasher` · `ISmsSender` · `IPaymentGateway`.
`Settings/`: `OtpSettings` · `SmsSettings` · `ZarinpalSettings` (bind از env).
`Behaviors/ValidationBehavior` (MediatR pipeline) · `PhoneNumberHelper` (نرمال‌سازی موبایل ایران).

### Infra.Data (`src/MahanShop.Infra.Data/`)
`Context/MyDbContext.cs` (= `IApplicationDbContext`) · `Configurations/*` (Fluent — DeleteBehavior دقیق ضدّ multiple-cascade-path) · `Migrations/` (7 تا؛ آخرین: `Add_Admin_ValueParent`) · `Seed/DataSeeder.cs` (dev: کاتالوگ+صفحه‌اصلی+ادمین؛ prod: فقط `SeedAdminOnlyAsync`؛ ادمین اولیه=`09037882674`) · `Services/`: `OtpHasher` (HMAC+Pepper)، `SmsIrSender`/`FakeSmsSender` (انتخاب بر اساس وجود ApiKey)، `ZarinpalGateway`.

### Web (`src/MahanShop.Web/`)
`Program.cs` — **نقطهٔ ورود + سیم‌کشی + routeها + middleware**. بلوک‌های ادمین با مارکر `=== ADMIN-PANEL START/END ===` (برای rollback). policy `AdminOnly` = RequireRole("Admin"). `OnValidatePrincipal` نقش ادمین رو هر درخواست از DB re-check می‌کنه.
`Controllers/` (6: Home, Catalog, Cart, Checkout, Account, Payment) — فروشگاه عمومی MVC.
`Views/` (27 .cshtml) — per controller + `Shared/_Layout`,`_Header`,`_Footer`,`_ProductCard` + `Components/CategoryMenu`.
`Pages/Panel/` — پنل کاربر (Razor Pages، `AuthorizeFolder("/Panel")`). base = `PanelPageModel` (UserId از claim).
`Areas/Admin/Pages/` — پنل ادمین (32 صفحه، `AuthorizeAreaFolder("Admin",...,"AdminOnly")`). Shared: `_AdminLayout`,`_AdminSidebar`. زیرپوشه‌ها = Products/Variants/Categories/Brands/Features/Tags/Inventory/Orders/Users/Home.
`Services/`: `CartStore` (سبد session-based، فقط ProductId/VariantId/Qty — **هیچ قیمتی client**) · `ImageUploadService` (whitelist پسوند/نوع/حجم → wwwroot/AdminPanel) · `InvoicePdfService` (QuestPDF، فونت family=`Vazir`).
`wwwroot/`: `css/` `js/` `lib/` (bootstrap RTL، jquery، validation — همه self-host) `fonts/vazirmatn/` `admin/` (JS ادمین) `AdminPanel/` (آپلودها، gitignored) `img/`.

---

## 6. قانون‌های اجرایی (نقض نکن — جزئیات: CLAUDE.md + CONVENTIONS.md)
- **net8.0 ثابت.** بدون Node/SPA/Docker. بدون asset خارجی (CDN/GoogleFonts/recaptcha/gravatar ممنوع).
- **secret فقط env** (`__` به nested config مپ می‌شه). hardcode ممنوع. `appsettings.*.json` gitignored جز `.sample`.
- async همه‌جا (نه `.Result`/`.Wait()`). read query → `AsNoTracking()`.
- قیمت/موجودی **همیشه سمت سرور از DB** (ضدّ دستکاری). کسر موجودی فقط بعد پرداخت موفق + idempotent (RowVersion).
- anti-forgery گلوبال روی همه POST. ViewModel جدا از Entity. خروجی Razor auto-encode.
- ترتیب middleware: `HttpsRedirection→StaticFiles→Routing→Session→Authentication→Authorization→Map`. (Session قبل auth — باگ نمونهٔ قدیم.)
- migration: اسم بامعنی `Add_<X>`؛ migration applied رو edit نکن.
- **مرز کد ادمین:** فقط داخل `Areas/Admin`, `wwwroot/admin`, `wwwroot/AdminPanel`, `Features/Admin` + بلوک‌های مارکردار `Program.cs`. → rollback آسون.

## 7. چرخهٔ کار هر سشن
1. این فایل + `PROGRESS.md` بخون (کار ادمین → جزئیات هم اینجا).
2. قبل کد: `CONVENTIONS.md` (+ `VARIANTS.md` اگه تنوع/موجودی).
3. بعد تسک: `PROGRESS.md` آپدیت (چی شد، قدم بعد) + این فایل اگه نقشه عوض شد.
4. کامیت روی `genspark_ai_developer` → PR به main → لینک PR بده.

## 8. تلهٔ سریع (gotchas)
- dotnet پیش‌فرض نصب نیست **ولی نصب‌شدنیه** → برای ادعای «build سبز» واقعاً `bash tools/build.sh` بزن و «0 Error» ببین (مرجع کامل docs/ENV_TESTING.md). فقط View/JS/CSS؟ → `bash tools/check-js.sh` کافیه. ⚠️ اجرای واقعی برنامه (dotnet run) بدون SQL Server بالا نمیاد (seed در Program.cs:106 کرش می‌کنه).
- `HomeCategoryStyle` در `HomeSection.Subtitle` (رشته) ذخیره می‌شه نه ستون جدا.
- فونت PDF: family واقعی TTF = `Vazir` (نه `Vazirmatn`) وگرنه فارسی tofu.
- `ProductComment` entity هست ولی feature امتیاز/کامنت **حذف** شده — استفاده نکن.
- لاگین: `Otp__Pepper` نباشه → `OtpHasher` در اولین لاگین throw. dev بدون `Sms__ApiKey` → `FakeSmsSender` (کد در لاگ).
