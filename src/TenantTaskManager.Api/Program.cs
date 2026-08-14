using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using TenantTaskManager.Infrastructure;
using TenantTaskManager.Api.Authentication;
using TenantTaskManager.Application.Tasks.GetTasks;
using TenantTaskManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TenantTaskManager.Application.Tasks.CreateTask;
using TenantTaskManager.Infrastructure.Authentication;
using TenantTaskManager.Application.Tasks.CompleteTask;
using TenantTaskManager.Application.Tasks.UpdateTask;
using TenantTaskManager.Application.Authentication.Login;
using TenantTaskManager.Application.Abstractions.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentTenant, CurrentTenant>();
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<CreateTaskHandler>();
builder.Services.AddScoped<GetTasksHandler>();
builder.Services.AddScoped<CompleteTaskHandler>();
builder.Services.AddScoped<UpdateTaskHandler>();

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtOptions = jwtSection.Get<JwtOptions>()
    ?? throw new InvalidOperationException("The JWT configuration is missing.");

if (string.IsNullOrWhiteSpace(jwtOptions.Issuer)
    || string.IsNullOrWhiteSpace(jwtOptions.Audience)
    || Encoding.UTF8.GetByteCount(jwtOptions.Secret) < 32)
{
    throw new InvalidOperationException("The JWT configuration is invalid.");
}

builder.Services.Configure<JwtOptions>(jwtSection);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IAccessTokenGenerator, JwtAccessTokenGenerator>();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "sub",
            RoleClaimType = ClaimTypes.Role
        };
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("The default database connection is not configured.");

builder.Services.AddInfrastructure(connectionString);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    var tenantName = builder.Configuration["DevelopmentSeed:TenantName"]
        ?? throw new InvalidOperationException("The development tenant name is missing.");
    var adminEmail = builder.Configuration["DevelopmentSeed:AdminEmail"]
        ?? throw new InvalidOperationException("The development admin email is missing.");
    var adminPassword = builder.Configuration["DevelopmentSeed:AdminPassword"]
        ?? throw new InvalidOperationException("The development admin password is missing.");

    await using var scope = app.Services.CreateAsyncScope();
    var initializer = scope.ServiceProvider
        .GetRequiredService<DevelopmentDatabaseInitializer>();
    await initializer.InitializeAsync(tenantName, adminEmail, adminPassword);
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();