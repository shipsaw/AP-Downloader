using System.Collections.ObjectModel;
using System.Net.Http;
using ApDownloader.DataAccess;
using ApDownloader.Model;
using ApDownloader.UI.Core;

namespace ApDownloader.UI.MVVM.ViewModels;

public class DownloadViewModel : ObservableObject
{
    private readonly SQLiteDataAccess _dataService;

    public Product _selectedProduct;

    public DownloadViewModel()
    {
        _dataService = new SQLiteDataAccess();
        Products = new ObservableCollection<Cell>();
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

    public ObservableCollection<Cell> Products { get; set; }
    public HttpClient? Client { get; set; }

    public void Load()
    {
        var products = SQLiteDataAccess.GetProductsOnly();
        foreach (var product in products.Result)
        {
            var cell = new Cell {ImageUrl = "../../Images/" + product.ImageName, Name = product.Name};
            Products.Add(cell);
        }
    }
}