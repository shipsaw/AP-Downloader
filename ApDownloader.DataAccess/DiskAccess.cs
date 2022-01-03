using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApDownloader.Model;

namespace ApDownloader.DataAccess;

public static class DiskAccess
{
    public static Dictionary<string, long> GetAllFilesOnDisk(string dlOptionDownloadFilepath)
    {
        Dictionary<string, long> allFiles = new();
        IEnumerable<FileInfo>? rootFiles = null;
        IEnumerable<FileInfo>? productFiles = null;
        IEnumerable<FileInfo>? extraStockFiles = null;
        IEnumerable<FileInfo>? brandingPatchFiles = null;
        IEnumerable<FileInfo>? liveryPackFiles = null;

        if (Directory.Exists(dlOptionDownloadFilepath))
            rootFiles = Directory.EnumerateFiles(dlOptionDownloadFilepath, "*.zip", SearchOption.TopDirectoryOnly)
                .Select(filename => new FileInfo(filename));
        if (Directory.Exists(Path.Combine(dlOptionDownloadFilepath, "Products")))
            productFiles = Directory.EnumerateFiles(Path.Combine(dlOptionDownloadFilepath, "Products"), "*.zip",
                SearchOption.TopDirectoryOnly).Select(filename => new FileInfo(filename));
        if (Directory.Exists(Path.Combine(dlOptionDownloadFilepath, "ExtraStock")))
            extraStockFiles = Directory.EnumerateFiles(Path.Combine(dlOptionDownloadFilepath, "ExtraStock"), "*.zip",
                SearchOption.TopDirectoryOnly).Select(filename => new FileInfo(filename));
        if (Directory.Exists(Path.Combine(dlOptionDownloadFilepath, "BrandingPatches")))
            brandingPatchFiles = Directory.EnumerateFiles(Path.Combine(dlOptionDownloadFilepath, "BrandingPatches"), "*.zip",
                SearchOption.TopDirectoryOnly).Select(filename => new FileInfo(filename));
        if (Directory.Exists(Path.Combine(dlOptionDownloadFilepath, "LiveryPacks")))
            liveryPackFiles = Directory.EnumerateFiles(Path.Combine(dlOptionDownloadFilepath, "LiveryPacks"), "*.zip",
                SearchOption.TopDirectoryOnly).Select(filename => new FileInfo(filename));

        foreach (var file in rootFiles ?? new List<FileInfo>())
            allFiles.TryAdd(file.Name, file.Length);
        foreach (var file in productFiles ?? new List<FileInfo>())
            allFiles.TryAdd(file.Name, file.Length);
        foreach (var file in extraStockFiles ?? new List<FileInfo>())
            allFiles.TryAdd(file.Name, file.Length);
        foreach (var file in brandingPatchFiles ?? new List<FileInfo>())
            allFiles.TryAdd(file.Name, file.Length);
        foreach (var file in liveryPackFiles ?? new List<FileInfo>())
            allFiles.TryAdd(file.Name, file.Length);

        return allFiles;
    }

    public static void InstallAllAddons(DownloadManifest downloadManifest, ApDownloaderConfig downloaderConfig, IProgress<int> progress)
    {
        var dir = new DirectoryInfo(Path.Combine(downloaderConfig.TempFilePath + "ApDownloads"));
        if (dir.Exists)
            dir.Delete(true);

        if (downloadManifest.ProductIds != null && (downloadManifest.ProductIds?.Any() ?? false))
            InstallAddons(downloaderConfig, downloadManifest.PrFilenames, "Products/", progress);

        if (downloaderConfig.GetExtraStock && downloadManifest.EsFilenames.Any())
            InstallAddons(downloaderConfig, downloadManifest.EsFilenames, "ExtraStock/", progress);
        if (downloaderConfig.GetBrandingPatch && downloadManifest.BpFilenames.Any())
            InstallAddons(downloaderConfig, downloadManifest.BpFilenames, "BrandingPatches/",
                progress);
        if (downloaderConfig.GetLiveryPack && downloadManifest.LpFilenames.Any())
            InstallAddons(downloaderConfig, downloadManifest.LpFilenames, "LiveryPacks/", progress);

        if (dir.Exists)
            dir.Delete(true);
    }

    private static void InstallAddons(ApDownloaderConfig downloadOption, IEnumerable<string> filenames, string folder,
        IProgress<int> progress)
    {
        var extractPath = AddonInstaller.AddonInstaller.UnzipAddons(downloadOption, filenames, folder);
        AddonInstaller.AddonInstaller.InstallAddons(downloadOption, extractPath, progress);
    }
}