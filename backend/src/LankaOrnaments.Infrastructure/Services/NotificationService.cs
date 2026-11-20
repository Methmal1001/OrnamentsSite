using LankaOrnaments.Core.Constants;
using LankaOrnaments.Core.Contracts;
using LankaOrnaments.Core.Entities;
using LankaOrnaments.Core.Exceptions;
using LankaOrnaments.Core.Interfaces;
using LankaOrnaments.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LankaOrnaments.Infrastructure.Services;

internal sealed class NotificationService(LankaOrnamentsDbContext db, TimeProvider timeProvider) : INotificationService
{
    public async Task<IReadOnlyList<NotificationDto>> ListForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var rows = await db.Notifications.AsNoTracking()
            .Where(n => n.UserId == userId
                || (n.UserId == null && !n.Receipts.Any(r => r.UserId == userId && r.DismissedAt != null)))
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.NotificationId)
            .Select(n => new
            {
                Notification = n,
                ReadShopWide = n.Receipts.Any(r => r.UserId == userId && r.ReadAt != null),
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => Mapping.ToDto(r.Notification, r.Notification.UserId is null ? r.ReadShopWide : r.Notification.IsRead))
            .ToList();
    }

    public async Task MarkAsReadAsync(Guid userId, int notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await FindVisibleAsync(userId, notificationId, cancellationToken);
        var now = Now();

        if (notification.UserId is null)
        {
            var receipt = await GetOrAddReceiptAsync(notificationId, userId, cancellationToken);
            receipt.ReadAt ??= now;
        }
        else
        {
            notification.IsRead = true;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = Now();
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        await db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), cancellationToken);

        await db.NotificationReceipts
            .Where(r => r.UserId == userId && r.ReadAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.ReadAt, now), cancellationToken);

        var unseenShopWide = await db.Notifications
            .Where(n => n.UserId == null && !n.Receipts.Any(r => r.UserId == userId))
            .Select(n => n.NotificationId)
            .ToListAsync(cancellationToken);

        db.NotificationReceipts.AddRange(unseenShopWide.Select(id => new NotificationReceipt
        {
            NotificationId = id,
            UserId = userId,
            ReadAt = now,
        }));

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid userId, bool isAdmin, int notificationId, CancellationToken cancellationToken = default)
    {
        if (isAdmin)
        {
            // Receipts are removed by the ON DELETE CASCADE foreign key.
            var deleted = await db.Notifications.Where(n => n.NotificationId == notificationId).ExecuteDeleteAsync(cancellationToken);
            if (deleted == 0) throw new NotFoundException("Notification not found");
            return;
        }

        var notification = await FindVisibleAsync(userId, notificationId, cancellationToken);
        if (notification.UserId is null)
        {
            // Customers can't delete a shop-wide announcement, only hide it for themselves.
            var receipt = await GetOrAddReceiptAsync(notificationId, userId, cancellationToken);
            var now = Now();
            receipt.DismissedAt = now;
            receipt.ReadAt ??= now;
        }
        else
        {
            db.Notifications.Remove(notification);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationDto>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        var notifications = await db.Notifications.AsNoTracking()
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.NotificationId)
            .ToListAsync(cancellationToken);

        // Shop-wide announcements have no single read flag, so they're reported as unread.
        return notifications.Select(n => Mapping.ToDto(n, n.UserId is not null && n.IsRead)).ToList();
    }

    public async Task<IReadOnlyList<NotificationDto>> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default)
    {
        Guid? userId = null;
        if (!string.Equals(request.Recipient, "all", StringComparison.OrdinalIgnoreCase))
        {
            userId = await ResolveCustomerAsync(request, cancellationToken);
        }

        var notification = new Notification
        {
            UserId = userId,
            Title = request.Title.Trim(),
            Message = request.Message.Trim(),
            Type = request.Type,
            IsRead = false,
            CreatedAt = Now(),
        };

        db.Notifications.Add(notification);
        await db.SaveChangesAsync(cancellationToken);
        return [Mapping.ToDto(notification, false)];
    }

    private async Task<Guid> ResolveCustomerAsync(CreateNotificationRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Recipient, out var userId))
        {
            throw new RequestValidationException("recipient", "Recipient must be a customer id or 'all'");
        }

        var isCustomer = await (
            from userRole in db.UserRoles
            join role in db.Roles on userRole.RoleId equals role.Id
            where userRole.UserId == userId && role.Name == Roles.Customer
            select userRole).AnyAsync(cancellationToken);
        if (!isCustomer) throw new NotFoundException("Customer not found");

        // Account messages are always delivered; other types respect the customer's preference.
        var inAppEnabled = await db.NotificationPreferences
            .Where(p => p.UserId == userId)
            .Select(p => (bool?)p.InAppNotificationsEnabled)
            .FirstOrDefaultAsync(cancellationToken) ?? true;
        if (!inAppEnabled && request.Type != NotificationTypes.Account)
        {
            throw new RequestValidationException("recipient", "This customer has turned off in-app notifications");
        }

        return userId;
    }

    private async Task<Notification> FindVisibleAsync(Guid userId, int notificationId, CancellationToken cancellationToken) =>
        await db.Notifications.FirstOrDefaultAsync(
            n => n.NotificationId == notificationId && (n.UserId == userId || n.UserId == null),
            cancellationToken)
        ?? throw new NotFoundException("Notification not found");

    private async Task<NotificationReceipt> GetOrAddReceiptAsync(int notificationId, Guid userId, CancellationToken cancellationToken)
    {
        var receipt = await db.NotificationReceipts.FirstOrDefaultAsync(
            r => r.NotificationId == notificationId && r.UserId == userId, cancellationToken);
        if (receipt is not null) return receipt;

        receipt = new NotificationReceipt { NotificationId = notificationId, UserId = userId };
        db.NotificationReceipts.Add(receipt);
        return receipt;
    }

    private DateTime Now() => timeProvider.GetUtcNow().UtcDateTime;
}
