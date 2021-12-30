using System.Windows;
using ApDownloader.UI.MVVM.ViewModels;
using ApDownloader.UI.MVVM.Views;

namespace ApDownloader.UI;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        MainWindow = new MainWindow
        {
            DataContext = new MainViewModel()
        };
        MainWindow.Show();
        base.OnStartup(e);
    }
}