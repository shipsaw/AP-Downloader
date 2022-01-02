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
using ApDownloader.UI.MVVM.Views;

namespace ApDownloader.UI.MVVM.ViewModels;

public class DownloadViewModel : ObservableObject
{
    private readonly SQLiteDataAccess _dataService;
    private HttpDataAccess _access;
    private string _busyText = "LOADING ADDONS";
    private int _canUpdateCount;
    private bool _downloadButtonVisible;
    private int _isNotOnDiskCount;
    private string _missingPackText = "";
    private string _outOfDateText = "";
    private bool _overlayVisibility = true;
    private bool _selectAllButtonEnabled;

    public DownloadViewModel()
    {
        DownloadCommand = new RelayCommand(list => DownloadAddons((IList) list));
        _dataService = new SQLiteDataAccess(MainViewModel.Config["DbConnectionString"]);
        var concurrentDownloads = int.TryParse(MainViewModel.Config["ConcurrentDownloads"], out var concDl) ? concDl : 1;
        _access = new HttpDataAccess(LoginView.Client, concurrentDownloads);
        LoadUserAddonsCommand = new AsyncRelayCommand.AsyncCommand(LoadUserAddons);
        RenderUserAddons();
    }

    public AsyncRelayCommand.AsyncCommand LoadUserAddonsCommand { get; set; }
    public RelayCommand DownloadCommand { get; set; }

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


    public ObservableCollection<Cell> ProductCells { get; set; } = new();

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

    public bool SelectAllButtonEnabled => MainViewModel.Products.Any();


    private async void DownloadAddons(IList selectedCells)
    {
        MainViewModel.IsNotBusy = false;
        var productIds = new List<int>();
        foreach (Cell cell in selectedCells)
            if (cell.ProductId != null)
                productIds.Add(cell.ProductId);


        var completedFileCount = 0;
        var totalFileCount =
            await _dataService.GetTotalFileCount(MainViewModel.DlOption, productIds);
        var progress =
            new Progress<int>(_ => { BusyText = $"Downloading file {++completedFileCount} of {totalFileCount}"; });
        OverlayVisibility = true;
        MainViewModel.DlManifest = await _dataService.GetDownloadManifest(MainViewModel.DlOption, productIds);
        await _access.Download(MainViewModel.DlManifest, MainViewModel.DlOption, progress);
        MainViewModel.IsDownloadDataDirty = true;
        await LoadUserAddons();
        BusyText = "Download Complete";
        MainViewModel.IsNotBusy = true;
    }

    private async Task LoadUserAddons()
    {
        if (!MainViewModel.Products.Any() || MainViewModel.IsDownloadDataDirty)
        {
            _access = new HttpDataAccess(LoginView.Client);
            MainViewModel.Products = await _dataService.GetProductsOnly();
            MainViewModel.Products = await _access.GetPurchasedProducts(MainViewModel.Products);
            var allFiles = Directory
                .EnumerateFiles(MainViewModel.DlOption.DownloadFilepath, "*.zip", SearchOption.AllDirectories)
                .Select(file => (new FileInfo(file).Length, new FileInfo(file).Name));
            foreach (var product in MainViewModel.Products)
            {
                product.UserContentLength = allFiles.FirstOrDefault(file => file.Name == product.FileName).Length;
                product.CanUpdate = product.UserContentLength != product.CurrentContentLength &&
                                    product.UserContentLength != 0;
                product.IsMissing = product.UserContentLength == 0;
            }

            MainViewModel.IsDownloadDataDirty = false;
        }
    }

    private async void RenderUserAddons()
    {
        if (MainViewModel.IsDownloadDataDirty)
        {
            await LoadUserAddons();
            MainViewModel.IsDownloadDataDirty = false;
        }

        foreach (var product in MainViewModel.Products)
        {
            var cell = new Cell(
                product.ProductID,
                "../../Images/" + product.ImageName,
                product.Name,
                product.CanUpdate,
                product.IsMissing
            );
            if (cell.IsNotOnDisk) _isNotOnDiskCount++;
            if (cell.CanUpdate) _canUpdateCount++;
            ProductCells.Add(cell);
        }

        OutOfDateText = $"Select out-of-date packs ({_canUpdateCount})";
        MissingPackText = $"Select missing packs ({_isNotOnDiskCount})";
        OverlayVisibility = false;
    }
}