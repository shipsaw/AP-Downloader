using System.Windows;
using System.Windows.Controls;
using ApDownloader.Model;

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

    private void ToggleSelected(object sender, RoutedEventArgs e)
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

    private void SelectUpdateCheckbox_OnClick(object sender, RoutedEventArgs e)
    {
        if (_toggleItemsToUpdate == false)
        {
            foreach (Cell cell in AddonsFoundList.Items)
                if (cell.CanUpdate)
                    AddonsFoundList.SelectedItems.Add(cell);
            _toggleItemsToUpdate = true;
        }
        else
        {
            foreach (Cell cell in AddonsFoundList.Items)
                if (cell.CanUpdate)
                    AddonsFoundList.SelectedItems.Remove(cell);
            _toggleItemsToUpdate = false;
        }
    }

    private void SelectUnDownloadedCheckbox(object sender, RoutedEventArgs e)
    {
        {
            if (_toggleItemsNotDownloaded == false)
            {
                foreach (Cell cell in AddonsFoundList.Items)
                    if (cell.IsNotOnDisk)
                        AddonsFoundList.SelectedItems.Add(cell);
                _toggleItemsNotDownloaded = true;
            }
            else
            {
                foreach (Cell cell in AddonsFoundList.Items)
                    if (cell.IsNotOnDisk)
                        AddonsFoundList.SelectedItems.Remove(cell);
                _toggleItemsNotDownloaded = false;
            }
        }
    }
}