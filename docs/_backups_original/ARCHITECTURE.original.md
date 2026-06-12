# ARCHITECTURE — Clean Architecture (Onion)

> قواعد وابستگی مقدسن. وابستگی فقط رو به داخل. نقض = build رد بشه.

## Projects (solution = MahanShop.sln)
```
MahanShop.sln
├── src/
│   ├── MahanShop.Domain          ← هسته. Entity, Enum, ValueObject. بدون وابستگی.
│   ├── MahanShop.Application      ← منطق. CQRS (MediatR), Interface, DTO, Validator.
│   │                                 وابسته: Domain فقط.
│   ├── MahanShop.Infra.Data       ← EF Core, DbContext, Repository, Migration.
│   │                                 وابسته: Application + Domain.
│   ├── MahanShop.Infra.IoC        ← ثبت DI. سیم‌کشی همه لایه‌ها.
│   └── MahanShop.Web              ← ASP.NET Core MVC+RazorPages. Controller, View, wwwroot.
│                                     وابسته: همه (از طریق IoC).
```

## Dependency rule (جهت وابستگی)
```
Web → IoC → Infra.Data → Application → Domain
                              ↘ Domain ↗
```
- Domain هیچی نمی‌دونه.
- Application فقط Domain + interface (نه EF مستقیم؛ از `IApplicationDbContext` استفاده کن).
- Infra.Data پیاده‌سازی interfaceهای Application.
- Web فقط Controller/View — بدون منطق business، فقط MediatR `Send`.

## CQRS pattern (MediatR)
- هر use case = یک `Command` یا `Query` + `Handler`.
- مسیر: `Application/Features/<Area>/Commands|Queries/<Name>/`
  - مثال: `Application/Features/Products/Queries/GetProductList/GetProductListQuery.cs` + `Handler.cs`
- اعتبارسنجی: FluentValidation `Validator` کنار هر Command.
- Controller فقط: model bind → `_mediator.Send(command)` → return View/Result.

## DbContext
- `IApplicationDbContext` در Application (interface).
- `MyDbContext : DbContext, IApplicationDbContext` در Infra.Data.
- Migration فقط در Infra.Data.

## Folder map — Web
```
MahanShop.Web/
├── Controllers/        (Home, Shop, ShopCart, Account, Search, PaymentResult, FilterProducts)
├── Views/              (per controller + Shared/_Layout)
├── Pages/              (Admin/, PanelUser/ — Razor Pages)
├── ViewComponents/     (BestSeller و …)
├── wwwroot/            (css, js, lib, fonts, AdminPanel/Photo)
├── Program.cs
└── web.config          (Plesk — AspNetCoreModuleV2 in-process)
```

## قواعد ثابت
- ساختار Controller/Action عین نمونه نگه‌دار (URL سازگار): `{controller=Home}/{action=Index}/{id?}`.
- صفحات Admin + PanelUser = Razor Pages.
- صفحات فروشگاه عمومی = MVC Controller + View.
- Photo upload → `wwwroot/AdminPanel/Photo/...` (مثل نمونه).
