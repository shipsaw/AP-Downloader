using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ApDownloader.DataAccess;

public static class DiskAccess
{
    public static HashSet<FileInfo> GetAllFilesOnDisk(string dlOptionDownloadFilepath)
    {
        var addonFolders = new List<string> {"", "Products", "ExtraStock", "BrandingPatches", "LiveryPacks"};
        var files = new HashSet<FileInfo>();
        foreach (var addonFile in addonFolders.SelectMany(folder => GetAddonFiles(dlOptionDownloadFilepath, folder)))
        {
            files.Add(addonFile);
        }
        return files;
    }

    private static IEnumerable<FileInfo> GetAddonFiles (string downloadFilepath, string folder)
    {
        var fullFolderPath = Path.Combine(downloadFilepath + folder);
        return Directory.Exists(fullFolderPath) ?
            Directory.EnumerateFiles(fullFolderPath, "*.zip").Select(filename => new FileInfo(filename))
            : new List<FileInfo>();
    }
}