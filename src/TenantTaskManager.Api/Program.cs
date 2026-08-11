using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using TenantTaskManager.Infrastructure;
using TenantTaskManager.Api.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TenantTaskManager.Infrastructure.Authentication;
using TenantTaskManager.Application.Abstractions.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentTenant, CurrentTenant>();

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

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();