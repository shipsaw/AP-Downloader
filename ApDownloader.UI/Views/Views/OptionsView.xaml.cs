using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using UserControl = System.Windows.Controls.UserControl;

namespace ApDownloader.UI.MVVM.Views;

public partial class OptionsView : UserControl
{
    public OptionsView()
    {
        InitializeComponent();
        ((Storyboard) FindResource("animate")).Begin(InvalidInstallpathText);
    }

    private void DownloadFolderSelection(object sender, RoutedEventArgs routedEventArgs)
    {
        var openFileDlg = new FolderBrowserDialog();
        openFileDlg.SelectedPath = Directory.Exists(openFileDlg.SelectedPath)
            ? ""
            : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var result = openFileDlg.ShowDialog().ToString();
    }

    private void InstallFolderSelection(object sender, RoutedEventArgs routedEventArgs)
    {
        var openFileDlg = new FolderBrowserDialog();
        openFileDlg.SelectedPath = Directory.Exists(openFileDlg.SelectedPath)
            ? ""
            : Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        var result = openFileDlg.ShowDialog().ToString();
        var valid = File.Exists(Path.Combine(openFileDlg.SelectedPath, "RailWorks.exe"));
        if (result != string.Empty && result != "Cancel" && valid)
        {
        }
        else if (result != string.Empty && result != "Cancel")
        {
            ((Storyboard) FindResource("animate")).Begin(InvalidInstallpathText);
        }
    }

    private void ImportDbFolderSelection(object sender, RoutedEventArgs e)
    {
        string? filePath;
        using var openFileDialog = new OpenFileDialog();

        openFileDialog.InitialDirectory = "c:\\";
        openFileDialog.Filter = "db files (*.db)|*.db";
        openFileDialog.RestoreDirectory = true;

        if (openFileDialog.ShowDialog() != DialogResult.OK) return;
        //Get the path of specified file
        var filename = openFileDialog.FileName;
        if (filename != null)
        {
        }

        ((Storyboard) FindResource("animate")).Begin(UpdatedDbNotificationTextBlock);
    }
}