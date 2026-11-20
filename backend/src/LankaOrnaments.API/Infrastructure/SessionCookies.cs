using LankaOrnaments.Core.Contracts;
using Microsoft.Extensions.Options;

namespace LankaOrnaments.API.Infrastructure;

/// <summary>Auth cookie settings (section <c>Auth</c>).</summary>
public sealed class AuthCookieOptions
{
    public const string SectionName = "Auth";

    /// <summary>Use a <c>__Host-</c> prefix in production (requires HTTPS, Path=/ and no Domain).</summary>
    public string CookieName { get; set; } = "lo_session";

    /// <summary>
    /// Strict works when the frontend and API share a site (e.g. localhost:3000 → localhost:5196, or
    /// www.example.lk → api.example.lk). Use None (HTTPS only) if they are on different sites.
    /// </summary>
    public SameSiteMode SameSite { get; set; } = SameSiteMode.Strict;
}

/// <summary>Issues and clears the HttpOnly session cookie that carries the JWT for browser clients.</summary>
public sealed class SessionCookies(IOptions<AuthCookieOptions> options, IWebHostEnvironment environment)
{
    private readonly AuthCookieOptions _options = options.Value;

    public string Name => _options.CookieName;

    public void Write(HttpResponse response, AuthSessionDto session, bool persistent)
    {
        var cookie = BaseOptions();
        // "Remember me" → persistent cookie; otherwise it ends with the browser session (the JWT still expires).
        if (persistent) cookie.Expires = new DateTimeOffset(session.ExpiresAt, TimeSpan.Zero);
        response.Cookies.Append(_options.CookieName, session.Token!, cookie);
    }

    public void Clear(HttpResponse response) => response.Cookies.Delete(_options.CookieName, BaseOptions());

    /// <summary>True when the current request was authenticated with the cookie rather than a bearer header.</summary>
    public bool IsCookieRequest(HttpRequest request) =>
        string.IsNullOrEmpty(request.Headers.Authorization) && request.Cookies.ContainsKey(_options.CookieName);

    private CookieOptions BaseOptions() => new()
    {
        HttpOnly = true,
        // Always Secure outside Development; in Development http://localhost is used.
        Secure = !environment.IsDevelopment(),
        SameSite = _options.SameSite,
        Path = "/",
        IsEssential = true,
    };
}

/// <summary>
/// CSRF defence for cookie authentication: state-changing requests must carry
/// <c>X-Requested-With</c>. Browsers only let a cross-origin page add that header after a
/// CORS preflight, which the API allows solely for configured frontend origins.
/// Requests using an Authorization header are not CSRF-prone and are exempt.
/// </summary>
public sealed class CsrfHeaderMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Requested-With";

    public Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        var unsafeMethod = !(HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method) || HttpMethods.IsOptions(request.Method));

        if (unsafeMethod
            && request.Path.StartsWithSegments("/api")
            && string.IsNullOrEmpty(request.Headers.Authorization)
            && string.IsNullOrEmpty(request.Headers[HeaderName]))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return context.Response.WriteAsJsonAsync(new
            {
                title = "Missing anti-forgery header",
                status = 403,
                detail = $"State-changing requests must include the {HeaderName} header.",
            });
        }

        return next(context);
    }
}
