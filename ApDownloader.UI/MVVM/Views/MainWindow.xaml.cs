using System.Windows;
using System.Windows.Input;
using ApDownloader.UI.MVVM.ViewModels;

namespace ApDownloader.UI.MVVM.Views;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    private void Exit(object sender, RoutedEventArgs e)
    {
        var viewModel = (MainViewModel) DataContext;
        if (MessageBox.Show("Sure you want to exit?",
                "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            viewModel.ExitCommand.Execute(null);
    }
}