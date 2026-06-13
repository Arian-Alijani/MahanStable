# ADMIN REVAMP ROADMAP — بازطراحی بزرگ پنل ادمین + ساختار محصولات

> **این فایل = منبع حقیقتِ کاملِ این بازطراحی. هر فاز در یک سشن جدا اجرا می‌شود و مدل به حافظهٔ سشن قبل دسترسی ندارد.**
> پس هر فاز اینجا **کامل و خودبسنده** نوشته شده: دقیقاً چه چیزی، در چه فایلی، چطور.
>
> 🔴 **شروع هر سشنِ این کار (اجباری به همین ترتیب):**
> 1. `docs/AI_CONTEXT.md` — نقشهٔ کل پروژه (کجا چی هست)
> 2. `CLAUDE.md` — قانون‌های آهنین استک (net8.0 / Domestic-only / Razor / Plesk …)
> 3. **این فایل** (`docs/ADMIN_REVAMP_ROADMAP.md`) — کلِّ نقشهٔ بازطراحی
> 4. `docs/ADMIN_REVAMP_PROGRESS.md` — چه فازی تمام شده، الان کجاییم، قدم بعدی دقیق چیست
> 5. در صورت کار روی تنوع/موجودی: `docs/VARIANTS.md`
>
> ⚠️ **محیط sandbox = Linux و dotnet نصب نیست** → build/test اینجا اجرا نمی‌شود. سینتکس C# را با دقت بنویس، سینتکس JS را با `node --check` چک کن. در PROGRESS بنویس «build اجرا نشد (sandbox)».

---

## 0. هدف کلان (چرا این کار)

دو دستهٔ تغییر بزرگ، که باید **جز‌به‌جز و بدون از دست رفتن حتی یک مورد** اعمال شوند:

### الف) بازطراحی ساختار پنل ادمین
هدف: **کاهش تعداد تب‌ها، کاهش شدید پیچیدگی، آسان‌سازی استفاده، و ارتقای UI/UX.**
از ۱۱ تب فعلی (Dashboard, Home, Products, Categories, Brands, Inventory, Variants, Features, Tags, Orders, Users) به **۶ تب** + لینک بازگشت به فروشگاه:

| # | تب جدید | از چه چیزی ساخته می‌شود |
|---|---------|--------------------------|
| 1 | **داشبورد** | بازطراحی گرافیکی Dashboard فعلی |
| 2 | **محصولات** | Products فعلی + باکس‌های آماری گرافیکی + پنل جست‌وجو/فیلتر |
| 3 | **مدیریت موجودی** | Inventory فعلی، بازنویسی برای پشتیبانی کامل واریانت‌ها (هر زیرشاخه قابل کنترل) |
| 4 | **کنترل ویژگی** | **Categories (دسته‌بندی‌ها)** + Brands + Variants(مدل‌ها) + Features(مشخصات فنی) + Tags + **نوع پست (جدید)** در یک تب با زیربخش‌ها |
| 5 | **سفارش‌ها** | Orders فعلی + سورت پیشرفته + جست‌وجو + کد رهگیری پستی + فاکتور |
| 6 | **کاربران** | Users فعلی + جست‌وجو + حذف + ادمین‌کردن + جزئیات+سفارش‌ها |
| — | **بازگشت به فروشگاه** | لینک انتهای سایدبار |

> **تصمیم مهم دربارهٔ تب «صفحه اصلی» (Home) و «دسته‌بندی‌ها» (Categories):**
> کاربر فقط ۶ تب نام برد و Home/Categories را نام نبرد. اما حذف کاملشان قابلیت از دست می‌رود.
> **تصمیم قفل‌شده (مگر کاربر خلافش بگوید):**
> - **دسته‌بندی‌ها** → به‌عنوان **زیربخش داخل تب «کنترل ویژگی»** منتقل می‌شود (چون ماهیتاً «تنظیمات پایه» است، هم‌خانوادهٔ برند/مدل/ویژگی). تب مستقل حذف.
>   🔴 **نکتهٔ تکمیلیِ صریحِ کاربر (قطعی — تردید نکن):** تب «دسته‌بندی» هم باید **دقیقاً مثل بقیهٔ گزینه‌ها (برند/مدل/مشخصات‌فنی/تگ)** که به تب «کنترل ویژگی» رفتند، داخل همان تب باشد. هیچ تب/ورودیِ مستقلِ «دسته‌بندی» در سایدبار باقی نمی‌ماند؛ تنها مسیر دسترسی، کارت/زیربخشِ «دسته‌بندی‌ها» داخل hub «کنترل ویژگی» است. (هرجای روادمپ که با این در تناقض بود اصلاح شد.)
> - **مدیریت صفحهٔ اصلی (Home)** → چون قابلیت سنگینی است (بنر/Hero/گرید دسته/ردیف محصول) و کاربر در این بازطراحی اشاره‌ای به آن نکرد، **به‌صورت یک زیربخش/لینک داخل داشبورد یا یک ورودی کم‌برجسته حفظ می‌شود** تا قابلیت گم نشود ولی شلوغی اصلی برداشته شود. **این مورد در فاز ۱ از کاربر تأیید گرفته می‌شود؛ تا تأیید، صرفاً از تب‌های اصلی خارج و زیر یک منوی «تنظیمات بیشتر» می‌رود.**

### ب) بازطراحی ساختار محصولات (فرم افزودن/ویرایش + نمایش به کاربر)
فرم محصول باید دقیقاً این ۱۱ بخش را داشته باشد (جزئیات کامل در فاز ۵). این تغییر روی صفحهٔ محصولِ عمومی، کاتالوگ، سبد و چک‌اوت هم اثر دارد و باید با سازگاری کامل پیاده شود.

---

## 1. قانون‌های آهنینِ این بازطراحی (نقض = خرابی)

1. **net8.0 ثابت. بدون Node/SPA/Docker. Domestic-only** (صفر CDN/فونت خارجی/سرویس خارجی). همه asset self-host. (مرجع: CLAUDE.md)
2. **مرز فیزیکی کد ادمین** (برای rollback آسان) — فقط داخل:
   - `src/MahanShop.Web/Areas/Admin/**`
   - `src/MahanShop.Web/wwwroot/admin/**` و `wwwroot/AdminPanel/**`
   - `src/MahanShop.Application/Features/Admin/**`
   - بلوک‌های مارک‌دار `=== ADMIN-PANEL START/END ===` در `Program.cs`
   - **استثناهای ناگزیر این بازطراحی** (چون ساختار محصول/سفارش عوض می‌شود): Domain entity جدید، Migration جدید، و فایل‌های فروشگاه عمومی (Catalog/Cart/Checkout/Payment View+Feature) که نمایش محصول/نوع‌پست را لمس می‌کنند. هرجا فایل فروشگاه عمومی را لمس کردی، **در PROGRESS صریح بنویس** کدام فایل و چرا.
3. **قیمت/موجودی/هزینهٔ پست همیشه سمت سرور از DB.** هیچ مبلغی از client اعتماد نشود. کاربر نوع پست را انتخاب می‌کند ولی **نرخ از DB خوانده و در سفارش snapshot می‌شود** (غیرقابل دستکاری). در فاکتور هم نوع پست + هزینه دقیق ثبت شود.
4. **CQRS + MediatR**: هر عملیات = Command/Query + Handler (+Validator). Controller/Page فقط `_mediator.Send()`. الگو را از فایل‌های موجود کپی کن (`docs/AI_CONTEXT.md §4`).
5. **ViewModel/DTO جدا از Entity.** View هرگز Entity نمی‌گیرد. خروجی Razor auto-encode.
6. **anti-forgery گلوبال** روی همه POST. policy `AdminOnly` روی کل Area. read query → `AsNoTracking()`. async همه‌جا.
7. **migration**: نام بامعنی `Add_<X>`؛ migration اعمال‌شده را edit نکن؛ فقط افزایشی.
8. **بعد هر فاز**: `docs/ADMIN_REVAMP_PROGRESS.md` را آپدیت کن (دقیق: چه شد، چه فایل‌هایی، قدم بعد). کامیت روی `genspark_ai_developer` → PR به main → لینک PR بده.
9. **هیچ قابلیتی حذف نشود مگر اینجا صریح گفته شده باشد.** اگر تب جابه‌جا می‌شود، قابلیتش باید در جای جدید کامل کار کند.

---

## 2. وضعیت فعلی (snapshot — تا مدلِ سشنِ جدید بدون اسکن بداند چه دارد)

**Domain entities موجود** (`src/MahanShop.Domain/Entities/`):
`Product` (Title/Slug/ShortDescription/Description/Price/DiscountPrice/Stock/**HasVariants**/IsActive/IsFeatured/ViewCount/Meta*/BrandId/CategoryId + Images/Variants/Features/Tags/Comments) ·
`Brand` (Name/Slug/LogoUrl/IsActive/DisplayOrder) ·
`Category` (درختی self-ref ParentId، ShowInMenu/ShowOnHome) ·
`Feature` + `ProductFeature` (مشخصات فنی key/value) ·
`Tag` + `ProductTag` ·
`VariantAttribute` (Name/DisplayOrder/IsColor/**Kind**: Other/Brand/Model/Color) + `VariantAttributeValue` (Value/ColorHex/LogoUrl/**ParentValueId** سلسله‌مراتبی) ·
`ProductVariant` (ProductId/Sku/Price/DiscountPrice/Stock/IsActive/DisplayOrder/RowVersion) + `ProductVariantValue` (join variant↔attributeValue) ·
`Order` (OrderCode/UserId/AddressId/TotalAmount/DiscountAmount/**ShippingCost**/FinalAmount/Status/TrackingCode/PaidAt/RowVersion) + `OrderItem` (ProductId/VariantId?/ProductTitle/VariantTitle?/UnitPrice/Quantity snapshot) ·
`Payment` · `Address` · `User` (Phone/OTP/IsAdmin/IsActive) · `Banner` · `HomeSection` · `OtpCode` · `ProductComment` (بلااستفاده).

**Enums**: `OrderStatus` (Pending/AwaitingPayment/Paid/Processing/Shipped/Delivered/Canceled/Refunded) · `PaymentStatus` · `VariantAttributeKind` · `HomeSectionType` · `HomeProductSource` · `HomeCategoryStyle`.

**Admin Pages موجود** (`Areas/Admin/Pages/`): `Index`(داشبورد) · `Home/`(بنر/Hero/ProductRow) · `Products/`(Index/Create/Edit) · `Categories/` · `Brands/` · `Features/` · `Tags/` · `Inventory/Index` · `Variants/`(Index/Edit/Values/Create) · `Orders/`(Index/Detail) · `Users/`(Index/Detail) · `Shared/_AdminLayout`+`_AdminSidebar`.

**Admin Features موجود** (`Features/Admin/`): `Dashboard/GetDashboardStatsQuery` · `Products/`(CreateProduct, CreateMultiBrandProduct, UpdateProduct, GetProductsQuery, GetProductForEdit, GetProductWizardData, ToggleActive, ProductImageCommands, DTOs) · `ProductVariants/` · `Variants/` · `Categories/` · `Brands/` · `Features/` · `Tags/` · `Inventory/`(+CSV) · `Orders/`(ChangeStatus, GetOrders, GetDetail, DTOs) · `Users/` · `Home/` · `Common/SlugHelper`.

**شکاف‌های کلیدی که این بازطراحی باید پر کند:**
- ❌ **نوع پست (Shipping/PostType) اصلاً وجود ندارد.** `Order.ShippingCost` هست ولی در `PlaceOrderCommandHandler` خط ۹۸ **هاردکد = 0**. هیچ entity نوع پست، هیچ انتخاب کاربر، هیچ snapshot نوع پست در سفارش.
- ❌ سایدبار ۱۱ تب دارد (باید ۶ شود).
- ❌ تب محصولات باکس آماری گرافیکی (همه/موجود/ناموجود/موجودی‌کم) ندارد یا کامل نیست → باید بازطراحی شود.
- ❌ داشبورد گرافیکی غنی نیست.
- ⚠️ Inventory باید پشتیبانی کامل و واضح واریانت‌ها را بدهد.
- ⚠️ فرم محصول باید با ۱۱ بخش مشخص‌شده هم‌تراز شود.

---

## 3. نقشهٔ فازها (هر فاز = یک سشن)

> ترتیب عمداً «پایه‌ها اول، نمایش آخر» است تا هر فاز روی فاز قبل بنا شود و تک‌تک قابل تست باشند.
> در شروع هر فاز: PROGRESS را بخوان تا بدانی فاز قبل واقعاً تمام شده یا نه.

| فاز | عنوان | خروجی اصلی | وابسته‌به |
|-----|-------|-----------|-----------|
| **F0** | برنامه‌ریزی (همین سشن) | این فایل + PROGRESS + تأیید جهت | — |
| **F1** | پوستهٔ ادمین جدید: سایدبار ۶‌تبی + UI/UX shell + بازگشت به فروشگاه + جابه‌جایی Categories/Home | navigation جدید، صفحات موجود سرِ جای جدید | F0 |
| **F2** | Domain: نوع پست (ShippingMethod) + snapshot روی Order + Migration | entity + migration + seed | F1 |
| **F3** | تب «کنترل ویژگی» — یکپارچه‌سازی: برند/مدل(Variants)/مشخصات‌فنی/تگ/دسته + **مدیریت نوع پست** | یک تب با زیربخش‌ها، CRUD نوع پست | F2 |
| **F4** | چک‌اوت + فاکتور: انتخاب نوع پست توسط کاربر، snapshot امن، نمایش در فاکتور/سفارش | PlaceOrder با ShippingCost واقعی، فاکتور | F2,F3 |
| **F5a** | بازطراحی فرم محصول: بخش‌های ۱-۹ (اطلاعات پایه + قیمت/موجودی + مشخصات‌فنی + گالری) + بدون‌برند | Create/Edit با تب‌بندی ۵‌تایی، مشخصات‌فنی inline | F3 |
| **F5b** | بازطراحی فرم محصول: بخش ۱۰ «چند‌مدلی» (HasVariants wizard کامل) + هماهنگی CreateMultiBrand | wizard واریانت کامل و هماهنگ | F5a |
| **F6** | تب «محصولات»: باکس‌های آماری گرافیکی + پنل جست‌وجو/فیلتر پیشرفته | Index محصولات بازطراحی‌شده | F5b |
| **F7** | تب «مدیریت موجودی»: پشتیبانی کامل واریانت‌ها، کنترل تفکیکی قیمت/تعداد هر زیرشاخه | Inventory واریانت‌محور | F5b |
| **F8** | تب «سفارش‌ها»: سورت پیشرفته + جست‌وجو + تغییر وضعیت (سمت کاربر هم) + کد رهگیری + فاکتور دقیق | Orders بازطراحی‌شده | F4 |
| **F9** | تب «کاربران»: جست‌وجو + حذف + ادمین‌کردن + جزئیات+سفارش‌ها | Users بازطراحی‌شده | F1 |
| **F10** | تب «داشبورد»: گرافیکی (آمار محصول/کاربر/سفارش + سفارش‌های اخیر + نمودار) | Dashboard بازطراحی‌شده | همه |
| **F11** | بازطراحی نمایش محصول به کاربر (تب ویژگی‌ها در صفحهٔ محصول + انتخاب مدل/زیرمدل + تخفیف کارت) | Catalog/Detail عمومی هماهنگ با F5b | F5b |
| **F12a** | پاکسازی تب‌های مرده + audit Domestic-only + تست یکپارچه (build/JS/لینک‌ها) | پنل تمیز از مسیر/تب/asset مرده، صفر URL خارجی، build سبز | همه |
| **F12b** | پولیش UI/UX کلی پنل (یکدستی بصری) + آپدیت نهایی AI_CONTEXT/PROGRESS | یکدستی بصری کامل + داکیومنت نهایی | F12a |

---

## 4. شرح کاملِ هر فاز (خودبسنده — برای سشن مستقل)

### ── F1: پوستهٔ ادمین جدید (Navigation + Shell + UX) ──
**هدف:** کاهش ۱۱ تب به ۶ تب، بدون از دست رفتن هیچ قابلیتی. فقط ساختار ناوبری و چیدمان عوض می‌شود؛ صفحات موجود همان‌جا می‌مانند ولی از مسیر جدید در دسترس‌اند.

**کارها:**
1. **از کاربر تأیید بگیر** (در ابتدای فاز، یک سؤال کوتاه): «مدیریت صفحهٔ اصلی (بنر/Hero/گرید) را زیر منوی فرعی نگه دارم یا کاملاً حذف؟ دسته‌بندی‌ها داخل تب کنترل ویژگی برود؟» — اگر کاربر در دسترس نبود، پیش‌فرضِ §0 را اعمال کن و در PROGRESS بنویس.
2. بازنویسی `Areas/Admin/Pages/Shared/_AdminSidebar.cshtml`:
   - ۶ آیتم اصلی: **داشبورد**(`/Admin`) · **محصولات**(`/Admin/Products`) · **مدیریت موجودی**(`/Admin/Inventory`) · **کنترل ویژگی**(`/Admin/Attributes` — صفحهٔ hub جدید) · **سفارش‌ها**(`/Admin/Orders`) · **کاربران**(`/Admin/Users`).
   - زیرمنوی جمع‌شونده «تنظیمات بیشتر» شامل: مدیریت صفحهٔ اصلی (`/Admin/Home`) — تا قابلیت حفظ شود.
   - انتهای سایدبار: لینک **بازگشت به فروشگاه** (`/` با `target` معمولی) با آیکن.
   - `ViewData["AdminActive"]` keyهای جدید: `dashboard|products|inventory|attributes|orders|users|home`.
3. ساخت صفحهٔ hub تب کنترل ویژگی: `Areas/Admin/Pages/Attributes/Index.cshtml(.cs)` — فعلاً فقط کارت‌های لینک به زیربخش‌ها (Brands/Variants/Features/Tags/Categories/PostTypes). محتوای واقعی در F3.
4. ارتقای `_AdminLayout.cshtml` + `wwwroot/admin/admin.css`: design system تمیزتر (توکن رنگ، فاصله، کارت، سایه، حالت موبایل/جمع‌شدن سایدبار). RTL کامل. آیکن inline SVG. **بدون asset خارجی.**
5. هر صفحهٔ موجود که `AdminActive` ست می‌کرد، key را با نگاشت جدید هماهنگ کن (مثلاً صفحات Brands/Variants/Features/Tags/Categories → `attributes`).

**Boundary:** فقط `Areas/Admin/**` + `wwwroot/admin/**`. صفر تغییر Domain/Feature/فروشگاه.
**تست (sandbox):** `node --check` روی JS تغییر‌یافته. چک بصری ساختار. در PROGRESS: «build اجرا نشد».
**Definition of Done:** سایدبار ۶ تب + منوی فرعی + بازگشت به فروشگاه؛ همهٔ صفحات قدیمی از مسیرشان باز می‌شوند؛ هیچ لینک شکسته.

---

### ── F2: Domain نوع پست + snapshot سفارش + Migration ──
**هدف:** زیرساخت دادهٔ «نوع پست» که F3/F4 رویش کار می‌کنند. نرخ امن و غیرقابل‌دستکاری.

**کارها:**
1. **Entity جدید** `src/MahanShop.Domain/Entities/ShippingMethod.cs`:
   ```csharp
   public class ShippingMethod : BaseEntity
   {
       public string Name { get; set; } = null!;   // «پست پیشتاز»، «تیپاکس»
       public long Cost { get; set; }               // تومان — نرخ ثابت
       public bool IsActive { get; set; } = true;
       public int DisplayOrder { get; set; }
       public string? Description { get; set; }     // اختیاری
   }
   ```
   > تصمیم: نرخ ثابت per-method (ساده و امن). اگر بعداً نرخ وزنی/منطقه‌ای خواست → فاز بعد.
2. **Order**: افزودن snapshot نوع پست (تغییر‌ناپذیر بعد ثبت):
   - `public int? ShippingMethodId { get; set; }` + `public ShippingMethod? ShippingMethod { get; set; }`
   - `public string? ShippingMethodName { get; set; }` (snapshot نام، مثل ProductTitle).
   - `ShippingCost` موجود = snapshot نرخ (همان فیلد فعلی، فقط دیگر هاردکد 0 نیست).
3. **Fluent config**: فایل جدید یا افزودن به config موجود — `ShippingMethod` (Name maxlen، Cost، index DisplayOrder)؛ FK `Order.ShippingMethodId` با `OnDelete = SetNull` (حذف method سفارش قدیمی را نشکند، چون snapshot داریم).
4. **IApplicationDbContext** + **MyDbContext**: افزودن `DbSet<ShippingMethod> ShippingMethods`.
5. **Migration** `Add_ShippingMethods` (نام بامعنی). در sandbox ساخته نمی‌شود → **فایل migration را دستی طبق الگوی migrationهای موجود بنویس** یا در PROGRESS بنویس «migration باید روی محیط dotnet ساخته شود: `dotnet ef migrations add Add_ShippingMethods`». **اولویت: دستورِ ساخت را مستند کن + Snapshot را به‌روز نگه‌داشتن یادآوری کن.**
6. **Seed** (`DataSeeder`, Development-only, idempotent): ۲–۳ نوع پست نمونه (پست پیشتاز/سفارشی/تیپاکس) با نرخ نمونه.

**Boundary:** Domain + Infra.Data (entity/config/context/migration/seed). **هیچ UI.**
**Definition of Done:** entity + DbSet + config + دستور/فایل migration + seed آماده. سینتکس C# دقیق.

---

### ── F3: تب «کنترل ویژگی» یکپارچه + مدیریت نوع پست ──
**هدف:** یک تب واحد که همهٔ «تنظیمات پایه» را دارد، با زیربخش‌های واضح. شامل CRUD کامل نوع پست.

**زیربخش‌ها (هرکدام از کد موجود استفاده می‌کند، فقط زیر یک چتر می‌آیند):**
1. **برندها** — از `Features/Admin/Brands` + `Areas/Admin/Pages/Brands` (موجود). فقط لینک/تب داخل hub.
2. **مدل‌ها / ویژگی‌های متغیر** — از `Variants` (VariantAttribute/Value سلسله‌مراتبی برند→مدل→زیرمدل). موجود.
3. **مشخصات فنی** — از `Features` (Feature key). موجود.
4. **برچسب‌ها** — از `Tags`. موجود.
5. **دسته‌بندی‌ها** — از `Categories` (درختی). منتقل‌شده به اینجا.
6. **نوع پست (جدید)** — CRUD کامل روی `ShippingMethod`:
   - `Features/Admin/Shipping/` : `GetShippingMethodsQuery`, `CreateShippingMethodCommand`(+Validator: Name لازم، Cost>=0), `UpdateShippingMethodCommand`, `ToggleShippingMethodActiveCommand`, `DeleteShippingMethodCommand`, DTOها.
   - `Areas/Admin/Pages/Shipping/` : `Index`(لیست + فعال/غیرفعال + حذف), `Create`, `Edit`.
   - ⚠️ **نرخ امن:** نرخ فقط اینجا تنظیم می‌شود؛ در چک‌اوت از DB خوانده می‌شود (F4)، هرگز از فرم client.

**UI تب کنترل ویژگی** (`Areas/Admin/Pages/Attributes/Index.cshtml`): کارت/تب برای هر زیربخش با شمارش (مثلاً «۵ برند»، «۳ نوع پست»). تب‌های داخل صفحه یا کارت‌های لینک — انتخاب با UX بهتر.

**Boundary:** `Areas/Admin/Attributes`, `Areas/Admin/Shipping`, `Features/Admin/Shipping`, `wwwroot/admin`. CRUDهای موجود (Brands/Variants/Features/Tags/Categories) فقط از نظر ناوبری زیر این تب می‌آیند — منطقشان دست‌نخورده.
**Definition of Done:** تب کنترل ویژگی همهٔ ۶ زیربخش را نشان می‌دهد؛ نوع پست CRUD کامل و کارا.

---

### ── F4: چک‌اوت + فاکتور با نوع پست (امن) ──
**هدف:** کاربر هنگام ثبت سفارش نوع پست را انتخاب می‌کند؛ هزینه از DB خوانده و snapshot می‌شود؛ در سفارش و فاکتور دقیق ثبت می‌شود.

**کارها:**
1. **GetCartQuery / صفحهٔ چک‌اوت** (`CheckoutController` + `Checkout/Index`): نمایش لیست نوع‌های پست فعال (از DB) با نرخ؛ کاربر یکی را انتخاب می‌کند (radio). مبلغ قابل‌پرداخت = اقلام + هزینهٔ پست انتخابی (محاسبهٔ سمت سرور).
2. **PlaceOrderCommand**: ورودی `ShippingMethodId` اضافه شود. Handler:
   - method را از DB بخوان (فعال؟ موجود؟) — اگر نامعتبر، خطا یا انتخاب پیش‌فرض.
   - `order.ShippingMethodId`, `order.ShippingMethodName = method.Name` (snapshot), `order.ShippingCost = method.Cost` (snapshot از DB، **نه از فرم**).
   - `FinalAmount = TotalAmount - DiscountAmount + ShippingCost`.
   - خط ۹۸ فعلی (`ShippingCost = 0`) جایگزین شود.
3. **Validator** PlaceOrder: `ShippingMethodId` لازم/معتبر.
4. **فاکتور** (`InvoicePdfService`): ردیف «نوع پست: {Name}» + «هزینهٔ ارسال: {Cost}». الان فقط هزینه را نشان می‌دهد → نام نوع پست هم اضافه شود.
5. **نمایش سفارش** (`GetOrderDetailQuery` کاربر + `GetAdminOrderDetailQuery` ادمین + Viewها): نوع پست + هزینه نمایش داده شود.

**Boundary:** فروشگاه عمومی (`Features/Cart`, `Features/Account`, `CheckoutController`, `Checkout/Index`) + `Features/Admin/Orders` + `Services/InvoicePdfService`. **در PROGRESS صریح بنویس کدام فایل فروشگاه عمومی لمس شد.**
**Definition of Done:** کاربر نوع پست انتخاب می‌کند؛ هزینه سمت سرور؛ snapshot در سفارش؛ فاکتور و صفحات سفارش نوع پست + هزینه را نشان می‌دهند؛ غیرقابل دستکاری (تست: ارسال ShippingMethodId جعلی → fallback/خطا، مبلغ از DB).

---

### ── F5a: بازطراحی فرم محصول — بخش‌های ۱-۹ + گالری (بدون wizard واریانت) ──
**هدف:** فرم محصول با تب‌بندی تمیز و پشتیبانی کامل از بخش‌های ۱ تا ۹ (اطلاعات پایه، قیمت/موجودی، مشخصات فنی، گالری). بخش ۱۰ «چند‌مدلی» در F5b انجام می‌شود.

**بخش‌های این فاز (۱ تا ۹ + ۱۱):**
1. **عنوان محصول** — `Product.Title` (لازم) + Slug خودکار از Title (SlugHelper موجود).
2. **دسته‌بندی** — انتخاب از Categoryهای موجود (درختی، dropdown). `Product.CategoryId`.
3. **برند** — انتخاب از Brandهای موجود + **گزینهٔ «بدون برند»**.
   - ✅ **تصمیم (از F0):** Brand سیستمی «بدون برند» با seed idempotent (بررسی `if AnyAsync(b => b.Slug == "no-brand")` قبل insert). `Product.BrandId` non-nullable باقی می‌ماند → صفر migration لازم.
4. **قیمت محصول** — `Product.Price` (اصلی).
5. **قیمت با تخفیف** — `Product.DiscountPrice?` (اختیاری). validator: DiscountPrice < Price.
6. **موجودی** — `Product.Stock` (برای محصول ساده).
7. **توضیح کوتاه** — `Product.ShortDescription?`.
8. **توضیحات** — `Product.Description?` (کامل).
9. **مشخصات فنی** — انتخاب Feature از لیست + مقدار (`ProductFeature`). inline داخل فرم Edit (و Create پس از ذخیره اولیه). Commands: `AddProductFeatureCommand`, `DeleteProductFeatureCommand`. این ویژگی‌ها در F11 در تب «ویژگی‌ها» صفحهٔ محصول نمایش داده می‌شوند.
11. **تصویر محصول** — آپلود (`ImageUploadService` موجود، whitelist) → `ProductImage` (Main + گالری). این بخش در Edit موجود است؛ در Create به صفحهٔ Edit ریدایرکت می‌شود — رفتار فعلی حفظ می‌شود.

**کارهای این فاز:**
1. **Seed «بدون برند»:** در `DataSeeder.cs` متد `SeedNoBrandAsync` idempotent (slug=`no-brand`). صدا از `SeedAsync`.
2. **ProductFeature Commands:** `AddProductFeatureCommand(ProductId, FeatureId, Value)` + Validator + Handler در `Features/Admin/Products/`. `DeleteProductFeatureCommand(ProductFeatureId)` + Handler.
3. **توسعهٔ `ProductEditDto`:** افزودن `List<ProductFeatureItemDto> Features` (Id، FeatureName، Value).
4. **توسعهٔ `GetProductForEditQuery`:** Include `Features` + projection. افزودن `List<FeatureOptionDto> AllFeatures` (همهٔ Feature موجود برای dropdown در فرم) — یا Query جداگانه.
5. **بازنویسی `Create.cshtml(.cs)`:** تب‌بندی ۵‌تایی: «اطلاعات پایه» / «قیمت و موجودی» / «مشخصات فنی» / «مدل‌ها (به‌زودی)» / «تصاویر (بعد از ذخیره)». در Create فقط تب‌های ۱ و ۲ فعال + مشخصات‌فنی بعد از ذخیره. dropdown برند حالا گزینهٔ «بدون برند» دارد. validator `BrandId > 0` → زیرا seed «بدون برند» یک Id معتبر دارد.
6. **بازنویسی `Edit.cshtml(.cs)`:** همان تب‌بندی + تب «مشخصات فنی» با CRUD inline (افزودن Feature+مقدار / حذف ردیف). گالری در تب «تصاویر». واریانت‌ها در تب «مدل‌ها» حفظ می‌شوند (کد موجود دست‌نخورده).
7. **CSS:** استایل تب‌های ادمین `admin-tabs` در `wwwroot/admin/admin.css` اگر موجود نبود.

**Boundary:** `Areas/Admin/Products`, `Features/Admin/Products`, `Infra.Data/Seed/DataSeeder.cs`. صفر Domain change. صفر migration.
**Definition of Done:** فرم بخش‌های ۱-۹+۱۱ کار می‌کند؛ «بدون برند» در dropdown وجود دارد؛ مشخصات فنی inline افزودن/حذف دارد؛ گالری در تب تصاویر؛ واریانت‌های موجود در Edit دست‌نخورده‌اند. بدون خرابی build.

---

### ── F5b: بازطراحی فرم محصول — بخش ۱۰ «چند‌مدلی» + wizard واریانت کامل ── *(سشن بعدی)*
**هدف:** wizard «چند‌مدلی» (HasVariants) در فرم محصول، با انتخاب مدل→زیرمدل→ترکیب → تولید خودکار `ProductVariant`ها. این بخش پیچیده‌ترین قسمت F5 است و یک سشن مستقل می‌خواهد.

**پیش‌نیاز:** F5a کامل باشد (تب «مدل‌ها» در فرم به‌عنوان placeholder وجود دارد).

**بخش ۱۰ (چند‌مدلی — HasVariants):**
- تیک «چند‌مدلی» در تب «مدل‌ها»:
  - اگر فعال → محصول واریانتی. مدل‌هایی که در «کنترل ویژگی» (VariantAttribute Kind=Model/Brand…) تعریف شده نمایش داده شود؛ با انتخاب مدل، زیرمدل‌هایش (ParentValueId) نشان داده شود تا انتخاب شوند؛ هر ترکیب → یک `ProductVariant` با قیمت/تخفیف/موجودی/SKU مستقل.
  - منطق wizard موجود (`CreateMultiBrandProductCommand`, `GetProductWizardDataQuery`, `ProductWizardDtos`) را **بازاستفاده/بهبود** کن — از صفر ننویس.
  - اگر غیرفعال → محصول ساده، فقط Price/Stock بخش ۴ و ۶.

**کارها (F5b):**
1. **تب «مدل‌ها» را کامل کن** در Create و Edit: wizard انتخاب برند→مدل→زیرمدل/رنگ (موجود در `product-wizard.js`) را به تب‌بندی جدید منتقل/هماهنگ کن.
2. **`CreateMultiBrandProductCommand`** را بررسی و در صورت نیاز توسعه بده تا با تب‌بندی جدید هماهنگ باشد.
3. **Edit — مدیریت واریانت‌ها:** بخش افزودن گزینه + QuickStock + Toggle + Delete را در تب «مدل‌ها» قرار بده (از Edit فعلی منتقل کن).
4. **JS:** هماهنگی `product-wizard.js` با تب‌بندی جدید (انیمیشن، نمایش/پنهان).
5. **Validator:** بررسی که وقتی `HasVariants=true`، قیمت/موجودی سطح محصول اجباری نباشد.

**Boundary:** `Areas/Admin/Products`, `Features/Admin/Products`, `wwwroot/admin/product-wizard.js`.
**Definition of Done:** فرم کامل ۱۱ بخش (F5a + F5b)؛ محصول ساده و واریانتی هر دو ثبت/ویرایش می‌شوند؛ تخفیف/بدون‌برند/مشخصات‌فنی/مدل‌ها همه کار می‌کنند.

---

### ── F6: تب «محصولات» (آمار گرافیکی + جست‌وجو/فیلتر) ──
**هدف:** صفحهٔ محصولات با باکس‌های آماری + پنل جست‌وجوی قوی.

**کارها:**
1. **باکس‌های گرافیکی بالای صفحه:** تعداد کل محصولات · موجود (Stock>0 یا variant موجود) · ناموجود · موجودی کم (Stock زیر آستانه، مثلاً <۵). کوئری: گسترش `GetProductsQuery` یا query جدید آماری.
2. **دکمهٔ افزودن محصول** → `/Admin/Products/Create`.
3. **پنل جست‌وجو/فیلتر:** جست‌وجوی متن (Title/Slug)، فیلتر دسته، برند، وضعیت (فعال/غیرفعال)، وضعیت موجودی (موجود/ناموجود/کم)، نوع (ساده/واریانتی)، سورت. همه **سمت سرور** (بدون SPA؛ فرم GET مثل کاتالوگ عمومی).
4. جدول/گرید محصولات با اکشن‌ها (ویرایش/فعال‌سازی/مشاهده). صفحه‌بندی.

**Boundary:** `Areas/Admin/Products/Index`, `Features/Admin/Products` (query آماری + فیلتر).
**Definition of Done:** باکس‌های آماری درست؛ جست‌وجو/فیلتر/سورت/صفحه‌بندی سمت سرور کار می‌کند.

---

### ── F7: تب «مدیریت موجودی» (واریانت کامل) ──
**هدف:** کنترل واضح و تفکیکی موجودی/قیمت همهٔ محصولات، به‌خصوص واریانتی.

**کارها:**
1. لیست محصولات؛ محصول ساده → ویرایش inline قیمت/موجودی.
2. محصول واریانتی → باز کردن ردیف نشان‌دهندهٔ همهٔ `ProductVariant`هایش با ترکیب ویژگی (برند+مدل+زیرمدل/رنگ)، هرکدام: قیمت/تخفیف/موجودی/فعال — قابل ویرایش تفکیکی (inline). از `Features/Admin/ProductVariants` + `Inventory` موجود بازاستفاده/بهبود.
3. فیلتر/جست‌وجو + شناسایی موجودی کم. CSV موجود (`InventoryCsv`) حفظ شود.
4. ذخیرهٔ امن (RowVersion واریانت — موجود).

**Boundary:** `Areas/Admin/Inventory`, `Features/Admin/Inventory`, `Features/Admin/ProductVariants`.
**Definition of Done:** هر زیرشاخهٔ هر محصول واریانتی به‌تفکیک قابل کنترل (قیمت/تعداد)؛ واضح و قابل‌فهم؛ ذخیرهٔ امن.

---

### ── F8: تب «سفارش‌ها» ──
**هدف:** کنترل پیشرفتهٔ سفارش‌ها.

**کارها:**
1. لیست همهٔ سفارش‌ها، **پیش‌فرض جدیدترین**. سورت قابل‌تغییر: جدیدترین/قدیمی‌ترین/مبلغ (صعودی/نزولی). پنل جست‌وجو (کد سفارش، نام/شمارهٔ کاربر، وضعیت).
2. **تغییر وضعیت** سفارش (`ChangeOrderStatusCommand` موجود) — تغییر باید در **سفارشی که کاربر می‌بیند** هم منعکس شود (`GetUserOrders`/`OrderDetail` کاربر همان Status را می‌خوانند → خودکار منعکس می‌شود؛ فقط مطمئن شو نگاشت Label فارسی هماهنگ است).
3. **کد رهگیری پستی:** ادمین `Order.TrackingCode` را وارد می‌کند (command جدید یا توسعهٔ ChangeStatus) → در پنل کاربر نمایش داده شود.
4. **فاکتور:** از هر سفارش (PDF موجود `InvoicePdfService`) — دقیق، شامل نوع پست (F4).
5. سورت/جست‌وجو سمت سرور؛ صفحه‌بندی.

**Boundary:** `Areas/Admin/Orders`, `Features/Admin/Orders`, و خواندنِ سمت کاربر (تأیید انعکاس وضعیت/رهگیری).
**Definition of Done:** سورت پیش‌فرض جدیدترین + سورت‌های دیگر + جست‌وجو؛ تغییر وضعیت منعکس در سمت کاربر؛ ورود کد رهگیری دیده‌شده توسط کاربر؛ فاکتور دقیق.

---

### ── F9: تب «کاربران» ──
**هدف:** مدیریت کامل کاربران.

**کارها:**
1. لیست + **جست‌وجو** (نام/شماره). از `Features/Admin/Users` موجود.
2. **مشاهدهٔ جزئیات** کاربر + سفارش‌هایش (`Users/Detail` موجود — بهبود).
3. **حذف کاربر** (command جدید `DeleteUserCommand` — با ملاحظه: سفارش‌های کاربر چه می‌شوند؟ soft-delete یا منع حذف اگر سفارش دارد. تصمیم در PROGRESS).
4. **ادمین‌کردن/سلب ادمین** (`User.IsAdmin` toggle — command؛ نقش هر درخواست از DB re-check می‌شود → خودکار اعمال). محافظت: ادمین نتواند خودش را حذف/سلب کند و آخرین ادمین حذف نشود.

**Boundary:** `Areas/Admin/Users`, `Features/Admin/Users`.
**Definition of Done:** جست‌وجو/جزئیات/سفارش‌ها/حذف/ادمین‌کردن، همه امن (آخرین ادمین محافظت‌شده).

---

### ── F10: تب «داشبورد» گرافیکی ──
**هدف:** داشبورد بصری و مفید.

**کارها:**
1. باکس‌های آماری: کل محصولات · کل کاربران · کل سفارش‌ها (+ شاید درآمد). از `GetDashboardStatsQuery` (توسعه).
2. **سفارش‌های اخیر** (جدول کوتاه آخرین N).
3. نمودار ساده (مثلاً سفارش‌های ۷ روز اخیر) — **بدون lib خارجی**: یا SVG/CSS دستی، یا یک lib چارت self-host در `wwwroot/lib` (Domestic-only؛ اگر افزوده شد در STACK.md ثبت + self-host). ترجیح: SVG/CSS سبک.
4. UI/UX گرافیکی (کارت‌ها، آیکن، رنگ).

**Boundary:** `Areas/Admin/Index`(داشبورد), `Features/Admin/Dashboard`, `wwwroot/admin`.
**Definition of Done:** داشبورد گرافیکی با آمار درست + سفارش‌های اخیر + نمودار self-host.

---

### ── F11: نمایش محصول به کاربر (هماهنگ با F5) ──
**هدف:** صفحهٔ محصول عمومی با ساختار جدید.

**کارها:**
1. **تب «ویژگی‌ها» کنار «معرفی»** در `Catalog/Detail` — مشخصات فنی (ProductFeature) را نشان دهد. (تب‌بندی pd-tabs موجود است — تکمیل/تأیید.)
2. **انتخاب مدل/زیرمدل** برای محصولات واریانتی: کاربر برند/مدل سپس زیرمدل را انتخاب می‌کند → variant مشخص → قیمت/موجودی آن variant. (`GetProductDetailQuery` + JS انتخابگر variant). منطق موجود variant در Detail را تأیید/تکمیل کن طبق VARIANTS.md.
3. **درصد تخفیف روی کارت‌ها** (کاتالوگ/صفحهٔ اصلی) — وقتی DiscountPrice ست است (منطق DiscountPercent موجود؛ تأیید).
4. سازگاری سبد/چک‌اوت با VariantId (موجود).

**Boundary:** فروشگاه عمومی: `Features/Catalog`, `Views/Catalog`, `wwwroot/css/product-detail.css`, `wwwroot/js/site.js`. **در PROGRESS لمس فروشگاه عمومی را ثبت کن.**
**Definition of Done:** صفحهٔ محصول تب ویژگی‌ها دارد؛ انتخاب مدل→زیرمدل→variant کار می‌کند؛ تخفیف روی کارت‌ها درست.

---

### ── F12a: پاکسازی + Domestic-only audit + تست یکپارچه ── *(مکانیکی، کم‌ریسک)*
**هدف:** قبل از پولیش بصری، پنل از نظر «ساختار/مسیر/asset» تمیز شود. مسیر/تب/لینکِ مرده، asset خارجی، یا build شکسته نمانَد. این فاز عمدتاً «حذف/redirect/audit/build» است — صفر تغییر طراحی.

**کارها (به ترتیب):**
1. **اسکن مسیرهای سایدبار** (`Areas/Admin/Pages/Shared/_AdminSidebar.cshtml`) و تطبیق با فایل‌های موجود در `Areas/Admin/Pages/**`:
   - فهرستِ صفحات ادمینِ موجود را در PROGRESS بنویس.
   - هر صفحه‌ای که در سایدبار/منوی فرعی/hub‌ها لینک ندارد و از جای دیگری هم لینک نمی‌شود → کاندیدای «تب مرده». تصمیم: **redirect** به مسیر جدید (پیش‌فرض) یا **حذف**. حذف فقط وقتی هیچ قابلیتی از دست نمی‌رود (طبق §1 قانون ۹).
   - مثال‌های احتمالی برای بررسی: مسیر مستقل `Categories` (طبق F1 باید فقط زیر hub «کنترل ویژگی» باشد)، هر Index قدیمی Brands/Variants/Features/Tags که از سایدبار حذف شده ولی هنوز فایل دارد.
2. **حذف یا Redirect**:
   - **Redirect (ترجیح):** در `OnGet` صفحهٔ قدیمی `return RedirectToPage("/Attributes/Index")` (یا مقصد مناسب). فایل `.cshtml` کوچک نگه‌داشته شود (یک پیام «این صفحه منتقل شده»). این روش امن‌تر است چون لینک‌های خارجی نمی‌شکند.
   - **حذف:** فقط اگر مطمئنی هیچ لینک داخلی به آن نمی‌رود (با `grep` کل `Areas/Admin` + `wwwroot/admin` چک کن). فایل `.cshtml` و `.cshtml.cs` همزمان حذف شوند.
   - هر مورد را در PROGRESS با دلیل ثبت کن.
3. **اسکن لینک‌های شکسته** داخل ادمین:
   - `grep -rn 'asp-page=\|asp-area=\|href=' src/MahanShop.Web/Areas/Admin` → لینک‌هایی که به مسیرِ حذف‌شده اشاره می‌کنند را پیدا و به مقصد جدید اصلاح کن.
   - `ViewData["AdminActive"]` همهٔ صفحات با key‌های فاز F1 (`dashboard|products|inventory|attributes|orders|users|home`) هم‌خوان باشد.
4. **Domestic-only audit (صفر URL خارجی)** — اسکن **همهٔ asset جدیدِ این بازطراحی**:
   - `grep -REn 'https?://(cdn|cdnjs|jsdelivr|unpkg|fonts\.google|gstatic|googleapis|gravatar|fontawesome|bootstrap[^/]*\.com)' src/MahanShop.Web/Areas/Admin src/MahanShop.Web/wwwroot/admin src/MahanShop.Web/wwwroot/AdminPanel` → باید **خالی** باشد.
   - `grep -REn 'integrity=|crossorigin=|preconnect' src/MahanShop.Web/Areas/Admin src/MahanShop.Web/wwwroot/admin` → بررسی هرکدام (در حالت Domestic-only این attributeها معمولاً لازم نیستند).
   - `grep -REn '<link[^>]+href="https?:|<script[^>]+src="https?:' src/MahanShop.Web/Areas/Admin src/MahanShop.Web/wwwroot/admin` → باید **خالی**.
   - فایل‌های فروشگاه عمومیِ لمس‌شده در F4/F11 هم همین audit را بگذرانند.
   - هر URL خارجی پیدا شد → asset را self-host کن در `wwwroot/admin/lib` یا `wwwroot/lib` (بدون اضافه‌کردن وابستگی جدید؛ اگر lib جدید بود → در STACK.md ثبت کن).
5. **تست یکپارچه (DoD هر فاز سریع)** — فقط چک‌لیستِ سریعِ «هنوز کار می‌کند؟»، **بدون** بازنویسی:
   - سایدبار ۶ تب + منوی فرعی + بازگشت به فروشگاه (F1). ✓
   - hub کنترل ویژگی همهٔ ۶ زیربخش را لینک می‌دهد (F3). ✓
   - فرم محصول ۵ تب دارد و wizard مدل‌ها باز می‌شود (F5a/F5b). ✓
   - باکس آماری محصولات/فیلتر کار می‌کند (F6). ✓
   - مدیریت موجودی واریانتی expand می‌شود (F7). ✓
   - سفارش‌ها سورت/جست‌وجو/کدرهگیری/فاکتور دارد (F8). ✓
   - کاربران آمار/جست‌وجو/حذف/ادمین‌کردن دارد (F9). ✓
   - داشبورد ۴ کارت + نمودار + ۲ جدول (F10). ✓
   - صفحهٔ محصول عمومی تب «ویژگی‌ها» + انتخاب مدل/زیرمدل + تخفیف کارت (F11). ✓
   - هر کدام مشکل دارد → در PROGRESS به‌عنوان «بدهی برای F12b» ثبت، یا اگر کوچک است همین‌جا fix.
6. **Build واقعی:**
   - `bash tools/build.sh` → باید **0 Error** بدهد. warnings را در PROGRESS ثبت کن (نباید جدید اضافه شود).
   - `bash tools/check-js.sh` → همهٔ JSهای ادمین `node --check` پاس.
7. **PROGRESS:** بخش F12a کامل بنویس: «چه فایل/مسیری حذف یا redirect شد»، «نتیجهٔ Domestic-only audit»، «build سبز»، «بدهی‌های منتقل‌شده به F12b».

**Boundary:** فقط **حذف/redirect/audit/fix لینک**. صفر طراحی جدید، صفر CSS جدید (مگر برای fix باگ شکسته)، صفر Domain/Feature. اگر لازم شد فایل فروشگاه عمومی لمس کنی (مثلاً URL خارجی در `_Layout`)، در PROGRESS ثبت کن.

**Definition of Done:**
- ✅ هیچ مسیر/تب/لینک شکسته در ادمین.
- ✅ Domestic-only audit پاک (صفر URL خارجی روی asset‌های این بازطراحی).
- ✅ `bash tools/build.sh` = 0 Error.
- ✅ `bash tools/check-js.sh` پاس.
- ✅ چک‌لیست DoD هر فاز در PROGRESS تیک خورده (یا «بدهی برای F12b» نوشته).
- ✅ آماده برای F12b (پولیش بصری).

---

### ── F12b: پولیش UI/UX کلی + داکیومنت نهایی ── *(بصری، بعد F12a)*
**هدف:** یکدستی بصری کامل پنل ادمین و نهایی‌سازی داکیومنت‌ها.

**پیش‌نیاز:** F12a کامل (هیچ تب مرده/لینک شکسته/URL خارجی نمانده، build سبز). اگر F12a بدهی گذاشت → اول آن‌ها fix.

**کارها:**
1. **مرور یکدستی بصری همهٔ صفحات ادمین** (همهٔ ۶ تب + زیربخش‌ها + Detailها):
   - **توکن‌های رنگ** (`wwwroot/admin/admin.css`) — همهٔ صفحات از همان CSS variables استفاده کنند، نه رنگ هاردکد. ناهماهنگی‌ها را لیست و یکدست کن.
   - **فاصله‌گذاری** — padding/margin/gap کارت‌ها و جدول‌ها بین صفحات یکسان باشد.
   - **فونت Vazir** — همه‌جا (نه فقط بعضی صفحات).
   - **RTL کامل** — هیچ `text-align: left` اشتباه، هیچ آیکن چپ‌چین.
   - **حالت موبایل** — همهٔ صفحات روی ≤640px شکسته نشود (سایدبار جمع‌شو، جدول‌ها افقی scroll، فرم‌ها stack).
   - **آیکن‌های inline SVG** — استایل یکسان (stroke-width، size).
   - **Badge/Pill/Chip** — کلاس‌های یکسان (`badge`, `chip`, `pill`) بین صفحات.
2. **پولیش انتخابی** — نکات کوچکِ بصری که جای کار دارند (نه بازنویسی):
   - hover/focus states روی دکمه‌ها/سطرها.
   - empty-state تمیز روی جدول‌های خالی.
   - loading-state ساده (اختیاری).
3. **داکیومنت نهایی:**
   - **`docs/AI_CONTEXT.md`** — بخش پنل ادمین را با نقشهٔ جدید ۶ تب کامل بازنویسی کن: کجا چی هست (سایدبار، hub، Pageها، Featureها)، تب‌های اصلی و زیربخش‌ها، نوع پست (ShippingMethod) کجاست، فرم محصول ۱۱ بخش کجاست. هدف: سشن آینده بدون این روادمپ هم بفهمد ادمین چه ساختاری دارد.
   - **`docs/ADMIN_REVAMP_PROGRESS.md`** — F12b را با خلاصهٔ کلِ بازطراحی ببند: همهٔ ۱۲ فاز ✅، بدهی‌های باز (اگر هست)، PR نهایی.
   - **`docs/PROGRESS.md`** (changelog کلی پروژه) — یک ورودی «ADMIN REVAMP بزرگ تمام شد» با خلاصه‌ٔ ۱۱ فاز.
   - اگر lib جدید (مثلاً برای چارت داشبورد F10) self-host شده → در **`docs/STACK.md`** ثبت.
4. **build نهایی + audit نهایی:**
   - `bash tools/build.sh` = 0 Error.
   - `bash tools/check-js.sh` پاس.
   - دوباره Domestic-only audit (F12a step 4) → باید همچنان پاک باشد.

**Boundary:** عمدتاً `wwwroot/admin/admin.css` + Razor Viewها برای یکدستی + `docs/*`. اگر صفحهٔ خاصی نیاز به اصلاح بزرگ دارد، در حد یکدستی نگه‌دار؛ بازنویسی = خارج از اسکوپ.

**Definition of Done:**
- ✅ همهٔ صفحات ادمین یکدست بصری (رنگ/فاصله/فونت/RTL/موبایل).
- ✅ `docs/AI_CONTEXT.md` نقشهٔ جدید پنل را دارد.
- ✅ `docs/ADMIN_REVAMP_PROGRESS.md` بازطراحی بسته‌شده.
- ✅ `docs/PROGRESS.md` ورودی نهایی دارد.
- ✅ build سبز + Domestic-only پاک.
- ✅ پنل کامل، هماهنگ، ۶ تب، همهٔ قابلیت‌ها سرِ جا — **پایان ADMIN REVAMP**.

---

## 5. ماتریس «هیچ موردی گم نشود» (چک‌لیست خواستهٔ کاربر ↔ فاز)

| خواستهٔ کاربر | فاز |
|----------------|-----|
| ۶ تب (کاهش از ۱۱) | F1 |
| داشبورد گرافیکی: تعداد محصولات/کاربران/سفارش‌ها + سفارش‌های اخیر | F10 |
| محصولات: باکس آماری (همه/موجود/ناموجود/موجودی‌کم) | F6 |
| محصولات: دکمهٔ افزودن | F6 (+F5 فرم) |
| محصولات: نمایش همه + پنل جست‌وجو/فیلتر | F6 |
| مدیریت موجودی: پشتیبانی کامل واریانت، کنترل تفکیکی قیمت/تعداد زیرشاخه | F7 |
| کنترل ویژگی: برند/مدل/ویژگی/مشخصات فنی یکجا | F3 |
| کنترل ویژگی: **نوع پست** + نرخ + انتخاب کاربر + امن + در فاکتور | F2+F3+F4 |
| سفارش‌ها: سورت (جدید/قدیم/مبلغ)، پیش‌فرض جدیدترین | F8 |
| سفارش‌ها: پنل جست‌وجو | F8 |
| سفارش‌ها: فاکتور دقیق | F8 (+F4) |
| سفارش‌ها: تغییر وضعیت دیده‌شده توسط کاربر | F8 |
| سفارش‌ها: ورود کد رهگیری پستی برای کاربر | F8 |
| کاربران: جست‌وجو/مشاهده/حذف/ادمین‌کردن/جزئیات+سفارش | F9 |
| لینک بازگشت به فروشگاه | F1 |
| فرم محصول: بخش‌های ۱-۹ + گالری (اطلاعات پایه / قیمت / مشخصات‌فنی / تصاویر) | F5a |
| فرم محصول: بخش ۱۰ چند‌مدلی (wizard واریانت کامل) | F5b |
| محصول: تب ویژگی‌ها در صفحهٔ محصول | F11 |
| محصول: چند مدلی (مدل→زیرمدل→سفارش) | F5b+F11 |
| محصول: قیمت با تخفیف → درصد روی کارت | F5a+F11 |
| محصول: گزینهٔ بدون برند | F5a |
| UI/UX بهتر کل پنل | F1+F10+F12b |

> اگر در هر سشن موردی از این ماتریس را پیدا کردی که هنوز انجام نشده و فازش گذشته → در PROGRESS به‌عنوان «بدهی» ثبت کن و قبل F12b ببند.

---

## 6. قراردادهای کار بین‌سشنی (تکرار برای اطمینان)
- شروع: AI_CONTEXT → CLAUDE → این فایل → ADMIN_REVAMP_PROGRESS → (VARIANTS اگر لازم).
- پایان هر فاز: PROGRESS را با «فاز X ✅ — چه شد / چه فایل‌هایی / لمس فروشگاه عمومی؟ / دستور migration؟ / قدم بعد = فاز X+1» آپدیت کن.
- کامیت conventional روی `genspark_ai_developer` → sync با main → squash → PR → **لینک PR را به کاربر بده**.
- build در sandbox اجرا نمی‌شود؛ صادقانه بنویس. migration دستورش را مستند کن.
- هیچ asset خارجی. هیچ تغییر TargetFramework. هیچ SPA.
