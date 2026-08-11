using TenantTaskManager.Application.Abstractions.Persistence;
using TenantTaskManager.Application.Abstractions.Authentication;

namespace TenantTaskManager.Application.Authentication.Login;

public sealed class LoginHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IAccessTokenGenerator accessTokenGenerator)
{
    public async Task<AccessToken> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Email)
            || string.IsNullOrWhiteSpace(command.Password))
        {
            throw new InvalidCredentialsException();
        }

        var normalizedEmail = command.Email.Trim().ToUpperInvariant();
        var user = await userRepository.GetByNormalizedEmailAsync(
            normalizedEmail,
            cancellationToken);

        if (user is null
            || !passwordHasher.Verify(user.PasswordHash, command.Password))
        {
            throw new InvalidCredentialsException();
        }

        return accessTokenGenerator.Generate(user);
    }
}