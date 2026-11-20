using System.ComponentModel.DataAnnotations;

namespace LankaOrnaments.Infrastructure.Options;

/// <summary>JWT settings (section <c>Jwt</c>). The signing key must come from user-secrets or an environment variable.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = "LankaOrnaments.API";

    [Required]
    public string Audience { get; set; } = "LankaOrnaments.Frontend";

    /// <summary>At least 32 characters. Set with <c>Jwt__SigningKey</c>; never commit it.</summary>
    [Required, MinLength(32)]
    public string SigningKey { get; set; } = string.Empty;

    [Range(5, 1440)]
    public int ExpiryMinutes { get; set; } = 120;

    [Range(1, 30)]
    public int RememberMeExpiryDays { get; set; } = 7;
}

/// <summary>
/// Seeding settings (section <c>Seed</c>). Default accounts are created only when their
/// password is supplied through user-secrets or environment variables.
/// </summary>
public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public bool SampleData { get; set; } = true;

    public string AdminEmail { get; set; } = "admin@lankaornaments.lk";

    public string AdminFullName { get; set; } = "Store Administrator";

    public string? AdminPassword { get; set; }

    public string CustomerEmail { get; set; } = "customer@lankaornaments.lk";

    public string CustomerFullName { get; set; } = "Nethmi Perera";

    public string? CustomerPassword { get; set; }
}

/// <summary>Database settings (section <c>Database</c>).</summary>
public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>
    /// Apply pending migrations at startup. Off by default: production databases are
    /// migrated explicitly with <c>dotnet ef database update</c> or a migration script.
    /// </summary>
    public bool ApplyMigrationsOnStartup { get; set; }
}
