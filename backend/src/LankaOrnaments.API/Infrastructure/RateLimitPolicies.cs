namespace LankaOrnaments.API.Infrastructure;

public static class RateLimitPolicies
{
    /// <summary>Login/register: limits password guessing and account spam per client IP.</summary>
    public const string Auth = "auth";
}
