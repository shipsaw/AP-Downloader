namespace ApDownloader.Model;

public class Product
{
    public int ProductID { get; set; }
    public string Name { get; set; }
    public string FileName { get; set; }
    public string ImageName { get; set; }
    public bool IsMissing { get; set; }
    public bool CanUpdate { get; set; }
    public long CurrentContentLength { get; set; }
    public long UserContentLength { get; set; }
}