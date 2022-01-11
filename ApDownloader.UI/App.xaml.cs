using System;
using System.IO;
using System.Reflection;
using System.Windows;
using ApDownloader.Core;
using ApDownloader.UI.Logging;

namespace ApDownloader.UI;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        Activated += StartElmish;
    }

    private void StartElmish(object sender, EventArgs e)
    {
        Activated -= StartElmish;
        var currentDomain = AppDomain.CurrentDomain;
        currentDomain.UnhandledException +=
            OnUnhandledException;

        CopyDatabase();
        Project.main(MainWindow);
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