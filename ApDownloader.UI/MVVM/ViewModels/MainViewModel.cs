﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Principal;
using ApDownloader.DataAccess;
using ApDownloader.Model;
using ApDownloader.UI.Core;
using Microsoft.Extensions.Configuration;

namespace ApDownloader.UI.MVVM.ViewModels;

public class MainViewModel : ObservableObject
{
    public static ApDownloaderConfig DlOption = new();
    public static DownloadManifest DlManifest = new();
    private static bool _isNotBusy;

    public static readonly string AppFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ApDownloader");

    private readonly SQLiteDataAccess _dataAccess;
    private object _currentView;

    public MainViewModel()
    {
        CheckAdmin();
        _dataAccess = new SQLiteDataAccess(AppFolder);
        DlOption = _dataAccess.GetUserOptions();

        IsNotBusy = true;
        LoginVm = new LoginViewModel();
        DownloadVm = new DownloadViewModel();
        InstallVm = new InstallViewModel();
        OptionsVm = new OptionsViewModel(IsInstallFolderValid);

        LoginViewCommand = new RelayCommand(clickEvent => CurrentView = LoginVm);
        DownloadViewCommand = new RelayCommand(clickEvent => CurrentView = DownloadVm);
        InstallViewCommand = new RelayCommand(clickEvent => CurrentView = InstallVm);
        OptionsViewCommand = new RelayCommand(clickEvent => CurrentView = OptionsVm);
        CurrentView = LoginVm;
    }

    public static IConfigurationRoot? Config { get; set; }
    public static bool IsDownloadDataDirty { get; set; } = true;

    public object GotoOptionPage { get; set; }

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

    public static bool IsNotAdmin { get; set; }

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

    private bool IsInstallFolderValid => File.Exists(Path.Combine(DlOption.InstallFilePath, "RailWorks.exe"));

    public static event PropertyChangedEventHandler StaticPropertyChanged;

    private void CheckAdmin()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        IsNotAdmin = !principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}