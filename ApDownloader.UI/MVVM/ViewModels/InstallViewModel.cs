using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ApDownloader.DataAccess;
using ApDownloader.Model;
using ApDownloader.UI.Core;
using static ApDownloader.UI.Core.AsyncRelayCommand;

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

    public InstallViewModel()
    {
        if (!File.Exists(Path.Combine(MainViewModel.DlOption.InstallFilePath, "RailWorks.exe")))
        {
            BusyText = "       Please select a valid\nInstallation folder in Options";
            OverlayVisibility = true;
            return;
        }

        _dataService = new SQLiteDataAccess(MainViewModel.AppFolder);
        InstallCommand = new RelayCommand(list => Install((IList) list), _ => !MainViewModel.IsNotAdmin);
        PopulateAllPreviousDownloadsCommand = new RelayCommand(async _ => await PopulateAllPrevDownloads(), _ => AllDownloadsEnabled);
        Loaded();
    }
    private bool AllDownloadsEnabled { get; set; } = true;

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

    public bool SelectAllButtonEnabled
    {
        get => _selectAllButtonEnabled || ProductCells.Any();
        set
        {
            _selectAllButtonEnabled = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand PopulateAllPreviousDownloadsCommand { get; set; }

    private void Loaded()
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
            new Progress<int>(report => { BusyText = $"Installing file {++completedFileCount} of {totalFileCount.Result}"; });
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

    private async Task PopulateAllPrevDownloads()
    {
        if (!Directory.Exists(Path.Combine(MainViewModel.DlOption.DownloadFilepath, "Products"))) return;
        var allfiles = DiskAccess.GetAllFilesOnDisk(MainViewModel.DlOption.DownloadFilepath);
        var allFilesList = allfiles.Select(kvp => kvp.Key).ToList();
        _downloadedProducts = await _dataService.GetDownloadedProductsByName(allFilesList);

        foreach (var product in _downloadedProducts)
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
}