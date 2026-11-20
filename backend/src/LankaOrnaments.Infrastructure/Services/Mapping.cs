using System.Globalization;
using System.Text.RegularExpressions;
using LankaOrnaments.Core.Constants;
using LankaOrnaments.Core.Contracts;
using LankaOrnaments.Core.Entities;

namespace LankaOrnaments.Infrastructure.Services;

internal static partial class Mapping
{
    public static UserDto ToDto(ApplicationUser user, Profile? profile, string role) => new(
        user.Id.ToString(),
        user.FullName,
        user.Email ?? string.Empty,
        profile?.PhoneNumber ?? string.Empty,
        profile?.Address ?? string.Empty,
        role.ToLowerInvariant(),
        user.CreatedAt);

    public static OrnamentDto ToDto(Ornament o) => new(
        o.OrnamentId.ToString(CultureInfo.InvariantCulture),
        o.Sku,
        o.Name,
        o.Category,
        o.Description,
        o.Price,
        AvailabilityOf(o.StockQuantity, o.IsAvailable),
        o.StockQuantity,
        o.IsAvailable,
        o.Metal,
        o.WeightGrams,
        o.Gemstone,
        o.ImageUrl,
        o.IsFeatured,
        o.CreatedAt,
        o.UpdatedAt);

    public static NotificationDto ToDto(Notification n, bool isRead) => new(
        n.NotificationId.ToString(CultureInfo.InvariantCulture),
        n.UserId?.ToString(),
        n.Title,
        n.Message,
        n.Type,
        isRead,
        n.UserId is null,
        n.CreatedAt);

    /// <summary>Availability label shown to customers, derived from the stored stock data.</summary>
    public static string AvailabilityOf(int stockQuantity, bool isAvailable) =>
        !isAvailable ? Availability.OutOfStock
        : stockQuantity == 0 ? Availability.PreOrder
        : stockQuantity <= Availability.LowStockThreshold ? Availability.LowStock
        : Availability.InStock;

    /// <summary>
    /// Copies an <see cref="OrnamentInput"/> onto an entity. If the client sends only an
    /// availability label, stock and availability are derived so the label round-trips.
    /// </summary>
    public static void Apply(OrnamentInput input, Ornament target)
    {
        target.Sku = input.Sku.Trim().ToUpperInvariant();
        target.Name = input.Name.Trim();
        target.Category = input.CategoryId;
        target.Description = input.Description.Trim();
        target.Price = input.Price;
        target.Metal = input.Metal.Trim();
        target.WeightGrams = input.WeightGrams;
        target.Gemstone = string.IsNullOrWhiteSpace(input.Gemstone) ? null : input.Gemstone.Trim();
        target.ImageUrl = string.IsNullOrWhiteSpace(input.ImageUrl) ? null : input.ImageUrl.Trim();
        target.IsFeatured = input.IsFeatured;

        if (input.StockQuantity is int stock)
        {
            target.StockQuantity = stock;
            target.IsAvailable = input.IsAvailable ?? input.Availability != Availability.OutOfStock;
            return;
        }

        var current = target.StockQuantity;
        (target.StockQuantity, target.IsAvailable) = input.Availability switch
        {
            Availability.OutOfStock => (0, false),
            Availability.PreOrder => (0, true),
            Availability.LowStock => (current is > 0 and <= Availability.LowStockThreshold ? current : Availability.LowStockThreshold, true),
            _ => (current > Availability.LowStockThreshold ? current : 10, true),
        };
        if (input.IsAvailable is bool available) target.IsAvailable = available;
    }

    /// <summary>Stores phone numbers without spaces or dashes.</summary>
    public static string NormalizePhone(string phone) => PhoneSeparators().Replace(phone.Trim(), string.Empty);

    [GeneratedRegex(@"[\s-]")]
    private static partial Regex PhoneSeparators();
}
