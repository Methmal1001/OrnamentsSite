using LankaOrnaments.Core.Constants;
using LankaOrnaments.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LankaOrnaments.Infrastructure.Data.Configurations;

internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users");
        builder.Property(u => u.Id).HasColumnName("UserId");

        builder.Property(u => u.FullName).HasMaxLength(FieldLengths.FullName).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(FieldLengths.Email).IsRequired();
        builder.Property(u => u.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        // Identity already adds a (non-unique) EmailIndex on NormalizedEmail; make it unique.
        builder.HasIndex(u => u.NormalizedEmail).HasDatabaseName("EmailIndex").IsUnique()
            .HasFilter("[NormalizedEmail] IS NOT NULL");
    }
}

internal sealed class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("Profiles");
        builder.HasKey(p => p.ProfileId);

        builder.Property(p => p.PhoneNumber).HasMaxLength(FieldLengths.Phone);
        builder.Property(p => p.Address).HasMaxLength(FieldLengths.Address);
        builder.Property(p => p.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        // One profile per user; removed together with the user.
        builder.HasIndex(p => p.UserId).IsUnique();
        builder.HasOne(p => p.User)
            .WithOne(u => u.Profile)
            .HasForeignKey<Profile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("NotificationPreferences");
        builder.HasKey(p => p.PreferenceId);

        builder.Property(p => p.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(p => p.UserId).IsUnique();
        builder.HasOne(p => p.User)
            .WithOne(u => u.NotificationPreference)
            .HasForeignKey<NotificationPreference>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
