using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ApDownloader.DataAccess;
using ApDownloader.Model;

namespace ApDownloader.UI.MVVM.Views;

public partial class InstallView : UserControl
{
    public static DownloadOption DownloadOption = new();
    public static DownloadManifest DownloadManifest;
    private readonly SQLiteDataAccess _dataService;
    private HttpDataAccess _access;
    private bool _selectedToggle;

    public InstallView()
    {
        InitializeComponent();
        DataContext = this;
        _dataService = new SQLiteDataAccess();
        ProductCells = new ObservableCollection<Cell>();
        Loaded += DownloadWindow_Loaded;
    }

    public HttpClient? Client { get; set; }
    public ObservableCollection<Cell> ProductCells { get; set; }
    public IEnumerable<Product> Products { get; set; }

    private async void DownloadWindow_Loaded(object sender, RoutedEventArgs e)
    {
        BusyTextBlock.Text = "LOADING ADDONS";
        Overlay.Visibility = Visibility.Visible;
        var products = await _dataService.GetDownloadedProductsOnly(DownloadView.DownloadManifest?.ProductIds);
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

    private async void Install(object sender, RoutedEventArgs e)
    {
        var selected = AddonsFoundList.SelectedItems;
        var productIds = new List<int>();
        foreach (Cell cell in selected)
            if (cell.ProductID != null)
                productIds.Add(cell.ProductID.Value);


        var completedFileCount = 0;
        var totalFileCount =
            _dataService.GetTotalFileCount(DownloadView.DownloadOption, productIds);
        var progress =
            new Progress<int>(report =>
            {
                BusyTextBlock.Text = $"Installing file {++completedFileCount} of {totalFileCount.Result}";
            });
        Overlay.Visibility = Visibility.Visible;
        DownloadManifest = await GenerateDownloadManifest(DownloadOption, productIds);
        BusyTextBlock.Text = "Download Complete";
        // UGLY TEST CODE
        Task.Delay(1000);
        BusyTextBlock.Text = "Installing Addons";
        if (DownloadManifest.ProductIds.Count() > 0)
        {
            var extractPath =
                AddonInstaller.AddonInstaller.UnzipAddons(DownloadOption, DownloadManifest.PrFilenames,
                    "Products/");
            AddonInstaller.AddonInstaller.InstallAddons(DownloadOption, extractPath);
        }

        if (DownloadOption.GetExtraStock && DownloadManifest.EsFilenames.Count() > 0)
        {
            var extractPath =
                AddonInstaller.AddonInstaller.UnzipAddons(DownloadOption, DownloadManifest.EsFilenames,
                    "ExtraStock/");
            AddonInstaller.AddonInstaller.InstallAddons(DownloadOption, extractPath);
        }

        if (DownloadOption.GetBrandingPatch && DownloadManifest.BpFilenames.Count() > 0)
        {
            var extractPath = AddonInstaller.AddonInstaller.UnzipAddons(DownloadOption, DownloadManifest.BpFilenames,
                "BrandingPacks/");
            AddonInstaller.AddonInstaller.InstallAddons(DownloadOption, extractPath);
        }

        if (DownloadOption.GetLiveryPack && DownloadManifest.LpFilenames.Count() > 0)
        {
            var extractPath = AddonInstaller.AddonInstaller.UnzipAddons(DownloadOption, DownloadManifest.LpFilenames,
                "LiveryPacks/");
            AddonInstaller.AddonInstaller.InstallAddons(DownloadOption, extractPath);
        }
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