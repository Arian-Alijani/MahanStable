# ADMIN PROGRESS — وضعیت زندهٔ پنل ادمین (هر سشن آپدیت کن)

> **شروع هر سشنِ کار روی پنل ادمین: اول این فایل را بخوان.** اینجا = حافظهٔ بین‌سشن‌ها برای رودمپ پنل ادمین.
> رودمپ کامل + معیار rollback هر مرحله: `docs/ADMIN_ROADMAP.md`.
> هدف کلی: پنل ادمین را تدریجی و **قابل‌برگشت** بساز؛ هر وقت لازم شد با tag `baseline-before-admin-panel` به حالت تمیز برگرد.

---

## وضعیت فعلی
**مرحله: ۴ DONE — Phase 9 (پنل ادمین) کامل شد (کد نوشته شد؛ بیلد/تست طبق درخواست کاربر اجرا نشد).**

### مرحله ۴ (سفارش‌ها/کاربران/داشبورد آمار) — این سشن
ادامهٔ کامیت `PH4 half` (لایهٔ Application + صفحات Orders از قبل بود). در این سشن تکمیل شد:
- **داشبورد:** `Areas/Admin/Pages/Index` به `GetDashboardStatsQuery` وصل شد (فروش امروز/کل، سفارش‌های منتظر پردازش، موجودی کم، شمارنده‌ها + جدول سفارش‌های اخیر + جدول موجودی کم).
- **کاربران:** `Areas/Admin/Pages/Users/Index` (لیست + جستجو/فیلتر مدیر/غیرفعال + صفحه‌بندی) و `Users/Detail` (پروفایل + toggle نقش مدیر/فعال + آدرس‌ها با حذف + سفارش‌های اخیر). `CurrentAdminId` از claim (نه فرم) — گارد خودقفل‌نشدن در command‌ها.
- **sidebar:** لینک‌های «سفارش‌ها» و «کاربران» اضافه شد.
- **CSS:** کلاس‌های داشبورد/جزییات/بج وضعیت به `wwwroot/admin/admin.css` افزوده شد (`admin-status-badge` toneها، `admin-dashboard-grid`، `admin-grid-2`، `admin-dl`، `admin-page-title`، `admin-box-title`، `admin-panel-box__head/__title`، `admin-stat-card--soft/__unit`).

موجود از قبل (PH4 half): `Features/Admin/Dashboard`، `Features/Admin/Orders` (Get/Detail/ChangeStatus + DTOها)، `Features/Admin/Users` (Get/Detail/Commands + DTOها)، صفحات `Orders/Index` و `Orders/Detail` (با فاکتور PDF reuse). **بدون migration جدید.**

---

## وضعیت قبلی
**مرحله: ۳ DONE + ممیزی امنیتی پنل ادمین DONE.**

### ممیزی امنیتی (Security Hardening — این سشن)
بررسی‌شده و **سالم** بود: policy `AdminOnly` روی کل Area، anti-forgery گلوبال، همهٔ عملیات تغییردهنده POST (هیچ GET-state-change)، بدون SQL خام، بدون `Html.Raw` در ادمین، OTP با هش constant-time + MaxAttempts، آپلود whitelist پسوند/Content-Type/حجم + نام فایل GUID، `Url.IsLocalUrl` روی returnUrl، حذف عکس فقط از DB (بدون path traversal).

اصلاح‌شده:
1. **Program.cs**: `OnValidatePrincipal` → revoke فوری نقش Admin اگر `IsAdmin/IsActive` در DB برداشته شود (کوکی ۱۰روزه دیگر تا انقضا معتبر نمی‌ماند) — فقط برای ادمین‌ها re-check (بدون هزینهٔ DB برای کاربر عادی).
2. **Program.cs**: کوکی auth/session → `Secure=Always` + `SameSite=Lax` + `HttpOnly` صریح.
3. **Program.cs**: هدرهای امنیتی `X-Content-Type-Options: nosniff` / `X-Frame-Options: DENY` / `Referrer-Policy`.
4. **Program.cs**: `AccessDeniedPath = "/"` (عدم افشای وجود پنل ادمین به کاربر لاگین‌شدهٔ غیرادمین).
5. **ImageUploadService**: اعتبارسنجی magic bytes (JPEG/PNG/GIF/WEBP) — جلوگیری از آپلود فایل جعلی با پسوند عکس.
6. **.gitignore**: `wwwroot/AdminPanel/` (آپلودهای dev کامیت نشوند).

تصمیم Phase 8.5: entityهای Variant (`ProductVariant/ProductVariantValue/VariantAttribute/VariantAttributeValue/Feature/ProductFeature/Tag/ProductTag`) و migration `Add_ProductVariants` از قبل موجود بودند → نسخهٔ **کامل** مرحله ۳ پیاده شد (نه نسخهٔ سادهٔ per-product). **هیچ migration جدیدی لازم نشد** (روی schema موجود).

ساخته‌شد (مرحله ۳):
- `Application/Features/Admin/Variants` (CQRS کامل: attribute pool + value pool — list/getForEdit/create/update/delete).
- `Application/Features/Admin/Features` (CRUD مشخصه فنی) + `Application/Features/Admin/Tags` (CRUD برچسب با slug).
- `Application/Features/Admin/ProductVariants` (per-product: Get view + Create/Update/QuickStock/Toggle/Delete variant، با بررسی قیمت/مقادیر معتبر).
- `Areas/Admin/Pages/Variants` (Index/Create/Edit + Values=مدیریت pool مقادیر، swatch رنگ).
- `Areas/Admin/Pages/Features` و `/Tags` (Index/Create/Edit + حذف).
- تب «گزینه‌ها/موجودی» در `Products/Edit` (گرید variant + ویرایش سریع موجودی inline + toggle/delete + فرم افزودن گزینه با انتخابگر مقادیر).
- لینک‌های sidebar (ویژگی‌های متغیر/مشخصات فنی/برچسب‌ها) + CSS `admin-inline-form/admin-stock-input/admin-value-row` در `admin.css`.

**مرحله ۲ (قبلی):** CRUD برند/دسته/محصول + آپلود عکس + ImageUploadService + بلوک نشان‌دار Program.cs — بدون تغییر.

قدم بعد = **پایان رودمپ — Phase 9 کامل.** (در صورت نیاز: بیلد/تست/smoke که طبق درخواست کاربر اجرا نشد.)

---

## نقطهٔ مبنا (BASELINE — مقصد بازگشت) — قفل‌شده
| مورد | مقدار |
|------|-------|
| Git tag | `baseline-before-admin-panel` |
| Commit baseline | `f279aa6` |
| فاز پروژه | پایان Phase 8 (User Panel) |
| فایل tracked | ۲۷۳ |
| کد ادمین | صفر |

**بازگشت کامل به این نقطه:**
```bash
cd /home/user/webapp
git checkout main && git reset --hard baseline-before-admin-panel
```
> فقط ارجاع‌های عمومی ادمین (`User.IsAdmin`، claim نقش در Auth، اشارهٔ docs) در baseline هستند و جزو پنل ادمین **نیستند** — حذف نکن.

---

## چک‌لیست مراحل (هر کدام تمام شد، اینجا تیک بزن + جزئیات ثبت کن)

- [x] **مرحله ۱ — Shell + Auth Gate** — Areas/Admin layout + داشبورد خالی + policy `AdminOnly`.
- [x] **مرحله ۲ — کاتالوگ پایه** — CRUD محصول/دسته/برند + آپلود عکس.
- [x] **مرحله ۳ — Variant/موجودی + ویژگی/تگ** — pool ویژگی/مقادیر + CRUD مشخصه/برچسب + گرید variant per-product (بدون migration جدید؛ Phase 8.5 از قبل موجود بود).
- [x] **مرحله ۴ — سفارش‌ها/کاربران/داشبورد آمار** — تکمیل Phase 9 (داشبورد واقعی + CRUD سفارش/تغییر وضعیت/فاکتور + مدیریت کاربر؛ بدون migration جدید).

---

## دفتر ثبت rollback (هر مرحله که DONE شد، اینجا پر کن)
> این جدول برای rollback دقیق ضروری است. بدون آن نمی‌دانیم هر مرحله چه commit/migration اضافه کرده.

| مرحله | commit hash | migrationهای اضافه‌شده | تاریخ | یادداشت |
|------|-------------|------------------------|-------|---------|
| baseline | `f279aa6` | — (تا `Add_Order_RowVersion`) | 2026-06-12 | tag: `baseline-before-admin-panel` |
| ۱ | — | — | — | — |
| ۲ | — | — | — | — |
| ۳ | — | — | — | — |
| ۴ | — | — | — | — |

---

## یادآوری‌های مهم برای ساخت (تا rollback نشکند)
1. کد ادمین **فقط** در: `Areas/Admin/**`, `wwwroot/admin/**`, `wwwroot/AdminPanel/**`, `Application/Features/Admin/**`.
2. تنها فایل مشترکِ قابل‌تغییر = `Program.cs`، فقط داخل بلوک نشان‌دار:
   `// === ADMIN-PANEL START === ... // === ADMIN-PANEL END ===`.
3. هر مرحله = build سبز + **یک commit مستقل** با پیشوند `admin(pN): ...`.
4. migration فقط افزایشی با نام `Add_Admin_*`؛ هیچ migration قبلی ویرایش نشود.
5. domestic-only، anti-forgery، دسترسی فقط `IsAdmin`، whitelist آپلود.
6. پایان هر مرحله: همین فایل را آپدیت کن (تیک چک‌لیست + ردیف جدول rollback + قدم بعد) و `commit` بزن.

---

## Open questions (وقتی رسیدی از کاربر بپرس)
- آیا Phase 8.5 (Variant) قبل از مرحلهٔ ۳ پنل ادمین انجام شود، یا مرحلهٔ ۳ موقتاً «موجودی per-product ساده» باشد؟
- کاربر ادمین اولیه چطور ساخته شود؟ (seed dev / دستی در DB / صفحهٔ bootstrap یک‌بارمصرف)
- ظاهر پنل ادمین دقیقاً مثل کدام نمونه باشد؟ (Old UI admin / طراحی جدید)

---

## Changelog
- 2026-06-12: رودمپ ۴ مرحله‌ای پنل ادمین (`ADMIN_ROADMAP.md`) + این فایل پیشرفت ساخته شد. tag `baseline-before-admin-panel` روی `f279aa6` ثبت شد (مقصد بازگشت). هنوز هیچ کد ادمینی نوشته نشده — مرحله ۰.
- 2026-06-13: **پاسخ سؤالِ باز «کاربر ادمین اولیه چطور ساخته شود؟» → seed استاندارد.** ادمین اولیه (`09037882674`) به شکل کاملاً استاندارد در `DataSeeder` seed می‌شود: یک رکورد `User` معمولی (همان ساختار کاربرانِ ورود OTP) فقط با `IsAdmin=true`، شماره نرمال‌شده با `PhoneNumberHelper`. idempotent (موجود نبود→ساخت / موجود بود→ادمین/فعال‌سازی / ادمین بود→بدون تغییر). متد جدید `SeedAdminOnlyAsync` (migrate + فقط ادمین، بدون داده‌ی نمونه) در `Program.cs` برای محیط‌های غیر-Development فراخوانی می‌شود تا شماره‌ی ادمین روی Production هم دسترسی پنل بگیرد؛ در Development داخل `SeedAsync` انجام می‌گیرد. تغییر `Program.cs` فقط در بلوک bootstrap seed (نه منطق پنل) و idempotent/rollback-safe.
