using System.Security.Claims;
using LankaOrnaments.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LankaOrnaments.API.Infrastructure;

/// <summary>Turns expected application exceptions into RFC 7807 ProblemDetails responses.</summary>
internal sealed class AppExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not AppException appException) return false;

        ProblemDetails problem = appException is RequestValidationException validation
            ? new ValidationProblemDetails(validation.Errors.ToDictionary(e => e.Key, e => e.Value))
            {
                Title = "One or more validation errors occurred.",
                Detail = validation.Message,
            }
            : new ProblemDetails { Title = appException.Message, Detail = appException.Message };

        problem.Status = appException.StatusCode;
        httpContext.Response.StatusCode = appException.StatusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });
    }
}

internal static class ClaimsPrincipalExtensions
{
    /// <summary>The user id from the JWT <c>sub</c> claim.</summary>
    public static Guid GetUserId(this ClaimsPrincipal principal) =>
        Guid.TryParse(principal.FindFirstValue("sub"), out var id)
            ? id
            : throw new UnauthorizedAppException("Invalid access token");
}
