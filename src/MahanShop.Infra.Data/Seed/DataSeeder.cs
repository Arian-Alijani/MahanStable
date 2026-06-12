using MahanShop.Application.Common;
using MahanShop.Domain.Entities;
using MahanShop.Domain.Enums;
using MahanShop.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Infra.Data.Seed;

/// <summary>
/// Seed داده‌ی نمونه (dev). idempotent — هر بخش فقط اگه خالیه پر می‌شه.
/// کاتالوگ نمونه + محتوای صفحه اصلی (بنر/دسته منتخب/نوارها). تصاویر = placeholder local.
/// </summary>
public static class DataSeeder
{
    /// <summary>شماره موبایل ادمین اولیه (کاربر استاندارد فقط با IsAdmin=true).</summary>
    private const string InitialAdminPhone = "09037882674";

    /// <summary>seed داده‌ی نمونه (فقط Development): migrate + کاتالوگ + محتوای صفحه اصلی + ادمین اولیه.</summary>
    public static async Task SeedAsync(MyDbContext db)
    {
        await db.Database.MigrateAsync();
        await SeedCatalogAsync(db);
        await SeedHomeAsync(db);
        await SeedAdminAsync(db);
    }

    /// <summary>
    /// فقط ادمین اولیه را تضمین می‌کند (بدون داده‌ی نمونه). در همه‌ی محیط‌ها (شامل Production)
    /// قابل فراخوانی است تا شماره‌ی ادمین همیشه دسترسی پنل داشته باشد. migrate را خودش انجام می‌دهد.
    /// </summary>
    public static async Task SeedAdminOnlyAsync(MyDbContext db)
    {
        await db.Database.MigrateAsync();
        await SeedAdminAsync(db);
    }

    /// <summary>
    /// ادمین اولیه را به شکل استاندارد می‌سازد: یک رکورد User معمولی (همان ساختار کاربرانِ
    /// ساخته‌شده با ورود OTP) که تنها تفاوتش <c>IsAdmin = true</c> است.
    /// idempotent: اگر کاربر با این شماره موجود نباشد ساخته می‌شود؛ اگر موجود باشد و ادمین نباشد
    /// نقش ادمین به او داده می‌شود؛ اگر از قبل ادمین باشد تغییری ایجاد نمی‌شود.
    /// </summary>
    private static async Task SeedAdminAsync(MyDbContext db)
    {
        var phone = PhoneNumberHelper.Normalize(InitialAdminPhone);
        if (phone is null) return;

        var user = await db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
        if (user is null)
        {
            db.Users.Add(new User
            {
                PhoneNumber = phone,
                FullName = string.Empty,
                IsAdmin = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
        else if (!user.IsAdmin || !user.IsActive)
        {
            user.IsAdmin = true;
            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedCatalogAsync(MyDbContext db)
    {
        if (await db.Products.AnyAsync()) return;

        var samsung = new Brand { Name = "سامسونگ", Slug = "samsung", DisplayOrder = 1 };
        var apple = new Brand { Name = "اپل", Slug = "apple", DisplayOrder = 2 };
        var xiaomi = new Brand { Name = "شیائومی", Slug = "xiaomi", DisplayOrder = 3 };
        db.Brands.AddRange(samsung, apple, xiaomi);

        var phones = new Category { Name = "گوشی موبایل", Slug = "mobile", ImageUrl = "/img/categories/mobile.svg", DisplayOrder = 1, ShowOnHome = true };
        var cases = new Category { Name = "قاب و محافظ", Slug = "case", ImageUrl = "/img/categories/case.svg", DisplayOrder = 2, ShowOnHome = true };
        var chargers = new Category { Name = "شارژر و کابل", Slug = "charger", ImageUrl = "/img/categories/charger.svg", DisplayOrder = 3, ShowOnHome = true };
        var airpods = new Category { Name = "هندزفری", Slug = "airpod", ImageUrl = "/img/categories/airpod.svg", DisplayOrder = 4, ShowOnHome = true };
        var glass = new Category { Name = "گلس و محافظ صفحه", Slug = "glass", ImageUrl = "/img/categories/glass.svg", DisplayOrder = 5, ShowOnHome = true };
        var gadget = new Category { Name = "گجت", Slug = "gadget", ImageUrl = "/img/categories/gadget.svg", DisplayOrder = 6, ShowOnHome = true };
        db.Categories.AddRange(phones, cases, chargers, airpods, glass, gadget);

        // ویژگیِ رنگ (global، IsColor) + مقادیرش
        var colorAttr = new VariantAttribute { Name = "رنگ", IsColor = true, DisplayOrder = 1 };
        var black = new VariantAttributeValue { Value = "مشکی", ColorHex = "#111827", DisplayOrder = 1 };
        var white = new VariantAttributeValue { Value = "سفید", ColorHex = "#F9FAFB", DisplayOrder = 2 };
        var blue = new VariantAttributeValue { Value = "آبی", ColorHex = "#2563EB", DisplayOrder = 3 };
        var gold = new VariantAttributeValue { Value = "طلایی", ColorHex = "#D4AF37", DisplayOrder = 4 };
        colorAttr.Values.Add(black);
        colorAttr.Values.Add(white);
        colorAttr.Values.Add(blue);
        colorAttr.Values.Add(gold);
        db.VariantAttributes.Add(colorAttr);

        await db.SaveChangesAsync();

        var rnd = 0;
        // محصول variant‌دار: یک variant به ازای هر رنگ. Product.Price/Stock = rollup (min قیمت / Σ موجودی).
        Product P(string title, string slug, Brand brand, Category cat, long price, long? discount,
            bool featured, int views, params VariantAttributeValue[] colors)
        {
            var p = new Product
            {
                Title = title,
                Slug = slug,
                ShortDescription = title + " اصل با گارانتی",
                HasVariants = true,
                Price = discount ?? price,           // rollup: کمترین قیمت فروش
                DiscountPrice = discount,
                Stock = colors.Length * 10,          // rollup: Σ موجودی variantها
                IsActive = true,
                IsFeatured = featured,
                ViewCount = views,
                BrandId = brand.Id,
                CategoryId = cat.Id,
                Images = { new ProductImage { Url = $"/img/products/p{++rnd}.svg", Alt = title, IsMain = true, DisplayOrder = 0 } },
            };
            int ord = 0;
            foreach (var c in colors)
            {
                var v = new ProductVariant
                {
                    Sku = $"{slug}-{ord + 1}",
                    Price = price,
                    DiscountPrice = discount,
                    Stock = 10,
                    IsActive = true,
                    DisplayOrder = ord++,
                };
                v.Values.Add(new ProductVariantValue { AttributeValue = c });
                p.Variants.Add(v);
            }
            return p;
        }

        db.Products.AddRange(
            P("گلکسی S24 اولترا", "galaxy-s24-ultra", samsung, phones, 62_000_000, 57_900_000, true, 1200, black, gold, blue),
            P("آیفون 15 پرو مکس", "iphone-15-pro-max", apple, phones, 89_000_000, null, true, 2100, black, white, blue),
            P("شیائومی 14", "xiaomi-14", xiaomi, phones, 41_500_000, 38_900_000, true, 800, black, white),
            P("گلکسی A55", "galaxy-a55", samsung, phones, 24_900_000, 22_500_000, false, 640, black, blue),
            P("قاب سیلیکونی S24", "case-s24-silicone", samsung, cases, 850_000, 590_000, true, 320, black, blue, white),
            P("قاب مگ‌سیف آیفون 15", "case-iphone15-magsafe", apple, cases, 1_290_000, 990_000, false, 410, black, white),
            P("شارژر 25 وات سامسونگ", "charger-25w", samsung, chargers, 1_650_000, 1_390_000, true, 530, black, white),
            P("کابل USB-C اپل", "cable-usbc-apple", apple, chargers, 980_000, null, false, 290, white),
            P("ایرپاد پرو 2", "airpods-pro-2", apple, airpods, 18_900_000, 16_750_000, true, 1500, white),
            P("هندزفری بازباندز", "buds-fe", samsung, airpods, 4_200_000, null, false, 360, black, white),
            P("گلس فول آیفون 15", "glass-iphone15", apple, glass, 450_000, 290_000, false, 220, black),
            P("ساعت گلکسی واچ 6", "galaxy-watch-6", samsung, gadget, 19_500_000, 17_900_000, true, 700, black, gold)
        );

        await db.SaveChangesAsync();
    }

    private static async Task SeedHomeAsync(MyDbContext db)
    {
        if (!await db.Banners.AnyAsync())
        {
            db.Banners.AddRange(
                new Banner { Title = "فروش ویژه گلکسی", ImageUrl = "/img/banners/banner-1.svg", MobileImageUrl = "/img/banners/banner-1-m.svg", LinkUrl = "/mobile", AltText = "فروش ویژه گوشی‌های سامسونگ", DisplayOrder = 1 },
                new Banner { Title = "آیفون ۱۵ موجود شد", ImageUrl = "/img/banners/banner-2.svg", MobileImageUrl = "/img/banners/banner-2-m.svg", LinkUrl = "/mobile", AltText = "عرضه آیفون ۱۵ پرو مکس", DisplayOrder = 2 },
                new Banner { Title = "تخفیف لوازم جانبی", ImageUrl = "/img/banners/banner-3.svg", MobileImageUrl = "/img/banners/banner-3-m.svg", LinkUrl = "/charger", AltText = "حراج شارژر و کابل", DisplayOrder = 3 }
            );
        }

        if (!await db.HomeSections.AnyAsync())
        {
            var caseCat = await db.Categories.FirstOrDefaultAsync(c => c.Slug == "case");

            db.HomeSections.AddRange(
                new HomeSection { Title = "محصولات پیشنهادی", SectionType = HomeSectionType.ProductRow, ProductSource = HomeProductSource.Featured, MaxItems = 10, DisplayOrder = 1 },
                new HomeSection { Title = "فروش ویژه و تخفیف‌دار", SectionType = HomeSectionType.ProductRow, ProductSource = HomeProductSource.Discounted, MaxItems = 10, DisplayOrder = 2 },
                new HomeSection { Title = "", SectionType = HomeSectionType.PromoBanner, ImageUrl = "/img/banners/promo-wide.svg", MobileImageUrl = "/img/banners/promo-wide-m.svg", LinkUrl = "/case", Subtitle = "جدیدترین قاب‌ها", IsHalfWidth = false, DisplayOrder = 3 },
                new HomeSection { Title = "جدیدترین محصولات", SectionType = HomeSectionType.ProductRow, ProductSource = HomeProductSource.Newest, MaxItems = 10, DisplayOrder = 4 },
                new HomeSection { Title = "قاب و محافظ‌های جدید", SectionType = HomeSectionType.ProductRow, ProductSource = HomeProductSource.ByCategory, CategoryId = caseCat?.Id, MaxItems = 10, DisplayOrder = 5 },
                // دو بنر با DisplayOrder یکسان (۶) → در یک ردیف، کنار هم، با تقسیم مساوی فضا
                new HomeSection { Title = "", SectionType = HomeSectionType.PromoBanner, ImageUrl = "/img/banners/promo-charger.svg", LinkUrl = "/charger", Subtitle = "شارژرهای سریع", IsHalfWidth = true, DisplayOrder = 6 },
                new HomeSection { Title = "", SectionType = HomeSectionType.PromoBanner, ImageUrl = "/img/banners/promo-airpod.svg", LinkUrl = "/airpod", Subtitle = "هندزفری بی‌سیم", IsHalfWidth = true, DisplayOrder = 6 }
            );
        }

        await db.SaveChangesAsync();
    }
}
