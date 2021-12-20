using System.Windows;
using System.Windows.Controls;
using ApDownloader.UI.MVVM.ViewModels;

namespace ApDownloader.UI.MVVM.Views;

public partial class DownloadView : UserControl
{
    private readonly DownloadViewModel _viewModel;

    public DownloadView()
    {
        InitializeComponent();
        _viewModel = new DownloadViewModel();
        DataContext = _viewModel;
        Loaded += DownloadWindow_Loaded;
    }

    private void DownloadWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _viewModel.Load();
        var test = Parent;
    }
}