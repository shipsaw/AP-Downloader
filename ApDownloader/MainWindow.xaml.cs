using System.Windows;

namespace ApDownloader;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void loginButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show($"Username: {loginBox.Text}");
    }
}