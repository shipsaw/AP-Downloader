using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace ApDownloader.MVVM.Views;

public partial class LoginView : UserControl
{
    private const string LoginUrl = @"https://www.armstrongpowerhouse.com/index.php?route=account/login";
    private const string LogoutUrl = @"https://www.armstrongpowerhouse.com/index.php?route=account/logout";
    private static readonly HttpClientHandler _handler = new() {AllowAutoRedirect = false};

    public LoginView()
    {
        InitializeComponent();
    }

    public HttpClient Client { get; } = new(_handler);

    private async void Login(object sender, RoutedEventArgs e)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(EmailBox.Text), "email");
        content.Add(new StringContent(PasswordBoxName.Password), "password");
        var response = await Client.PostAsync(LoginUrl, content);
        if (response.StatusCode == HttpStatusCode.Redirect)
        {
            LoginResult.Text = "Login Successful";
            LoginButton.IsEnabled = false;
            LogoutButton.IsEnabled = true;
        }
        else
        {
            LoginResult.Text = "Login Failed";
        }
    }

    private void Logout(object sender, RoutedEventArgs e)
    {
        Client.GetAsync(LogoutUrl);
        LoginButton.IsEnabled = true;
        LogoutButton.IsEnabled = false;
        LoginResult.Text = "Logged out";
    }
}