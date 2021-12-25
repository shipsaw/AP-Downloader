using System.Net.Http;
using System.Windows;
using System.Windows.Input;

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


    private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    private void Exit(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
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