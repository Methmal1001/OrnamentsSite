using LankaOrnaments.API.Infrastructure;
using LankaOrnaments.Core.Contracts;
using LankaOrnaments.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LankaOrnaments.API.Controllers;

/// <summary>
/// The signed-in user's profile. Served at <c>/api/profile/me</c>; <c>/api/users/me</c> is kept as an alias
/// for existing clients.
/// </summary>
[ApiController]
[Route("api/profile/me")]
[Route("api/users/me")]
[Authorize]
public sealed class UsersController(IUserService userService, IAuthService authService, SessionCookies cookies) : ControllerBase
{
    [HttpGet]
    public Task<UserDto> Get(CancellationToken cancellationToken) =>
        userService.GetAsync(User.GetUserId(), cancellationToken);

    /// <summary>Updates name, email (unverified until email confirmation exists), phone and address.</summary>
    [HttpPut]
    public async Task<UserDto> Update(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var user = await userService.UpdateProfileAsync(User.GetUserId(), request, cancellationToken);
        // Changing the email rotates the security stamp, which revokes existing tokens; keep this browser signed in.
        await RefreshCookieSessionAsync(cancellationToken);
        return user;
    }

    /// <summary>Changes the password. Other sessions are revoked; this browser session is renewed.</summary>
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        await userService.ChangePasswordAsync(User.GetUserId(), request, cancellationToken);
        await RefreshCookieSessionAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("notification-preferences")]
    public Task<NotificationPreferencesDto> GetPreferences(CancellationToken cancellationToken) =>
        userService.GetPreferencesAsync(User.GetUserId(), cancellationToken);

    [HttpPut("notification-preferences")]
    public Task<NotificationPreferencesDto> UpdatePreferences(UpdateNotificationPreferencesRequest request, CancellationToken cancellationToken) =>
        userService.UpdatePreferencesAsync(User.GetUserId(), request, cancellationToken);

    private async Task RefreshCookieSessionAsync(CancellationToken cancellationToken)
    {
        if (!cookies.IsCookieRequest(Request)) return;
        var rememberMe = User.FindFirst("rm")?.Value == "1";
        var session = await authService.CreateSessionAsync(User.GetUserId(), rememberMe, cancellationToken);
        cookies.Write(Response, session, rememberMe);
    }
}
