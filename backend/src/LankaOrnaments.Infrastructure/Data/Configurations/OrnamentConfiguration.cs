using LankaOrnaments.Core.Constants;
using LankaOrnaments.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LankaOrnaments.Infrastructure.Data.Configurations;

internal sealed class OrnamentConfiguration : IEntityTypeConfiguration<Ornament>
{
    public void Configure(EntityTypeBuilder<Ornament> builder)
    {
        var categoryList = string.Join(", ", OrnamentCategories.All.Select(c => $"N'{c}'"));

        builder.ToTable("Ornaments", table =>
        {
            table.HasCheckConstraint("CK_Ornaments_Price_Positive", "[Price] > 0");
            table.HasCheckConstraint("CK_Ornaments_StockQuantity_NonNegative", "[StockQuantity] >= 0");
            table.HasCheckConstraint("CK_Ornaments_WeightGrams_Positive", "[WeightGrams] > 0");
            table.HasCheckConstraint("CK_Ornaments_Category", $"[Category] IN ({categoryList})");
        });

        builder.HasKey(o => o.OrnamentId);

        builder.Property(o => o.Sku).HasMaxLength(FieldLengths.Sku).IsRequired();
        builder.Property(o => o.Name).HasMaxLength(FieldLengths.OrnamentName).IsRequired();
        builder.Property(o => o.Description).HasMaxLength(FieldLengths.Description).IsRequired();
        builder.Property(o => o.Category).HasMaxLength(FieldLengths.Category).IsRequired();
        builder.Property(o => o.Price).HasPrecision(18, 2);
        builder.Property(o => o.ImageUrl).HasMaxLength(FieldLengths.ImageUrl);
        builder.Property(o => o.StockQuantity).HasDefaultValue(0);
        builder.Property(o => o.Metal).HasMaxLength(FieldLengths.Metal).IsRequired();
        builder.Property(o => o.WeightGrams).HasPrecision(8, 2);
        builder.Property(o => o.Gemstone).HasMaxLength(FieldLengths.Gemstone);
        builder.Property(o => o.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(o => o.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(o => o.Sku).IsUnique();
        builder.HasIndex(o => o.Category);
        builder.HasIndex(o => o.Name);
        builder.HasIndex(o => new { o.IsFeatured, o.CreatedAt });
    }
}
