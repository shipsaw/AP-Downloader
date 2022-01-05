using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using ApDownloader.DataAccess;
using ApDownloader.Model;
using ApDownloader.UI.Core;

namespace ApDownloader.UI.MVVM.ViewModels;

public class MainViewModel : ObservableObject
{
    public static ApDownloaderConfig DlOption = new();
    public static DownloadManifest DlManifest = new();
    private static bool _isNotBusy;
    public RelayCommand ExitCommand;
    private bool IsInstallFolderValid => File.Exists(Path.Combine(DlOption.InstallFilePath, "RailWorks.exe"));
    public static event PropertyChangedEventHandler StaticPropertyChanged;

    public static readonly string AppFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ApDownloader");

    private readonly SQLiteDataAccess _dataAccess;
    private object _currentView;

    public MainViewModel()
    {
        _dataAccess = new SQLiteDataAccess(AppFolder);
        DlOption = _dataAccess.GetUserOptions();

        IsNotBusy = true;
        //LoginVm = new LoginViewModel();
        //DownloadVm = new DownloadViewModel();
        //InstallVm = new InstallViewModel();
        //OptionsVm = new OptionsViewModel(IsInstallFolderValid);

        LoginViewCommand = new RelayCommand(clickEvent => CurrentView = new LoginViewModel());
        DownloadViewCommand = new RelayCommand(clickEvent => CurrentView = new DownloadViewModel());
        InstallViewCommand = new RelayCommand(clickEvent => CurrentView = new InstallViewModel());
        OptionsViewCommand = new RelayCommand(clickEvent => CurrentView = new OptionsViewModel(IsInstallFolderValid));
        ExitCommand = new RelayCommand(_ => Exit());
        CurrentView = new LoginViewModel();
    }

    private void Exit()
    {
        Application.Current.Shutdown();
        var _tempPath = Path.Combine(Path.GetTempPath(), "ApDownloader");
        var dir = new DirectoryInfo(_tempPath);
        if (dir.Exists)
            dir.Delete(true);
    }

    public static bool IsDownloadDataDirty { get; set; } = false;

    public static IEnumerable<Product> Products { get; set; } = new List<Product>();

    public RelayCommand LoginViewCommand { get; set; }
    public RelayCommand DownloadViewCommand { get; set; }
    public RelayCommand InstallViewCommand { get; set; }
    public RelayCommand OptionsViewCommand { get; set; }

    private LoginViewModel LoginVm { get; }
    private DownloadViewModel DownloadVm { get; }
    private InstallViewModel InstallVm { get; }
    private OptionsViewModel OptionsVm { get; }

    public object CurrentView
    {
        get => _currentView;
        set
        {
            _currentView = value;
            OnPropertyChanged();
        }
    }

    public static bool IsNotBusy
    {
        get => _isNotBusy;
        set
        {
            _isNotBusy = value;
            StaticPropertyChanged?.Invoke(null,
                new PropertyChangedEventArgs(nameof(IsNotBusy)));
        }
    }
}