using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace TenantTaskManager.Api.IntegrationTests;

public sealed class AuthenticationTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAccessToken()
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "user@tenanttask.local",
            Password = "User123!"
        });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.False(string.IsNullOrWhiteSpace(
            body.GetProperty("accessToken").GetString()));
    }

    [Fact]
    public async Task GetTasks_WithoutAccessToken_ReturnsUnauthorized()
    {
        var response = await client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
