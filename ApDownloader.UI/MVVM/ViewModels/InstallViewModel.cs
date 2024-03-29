﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using ApDownloader.DataAccess;
using ApDownloader.Model;
using ApDownloader.UI.Core;
using ApDownloader.UI.MVVM.Views;

namespace ApDownloader.UI.MVVM.ViewModels;

public class InstallViewModel : ObservableObject
{
    private readonly SQLiteDataAccess _dataService;

    private readonly string _previewImagesPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ApDownloader", "PreviewImages");

    private readonly HttpDataAccess _access;
    private string _busyText;
    private bool _overlayVisibility;
    private bool _selectAllButtonEnabled;
    private bool AllDownloadsEnabled { get; set; } = true;
    public RelayCommand InstallCommand { get; set; }
    public ObservableCollection<Cell> ProductCells { get; } = new();
    public RelayCommand RenderAllPreviousDownloadsCommand { get; set; }
    public RelayCommand LoadDownloadsCommand { get; }

    public InstallViewModel()
    {
        _access = new HttpDataAccess(LoginView.Client ?? new HttpClient(), 3);
        _dataService = new SQLiteDataAccess(MainViewModel.AppFolder);
        InstallCommand = new RelayCommand(async list => await Install((IList) list));
        RenderAllPreviousDownloadsCommand = new RelayCommand(_ => RenderAllPrevDownloads(), _ => AllDownloadsEnabled);
        LoadDownloadsCommand = new RelayCommand(async _ => await Loaded());

        if (File.Exists(Path.Combine(MainViewModel.DlOption.InstallFilePath, "RailWorks.exe"))) return;
        BusyText = "       Please select a valid\nInstallation folder in Options";
        OverlayVisibility = true;
    }

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

    public bool SelectAllButtonEnabled
    {
        get => _selectAllButtonEnabled || ProductCells.Any();
        set
        {
            _selectAllButtonEnabled = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<Product> DownloadedProducts { get; private set; }

    private async Task Loaded()
    {
        var products = _dataService.GetDownloadedProductsOnly(MainViewModel.DlManifest.ProductIds).Result;
        foreach (var product in products)
        {
            var cell = new Cell
            (
                product.ProductID,
                Path.Combine(_previewImagesPath, Path.GetFileName(product.ImageName)),
                product.Name
            );
            ProductCells.Add(cell);
        }

        if (ProductCells.Any())
            SelectAllButtonEnabled = true;
        await PopulateAllPrevDownloads();
    }

    private async Task Install(IList selectedCells)
    {
        var logFailurePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "ApDownloader\\logFailure.log");
        var logSuccessPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "ApDownloader\\logSuccess.log");
        var logLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "\nApDownloader\\programLog.log");
        var manifestPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"ApDownloader\Downloads.txt");
        MainViewModel.IsNotBusy = false;
        BusyText = "Installing Addons";
        OverlayVisibility = true;

        var productIds = (from Cell cell in selectedCells select cell.ProductId).ToList();

        var fullName = Directory.GetParent(Assembly.GetExecutingAssembly().Location)?.FullName ?? "";
        MainViewModel.DlManifest = await _dataService.GetDownloadManifest(MainViewModel.DlOption, productIds);
        var downloadList = await GetDownloadList(MainViewModel.DlOption, MainViewModel.DlManifest);
        var downloadListWithData = downloadList.Prepend(MainViewModel.DlOption.DownloadFilepath).ToList();
        downloadListWithData = downloadListWithData.Prepend(MainViewModel.DlOption.InstallFilePath).ToList();
        downloadListWithData = downloadListWithData.Prepend(Path.Combine(fullName, "InstallerExe\\7za.exe" )).ToList();
        await File.WriteAllLinesAsync(manifestPath, downloadListWithData);
        if (fullName != null)
        {
            var path = Path.Combine(fullName, "InstallerExe") +
                       @"\ApInstallManager.exe";
            var info = new ProcessStartInfo(
                path)
            {
                Arguments = manifestPath, 
                UseShellExecute = true,
                Verb = "runas"
            };
            var process = new Process
            {
                StartInfo = info
            };
            var exitedAbnormally = false;
            try
            {
                process.Start();
                await process.WaitForExitAsync();
            }
            catch
            {
                exitedAbnormally = true;
            }

            var someSuccess = File.Exists(logSuccessPath);
            var someFailed = File.Exists(logFailurePath);
            var allSuccess = someSuccess && !someFailed;

            if (exitedAbnormally)
                BusyText = "Installation Failed.\n\nInstaller Exited Abnormally";
            else if (process.ExitCode != 0 && !someSuccess)
                BusyText = $"Installation Failed.\n\nCheck log for details\n{logLogPath}";
            else if (process.ExitCode != 0 && allSuccess)
                BusyText = $"Installation Completed.\n\nAll addons installed but\nsome errors occured.\nCheck log for details\n{logLogPath}";
            else if (allSuccess)
                BusyText = "Installation Completed.\n\nAll addons installed";
            else
                BusyText = $"Installation Completed.\n\nSome addons have not installed\nCheck log for details\n{logLogPath}";
        }

        MainViewModel.IsNotBusy = true;
    }

    private async Task PopulateAllPrevDownloads()
    {
        var allApProducts = (await _dataService.GetProductsOnly().ConfigureAwait(false)).ToList();
        if (!Directory.Exists(_previewImagesPath) || Directory.GetFiles(_previewImagesPath, "*.png").Length != allApProducts.Count)
        {
            MainViewModel.IsNotBusy = false;
            BusyText = "Loading Addons";
            OverlayVisibility = true;
            await _access.DownloadPreviewImages(allApProducts.Select(p => p.ImageName), _previewImagesPath).ConfigureAwait(false);
            MainViewModel.IsNotBusy = true;
            OverlayVisibility = false;
        }

        if (!Directory.Exists(Path.Combine(MainViewModel.DlOption.DownloadFilepath, "Products"))) return;
        var allFiles = DiskAccess.GetAllFilesOnDisk(MainViewModel.DlOption.DownloadFilepath);
        var allFilesList = allFiles.Select(kvp => kvp.Key).ToList();
        DownloadedProducts = await _dataService.GetDownloadedProductsByName(allFilesList);
    }

    private async void RenderAllPrevDownloads()
    {
        if (DownloadedProducts == null)
            return;
        MainViewModel.IsNotBusy = false;
        BusyText = "Getting Previous Downloads";
        OverlayVisibility = true;

        var productList = ProductCells.ToList();
        var builderList = new List<Cell>();
        await Task.Run(() =>
        {
            AllDownloadsEnabled = false;
            foreach (var product in DownloadedProducts)
            {
                var cell = new Cell
                (
                    product.ProductID,
                    _previewImagesPath + "\\" + product.ImageName,
                    product.Name
                );
                if (!productList.Exists(listCell => listCell.ProductId == cell.ProductId))
                    builderList.Add(cell);
            }

            builderList = builderList.OrderByDescending(cell => cell.ProductId).ToList();
        });
        foreach (var cell in builderList) ProductCells.Add(cell);

        if (ProductCells.Any()) SelectAllButtonEnabled = true;

        MainViewModel.IsNotBusy = true;
        OverlayVisibility = false;
    }

    private static async Task<List<string>> GetDownloadList(ApDownloaderConfig config, DownloadManifest manifest)
    {
        var retList = new List<string>();
        await Task.Run(() =>
        {
            // Products
            foreach (var filename in manifest.PrFilenames)
                retList.Add(Path.Combine(config.DownloadFilepath, "Products") + @"\" + filename);
            // ExtraStock
            foreach (var filename in manifest.EsFilenames)
                retList.Add(Path.Combine(config.DownloadFilepath, "ExtraStock") + @"\" + filename);
            // BrandingPatches
            foreach (var filename in manifest.BpFilenames)
                retList.Add(Path.Combine(config.DownloadFilepath, "BrandingPatches") + @"\" + filename);
            // LiveryPacks
            foreach (var filename in manifest.LpFilenames)
                retList.Add(Path.Combine(config.DownloadFilepath, "LiveryPacks") + @"\" + filename);
        });
        return retList;
    }
}