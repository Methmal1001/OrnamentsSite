using System.Globalization;
using LankaOrnaments.API.Infrastructure;
using LankaOrnaments.Core.Contracts;
using LankaOrnaments.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LankaOrnaments.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService, IUserService userService, SessionCookies cookies) : ControllerBase
{
    /// <summary>
    /// Signs in. With <c>useCookies=true</c> (browser) the JWT is set as an HttpOnly cookie and left
    /// out of the body; otherwise it is returned for use as a bearer token.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [ProducesResponseType<AuthSessionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<AuthSessionDto> Login(LoginRequest request, [FromQuery] bool useCookies, CancellationToken cancellationToken)
    {
        var session = await authService.LoginAsync(request, cancellationToken);
        return Respond(session, useCookies, persistent: request.RememberMe);
    }

    /// <summary>Creates a customer account and signs it in (same cookie/token behaviour as login).</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [ProducesResponseType<AuthSessionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<AuthSessionDto> Register(RegisterRequest request, [FromQuery] bool useCookies, CancellationToken cancellationToken)
    {
        var session = await authService.RegisterAsync(request, cancellationToken);
        return Respond(session, useCookies, persistent: false);
    }

    /// <summary>
    /// Current session (user + expiry), used by the frontend on start-up.
    /// 204 No Content when signed out; an expired or revoked cookie is cleared at the same time.
    /// </summary>
    [HttpGet("session")]
    [AllowAnonymous]
    [ProducesResponseType<AuthSessionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<AuthSessionDto>> Session(CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            if (Request.Cookies.ContainsKey(cookies.Name)) cookies.Clear(Response);
            return NoContent();
        }

        var user = await userService.GetAsync(User.GetUserId(), cancellationToken);
        var exp = long.Parse(User.FindFirst("exp")!.Value, CultureInfo.InvariantCulture);
        return new AuthSessionDto(null, DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime, user);
    }

    /// <summary>Clears the session cookie. Bearer clients simply discard their token.</summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Logout()
    {
        cookies.Clear(Response);
        return NoContent();
    }

    private AuthSessionDto Respond(AuthSessionDto session, bool useCookies, bool persistent)
    {
        if (!useCookies) return session;
        cookies.Write(Response, session, persistent);
        return session with { Token = null };
    }
}
