using MahanShop.Domain.Enums;

namespace MahanShop.Application.Features.Home.Queries.GetHomePage;

/// <summary>ViewModel کل صفحه اصلی — رندر کامل از این یک شیء.</summary>
public class HomePageViewModel
{
    public List<HomeBannerDto> Banners { get; set; } = new();
    public List<FeaturedCategoryDto> FeaturedCategories { get; set; } = new();
    public List<HomeSectionViewModel> Sections { get; set; } = new();
}

/// <summary>اسلاید بنر هیرو.</summary>
public class HomeBannerDto
{
    public string ImageUrl { get; set; } = null!;
    public string? MobileImageUrl { get; set; }
    public string? LinkUrl { get; set; }
    public string AltText { get; set; } = null!;
    public string? Title { get; set; }
}

/// <summary>دسته منتخب گرید زیر بنر.</summary>
public class FeaturedCategoryDto
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? ImageUrl { get; set; }
}

/// <summary>یک نوار بدنه — یا ProductRow (Products پر) یا PromoBanner (فیلدهای promo پر).</summary>
public class HomeSectionViewModel
{
    public string Title { get; set; } = null!;
    public HomeSectionType SectionType { get; set; }

    // ProductRow
    public List<ProductCardDto> Products { get; set; } = new();
    public string? ViewAllUrl { get; set; }

    // PromoBanner
    public string? ImageUrl { get; set; }
    public string? MobileImageUrl { get; set; }
    public string? LinkUrl { get; set; }
    public string? Subtitle { get; set; }
    public bool IsHalfWidth { get; set; }
}
