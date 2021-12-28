using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ApDownloader.DataAccess;
using ApDownloader.Model;

namespace ApDownloader.UI.MVVM.Views;

public partial class DownloadView : UserControl
{
    public static DownloadOption DownloadOption = new();
    public static DownloadManifest DownloadManifest;
    private readonly SQLiteDataAccess _dataService;
    private HttpDataAccess _access;
    private int _canUpdateCount;
    private int _isNotOnDiskCount;
    private bool _selectedToggle;
    private bool _toggleItemsNotDownloaded;
    private bool _toggleItemsToUpdate;

    public DownloadView()
    {
        InitializeComponent();
        DataContext = this;
        _dataService = new SQLiteDataAccess();
        ProductCells = new ObservableCollection<Cell>();
        Loaded += DownloadWindow_Loaded;
    }

    public ObservableCollection<Cell> ProductCells { get; set; }
    public IEnumerable<Product> Products { get; set; }

    private async void DownloadWindow_Loaded(object sender, RoutedEventArgs e)
    {
        BusyTextBlock.Text = "LOADING ADDONS";
        Overlay.Visibility = Visibility.Visible;
        _access = new HttpDataAccess(LoginView.Client);
        Products = await _dataService.GetProductsOnly();
        Products = await _access.GetPurchasedProducts(Products);
        await _dataService.UpdateCurrentContentLength(Products);
        var allFiles = Directory.EnumerateFiles(DownloadOption.DownloadFilepath, "*.zip", SearchOption.AllDirectories)
            .Select(file => (new FileInfo(file).Length, new FileInfo(file).Name));
        foreach (var product in Products)
            product.UserContentLength = allFiles.FirstOrDefault(file => file.Name == product.FileName).Length;
        //await _dataService.UpdateUserContentLength(products);
        foreach (var product in Products)
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

        OutOfDateTextBlock.Text = $"Select out-of-date packs ({_canUpdateCount})";
        MissingPackTextBlock.Text = $"Select missing packs ({_isNotOnDiskCount})";
        Overlay.Visibility = Visibility.Collapsed;
    }

    public void ToggleSelected(object sender, RoutedEventArgs e)
    {
        if (!_selectedToggle)
        {
            AddonsFoundList.SelectAll();
            SelectAllButton.Content = "Deselect All";
            _toggleItemsToUpdate = true;
            _toggleItemsNotDownloaded = true;
            UpdateCheckbox.IsChecked = true;
            UnDownloadedCheckbox.IsChecked = true;
        }
        else
        {
            AddonsFoundList.UnselectAll();
            SelectAllButton.Content = "Select All";
            _toggleItemsToUpdate = false;
            _toggleItemsNotDownloaded = false;
            UpdateCheckbox.IsChecked = false;
            UnDownloadedCheckbox.IsChecked = false;
        }

        _selectedToggle = !_selectedToggle;
    }

    private async void Download(object sender, RoutedEventArgs e)
    {
        var selected = AddonsFoundList.SelectedItems;
        var productIds = new List<int>();
        foreach (Cell cell in selected)
            if (cell.ProductID != null)
                productIds.Add(cell.ProductID.Value);


        var completedFileCount = 0;
        var totalFileCount =
            _dataService.GetTotalFileCount(DownloadOption, productIds);
        var progress =
            new Progress<int>(report =>
            {
                BusyTextBlock.Text = $"Downloading file {++completedFileCount} of {totalFileCount.Result}";
            });
        Overlay.Visibility = Visibility.Visible;
        DownloadManifest = await GenerateDownloadManifest(DownloadOption, productIds);
        await _access.Download(DownloadManifest, DownloadOption, progress);
        BusyTextBlock.Text = "Download Complete";
    }

    public async Task<DownloadManifest> GenerateDownloadManifest(DownloadOption downloadOption,
        IEnumerable<int> productIds)
    {
        var dbAccess = new SQLiteDataAccess();
        return new DownloadManifest
        {
            ProductIds = productIds.Select(id => id.ToString()),
            PrFilenames = await dbAccess.GetExtras("Product", productIds),
            EsFilenames = await dbAccess.GetExtras("ExtraStock", productIds),
            BpFilenames = await dbAccess.GetExtras("BrandingPatch", productIds),
            LpFilenames = await dbAccess.GetExtras("LiveryPack", productIds)
        };
    }

    private void SelectUpdateCheckbox_OnClick(object sender, RoutedEventArgs e)
    {
        if (_toggleItemsToUpdate == false)
        {
            foreach (var product in ProductCells)
                if (product.CanUpdate == Visibility.Visible)
                    AddonsFoundList.SelectedItems.Add(product);
            _toggleItemsToUpdate = true;
        }
        else
        {
            foreach (var product in ProductCells)
                if (product.CanUpdate == Visibility.Visible)
                    AddonsFoundList.SelectedItems.Remove(product);
            _toggleItemsToUpdate = false;
        }
    }

    private void SelectUnDownloadedCheckbox(object sender, RoutedEventArgs e)
    {
        {
            if (_toggleItemsNotDownloaded == false)
            {
                foreach (var product in ProductCells)
                    if (product.IsNotOnDisk == Visibility.Visible)
                        AddonsFoundList.SelectedItems.Add(product);
                _toggleItemsNotDownloaded = true;
            }
            else
            {
                foreach (var product in ProductCells)
                    if (product.IsNotOnDisk == Visibility.Visible)
                        AddonsFoundList.SelectedItems.Remove(product);
                _toggleItemsNotDownloaded = false;
            }
        }
    }
}