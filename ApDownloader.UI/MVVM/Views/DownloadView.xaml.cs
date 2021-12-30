using System.Windows;
using System.Windows.Controls;

namespace ApDownloader.UI.MVVM.Views;

public partial class DownloadView : UserControl
{
    private bool _selectedToggle;
    private bool _toggleItemsNotDownloaded;
    private bool _toggleItemsToUpdate;

    public DownloadView()
    {
        InitializeComponent();
    }

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

    /*
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