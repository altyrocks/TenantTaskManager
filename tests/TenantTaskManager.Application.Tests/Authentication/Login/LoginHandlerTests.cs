using TenantTaskManager.Domain.Users;
using TenantTaskManager.Domain.Entities;
using TenantTaskManager.Application.Authentication.Login;
using TenantTaskManager.Application.Abstractions.Persistence;
using TenantTaskManager.Application.Abstractions.Authentication;

namespace TenantTaskManager.Application.Tests.Authentication.Login;

public sealed class LoginHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidCredentials_ReturnsToken()
    {
        var user = CreateUser();
        var repository = new StubUserRepository(user);
        var passwordHasher = new StubPasswordHasher(true);
        var expectedToken = new AccessToken(
            "access-token",
            DateTimeOffset.UtcNow.AddHours(1));
        var tokenGenerator = new StubAccessTokenGenerator(expectedToken);
        var handler = new LoginHandler(
            repository,
            passwordHasher,
            tokenGenerator);

        var token = await handler.HandleAsync(
            new LoginCommand("  User@Example.com  ", "correct-password"));

        Assert.Equal(expectedToken, token);
        Assert.Equal("USER@EXAMPLE.COM", repository.NormalizedEmail);
        Assert.Equal("password-hash", passwordHasher.PasswordHash);
        Assert.Equal("correct-password", passwordHasher.Password);
        Assert.Same(user, tokenGenerator.User);
    }

    [Fact]
    public async Task HandleAsync_WithUnknownEmail_ThrowsInvalidCredentialsException()
    {
        var passwordHasher = new StubPasswordHasher(true);
        var tokenGenerator = new StubAccessTokenGenerator();
        var handler = new LoginHandler(
            new StubUserRepository(null),
            passwordHasher,
            tokenGenerator);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            handler.HandleAsync(
                new LoginCommand("unknown@example.com", "password")));

        Assert.Null(passwordHasher.Password);
        Assert.Null(tokenGenerator.User);
    }

    [Fact]
    public async Task HandleAsync_WithIncorrectPassword_ThrowsInvalidCredentialsException()
    {
        var tokenGenerator = new StubAccessTokenGenerator();
        var handler = new LoginHandler(
            new StubUserRepository(CreateUser()),
            new StubPasswordHasher(false),
            tokenGenerator);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            handler.HandleAsync(
                new LoginCommand("user@example.com", "incorrect-password")));

        Assert.Null(tokenGenerator.User);
    }

    private static UserAccount CreateUser() => new(
        Guid.NewGuid(),
        "user@example.com",
        "password-hash",
        UserRole.User);

    private sealed class StubUserRepository(UserAccount? user) : IUserRepository
    {
        public string? NormalizedEmail { get; private set; }

        public Task<UserAccount?> GetByNormalizedEmailAsync(
            string normalizedEmail,
            CancellationToken cancellationToken = default)
        {
            NormalizedEmail = normalizedEmail;
            return Task.FromResult(user);
        }

        public Task<IReadOnlyList<UserAccount>> GetAllForCurrentTenantAsync(
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class StubPasswordHasher(bool isValid) : IPasswordHasher
    {
        public string? PasswordHash { get; private set; }

        public string? Password { get; private set; }

        public string Hash(string password) => throw new NotSupportedException();

        public bool Verify(string passwordHash, string password)
        {
            PasswordHash = passwordHash;
            Password = password;
            return isValid;
        }
    }

    private sealed class StubAccessTokenGenerator(
        AccessToken? token = null) : IAccessTokenGenerator
    {
        public UserAccount? User { get; private set; }

        public AccessToken Generate(UserAccount user)
        {
            User = user;
            return token ?? new AccessToken(
                "access-token",
                DateTimeOffset.UtcNow.AddHours(1));
        }
    }
}