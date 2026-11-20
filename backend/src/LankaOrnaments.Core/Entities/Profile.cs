namespace LankaOrnaments.Core.Entities;

/// <summary>Customer contact details (one per user).</summary>
public class Profile
{
    public int ProfileId { get; set; }

    public Guid UserId { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
