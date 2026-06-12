# Phase 3 — Layout + data-driven Home UI — Design Spec

> Status: approved 2026-06-11. Stack locked (CLAUDE.md). Domestic-only, Bootstrap 5 RTL, no CDN.

## Goal
Render the homepage from DB so the (future) admin panel can fully control it: banners, featured
categories, and an **orderable list of body rows** (product carousels + promo banners). Visual
direction = the `Old UI/index.html` sample, modern & minimal, but rebuilt in Bootstrap 5 RTL with
all assets self-hosted. Favorites/"محبوب" removed entirely.

## 1. Data model (Domain + migration `Add_HomeContent`)

### New entity `Banner` (hero slider slides)
- `Title?`, `ImageUrl`, `MobileImageUrl?`, `LinkUrl?`, `AltText`, `DisplayOrder`, `IsActive`

### New entity `HomeSection` (the orderable body — admin reorders by `DisplayOrder`)
- Common: `Title`, `SectionType`, `DisplayOrder`, `IsActive`
- `SectionType` enum (`Domain/Enums/HomeSectionType`) = `ProductRow | PromoBanner`
- ProductRow fields: `ProductSource` enum (`Domain/Enums/HomeProductSource` =
  `Featured | BestSelling | Newest | Discounted | ByCategory`), `CategoryId?` (FK Category, used by
  `ByCategory`), `MaxItems` (default 10)
- PromoBanner fields: `ImageUrl`, `MobileImageUrl?`, `LinkUrl?`, `Subtitle?`, `IsHalfWidth`
  (two consecutive half-width promo banners render side-by-side = sample's 2-up promo cards)

### `Category` — add field
- `ShowOnHome` bool (default false) — selects the featured-category grid below the banner.

### Wiring
- `IApplicationDbContext`: add `DbSet<Banner> Banners`, `DbSet<HomeSection> HomeSections`.
- `MyDbContext`: DbSets + apply config.
- New `Infra.Data/Configurations/HomeConfigurations.cs` (Fluent): keys, max lengths,
  `HomeSection.Category` FK `DeleteBehavior.SetNull`, indexes on `DisplayOrder`.
- Migration `Add_HomeContent` (Banners, HomeSections tables + `Category.ShowOnHome`).
- **Seed** (in migration or a `HomeSeeder` run at startup): 2–3 banners (local placeholder imgs),
  top-level categories `ShowOnHome=true`, sections in order:
  1. ProductRow `Featured` "محصولات پیشنهادی"
  2. ProductRow `Discounted` "فروش ویژه / تخفیف‌دار"
  3. PromoBanner full-width (mid banner)
  4. ProductRow `Newest` "جدیدترین محصولات"
  5. ProductRow `ByCategory`(قاب) "قاب و محافظ‌های جدید"
  6. 2× PromoBanner `IsHalfWidth` (شارژر / اکسسوری)

> **Discount row** = `ProductSource.Discounted` → `Products` where
> `DiscountPrice != null && DiscountPrice < Price && IsActive`. Satisfies "one discount-only row".
> **BestSelling** = sum `OrderItem.Quantity` desc; no orders yet → fall back to `ViewCount` desc.

## 2. Application (CQRS, `Application/Features/Home/Queries/`)
- `GetHomePageQuery` → `HomePageViewModel { Banners, FeaturedCategories, Sections }`.
  - Handler: `AsNoTracking` queries. For each active `HomeSection` ordered by `DisplayOrder`:
    ProductRow → resolve `List<ProductCardDto>` by source (max `MaxItems`); PromoBanner → promo data.
  - `ProductCardDto`: `Id, Title, Slug, BrandName, Price, DiscountPrice, DiscountPercent,
    PrimaryImageUrl, ColorHexes (List<string>), CategorySlug`.
  - `HomeSectionViewModel`: section meta + (`Products` | promo fields) + `ViewAllUrl`.
- `GetMenuCategoriesQuery` → category tree where `ShowInMenu`, ordered — for header mega-menu.
- ViewModels live in the feature folder (entity never passed to View — CONVENTIONS).

## 3. Web (MVC + ViewComponents)
- `HomeController.Index` → `_mediator.Send(GetHomePageQuery)` → `Views/Home/Index.cshtml`.
- ViewComponents (`Web/ViewComponents/`):
  - `CategoryMenuViewComponent` → header mega-menu (invoked in `_Header`).
  - (Roadmap "BestSeller ViewComponent" is folded into ProductRow `source=BestSelling`.)
- Partials (`Views/Home/` + `Views/Shared/`):
  - `_HeroBanner`, `_CategoryGrid`, `_ProductRow`, `_PromoBanner`, **`_ProductCard`** (reused).
- Layout: rewrite `_Layout.cshtml` (RTL, `lang="fa"`), new `_Header` + `_Footer` partials,
  mobile drawer. **Remove all favorites**: header heart icon, card heart button, drawer "محبوب‌ها".
  Card keeps only the add-to-cart icon (cart wired in P6; now a link/no-op).

## 4. Frontend (domestic-only — zero CDN/external)
- **Bootstrap 5 RTL** (`lib/bootstrap/dist/css/bootstrap.rtl.min.css`) + custom `wwwroot/css/site.css`
  rebuilding the sample look: bg `#F8FAFC`, primary `#2563EB`, ink `#1D1B20`, white rounded cards
  (`rounded-3`), horizontal **snap carousels**, overlay category cards, **discount % badge** on
  discounted cards.
- **Vazirmatn** self-hosted `@font-face` (woff2 → `wwwroot/fonts/vazirmatn/`).
- Icons: **Bootstrap Icons** local font (`wwwroot/lib/bootstrap-icons/`) replacing Material Symbols.
- jQuery already local. `wwwroot/js/site.js`: banner slider (auto + dots), drag/snap product rows,
  mobile drawer open/close, desktop mega-menu hover/focus.
- ⚠️ **Asset acquisition task**: vendor Vazirmatn woff2 + Bootstrap-Icons font/css locally
  (check `Old Ui + Back End from host` / `MyEshop_Phone-master` first; else download on dev machine).
  Runtime must stay 100% self-hosted.

## 5. SEO (cross-cutting from P3)
- Home: dynamic `<title>` + meta description, canonical, Open Graph/Twitter, `lang/dir`,
  single `<h1>` (sr-only ok), `alt` on all imgs.

## 6. Done criteria
- `dotnet build` green; `dotnet run` → home renders from DB seed.
- Mobile responsive; header mega-menu + mobile drawer work.
- Discount row shows ONLY discounted products.
- Admin-reorderable model in place (DisplayOrder) — CRUD/drag deferred to P9.
- grep over `Views/`, `css/`, `js/` → zero external domain/CDN (domestic-only audit passes).
- No favorites anywhere.

## Out of scope (later phases)
- Admin CRUD + drag-reorder UI (P9). Cart/add-to-cart logic (P6). Product detail/list (P4).
- Product JSON-LD structured data (P4).
