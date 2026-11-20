namespace LankaOrnaments.Core.Entities;

/// <summary>
/// A notification for one customer, or a shop-wide announcement when <see cref="UserId"/> is null.
/// For personal notifications <see cref="IsRead"/> is the read flag; shop-wide announcements
/// track read/dismissed state per customer in <see cref="NotificationReceipt"/>.
/// </summary>
public class Notification
{
    public int NotificationId { get; set; }

    public Guid? UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    /// <summary>One of <see cref="Constants.NotificationTypes"/>.</summary>
    public string Type { get; set; } = Constants.NotificationTypes.System;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public ApplicationUser? User { get; set; }

    public ICollection<NotificationReceipt> Receipts { get; set; } = [];
}

/// <summary>Per-customer state for a shop-wide announcement.</summary>
public class NotificationReceipt
{
    public int NotificationId { get; set; }

    public Guid UserId { get; set; }

    public DateTime? ReadAt { get; set; }

    public DateTime? DismissedAt { get; set; }

    public Notification Notification { get; set; } = null!;

    public ApplicationUser User { get; set; } = null!;
}

public class NotificationPreference
{
    public int PreferenceId { get; set; }

    public Guid UserId { get; set; }

    public bool EmailNotificationsEnabled { get; set; } = true;

    public bool InAppNotificationsEnabled { get; set; } = true;

    public DateTime UpdatedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
