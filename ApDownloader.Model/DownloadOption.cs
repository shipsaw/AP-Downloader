namespace ApDownloader.Model;

public class DownloadOption
{
    public bool GetExtraStock { get; set; } = true;
    public bool GetBrandingPatch { get; set; } = true;
    public bool GetLiveryPack { get; set; } = true;
    public string DownloadFilepath { get; set; } = @".\";
    public string InstallFilePath { get; set; } = @"C:\Program Files (x86)\Steam\steamapps\common\Railworks";
}