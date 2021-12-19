using System.Net.Http;

namespace ApDownloader.UI.MVVM.ViewModels;

public class DownloadViewModel
{
    public HttpClient? Client { get; set; }
}