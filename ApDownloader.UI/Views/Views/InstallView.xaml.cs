using System.Windows;
using System.Windows.Controls;
using ApDownloader.DataAccess;
using ApDownloader.Model;

namespace ApDownloader.UI.MVVM.Views;

public partial class InstallView : UserControl
{
    //private readonly SQLiteDataAccess _dataService;
    private HttpDataAccess _access;
    private bool _selectedToggle;
    private DownloadManifest DownloadManifest;

    public InstallView()
    {
        InitializeComponent();
        Loaded += Window_loaded;
    }

    private void Window_loaded(object sender, RoutedEventArgs e)
    {
    }

    public void ToggleSelected(object sender, RoutedEventArgs e)
    {
        if (!_selectedToggle)
        {
            AddonsFoundList.SelectAll();
            SelectAllButton.Content = "Deselect All";
            if (AddonsFoundList.SelectedItems.Count > 0)
                InstallButton.IsEnabled = true;
        }
        else
        {
            AddonsFoundList.UnselectAll();
            SelectAllButton.Content = "Select All";
        }

        _selectedToggle = !_selectedToggle;
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        InstallButton.IsEnabled = AddonsFoundList.SelectedItems.Count > 0;
    }
}