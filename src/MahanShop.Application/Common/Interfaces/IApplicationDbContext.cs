using MahanShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Common.Interfaces;

/// <summary>قرارداد دسترسی داده برای لایه Application — وابستگی به EF مستقیم ممنوع.</summary>
public interface IApplicationDbContext
{
    DbSet<Brand> Brands { get; }
    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<VariantAttribute> VariantAttributes { get; }
    DbSet<VariantAttributeValue> VariantAttributeValues { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<ProductVariantValue> ProductVariantValues { get; }
    DbSet<Feature> Features { get; }
    DbSet<ProductFeature> ProductFeatures { get; }
    DbSet<Tag> Tags { get; }
    DbSet<ProductTag> ProductTags { get; }
    DbSet<ProductComment> ProductComments { get; }
    DbSet<User> Users { get; }
    DbSet<OtpCode> OtpCodes { get; }
    DbSet<Address> Addresses { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Banner> Banners { get; }
    DbSet<HomeSection> HomeSections { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
