using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

    private readonly string _previewImagesPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ApDownloader", "PreviewImages");

    private string _busyText;
    private IEnumerable<Product> _downloadedProducts;
    private bool _overlayVisibility;
    private bool _selectAllButtonEnabled;

    private bool AllDownloadsEnabled { get; set; } = true;
    public RelayCommand InstallCommand { get; set; }
    public ObservableCollection<Cell> ProductCells { get; } = new();
    public RelayCommand RenderAllPreviousDownloadsCommand { get; set; }
    public RelayCommand LoadDownloadsCommand { get; }

    public InstallViewModel()
    {
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

    public async Task Loaded()
    {
        var products = _dataService.GetDownloadedProductsOnly(MainViewModel.DlManifest?.ProductIds).Result;
        var builderlist = new List<Cell>();
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

        await PopulateAllPrevDownloads();
    }

    private async Task Install(IList selectedCells)
    {
        MainViewModel.IsNotBusy = false;
        BusyText = "Installing Addons";
        OverlayVisibility = true;
        var productIds = new List<int>();
        foreach (Cell cell in selectedCells)
            productIds.Add(cell.ProductId);

        MainViewModel.DlManifest = await _dataService.GetDownloadManifest(MainViewModel.DlOption, productIds);
        var downloadList = await GetDownloadList(MainViewModel.DlOption, MainViewModel.DlManifest);
        downloadList = downloadList.Prepend(MainViewModel.DlOption.InstallFilePath).ToList();
        await File.WriteAllLinesAsync(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ApDownloader") + @"\Downloads.txt",
            downloadList);
        var info = new ProcessStartInfo(
            @"..\..\..\..\ApDownloader_Installer\bin\Release\net6.0-windows\ApDownloader_Installer.exe");
        info.UseShellExecute = true;
        info.Verb = "runas";
        Process.Start(info);
        Process.
        BusyText = "Installation Complete";
        MainViewModel.IsNotBusy = true;
    }

    private async Task PopulateAllPrevDownloads()
    {
        if (!Directory.Exists(Path.Combine(MainViewModel.DlOption.DownloadFilepath, "Products"))) return;
        var allfiles = DiskAccess.GetAllFilesOnDisk(MainViewModel.DlOption.DownloadFilepath);
        var allFilesList = allfiles.Select(kvp => kvp.Key).ToList();
        DownloadedProducts = await _dataService.GetDownloadedProductsByName(allFilesList);
    }

    private void RenderAllPrevDownloads()
    {
        AllDownloadsEnabled = false;
        if (DownloadedProducts == null) return;
        foreach (var product in DownloadedProducts)
        {
            var cell = new Cell
            (
                product.ProductID,
                _previewImagesPath + "\\" + product.ImageName,
                product.Name
            );
            if (!ProductCells.Contains(cell))
                ProductCells.Add(cell);
        }

        if (ProductCells.Any())
            SelectAllButtonEnabled = true;
    }

    private async Task<List<string>> GetDownloadList(ApDownloaderConfig config, DownloadManifest manifest)
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