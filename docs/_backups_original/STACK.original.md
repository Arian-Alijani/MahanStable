# STACK — Pinned versions

> منبع حقیقت نسخه‌ها. EF Core و runtime باید هم‌نسخه net8.0 بمونن. نسخه جدید = اول اینجا آپدیت کن.

## Target
- TargetFramework: `net8.0`
- SDK: .NET 8 SDK (dev) / ASP.NET Core 8 Runtime (host)
- Hosting model: IIS in-process (`AspNetCoreModuleV2`)
- DB engine: SQL Server 2019 (compat level 150)

## NuGet packages (پین‌شده روی 8.0.x)

### Web (MyEshop_Phone)
```
Microsoft.EntityFrameworkCore.Design        8.0.*
Microsoft.EntityFrameworkCore.Tools         8.0.*
QuestPDF                                     2024.*  (Community license)
```

### Application
```
MediatR                                      12.*
Microsoft.EntityFrameworkCore                8.0.*
FluentValidation                             11.*   (اعتبارسنجی command/query)
```

### Domain
```
(بدون وابستگی خارجی — pure C#)
```

### Infra.Data
```
Microsoft.EntityFrameworkCore                8.0.*
Microsoft.EntityFrameworkCore.SqlServer      8.0.*
Microsoft.EntityFrameworkCore.Design         8.0.*
```

### Infra.IoC
```
Microsoft.Extensions.DependencyInjection     8.0.*
```

### امنیت / هدرها
```
NWebsec.AspNetCore.Middleware  (یا CSP دستی) — security headers
Rate limiting: بومی .NET 8 (Microsoft.AspNetCore.RateLimiting) — پکیج لازم نیست
```

## Frontend libs (wwwroot/lib — بدون npm/Node)
```
Bootstrap 5.x   (RTL build برای فارسی)
jQuery 3.x
jquery-validation + unobtrusive
فونت: Vazir (wwwroot/fonts/Vazir.ttf)
```

## نکات سازگاری
- EF Core 8 → SQL Server 2016+ پشتیبانی. SQL 2019 کامل OK.
- QuestPDF: فونت فارسی Vazir باید register بشه (RTL).
- بدون build pipeline سمت Node → asset‌ها مستقیم در wwwroot، bundle با کتابخانه بومی یا دستی minify.
