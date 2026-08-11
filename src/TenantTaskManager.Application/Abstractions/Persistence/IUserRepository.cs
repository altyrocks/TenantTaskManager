using TenantTaskManager.Domain.Entities;

namespace TenantTaskManager.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<UserAccount?> GetByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default);
}