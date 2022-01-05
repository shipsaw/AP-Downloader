using System.Windows;
using System.Windows.Input;

namespace AP_Installer;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    public MainWindow()
    {
        var viewModel = new MainWindowViewModel();
        DataContext = viewModel;
        InitializeComponent();
        Loaded += Window_Loaded;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var viewModel = (MainWindowViewModel) DataContext;
        viewModel.BeginInstall.Execute(null);
    }
}