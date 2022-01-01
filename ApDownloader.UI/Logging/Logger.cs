using System.IO;
using System.Text;

namespace ApDownloader.UI.Logging;

public static class Logger
{
    public static void LogFatal(string? exceptionString)
    {
        if (exceptionString != null) File.WriteAllBytes(@".\log.txt", Encoding.ASCII.GetBytes(exceptionString));
    }
}