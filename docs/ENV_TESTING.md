# ENV_TESTING — تست/بیلد تو sandbox واقعاً چطوری جواب میده (CAVEMAN)

> تست‌شده عملی روی همین محیط (Debian 13، x64، 2026-06-13). اعداد واقعی‌ان، حدس نیست.
> هدف: سشن بعد **مستقیم** بدونه چی نصب کنه و چی جواب میده — بدون آزمون‌وخطا.

---

## TL;DR (همینو بخون و بزن)
```bash
source tools/setup-dotnet.sh          # نصب .NET 8 SDK (~17s) + ست env. idempotent.
bash   tools/build.sh                  # restore+build کل sln. «0 Error» = سبز.
bash   tools/build.sh publish          # + publish IIS به ~/publish_test
bash   tools/check-js.sh               # تست سبک JS، بدون dotnet
```
**dotnet تو sandbox پیش‌فرض نیست. ولی نصبش می‌شه و build/publish کامل سبز می‌شه.**
(فرض قدیمی «اینجا build نمی‌شه» باطل شد — می‌شه.)

---

## محیط چی داره / چی نداره
| چیز | هست؟ | نکته |
|-----|------|------|
| node / npm | ✅ v20 / v10 | برای `node --check` و تست JS. همیشه آماده. |
| curl/wget/git/apt/sudo | ✅ | شبکه باز به builds.dotnet.microsoft.com و packages.microsoft.com |
| **dotnet** | ❌ پیش‌فرض | نصب‌شدنی در ~17s (پایین) |
| دیسک `/` | ✅ ~19GB آزاد | جا برای SDK(580M)+nuget(510M) هست |
| **`/tmp`** | ⚠️ فقط **493MB tmpfs** | **تلهٔ مرگبار** — extract آرشیو 216MB اینجا می‌ترکه |
| SQL Server | ❌ | نیست. اجرای واقعی برنامه گیر می‌کنه (پایین) |

---

## نصب dotnet — روش درست (تنها روشی که جواب داد)
```bash
source tools/setup-dotnet.sh
```
زیرکاپوت همین چند خط (اگه دستی خواستی):
```bash
export TMPDIR="$HOME/tmp"; mkdir -p "$TMPDIR"      # ← کلیدِ همه‌چی
curl -sSL -o ~/dotnet-install.sh https://dot.net/v1/dotnet-install.sh
TMPDIR="$HOME/tmp" bash ~/dotnet-install.sh --channel 8.0 --install-dir "$HOME/.dotnet"
export DOTNET_ROOT="$HOME/.dotnet"; export PATH="$HOME/.dotnet:$PATH"
export NUGET_PACKAGES="$HOME/.nuget/packages"; export DOTNET_CLI_TELEMETRY_OPTOUT=1
```
**چرا `TMPDIR=$HOME/tmp` واجبه:** بدونش اسکریپت رسمی فایل‌ها رو تو `/tmp` (493MB tmpfs)
باز می‌کنه و با `No space left on device` می‌ترکه — هرچند `/` ۱۹گیگ آزاد داره. تست‌شده، قطعی.

**نسخه:** `--channel 8.0` → الان `8.0.422` SDK + ASP.NET runtime `8.0.28`. net8.0 قفله، بالاتر نرو.

**apt/sudo نزن.** نصب user-local (`~/.dotnet`) سریع‌تر و بی‌دردسرتره. root لازم نیست.

---

## چی واقعاً جواب میده (با زمان واقعی)
| تست | دستور | نتیجه | زمان |
|-----|-------|-------|------|
| نصب SDK | `tools/setup-dotnet.sh` | ✅ | ~17s |
| restore | `dotnet restore MahanShop.sln` | ✅ همه ۵ پروژه | ~16s (بار اول؛ بعد cache) |
| **build** | `dotnet build --no-restore` | ✅ **0 Error**، 5 Warning بی‌خطر | ~24s |
| publish | `dotnet publish ...Web -c Release` | ✅ + `web.config` IIS تولید شد | ~18s |
| تست JS | `node tests/*.test.js` | ✅ 10 passed | ~1s |
| سینتکس JS | `node --check` روی ۸ فایل | ✅ همه ok | ~2s |

Warningها (بی‌خطر، نادیده بگیر): چندتا `CS0108 'Page' hides...` در Areas/Admin + یک `CS8321` local function بلااستفاده.

---

## چی جواب **نمیده** (مرز محیط)
- **`dotnet test`** → هیچ پروژه‌ی تست `xUnit/.csproj` وجود نداره. خروجی خالی. تست واقعی = `tests/*.test.js` (Node).
- **اجرای واقعی برنامه (`dotnet run`)** → بدون SQL Server بالا **نمیاد**.
  علتش دقیق: `Program.cs:106` در startup `DataSeeder.SeedAdminOnlyAsync` رو صدا می‌زنه که `MigrateAsync` می‌کنه →
  چون DB نیست، **قبل از باز شدن پورت HTTP** کرش می‌کنه (`ConnectionString ... not been initialized`).
  یعنی **smoke test واقعی HTTP اینجا ممکن نیست** مگر:
  - SQL Server بیاری (سنگین، توصیه نمی‌شه)، یا
  - موقتاً بلوک seed در `Program.cs` رو رد کنی فقط برای تست بالاآمدن (بعد commit نکن).
- نتیجه: تو sandbox **build/publish/تست‌منطق سبز = کافی**. درستیِ DB/HTTP رو روی هاست واقعی بسنج.

---

## چک‌لیست سشن بعد (کپی کن)
1. می‌خوای صحت کامپایل C# رو ثابت کنی؟ → `source tools/setup-dotnet.sh && bash tools/build.sh` → دنبال «0 Error».
2. فقط View/JS/CSS عوض کردی؟ → نیازی به dotnet نیست؛ `bash tools/check-js.sh` کافیه (سریع، توکن‌کم).
3. آماده‌ی deploy؟ → `bash tools/build.sh publish` → خروجی `~/publish_test` (شامل `web.config`).
4. **نگو «build سبز» مگه واقعاً `tools/build.sh` زده باشی و «0 Error» دیده باشی.**

> اگه AI_CONTEXT گفت «dotnet نیست/build اجرا نمی‌شه» → الان دیگه می‌شه؛ این فایل مرجعه.
