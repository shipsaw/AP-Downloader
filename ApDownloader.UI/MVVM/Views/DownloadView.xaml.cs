using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    private bool _selectedToggle;

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
        var products = await _access.GetPurchasedProducts(Products);
        await _dataService.UpdateContentLength(products);
        foreach (var product in products)
        {
            var cell = new Cell
            {
                ProductID = product.ProductID,
                ImageUrl = "../../Images/" + product.ImageName,
                Name = product.Name
            };
            ProductCells.Add(cell);
        }

        Overlay.Visibility = Visibility.Collapsed;
    }

    public void ToggleSelected(object sender, RoutedEventArgs e)
    {
        if (!_selectedToggle)
        {
            AddonsFoundList.SelectAll();
            SelectAllButton.Content = "Deselect All";
        }
        else
        {
            AddonsFoundList.UnselectAll();
            SelectAllButton.Content = "Select All";
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
}