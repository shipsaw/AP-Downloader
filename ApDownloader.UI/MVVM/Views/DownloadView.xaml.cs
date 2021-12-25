using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using ApDownloader.DataAccess;
using ApDownloader.Model;
using ApDownloader.UI.MVVM.ViewModels;

namespace ApDownloader.UI.MVVM.Views;

public partial class DownloadView : UserControl
{
    private readonly SQLiteDataAccess _dataService;
    private readonly DownloadViewModel _viewModel;
    private string? _areProductsLoading = "Loading...";
    private ObservableCollection<Cell> _products;
    private bool _productsLoaded;
    private bool selectedToggle;

    public DownloadView()
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

    public string? AreProductsLoading
    {
        get => _areProductsLoading;
        set
        {
            _areProductsLoading = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;


    private async void DownloadWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Overlay.Visibility = Visibility.Visible;
        var access = new HttpDataAccess(LoginView.Client);
        Products = await _dataService.GetProductsOnly();
        var products = await access.GetPurchasedProducts(Products);
        foreach (var product in products)
        {
            var cell = new Cell
                {ImageUrl = "../../Images/" + product.ImageName, Name = product.Name};
            ProductCells.Add(cell);
        }

        Overlay.Visibility = Visibility.Collapsed;
    }

    public void ToggleSelected(object sender, RoutedEventArgs e)
    {
        if (!selectedToggle)
        {
            AddonsFoundList.SelectAll();
            SelectAllButton.Content = "Deselect All";
        }
        else
        {
            AddonsFoundList.UnselectAll();
            SelectAllButton.Content = "Select All";
        }

        selectedToggle = !selectedToggle;
    }

    private async void Download(object sender, RoutedEventArgs e)
    {
    }

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}