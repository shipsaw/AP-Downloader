using System.Windows;
using System.Windows.Controls;
using ApDownloader.DataAccess;
using ApDownloader.Model;

namespace ApDownloader.UI.MVVM.Views;

public partial class InstallView : UserControl
{
    private readonly SQLiteDataAccess _dataService;
    private HttpDataAccess _access;
    private bool _selectedToggle;
    private DownloadManifest DownloadManifest;

    public InstallView()
    {
        InitializeComponent();
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

    private void DisablePrevDownloadsAfterClick(object sender, RoutedEventArgs e)
    {
        PreviousDownloadButton.IsEnabled = false;
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        InstallButton.IsEnabled = AddonsFoundList.SelectedItems.Count > 0;
    }
}