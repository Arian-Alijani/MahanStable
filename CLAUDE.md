# MahanShop — Project Instructions (READ FIRST EVERY SESSION)

> هر سشن خودکار لود می‌شه. منبع حقیقت استک و تصمیم‌ها. هر تصمیم اینجا قفل — تغییر نده مگر کاربر صریح بگه.
>
> 📍 **اول از هر چیز `docs/AI_CONTEXT.md` رو بخون** — نقشهٔ فشردهٔ کل پروژه (کجا چی هست). باهاش با حداقل توکن جهت‌یابی می‌کنی.
>
> **تقسیم کار فایل‌ها:** `CLAUDE.md` = تصمیم/قانون (ثابت) · `AI_CONTEXT.md` = نقشه/ایندکس (کجا چی) · `PROGRESS.md` = وضعیت زنده (چی شد/قدم بعد).

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
1. **شروع هر سشن:** اول `docs/AI_CONTEXT.md` (نقشه) بعد `docs/PROGRESS.md` (وضعیت فعلی + قدم بعد). کار پنل ادمین → جزئیات ادمین هم در `PROGRESS.md` + `AI_CONTEXT.md` §۵.
2. **قبل کد زدن:** `docs/ARCHITECTURE.md` + `docs/CONVENTIONS.md` چک کن (تنوع/موجودی → `docs/VARIANTS.md`).
3. **نسخه پکیج:** فقط نسخه‌های پین‌شده `docs/STACK.md`. نسخه جدید بدون آپدیت اون فایل اضافه نکن.
4. **بعد هر تسک کامل:** `docs/PROGRESS.md` آپدیت کن (چی شد، قدم بعد). اگر نقشهٔ پروژه عوض شد → `docs/AI_CONTEXT.md` هم fix کن.
5. **هیچوقت** TargetFramework رو از net8.0 عوض نکن. هیچوقت Node/SPA اضافه نکن.
6. تغییر تصمیم استک → اول از کاربر بپرس، بعد این فایل + STACK.md آپدیت.
7. **تست/بیلد:** ادعای «build سبز» فقط بعد از `bash tools/build.sh` و دیدن «0 Error». فقط View/JS/CSS؟ → `bash tools/check-js.sh`. روش/اعداد کامل: `docs/ENV_TESTING.md`. (dotnet با `source tools/setup-dotnet.sh` در ~17s نصب می‌شه.)

## Pointers (فقط فایل‌های موجود)
- `docs/AI_CONTEXT.md` — ⭐ نقشهٔ فشردهٔ کل پروژه (کجا چی) — اول اینو بخون
- `docs/PROGRESS.md` — وضعیت زنده + changelog کامل هر فاز (هر سشن آپدیت)
- `docs/STACK.md` — نسخه دقیق پکیج‌ها (پین‌شده)
- `docs/ARCHITECTURE.md` — نقشه لایه‌ها، قواعد وابستگی، ساختار پوشه
- `docs/CONVENTIONS.md` — قواعد کدنویسی + Domestic-only + ترتیب middleware
- `docs/VARIANTS.md` — طراحی سیستم تنوع (EAV) + موجودی per-variant (LOCKED)
- `docs/DEPLOYMENT.md` — Plesk + web.config + env vars
- `docs/ENV_TESTING.md` — ⚙️ تست/بیلد تو sandbox: نصب dotnet (~17s) + اسکریپت‌های `tools/` + چی جواب میده/نمیده (اعداد واقعی)
