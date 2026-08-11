using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TenantTaskManager.Infrastructure.Persistence;
using TenantTaskManager.Application.Abstractions.Persistence;
using TenantTaskManager.Infrastructure.Persistence.Repositories;

namespace TenantTaskManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<ITaskRepository, TaskRepository>();

        return services;
    }
}