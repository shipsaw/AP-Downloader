using System.Net.Http;
using System.Windows;

namespace ApDownloader.UI.MVVM.Views;

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
    }

    private void logoutButton_Click(object sender, RoutedEventArgs e)
    {
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