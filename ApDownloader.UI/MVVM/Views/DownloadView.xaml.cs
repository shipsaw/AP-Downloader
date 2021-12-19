using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ApDownloader.UI.MVVM.Views;

public partial class DownloadView : UserControl
{
    private readonly List<string> AddonNames = new()
    {
        "Class 37 Pack",
        "Weather Enhancement Pack",
        "Class 317 Pack"
    };

    public DownloadView()
    {
        InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        foreach (var addonName in AddonNames)
        {
            var item = new TreeViewItem();
            item.Header = addonName;
            item.Tag = "TheTag";
            item.Items.Add(null);
            AddonList.Items.Add(item);
        }
    }
}