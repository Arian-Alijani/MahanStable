using MahanShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MahanShop.Infra.Data.Configurations;

public class BannerConfiguration : IEntityTypeConfiguration<Banner>
{
    public void Configure(EntityTypeBuilder<Banner> b)
    {
        b.Property(x => x.Title).HasMaxLength(200);
        b.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();
        b.Property(x => x.MobileImageUrl).HasMaxLength(500);
        b.Property(x => x.LinkUrl).HasMaxLength(500);
        b.Property(x => x.AltText).HasMaxLength(300).IsRequired();
        b.HasIndex(x => x.DisplayOrder);
    }
}

public class HomeSectionConfiguration : IEntityTypeConfiguration<HomeSection>
{
    public void Configure(EntityTypeBuilder<HomeSection> b)
    {
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.ImageUrl).HasMaxLength(500);
        b.Property(x => x.MobileImageUrl).HasMaxLength(500);
        b.Property(x => x.LinkUrl).HasMaxLength(500);
        b.Property(x => x.Subtitle).HasMaxLength(300);
        b.HasIndex(x => x.DisplayOrder);

        b.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);  // حذف دسته → نوار نمی‌میره
    }
}
