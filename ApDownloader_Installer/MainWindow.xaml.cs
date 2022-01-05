using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ApDownloader_Installer;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private static readonly string _tempPath = Path.Combine(Path.GetTempPath(), "ApDownloader");

    public MainWindow()
    {
        InitializeComponent();
        Loaded += Window_Loaded;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Install();
    }

    private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    private void Install()
    {
        var list = File.ReadLines(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ApDownloader") +
                @"\Downloads.txt")
            .ToList();
        var installPath = list[0];
        var files = list.GetRange(1, list.Count() - 1);
        var totalFileCount = files.Count();
        var completedFileCount = 0;
        var progress = new Progress<int>(report => { ProgressText.Text = $"Installing file {++completedFileCount} of {totalFileCount}"; });
        InstallAllAddons(files, installPath, progress);
    }

    private static void InstallAllAddons(List<string> filenames, string installFolder, IProgress<int> progress)
    {
        var dir = new DirectoryInfo(_tempPath);
        if (dir.Exists)
            dir.Delete(true);
        dir.Create();

        UnzipAddons(filenames);
        InstallAddons(installFolder, progress);

        if (dir.Exists)
            dir.Delete(true);
    }

    private static void UnzipAddons(IEnumerable<string> filePaths)
    {
        var fileCount = 1;
        foreach (var filepath in filePaths)
        {
            var filename = Path.GetFileName(filepath);
            ZipFile.ExtractToDirectory(filepath, _tempPath, true);

            using (var archive = ZipFile.OpenRead(filepath))
            {
                foreach (var entry in archive.Entries)
                    if (entry.FullName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
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
            }
    }
}