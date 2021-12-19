using System.Windows;
using ApDownloader.UI.MVVM.Views;
using ApDownloader.UI.Startup;
using Autofac;

namespace ApDownloader.UI;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        var bootstrapper = new Bootstrapper();
        var container = bootstrapper.Bootstrap();
        var mainWindow = container.Resolve<MainWindow>();
        mainWindow.Show();
    }
}