using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ApDownloader.UI.Core;

namespace ApDownloader_Installer;

public class MainWindowViewModel : ObservableObject
{
    private static readonly string TempPath = Path.Combine(Path.GetTempPath(), "ApDownloader");
    private string _progressText = "";
    public readonly RelayCommand BeginInstall;
    public readonly RelayCommand ExitIfDoneCommand;
    private bool done;

    public MainWindowViewModel()
    {
        BeginInstall = new RelayCommand(_ => ReadFilesFromList());
        ExitIfDoneCommand = new RelayCommand(_ => ExitIfDone());
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

    private async void ReadFilesFromList()
    {
        await Task.Run(() =>
        {
            var list = File.ReadLines(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ApDownloader") +
                    @"\Downloads.txt")
                .ToList();
            var installPath = list[0];
            var files = list.GetRange(1, list.Count - 1);
            var completedFileCount = 0;
            UnzipAndInstall(files, installPath, files.Count);
            done = true;
        });
    }

    public void ExitIfDone()
    {
        if (done)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "ApDownloader");
            var dir = new DirectoryInfo(tempPath);
            if (dir.Exists)
                dir.Delete(true);
            Application.Current.Shutdown();
        }
    }

    private void UnzipAndInstall(List<string> filenames, string installFolder, int totalFileCount)
    {
        var completedFileCount = 0;
        var progress = new Progress<string>();
        var dir = new DirectoryInfo(TempPath);
        if (dir.Exists)
            dir.Delete(true);
        dir.Create();
        progress = new Progress<string>(filename =>
        {
            ProgressText = $"Extracting file {++completedFileCount} of {totalFileCount}\n{Path.GetFileName(filename)}";
        });
        UnzipAddons(filenames, progress);

        completedFileCount = 0;
        progress = new Progress<string>(filepath =>
        {
            var filename = Path.GetFileName(filepath).TrimStart('1', '2', '3', '4', '5', '6', '7', '9', '0');
            ProgressText = $"Installing file {++completedFileCount} of {totalFileCount}\n{filename}";
        });
        InstallAddons(installFolder, progress);

        ProgressText = "Running scripts; Press any key when finished";
    }

    private static void UnzipAddons(IEnumerable<string> filePaths, IProgress<string> progress)
    {
        var fileCount = 1;
        foreach (var filepath in filePaths)
        {
            using (var archive = ZipFile.OpenRead(filepath))
            {
                foreach (var entry in archive.Entries)
                    if (entry.FullName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                        entry.FullName.EndsWith(".rwp", StringComparison.OrdinalIgnoreCase))
                    {
                        var destinationPath = Path.GetFullPath(Path.Combine(TempPath, fileCount + entry.FullName));
                        progress.Report(entry.FullName);
                        entry.ExtractToFile(destinationPath);
                    }
            }

            fileCount++;
        }
    }

    private static void InstallAddons(string installFolder, IProgress<string> progress)
    {
        var files = Directory.GetFiles(TempPath);
        files = files.OrderBy(o => o).ToArray();
        foreach (var filepath in files)
            if (filepath.Trim('"').EndsWith(".exe"))
            {
                progress.Report(filepath);
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = filepath.Trim('"'),
                        Arguments =
                            $"/b\"{TempPath}\" /s /v\"/qn INSTALLDIR=\"{installFolder}\"\""
                    }
                };
                process.Start();
                process.WaitForExit();
            }
            else if (filepath.Trim('"').EndsWith(".rwp"))
            {
                progress.Report(filepath);
                var zPath = @"7zip\7za.exe"; //add to proj and set CopyToOuputDir
                try
                {
                    var info = new ProcessStartInfo();
                    info.WindowStyle = ProcessWindowStyle.Hidden;
                    info.FileName = zPath;
                    info.Arguments = string.Format("x \"{0}\" -aoa -y -o\"{1}\"", filepath, installFolder);
                    var process = Process.Start(info);
                    process.WaitForExit();
                }
                catch (Exception e)
                {
                    File.WriteAllBytes(
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "ApDownloaderInstallerLog.txt"),
                        Encoding.ASCII.GetBytes(e.Message));
                }
            }
    }
}