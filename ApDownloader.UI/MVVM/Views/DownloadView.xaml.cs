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
        Products = new ObservableCollection<Cell>();
        Loaded += DownloadWindow_Loaded;
    }

    public HttpClient? Client { get; set; }

    public ObservableCollection<Cell> Products { get; set; }

    public event DependencyPropertyChangedEventHandler PropertyChanged;

    private void DownloadWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var products = _dataService.GetProductsOnly();
        foreach (var product in products.Result)
        {
            var cell = new Cell
                {ImageUrl = "../../Images/" + product.ImageName, Name = product.Name};
            Products.Add(cell);
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
}