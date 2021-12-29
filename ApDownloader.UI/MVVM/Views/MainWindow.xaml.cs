using System.Net.Http;
using System.Security.Principal;
using System.Windows;
using System.Windows.Input;
using ApDownloader.DataAccess;
using ApDownloader.Model;

namespace ApDownloader.UI.MVVM.Views;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const string LoginUrl = @"https://www.armstrongpowerhouse.com/index.php?route=account/login";
    private static readonly HttpClientHandler _handler = new() {AllowAutoRedirect = false};
    public static DownloadOption DlOption = new();
    public static bool IsAdmin;
    private readonly HttpClient _client = new(_handler);
    private readonly SQLiteDataAccess _dataAccess;

    public MainWindow()
    {
        InitializeComponent();
        CheckAdmin();
        _dataAccess = new SQLiteDataAccess();
        Loaded += InitializeDownloadOption;
    }

    private async void InitializeDownloadOption(object sender, RoutedEventArgs e)
    {
        DlOption = await _dataAccess.GetUserOptions();
    }


    private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    private void Exit(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Sure you want to exit?",
                "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            Application.Current.Shutdown();
        else
            return;
    }

    private void CheckAdmin()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
        {
            AdminWarning.Visibility = Visibility.Visible;
            Application.Current.MainWindow.Height += 30;
            var margin = Control.Margin;
            margin.Bottom = 30;
            Control.Margin = margin;
            IsAdmin = false;
        }
        else
        {
            AdminWarning.Visibility = Visibility.Collapsed;
            IsAdmin = true;
        }
    }
}