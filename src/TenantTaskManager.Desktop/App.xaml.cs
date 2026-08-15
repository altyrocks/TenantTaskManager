using System.Windows;
using System.Net.Http;
using TenantTaskManager.Desktop.Services;

namespace TenantTaskManager.Desktop;

public partial class App : Application
{
    private TaskApiClient? apiClient;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        apiClient = new TaskApiClient(new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5010/")
        });

        new MainWindow(apiClient).Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        apiClient?.Dispose();
        base.OnExit(e);
    }
}
