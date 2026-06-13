# ADMIN REVAMP PROGRESS — حافظهٔ زندهٔ بین‌سشنی

> 🔴 **این فایل = تنها حافظهٔ بین سشن‌ها.** هر سشن جدید این را می‌خواند تا بداند کجاییم و قدم بعد چیست.
> **پایان هر فاز این فایل را آپدیت کن** (وگرنه سشن بعد از صفر شروع می‌کند).
>
> ترتیب خواندن در شروع سشن: `docs/AI_CONTEXT.md` → `CLAUDE.md` → `docs/ADMIN_REVAMP_ROADMAP.md` → **این فایل** → (`docs/VARIANTS.md` اگر تنوع/موجودی).

---

## وضعیت فعلی
**فاز جاری: F8 (تب «سفارش‌ها») ✅ تمام. قدم بعد = F9 (تب «کاربران»: جست‌وجو + حذف + ادمین‌کردن + جزییات+سفارش‌ها).**

محیط: Linux sandbox، dotnet **نصب شد** (8.0.422، ~۱۷s) → build واقعی اجرا شد. JS با `node --check`.
شاخه: `genspark_ai_developer`. baseline تمیز قبل ادمین: tag `baseline-before-admin-panel`.

---

## چک‌لیست فازها

- [x] **F0** — برنامه‌ریزی: `ADMIN_REVAMP_ROADMAP.md` + این فایل ساخته شد. وضعیت فعلی پروژه بازرسی شد.
- [x] **F1** — پوستهٔ ادمین: سایدبار ۶ تب + منوی فرعی + بازگشت به فروشگاه + hub کنترل‌ویژگی + ارتقای CSS shell.
- [x] **F2** — Domain `ShippingMethod` + snapshot روی Order + Migration `Add_ShippingMethods` + seed.
- [x] **F3** — تب کنترل ویژگی یکپارچه (برند/مدل/ویژگی/تگ/دسته) + CRUD نوع پست.
- [x] **F4** — چک‌اوت: انتخاب نوع پست + هزینهٔ سمت سرور + snapshot + فاکتور/سفارش نوع‌پست.
- [x] **F5a** — فرم محصول بخش‌های ۱-۹ + گالری: تب‌بندی ۵تایی + بدون‌برند + مشخصات‌فنی inline.
- [x] **F5b** — بخش ۱۰ «چند‌مدلی»: wizard واریانت کامل در تب «مدل‌ها».
- [x] **F6** — تب محصولات: باکس آماری + جست‌وجو/فیلتر.
- [x] **F7** — تب موجودی: product-group view، واریانت کامل، کنترل تفکیکی.
- [x] **F8** — تب سفارش‌ها: سورت/جست‌وجو/تغییر‌وضعیت/کدرهگیری/فاکتور.
- [ ] **F9** — تب کاربران: جست‌وجو/جزئیات/حذف/ادمین‌کردن.
- [ ] **F10** — داشبورد گرافیکی.
- [ ] **F11** — نمایش محصول عمومی: تب ویژگی‌ها + انتخاب مدل/زیرمدل + تخفیف کارت.
- [ ] **F12** — پولیش UI/UX + تست یکپارچه + پاکسازی تب‌های مرده.

---

## دفتر ثبت (هر فاز: چه شد / فایل‌ها / لمس فروشگاه عمومی / migration / قدم بعد)

### F8 — تب «سفارش‌ها» (سورت/آمار/جست‌وجو/کدرهگیری/جزییات) ✅ (2026-06-13)
- **چه شد:** تب «سفارش‌ها» از لیست ساده به یک کنسول کامل مدیریت سفارش بازطراحی شد.
- **تصمیم‌های طراحی:**
  - **سورت چهارگانه:** `OrderSortOption` enum با Newest (پیش‌فرض)/Oldest/AmountDesc/AmountAsc. لینک‌های کلیک‌پذیر روی سرستون‌های «تاریخ» و «مبلغ».
  - **کارت‌های آمار ۷‌تایی:** Total/Pending/Paid/Processing/Shipped/Delivered/Canceled — هر کارت فیلتر سریع است. آمار روی همان query فیلترشده محاسبه می‌شود.
  - **کد رهگیری مستقل:** `UpdateOrderTrackingCommand` جدید برای ثبت/ویرایش کد رهگیری بدون تغییر وضعیت — handler `OnPostTrackingAsync` در Detail. انعکاس خودکار در پنل کاربر (`Order.TrackingCode` خوانده می‌شود).
  - **Progress bar وضعیت:** در صفحه جزییات نوار گرافیکی Paid→Processing→Shipped→Delivered با حالت‌های done/active/pending. برای لغو و در انتظار، notice رنگی جداگانه.
  - **فرم تغییر وضعیت:** حفظ `ChangeOrderStatusCommand` + bind جداگانه `StatusTrackingCode` برای ثبت همزمان.
  - **ستون روش ارسال:** `ShippingMethodName` به `AdminOrderListItemDto` و پروجکشن `GetOrdersQuery` اضافه شد.
  - **چیپس فیلتر فعال:** جست‌وجو/وضعیت/بازه تاریخ با لینک حذف تفکیکی.
  - **تغییر وضعیت → سمت کاربر:** `Order.Status` در هر دو پنل کاربر و ادمین از همان جدول خوانده می‌شود → تغییر ادمین خودکار منعکس است (صفر تغییر لازم نبود).
  - **فاکتور:** کامل است از F4 (ShippingMethodName در PDF) — در این فاز دکمه فاکتور در detail بهتر طراحی شد.
- **فایل‌های جدید:**
  - `src/MahanShop.Application/Features/Admin/Orders/UpdateOrderTrackingCommand.cs` — command + validator جدید.
- **فایل‌های تغییریافته:**
  - `src/MahanShop.Application/Features/Admin/Orders/GetOrdersQuery.cs` — سورت + آمار.
  - `src/MahanShop.Application/Features/Admin/Orders/OrderAdminDtos.cs` — `AdminOrderStatsDto` + `OrderSortOption` + `ShippingMethodName`.
  - `src/MahanShop.Web/Areas/Admin/Pages/Orders/Index.cshtml.cs` — Sort + HasFilter.
  - `src/MahanShop.Web/Areas/Admin/Pages/Orders/Index.cshtml` — بازنویسی کامل.
  - `src/MahanShop.Web/Areas/Admin/Pages/Orders/Detail.cshtml.cs` — handler رهگیری جدید.
  - `src/MahanShop.Web/Areas/Admin/Pages/Orders/Detail.cshtml` — بازنویسی کامل.
  - `src/MahanShop.Web/wwwroot/admin/admin.css` — بلوک `orders8-*`.
- **لمس فروشگاه عمومی:** هیچ. فقط `Areas/Admin/Orders`, `Features/Admin/Orders`, `wwwroot/admin`.
- **migration:** هیچ. صفر تغییر Domain/DB.
- **build:** ✅ `dotnet build` = **0 Error** (5 warning همگی pre-existing). `node --check` = 8 ok. JS tests = 10 passed. Domestic-only: صفر URL خارجی.
- **PR:** https://github.com/Arian-Alijani/MahanStable/pull/27
- **قدم بعد = F9** (تب «کاربران»: جست‌وجو + حذف (منع اگر سفارش دارد) + ادمین‌کردن/سلب (محافظت آخرین ادمین) + جزییات+سفارش‌ها).

### F7 — تب «مدیریت موجودی» (product-group view + کنترل تفکیکی) ✅ (2026-06-13)
- **چه شد:** تب «مدیریت موجودی» از نمای واریانت‌محور قدیمی به نمای **product-group** بازطراحی شد. هر ردیف یک محصول است. محصول ساده با ویرایش inline قیمت/تخفیف/موجودی، محصول واریانتی با expand قابل کلیک که جدول همهٔ واریانت‌هایش را با کنترل تفکیکی نشان می‌دهد.
- **تصمیم‌های طراحی:**
  - **product-group view:** جدول اصلی نشان‌دهندهٔ محصولات است نه واریانت‌ها. expand/collapse هر ردیف واریانتی با انیمیشن SVG.
  - **دو مسیر ویرایش inline:**
    - محصول ساده: `SetSimpleProductPriceCommand` (قیمت+تخفیف) + `SetSimpleProductStockCommand` (موجودی) — هر دو AJAX.
    - واریانت: `SetVariantPriceAndStockCommand` (قیمت+تخفیف+موجودی یکجا) + `AdjustVariantStockCommand` (دکمه‌های +/-).
  - **برند dropdown:** از جدول `Brands` (نه `VariantAttributeValues` مثل قبل) — چون موجودی از دیدگاه محصول است.
  - **نوع فیلتر:** `InventoryProductTypeFilter.Simple/Variant` اضافه شد.
  - **CSV export/import حفظ شد** (`ExportInventoryCsvQuery`/`ImportInventoryCsvCommand` دست‌نخورده).
- **فایل‌های جدید:**
  - `src/MahanShop.Application/Features/Admin/Inventory/GetInventoryProductsQuery.cs` — query اصلی product-group با فیلترهای جدید.
- **فایل‌های تغییریافته:**
  - `src/MahanShop.Application/Features/Admin/Inventory/InventoryDtos.cs` — `InventoryProductRowDto` + `InventoryVariantRowDto` + `InventoryProductsDto` + `InventoryProductTypeFilter` enum جدید؛ `InventoryBrandOptionDto.ValueId` → `BrandId`؛ DTOهای قدیمی (`InventoryRowDto`/`InventoryOverviewDto`) حفظ‌شده برای CSV.
  - `src/MahanShop.Application/Features/Admin/Inventory/InventoryCommands.cs` — سه command جدید: `SetSimpleProductPriceCommand` (+ Validator)، `SetSimpleProductStockCommand`، `SetVariantPriceAndStockCommand` (+ Validator).
  - `src/MahanShop.Application/Features/Admin/Inventory/GetInventoryOverviewQuery.cs` — `ValueId` → `BrandId` در projection `InventoryBrandOptionDto`.
  - `src/MahanShop.Web/Areas/Admin/Pages/Inventory/Index.cshtml.cs` — بازنویسی کامل: `GetInventoryProductsQuery` + handler‌های AJAX: `SetSimplePrice`, `SetSimpleStock`, `SetVariant`, `AdjustVariantStock`, `SetVariantStock` + CSV حفظ‌شده.
  - `src/MahanShop.Web/Areas/Admin/Pages/Inventory/Index.cshtml` — بازنویسی کامل: product-group view با فیلتر نوع/برند/موجودی؛ ردیف ساده inline؛ ردیف واریانتی expand → جدول داخلی واریانت‌ها؛ صفحه‌بندی/نوار هشدار.
  - `src/MahanShop.Web/wwwroot/admin/inventory.js` — بازنویسی کامل: expand/collapse با SVG arrow؛ ویرایش inline محصول ساده (قیمت/تخفیف/موجودی)؛ ویرایش inline واریانت؛ دکمه‌های +/-؛ blur/Enter حمایت؛ toast.
  - `src/MahanShop.Web/wwwroot/admin/admin.css` — افزودن CSS بلوک F7: `.inv7-*` کلاس‌ها (toolbar/filters/table/simple-row/variant-head/expand-btn/variants-row/variants-inner/variants-table/badge/active-dot/adj/…) + ریسپانسیو.
- **لمس فروشگاه عمومی:** هیچ. فقط `Areas/Admin/Inventory`, `Features/Admin/Inventory`, `wwwroot/admin`.
- **migration:** هیچ. صفر تغییر Domain/DB.
- **build:** ✅ `dotnet build MahanShop.sln` = **0 Error** (5 warning همگی pre-existing). `node --check inventory.js` سبز. Domestic-only audit: صفر URL خارجی.
- **قدم بعد = F8** (تب «سفارش‌ها»: سورت پیش‌فرض جدیدترین + سورت‌های دیگر + پنل جست‌وجو + تغییر وضعیت → انعکاس در سمت کاربر + ورود کد رهگیری + فاکتور دقیق).

### F6 — تب «محصولات» (باکس‌های آماری گرافیکی + پنل جست‌وجو/فیلتر) ✅ (2026-06-13)
- **چه شد:** تب «محصولات» از فازهای قبل پایهٔ قوی داشت (جدول/فیلتر/آمار). در این فاز موارد جامانده برای تکمیل DoD اضافه شد:
  1. **`ProductTypeFilter` (ساده/واریانتی):** enum جدید + فیلتر در `GetProductsQuery` برای فیلتر بر اساس `HasVariants`.
  2. **آمار کامل:** `ProductStatsDto` با فیلدهای `InStock` (موجودی کافی > آستانه) + `SimpleCount` + `VariantCount` تکمیل شد.
  3. **۶ کارت آماری:** کارت‌های بالای صفحه به ۶ کارت گسترش یافت: همه / موجود(کافی) / موجودی‌کم / ناموجود / چندمدلی / ساده. هر کارت = فیلتر سریع (لینک قابل‌کلیک).
  4. **Type filter dropdown:** انتخاب «ساده/واریانتی» به نوار فیلتر اضافه شد.
  5. **چیپ فیلتر فعال نوع:** چیپس‌های فیلتر فعال با `asp-route-Type` تکمیل شدند (هم چیپ «نوع» + هم تمام چیپس‌های قبلی با Type حفظ شد).
  6. **Pagination:** پارامتر `Type` به لینک‌های صفحه‌بندی اضافه شد.
  7. **CSS:** گرید ۵ستونی → ۶ستونی + رنگ `prod-stat--violet` برای کارت چندمدلی + بریک‌پوینت ۱۳۰۰ px.
- **فایل‌های تغییر‌یافته:**
  - `src/MahanShop.Application/Features/Admin/Products/GetProductsQuery.cs` — `ProductTypeFilter` enum + فیلتر `Type` در handler + `InStock`/`SimpleCount`/`VariantCount` در `BuildStatsAsync`.
  - `src/MahanShop.Application/Features/Admin/Products/ProductAdminDtos.cs` — `ProductStatsDto` با `InStock`, `SimpleCount`, `VariantCount`.
  - `src/MahanShop.Web/Areas/Admin/Pages/Products/Index.cshtml.cs` — `Type` property bind + پاس به query.
  - `src/MahanShop.Web/Areas/Admin/Pages/Products/Index.cshtml` — ۶ کارت آماری + Type filter dropdown + `Type` در همهٔ chip/pagination linkها + `HasFilter` با Type.
  - `src/MahanShop.Web/wwwroot/admin/admin.css` — گرید ۶ستونی + `prod-stat--violet` + بریک‌پوینت ۱۳۰۰.
- **لمس فروشگاه عمومی:** هیچ. فقط `Areas/Admin/Products` + `Features/Admin/Products` + `wwwroot/admin/admin.css`.
- **migration:** هیچ. صفر تغییر Domain/DB.
- **build:** ✅ `bash tools/build.sh` = **0 Error** (۵ warning همگی pre-existing از فازهای قبل). `bash tools/check-js.sh` = 8 ok, 0 bad, 10 tests passed. Domestic-only audit: صفر URL خارجی.
- **قدم بعد = F7** (تب «مدیریت موجودی»: پشتیبانی کامل واریانت‌ها، کنترل تفکیکی قیمت/تعداد هر زیرشاخه).

### F5b — بخش ۱۰ «چندمدلی» + wizard واریانت کامل ✅ (2026-06-13)
- **چه شد:** تب «مدل‌ها» در فرم افزودن محصول (Create) از placeholder به wizardِ کاملِ چندمدلی تبدیل شد و با تب‌بندیِ ۵تاییِ F5a کاملاً هماهنگ گردید. تب «مدل‌ها» در ویرایش (Edit) با افزودن اکشن «تبدیل به چندمدلی» و گِیت‌کردنِ گرید/فرمِ افزودن گزینه بهبود یافت.
- **🔴 باگِ بحرانیِ کشف‌شده و رفع‌شده:** `product-wizard.js` با `getElementById("product-wizard")` دنبال ریشه می‌گشت، اما `Create.cshtml` ریشه‌اش `id="product-create-form"` بود → **کل JS ویزارد بی‌اثر بود** (سوییچِ ساده/چندبرندی، باز/بستِ مدل‌های هر برند، و شمارندهٔ زندهٔ گزینه‌ها هیچ‌کدام کار نمی‌کرد). JS اصلاح شد تا هر دو شناسه را بپذیرد.
- **فایل‌های تغییر‌یافته:**
  - `src/MahanShop.Web/wwwroot/admin/product-wizard.js` — بازنویسی: (۱) ریشه = `product-wizard || product-create-form`؛ (۲) گوش‌دادن به **همهٔ** رادیوهای `wizardModeToggle` (نه فقط toggleِ فرمِ ساده) + هم‌گام‌سازیِ همهٔ گروه‌ها هنگام تغییر حالت؛ (۳) رویدادِ سفارشیِ `wizard:modechange` که به لایهٔ تب‌بندی خبر می‌دهد تا تبِ فعال را در فرمِ جدید نشان دهد؛ (۴) `setPaneDisabled` فیلدهای فرمِ غیرفعال را disable می‌کند (ضدّ ارسالِ ناخواسته).
  - `src/MahanShop.Web/Areas/Admin/Pages/Products/Create.cshtml` — بازنویسی: تب «مدل‌ها» حالا فعال است؛ سوییچِ «ساده/چندمدلی» داخل همین تب در **هر دو** فرم؛ فرمِ چندمدلی پنل‌های `tab-basic`/`tab-price`/`tab-models` خودش را دارد؛ انتخاب برند→مدل→رنگ + شمارندهٔ زنده داخل تب «مدل‌ها»؛ دکمه‌های پیمایش `data-goto-tab`؛ JS تب‌بندیِ صفحه روی فرمِ فعال (نه d-none) کار می‌کند و با `wizard:modechange` همگام می‌شود. متن‌ها از «چندبرندی» به «چندمدلی» یکدست شد. dropdown برند راهنمای «بدون برند» گرفت.
  - `src/MahanShop.Web/Areas/Admin/Pages/Products/Edit.cshtml` — تب «مدل‌ها»: اگر `HasVariants=false` فقط کارتِ راهنما + دکمهٔ «تبدیل به محصول چندمدلی» نشان داده می‌شود (گرید/فرمِ افزودن گزینه پنهان تا سردرگمی نشود)؛ اگر `true` همان گرید/افزودن/CSV/QuickStock/Toggle/Delete موجود.
  - `src/MahanShop.Web/Areas/Admin/Pages/Products/Edit.cshtml.cs` — handler جدید `OnPostEnableVariantsAsync` (محصول را با همان مقادیر فعلی + `HasVariants=true` از طریق `UpdateProductCommand` ذخیره می‌کند) + `TempData["ReturnTab"]="ep-variants"`.
- **بررسی Application (طبق روادمپ):** `CreateMultiBrandProductCommand`/`GetProductWizardDataQuery`/`ProductWizardDtos` بازبینی شدند و **بدون تغییر** هماهنگ با تب‌بندیِ جدیدند (همان نام‌های فیلدِ فرم: `ModelValueIds`/`ColorValueIds`/`BasePrice`/`BaseDiscountPrice`/`BaseStock`). Validatorها: `CreateProductCommand` و `CreateMultiBrandProductCommand` هر دو `Price/BasePrice` و `Stock/BaseStock` را `>= 0` می‌گیرند → برای `HasVariants=true` قیمت/موجودیِ سطحِ محصول اجباریِ مثبت نیست (مسیرِ چندمدلی اصلاً Price/Stock محصول را نمی‌فرستد و handler آن را `Stock=0` می‌گذارد). پس تغییرِ validator لازم نشد.
- **لمس فروشگاه عمومی:** هیچ. فقط `Areas/Admin/Products` + `wwwroot/admin/product-wizard.js` — مرز فاز رعایت شد.
- **migration:** هیچ. صفر تغییر Domain/DB.
- **build:** ✅ `bash tools/build.sh` = **0 Error** (۵ warning همگی pre-existing از فازهای قبل). JS: `bash tools/check-js.sh` = 10 passed, 0 failed + `node --check product-wizard.js` سبز. Domestic-only audit: صفر URL خارجی در فایل‌های تغییریافته.
- **قدم بعد = F6** (تب «محصولات»: باکس‌های آماری گرافیکی همه/موجود/ناموجود/موجودی‌کم + پنل جست‌وجو/فیلتر/سورت سمت سرور).

### F5a — فرم محصول بخش‌های ۱-۹ + گالری ✅ (2026-06-13)
- **چه شد:** فرم افزودن/ویرایش محصول با تب‌بندی ۵تایی بازطراحی شد. بخش‌های ۱ تا ۹ (اطلاعات پایه / قیمت و موجودی / مشخصات فنی / مدل‌ها placeholder / تصاویر) پیاده شدند. گزینهٔ «بدون برند» به dropdown برند اضافه شد. مشخصات فنی با CRUD inline (افزودن Feature+مقدار / حذف) در تب Edit فعال است.
- **تصمیم «بدون برند»:** Brand سیستمی با `Slug="no-brand"` و `IsActive=false` (seed idempotent) — `Product.BrandId` non-nullable باقی ماند، صفر migration لازم بود.
- **تب‌بندی Edit:** اطلاعات پایه / قیمت و موجودی / مشخصات فنی / مدل‌ها و گزینه‌ها / تصاویر. پس از هر POST، `TempData["ReturnTab"]` تب مربوطه را نگه می‌دارد و JS آن را restore می‌کند.
- **فایل‌های جدید:**
  - `src/MahanShop.Application/Features/Admin/Products/ProductFeatureCommands.cs` — `AddProductFeatureCommand` (+ Validator: upsert — اگر همان Feature قبلاً ثبت شده مقدارش آپدیت می‌شود) + `DeleteProductFeatureCommand`.
- **فایل‌های تغییر‌یافته:**
  - `src/MahanShop.Infra.Data/Seed/DataSeeder.cs` — `SeedNoBrandAsync` idempotent (slug=no-brand، IsActive=false، DisplayOrder=9999). قبل از `SeedCatalogAsync` صدا می‌شود.
  - `src/MahanShop.Application/Features/Admin/Products/ProductAdminDtos.cs` — `ProductFeatureItemDto` جدید + `List<ProductFeatureItemDto> Features` به `ProductEditDto`.
  - `src/MahanShop.Application/Features/Admin/Products/GetProductForEditQuery.cs` — Include و projection مشخصات فنی (FeatureName + Value) به `ProductEditDto.Features`.
  - `src/MahanShop.Web/Areas/Admin/Pages/Products/Create.cshtml` — بازنویسی کامل: تب‌بندی ۵تایی (۲ تب فعال + ۳ تب disabled-until-save) + چیدمان بخش‌های ۱-۹ + dropdown برند با گزینهٔ «بدون برند» + فرم چندبرندی (wizard موجود حفظ).
  - `src/MahanShop.Web/Areas/Admin/Pages/Products/Edit.cshtml` — بازنویسی کامل: تب‌بندی ۵تایی + تب «مشخصات فنی» با جدول inline و فرم افزودن + تب «تصاویر» + تب «مدل‌ها» (کد موجود واریانت/CSV منتقل شد) + JS hash-based tab restore.
  - `src/MahanShop.Web/Areas/Admin/Pages/Products/Edit.cshtml.cs` — بازنویسی: inject `GetFeaturesQuery` برای `AllFeatures` + `OnPostAddFeatureAsync` + `OnPostDeleteFeatureAsync` + `TempData["ReturnTab"]` روی همهٔ handlerها + `ProductFeatures`/`AllFeatures` properties.
  - `src/MahanShop.Web/wwwroot/admin/admin.css` — افزودن: `.prod-form-tabs`, `.prod-form-tab`, `.prod-form-pane`, `.feature-table`, `.feature-add-row`.
  - `docs/ADMIN_REVAMP_ROADMAP.md` — F5 به **F5a + F5b** تقسیم شد؛ جدول فازها، ماتریس خواسته‌ها، و شرح کامل هر دو فاز اضافه شد.
- **لمس فروشگاه عمومی:** هیچ. فقط `Areas/Admin/Products`, `Features/Admin/Products`, `Infra.Data/Seed`.
- **migration:** هیچ. Brand seed با `IsActive=false` — Domain/DB بدون تغییر ساختاری.
- **build:** ✅ `dotnet build MahanShop.sln` = **0 Error** (۵ warning همگی pre-existing از فازهای قبل). Domestic-only audit: صفر URL خارجی.
- **قدم بعد = F5b** (سشن بعدی) — تب «مدل‌ها» در فرم کامل شود: wizard انتخاب مدل→زیرمدل با `product-wizard.js` هماهنگ، `CreateMultiBrandProductCommand` بررسی، تب «مدل‌ها» در Edit (موجودی/افزودن گزینه) از placeholder به عملکرد کامل.



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
- **قدم بعد = F4** (چک‌اوت: انتخاب نوع پست توسط کاربر + هزینه سمت سرور + snapshot امن + نمایش در سفارش/فاکتور). ← **انجام شد (F4)**

### F4 — چک‌اوت + فاکتور با نوع پست (امن) ✅ (2026-06-13)
- **چه شد:** کاربر هنگام ثبت سفارش نوع پست را انتخاب می‌کند؛ هزینه از DB خوانده و snapshot می‌شود؛ در سفارش، صفحه‌ی سفارش کاربر، صفحه‌ی سفارش ادمین، و فاکتور PDF ثبت می‌شود. هاردکد `ShippingCost = 0` (خط ۹۸ قدیم) حذف شد.
- **فایل‌های جدید:**
  - `src/MahanShop.Application/Features/Cart/Queries/GetShippingMethods/GetShippingMethodsForCheckoutQuery.cs` — کوئری لیست روش‌های ارسال فعال (`IsActive`) مرتب‌شده بر اساس `DisplayOrder`.
- **فایل‌های تغییریافته:**
  - `src/MahanShop.Application/Features/Cart/Commands/PlaceOrder/PlaceOrderCommand.cs` — `ShippingMethodId` اضافه شد به record.
  - `src/MahanShop.Application/Features/Cart/Commands/PlaceOrder/PlaceOrderCommandValidator.cs` — `ShippingMethodId > 0` اعتبارسنجی شد.
  - `src/MahanShop.Application/Features/Cart/Commands/PlaceOrder/PlaceOrderCommandHandler.cs` — روش ارسال از DB خوانده + اعتبارسنجی فعال‌بودن + snapshot `ShippingMethodId/ShippingMethodName/ShippingCost=method.Cost` + `FinalAmount = payable + shippingCost`.
  - `src/MahanShop.Application/Features/Account/Queries/GetOrderDetail/GetOrderDetailQuery.cs` — `ShippingMethodName` به `OrderDetailDto` + به projection اضافه شد.
  - `src/MahanShop.Application/Features/Admin/Orders/GetAdminOrderDetailQuery.cs` — `ShippingMethodName` به projection اضافه شد.
  - `src/MahanShop.Web/Models/Checkout/CheckoutViewModels.cs` — `ShippingMethods` + `SelectedShippingMethodId` + `SelectedShippingCost` + `FinalPayableAmount` اضافه شد.
  - `src/MahanShop.Web/Controllers/CheckoutController.cs` — بارگذاری `ShippingMethods` از DB در `BuildVm`؛ دریافت `shippingMethodId` از فرم `Place`؛ پاس به `PlaceOrderCommand`.
  - `src/MahanShop.Web/Views/Checkout/Index.cshtml` — بازنویسی: بخش انتخاب روش ارسال (radio + قیمت) + `hidden shippingMethodId` در فرم `place-form` + JS live-update مبلغ پرداختی در خلاصه سفارش بدون reload.
  - `src/MahanShop.Web/wwwroot/css/cart.css` — استایل‌های `.shipping-item` + `.shipping-item--selected` + `.shipping-item__cost` + `.shipping-item__free`.
  - `src/MahanShop.Web/Services/InvoicePdfService.cs` — ردیف «ارسال ({ShippingMethodName})» در فاکتور PDF (با fallback به «هزینه ارسال» اگر name خالی).
  - `src/MahanShop.Web/Pages/Panel/OrderDetail.cshtml` — نمایش نام روش ارسال در خلاصه مبلغ.
  - `src/MahanShop.Web/Areas/Admin/Pages/Orders/Detail.cshtml` — نمایش نام روش ارسال در جدول اقلام.
- **امنیت:** نرخ ارسال فقط از DB (`method.Cost`) snapshot می‌شود — هیچ مبلغی از client پذیرفته نمی‌شود. `ShippingMethodId` جعلی/غیرفعال → خطا صریح (`Fail("...")`).
- **لمس فروشگاه عمومی (صریح):** `CheckoutController`, `Views/Checkout/Index.cshtml`, `Cart/Commands/PlaceOrder/*`, `Cart/Queries/GetShippingMethods/`, `Services/InvoicePdfService.cs`, `Pages/Panel/OrderDetail.cshtml` — تمامی در مرز مجاز «استثناهای ناگزیر» روادمپ (قانون ۲).
- **migration:** هیچ. (زیرساخت DB در F2 کامل شد؛ `ShippingMethodId?`/`ShippingMethodName?` قبلاً به `Orders` اضافه شده.)
- **build:** ✅ `bash tools/build.sh` = **0 Error** (5 warning همگی pre-existing). JS: `bash tools/check-js.sh` = 10 passed, 0 failed. Domestic-only audit: صفر URL خارجی.
- **قدم بعد = F5** (بازطراحی فرم افزودن/ویرایش محصول با ۱۱ بخش + چند‌مدلی + بدون‌برند).

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
