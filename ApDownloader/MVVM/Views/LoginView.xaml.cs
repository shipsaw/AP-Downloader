using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace ApDownloader.MVVM.Views;

public partial class LoginView : UserControl
{
    private const string LoginUrl = @"https://www.armstrongpowerhouse.com/index.php?route=account/login";
    private static readonly HttpClientHandler _handler = new() {AllowAutoRedirect = false};
    private readonly HttpClient _client = new(_handler);

    public LoginView()
    {
        InitializeComponent();
    }

    private async void Login(object sender, RoutedEventArgs e)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(EmailBox.Text), "email");
        content.Add(new StringContent(PasswordBoxName.Password), "password");
        var response = await _client.PostAsync(LoginUrl, content);
        LoginResult.Text = response.StatusCode == HttpStatusCode.Redirect ? "Login Successful" : "Login Failed";
    }
}