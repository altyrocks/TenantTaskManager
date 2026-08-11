using TenantTaskManager.Infrastructure;
using TenantTaskManager.Api.Authentication;
using TenantTaskManager.Application.Abstractions.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentTenant, CurrentTenant>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("The default database connection is not configured.");

builder.Services.AddInfrastructure(connectionString);

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();