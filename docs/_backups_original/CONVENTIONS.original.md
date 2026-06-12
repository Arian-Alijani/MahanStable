# CONVENTIONS — Coding rules

> ثابت نگه‌دار بین سشن‌ها. کد جدید عین این الگو.

## C#
- `Nullable` enable, `ImplicitUsings` enable (همه پروژه‌ها).
- async/await همه‌جا برای I/O و DB. هیچ `.Result`/`.Wait()`.
- EF read query → `AsNoTracking()`.
- نام: PascalCase کلاس/متد، camelCase لوکال، `_camelCase` فیلد private، `I` پیشوند interface.

## CQRS
- یک فایل = یک Command/Query + Handler (یا Handler جدا، ثابت بمون).
- ساختار: `Application/Features/<Area>/{Commands|Queries}/<Name>/`.
- Validator FluentValidation کنار هر Command.
- Controller چیزی جز bind + `_mediator.Send()` نداره.

## Web / Razor
- Controller per area فروشگاه؛ Admin + PanelUser = Razor Pages.
- ViewModel جدا از Domain entity (entity رو مستقیم به View نده).
- anti-forgery token رو همه فرم POST.
- RTL: `<html dir="rtl" lang="fa">`, Bootstrap 5 RTL، فونت Vazir.

## امنیت (همیشه)
- secret فقط از config/env. هیچ hardcode.
- ورودی validate (FluentValidation سمت سرور — به client اعتماد نکن).
- خروجی Razor خودکار encode می‌شه؛ `Html.Raw` فقط با محتوای امن.
- فایل آپلود: whitelist پسوند + content-type + حد اندازه.

## Program.cs — ترتیب middleware درست (باگ نمونه قدیم اصلاح شد)
```
UseHttpsRedirection → UseStaticFiles → UseRouting
→ UseSession            (قبل auth و endpoint، نه بعدش)
→ UseAuthentication → UseAuthorization
→ MapRazorPages / MapControllerRoute
```
- نمونه قدیم `UseSession()` بعد از map صدا می‌زد → session تو controller کار نمی‌کرد. تکرار نکن.
- typo نمونه: `LogoutPath="/Loguot"` → درستش `/Logout`.

## Migration
- اسم بامعنی: `Add_Product_Discount`.
- هر تغییر مدل → migration جدا، هیچوقت migration applied رو edit نکن.

## Git
- secret commit نکن. `.gitignore`: `appsettings.*.json` (به‌جز نمونه)، `publish/`, `logs/`, `bin/`, `obj/`.
- `appsettings.sample.json` بدون مقدار واقعی نگه‌دار.
