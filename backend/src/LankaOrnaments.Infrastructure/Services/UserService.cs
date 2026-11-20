using LankaOrnaments.Core.Constants;
using LankaOrnaments.Core.Contracts;
using LankaOrnaments.Core.Entities;
using LankaOrnaments.Core.Exceptions;
using LankaOrnaments.Core.Interfaces;
using LankaOrnaments.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LankaOrnaments.Infrastructure.Services;

internal sealed class UserService(
    UserManager<ApplicationUser> userManager,
    LankaOrnamentsDbContext db,
    TimeProvider timeProvider) : IUserService
{
    public async Task<UserDto> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.AsNoTracking().Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User not found");

        var role = await UserQueries.GetRoleAsync(db, userId, cancellationToken);
        return Mapping.ToDto(user, user.Profile, role);
    }

    public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await FindUserAsync(userId);
        var email = request.Email.Trim();
        var now = timeProvider.GetUtcNow().UtcDateTime;

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            var owner = await userManager.FindByEmailAsync(email);
            if (owner is not null && owner.Id != userId)
            {
                throw new ConflictException("That email is already used by another account");
            }

            // Marks the address as unconfirmed; verification emails arrive in a later phase.
            IdentityErrors.ThrowIfFailed(await userManager.SetEmailAsync(user, email));
            IdentityErrors.ThrowIfFailed(await userManager.SetUserNameAsync(user, email));
        }

        user.FullName = request.FullName.Trim();
        IdentityErrors.ThrowIfFailed(await userManager.UpdateAsync(user));

        var profile = await db.Profiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        if (profile is null)
        {
            profile = new Profile { UserId = userId };
            db.Profiles.Add(profile);
        }

        profile.PhoneNumber = Mapping.NormalizePhone(request.Phone);
        profile.Address = request.Address.Trim();
        profile.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var role = await UserQueries.GetRoleAsync(db, userId, cancellationToken);
        return Mapping.ToDto(user, profile, role);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await FindUserAsync(userId);
        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.PasswordMismatch)))
        {
            throw new RequestValidationException("currentPassword", "Current password is incorrect");
        }

        IdentityErrors.ThrowIfFailed(result, passwordField: "newPassword");
    }

    public async Task<NotificationPreferencesDto> GetPreferencesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var preference = await GetOrCreatePreferenceAsync(userId, cancellationToken);
        return new NotificationPreferencesDto(preference.EmailNotificationsEnabled, preference.InAppNotificationsEnabled, preference.UpdatedAt);
    }

    public async Task<NotificationPreferencesDto> UpdatePreferencesAsync(Guid userId, UpdateNotificationPreferencesRequest request, CancellationToken cancellationToken = default)
    {
        var preference = await GetOrCreatePreferenceAsync(userId, cancellationToken);
        preference.EmailNotificationsEnabled = request.EmailNotificationsEnabled;
        preference.InAppNotificationsEnabled = request.InAppNotificationsEnabled;
        preference.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync(cancellationToken);
        return new NotificationPreferencesDto(preference.EmailNotificationsEnabled, preference.InAppNotificationsEnabled, preference.UpdatedAt);
    }

    public async Task<IReadOnlyList<UserDto>> ListCustomersAsync(CancellationToken cancellationToken = default)
    {
        var customerRoleIds = db.Roles.Where(r => r.Name == Roles.Customer).Select(r => r.Id);

        var customers = await db.Users.AsNoTracking()
            .Where(u => db.UserRoles.Any(ur => ur.UserId == u.Id && customerRoleIds.Contains(ur.RoleId)))
            .Include(u => u.Profile)
            .OrderBy(u => u.FullName)
            .ToListAsync(cancellationToken);

        return customers.Select(u => Mapping.ToDto(u, u.Profile, Roles.Customer)).ToList();
    }

    private async Task<ApplicationUser> FindUserAsync(Guid userId) =>
        await userManager.FindByIdAsync(userId.ToString()) ?? throw new NotFoundException("User not found");

    private async Task<NotificationPreference> GetOrCreatePreferenceAsync(Guid userId, CancellationToken cancellationToken)
    {
        var preference = await db.NotificationPreferences.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        if (preference is not null) return preference;

        if (!await db.Users.AnyAsync(u => u.Id == userId, cancellationToken))
        {
            throw new NotFoundException("User not found");
        }

        preference = new NotificationPreference
        {
            UserId = userId,
            EmailNotificationsEnabled = true,
            InAppNotificationsEnabled = true,
            UpdatedAt = timeProvider.GetUtcNow().UtcDateTime,
        };
        db.NotificationPreferences.Add(preference);
        await db.SaveChangesAsync(cancellationToken);
        return preference;
    }
}
