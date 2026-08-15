using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace TenantTaskManager.Api.IntegrationTests;

public sealed class TenantIsolationTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task TaskEndpoints_IsolateDataBetweenTenants()
    {
        using var demoTenantClient = await CreateAuthorizedClientAsync(
            "user@tenanttask.local",
            "User123!");
        using var otherTenantClient = await CreateAuthorizedClientAsync(
            "user@othertenant.local",
            "Other123!");

        var demoTask = await CreateTaskAsync(
            demoTenantClient,
            "Demo tenant integration task");
        var otherTask = await CreateTaskAsync(
            otherTenantClient,
            "Other tenant integration task");

        var demoTasks = await demoTenantClient
            .GetFromJsonAsync<List<TaskResponse>>("/api/tasks");
        var otherTasks = await otherTenantClient
            .GetFromJsonAsync<List<TaskResponse>>("/api/tasks");

        Assert.NotNull(demoTasks);
        Assert.Contains(demoTasks, task => task.Id == demoTask.Id);
        Assert.DoesNotContain(demoTasks, task => task.Id == otherTask.Id);

        Assert.NotNull(otherTasks);
        Assert.Contains(otherTasks, task => task.Id == otherTask.Id);
        Assert.DoesNotContain(otherTasks, task => task.Id == demoTask.Id);

        var crossTenantResponse = await demoTenantClient.PatchAsync(
            $"/api/tasks/{otherTask.Id}/complete",
            null);

        Assert.Equal(HttpStatusCode.NotFound, crossTenantResponse.StatusCode);
    }

    private async Task<HttpClient> CreateAuthorizedClientAsync(
        string email,
        string password)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = email,
            Password = password
        });
        response.EnsureSuccessStatusCode();

        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(login);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login.AccessToken);

        return client;
    }

    private static async Task<CreateTaskResponse> CreateTaskAsync(
        HttpClient client,
        string title)
    {
        var response = await client.PostAsJsonAsync("/api/tasks", new
        {
            Title = title
        });
        response.EnsureSuccessStatusCode();

        var task = await response.Content.ReadFromJsonAsync<CreateTaskResponse>();
        Assert.NotNull(task);

        return task;
    }

    private sealed record LoginResponse(string AccessToken);

    private sealed record CreateTaskResponse(Guid Id);

    private sealed record TaskResponse(Guid Id, string Title, bool IsCompleted);
}
