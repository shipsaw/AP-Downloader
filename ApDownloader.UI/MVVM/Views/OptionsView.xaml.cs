using System.Windows;
using System.Windows.Forms;
using UserControl = System.Windows.Controls.UserControl;

namespace ApDownloader.UI.MVVM.Views;

public partial class OptionsView : UserControl
{
    public OptionsView()
    {
        InitializeComponent();
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
    }
}