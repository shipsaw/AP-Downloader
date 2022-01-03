using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ApDownloader.DataAccess;

public static class DiskAccess
{
    public static Dictionary<string, long> GetAllFilesOnDisk(string dlOptionDownloadFilepath)
    {
        Dictionary<string, long> allFiles = new();
        List<FileInfo> rootFiles = new();
        List<FileInfo> productFiles = new();
        List<FileInfo> extraStockFiles = new();
        List<FileInfo> brandingPatchFiles = new();
        List<FileInfo> liveryPackFiles = new();
        if (Directory.Exists(dlOptionDownloadFilepath))
            rootFiles = Directory
                .EnumerateFiles(dlOptionDownloadFilepath, "*.zip", SearchOption.TopDirectoryOnly)
                .Select(file => new FileInfo(file)).ToList();
        if (Directory.Exists(Path.Combine(dlOptionDownloadFilepath, "Products")))
            productFiles = Directory
                .EnumerateFiles(Path.Combine(dlOptionDownloadFilepath, "Products"), "*.zip", SearchOption.TopDirectoryOnly)
                .Select(file => new FileInfo(file)).ToList();
        if (Directory.Exists(Path.Combine(dlOptionDownloadFilepath, "ExtraStock")))
            extraStockFiles = Directory
                .EnumerateFiles(Path.Combine(dlOptionDownloadFilepath, "ExtraStock"), "*.zip", SearchOption.TopDirectoryOnly)
                .Select(file => new FileInfo(file)).ToList();
        if (Directory.Exists(Path.Combine(dlOptionDownloadFilepath, "BrandingPatches")))
            brandingPatchFiles = Directory
                .EnumerateFiles(Path.Combine(dlOptionDownloadFilepath, "BrandingPatches"), "*.zip",
                    SearchOption.TopDirectoryOnly)
                .Select(file => new FileInfo(file)).ToList();
        if (Directory.Exists(Path.Combine(dlOptionDownloadFilepath, "LiveryPacks")))
            liveryPackFiles = Directory
                .EnumerateFiles(Path.Combine(dlOptionDownloadFilepath, "LiveryPacks"), "*.zip",
                    SearchOption.TopDirectoryOnly)
                .Select(file => new FileInfo(file)).ToList();
        foreach (var file in rootFiles) allFiles.TryAdd(file.Name, file.Length);
        foreach (var file in productFiles) allFiles.TryAdd(file.Name, file.Length);
        foreach (var file in extraStockFiles) allFiles.TryAdd(file.Name, file.Length);
        foreach (var file in liveryPackFiles) allFiles.TryAdd(file.Name, file.Length);
        return allFiles;
    }
}