namespace TenantTaskManager.Application.Users.GetUsers;

public sealed record UserDto(
    Guid Id,
    string Email,
    string Role,
    DateTimeOffset CreatedAtUtc);