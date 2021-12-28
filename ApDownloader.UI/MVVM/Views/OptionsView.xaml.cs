using System.Windows;
using System.Windows.Forms;
using UserControl = System.Windows.Controls.UserControl;

namespace ApDownloader.UI.MVVM.Views;

public partial class OptionsView : UserControl
{
    public OptionsView()
    {
        InitializeComponent();
        RootFolder.Text = DownloadView.DownloadOption.DownloadFilepath;
        ApplyButton.IsEnabled = false;
    }

    private void DownloadFolderSelection(object sender, RoutedEventArgs e)
    {
        var openFileDlg = new FolderBrowserDialog();
        var result = openFileDlg.ShowDialog();
        if (result.ToString() != string.Empty) RootFolder.Text = openFileDlg.SelectedPath;
    }

    private void RailwordsFolderSelection(object sender, RoutedEventArgs e)
    {
        var openFileDlg = new FolderBrowserDialog();
        var result = openFileDlg.ShowDialog();
        if (result.ToString() != string.Empty) RailworksFolder.Text = openFileDlg.SelectedPath;
    }

    private void ApplySettings(object sender, RoutedEventArgs e)
    {
        DownloadView.DownloadOption.GetExtraStock = ExtraStockCheckbox.IsChecked.Value;
        DownloadView.DownloadOption.GetBrandingPatch = BrandingPatchCheckbox.IsChecked.Value;
        DownloadView.DownloadOption.GetLiveryPack = LiveryPackCheckbox.IsChecked.Value;
        DownloadView.DownloadOption.DownloadFilepath = RootFolder.Text;
        DownloadView.DownloadOption.InstallFilePath = RailworksFolder.Text;
        ApplyResponse.Visibility = Visibility.Visible;
        ApplyButton.IsEnabled = false;
    }

    private void ResetDirty(object sender, RoutedEventArgs routedEventArgs)
    {
        ApplyResponse.Visibility = Visibility.Hidden;
        ApplyButton.IsEnabled = true;
    }
}