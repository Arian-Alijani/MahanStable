# ADMIN ROADMAP — پنل ادمین (Phase 9) — رودمپ ۴ مرحله‌ای قابل‌برگشت

> این فایل = نقشهٔ راه ساخت پنل ادمین، طراحی‌شده طوری که **هر مرحله مستقل و قابل حذف کامل (rollback)** باشد.
> تلاش قبلی پنل ادمین نیمه‌کاره رها شد؛ این بار با مرزبندی دقیق پیش می‌رویم تا هر وقت لازم شد، با چند دستور به **حالت تمیز قبل از پنل ادمین** برگردیم.
>
> **پیشرفت زنده این رودمپ در `docs/ADMIN_PROGRESS.md` است — شروع هر سشن اول آن را بخوان.**

---

## 0. نقطهٔ مبنا (BASELINE — مقصد بازگشت)

این وضعیتی است که می‌خواهیم در صورت لزوم به آن برگردیم — **هیچ کدی از پنل ادمین وجود ندارد**.

| مورد | مقدار |
|------|-------|
| Git tag | `baseline-before-admin-panel` |
| Commit | `f279aa6` (Initial commit) |
| فاز پروژه | پایان **Phase 8 (User Panel)** |
| تعداد فایل tracked | ۲۷۳ |
| کد ادمین موجود | **صفر** (نه `Areas/Admin`، نه `Pages/Admin`، نه `Features/Admin`، نه Controller، نه CSS/JS ادمین) |

> فقط ارجاع‌های عمومی به ادمین در baseline وجود دارد و **جزو پنل ادمین حساب نمی‌شوند** (نباید حذف شوند):
> - `User.IsAdmin` (فیلد Domain)
> - claim نقش در `VerifyLoginCode` (Auth)
> - اشارهٔ متنی به Admin در `docs/ARCHITECTURE.md` و `docs/ROADMAP.md`

### 🔙 بازگشت کامل به baseline (نسخهٔ git — توصیه‌شده)
```bash
cd /home/user/webapp
# همهٔ کار پنل ادمین را دور بریز و دقیقاً به نقطهٔ مبنا برگرد:
git checkout main
git reset --hard baseline-before-admin-panel
# (برنچ کاری پنل ادمین را هم می‌توان حذف کرد)
git branch -D genspark_ai_developer   # اختیاری
```

### 🔙 بازگشت دستی (نسخهٔ بدون git — اگر کد merge شده بود)
هر مرحله در بخش خودش یک «لیست مرزی» (Boundary list) دارد: دقیقاً فایل‌ها/پوشه‌ها/خط‌هایی که آن مرحله اضافه می‌کند. برای rollback دستی کافی است **به‌ترتیب معکوس** (مرحله ۴ → ۱) لیست مرزی هر مرحله را حذف/برگردانی. جزئیات هر مرحله پایین آمده.

---

## اصول طلایی این رودمپ (تا rollback همیشه آسان بماند)

1. **مرز فیزیکی واحد:** تمام کد ادمین فقط در این مسیرها قرار می‌گیرد و جای دیگری نه:
   - `src/MahanShop.Web/Areas/Admin/**`  (صفحات Razor Pages ادمین)
   - `src/MahanShop.Web/wwwroot/admin/**`  (css/js اختصاصی ادمین)
   - `src/MahanShop.Web/wwwroot/AdminPanel/**`  (آپلود عکس — طبق ARCHITECTURE)
   - `src/MahanShop.Application/Features/Admin/**`  (CQRS مخصوص ادمین)
2. **حداقل دست‌کاری فایل‌های موجود:** تنها فایل مشترکی که لمس می‌شود `Program.cs` است و آن هم فقط با **یک بلوک نشان‌دار**:
   ```csharp
   // === ADMIN-PANEL START (Phase 9) — remove this whole block to rollback ===
   ...
   // === ADMIN-PANEL END ===
   ```
   هر تغییر دیگری در فایل‌های مشترک ممنوع است مگر داخل همین نوع بلوک نشان‌دار.
3. **بدون migration تخریبی:** هر تغییر مدل داده در مرحله‌ای جدا، با migration **افزایشی** و نام `Add_Admin_*`. هیچ migration موجود ویرایش نمی‌شود. (rollback DB = `dotnet ef migrations remove` به‌ترتیب معکوس.)
4. **هر مرحله = build سبز + commit مستقل.** هر مرحله یک commit جدا با پیشوند `admin(pN):` می‌گیرد تا `git revert`/`reset` تک‌مرحله‌ای ممکن باشد.
5. **بدون شکستن فروشگاه عمومی:** هیچ مرحله نباید مسیرهای عمومی/پنل کاربر را تغییر دهد.
6. رعایت کامل `CLAUDE.md` + `ARCHITECTURE.md` + `CONVENTIONS.md` (Clean Arch، CQRS، Razor Pages برای ادمین، Bootstrap5 RTL + CSS دستی، domestic-only، anti-forgery، دسترسی فقط `IsAdmin`).

---

## مرحله ۱ — Shell + Auth Gate ادمین (اسکلت و دروازهٔ دسترسی)

**هدف:** پوستهٔ پنل ادمین که فقط برای کاربر `IsAdmin` باز شود؛ هنوز هیچ CRUD واقعی ندارد.

**خروجی:**
- `Areas/Admin/` با `_ViewStart`, `_ViewImports`, layout اختصاصی ادمین (`_AdminLayout.cshtml`) + sidebar/topbar (Bootstrap5 RTL، CSS دستی در `wwwroot/admin/admin.css`).
- صفحهٔ `Areas/Admin/Pages/Index.cshtml` (داشبورد خالی با placeholder آمار).
- **Authorization policy `"AdminOnly"`** مبتنی بر claim نقش/`IsAdmin` + `AuthorizeAreaFolder("Admin", "/", "AdminOnly")` در بلوک نشان‌دار `Program.cs`.
- مسیر `/Admin` فقط برای ادمین؛ کاربر عادی → AccessDenied؛ ناشناس → login.
- (در صورت نبود ادمین در seed) یک seed dev اختیاری برای یک کاربر ادمین — **فقط در بلوک نشان‌دار DataSeeder یا فایل seed جدا**، فقط Development.

**Done:** build سبز + `/Admin` با کاربر ادمین 200، با کاربر عادی 403/AccessDenied، ناشناس 302 login. فروشگاه و پنل کاربر دست‌نخورده.

**🧱 Boundary list (آنچه این مرحله اضافه می‌کند):**
- `src/MahanShop.Web/Areas/Admin/**` (layout + Index + sidebar)
- `src/MahanShop.Web/wwwroot/admin/admin.css` (+ `admin.js` اگر لازم)
- بلوک نشان‌دار در `Program.cs` (policy + AuthorizeAreaFolder + route Area)
- (اختیاری) بلوک نشان‌دار seed ادمین dev

**🔙 rollback این مرحله:** حذف پوشهٔ `Areas/Admin` و `wwwroot/admin`، حذف بلوک نشان‌دار از `Program.cs` (+ seed). بدون تغییر DB.

---

## مرحله ۲ — مدیریت کاتالوگ پایه: محصولات + دسته/برند (CRUD اصلی)

**هدف:** ادمین بتواند موجودیت‌های هستهٔ فروشگاه را مدیریت کند.

**خروجی:**
- `Features/Admin/Products/` (CQRS): `GetProductsQuery` (لیست + صفحه‌بندی/جستجو)، `GetProductForEditQuery`، `CreateProductCommand`، `UpdateProductCommand`، `ToggleProductActiveCommand` (+ Validatorها).
- `Features/Admin/Categories/` و `Features/Admin/Brands/`: CRUD کامل.
- صفحات Razor در `Areas/Admin/Pages/Products/`, `/Categories/`, `/Brands/` (Index/Create/Edit + حذف نرم/Toggle).
- **آپلود عکس محصول** → `wwwroot/AdminPanel/Photo/...` با whitelist پسوند/Content-Type/اندازه (طبق CONVENTIONS). گالری چندعکسه.
- اتصال به منوی sidebar مرحله ۱.

**Done:** ادمین محصول/دسته/برند می‌سازد/ویرایش/غیرفعال می‌کند، آپلود عکس کار می‌کند، تغییرات در فروشگاه عمومی دیده می‌شود. build سبز.

**🧱 Boundary list:**
- `src/MahanShop.Application/Features/Admin/Products/**`, `/Categories/**`, `/Brands/**`
- `src/MahanShop.Web/Areas/Admin/Pages/Products/**`, `/Categories/**`, `/Brands/**`
- فایل‌های آپلودشده در `wwwroot/AdminPanel/Photo/**` (gitignore شود، داده dev)
- سرویس آپلود (اگر جدا): `src/MahanShop.Web/Services/ImageUploadService.cs`

**🔙 rollback این مرحله:** حذف پوشه‌های Boundary بالا + لینک‌های منوی این بخش در sidebar. **بدون migration** در این مرحله (فقط روی موجودیت‌های فعلی کار می‌کند).

---

## مرحله ۳ — Variant / موجودی + مدیریت ویژگی‌ها (وابسته به Phase 8.5)

> ⚠️ این مرحله به سیستم Variant (`docs/VARIANTS.md`) نیاز دارد. اگر Phase 8.5 هنوز انجام نشده، یا اول آن را بساز یا این مرحله را به نسخهٔ سادهٔ «موجودی per-product» محدود کن. تصمیم در `ADMIN_PROGRESS.md` ثبت شود.

**هدف:** مدیریت گزینه‌های فروش و موجودی + ویژگی‌ها/تگ‌ها.

**خروجی:**
- `Features/Admin/Variants/`: CRUD `VariantAttribute` + `VariantAttributeValue` (pool).
- ویرایشگر محصول، تب «گزینه‌ها/موجودی»: گرید `ProductVariant` (Sku/قیمت/تخفیف/موجودی/فعال)، ویرایش سریع inline موجودی، toggle `Product.HasVariants`.
- `Features/Admin/Features/` (ویژگی‌ها) + `Features/Admin/Tags/` و اتصالشان به محصول.
- migration افزایشی در صورت نیاز (نام `Add_Admin_*`)، **بدون دست‌زدن به migrationهای قبلی**.

**Done:** محصول چندگزینه‌ای با موجودی مستقل از پنل قابل ثبت/ویرایش است؛ ویژگی/تگ مدیریت می‌شود؛ قیمت همیشه سمت سرور. build سبز + smoke.

**🧱 Boundary list:**
- `src/MahanShop.Application/Features/Admin/Variants/**`, `/Features/**`, `/Tags/**`
- `src/MahanShop.Web/Areas/Admin/Pages/Variants/**` + تب گزینه‌ها در صفحهٔ Edit محصول
- migrationهای جدید `Add_Admin_*` در `src/MahanShop.Infra.Data/Migrations/**`

**🔙 rollback این مرحله:** `dotnet ef migrations remove` برای migrationهای این مرحله (به‌ترتیب معکوس) + حذف پوشه‌های Boundary. اگر روی DB اعمال شده: اول `database update` به migration قبلی، بعد remove.

---

## مرحله ۴ — سفارش‌ها + کاربران + داشبورد آمار (تکمیل)

**هدف:** مدیریت عملیاتی فروشگاه و کامل‌کردن داشبورد.

**خروجی:**
- `Features/Admin/Orders/`: `GetOrdersQuery` (فیلتر وضعیت/تاریخ)، `GetOrderDetailQuery` (ادمین)، `ChangeOrderStatusCommand`، دانلود PDF فاکتور (reuse `InvoicePdfService`).
- `Features/Admin/Users/`: لیست/جزئیات کاربر، toggle `IsAdmin`/فعال، مدیریت آدرس‌ها.
- `Features/Admin/Dashboard/`: `GetDashboardStatsQuery` (فروش/تعداد سفارش/موجودی کم/سفارش‌های اخیر) → پرکردن داشبورد مرحله ۱.
- صفحات Razor مربوطه + اتصال نهایی منو.

**Done:** ادمین سفارش‌ها را می‌بیند/وضعیت تغییر می‌دهد/فاکتور می‌گیرد، کاربران را مدیریت می‌کند، داشبورد آمار واقعی نشان می‌دهد. **Phase 9 کامل.** build سبز + smoke کامل.

**🧱 Boundary list:**
- `src/MahanShop.Application/Features/Admin/Orders/**`, `/Users/**`, `/Dashboard/**`
- `src/MahanShop.Web/Areas/Admin/Pages/Orders/**`, `/Users/**` + بدنهٔ داشبورد `Index`

**🔙 rollback این مرحله:** حذف پوشه‌های Boundary + برگرداندن `Index.cshtml` داشبورد به نسخهٔ placeholder مرحله ۱. بدون migration (روی موجودیت‌های موجود).

---

## نقشهٔ بازگشت سریع (Rollback matrix)

| می‌خواهم برگردم به… | دستور |
|---|---|
| کاملاً قبل از پنل ادمین (baseline) | `git reset --hard baseline-before-admin-panel` |
| فقط مرحلهٔ آخر را بردارم | `git revert <commit آن مرحله>` یا `git reset --hard <commit مرحلهٔ قبل>` |
| بدون git (دستی) | لیست‌های Boundary را از مرحلهٔ ۴ به ۱ حذف کن + بلوک‌های نشان‌دار `Program.cs` را بردار + migrationهای `Add_Admin_*` را remove کن |

> هر مرحله که در `ADMIN_PROGRESS.md` «DONE» علامت خورد، hash commit و migrationهایش هم آنجا ثبت می‌شود تا rollback دقیق ممکن باشد.

---

## Cross-cutting (هر مرحله رعایت)
- build سبز پایان هر مرحله + commit مستقل با پیشوند `admin(pN):`.
- domestic-only: همهٔ asset ادمین self-host در `wwwroot/admin` (صفر CDN).
- دسترسی فقط `IsAdmin` (policy `AdminOnly`) روی کل Area.
- anti-forgery روی همهٔ POST (گلوبال فعال است).
- آپلود فایل: whitelist پسوند + content-type + حد اندازه.
- پایان هر مرحله: `docs/ADMIN_PROGRESS.md` آپدیت (چه شد، hash commit، migrationها، قدم بعد).
