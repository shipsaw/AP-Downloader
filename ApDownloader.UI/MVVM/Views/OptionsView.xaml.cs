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

    private void FolderSelection(object sender, RoutedEventArgs e)
    {
        var openFileDlg = new FolderBrowserDialog();
        var result = openFileDlg.ShowDialog();
        if (result.ToString() != string.Empty) RootFolder.Text = openFileDlg.SelectedPath;
        //root = txtPath.Text;
    }

    private void ApplySettings(object sender, RoutedEventArgs e)
    {
        DownloadView.DownloadOption.GetExtraStock = ExtraStockCheckbox.IsChecked.Value;
        DownloadView.DownloadOption.GetBrandingPatch = BrandingPatchCheckbox.IsChecked.Value;
        DownloadView.DownloadOption.GetLiveryPack = LiveryPackCheckbox.IsChecked.Value;
        DownloadView.DownloadOption.DownloadFilepath = RootFolder.Text;
        ApplyResponse.Visibility = Visibility.Visible;
        ApplyButton.IsEnabled = false;
    }

    private void ResetDirty(object sender, RoutedEventArgs routedEventArgs)
    {
        ApplyResponse.Visibility = Visibility.Hidden;
        ApplyButton.IsEnabled = true;
    }
}