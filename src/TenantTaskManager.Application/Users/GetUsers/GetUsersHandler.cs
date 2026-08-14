using TenantTaskManager.Application.Abstractions.Persistence;

namespace TenantTaskManager.Application.Users.GetUsers;

public sealed class GetUsersHandler(IUserRepository userRepository)
{
    public async Task<IReadOnlyList<UserDto>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await userRepository
            .GetAllForCurrentTenantAsync(cancellationToken);

        return users
            .Select(user => new UserDto(
                user.Id,
                user.Email,
                user.Role.ToString(),
                user.CreatedAtUtc))
            .ToList();
    }
}