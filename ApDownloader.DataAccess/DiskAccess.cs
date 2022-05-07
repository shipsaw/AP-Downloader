using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ApDownloader.DataAccess;

public static class DiskAccess
{
    /// <summary>
    /// Gets all .zip files that are in the download folder specified.
    /// </summary>
    /// <param name="dlOptionDownloadFilepath"></param>
    /// <returns>Dictionary of filename, file length</returns>
    public static Dictionary<string, long> GetAllFilesOnDisk(string dlOptionDownloadFilepath)
    {
        var addonFolders = new List<string> {"", "Products", "ExtraStock", "BrandingPatches", "LiveryPacks"};
        var files = new Dictionary<string, long>();
        foreach (var addonFile in addonFolders.SelectMany(folder => GetAddonFiles(dlOptionDownloadFilepath, folder)))
        {
            files.TryAdd(addonFile.Name, addonFile.Length);
        }
        return files;
    }

    private static IEnumerable<FileInfo> GetAddonFiles (string downloadFilepath, string folder)
    {
        var fullFolderPath = Path.Combine(downloadFilepath, folder);
        return Directory.Exists(fullFolderPath) ?
            Directory.EnumerateFiles(fullFolderPath, "*.zip").Select(filename => new FileInfo(filename))
            : new List<FileInfo>();
    }
}