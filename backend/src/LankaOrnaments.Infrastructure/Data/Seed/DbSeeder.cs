using LankaOrnaments.Core.Constants;
using LankaOrnaments.Core.Entities;
using LankaOrnaments.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LankaOrnaments.Infrastructure.Data.Seed;

/// <summary>
/// Idempotent seeding: only inserts what is missing and never updates or deletes existing rows.
/// Account passwords come from configuration (user-secrets / environment variables) and are
/// stored only as Identity password hashes.
/// </summary>
public sealed class DbSeeder(
    LankaOrnamentsDbContext db,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IOptions<SeedOptions> seedOptions,
    TimeProvider timeProvider,
    ILogger<DbSeeder> logger)
{
    private readonly SeedOptions _options = seedOptions.Value;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await EnsureRolesAsync();

        await EnsureAccountAsync(_options.AdminEmail, _options.AdminFullName, _options.AdminPassword, Roles.Admin,
            phone: "0112345678", address: "Lanka Ornaments, 125 Main Street, Colombo 11", configKey: "Seed:AdminPassword");
        var customer = await EnsureAccountAsync(_options.CustomerEmail, _options.CustomerFullName, _options.CustomerPassword, Roles.Customer,
            phone: "0771234567", address: "42 Galle Road, Colombo 03", configKey: "Seed:CustomerPassword");

        if (!_options.SampleData) return;

        if (!await db.Ornaments.AnyAsync(cancellationToken))
        {
            db.Ornaments.AddRange(SampleOrnaments.Create());
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded sample ornaments");
        }

        if (!await db.Notifications.AnyAsync(cancellationToken))
        {
            await SeedNotificationsAsync(customer, cancellationToken);
        }
    }

    private async Task EnsureRolesAsync()
    {
        foreach (var roleName in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                ThrowIfFailed(await roleManager.CreateAsync(new ApplicationRole(roleName)), $"role {roleName}");
                logger.LogInformation("Created role {Role}", roleName);
            }
        }
    }

    private async Task<ApplicationUser?> EnsureAccountAsync(
        string email, string fullName, string? password, string role, string phone, string address, string configKey)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning("{ConfigKey} is not configured; the default {Role} account {Email} was not created", configKey, role, email);
                return null;
            }

            var now = timeProvider.GetUtcNow().UtcDateTime;
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                CreatedAt = now,
                Profile = new Profile { PhoneNumber = phone, Address = address, UpdatedAt = now },
                NotificationPreference = new NotificationPreference
                {
                    EmailNotificationsEnabled = true,
                    InAppNotificationsEnabled = true,
                    UpdatedAt = now,
                },
            };
            ThrowIfFailed(await userManager.CreateAsync(user, password), $"account {email}");
            logger.LogInformation("Created default {Role} account {Email}", role, email);
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            ThrowIfFailed(await userManager.AddToRoleAsync(user, role), $"role assignment for {email}");
        }

        return user;
    }

    private async Task SeedNotificationsAsync(ApplicationUser? customer, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        db.Notifications.Add(new Notification
        {
            UserId = null,
            Type = NotificationTypes.Promotion,
            Title = "Festive Season Sale: 15% off",
            Message = "Enjoy 15% off making charges on all necklaces and bracelets until 31 December.",
            CreatedAt = now.AddDays(-1),
        });

        if (customer is not null)
        {
            db.Notifications.AddRange(
                new Notification
                {
                    UserId = customer.Id,
                    Type = NotificationTypes.Order,
                    Title = "Your reservation is ready",
                    Message = "The Ceylon Blue Sapphire Solitaire you reserved is ready for viewing at our Colombo showroom.",
                    CreatedAt = now.AddDays(-2),
                },
                new Notification
                {
                    UserId = customer.Id,
                    Type = NotificationTypes.Account,
                    Title = "Welcome to Lanka Ornaments",
                    Message = "Your account is ready. Explore our collections and save your favourite pieces.",
                    IsRead = true,
                    CreatedAt = now.AddDays(-7),
                });
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded sample notifications");
    }

    private static void ThrowIfFailed(IdentityResult result, string what)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Seeding {what} failed: {string.Join("; ", result.Errors.Select(e => e.Description))}");
        }
    }
}

public static class DatabaseInitializer
{
    /// <summary>
    /// Optionally applies migrations, then seeds. If migrations are pending and automatic
    /// migration is off, seeding is skipped with a warning (the schema is never changed implicitly).
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        var db = provider.GetRequiredService<LankaOrnamentsDbContext>();
        var options = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
        var logger = provider.GetRequiredService<ILogger<DbSeeder>>();

        if (options.ApplyMigrationsOnStartup)
        {
            await db.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            var pending = (await db.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
            if (pending.Count > 0)
            {
                logger.LogWarning(
                    "Database has {Count} pending migration(s): {Migrations}. Run 'dotnet ef database update'. Seeding skipped.",
                    pending.Count, string.Join(", ", pending));
                return;
            }
        }

        await provider.GetRequiredService<DbSeeder>().SeedAsync(cancellationToken);
    }
}
