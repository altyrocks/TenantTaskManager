using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TenantTaskManager.Desktop.Models;

namespace TenantTaskManager.Desktop.Services;

public sealed class TaskApiClient(HttpClient httpClient) : IDisposable
{
    public async Task<bool> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/auth/login",
            new { Email = email, Password = password },
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>(
            cancellationToken);

        if (login is null || string.IsNullOrWhiteSpace(login.AccessToken))
        {
            throw new InvalidOperationException(
                "The API returned an invalid login response.");
        }

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login.AccessToken);

        return true;
    }

    public async Task<IReadOnlyList<TaskItemDto>> GetTasksAsync(
        CancellationToken cancellationToken = default)
    {
        var tasks = await httpClient.GetFromJsonAsync<List<TaskItemDto>>(
            "api/tasks",
            cancellationToken);

        return tasks
            ?? throw new InvalidOperationException(
                "The API returned an invalid task response.");
    }

    public void Dispose()
    {
        httpClient.Dispose();
    }

    private sealed record LoginResponse(
        string AccessToken,
        DateTimeOffset ExpiresAtUtc);
}
