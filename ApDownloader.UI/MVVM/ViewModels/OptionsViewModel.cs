﻿using System;
using System.IO;
using System.Linq;
using ApDownloader.DataAccess;
using ApDownloader.UI.Core;
using ApDownloader.UI.Logging;
using Logger = ApDownloader.UI.Logging.Logger;

namespace ApDownloader.UI.MVVM.ViewModels;

public class OptionsViewModel : ObservableObject
{
    private readonly SQLiteDataAccess _dataAccess;
    private object _applyResponseVisibility = false;
    private bool _canApply;
    private bool _canOrganize;
    private string _downloadFilepath;
    private bool _getBrandingPatch;
    private bool _getExtraStock;
    private bool _getLiveryPack;
    private string _installFilepath;
    private bool _isInstallFolderInvalid;
    private string _updateDbNotificationText;
    private bool _updateDbNotificationVisibility;

    public OptionsViewModel(bool isInstallFolderValid)
    {
        IsInstallFolderInValid = !isInstallFolderValid;
        _dataAccess = new SQLiteDataAccess(MainViewModel.AppFolder);
        _getExtraStock = MainViewModel.DlOption.GetExtraStock;
        _getBrandingPatch = MainViewModel.DlOption.GetBrandingPatch;
        _getLiveryPack = MainViewModel.DlOption.GetLiveryPack;
        _downloadFilepath = MainViewModel.DlOption.DownloadFilepath;
        _installFilepath = MainViewModel.DlOption.InstallFilePath;
        CanOrganize = true;

        OrganizeDownloadFolderCommand = new RelayCommand(clickEvent => OrganizeDownloadFolder(), _ => CanOrganize);
        ImportProductDbCommand =
            new RelayCommand(filename => UpdateProductDb((string) filename));
        SetDownloadFilepathCommand = new RelayCommand(path =>
        {
            DownloadFilepath = (string) path;
            CanApply = true;
        });
        SetInstallFilepathCommand = new RelayCommand(path =>
        {
            InstallFilepath = (string) path;
            CanApply = true;
        });
        ApplySettingsCommand = new RelayCommand(clickEvent => ApplySettings(), clickEvent => CanApply);
    }

    public RelayCommand ApplySettingsCommand { get; set; }
    public RelayCommand SetDownloadFilepathCommand { get; set; }
    public RelayCommand SetInstallFilepathCommand { get; set; }
    public RelayCommand OrganizeDownloadFolderCommand { get; set; }
    public RelayCommand ImportProductDbCommand { get; set; }

    public bool CanApply
    {
        get => _canApply && !IsInstallFolderInValid;
        set
        {
            _canApply = value;
            if (value)
                ApplyResponseVisibility = false;
            OnPropertyChanged();
        }
    }

    public bool IsInstallFolderInValid
    {
        get => _isInstallFolderInvalid;
        set
        {
            _isInstallFolderInvalid = value;
            OnPropertyChanged();
        }
    }

    public bool CanOrganize
    {
        get => _canOrganize;
        set
        {
            _canOrganize = value;
            OnPropertyChanged();
        }
    }

    public bool GetExtraStock
    {
        get => _getExtraStock;
        set
        {
            _getExtraStock = value;
            OnPropertyChanged();
            CanApply = true;
        }
    }

    public bool GetBrandingPatch
    {
        get => _getBrandingPatch;
        set
        {
            _getBrandingPatch = value;
            OnPropertyChanged();
            CanApply = true;
        }
    }

    public bool GetLiveryPack
    {
        get => _getLiveryPack;
        set
        {
            _getLiveryPack = value;
            OnPropertyChanged();
            CanApply = true;
        }
    }

    public string DownloadFilepath
    {
        get => _downloadFilepath;
        set
        {
            // If selecting existing Downloads folder, use that, else create one
            _downloadFilepath = value;
            OnPropertyChanged();
            CanApply = true;
            MainViewModel.IsDownloadDataDirty = true;
            CanOrganize = false;
        }
    }

    public string InstallFilepath
    {
        get => _installFilepath;
        set
        {
            _installFilepath = value;
            OnPropertyChanged();
            CanApply = true;
        }
    }

    public object ApplyResponseVisibility
    {
        get => _applyResponseVisibility;
        set
        {
            _applyResponseVisibility = value;
            OnPropertyChanged();
        }
    }

    public string UpdateDbNotificationText
    {
        get => _updateDbNotificationText;
        set
        {
            _updateDbNotificationText = value;
            OnPropertyChanged();
        }
    }

    public bool UpdatedDbNotificationVisibility
    {
        get => _updateDbNotificationVisibility;
        set
        {
            _updateDbNotificationVisibility = value;
            OnPropertyChanged();
        }
    }

    private async void OrganizeDownloadFolder()
    {
        if (!Directory.Exists(Path.Combine(MainViewModel.DlOption.DownloadFilepath))) return;
        var productsSet = await _dataAccess.GetFilesFolders();

        Directory.CreateDirectory(MainViewModel.DlOption.DownloadFilepath + @"\Products");
        Directory.CreateDirectory(MainViewModel.DlOption.DownloadFilepath + @"\ExtraStock");
        Directory.CreateDirectory(MainViewModel.DlOption.DownloadFilepath + @"\BrandingPatches");
        Directory.CreateDirectory(MainViewModel.DlOption.DownloadFilepath + @"\LiveryPacks");

        var allFiles = Directory
            .EnumerateFiles(Path.Combine(MainViewModel.DlOption.DownloadFilepath), "*.zip",
                SearchOption.TopDirectoryOnly)
            .Select(file => new FileInfo(file).Name);
        foreach (var filename in allFiles)
            File.Move(Path.Combine(MainViewModel.DlOption.DownloadFilepath, filename),
                Path.Combine(MainViewModel.DlOption.DownloadFilepath, productsSet[filename], filename));
        CanOrganize = true;
    }

    private async void ApplySettings()
    {
        MainViewModel.DlOption.GetExtraStock = GetExtraStock;
        MainViewModel.DlOption.GetBrandingPatch = GetBrandingPatch;
        MainViewModel.DlOption.GetLiveryPack = GetLiveryPack;
        MainViewModel.DlOption.DownloadFilepath = DownloadFilepath;
        MainViewModel.DlOption.InstallFilePath = InstallFilepath;
        Directory.CreateDirectory(MainViewModel.DlOption.DownloadFilepath + @"\Products");
        Directory.CreateDirectory(MainViewModel.DlOption.DownloadFilepath + @"\ExtraStock");
        Directory.CreateDirectory(MainViewModel.DlOption.DownloadFilepath + @"\BrandingPatches");
        Directory.CreateDirectory(MainViewModel.DlOption.DownloadFilepath + @"\LiveryPacks");
        await _dataAccess.SetUserOptions(MainViewModel.DlOption);
        ApplyResponseVisibility = true;
        CanApply = false;
        CanOrganize = true;
    }

    private void UpdateProductDb(string filename)
    {
        UpdateDbNotificationText = "Database update failed, run as administrator";
        try
        {
            _dataAccess.ImportProductDb(filename);
            UpdateDbNotificationText = "Database updated successfully";
        }
        catch (Exception e)
        {
            Logger.LogFatal(e.Message);
        }

        UpdatedDbNotificationVisibility = true;
    }
}