# MahanShop — Project Instructions (READ FIRST EVERY SESSION)

> هر سشن خودکار لود می‌شه. منبع حقیقت استک و ساختار. هر تصمیم اینجا قفل — تغییر نده مگر کاربر صریح بگه.

## What this is
فروشگاه آنلاین موبایل (متوسط). بازنویسی تمیز از صفر. داده قبلی مهم نیست. UI مشابه نمونه `Old UI` با ارتقا.

## HARD CONSTRAINTS (نقض نکن)
- **Host = Plesk 18.0.75 Windows shared.** هر تصمیم باید روش اجرا بشه.
- **Runtime = .NET 8 LTS (net8.0).** نه بالاتر، نه پایین‌تر.
- **DB = SQL Server 2019.** EF Core 8 سازگار (SQL 2016+).
- **Hosting = IIS in-process, `AspNetCoreModuleV2`, via `web.config`.** بدون Kestrel استندالون، بدون Docker، **بدون Node build روی هاست**.
- **Frontend = Razor server-render (MVC + Razor Pages) + Bootstrap 5 + jQuery.** نه SPA، نه React/Next.
- **بدون secret داخل سورس.** همه از Environment Variables (Plesk) یا user-secrets (dev).
- **Domestic-only (اینترنت داخلی).** سایت باید بدون اینترنت بین‌الملل کامل کار کنه. **هیچ وابستگی خارجی runtime**: نه CDN (cdnjs/jsdelivr/unpkg)، نه Google Fonts/gstatic، نه reCAPTCHA، نه Google Maps/Analytics، نه Gravatar. همه asset (css/js/font/lib) self-host در `wwwroot`. سرویس‌های خارجی فقط داخلی: پرداخت Zarinpal، SMS داخلی، نقشه = Neshan/Balad (نه Google). هیچ `<link>/<script>` به دامنه خارجی، هیچ preconnect/integrity خارجی.

## Stack lock (خلاصه — جزییات در docs/STACK.md)
| لایه | انتخاب |
|------|--------|
| Runtime | ASP.NET Core 8 (net8.0) |
| معماری | Clean: Domain / Application / Infra.Data / Infra.IoC / Web |
| الگو | MediatR (CQRS) |
| ORM/DB | EF Core 8 + SQL Server 2019 |
| Auth | Cookie Authentication |
| Frontend | Razor + Bootstrap 5 + jQuery |
| PDF | QuestPDF (Community) |
| پرداخت | Zarinpal |
| SMS | provider فعلی |
| Deploy | Plesk IIS in-process |

## Session continuity rules (مهم — جلوگیری از خرابی بین سشن‌ها)
1. **شروع هر سشن:** اول `docs/PROGRESS.md` بخون — وضعیت فعلی و قدم بعد اونجاست. **اگر روی پنل ادمین (Phase 9) کار می‌کنی، بعدش `docs/ADMIN_PROGRESS.md` رو هم بخون.**
2. **قبل کد زدن:** `docs/ARCHITECTURE.md` + `docs/CONVENTIONS.md` چک کن.
3. **نسخه پکیج:** فقط نسخه‌های پین‌شده `docs/STACK.md`. نسخه جدید بدون آپدیت اون فایل اضافه نکن.
4. **بعد هر تسک کامل:** `docs/PROGRESS.md` آپدیت کن (چی شد، قدم بعد).
5. **هیچوقت** TargetFramework رو از net8.0 عوض نکن. هیچوقت Node/SPA اضافه نکن.
6. تغییر تصمیم استک → اول از کاربر بپرس، بعد این فایل + STACK.md آپدیت.

## Pointers
- `docs/ROADMAP.md` — فازبندی 12-مرحله‌ای (با done-criteria)
- `docs/ADMIN_ROADMAP.md` — رودمپ ۴ مرحله‌ای پنل ادمین (Phase 9) با معیار rollback هر مرحله + tag `baseline-before-admin-panel`
- `docs/ADMIN_PROGRESS.md` — پیشرفت زندهٔ پنل ادمین (هر سشن کار ادمین آپدیت)
- `docs/STACK.md` — نسخه دقیق پکیج‌ها
- `docs/ARCHITECTURE.md` — نقشه لایه‌ها، قواعد وابستگی، ساختار پوشه
- `docs/DEPLOYMENT.md` — Plesk + web.config + env vars
- `docs/CONVENTIONS.md` — قواعد کدنویسی
- `docs/PROGRESS.md` — وضعیت زنده (هر سشن آپدیت)
