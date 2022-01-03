using System;
using System.IO;
using System.Reflection;
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

        CopyDatabase();
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

    private void CopyDatabase()
    {
        var result = Assembly.GetExecutingAssembly().Location;
        var index = result.LastIndexOf("\\");
        var ProductsDbPath = $"{result.Substring(0, index)}\\ProductsDb.db";
        var SettingsDbPath = $"{result.Substring(0, index)}\\Settings.db";

        var ProductsDestPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\\ApDownloader\\ProductsDb.db";
        var SettingsDestPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\\ApDownloader\\Settings.db";
        var destinationFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\\ApDownloader\\";

        if (File.Exists(ProductsDestPath) && File.Exists(SettingsDestPath)) return;

        Directory.CreateDirectory(destinationFolder);
        File.Copy(ProductsDbPath, ProductsDestPath, true);
        File.Copy(SettingsDbPath, SettingsDestPath, true);
    }
}