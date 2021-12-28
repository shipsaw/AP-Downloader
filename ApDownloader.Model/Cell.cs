namespace ApDownloader.Model;

public class Cell
{
    public int? ProductID { get; set; }
    public string ImageUrl { get; set; }
    public string Name { get; set; }
    public bool CanUpdate { get; set; }
    public bool IsNotOnDisk { get; set; }
}