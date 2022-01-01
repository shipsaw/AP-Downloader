using System.IO;

namespace ApDownloader.Model;

public class ApDownloaderConfig
{
    public bool GetExtraStock { get; set; } = true;
    public bool GetBrandingPatch { get; set; } = true;
    public bool GetLiveryPack { get; set; } = true;
    public string DownloadFilepath { get; set; } = @".\ApDownloads";
    public string InstallFilePath { get; set; } = @"C:\Program Files (x86)\Steam\steamapps\common\Railworks";
    public string TempFilePath { get; set; } = Path.GetTempPath();
}