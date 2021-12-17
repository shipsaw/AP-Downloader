using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Media;

namespace ApDownloader;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const string LoginUrl = @"https://www.armstrongpowerhouse.com/index.php?route=account/login";
    private static readonly HttpClientHandler _handler = new() {AllowAutoRedirect = false};
    private readonly HttpClient _client = new(_handler);

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void loginButton_Click(object sender, RoutedEventArgs e)
    {
        var loginSuccessful = false;
        loginStatus.Text = "ATTEMPTING LOGIN...";
        loginStatus.BorderBrush = new SolidColorBrush(Colors.Goldenrod);
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(loginBox.Text), "email");
        content.Add(new StringContent(passwordBox.Password), "password");
        var response = await _client.PostAsync(LoginUrl, content);
        loginSuccessful = response.StatusCode == HttpStatusCode.Redirect;
        loginStatus.Text = loginSuccessful ? "LOGIN SUCCESSFUL" : "LOGIN FAILED";
        logoutButton.IsEnabled = loginSuccessful;
        loginStatus.BorderBrush = new SolidColorBrush(loginSuccessful ? Colors.Green : Colors.Red);
        getDownloadInfo();
    }

    private void logoutButton_Click(object sender, RoutedEventArgs e)
    {
        _client.GetAsync("https://www.armstrongpowerhouse.com/index.php?route=account/logout");
        loginStatus.BorderBrush = new SolidColorBrush(Colors.Red);
        loginStatus.Text = "NOT LOGGED IN";
        logoutButton.IsEnabled = false;
    }

    private void getDownloadInfo()
    {
        var response = _client.SendAsync(new HttpRequestMessage(HttpMethod.Head,
            "https://www.armstrongpowerhouse.com/index.php?route=account/download/download&download_id=107"));
        var result = response.Result.Content.Headers.ContentDisposition;
    }
}

public class LoginInfo
{
    public LoginInfo(string username, string password)
    {
        Username = username;
        Password = password;
    }

    public string Username { get; set; }
    public string Password { get; set; }
}