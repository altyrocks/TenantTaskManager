using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TenantTaskManager.Infrastructure.Persistence;

namespace TenantTaskManager.Api.IntegrationTests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = $"ApiTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "integration-tests-only-secret-key-1234567890",
                ["DevelopmentSeed:AdminPassword"] = "Admin123!",
                ["DevelopmentSeed:UserPassword"] = "User123!",
                ["DevelopmentSeed:SecondUserPassword"] = "Other123!"
            });
        });
        builder.ConfigureServices(services =>
        {
            var dbContextRegistrations = services
                .Where(service =>
                    service.ServiceType == typeof(DbContextOptions<AppDbContext>)
                    || service.ServiceType == typeof(IDbContextOptionsConfiguration<AppDbContext>))
                .ToList();

            foreach (var descriptor in dbContextRegistrations)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(databaseName));
        });
    }
}
