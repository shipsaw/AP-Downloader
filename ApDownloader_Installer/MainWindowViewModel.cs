using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ApDownloader.UI.Core;

namespace ApDownloader_Installer;

public class MainWindowViewModel : ObservableObject
{
    private static readonly string _tempPath = Path.Combine(Path.GetTempPath(), "ApDownloader");
    private string _progressText;
    public RelayCommand BeginInstall;

    public MainWindowViewModel()
    {
        BeginInstall = new RelayCommand(_ => Install());
    }

    public string ProgressText
    {
        get => _progressText;
        set
        {
            _progressText = value;
            OnPropertyChanged();
        }
    }

    public async void Install()
    {
        await Task.Run(() => {
        var list = File.ReadLines(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ApDownloader") +
                @"\Downloads.txt")
            .ToList();
        var installPath = list[0];
        var files = list.GetRange(1, list.Count() - 1);
        var totalFileCount = files.Count();
        var completedFileCount = 0;
        var progress = new Progress<int>(report => { ProgressText = $"Installing file {++completedFileCount} of {totalFileCount}"; });
        InstallAllAddons(files, installPath, progress);
    });
            Application.Current.Shutdown();
    }

    private void InstallAllAddons(List<string> filenames, string installFolder, IProgress<int> progress)
    {
        var dir = new DirectoryInfo(_tempPath);
        if (dir.Exists)
            dir.Delete(true);
        dir.Create();

        UnzipAddons(filenames);
        InstallAddons(installFolder, progress);

        ProgressText = "Running Install Scripts..";
        Thread.Sleep(2000); // Make sure everything is wrapped up
    }

    private static void UnzipAddons(IEnumerable<string> filePaths)
    {
        var fileCount = 1;
        foreach (var filepath in filePaths)
        {
            var filename = Path.GetFileName(filepath);

            using (var archive = ZipFile.OpenRead(filepath))
            {
                foreach (var entry in archive.Entries)
                    if (entry.FullName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) || entry.FullName.EndsWith(".rwp", StringComparison.OrdinalIgnoreCase))
                    {
                        var destinationPath = Path.GetFullPath(Path.Combine(_tempPath, fileCount + entry.FullName));
                        entry.ExtractToFile(destinationPath);
                    }
            }

            fileCount++;
        }
    }

    private static void InstallAddons(string installFolder, IProgress<int> progress)
    {
        var files = Directory.GetFiles(_tempPath);
        files = files.OrderBy(o => o).ToArray();
        foreach (var filepath in files)
            if (filepath.Trim('"').EndsWith(".exe"))
            {
                progress.Report(1);
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = filepath.Trim('"'),
                        Arguments =
                            $"/b\"{Path.Combine(_tempPath, "ApDownloads")}\" /s /v\"/qn INSTALLDIR=\"{installFolder}\"\""
                    }
                };
                process.Start();
                process.WaitForExit();
            } else if (filepath.Trim('"').EndsWith(".rwp"))
            {
                // How to extract rwp with c#?
                // ZipFile.ExtractToDirectory(filepath, installFolder);
            }
    }
}