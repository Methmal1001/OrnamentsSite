using System.Security.Claims;
using System.Text;
using LankaOrnaments.Core.Entities;
using LankaOrnaments.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace LankaOrnaments.Infrastructure.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(ApplicationUser user, string role, bool rememberMe);
}

internal sealed class TokenService(IOptions<JwtOptions> options, TimeProvider timeProvider) : ITokenService
{
    private readonly JwtOptions _options = options.Value;
    private readonly JsonWebTokenHandler _handler = new();

    public (string Token, DateTime ExpiresAt) CreateToken(ApplicationUser user, string role, bool rememberMe)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var expiresAt = rememberMe
            ? now.AddDays(_options.RememberMeExpiryDays)
            : now.AddMinutes(_options.ExpiryMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = expiresAt,
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Name, user.FullName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("role", role),
                // Changes when the password or email changes; validated on every request (see Program.cs).
                new Claim("stamp", user.SecurityStamp ?? string.Empty),
                // Lets the API re-issue a session with the same lifetime after a profile/password change.
                new Claim("rm", rememberMe ? "1" : "0"),
            ]),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
                SecurityAlgorithms.HmacSha256),
        };

        return (_handler.CreateToken(descriptor), expiresAt);
    }
}
