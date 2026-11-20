using LankaOrnaments.Core.Constants;
using LankaOrnaments.Core.Contracts;
using LankaOrnaments.Core.Entities;
using LankaOrnaments.Core.Exceptions;
using LankaOrnaments.Core.Interfaces;
using LankaOrnaments.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LankaOrnaments.Infrastructure.Services;

internal sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    LankaOrnamentsDbContext db,
    ITokenService tokenService,
    TimeProvider timeProvider) : IAuthService
{
    private const string InvalidCredentials = "Invalid email or password";

    public async Task<AuthSessionDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim())
            ?? throw new UnauthorizedAppException(InvalidCredentials);

        if (await userManager.IsLockedOutAsync(user))
        {
            throw new UnauthorizedAppException("Too many failed attempts. Please try again in a few minutes.");
        }

        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            await userManager.AccessFailedAsync(user);
            throw new UnauthorizedAppException(InvalidCredentials);
        }

        await userManager.ResetAccessFailedCountAsync(user);
        return await CreateSessionAsync(user, request.RememberMe, cancellationToken);
    }

    public async Task<AuthSessionDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var email = request.Email.Trim();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = request.FullName.Trim(),
            CreatedAt = now,
        };

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        IdentityErrors.ThrowIfFailed(await userManager.CreateAsync(user, request.Password), passwordField: "password");
        IdentityErrors.ThrowIfFailed(await userManager.AddToRoleAsync(user, Roles.Customer));

        db.Profiles.Add(new Profile
        {
            UserId = user.Id,
            PhoneNumber = Mapping.NormalizePhone(request.Phone),
            Address = request.Address.Trim(),
            UpdatedAt = now,
        });
        db.NotificationPreferences.Add(new NotificationPreference
        {
            UserId = user.Id,
            EmailNotificationsEnabled = true,
            InAppNotificationsEnabled = true,
            UpdatedAt = now,
        });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await CreateSessionAsync(user, rememberMe: false, cancellationToken);
    }

    public async Task<AuthSessionDto> CreateSessionAsync(Guid userId, bool rememberMe, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new UnauthorizedAppException("Your session has expired. Please sign in again.");
        return await CreateSessionAsync(user, rememberMe, cancellationToken);
    }

    public async Task<bool> IsSecurityStampValidAsync(Guid userId, string? securityStamp, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(securityStamp)) return false;
        var current = await db.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.SecurityStamp)
            .FirstOrDefaultAsync(cancellationToken);
        return current is not null && string.Equals(current, securityStamp, StringComparison.Ordinal);
    }

    private async Task<AuthSessionDto> CreateSessionAsync(ApplicationUser user, bool rememberMe, CancellationToken cancellationToken)
    {
        var role = await UserQueries.GetRoleAsync(db, user.Id, cancellationToken);
        var profile = await db.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == user.Id, cancellationToken);
        var (token, expiresAt) = tokenService.CreateToken(user, role, rememberMe);
        return new AuthSessionDto(token, expiresAt, Mapping.ToDto(user, profile, role));
    }
}

internal static class UserQueries
{
    /// <summary>Returns "Admin" when the user has the admin role, otherwise "Customer".</summary>
    public static async Task<string> GetRoleAsync(LankaOrnamentsDbContext db, Guid userId, CancellationToken cancellationToken)
    {
        var roles = await (
            from userRole in db.UserRoles
            join role in db.Roles on userRole.RoleId equals role.Id
            where userRole.UserId == userId
            select role.Name).ToListAsync(cancellationToken);

        return roles.Contains(Roles.Admin) ? Roles.Admin : Roles.Customer;
    }
}

internal static class IdentityErrors
{
    /// <summary>Converts Identity errors into a 400 (or 409 for duplicate accounts).</summary>
    public static void ThrowIfFailed(IdentityResult result, string passwordField = "password")
    {
        if (result.Succeeded) return;

        if (result.Errors.Any(e => e.Code is nameof(IdentityErrorDescriber.DuplicateEmail) or nameof(IdentityErrorDescriber.DuplicateUserName)))
        {
            throw new ConflictException("An account with this email already exists");
        }

        var errors = result.Errors
            .GroupBy(e => e.Code.StartsWith("Password", StringComparison.Ordinal) ? passwordField
                : e.Code.Contains("Email", StringComparison.Ordinal) ? "email"
                : "general")
            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

        throw new RequestValidationException(errors, errors.Values.First()[0]);
    }
}
