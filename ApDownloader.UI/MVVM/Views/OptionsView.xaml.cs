using System.Windows;
using System.Windows.Forms;
using ApDownloader.DataAccess;
using UserControl = System.Windows.Controls.UserControl;

namespace ApDownloader.UI.MVVM.Views;

public partial class OptionsView : UserControl
{
    private readonly SQLiteDataAccess _dataAccess;
    private string _actualDownloadFolderLoc;

    public OptionsView()
    {
        InitializeComponent();
        ApplyButton.IsEnabled = false;
        _dataAccess = new SQLiteDataAccess();

        ExtraStockCheckbox.IsChecked = MainWindow.DlOption.GetExtraStock;
        BrandingPatchCheckbox.IsChecked = MainWindow.DlOption.GetBrandingPatch;
        LiveryPackCheckbox.IsChecked = MainWindow.DlOption.GetLiveryPack;
        DownloadFolder.Text = MainWindow.DlOption.DownloadFilepath;
        InstallFolder.Text = MainWindow.DlOption.InstallFilePath;
    }


    private void DownloadFolderSelection(object sender, RoutedEventArgs e)
    {
        var openFileDlg = new FolderBrowserDialog();
        var result = openFileDlg.ShowDialog().ToString();
        if (result != string.Empty && result != "Cancel")
        {
            _actualDownloadFolderLoc = openFileDlg.SelectedPath;
            if (_actualDownloadFolderLoc.EndsWith(@"\ApDownloads"))
                _actualDownloadFolderLoc =
                    _actualDownloadFolderLoc.Remove(_actualDownloadFolderLoc.LastIndexOf(@"\ApDownloads"));
            DownloadFolder.Text = _actualDownloadFolderLoc + @"\ApDownloads";
        }
    }

    private void RailwordsFolderSelection(object sender, RoutedEventArgs e)
    {
        var openFileDlg = new FolderBrowserDialog();
        var result = openFileDlg.ShowDialog();
        if (result.ToString() != string.Empty) InstallFolder.Text = openFileDlg.SelectedPath;
    }

    private async void ApplySettings(object sender, RoutedEventArgs e)
    {
        MainWindow.DlOption.GetExtraStock = ExtraStockCheckbox.IsChecked ?? false;
        MainWindow.DlOption.GetBrandingPatch = BrandingPatchCheckbox.IsChecked ?? false;
        MainWindow.DlOption.GetLiveryPack = LiveryPackCheckbox.IsChecked ?? false;
        MainWindow.DlOption.DownloadFilepath = _actualDownloadFolderLoc;
        MainWindow.DlOption.InstallFilePath = InstallFolder.Text;
        await _dataAccess.SetUserOptions(MainWindow.DlOption);
        ApplyResponse.Visibility = Visibility.Visible;
        ApplyButton.IsEnabled = false;
    }

    private void ResetDirty(object sender, RoutedEventArgs routedEventArgs)
    {
        ApplyResponse.Visibility = Visibility.Hidden;
        ApplyButton.IsEnabled = true;
    }
}