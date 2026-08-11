using System.Text;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TenantTaskManager.Domain.Entities;
using Microsoft.IdentityModel.JsonWebTokens;
using TenantTaskManager.Application.Abstractions.Authentication;

namespace TenantTaskManager.Infrastructure.Authentication;

public sealed class JwtAccessTokenGenerator(
    IOptions<JwtOptions> options,
    TimeProvider timeProvider) : IAccessTokenGenerator
{
    private readonly JwtOptions _options = options.Value;

    public AccessToken Generate(UserAccount user)
    {
        ArgumentNullException.ThrowIfNull(user);
        ValidateOptions();

        var issuedAt = timeProvider.GetUtcNow();
        var expiresAt = issuedAt.AddMinutes(_options.ExpirationMinutes);
        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.Secret));
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("tenant_id", user.TenantId.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = issuedAt.UtcDateTime,
            NotBefore = issuedAt.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256)
        };
        var token = new JsonWebTokenHandler().CreateToken(descriptor);

        return new AccessToken(token, expiresAt);
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.Issuer)
            || string.IsNullOrWhiteSpace(_options.Audience)
            || Encoding.UTF8.GetByteCount(_options.Secret) < 32
            || _options.ExpirationMinutes <= 0)
        {
            throw new InvalidOperationException("The JWT configuration is invalid.");
        }
    }
}