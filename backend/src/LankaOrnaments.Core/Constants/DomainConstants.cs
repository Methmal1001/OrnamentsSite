namespace LankaOrnaments.Core.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Customer = "Customer";

    public static readonly IReadOnlyList<string> All = [Admin, Customer];
}

/// <summary>Category identifiers, shared with the frontend (<c>categoryId</c>).</summary>
public static class OrnamentCategories
{
    public const string GoldRings = "gold-rings";
    public const string Necklaces = "necklaces";
    public const string Bracelets = "bracelets";
    public const string Earrings = "earrings";
    public const string Pendants = "pendants";

    public static readonly IReadOnlyList<string> All = [GoldRings, Necklaces, Bracelets, Earrings, Pendants];
}

/// <summary>Availability labels exposed by the API, derived from stock data.</summary>
public static class Availability
{
    public const string InStock = "in-stock";
    public const string LowStock = "low-stock";
    public const string OutOfStock = "out-of-stock";
    public const string PreOrder = "pre-order";

    /// <summary>Stock at or below this level is reported as "low-stock".</summary>
    public const int LowStockThreshold = 3;

    public static readonly IReadOnlyList<string> All = [InStock, LowStock, OutOfStock, PreOrder];
}

public static class NotificationTypes
{
    public const string Order = "order";
    public const string Promotion = "promotion";
    public const string Account = "account";
    public const string System = "system";

    public static readonly IReadOnlyList<string> All = [Order, Promotion, Account, System];
}

/// <summary>Column lengths shared by the EF configuration and request validation.</summary>
public static class FieldLengths
{
    public const int FullName = 100;
    public const int Email = 256;
    public const int Phone = 20;
    public const int Address = 500;
    public const int OrnamentName = 150;
    public const int Sku = 32;
    public const int Category = 32;
    public const int Description = 2000;
    public const int ImageUrl = 2048;
    public const int Metal = 50;
    public const int Gemstone = 50;
    public const int NotificationTitle = 150;
    public const int NotificationMessage = 1000;
    public const int NotificationType = 20;
}
