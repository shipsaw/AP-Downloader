using System.Net;
using System.Net.Http;
using System.Windows;

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
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(loginBox.Text), "email");
        content.Add(new StringContent(passwordBox.Password), "password");
        var response = await _client.PostAsync(LoginUrl, content);
        loginStatus.Text = response.StatusCode == HttpStatusCode.Redirect ? "LOGIN SUCCESSFUL" : "LOGIN UNSUCCESSFUL";
        getDownloadInfo();
    }

    private void logoutButton_Click(object sender, RoutedEventArgs e)
    {
        _client.GetAsync("https://www.armstrongpowerhouse.com/index.php?route=account/logout");
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