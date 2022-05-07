using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ApDownloader.UI.Logging;

namespace ApDownloader_Installer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException +=
                OnUnhandledException;
            base.OnStartup(e);
        }

        private static void OnUnhandledException(
            object sender, UnhandledExceptionEventArgs e)
        {
            var exceptionStr = e.ExceptionObject.ToString();
            Logger.LogFatal(exceptionStr);
        }
    }
}