using System.Collections.Generic;

namespace ApDownloader.Model;

public class DownloadManifest
{
    public IEnumerable<string> ProductIds { get; set; } = new List<string>();
    public IEnumerable<string> PrFilenames { get; set; } = new List<string>();
    public IEnumerable<string> EsFilenames { get; set; } = new List<string>();
    public IEnumerable<string> BpFilenames { get; set; } = new List<string>();
    public IEnumerable<string> LpFilenames { get; set; } = new List<string>();
}