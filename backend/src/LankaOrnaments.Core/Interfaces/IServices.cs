using LankaOrnaments.Core.Contracts;

namespace LankaOrnaments.Core.Interfaces;

public interface IAuthService
{
    Task<AuthSessionDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<AuthSessionDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>Issues a fresh session for an existing user (e.g. after the security stamp changed).</summary>
    Task<AuthSessionDto> CreateSessionAsync(Guid userId, bool rememberMe, CancellationToken cancellationToken = default);

    /// <summary>True when the token's security stamp still matches the user (password/email unchanged).</summary>
    Task<bool> IsSecurityStampValidAsync(Guid userId, string? securityStamp, CancellationToken cancellationToken = default);
}

public interface IUserService
{
    Task<UserDto> GetAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);

    Task<NotificationPreferencesDto> GetPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<NotificationPreferencesDto> UpdatePreferencesAsync(Guid userId, UpdateNotificationPreferencesRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserDto>> ListCustomersAsync(CancellationToken cancellationToken = default);
}

public interface IOrnamentService
{
    Task<IReadOnlyList<OrnamentDto>> ListAsync(OrnamentQuery? query = null, CancellationToken cancellationToken = default);

    Task<OrnamentDto> GetAsync(int id, CancellationToken cancellationToken = default);

    Task<OrnamentDto> CreateAsync(OrnamentInput input, CancellationToken cancellationToken = default);

    Task<OrnamentDto> UpdateAsync(int id, OrnamentInput input, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public interface INotificationService
{
    /// <summary>Personal notifications plus shop-wide announcements the customer has not dismissed.</summary>
    Task<IReadOnlyList<NotificationDto>> ListForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(Guid userId, int notificationId, CancellationToken cancellationToken = default);

    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Customers delete their own notifications or dismiss announcements; admins delete permanently.</summary>
    Task DeleteAsync(Guid userId, bool isAdmin, int notificationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationDto>> ListAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationDto>> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default);
}
