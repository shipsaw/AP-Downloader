using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
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
    private ObservableCollection<Cell> _products;
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

    private async void DownloadWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Products = await _dataService.GetProductsOnly();
        foreach (var product in Products)
        {
            var cell = new Cell
                {ImageUrl = "../../Images/" + product.ImageName, Name = product.Name};
            ProductCells.Add(cell);
        }
    }

    public void ToggleSelected(object sender, RoutedEventArgs e)
    {
        if (!selectedToggle)
            AddonsFoundList.SelectAll();
        else
            AddonsFoundList.UnselectAll();

        selectedToggle = !selectedToggle;
    }

    private void Download(object sender, RoutedEventArgs e)
    {
        var access = new HttpDataAccess(Client);
        foreach (var product in ProductCells) access.GetDownloadInfo(Products);
    }
}