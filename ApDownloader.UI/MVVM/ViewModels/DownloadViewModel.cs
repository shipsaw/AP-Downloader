using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
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
    private string _missingPackText;
    private string _outOfDateText;
    private bool _overlayVisibility = true;
    private bool _selectAllButtonEnabled;
    private bool _selectedToggle;
    private bool _toggleItemsNotDownloaded;
    private bool _toggleItemsToUpdate;

    public DownloadViewModel()
    {
        _dataService = new SQLiteDataAccess();
        _access = new HttpDataAccess(LoginView.Client);
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

    public bool DownloadButtonEnabled
    {
        get => _downloadButtonVisible;
        set
        {
            _downloadButtonVisible = value;
            OnPropertyChanged();
        }
    }

    public bool SelectAllButtonEnabled
    {
        get => _selectAllButtonEnabled;
        set
        {
            _selectAllButtonEnabled = value;
            OnPropertyChanged();
        }
    }

    public async void Loaded()
    {
        _access = new HttpDataAccess(LoginView.Client);
        MainViewModel.Products = await _dataService.GetProductsOnly();
        MainViewModel.Products = await _access.GetPurchasedProducts(MainViewModel.Products);
        var allFiles = Directory
            .EnumerateFiles(MainViewModel.DlOption.DownloadFilepath, "*.zip", SearchOption.AllDirectories)
            .Select(file => (new FileInfo(file).Length, new FileInfo(file).Name));
        foreach (var product in MainViewModel.Products)
            product.UserContentLength = allFiles.FirstOrDefault(file => file.Name == product.FileName).Length;
        //await _dataService.UpdateUserContentLength(products);
        foreach (var product in MainViewModel.Products)
        {
            var cell = new Cell
            {
                ProductID = product.ProductID,
                ImageUrl = "../../Images/" + product.ImageName,
                Name = product.Name,
                IsNotOnDisk = product.UserContentLength == 0 ? Visibility.Visible : Visibility.Hidden,
                CanUpdate = product.UserContentLength != 0 && product.UserContentLength != product.CurrentContentLength
                    ? Visibility.Visible
                    : Visibility.Hidden
            };
            if (cell.IsNotOnDisk == Visibility.Visible) _isNotOnDiskCount++;
            if (cell.CanUpdate == Visibility.Visible) _canUpdateCount++;
            ProductCells.Add(cell);
        }

        OutOfDateText = $"Select out-of-date packs ({_canUpdateCount})";
        MissingPackText = $"Select missing packs ({_isNotOnDiskCount})";
        DownloadButtonEnabled = ProductCells.Any();
        SelectAllButtonEnabled = ProductCells.Any();
        OverlayVisibility = false;
    }
}