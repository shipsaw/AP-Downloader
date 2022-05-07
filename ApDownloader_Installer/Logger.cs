using System;
using System.IO;
using System.Text;

namespace ApDownloader.UI.Logging;

public static class Logger
{
    public static void LogFatal(string exceptionString)
    {
        File.WriteAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "ApDownloaderInstalllog.txt"),
            Encoding.ASCII.GetBytes(exceptionString));
    }
    public static void Log(string exceptionString)
    {
        File.WriteAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "ApDownloaderInstalllog.txt"),
            Encoding.ASCII.GetBytes(exceptionString));
    }
}