# DEPLOYMENT — Plesk 18.0.75 Windows shared

> هدف #1: بالا اومدن روی این هاست. مدل = عین سایت فعلی که کار می‌کنه.

## Publish
```
dotnet publish src/MahanShop.Web -c Release -o ./publish
```
- خروجی framework-dependent (هاست .NET 8 runtime داره). نه self-contained مگر لازم شد.
- محتوای `publish/` → آپلود به httpdocs (یا root دامنه در Plesk).

## web.config (الزامی — in-process)
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" arguments=".\MahanShop.Web.dll"
                  stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout"
                  hostingModel="inprocess" />
    </system.webServer>
  </location>
</configuration>
```

## Plesk steps
1. Plesk > Domains > دامنه > .NET settings → .NET 8 runtime فعال.
2. Document root → پوشه publish.
3. Application pool: No Managed Code (AspNetCoreModule خودش host می‌کنه).
4. logs/ folder بساز (stdout اگه دیباگ خواستی → stdoutLogEnabled=true موقت).

## Secrets — هرگز در appsettings.json
Plesk > .NET > Environment Variables (یا System Environment). نام‌ها:
```
ConnectionStrings__EShop_PhoneDb = Data Source=<sql2019-host>,<port>;Initial Catalog=<db>;User Id=<u>;Password=<p>;Encrypt=True;TrustServerCertificate=True;
Sms__ApiKey       = <key>
Zarinpal__MerchantId   = <id>
Zarinpal__CallbackUrl  = https://<domain>/api/Shop/verify
```
- `Encrypt=True` رو connection (نمونه قدیم False بود — اصلاح شد).
- ASP.NET Core خودکار `__` رو به nested config مپ می‌کنه.

## ⚠️ امنیت — اقدام فوری
creds نمونه قدیمی (DB pass, SMS key, Zarinpal merchant) لو رفتن. **rotate** قبل launch.

## DB
- SQL Server 2019 روی هاست/ریموت.
- migration: `dotnet ef database update --project src/MahanShop.Infra.Data --startup-project src/MahanShop.Web`
- یا script: `dotnet ef migrations script -o migrate.sql` → اجرا دستی در Plesk SQL.
