using System.IO;

namespace ApDownloader.Model;

public class DownloadOption
{
    public bool GetExtraStock { get; set; } = true;
    public bool GetBrandingPatch { get; set; } = true;
    public bool GetLiveryPack { get; set; } = true;
    public string DownloadFilepath { get; set; } = @".\";
    public string InstallFilePath { get; set; } = @"C:\Test";
    public string TempFilePath { get; set; } = Path.GetTempPath();
}