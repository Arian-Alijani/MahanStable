# ADMIN PROGRESS — وضعیت زندهٔ پنل ادمین (هر سشن آپدیت کن)

> **شروع هر سشنِ کار روی پنل ادمین: اول این فایل را بخوان.** اینجا = حافظهٔ بین‌سشن‌ها برای رودمپ پنل ادمین.
> رودمپ کامل + معیار rollback هر مرحله: `docs/ADMIN_ROADMAP.md`.
> هدف کلی: پنل ادمین را تدریجی و **قابل‌برگشت** بساز؛ هر وقت لازم شد با tag `baseline-before-admin-panel` به حالت تمیز برگرد.

---

## وضعیت فعلی
**مرحله: ۳ — Variant/موجودی + ویژگی/تگ DONE (کد نوشته شد؛ بیلد/تست طبق درخواست کاربر اجرا نشد).**

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

قدم بعد = **مرحلهٔ ۴ (سفارش‌ها/کاربران/داشبورد آمار)** = تکمیل Phase 9.

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
- [ ] **مرحله ۴ — سفارش‌ها/کاربران/داشبورد آمار** — تکمیل Phase 9.

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
