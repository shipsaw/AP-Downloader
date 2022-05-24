using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ApDownloader.UI.Logging;
using ApDownloader.UI.MVVM.ViewModels;

namespace ApDownloader.UI.MVVM.Views;

public partial class LoginView : UserControl
{
    private const string LoginUrl = @"https://www.armstrongpowerhouse.com/index.php?route=account/login";
    private const string LogoutUrl = @"https://www.armstrongpowerhouse.com/index.php?route=account/logout";
    private static readonly HttpClientHandler Handler = new() {AllowAutoRedirect = false};

    public LoginView()
    {
        InitializeComponent();
        DataContext = new DownloadViewModel();
        Client = new HttpClient(Handler);
        Client.Timeout = TimeSpan.FromMinutes(60);
        Loaded += Login_Loaded;
    }

    public static bool IsLoggedIn { get; private set; }

    public static HttpClient Client { get; private set; } = new(Handler);

    private void Login_Loaded(object sender, RoutedEventArgs e)
    {
        LoginButton.IsEnabled = !IsLoggedIn;
        LogoutButton.IsEnabled = IsLoggedIn;
        CheckForProductDbUpdates();
    }

    private async void Login(object sender, RoutedEventArgs e)
    {
        LoginResult.FontStyle = FontStyles.Normal;
        LoginResult.Foreground = new SolidColorBrush(Colors.White);
        MainViewModel.IsNotBusy = false;
        LoginResult.Text = "Attempting Login";
        var viewModel = (DownloadViewModel) DataContext;
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(EmailBox.Text), "email");
        content.Add(new StringContent(PasswordBoxName.Password), "password");
        try
        {
            var response = await Client.PostAsync(LoginUrl, content);
            if (response.StatusCode == HttpStatusCode.Redirect)
            {
                LoginResult.Text = "Retrieving Purchased Addons";
                await viewModel.LoadUserAddonsCommand.ExecuteAsync();

                LoginResult.Text = "Login Successful";
                LoginButton.IsEnabled = false;
                LogoutButton.IsEnabled = true;
                IsLoggedIn = true;
            }
            else
            {
                LoginResult.Text = "Login Failed";
                LoginButton.IsEnabled = true;
                LogoutButton.IsEnabled = false;
                IsLoggedIn = false;
            }
        }
        catch (HttpRequestException exception)
        {
            Logger.LogFatal(exception.Message);
            LoginResult.Text = "Unable to connect to website";
            LoginButton.IsEnabled = true;
            LogoutButton.IsEnabled = false;
            IsLoggedIn = false;
        }

        MainViewModel.IsNotBusy = true;
    }

    private void Logout(object sender, RoutedEventArgs e)
    {
        Client.GetAsync(LogoutUrl);
        LoginButton.IsEnabled = true;
        LogoutButton.IsEnabled = false;
        LoginResult.Text = "Logged out";
        IsLoggedIn = false;
    }

    private void OnEnterKeyHandler(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Return)
            Login(sender, e);
    }
    private void CheckForProductDbUpdates()
    {
        try
        {
            var tempClient = new HttpClient();
            var response = tempClient.Send(new HttpRequestMessage(HttpMethod.Head,
                "https://github.com/shipsaw/AP-Downloader/releases/download/ProductsDb/ProductsDb.db"));
            byte[] serverMd5 = response?.Content?.Headers?.ContentMD5 ?? new byte[0];
            var serverMd5String = Convert.ToBase64String(serverMd5);
            using var md5 = MD5.Create();
            using var stream =
                File.OpenRead(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "ApDownloader\\ProductsDb.db"));
            var localMd5String = Convert.ToBase64String(md5.ComputeHash(stream));
            if (serverMd5String != localMd5String)
            {
                LoginResult.FontStyle = FontStyles.Italic;
                LoginResult.Foreground = new SolidColorBrush(Colors.Goldenrod);
                LoginResult.Text = "Product Database out-of-date";
            }
        }
        catch
        {
                LoginResult.FontStyle = FontStyles.Italic;
                LoginResult.Foreground = new SolidColorBrush(Colors.Goldenrod);
                LoginResult.Text = "Unable to check for Product\nDatabase updates";
        }
    }
}