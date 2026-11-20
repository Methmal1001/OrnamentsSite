using Microsoft.AspNetCore.Identity;

namespace LankaOrnaments.Core.Entities;

/// <summary>
/// Identity user, stored in the <c>Users</c> table. The primary key column is
/// <c>UserId</c>; <c>PasswordHash</c> holds an Identity (PBKDF2) hash, never plain text.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public Profile? Profile { get; set; }

    public NotificationPreference? NotificationPreference { get; set; }

    public ICollection<Notification> Notifications { get; set; } = [];
}

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole()
    {
    }

    public ApplicationRole(string roleName) : base(roleName)
    {
    }
}
