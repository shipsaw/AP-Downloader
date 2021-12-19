using System.Collections.ObjectModel;
using System.Net.Http;
using ApDownloader.Model;
using ApDownloader.UI.Core;
using ApDownloader.UI.MVVM.Data;

namespace ApDownloader.UI.MVVM.ViewModels;

public class DownloadViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    public Product _selectedProduct;

    public DownloadViewModel()
    {
        _dataService = new DataService();
        Products = new ObservableCollection<Product>();
    }

    public Product SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            _selectedProduct = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<Product> Products { get; set; }
    public HttpClient? Client { get; set; }

    public void Load()
    {
        var products = _dataService.GetAll();
        Products.Clear();
        foreach (var product in products) Products.Add(product);
    }
}