using System.Windows;
using System.Net.Http;
using System.Windows.Controls;
using TenantTaskManager.Desktop.Models;
using TenantTaskManager.Desktop.Services;

namespace TenantTaskManager.Desktop;

public partial class MainWindow : Window
{
    private readonly TaskApiClient apiClient;

    public MainWindow(TaskApiClient apiClient)
    {
        InitializeComponent();
        this.apiClient = apiClient;
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        var email = EmailTextBox.Text.Trim();
        var password = PasswordInput.Password;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ShowMessage("Email and password are required.", isError: true);
            return;
        }

        SetLoginInProgress(true);
        var signedIn = false;

        try
        {
            var succeeded = await apiClient.LoginAsync(email, password);

            if (!succeeded)
            {
                ShowMessage("The email or password is incorrect.", isError: true);
                return;
            }

            await LoadTasksAsync();

            signedIn = true;
            LoginPanel.Visibility = Visibility.Collapsed;
            TaskPanel.Visibility = Visibility.Visible;
        }
        catch (HttpRequestException)
        {
            ShowMessage(
                "Unable to reach the API. Make sure it is running on port 5010.",
                isError: true);
        }
        finally
        {
            if (!signedIn)
            {
                SetLoginInProgress(false);
            }
        }
    }

    private async void CompleteTaskButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button
            || button.DataContext is not TaskItemDto task
            || !task.CanComplete)
        {
            return;
        }

        button.IsEnabled = false;
        button.Content = "Completing...";
        TaskMessageTextBlock.Visibility = Visibility.Collapsed;

        try
        {
            await apiClient.CompleteTaskAsync(task.Id);
            await LoadTasksAsync();
            ShowTaskMessage($"Completed \"{task.Title}\".", isError: false);
        }
        catch (HttpRequestException)
        {
            button.IsEnabled = true;
            button.Content = task.CompletionAction;
            ShowTaskMessage("Unable to complete the task.", isError: true);
        }
    }

    private async Task LoadTasksAsync()
    {
        var tasks = await apiClient.GetTasksAsync();
        TaskList.ItemsSource = tasks;
        EmptyTasksMessage.Visibility = tasks.Count == 0
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void SetLoginInProgress(bool isInProgress)
    {
        LoginButton.IsEnabled = !isInProgress;
        LoginButton.Content = isInProgress ? "Signing in..." : "Sign in";
    }

    private void ShowMessage(string message, bool isError)
    {
        MessageTextBlock.Text = message;
        MessageTextBlock.Foreground = isError
            ? System.Windows.Media.Brushes.Firebrick
            : System.Windows.Media.Brushes.ForestGreen;
        MessageTextBlock.Visibility = Visibility.Visible;
    }

    private void ShowTaskMessage(string message, bool isError)
    {
        TaskMessageTextBlock.Text = message;
        TaskMessageTextBlock.Foreground = isError
            ? System.Windows.Media.Brushes.Firebrick
            : System.Windows.Media.Brushes.ForestGreen;
        TaskMessageTextBlock.Visibility = Visibility.Visible;
    }
}
