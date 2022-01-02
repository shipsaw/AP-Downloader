using System;
using System.IO;
using System.Text;

namespace ApDownloader.UI.Logging;

public static class Logger
{
    public static void LogFatal(string? exceptionString)
    {
        if (exceptionString != null)
            File.WriteAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "ApDownloaderlog.txt"),
                Encoding.ASCII.GetBytes(exceptionString));
    }
}