using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using LankaOrnaments.Core.Constants;

namespace LankaOrnaments.Core.Contracts;

// JSON uses camelCase, matching the Nuxt frontend types in app/types/index.ts.
// Ids are serialised as strings so the frontend contract (`id: string`) is unchanged.

internal static class Patterns
{
    /// <summary>Sri Lankan phone numbers, e.g. 0771234567, +94 77 123 4567, 011-234-5678.</summary>
    public const string SriLankanPhone = @"^\s*(?:\+94|0)(?:[\s-]?\d){9}\s*$";
}

// ---------- Users & authentication ----------

public sealed record UserDto(
    string Id,
    string FullName,
    string Email,
    string Phone,
    string Address,
    string Role,
    DateTime CreatedAt);

/// <summary>
/// Sign-in result. Browser clients receive the token in an HttpOnly cookie, so <see cref="Token"/> is
/// omitted from the body; non-browser clients (e.g. Postman) get it here for the Authorization header.
/// </summary>
public sealed record AuthSessionDto(
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Token,
    DateTime ExpiresAt,
    UserDto User);

public sealed class LoginRequest
{
    [Required, EmailAddress, StringLength(FieldLengths.Email)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public sealed class RegisterRequest
{
    [Required, StringLength(FieldLengths.FullName, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(FieldLengths.Email)]
    public string Email { get; set; } = string.Empty;

    [Required, RegularExpression(Patterns.SriLankanPhone, ErrorMessage = "Enter a valid Sri Lankan phone number")]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required, StringLength(FieldLengths.Address, MinimumLength = 5)]
    public string Address { get; set; } = string.Empty;
}

public sealed class UpdateProfileRequest
{
    [Required, StringLength(FieldLengths.FullName, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(FieldLengths.Email)]
    public string Email { get; set; } = string.Empty;

    [Required, RegularExpression(Patterns.SriLankanPhone, ErrorMessage = "Enter a valid Sri Lankan phone number")]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(FieldLengths.Address, MinimumLength = 5)]
    public string Address { get; set; } = string.Empty;
}

public sealed class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;
}

public sealed record NotificationPreferencesDto(
    bool EmailNotificationsEnabled,
    bool InAppNotificationsEnabled,
    DateTime UpdatedAt);

public sealed class UpdateNotificationPreferencesRequest
{
    public bool EmailNotificationsEnabled { get; set; }

    public bool InAppNotificationsEnabled { get; set; }
}

// ---------- Ornaments ----------

public sealed record OrnamentDto(
    string Id,
    string Sku,
    string Name,
    string CategoryId,
    string Description,
    decimal Price,
    string Availability,
    int StockQuantity,
    bool IsAvailable,
    string Metal,
    decimal WeightGrams,
    string? Gemstone,
    string? ImageUrl,
    bool IsFeatured,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>Optional filters for <c>GET /api/ornaments</c> (all combinable).</summary>
public sealed class OrnamentQuery
{
    /// <summary>Matches name, description, SKU, metal or gemstone.</summary>
    [StringLength(100)]
    public string? Search { get; set; }

    [AllowedValues(null, "", OrnamentCategories.GoldRings, OrnamentCategories.Necklaces, OrnamentCategories.Bracelets,
        OrnamentCategories.Earrings, OrnamentCategories.Pendants, ErrorMessage = "Unknown category")]
    public string? Category { get; set; }

    [AllowedValues(null, "", Constants.Availability.InStock, Constants.Availability.LowStock,
        Constants.Availability.OutOfStock, Constants.Availability.PreOrder, ErrorMessage = "Unknown availability")]
    public string? Availability { get; set; }

    /// <summary><c>featured</c> (default), <c>newest</c>, <c>price-asc</c> or <c>price-desc</c>.</summary>
    [AllowedValues(null, "", "featured", "newest", "price-asc", "price-desc", ErrorMessage = "Unknown sort order")]
    public string? Sort { get; set; }
}

/// <summary>
/// Create/update body. Matches the frontend's <c>OrnamentInput</c>; <see cref="StockQuantity"/>
/// and <see cref="IsAvailable"/> are optional. When omitted they are derived from <see cref="Availability"/>.
/// </summary>
public sealed class OrnamentInput
{
    [Required, StringLength(FieldLengths.Sku, MinimumLength = 2)]
    public string Sku { get; set; } = string.Empty;

    [Required, StringLength(FieldLengths.OrnamentName, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [Required, AllowedValues(OrnamentCategories.GoldRings, OrnamentCategories.Necklaces, OrnamentCategories.Bracelets,
        OrnamentCategories.Earrings, OrnamentCategories.Pendants, ErrorMessage = "Unknown category")]
    public string CategoryId { get; set; } = string.Empty;

    [Required, StringLength(FieldLengths.Description, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "Price must be greater than zero")]
    public decimal Price { get; set; }

    [Required, AllowedValues(Constants.Availability.InStock, Constants.Availability.LowStock,
        Constants.Availability.OutOfStock, Constants.Availability.PreOrder, ErrorMessage = "Unknown availability")]
    public string Availability { get; set; } = Constants.Availability.InStock;

    [Range(0, 100000)]
    public int? StockQuantity { get; set; }

    public bool? IsAvailable { get; set; }

    [Required, StringLength(FieldLengths.Metal)]
    public string Metal { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "100000", ErrorMessage = "Weight must be greater than zero")]
    public decimal WeightGrams { get; set; }

    [StringLength(FieldLengths.Gemstone)]
    public string? Gemstone { get; set; }

    [StringLength(FieldLengths.ImageUrl)]
    [RegularExpression(@"^(https?://|/).+", ErrorMessage = "Use an absolute http(s) URL or a path starting with /")]
    public string? ImageUrl { get; set; }

    public bool IsFeatured { get; set; }
}

// ---------- Notifications ----------

public sealed record NotificationDto(
    string Id,
    string? UserId,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    bool IsShopWide,
    DateTime CreatedAt);

public sealed class CreateNotificationRequest
{
    /// <summary>A customer id, or <c>all</c> for a shop-wide announcement.</summary>
    [Required]
    public string Recipient { get; set; } = "all";

    [Required, StringLength(FieldLengths.NotificationTitle, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(FieldLengths.NotificationMessage, MinimumLength = 10)]
    public string Message { get; set; } = string.Empty;

    [Required, AllowedValues(NotificationTypes.Order, NotificationTypes.Promotion, NotificationTypes.Account,
        NotificationTypes.System, ErrorMessage = "Unknown notification type")]
    public string Type { get; set; } = NotificationTypes.System;
}
