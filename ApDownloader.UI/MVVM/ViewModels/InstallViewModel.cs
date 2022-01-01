﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ApDownloader.DataAccess;
using ApDownloader.Model;
using ApDownloader.UI.Core;

namespace ApDownloader.UI.MVVM.ViewModels;

public class InstallViewModel : ObservableObject
{
    private readonly SQLiteDataAccess _dataService;
    private HttpDataAccess _access;

    private string _busyText;

    private bool _overlayVisibility;

    private bool _selectedToggle = true;
    //private DownloadManifest DownloadManifest;

    public InstallViewModel()
    {
        if (!File.Exists(Path.Combine(MainViewModel.DlOption.InstallFilePath, "RailWorks.exe")))
        {
            BusyText = "       Please select a valid\nInstallation folder in Options";
            OverlayVisibility = true;
            return;
        }

        _dataService = new SQLiteDataAccess();
        InstallCommand = new RelayCommand(list => Install((IList) list));
        GetAllPrevDownloadsCommand = new RelayCommand(clickEvent => GetAllPrevDownloads());
        Loaded();
    }

    public RelayCommand GetAllPrevDownloadsCommand { get; set; }

    public RelayCommand InstallCommand { get; set; }
    public ObservableCollection<Cell> ProductCells { get; } = new();

    public string BusyText
    {
        get => _busyText;
        set
        {
            _busyText = value;
            OnPropertyChanged();
        }
    }

    public bool OverlayVisibility
    {
        get => _overlayVisibility;
        set
        {
            _overlayVisibility = value;
            OnPropertyChanged();
        }
    }

    public bool SelectAllButtonEnabled => MainViewModel.Products.Any();

    private async void Loaded()
    {
        var products = await _dataService.GetDownloadedProductsOnly(MainViewModel.DlManifest?.ProductIds);
        foreach (var product in products)
        {
            var cell = new Cell
            (
                product.ProductID,
                "../../Images/" + product.ImageName,
                product.Name
            );
            ProductCells.Add(cell);
        }
    }

    private async Task Install(IList selectedCells)
    {
        MainViewModel.IsNotBusy = false;
        BusyText = "Installing Addons";
        OverlayVisibility = true;
        var productIds = new List<int>();
        foreach (Cell cell in selectedCells)
            productIds.Add(cell.ProductId);


        var completedFileCount = 0;
        var totalFileCount =
            _dataService.GetTotalFileCount(MainViewModel.DlOption, productIds);
        var progress =
            new Progress<int>(report =>
            {
                BusyText = $"Installing file {++completedFileCount} of {totalFileCount.Result}";
            });
        OverlayVisibility = true;
        MainViewModel.DlManifest = await _dataService.GetDownloadManifest(MainViewModel.DlOption, productIds);
        await Task.Run(() =>
        {
            var dir = new DirectoryInfo(Path.Combine(MainViewModel.DlOption.TempFilePath + "ApDownloads"));
            if (dir.Exists)
                dir.Delete(true);

            if (MainViewModel.DlManifest.ProductIds != null && MainViewModel.DlManifest.ProductIds.Any())
                InstallAddons(MainViewModel.DlOption, MainViewModel.DlManifest.PrFilenames, "Products/", progress);

            if (MainViewModel.DlOption.GetExtraStock && MainViewModel.DlManifest.EsFilenames.Any())
                InstallAddons(MainViewModel.DlOption, MainViewModel.DlManifest.EsFilenames, "ExtraStock/", progress);
            if (MainViewModel.DlOption.GetBrandingPatch && MainViewModel.DlManifest.BpFilenames.Any())
                InstallAddons(MainViewModel.DlOption, MainViewModel.DlManifest.BpFilenames, "BrandingPatches/",
                    progress);

            if (MainViewModel.DlOption.GetLiveryPack && MainViewModel.DlManifest.LpFilenames.Any())
                InstallAddons(MainViewModel.DlOption, MainViewModel.DlManifest.LpFilenames, "LiveryPacks/", progress);

            if (dir.Exists)
                dir.Delete(true);
        });
        BusyText = "Installation Complete";
        MainViewModel.IsNotBusy = true;
    }

    private static void InstallAddons(ApDownloaderConfig downloadOption, IEnumerable<string> filenames, string folder,
        IProgress<int> progress)
    {
        var extractPath = AddonInstaller.AddonInstaller.UnzipAddons(downloadOption, filenames, folder);
        AddonInstaller.AddonInstaller.InstallAddons(downloadOption, extractPath, progress);
    }

    private async void GetAllPrevDownloads()
    {
        if (!Directory.Exists(Path.Combine(MainViewModel.DlOption.DownloadFilepath, "Products"))) return;
        var allFiles = Directory
            .EnumerateFiles(Path.Combine(MainViewModel.DlOption.DownloadFilepath, "Products"), "*.zip",
                SearchOption.AllDirectories)
            .Select(file => new FileInfo(file).Name);
        var products = await _dataService.GetDownloadedProductsByName(allFiles);

        foreach (var product in products)
        {
            var cell = new Cell
            (
                product.ProductID,
                "../../Images/" + product.ImageName,
                product.Name
            );
            if (!ProductCells.Contains(cell))
                ProductCells.Add(cell);
        }
    }
}