using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TenantTaskManager.Domain.Users;
using TenantTaskManager.Domain.Entities;
using Microsoft.IdentityModel.JsonWebTokens;
using TenantTaskManager.Infrastructure.Authentication;

namespace TenantTaskManager.Infrastructure.Tests.Authentication;

public sealed class JwtAccessTokenGeneratorTests
{
    private static readonly DateTimeOffset CurrentTime =
        new(2026, 8, 11, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Generate_IncludesIdentityTenantAndRoleClaims()
    {
        var user = new UserAccount(
            Guid.NewGuid(),
            "admin@example.com",
            "password-hash",
            UserRole.Admin);
        var generator = CreateGenerator();

        var result = generator.Generate(user);
        var token = new JsonWebTokenHandler().ReadJsonWebToken(result.Value);

        Assert.Equal(user.Id.ToString(), token.GetClaim(JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(user.Email, token.GetClaim(JwtRegisteredClaimNames.Email).Value);
        Assert.Equal(user.TenantId.ToString(), token.GetClaim("tenant_id").Value);
        Assert.Equal(UserRole.Admin.ToString(), token.GetClaim(ClaimTypes.Role).Value);
    }

    [Fact]
    public void Generate_UsesConfiguredIssuerAudienceAndExpiration()
    {
        var generator = CreateGenerator();

        var result = generator.Generate(new UserAccount(
            Guid.NewGuid(),
            "user@example.com",
            "password-hash",
            UserRole.User));

        var token = new JsonWebTokenHandler().ReadJsonWebToken(result.Value);

        Assert.Equal("TenantTaskManager.Api", token.Issuer);
        Assert.Contains("TenantTaskManager.Clients", token.Audiences);
        Assert.Equal(CurrentTime.AddMinutes(30), result.ExpiresAtUtc);
        Assert.Equal(SecurityAlgorithms.HmacSha256, token.Alg);
    }

    [Fact]
    public void Generate_WithShortSecret_ThrowsInvalidOperationException()
    {
        var generator = CreateGenerator(secret: "too-short");

        Assert.Throws<InvalidOperationException>(() => generator.Generate(
            new UserAccount(
                Guid.NewGuid(),
                "user@example.com",
                "password-hash",
                UserRole.User)));
    }

    private static JwtAccessTokenGenerator CreateGenerator(
        string secret = "development-secret-that-is-at-least-32-bytes")
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "TenantTaskManager.Api",
            Audience = "TenantTaskManager.Clients",
            Secret = secret,
            ExpirationMinutes = 30
        });

        return new JwtAccessTokenGenerator(
            options,
            new FixedTimeProvider(CurrentTime));
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}