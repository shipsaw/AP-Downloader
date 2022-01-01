using System;
using System.Windows;
using ApDownloader.UI.Logging;
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
        var currentDomain = AppDomain.CurrentDomain;
        currentDomain.UnhandledException +=
            OnUnhandledException;

        MainWindow = new MainWindow
        {
            DataContext = new MainViewModel()
        };
        MainWindow.Show();
        base.OnStartup(e);
    }

    private static void OnUnhandledException(
        object sender, UnhandledExceptionEventArgs e)
    {
        var exceptionStr = e.ExceptionObject.ToString();
        Logger.LogFatal(exceptionStr);
    }
}