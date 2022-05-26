using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApDownloader.DataAccess;
using ApDownloader.Model;
using ApDownloader.Model.Exceptions;
using ApDownloader.UI.Core;
using ApDownloader.UI.Logging;
using ApDownloader.UI.MVVM.Views;
using Logger = ApDownloader.UI.Logging.Logger;

namespace ApDownloader.UI.MVVM.ViewModels;

public class DownloadViewModel : ObservableObject
{
    private readonly SQLiteDataAccess _dataService;

    private readonly string _previewImagesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "ApDownloader", "PreviewImages");

    private HttpDataAccess _access;
    private string _busyText = "LOADING ADDONS";
    private int _canUpdateCount;
    private int _isNotOnDiskCount;
    private string _missingPackText = "";
    private string _outOfDateText = "";
    private bool _overlayVisibility;

    public DownloadViewModel()
    {
        DownloadCommand = new RelayCommand(list => DownloadAddons((IList) list));
        _dataService = new SQLiteDataAccess(MainViewModel.AppFolder);
        _access = new HttpDataAccess(LoginView.Client, 3);
        LoadUserAddonsCommand = new AsyncRelayCommand.AsyncCommand(LoadUserAddons);
        RenderUserAddonsCommand = new AsyncRelayCommand.AsyncCommand(RenderUserAddons);
    }

    public AsyncRelayCommand.AsyncCommand LoadUserAddonsCommand { get; set; }
    public AsyncRelayCommand.AsyncCommand RenderUserAddonsCommand { get; set; }
    public RelayCommand DownloadCommand { get; set; }
    public ObservableCollection<Cell> ProductCells { get; set; } = new();
    public bool SelectAllButtonEnabled => MainViewModel.Products.Any();
    private IEnumerable<Product> AllApProducts { get; set; }

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

    public string OutOfDateText
    {
        get => _outOfDateText;
        set
        {
            _outOfDateText = value;
            OnPropertyChanged();
        }
    }

    public string MissingPackText
    {
        get => _missingPackText;
        set
        {
            _missingPackText = value;
            OnPropertyChanged();
        }
    }

    private async void DownloadAddons(IList selectedCells)
    {
        MainViewModel.IsNotBusy = false;
        OverlayVisibility = true;
        Directory.CreateDirectory(MainViewModel.DlOption.DownloadFilepath + @"\Products");
        Directory.CreateDirectory(MainViewModel.DlOption.DownloadFilepath + @"\ExtraStock");
        Directory.CreateDirectory(MainViewModel.DlOption.DownloadFilepath + @"\BrandingPatches");
        Directory.CreateDirectory(MainViewModel.DlOption.DownloadFilepath + @"\LiveryPacks");
        var productIds = (from Cell cell in selectedCells select cell.ProductId).ToList();
        var completedFileCount = 0;
        var totalFileCount =
            await _dataService.GetTotalFileCount(MainViewModel.DlOption, productIds);
        var progress =
            new Progress<int>(_ => { BusyText = $"Downloading file {++completedFileCount} of {totalFileCount}"; });
        MainViewModel.DlManifest = await _dataService.GetDownloadManifest(MainViewModel.DlOption, productIds);
        try
        {
            await _access.Download(MainViewModel.DlManifest, MainViewModel.DlOption, progress);
            MainViewModel.IsDownloadDataDirty = true;
            await LoadUserAddons();
            BusyText = "Download Complete";
        }
        catch (ErrorCheckingPurchasesException)
        {
            BusyText = "Error checking purchased products";
        }
        catch (Exception exception)
        {
            Logger.LogFatal(exception.Message);
            MainViewModel.IsDownloadDataDirty = true;
            await LoadUserAddons();
            BusyText = "Download Failed";
        }

        MainViewModel.IsNotBusy = true;
    }

    private async Task<bool> LoadUserAddons()
    {
        var success = false;
        AllApProducts = (await _dataService.GetProductsOnly()).ToList();
        if (!Directory.Exists(_previewImagesPath) || Directory.GetFiles(_previewImagesPath, "*.png").Length != AllApProducts.Count())
            await _access.DownloadPreviewImages(AllApProducts.Select(p => p.ImageName), _previewImagesPath);
        if (!MainViewModel.Products.Any() || MainViewModel.IsDownloadDataDirty)
            try
            {
                _access = new HttpDataAccess(LoginView.Client);
                MainViewModel.Products = await _access.GetPurchasedProducts(AllApProducts);
                var allFiles = DiskAccess.GetAllFilesOnDisk(MainViewModel.DlOption.DownloadFilepath);
                foreach (var product in MainViewModel.Products)
                {
                    product.UserContentLength = allFiles.TryGetValue(product.FileName, out var value) ? value : 0;
                    product.CanUpdate = product.UserContentLength != product.CurrentContentLength &&
                                        product.UserContentLength != 0;
                    product.IsMissing = product.UserContentLength == 0;
                }

                MainViewModel.IsDownloadDataDirty = false;
                success = true;
            }
            catch (Exception)
            {
                BusyText = "Unable to connect to server";
            }

        return success;
    }

    private async Task RenderUserAddons()
    {
        OverlayVisibility = true;
        if (MainViewModel.IsDownloadDataDirty)
        {
            if (await LoadUserAddons() == false)
            {
                MainViewModel.IsNotBusy = true;
                return;
            }

            MainViewModel.IsDownloadDataDirty = false;
        }

        var builderList = new List<Cell>();
        await Task.Run(() =>
        {
            foreach (var product in MainViewModel.Products)
            {
                var cell = new Cell(
                    product.ProductID,
                    Path.Combine(_previewImagesPath, Path.GetFileName(product.ImageName)),
                    product.Name,
                    product.CanUpdate,
                    product.IsMissing
                );
                if (cell.IsNotOnDisk) _isNotOnDiskCount++;
                if (cell.CanUpdate) _canUpdateCount++;
                builderList.Add(cell);
            }
        });
        builderList = builderList.OrderByDescending(cell => cell.ProductId).ToList();
        foreach (var cell in builderList) ProductCells.Add(cell);

        OutOfDateText = $"Select out-of-date packs ({_canUpdateCount})";
        MissingPackText = $"Select missing packs ({_isNotOnDiskCount})";
        Thread.Sleep(500);
        OverlayVisibility = false;
    }
}