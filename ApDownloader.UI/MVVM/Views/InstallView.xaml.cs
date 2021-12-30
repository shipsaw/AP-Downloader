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
using ApDownloader.UI.MVVM.ViewModels;

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
        Loaded += InstallWindow_Loaded;
    }

    public HttpClient? Client { get; set; }
    public ObservableCollection<Cell> ProductCells { get; }
    public IEnumerable<Product> Products { get; set; }

    private async void InstallWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var products = await _dataService.GetDownloadedProductsOnly(MainViewModel.DlManifest?.ProductIds);
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
        InstallButton.IsEnabled = ProductCells.Any(); // && MainViewModel.IsAdmin;
        SelectAllButton.IsEnabled = ProductCells.Any();
    }

    public void ToggleSelected(object sender, RoutedEventArgs e)
    {
        if (!_selectedToggle)
        {
            AddonsFoundList.SelectAll();
            SelectAllButton.Content = "Deselect All";
            if (AddonsFoundList.SelectedItems.Count > 0) // && MainViewModel.IsAdmin)
                InstallButton.IsEnabled = true;
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
            _dataService.GetTotalFileCount(MainViewModel.DlOption, productIds);
        var progress =
            new Progress<int>(report =>
            {
                BusyTextBlock.Text = $"Installing file {++completedFileCount} of {totalFileCount.Result}";
            });
        InstallOverlay.Visibility = Visibility.Visible;
        DownloadManifest = await _dataService.GetDownloadManifest(MainViewModel.DlOption, productIds);
        await Task.Run(() =>
        {
            var dir = new DirectoryInfo(Path.Combine(MainViewModel.DlOption.TempFilePath + "ApDownloads"));
            if (dir.Exists)
                dir.Delete(true);

            if (DownloadManifest.ProductIds != null && DownloadManifest.ProductIds.Any())
                InstallAddons(MainViewModel.DlOption, DownloadManifest.PrFilenames, "Products/", progress);

            if (MainViewModel.DlOption.GetExtraStock && DownloadManifest.EsFilenames.Any())
                InstallAddons(MainViewModel.DlOption, DownloadManifest.EsFilenames, "ExtraStock/", progress);
            if (MainViewModel.DlOption.GetBrandingPatch && DownloadManifest.BpFilenames.Any())
                InstallAddons(MainViewModel.DlOption, DownloadManifest.BpFilenames, "BrandingPatches/", progress);

            if (MainViewModel.DlOption.GetLiveryPack && DownloadManifest.LpFilenames.Any())
                InstallAddons(MainViewModel.DlOption, DownloadManifest.LpFilenames, "LiveryPacks/", progress);

            if (dir.Exists)
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

    private async void GetAllPrevDownloads(object sender, RoutedEventArgs e)
    {
        var allFiles = Directory
            .EnumerateFiles(Path.Combine(MainViewModel.DlOption.DownloadFilepath, "ApDownloads"), "*.zip",
                SearchOption.AllDirectories)
            .Select(file => new FileInfo(file).Name);
        var products = await _dataService.GetDownloadedProductsByName(allFiles);

        foreach (var product in products)
        {
            var cell = new Cell
            {
                ProductID = product.ProductID,
                ImageUrl = "../../Images/" + product.ImageName,
                Name = product.Name
            };
            if (!ProductCells.Contains(cell))
                ProductCells.Add(cell);
        }

        if (ProductCells.Any() /* && MainViewModel.IsAdmin*/)
            InstallButton.IsEnabled = true;
        if (ProductCells.Any())
            SelectAllButton.IsEnabled = true;
        if (allFiles.Any())
        {
            SelectAllButton.Content = "Select All";
            _selectedToggle = false;
        }

        GetAllDownloadedButton.IsEnabled = false;
    }
}