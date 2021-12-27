using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using ApDownloader.Model;

namespace ApDownloader.AddonInstaller;

public static class AddonInstaller
{
    public static string UnzipAddons(DownloadOption downloadOption, IEnumerable<string> filePaths, string folder)
    {
        var extractPath = Path.Combine(downloadOption.TempFilePath, folder);
        foreach (var filepath in filePaths)
            ZipFile.ExtractToDirectory(Path.Combine("Apdownloads/Products", filepath), extractPath, true);
        return extractPath;
    }

    public static void InstallAddons(DownloadOption downloadOption, string folder)
    {
        var files = Directory.GetFiles(folder);
        foreach (var filepath in files)
            if (filepath.Trim('"').EndsWith(".exe"))
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = filepath.Trim('"'),
                        Arguments = $"/s /v\"/qb INSTALLDIR=\"{downloadOption.InstallFilePath}\"\""
                    }
                };
                process.Start();
                process.WaitForExit();
            }
    }
}