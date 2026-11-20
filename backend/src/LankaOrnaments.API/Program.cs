using System.Text;
using System.Threading.RateLimiting;
using LankaOrnaments.API.Infrastructure;
using LankaOrnaments.Core.Interfaces;
using LankaOrnaments.Infrastructure;
using LankaOrnaments.Infrastructure.Data;
using LankaOrnaments.Infrastructure.Data.Seed;
using LankaOrnaments.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Database (SQL Server via EF Core), Identity and application services.
// The connection string is read from ConnectionStrings__DefaultConnection.
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddOpenApi();

builder.Services.Configure<AuthCookieOptions>(builder.Configuration.GetSection(AuthCookieOptions.SectionName));
builder.Services.AddSingleton<SessionCookies>();

// JWT authentication. Browsers send the token in the HttpOnly session cookie;
// other clients may use "Authorization: Bearer". Every token's security stamp is checked,
// so changing the password or email revokes previously issued tokens.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();
builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>, SessionCookies>((bearer, jwtOptions, cookies) =>
    {
        var jwt = jwtOptions.Value;
        bearer.MapInboundClaims = false;
        bearer.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ValidateIssuerSigningKey = true,
            NameClaimType = "name",
            RoleClaimType = "role",
            ClockSkew = TimeSpan.FromSeconds(30),
        };
        bearer.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrEmpty(context.Request.Headers.Authorization)
                    && context.Request.Cookies.TryGetValue(cookies.Name, out var token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = async context =>
            {
                var principal = context.Principal!;
                var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
                var valid = Guid.TryParse(principal.FindFirst("sub")?.Value, out var userId)
                    && await authService.IsSecurityStampValidAsync(userId, principal.FindFirst("stamp")?.Value, context.HttpContext.RequestAborted);
                if (!valid) context.Fail("The session has been revoked.");
            },
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(RateLimitPolicies.Auth, httpContext => RateLimitPartition.GetFixedWindowLimiter(
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = builder.Configuration.GetValue("RateLimiting:AuthPermitsPerMinute", 20),
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        }));
});

// Credentialed CORS for the configured frontend origins only (never "*" with cookies).
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:3000"];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(allowedOrigins)
    .WithMethods("GET", "POST", "PUT", "DELETE")
    .WithHeaders("Content-Type", "Authorization", CsrfHeaderMiddleware.HeaderName)
    .AllowCredentials()
    .SetPreflightMaxAge(TimeSpan.FromMinutes(10))));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Lanka Ornaments API v1"));
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseMiddleware<CsrfHeaderMiddleware>();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/api/health", async (LankaOrnamentsDbContext db, CancellationToken cancellationToken) =>
        await db.Database.CanConnectAsync(cancellationToken)
            ? Results.Ok(new { status = "Healthy", database = db.Database.GetDbConnection().Database })
            : Results.Problem("Database unavailable", statusCode: StatusCodes.Status503ServiceUnavailable))
    .AllowAnonymous();

// Seed roles, sample data and default accounts (insert-only, idempotent).
await app.Services.InitializeDatabaseAsync();

await app.RunAsync();

/// <summary>Exposed for WebApplicationFactory in the integration tests.</summary>
public partial class Program;
