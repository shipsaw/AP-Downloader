using System.Windows;

namespace ApDownloader.Model;

public class Cell
{
    public int? ProductID { get; set; }
    public string ImageUrl { get; set; }
    public string Name { get; set; }
    public Visibility CanUpdate { get; set; }
    public Visibility IsNotOnDisk { get; set; }
}