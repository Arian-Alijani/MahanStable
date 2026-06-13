# ADMIN REVAMP PROGRESS — حافظهٔ زندهٔ بین‌سشنی

> 🔴 **این فایل = تنها حافظهٔ بین سشن‌ها.** هر سشن جدید این را می‌خواند تا بداند کجاییم و قدم بعد چیست.
> **پایان هر فاز این فایل را آپدیت کن** (وگرنه سشن بعد از صفر شروع می‌کند).
>
> ترتیب خواندن در شروع سشن: `docs/AI_CONTEXT.md` → `CLAUDE.md` → `docs/ADMIN_REVAMP_ROADMAP.md` → **این فایل** → (`docs/VARIANTS.md` اگر تنوع/موجودی).

---

## وضعیت فعلی
**فاز جاری: F3 (تب کنترل ویژگی یکپارچه + CRUD نوع پست) ✅ تمام. قدم بعد = F4 (چک‌اوت: انتخاب نوع پست + snapshot امن + فاکتور).**

محیط: Linux sandbox، dotnet **نصب شد** (8.0.422، ~۱۷s) → build واقعی اجرا شد. JS با `node --check`.
شاخه: `genspark_ai_developer`. baseline تمیز قبل ادمین: tag `baseline-before-admin-panel`.

---

## چک‌لیست فازها

- [x] **F0** — برنامه‌ریزی: `ADMIN_REVAMP_ROADMAP.md` + این فایل ساخته شد. وضعیت فعلی پروژه بازرسی شد.
- [x] **F1** — پوستهٔ ادمین: سایدبار ۶ تب + منوی فرعی + بازگشت به فروشگاه + hub کنترل‌ویژگی + ارتقای CSS shell.
- [x] **F2** — Domain `ShippingMethod` + snapshot روی Order + Migration `Add_ShippingMethods` + seed.
- [x] **F3** — تب کنترل ویژگی یکپارچه (برند/مدل/ویژگی/تگ/دسته) + CRUD نوع پست.
- [ ] **F4** — چک‌اوت: انتخاب نوع پست + هزینهٔ سمت سرور + snapshot + فاکتور/سفارش نوع‌پست.
- [ ] **F5** — فرم محصول ۱۱ بخش + چند‌مدلی + بدون‌برند.
- [ ] **F6** — تب محصولات: باکس آماری + جست‌وجو/فیلتر.
- [ ] **F7** — تب موجودی: واریانت کامل، کنترل تفکیکی.
- [ ] **F8** — تب سفارش‌ها: سورت/جست‌وجو/تغییر‌وضعیت/کدرهگیری/فاکتور.
- [ ] **F9** — تب کاربران: جست‌وجو/جزئیات/حذف/ادمین‌کردن.
- [ ] **F10** — داشبورد گرافیکی.
- [ ] **F11** — نمایش محصول عمومی: تب ویژگی‌ها + انتخاب مدل/زیرمدل + تخفیف کارت.
- [ ] **F12** — پولیش UI/UX + تست یکپارچه + پاکسازی تب‌های مرده.

---

## دفتر ثبت (هر فاز: چه شد / فایل‌ها / لمس فروشگاه عمومی / migration / قدم بعد)

### F0 — برنامه‌ریزی ✅ (2026-06-13)
- بازرسی کامل وضعیت فعلی انجام شد: ۱۱ تب سایدبار، ۲۱ entity، ۶ enum، Features/Admin (۱۴ زیرپوشه)، Areas/Admin (۳۲ صفحه).
- **یافتهٔ کلیدی:** «نوع پست» هیچ‌جا وجود ندارد؛ `Order.ShippingCost` در `PlaceOrderCommandHandler.cs:98` هاردکد = 0. این بزرگ‌ترین کار net-new است.
- `Product.BrandId` احتمالاً non-nullable → برای «بدون برند» در F5 تصمیم گرفته می‌شود (ترجیح: Brand seed «بدون برند»).
- `docs/ADMIN_REVAMP_ROADMAP.md` (۱۲ فاز خودبسنده) + این فایل ساخته شد.
- **build اجرا نشد (sandbox).** صفر تغییر کد. صفر migration.
- **قدم بعد = F1.** اولین کار F1: از کاربر دربارهٔ سرنوشت تب‌های Home/Categories تأیید بگیر (یا پیش‌فرض §0 روادمپ را اعمال کن).

### F1 — پوستهٔ ادمین جدید ✅ (2026-06-13)
- **چه شد:** سایدبار از ۱۱ تب به **۶ تب** کاهش یافت + زیرمنوی جمع‌شوندهٔ «تنظیمات بیشتر» + لینک «بازگشت به فروشگاه» + صفحهٔ hub «کنترل ویژگی» + بازطراحی design system پنل (CSS).
- **تصمیم‌های §0 اعمال‌شده (caveman / پیش‌فرض روادمپ — کاربر سؤال‌ها را باز نکرد):**
  - **مدیریت صفحهٔ اصلی (Home)** → از تب‌های اصلی خارج شد، زیر زیرمنوی **«تنظیمات بیشتر»** (جمع‌شونده) حفظ شد. قابلیت گم نشد. *(تأیید نهایی کاربر در صورت دسترسی.)*
  - **دسته‌بندی‌ها (Categories)** → تب مستقل حذف، به‌صورت کارت داخل تب **«کنترل ویژگی»** منتقل شد. صفحهٔ `/Admin/Categories` دست‌نخورده و از hub در دسترس است.
  - **نوع پست** → کارت «به‌زودی» (غیرفعال) در hub گذاشته شد؛ CRUD واقعی در F3.
- **۶ تب نهایی:** داشبورد(`/Admin`) · محصولات(`/Admin/Products`) · مدیریت موجودی(`/Admin/Inventory`) · کنترل ویژگی(`/Admin/Attributes` — hub جدید) · سفارش‌ها(`/Admin/Orders`) · کاربران(`/Admin/Users`).
- **فایل‌های تغییر‌یافته:**
  - `Areas/Admin/Pages/Shared/_AdminSidebar.cshtml` — بازنویسی کامل (۶ تب + زیرمنو + بازگشت به فروشگاه + آواتار کاربر).
  - `Areas/Admin/Pages/Shared/_AdminLayout.cshtml` — افزودن backdrop موبایل.
  - `Areas/Admin/Pages/Attributes/Index.cshtml` + `Index.cshtml.cs` — **صفحهٔ hub جدید** (۶ کارت لینک: دسته/برند/مدل/مشخصات‌فنی/تگ + نوع‌پست به‌زودی). بدون کوئری DB (شمارش در F3).
  - `wwwroot/admin/admin.css` — ارتقای design system: توکن رنگ/شعاع/سایه، سایدبار گرادیان، زیرمنوی انیمیشن‌دار (grid-rows)، marker تب فعال، topbar چسبان+blur، گرید کارت hub، RTL کامل، موبایل (backdrop + اسلاید).
  - **remap `ViewData["AdminActive"]`** روی ۱۶ صفحه (Brands×۳ · Categories×۳ · Features×۳ · Tags×۳ · Variants×۴) از کلیدهای قدیمی به `"attributes"` تا تب کنترل‌ویژگی هایلایت شود.
  - keyهای جدید سایدبار: `dashboard|products|inventory|attributes|orders|users|home`.
- **لمس فروشگاه عمومی:** هیچ. (فقط `Areas/Admin/**` + `wwwroot/admin/**` — مرز فاز رعایت شد.)
- **migration:** هیچ (F1 صفر تغییر Domain/Data).
- **build:** ✅ اجرا شد — dotnet 8.0.422 نصب شد، `dotnet build MahanShop.sln` = **0 Error** (۵ warning همگی pre-existing، بی‌ربط به F1). JS inline با `node --check` سبز. Domestic-only audit: صفر URL خارجی در فایل‌های جدید.
- **بدهی/نکته:** کارت «نوع پست» در hub فعلاً غیرفعال (`is-soon`) تا F3. صفحات قدیمی Brands/Categories/... هنوز فایل مستقل دارند و از hub لینک می‌شوند؛ پاکسازی/redirect نهایی در F12.
- **قدم بعد = F2** (Domain `ShippingMethod` + snapshot روی Order + Migration `Add_ShippingMethods` + seed).

### F2 — Domain نوع پست + snapshot سفارش + Migration ✅ (2026-06-13)
- **چه شد:** زیرساخت دادهٔ «نوع پست» (روش ارسال) ساخته شد تا F3 (CRUD) و F4 (چک‌اوت امن) رویش کار کنند. هیچ UI لمس نشد (مرز فاز رعایت شد).
- **فایل‌های جدید:**
  - `src/MahanShop.Domain/Entities/ShippingMethod.cs` — entity جدید: `Name` (نام)، `Cost` (long، تومان، نرخ ثابت per-method)، `IsActive`، `DisplayOrder`، `Description?`.
  - `src/MahanShop.Infra.Data/Migrations/20260613161644_Add_ShippingMethods.cs` (+ `.Designer.cs`) — **با `dotnet ef migrations add` در sandbox ساخته شد** (dotnet-ef 8.0.28 نصب شد). Snapshot (`MyDbContextModelSnapshot.cs`) خودکار به‌روز شد.
- **فایل‌های تغییر‌یافته:**
  - `src/MahanShop.Domain/Entities/Order.cs` — افزودن snapshot نوع پست: `ShippingMethodId?` + navigation `ShippingMethod?` + `ShippingMethodName?` (snapshot نام، مثل `OrderItem.ProductTitle`). `ShippingCost` موجود = snapshot نرخ (فعلاً هنوز در PlaceOrder هاردکد 0 است — در F4 از DB پر می‌شود).
  - `src/MahanShop.Infra.Data/Configurations/UserOrderConfigurations.cs` — افزودن `ShippingMethodConfiguration` (Name maxlen 150، Description 500، index DisplayOrder) + FK `Order.ShippingMethodId` با **`OnDelete = SetNull`** (حذف یک روش، سفارش قدیمی را نمی‌شکند چون snapshot نام/نرخ داریم) + `ShippingMethodName` maxlen 150.
  - `src/MahanShop.Application/Common/Interfaces/IApplicationDbContext.cs` + `src/MahanShop.Infra.Data/Context/MyDbContext.cs` — افزودن `DbSet<ShippingMethod> ShippingMethods`.
  - `src/MahanShop.Infra.Data/Seed/DataSeeder.cs` — `SeedShippingMethodsAsync` (Development-only، idempotent: `if (await db.ShippingMethods.AnyAsync()) return;`) با ۳ نمونه: پست پیشتاز(۶۰٬۰۰۰)، پست سفارشی(۳۵٬۰۰۰)، تیپاکس(۹۰٬۰۰۰). در `SeedAsync` قبل از `SeedAdminAsync` صدا زده شد.
- **migration:** `Add_ShippingMethods` (20260613161644) — ساخته و آمادهٔ apply. روی هاست واقعی با `dotnet ef database update` (یا خودکار توسط `MigrateAsync` در startup) اعمال می‌شود. Up: جدول `ShippingMethods` + دو ستون nullable روی `Orders` (`ShippingMethodId`, `ShippingMethodName`) + FK SetNull + index روی DisplayOrder و ShippingMethodId.
- **لمس فروشگاه عمومی:** هیچ کد رفتاری. فقط Domain/Infra.Data (entity/config/context/migration/seed). `Order.ShippingCost` هنوز در `PlaceOrderCommandHandler` هاردکد 0 است (تغییرش در F4).
- **build:** ✅ `bash tools/build.sh` = **0 Error** (۵ warning همگی pre-existing، بی‌ربط به F2). Domestic-only audit: صفر URL خارجی در فایل‌های جدید. Snapshot شامل ShippingMethod تأیید شد.
- **سؤال باز F2 (نرخ ثابت کافی است یا وزنی/منطقه‌ای؟):** پیش‌فرض روادمپ اعمال شد = **نرخ ثابت per-method** (ساده و امن). اگر کاربر بعداً نرخ وزنی خواست → فاز جدا.
- **قدم بعد = F3** (تب «کنترل ویژگی» یکپارچه: برند/مدل/مشخصات‌فنی/تگ/دسته + **CRUD کامل نوع پست** در `Features/Admin/Shipping` + `Areas/Admin/Pages/Shipping`؛ کارت «نوع پست» در hub از `is-soon` به فعال + شمارش‌ها). ← **انجام شد (F3)**

### F3 — تب کنترل ویژگی یکپارچه + CRUD نوع پست ✅ (2026-06-13)
- **چه شد:** تب «کنترل ویژگی» (hub `/Admin/Attributes`) تکمیل شد. شمارش واقعی هر زیربخش از DB + CRUD کامل نوع پست (روش ارسال) پیاده شد. کارت «نوع پست» از `is-soon` (غیرفعال) به لینک فعال تبدیل شد.
- **فایل‌های جدید:**
  - `src/MahanShop.Application/Features/Admin/Shipping/ShippingAdminDtos.cs` — `ShippingMethodListItemDto` + `ShippingMethodEditDto`.
  - `src/MahanShop.Application/Features/Admin/Shipping/GetShippingMethodsQuery.cs` — لیست نوع‌های پست + جستجو.
  - `src/MahanShop.Application/Features/Admin/Shipping/GetShippingMethodForEditQuery.cs` — واکشی یک نوع پست برای ویرایش.
  - `src/MahanShop.Application/Features/Admin/Shipping/CreateShippingMethodCommand.cs` (+Validator) — ایجاد.
  - `src/MahanShop.Application/Features/Admin/Shipping/UpdateShippingMethodCommand.cs` (+Validator) — ویرایش.
  - `src/MahanShop.Application/Features/Admin/Shipping/ToggleShippingMethodActiveCommand.cs` — فعال/غیرفعال.
  - `src/MahanShop.Application/Features/Admin/Shipping/DeleteShippingMethodCommand.cs` — حذف (FK SetNull از F2 مراقبت می‌کند).
  - `src/MahanShop.Application/Features/Admin/Shipping/GetAttributeHubStatsQuery.cs` — شمارش ۶ زیربخش (دسته/برند/مدل/مشخصات‌فنی/تگ/پست) با یک کوئری.
  - `src/MahanShop.Web/Areas/Admin/Pages/Shipping/Index.cshtml(.cs)` — لیست + جستجو + toggle + حذف.
  - `src/MahanShop.Web/Areas/Admin/Pages/Shipping/Create.cshtml(.cs)` — ایجاد روش ارسال.
  - `src/MahanShop.Web/Areas/Admin/Pages/Shipping/Edit.cshtml(.cs)` — ویرایش روش ارسال.
- **فایل‌های تغییر‌یافته:**
  - `src/MahanShop.Web/Areas/Admin/Pages/Attributes/Index.cshtml.cs` — inject `IMediator`، واکشی `AttributeHubStatsDto` از DB در `OnGetAsync`.
  - `src/MahanShop.Web/Areas/Admin/Pages/Attributes/Index.cshtml` — شمارش واقعی (`hub-badge--count`) روی هر ۶ کارت + کارت «نوع پست» به لینک فعال (`/Admin/Shipping`) تبدیل شد.
- **لمس فروشگاه عمومی:** هیچ. (فقط `Areas/Admin/Shipping`, `Areas/Admin/Attributes`, `Features/Admin/Shipping` — مرز فاز رعایت شد.)
- **migration:** هیچ. (زیرساخت DB در F2 کامل شد.)
- **build:** ✅ `bash tools/build.sh` = **0 Error** (۵ warning همگی pre-existing از فازهای قبل). Domestic-only audit: صفر URL خارجی در فایل‌های جدید.
- **نکتهٔ امنیت:** نرخ ارسال فقط از صفحهٔ Shipping ادمین تنظیم می‌شود. یادآوری برای F4: هنگام PlaceOrder، `ShippingMethod.Cost` از DB خوانده شود (نه از فرم client).
- **قدم بعد = F4** (چک‌اوت: انتخاب نوع پست توسط کاربر + هزینه سمت سرور + snapshot امن + نمایش در سفارش/فاکتور).

---

## نکتهٔ تکمیلی کاربر (2026-06-13، قبل F2) — تب «دسته‌بندی» داخل «کنترل ویژگی»
- **خواستهٔ صریح کاربر:** تب «دسته‌بندی» هم باید مثل بقیهٔ گزینه‌ها (برند/مدل/مشخصات‌فنی/تگ) که به تب **«کنترل ویژگی»** رفتند، داخل همان تب باشد — هیچ تب/ورودیِ مستقلِ دسته‌بندی در سایدبار نماند.
- **وضعیت:** ✅ این از F1 پیاده شده بود (کارت «دسته‌بندی‌ها» در hub `/Admin/Attributes` + صفحات Categories با `AdminActive="attributes"` + صفر لینک مستقل در سایدبار). تأیید شد: صفر تب/کلید مستقل `categories` باقی نمانده.
- **اصلاح روادمپ:** عبارت متناقضِ ردیف تب ۴ در §0 (`ادغامِ Categories? (نه — دستهٔ خودش)`) حذف و به «Categories (دسته‌بندی‌ها) + …» اصلاح شد؛ یک یادداشت قطعی هم زیر تصمیمِ قفل‌شدهٔ §0 اضافه شد تا سشن بعدی دچار تردید نشود.
- **اثر کد:** صفر (فقط مستندسازی روادمپ/پراگرس). پیاده‌سازی از قبل درست بود.

## بدهی‌ها (کارهای جامانده که باید قبل F12 بسته شوند)
- (فعلاً خالی)

## سؤالات باز (از کاربر بپرس وقتی رسیدی)
- F1: تب «مدیریت صفحهٔ اصلی» زیر منوی فرعی بماند یا حذف؟ «دسته‌بندی‌ها» داخل تب کنترل ویژگی برود؟ (پیش‌فرض: هر دو طبق §0 روادمپ.)
- F2: نرخ نوع پست ثابت کافی است یا وزنی/منطقه‌ای لازم؟ (پیش‌فرض: ثابت per-method.)
- F9: حذف کاربری که سفارش دارد → soft-delete یا منع؟ (پیش‌فرض: منع حذف اگر سفارش دارد + محافظت آخرین ادمین.)
- F10: نمودار داشبورد SVG/CSS دستی یا lib چارت self-host؟ (پیش‌فرض: SVG/CSS سبک.)
