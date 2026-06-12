# Variant & Inventory System — Design (LOCKED)

> سیستم ویژگی‌های پویا + موجودی per-variant. منبع حقیقت طراحی. تغییر = تایید کاربر.

## Why
محصول می‌تواند چند «گزینه قابل فروش» داشته باشد که هرکدام موجودی/قیمت مستقل دارند:
- **قاب گوشی:** برند (سامسونگ/اپل) → مدل (A10/S12/…) → کد اختصاصی. موجودی هر مدل جدا (A10→۱۰، S12→۲).
- **گوشی:** رنگ + حافظه.
- بعضی محصولات هیچ گزینه‌ای ندارند (محصول ساده، موجودی واحد).

سیستم قدیم فقط «رنگ» (`ProductColor.Stock`) را پشتیبانی می‌کرد. این سیستم تعمیمِ آن است.

## Model (EAV — dynamic attributes)

| Entity | نقش |
|--------|-----|
| `VariantAttribute` | نوع ویژگی **global**: «برند»، «مدل»، «کد»، «رنگ»، «حافظه». فیلد `IsColor` برای نمایش swatch. `DisplayOrder`. |
| `VariantAttributeValue` | مقدار در pool هر attribute: «سامسونگ»، «A10»، «#FF0000». `ColorHex?` برای رنگ. `DisplayOrder`. |
| `ProductVariant` | یک گزینه فروش = یک ردیف موجودی. `ProductId`, `Sku?` (کد داخلی/بارکد)، `Price`, `DiscountPrice?`, `Stock`, `IsActive`, `DisplayOrder`, `RowVersion`. |
| `ProductVariantValue` | join: هر variant = ترکیب چند مقدار ویژگی (برند=سامسونگ، مدل=A10، کد=X123). `ProductVariantId`, `AttributeValueId`. |

`Product.HasVariants : bool` — اگر `false` → موجودی/قیمت از خود `Product` (محصول ساده). اگر `true` → فقط از `ProductVariant`ها.

### Rules
- قیمت/موجودی **مستقل هر variant** (انتخاب کاربر). نمایش لیست = حداقل قیمت variantهای فعال موجود.
- `in-stock` محصول variant‌دار = وجود حداقل یک variant با `IsActive && Stock>0`.
- در checkout: variant با `Stock<=0` یا `!IsActive` → **disabled/مخفی**، قابل سفارش نیست.
- قیمت همیشه **سمت سرور** از DB (ضد دستکاری) — مثل سیستم فعلی سبد.
- کسر موجودی فقط بعد پرداخت موفق، روی `ProductVariant.Stock` (یا `Product.Stock` برای ساده). idempotent + RowVersion.

## Migration impact (ProductColor → ProductVariant)
داده قبلی مهم نیست (CLAUDE.md). رنگ یک attribute می‌شود:
- `Color`/`ProductColor` حذف؛ رنگ‌ها → `VariantAttribute("رنگ", IsColor=true)` + valueها.
- `OrderItem.ColorId` → `OrderItem.VariantId?` + snapshot `VariantTitle?`.
- سبد session: `ColorId` → `VariantId`.

## Touch points (کد موجود که آپدیت می‌شود)
- `GetProductDetailQuery` → variantها + attribute groupها + قیمت/موجودی per variant.
- `GetCatalogQuery` → قیمت = min variant، فیلتر موجودی = وجود variant موجود.
- `CartStore` / `GetCartQuery` / `PlaceOrderCommand` → `VariantId`.
- `VerifyPaymentCommand.DecrementStockAsync` → کسر `ProductVariant.Stock`.
- صفحه محصول + checkout → انتخابگر variant (dropdown گروه‌بندی‌شده برند→مدل، swatch رنگ).

## Admin (Phase 9)
- CRUD `VariantAttribute` + `VariantAttributeValue` (pool).
- ویرایشگر محصول: تب «گزینه‌ها/موجودی» = گرید `ProductVariant`. هر ردیف: انتخاب مقدار هر attribute + Sku + قیمت + تخفیف + موجودی + فعال.
- ویرایش سریع موجودی (inline edit ستون Stock).
- toggle `HasVariants`؛ محصول ساده فقط Price/Stock.
