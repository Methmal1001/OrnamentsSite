using LankaOrnaments.Core.Constants;
using LankaOrnaments.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LankaOrnaments.Infrastructure.Data.Configurations;

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        var typeList = string.Join(", ", NotificationTypes.All.Select(t => $"N'{t}'"));

        builder.ToTable("Notifications", table =>
            table.HasCheckConstraint("CK_Notifications_Type", $"[Type] IN ({typeList})"));

        builder.HasKey(n => n.NotificationId);

        builder.Property(n => n.Title).HasMaxLength(FieldLengths.NotificationTitle).IsRequired();
        builder.Property(n => n.Message).HasMaxLength(FieldLengths.NotificationMessage).IsRequired();
        builder.Property(n => n.Type).HasMaxLength(FieldLengths.NotificationType).IsRequired()
            .HasDefaultValue(NotificationTypes.System);
        builder.Property(n => n.IsRead).HasDefaultValue(false);
        builder.Property(n => n.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        // UserId is NULL for shop-wide announcements.
        builder.HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        // Supports "my notifications, newest first" and unread counts.
        builder.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });
        builder.HasIndex(n => n.CreatedAt);
    }
}

internal sealed class NotificationReceiptConfiguration : IEntityTypeConfiguration<NotificationReceipt>
{
    public void Configure(EntityTypeBuilder<NotificationReceipt> builder)
    {
        builder.ToTable("NotificationReceipts");
        builder.HasKey(r => new { r.NotificationId, r.UserId });

        builder.HasOne(r => r.Notification)
            .WithMany(n => n.Receipts)
            .HasForeignKey(r => r.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        // NO ACTION avoids a second cascade path from Users (SQL Server restriction);
        // receipts are removed in code before a user is deleted.
        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(r => r.UserId);
    }
}
