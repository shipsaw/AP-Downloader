using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
    public static DownloadOption DownloadOption;
    private readonly SQLiteDataAccess _dataService;
    private HttpDataAccess _access;
    private bool _selectedToggle = true;
    private DownloadManifest DownloadManifest;

    public InstallView()
    {
        InitializeComponent();
        DataContext = this;
        _dataService = new SQLiteDataAccess();
        ProductCells = new ObservableCollection<Cell>();
        DownloadOption = DownloadView.DownloadOption;
        Loaded += DownloadWindow_Loaded;
    }

    public HttpClient? Client { get; set; }
    public ObservableCollection<Cell> ProductCells { get; set; }
    public IEnumerable<Product> Products { get; set; }

    private async void DownloadWindow_Loaded(object sender, RoutedEventArgs e)
    {
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

        AddonsFoundList.SelectAll();
        BusyTextBlock.Text = "Installing Addons";
        InstallOverlay.Visibility = Visibility.Collapsed;
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
        InstallOverlay.Visibility = Visibility.Visible;
        DownloadManifest = await GenerateDownloadManifest(DownloadOption, productIds);
        await Task.Run(() =>
        {
            var dir = new DirectoryInfo(Path.Combine(DownloadOption.TempFilePath + "ApDownloads"));
            if (dir.Exists)
                dir.Delete(true);

            if (DownloadManifest.ProductIds != null && DownloadManifest.ProductIds.Any())
                InstallAddons(DownloadOption, DownloadManifest.PrFilenames, "Products/", progress);

            if (DownloadOption.GetExtraStock && DownloadManifest.EsFilenames.Any())
                InstallAddons(DownloadOption, DownloadManifest.EsFilenames, "ExtraStock/", progress);
            if (DownloadOption.GetBrandingPatch && DownloadManifest.BpFilenames.Any())
                InstallAddons(DownloadOption, DownloadManifest.BpFilenames, "BrandingPatches/", progress);

            if (DownloadOption.GetLiveryPack && DownloadManifest.LpFilenames.Any())
                InstallAddons(DownloadOption, DownloadManifest.LpFilenames, "LiveryPacks/", progress);

            dir.Delete(true);
        });
        BusyTextBlock.Text = "Installation Complete";
    }

    private static void InstallAddons(DownloadOption downloadOption, IEnumerable<string> filenames, string folder,
        IProgress<int> progress)
    {
        var extractPath = AddonInstaller.AddonInstaller.UnzipAddons(downloadOption, filenames, folder);
        AddonInstaller.AddonInstaller.InstallAddons(downloadOption, extractPath, progress);
    }

    private async Task<DownloadManifest> GenerateDownloadManifest(DownloadOption downloadOption,
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