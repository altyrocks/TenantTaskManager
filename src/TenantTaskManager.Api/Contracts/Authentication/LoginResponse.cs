namespace TenantTaskManager.Api.Contracts.Authentication;

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc);