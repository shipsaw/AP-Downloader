using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using ApDownloader.UI.MVVM.ViewModels;
using UserControl = System.Windows.Controls.UserControl;

namespace ApDownloader.UI.MVVM.Views;

public partial class OptionsView : UserControl
{
    public OptionsView()
    {
        InitializeComponent();
        DataContext = new OptionsViewModel(false);
        ((Storyboard) FindResource("animate")).Begin(InvalidInstallpathText);
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
        var valid = File.Exists(Path.Combine(openFileDlg.SelectedPath, "RailWorks.exe"));
        if (result != string.Empty && result != "Cancel" && valid)
        {
            viewModel.IsInstallFolderInValid = false;
            viewModel.SetInstallFilepathCommand.Execute(openFileDlg.SelectedPath);
        }
        else
        {
            ((Storyboard) FindResource("animate")).Begin(InvalidInstallpathText);
            var viewmodel = (OptionsViewModel) DataContext;
            viewmodel.IsInstallFolderInValid = true;
        }
    }
}