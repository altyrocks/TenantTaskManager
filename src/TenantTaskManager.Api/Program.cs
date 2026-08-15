using System.Text;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TenantTaskManager.Infrastructure;
using TenantTaskManager.Api.ErrorHandling;
using TenantTaskManager.Api.Authentication;
using TenantTaskManager.Application.Users.GetUsers;
using TenantTaskManager.Application.Tasks.GetTasks;
using TenantTaskManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TenantTaskManager.Application.Tasks.CreateTask;
using TenantTaskManager.Application.Tasks.UpdateTask;
using TenantTaskManager.Infrastructure.Authentication;
using TenantTaskManager.Application.Tasks.CompleteTask;
using TenantTaskManager.Application.Authentication.Login;
using TenantTaskManager.Application.Abstractions.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentTenant, CurrentTenant>();
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<CreateTaskHandler>();
builder.Services.AddScoped<GetTasksHandler>();
builder.Services.AddScoped<CompleteTaskHandler>();
builder.Services.AddScoped<UpdateTaskHandler>();
builder.Services.AddScoped<GetUsersHandler>();

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
builder.Services
    .AddOptions<JwtOptions>()
    .Bind(jwtSection)
    .Validate(options =>
        !string.IsNullOrWhiteSpace(options.Issuer)
        && !string.IsNullOrWhiteSpace(options.Audience)
        && Encoding.UTF8.GetByteCount(options.Secret) >= 32
        && options.ExpirationMinutes > 0,
        "The JWT configuration is invalid.")
    .ValidateOnStart();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IAccessTokenGenerator, JwtAccessTokenGenerator>();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();
builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((options, jwtOptionsAccessor) =>
    {
        var jwtOptions = jwtOptionsAccessor.Value;
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
    var userEmail = builder.Configuration["DevelopmentSeed:UserEmail"]
        ?? throw new InvalidOperationException("The development user email is missing.");
    var userPassword = builder.Configuration["DevelopmentSeed:UserPassword"]
        ?? throw new InvalidOperationException("The development user password is missing.");
    var secondTenantName = builder.Configuration["DevelopmentSeed:SecondTenantName"]
        ?? throw new InvalidOperationException("The second development tenant name is missing.");
    var secondUserEmail = builder.Configuration["DevelopmentSeed:SecondUserEmail"]
        ?? throw new InvalidOperationException("The second development user email is missing.");
    var secondUserPassword = builder.Configuration["DevelopmentSeed:SecondUserPassword"]
        ?? throw new InvalidOperationException("The second development user password is missing.");

    await using var scope = app.Services.CreateAsyncScope();
    var initializer = scope.ServiceProvider
        .GetRequiredService<DevelopmentDatabaseInitializer>();
    await initializer.InitializeAsync(
        tenantName,
        adminEmail,
        adminPassword,
        userEmail,
        userPassword,
        secondTenantName,
        secondUserEmail,
        secondUserPassword);
}

app.UseHttpsRedirection();

app.UseExceptionHandler(_ => { });

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
