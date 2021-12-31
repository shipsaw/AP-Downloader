using System.Windows;
using System.Windows.Forms;
using ApDownloader.UI.MVVM.ViewModels;
using UserControl = System.Windows.Controls.UserControl;

namespace ApDownloader.UI.MVVM.Views;

public partial class OptionsView : UserControl
{
    public OptionsView()
    {
        InitializeComponent();
        DataContext = new OptionsViewModel();
    }

    private void DownloadFolderSelection(object sender, RoutedEventArgs routedEventArgs)
    {
        var viewModel = (OptionsViewModel) DataContext;
        var openFileDlg = new FolderBrowserDialog();
        var result = openFileDlg.ShowDialog().ToString();
        if (result != string.Empty && result != "Cancel")
            viewModel.SetDownloadFilepathCommand.Execute(openFileDlg.SelectedPath);
    }

    private void InstallFolderSelection(object sender, RoutedEventArgs routedEventArgs)
    {
        var viewModel = (OptionsViewModel) DataContext;
        var openFileDlg = new FolderBrowserDialog();
        var result = openFileDlg.ShowDialog().ToString();
        if (result != string.Empty && result != "Cancel")
            viewModel.SetInstallFilepathCommand.Execute(openFileDlg.SelectedPath);
    }
}