using System.Collections.Generic;
using System.Diagnostics;
using ApDownloader.Model;

namespace AddonInstaller;

public class AddonInstaller
{
    public void InstallAddons(DownloadOption downloadOption, IEnumerable<string> filePaths)
    {
        foreach (var filepath in filePaths)
            Process.Start(
                filepath,
                $"/s /v\"/qb INSTALLDIR=\"{downloadOption.InstallFilePath}\"\"");
    }
}