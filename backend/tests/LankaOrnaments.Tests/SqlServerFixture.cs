using LankaOrnaments.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LankaOrnaments.Tests;

/// <summary>
/// Shared SQL Server test database (<c>LankaOrnamentsDB_Tests</c>), created and migrated once per run
/// with the same migrations as production. Override with the <c>LANKA_TEST_CONNECTION</c> environment variable.
/// Tests never drop the database; DbContext tests roll back their transactions and API tests use unique data.
/// </summary>
public sealed class SqlServerFixture : IAsyncLifetime
{
    public static readonly string ConnectionString =
        Environment.GetEnvironmentVariable("LANKA_TEST_CONNECTION")
        ?? @"Server=.\SQLEXPRESS;Database=LankaOrnamentsDB_Tests;Trusted_Connection=True;TrustServerCertificate=True;";

    public LankaOrnamentsDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<LankaOrnamentsDbContext>().UseSqlServer(ConnectionString).Options);

    public async Task InitializeAsync()
    {
        await using var db = CreateContext();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

[CollectionDefinition(Name)]
public sealed class SqlServerCollection : ICollectionFixture<SqlServerFixture>
{
    public const string Name = "SQL Server";
}
