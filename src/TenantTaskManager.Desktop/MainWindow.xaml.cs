using System.Windows;
using System.Net.Http;
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

            ShowMessage("You are signed in.", isError: false);
            signedIn = true;
            EmailTextBox.IsEnabled = false;
            PasswordInput.IsEnabled = false;
            LoginButton.IsEnabled = false;
            LoginButton.Content = "Signed in";
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
}
