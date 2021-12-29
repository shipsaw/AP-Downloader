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
            _dataService.GetTotalFileCount(MainWindow.DlOption, productIds);
        var progress =
            new Progress<int>(report =>
            {
                BusyTextBlock.Text = $"Installing file {++completedFileCount} of {totalFileCount.Result}";
            });
        InstallOverlay.Visibility = Visibility.Visible;
        DownloadManifest = await _dataService.GetDownloadManifest(MainWindow.DlOption, productIds);
        await Task.Run(() =>
        {
            var dir = new DirectoryInfo(Path.Combine(MainWindow.DlOption.TempFilePath + "ApDownloads"));
            if (dir.Exists)
                dir.Delete(true);

            if (DownloadManifest.ProductIds != null && DownloadManifest.ProductIds.Any())
                InstallAddons(MainWindow.DlOption, DownloadManifest.PrFilenames, "Products/", progress);

            if (MainWindow.DlOption.GetExtraStock && DownloadManifest.EsFilenames.Any())
                InstallAddons(MainWindow.DlOption, DownloadManifest.EsFilenames, "ExtraStock/", progress);
            if (MainWindow.DlOption.GetBrandingPatch && DownloadManifest.BpFilenames.Any())
                InstallAddons(MainWindow.DlOption, DownloadManifest.BpFilenames, "BrandingPatches/", progress);

            if (MainWindow.DlOption.GetLiveryPack && DownloadManifest.LpFilenames.Any())
                InstallAddons(MainWindow.DlOption, DownloadManifest.LpFilenames, "LiveryPacks/", progress);

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
}