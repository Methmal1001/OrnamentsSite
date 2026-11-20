using LankaOrnaments.API.Infrastructure;
using LankaOrnaments.Core.Constants;
using LankaOrnaments.Core.Contracts;
using LankaOrnaments.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LankaOrnaments.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController(INotificationService notificationService) : ControllerBase
{
    /// <summary>The signed-in user's notifications plus shop-wide announcements, newest first.</summary>
    [HttpGet]
    public Task<IReadOnlyList<NotificationDto>> List(CancellationToken cancellationToken) =>
        notificationService.ListForUserAsync(User.GetUserId(), cancellationToken);

    [HttpPut("{id:int}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAsRead(int id, CancellationToken cancellationToken)
    {
        await notificationService.MarkAsReadAsync(User.GetUserId(), id, cancellationToken);
        return NoContent();
    }

    [HttpPut("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        await notificationService.MarkAllAsReadAsync(User.GetUserId(), cancellationToken);
        return NoContent();
    }

    /// <summary>Customers delete their own notification or hide an announcement; admins delete permanently.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await notificationService.DeleteAsync(User.GetUserId(), User.IsInRole(Roles.Admin), id, cancellationToken);
        return NoContent();
    }
}

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public sealed class AdminController(IUserService userService, INotificationService notificationService) : ControllerBase
{
    [HttpGet("customers")]
    public Task<IReadOnlyList<UserDto>> Customers(CancellationToken cancellationToken) =>
        userService.ListCustomersAsync(cancellationToken);

    [HttpGet("notifications")]
    public Task<IReadOnlyList<NotificationDto>> Notifications(CancellationToken cancellationToken) =>
        notificationService.ListAllAsync(cancellationToken);

    /// <summary>Recipient <c>all</c> creates one shop-wide announcement (UserId NULL).</summary>
    [HttpPost("notifications")]
    public Task<IReadOnlyList<NotificationDto>> CreateNotification(CreateNotificationRequest request, CancellationToken cancellationToken) =>
        notificationService.CreateAsync(request, cancellationToken);
}
