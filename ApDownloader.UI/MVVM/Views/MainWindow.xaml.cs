using System.Net.Http;
using System.Security.Principal;
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
        CheckAdmin();
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
        }
        else
        {
            AdminWarning.Visibility = Visibility.Collapsed;
        }
    }
}