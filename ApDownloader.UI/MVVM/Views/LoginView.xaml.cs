using System;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace ApDownloader.UI.MVVM.Views;

public partial class LoginView : UserControl
{
    private const string LoginUrl = @"https://www.armstrongpowerhouse.com/index.php?route=account/login";
    private const string LogoutUrl = @"https://www.armstrongpowerhouse.com/index.php?route=account/logout";
    private static readonly HttpClientHandler _handler = new() {AllowAutoRedirect = false};

    public LoginView()
    {
        InitializeComponent();
        Client = new HttpClient(_handler);
        Client.Timeout = TimeSpan.FromMinutes(5);
        Loaded += Login_Loaded;
    }

    public static bool IsLoggedIn { get; private set; }

    public static HttpClient? Client { get; private set; } = new(_handler);

    private void Login_Loaded(object sender, RoutedEventArgs e)
    {
        LoginButton.IsEnabled = !IsLoggedIn;
        LogoutButton.IsEnabled = IsLoggedIn;
    }

    private async void Login(object sender, RoutedEventArgs e)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(EmailBox.Text), "email");
        content.Add(new StringContent(PasswordBoxName.Password), "password");
        var response = await Client.PostAsync(LoginUrl, content);
        if (response.StatusCode == HttpStatusCode.Redirect)
        {
            //((DownloadView) DataContext).Client = Client;
            LoginResult.Text = "Login Successful";
            LoginButton.IsEnabled = false;
            LogoutButton.IsEnabled = true;
            IsLoggedIn = true;
        }
        else
        {
            //((DownlojjadView) DataContext).Client = null;
            LoginResult.Text = "Login Failed";
            IsLoggedIn = false;
        }
    }

    private void Logout(object sender, RoutedEventArgs e)
    {
        Client.GetAsync(LogoutUrl);
        LoginButton.IsEnabled = true;
        LogoutButton.IsEnabled = false;
        LoginResult.Text = "Logged out";
        IsLoggedIn = false;
    }
}