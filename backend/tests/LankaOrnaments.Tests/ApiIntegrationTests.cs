using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using LankaOrnaments.Core.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LankaOrnaments.Tests;

/// <summary>Hosts the real API (Program.cs) against the SQL Server test database.</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public const string AdminPassword = "TestAdmin@2026";
    public const string CustomerPassword = "TestCustomer@2026";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", SqlServerFixture.ConnectionString);
        builder.UseSetting("Jwt:SigningKey", Convert.ToBase64String(RandomNumberGenerator.GetBytes(48)));
        builder.UseSetting("Database:ApplyMigrationsOnStartup", "true");
        builder.UseSetting("Seed:AdminPassword", AdminPassword);
        builder.UseSetting("Seed:CustomerPassword", CustomerPassword);
        builder.UseSetting("RateLimiting:AuthPermitsPerMinute", "1000");
    }

    /// <summary>HTTPS client (the session cookie is Secure outside Development) with the CSRF header.</summary>
    public HttpClient CreateBrowserLikeClient(bool csrfHeader = true)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), HandleCookies = true });
        if (csrfHeader) client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        return client;
    }
}

[Collection(SqlServerCollection.Name)]
public sealed class ApiIntegrationTests(SqlServerFixture fixture)
{
    private readonly SqlServerFixture _fixture = fixture;

    /// <summary>Bearer-token client (non-browser API usage).</summary>
    private static async Task<HttpClient> LoginWithBearerAsync(ApiFactory factory, string email, string password)
    {
        var client = factory.CreateBrowserLikeClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password, rememberMe = false });
        response.EnsureSuccessStatusCode();
        var session = (await response.Content.ReadFromJsonAsync<AuthSessionDto>())!;
        Assert.False(string.IsNullOrEmpty(session.Token));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
        return client;
    }

    private static Task<HttpResponseMessage> RegisterWithCookieAsync(HttpClient client, string email, string password) =>
        client.PostAsJsonAsync("/api/auth/register?useCookies=true",
            new { fullName = "Integration Customer", email, phone = "0712345678", password, address = "1 Test Lane, Galle" });

    [Fact]
    public async Task Admin_Can_Create_Update_Delete_Ornament_Through_Api()
    {
        await using var factory = new ApiFactory();
        var admin = await LoginWithBearerAsync(factory, "admin@lankaornaments.lk", ApiFactory.AdminPassword);
        var sku = $"IT-{Guid.NewGuid():N}"[..20];
        var input = new
        {
            sku, name = "Integration Test Pendant", categoryId = "pendants",
            description = "Created by ApiIntegrationTests.", price = 25000m, availability = "pre-order",
            metal = "18K Yellow Gold", weightGrams = 2.1m, gemstone = "Moonstone", imageUrl = (string?)null, isFeatured = false,
        };

        var created = await admin.PostAsJsonAsync("/api/ornaments", input);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var ornament = (await created.Content.ReadFromJsonAsync<OrnamentDto>())!;
        Assert.Equal("pre-order", ornament.Availability);

        // Server-side search / filter
        var found = (await admin.GetFromJsonAsync<List<OrnamentDto>>($"/api/ornaments?search={sku}&category=pendants&availability=pre-order"))!;
        Assert.Equal(ornament.Id, Assert.Single(found).Id);
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.GetAsync("/api/ornaments?category=watches")).StatusCode);

        var updated = await admin.PutAsJsonAsync($"/api/ornaments/{ornament.Id}", input with { name = "Renamed Pendant", availability = "in-stock" });
        updated.EnsureSuccessStatusCode();
        var after = (await updated.Content.ReadFromJsonAsync<OrnamentDto>())!;
        Assert.Equal("Renamed Pendant", after.Name);
        Assert.Equal("in-stock", after.Availability);

        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/api/ornaments/{ornament.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await admin.GetAsync($"/api/ornaments/{ornament.Id}")).StatusCode);
    }

    [Fact]
    public async Task Cookie_Session_Register_Profile_Notifications_And_Logout()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateBrowserLikeClient();
        var email = $"it-{Guid.NewGuid():N}@example.lk";

        var register = await RegisterWithCookieAsync(client, email, "Register@2026");
        register.EnsureSuccessStatusCode();
        var setCookie = Assert.Single(register.Headers.GetValues("Set-Cookie"));
        Assert.Contains("lo_session=", setCookie, StringComparison.Ordinal);
        Assert.Contains("httponly", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"token\"", await register.Content.ReadAsStringAsync(), StringComparison.Ordinal);

        // Session restore and the /api/profile/me route use the cookie automatically.
        var session = (await client.GetFromJsonAsync<AuthSessionDto>("/api/auth/session"))!;
        Assert.Equal(email, session.User.Email);
        Assert.True(session.ExpiresAt > DateTime.UtcNow);

        var me = (await client.GetFromJsonAsync<UserDto>("/api/profile/me"))!;
        Assert.Equal("customer", me.Role);
        Assert.Equal("0712345678", me.Phone);

        var update = await client.PutAsJsonAsync("/api/profile/me",
            new { fullName = "Renamed Customer", email = $"new-{email}", phone = "0779998888", address = "2 New Road, Kandy" });
        update.EnsureSuccessStatusCode();
        // Email change rotates the security stamp; the cookie must have been renewed.
        Assert.Contains(update.Headers.GetValues("Set-Cookie"), c => c.StartsWith("lo_session=", StringComparison.Ordinal));
        Assert.Equal("Renamed Customer", (await client.GetFromJsonAsync<UserDto>("/api/profile/me"))!.FullName);

        // Password is stored only as an Identity hash.
        await using (var db = _fixture.CreateContext())
        {
            var hash = db.Users.Single(u => u.Email == $"new-{email}").PasswordHash!;
            Assert.DoesNotContain("Register@2026", hash, StringComparison.Ordinal);
            Assert.StartsWith("AQAAAA", hash, StringComparison.Ordinal);
        }

        // Preferences
        var prefs = await client.PutAsJsonAsync("/api/profile/me/notification-preferences", new { emailNotificationsEnabled = false, inAppNotificationsEnabled = true });
        prefs.EnsureSuccessStatusCode();
        Assert.False((await prefs.Content.ReadFromJsonAsync<NotificationPreferencesDto>())!.EmailNotificationsEnabled);

        // Notification round-trip
        var admin = await LoginWithBearerAsync(factory, "admin@lankaornaments.lk", ApiFactory.AdminPassword);
        var sent = await admin.PostAsJsonAsync("/api/admin/notifications",
            new { recipient = me.Id, title = "Integration", message = "Integration test notification.", type = "account" });
        sent.EnsureSuccessStatusCode();
        var notificationId = (await sent.Content.ReadFromJsonAsync<List<NotificationDto>>())![0].Id;

        Assert.Contains((await client.GetFromJsonAsync<List<NotificationDto>>("/api/notifications"))!, n => n.Id == notificationId && !n.IsRead);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsync($"/api/notifications/{notificationId}/read", null)).StatusCode);
        Assert.Contains((await client.GetFromJsonAsync<List<NotificationDto>>("/api/notifications"))!, n => n.Id == notificationId && n.IsRead);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/notifications/{notificationId}")).StatusCode);

        // Logout clears the cookie; the session endpoint then returns 401.
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync("/api/auth/logout", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.GetAsync("/api/auth/session")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/profile/me")).StatusCode);
    }

    [Fact]
    public async Task Cookie_Requests_Without_Csrf_Header_Are_Rejected()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateBrowserLikeClient();
        (await RegisterWithCookieAsync(client, $"csrf-{Guid.NewGuid():N}@example.lk", "Csrf@20261")).EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Remove("X-Requested-With");
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/notifications")).StatusCode); // safe method
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PutAsync("/api/notifications/read-all", null)).StatusCode);
    }

    [Fact]
    public async Task Password_Change_Revokes_Other_Sessions_But_Keeps_Current()
    {
        await using var factory = new ApiFactory();
        var email = $"rev-{Guid.NewGuid():N}@example.lk";
        var browserA = factory.CreateBrowserLikeClient();
        (await RegisterWithCookieAsync(browserA, email, "Original@2026")).EnsureSuccessStatusCode();

        var browserB = factory.CreateBrowserLikeClient();
        (await browserB.PostAsJsonAsync("/api/auth/login?useCookies=true", new { email, password = "Original@2026", rememberMe = true })).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, (await browserB.GetAsync("/api/auth/session")).StatusCode);

        var change = await browserA.PostAsJsonAsync("/api/profile/me/change-password", new { currentPassword = "Original@2026", newPassword = "Changed@2026" });
        Assert.Equal(HttpStatusCode.NoContent, change.StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await browserA.GetAsync("/api/auth/session")).StatusCode);         // renewed
        Assert.Equal(HttpStatusCode.NoContent, (await browserB.GetAsync("/api/auth/session")).StatusCode);    // revoked
        Assert.Equal(HttpStatusCode.Unauthorized, (await browserB.GetAsync("/api/profile/me")).StatusCode);
    }

    [Fact]
    public async Task Validation_Errors_Are_Field_Specific()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateBrowserLikeClient();
        var response = await client.PostAsJsonAsync("/api/auth/register?useCookies=true",
            new { fullName = "X", email = "not-an-email", phone = "123", password = "short", address = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = (await response.Content.ReadFromJsonAsync<ValidationProblem>())!;
        foreach (var field in new[] { "FullName", "Email", "Phone", "Password", "Address" })
        {
            Assert.True(problem.Errors.ContainsKey(field), $"missing error for {field}");
        }
    }

    [Fact]
    public async Task Protected_Endpoints_Enforce_Roles()
    {
        await using var factory = new ApiFactory();
        var anonymous = factory.CreateBrowserLikeClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/profile/me")).StatusCode);

        var customer = await LoginWithBearerAsync(factory, "customer@lankaornaments.lk", ApiFactory.CustomerPassword);
        Assert.Equal(HttpStatusCode.Forbidden, (await customer.GetAsync("/api/admin/customers")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await customer.DeleteAsync("/api/ornaments/1")).StatusCode);
    }

    private sealed record ValidationProblem(Dictionary<string, string[]> Errors);
}
