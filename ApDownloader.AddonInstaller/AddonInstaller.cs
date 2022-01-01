using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using ApDownloader.Model;

namespace ApDownloader.AddonInstaller;

public static class AddonInstaller
{
    public static string UnzipAddons(ApDownloaderConfig downloadOption, IEnumerable<string> filePaths, string folder)
    {
        var extractPath = Path.Combine(downloadOption.TempFilePath, "ApDownloads", folder);
        foreach (var filepath in filePaths)
            ZipFile.ExtractToDirectory(Path.Combine(downloadOption.DownloadFilepath, folder, filepath),
                extractPath,
                true);

        return extractPath;
    }

    public static void InstallAddons(ApDownloaderConfig downloadOption, string folder, IProgress<int> progress)
    {
        var files = Directory.GetFiles(folder);
        foreach (var filepath in files)
            if (filepath.Trim('"').EndsWith(".exe"))
            {
                progress.Report(1);
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = filepath.Trim('"'),
                        Arguments = $"/s /v\"/qn INSTALLDIR=\"{downloadOption.InstallFilePath}\"\""
                    }
                };
                process.Start();
                process.WaitForExit();
            }
    }
}