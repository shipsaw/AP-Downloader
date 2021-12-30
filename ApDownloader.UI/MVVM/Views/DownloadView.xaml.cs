using System.Windows;
using System.Windows.Controls;
using ApDownloader.UI.MVVM.ViewModels;

namespace ApDownloader.UI.MVVM.Views;

public partial class DownloadView : UserControl
{
    public DownloadView()
    {
        InitializeComponent();
        DataContext = new DownloadViewModel();
        Loaded += DownloadWindow_Loaded;
    }

    private async void DownloadWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var viewModel = (DownloadViewModel) DataContext;
        viewModel.Loaded();
    }

    /*
    public void ToggleSelected(object sender, RoutedEventArgs e)
    {
        if (!_selectedToggle)
        {
            AddonsFoundList.SelectAll();
            SelectAllButton.Content = "Deselect All";
            _toggleItemsToUpdate = true;
            _toggleItemsNotDownloaded = true;
            UpdateCheckbox.IsChecked = true;
            UnDownloadedCheckbox.IsChecked = true;
        }
        else
        {
            AddonsFoundList.UnselectAll();
            SelectAllButton.Content = "Select All";
            _toggleItemsToUpdate = false;
            _toggleItemsNotDownloaded = false;
            UpdateCheckbox.IsChecked = false;
            UnDownloadedCheckbox.IsChecked = false;
        }

        _selectedToggle = !_selectedToggle;
    }

    private async void Download(object sender, RoutedEventArgs e)
    {
        var selected = AddonsFoundList.SelectedItems;
        var productIds = new List<int>();
        foreach (Cell cell in selected)
            if (cell.ProductID != null)
                productIds.Add(cell.ProductID.Value);


        var completedFileCount = 0;
        var totalFileCount =
            await _dataService.GetTotalFileCount(MainViewModel.DlOption, productIds);
        var progress =
            new Progress<int>(report =>
            {
                BusyTextBlock.Text = $"Downloading file {++completedFileCount} of {totalFileCount}";
            });
        Overlay.Visibility = Visibility.Visible;
        DownloadManifest = await _dataService.GetDownloadManifest(MainViewModel.DlOption, productIds);
        await _access.Download(DownloadManifest, MainViewModel.DlOption, progress);
        BusyTextBlock.Text = "Download Complete";
    }

    private void SelectUpdateCheckbox_OnClick(object sender, RoutedEventArgs e)
    {
        if (_toggleItemsToUpdate == false)
        {
            foreach (var product in ProductCells)
                if (product.CanUpdate == Visibility.Visible)
                    AddonsFoundList.SelectedItems.Add(product);
            _toggleItemsToUpdate = true;
        }
        else
        {
            foreach (var product in ProductCells)
                if (product.CanUpdate == Visibility.Visible)
                    AddonsFoundList.SelectedItems.Remove(product);
            _toggleItemsToUpdate = false;
        }
    }

    private void SelectUnDownloadedCheckbox(object sender, RoutedEventArgs e)
    {
        {
            if (_toggleItemsNotDownloaded == false)
            {
                foreach (var product in ProductCells)
                    if (product.IsNotOnDisk == Visibility.Visible)
                        AddonsFoundList.SelectedItems.Add(product);
                _toggleItemsNotDownloaded = true;
            }
            else
            {
                foreach (var product in ProductCells)
                    if (product.IsNotOnDisk == Visibility.Visible)
                        AddonsFoundList.SelectedItems.Remove(product);
                _toggleItemsNotDownloaded = false;
            }
        }
    }
        */
}