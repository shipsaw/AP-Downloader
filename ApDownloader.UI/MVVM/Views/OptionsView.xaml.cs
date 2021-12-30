using System.IO;
using System.Windows;
using System.Windows.Forms;
using ApDownloader.DataAccess;
using ApDownloader.UI.MVVM.ViewModels;
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

        ExtraStockCheckbox.IsChecked = MainViewModel.DlOption.GetExtraStock;
        BrandingPatchCheckbox.IsChecked = MainViewModel.DlOption.GetBrandingPatch;
        LiveryPackCheckbox.IsChecked = MainViewModel.DlOption.GetLiveryPack;
        DownloadFolder.Text = Path.Combine(MainViewModel.DlOption.DownloadFilepath, @"ApDownloads");
        InstallFolder.Text = MainViewModel.DlOption.InstallFilePath;
        _actualDownloadFolderLoc = MainViewModel.DlOption.DownloadFilepath;
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

    private void InstallFolderSelection(object sender, RoutedEventArgs e)
    {
        var openFileDlg = new FolderBrowserDialog();
        var result = openFileDlg.ShowDialog();
        if (result.ToString() != string.Empty) InstallFolder.Text = openFileDlg.SelectedPath;
    }

    private async void ApplySettings(object sender, RoutedEventArgs e)
    {
        MainViewModel.DlOption.GetExtraStock = ExtraStockCheckbox.IsChecked ?? false;
        MainViewModel.DlOption.GetBrandingPatch = BrandingPatchCheckbox.IsChecked ?? false;
        MainViewModel.DlOption.GetLiveryPack = LiveryPackCheckbox.IsChecked ?? false;
        MainViewModel.DlOption.DownloadFilepath = _actualDownloadFolderLoc;
        MainViewModel.DlOption.InstallFilePath = InstallFolder.Text;
        await _dataAccess.SetUserOptions(MainViewModel.DlOption);
        ApplyResponse.Visibility = Visibility.Visible;
        ApplyButton.IsEnabled = false;
    }

    private void ResetDirty(object sender, RoutedEventArgs routedEventArgs)
    {
        ApplyResponse.Visibility = Visibility.Hidden;
        ApplyButton.IsEnabled = true;
    }
}