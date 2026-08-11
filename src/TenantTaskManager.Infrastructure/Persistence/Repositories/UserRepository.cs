using Microsoft.EntityFrameworkCore;
using TenantTaskManager.Domain.Entities;
using TenantTaskManager.Application.Abstractions.Persistence;

namespace TenantTaskManager.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<UserAccount?> GetByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                user => user.NormalizedEmail == normalizedEmail,
                cancellationToken);
    }
}