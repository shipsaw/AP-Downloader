using System.Windows;
using System.Windows.Input;

namespace ApDownloader_Installer;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Install();
    }

    private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    private void Install()
    {
        //var progress =
        //new Progress<int>(report => { BusyText = $"Installing file {++completedFileCount} of {totalFileCount.Result}"; });
    }
}