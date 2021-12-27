using System.Collections.Generic;

namespace ApDownloader.Model;

public class DownloadManifest
{
    public IEnumerable<string>? ProductIds { get; set; }
    public IEnumerable<string> PrFilenames { get; set; }
    public IEnumerable<string> EsFilenames { get; set; }
    public IEnumerable<string> BpFilenames { get; set; }
    public IEnumerable<string> LpFilenames { get; set; }
}