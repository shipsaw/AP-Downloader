﻿using System;
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
        DataContext = new OptionsViewModel(true);
        var viewmodel = (OptionsViewModel) DataContext;
        if (viewmodel.IsInstallFolderInValid)
            ((Storyboard) FindResource("animate")).Begin(InvalidInstallpathText);
    }

    private void DownloadFolderSelection(object sender, RoutedEventArgs routedEventArgs)
    {
        var viewModel = (OptionsViewModel) DataContext;
        var openFileDlg = new FolderBrowserDialog();
        openFileDlg.SelectedPath = Directory.Exists(DownloadFolderPath.Text)
            ? DownloadFolderPath.Text
            : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var result = openFileDlg.ShowDialog().ToString();
        if (result != string.Empty && result != "Cancel")
            viewModel.SetDownloadFilepathCommand.Execute(openFileDlg.SelectedPath);
    }

    private void InstallFolderSelection(object sender, RoutedEventArgs routedEventArgs)
    {
        var viewModel = (OptionsViewModel) DataContext;
        var currentStatus = viewModel.IsInstallFolderInValid;
        var openFileDlg = new FolderBrowserDialog();
        openFileDlg.SelectedPath = Directory.Exists(InstallFolderPath.Text)
            ? InstallFolderPath.Text
            : Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        var result = openFileDlg.ShowDialog().ToString();
        var valid = File.Exists(Path.Combine(openFileDlg.SelectedPath, "RailWorks.exe"));
        if (result != string.Empty && result != "Cancel" && valid)
        {
            viewModel.IsInstallFolderInValid = false;
            viewModel.SetInstallFilepathCommand.Execute(openFileDlg.SelectedPath);
        }
        else if (result != string.Empty && result != "Cancel")
        {
            ((Storyboard) FindResource("animate")).Begin(InvalidInstallpathText);
            var viewmodel = (OptionsViewModel) DataContext;
            viewmodel.IsInstallFolderInValid = currentStatus;
        }
    }
}