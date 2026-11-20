using LankaOrnaments.Core.Constants;
using LankaOrnaments.Core.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LankaOrnaments.Tests;

/// <summary>
/// CRUD and constraint tests directly against SQL Server. Each test runs inside a transaction
/// that is rolled back, so the test database is left unchanged.
/// </summary>
[Collection(SqlServerCollection.Name)]
public sealed class DatabaseCrudTests(SqlServerFixture fixture)
{
    private static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}"[..Math.Min(prefix.Length + 13, 32)];

    private static Ornament NewOrnament(string? sku = null) => new()
    {
        Sku = sku ?? Unique("T"),
        Name = "Test Ring",
        Description = "Ornament created by DatabaseCrudTests.",
        Category = OrnamentCategories.GoldRings,
        Price = 1000m,
        StockQuantity = 5,
        IsAvailable = true,
        Metal = "18K Yellow Gold",
        WeightGrams = 2.5m,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
    };

    private static ApplicationUser NewUser()
    {
        var email = $"{Guid.NewGuid():N}@example.lk";
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            FullName = "Test User",
            PasswordHash = "not-a-real-hash",
            SecurityStamp = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
        };
    }

    [Fact]
    public async Task Ornament_Create_Read_Update_Delete()
    {
        await using var db = fixture.CreateContext();
        await using var tx = await db.Database.BeginTransactionAsync();

        // Create
        var ornament = NewOrnament();
        db.Ornaments.Add(ornament);
        await db.SaveChangesAsync();
        Assert.True(ornament.OrnamentId > 0);

        // Read (fresh query, not the tracked instance)
        db.ChangeTracker.Clear();
        var read = await db.Ornaments.SingleAsync(o => o.OrnamentId == ornament.OrnamentId);
        Assert.Equal(1000m, read.Price);
        Assert.Equal(DateTimeKind.Utc, read.CreatedAt.Kind);

        // Update
        read.Price = 1500.50m;
        read.StockQuantity = 0;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var updated = await db.Ornaments.SingleAsync(o => o.OrnamentId == ornament.OrnamentId);
        Assert.Equal(1500.50m, updated.Price);
        Assert.Equal(0, updated.StockQuantity);

        // Delete
        db.Ornaments.Remove(updated);
        await db.SaveChangesAsync();
        Assert.False(await db.Ornaments.AnyAsync(o => o.OrnamentId == ornament.OrnamentId));
    }

    [Fact]
    public async Task Ornament_Sku_Is_Unique()
    {
        await using var db = fixture.CreateContext();
        await using var tx = await db.Database.BeginTransactionAsync();

        var sku = Unique("DUP");
        db.Ornaments.Add(NewOrnament(sku));
        await db.SaveChangesAsync();
        db.Ornaments.Add(NewOrnament(sku));

        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.Contains(((SqlException)ex.InnerException!).Number, new[] { 2601, 2627 });
    }

    [Theory]
    [InlineData("price")]
    [InlineData("stock")]
    [InlineData("category")]
    public async Task Ornament_Check_Constraints_Reject_Invalid_Rows(string invalidField)
    {
        await using var db = fixture.CreateContext();
        await using var tx = await db.Database.BeginTransactionAsync();

        var ornament = NewOrnament();
        switch (invalidField)
        {
            case "price": ornament.Price = 0; break;
            case "stock": ornament.StockQuantity = -1; break;
            case "category": ornament.Category = "watches"; break;
        }
        db.Ornaments.Add(ornament);

        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.Equal(547, ((SqlException)ex.InnerException!).Number); // CHECK constraint violation
    }

    [Fact]
    public async Task User_Profile_Preference_Notification_CRUD_And_Cascade_Delete()
    {
        await using var db = fixture.CreateContext();
        await using var tx = await db.Database.BeginTransactionAsync();

        var user = NewUser();
        user.Profile = new Profile { PhoneNumber = "0771234567", Address = "1 Test Road", UpdatedAt = DateTime.UtcNow };
        user.NotificationPreference = new NotificationPreference { EmailNotificationsEnabled = false, InAppNotificationsEnabled = true, UpdatedAt = DateTime.UtcNow };
        user.Notifications.Add(new Notification { Title = "Hello", Message = "Personal notification", Type = NotificationTypes.Account, CreatedAt = DateTime.UtcNow });
        db.Users.Add(user);
        await db.SaveChangesAsync();

        db.ChangeTracker.Clear();
        var loaded = await db.Users
            .Include(u => u.Profile)
            .Include(u => u.NotificationPreference)
            .Include(u => u.Notifications)
            .SingleAsync(u => u.Id == user.Id);
        Assert.Equal("0771234567", loaded.Profile!.PhoneNumber);
        Assert.False(loaded.NotificationPreference!.EmailNotificationsEnabled); // false is persisted, not replaced by a default
        Assert.Single(loaded.Notifications);

        loaded.Profile.Address = "2 Updated Road";
        loaded.Notifications.First().IsRead = true;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        Assert.Equal("2 Updated Road", (await db.Profiles.SingleAsync(p => p.UserId == user.Id)).Address);
        Assert.True((await db.Notifications.SingleAsync(n => n.UserId == user.Id)).IsRead);

        // Deleting the user cascades to profile, preferences and personal notifications (database-level).
        await db.Users.Where(u => u.Id == user.Id).ExecuteDeleteAsync();
        Assert.False(await db.Profiles.AnyAsync(p => p.UserId == user.Id));
        Assert.False(await db.NotificationPreferences.AnyAsync(p => p.UserId == user.Id));
        Assert.False(await db.Notifications.AnyAsync(n => n.UserId == user.Id));
    }

    [Fact]
    public async Task Profile_Is_One_Per_User()
    {
        await using var db = fixture.CreateContext();
        await using var tx = await db.Database.BeginTransactionAsync();

        var user = NewUser();
        db.Users.Add(user);
        await db.SaveChangesAsync();
        db.Profiles.Add(new Profile { UserId = user.Id, UpdatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        // Clear tracking so EF can't "replace" the first profile; the unique index must reject the second row.
        db.ChangeTracker.Clear();
        db.Profiles.Add(new Profile { UserId = user.Id, UpdatedAt = DateTime.UtcNow });

        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.Contains(((SqlException)ex.InnerException!).Number, new[] { 2601, 2627 });
    }

    [Fact]
    public async Task Email_Is_Unique()
    {
        await using var db = fixture.CreateContext();
        await using var tx = await db.Database.BeginTransactionAsync();

        var first = NewUser();
        var second = NewUser();
        second.Email = first.Email;
        second.NormalizedEmail = first.NormalizedEmail;
        db.Users.AddRange(first, second);

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task ShopWide_Notification_Has_Null_User_And_Receipts_Cascade_With_It()
    {
        await using var db = fixture.CreateContext();
        await using var tx = await db.Database.BeginTransactionAsync();

        var user = NewUser();
        var announcement = new Notification { UserId = null, Title = "Sale", Message = "Shop-wide announcement", Type = NotificationTypes.Promotion, CreatedAt = DateTime.UtcNow };
        db.Users.Add(user);
        db.Notifications.Add(announcement);
        await db.SaveChangesAsync();

        db.NotificationReceipts.Add(new NotificationReceipt { NotificationId = announcement.NotificationId, UserId = user.Id, ReadAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        db.ChangeTracker.Clear();
        Assert.Null((await db.Notifications.SingleAsync(n => n.NotificationId == announcement.NotificationId)).UserId);

        await db.Notifications.Where(n => n.NotificationId == announcement.NotificationId).ExecuteDeleteAsync();
        Assert.False(await db.NotificationReceipts.AnyAsync(r => r.NotificationId == announcement.NotificationId));
    }

    [Fact]
    public async Task Notification_Type_Check_Constraint()
    {
        await using var db = fixture.CreateContext();
        await using var tx = await db.Database.BeginTransactionAsync();

        db.Notifications.Add(new Notification { Title = "Bad", Message = "Invalid type", Type = "spam", CreatedAt = DateTime.UtcNow });

        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.Equal(547, ((SqlException)ex.InnerException!).Number);
    }

    [Fact]
    public async Task Migrations_Are_Fully_Applied()
    {
        await using var db = fixture.CreateContext();
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        Assert.False(db.Database.HasPendingModelChanges(), "The model has changes that are not captured in a migration.");
    }
}
