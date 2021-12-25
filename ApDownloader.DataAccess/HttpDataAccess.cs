using System.Collections.Generic;
using System.Net.Http;
using ApDownloader.Model;

namespace ApDownloader.DataAccess;

public class HttpDataAccess
{
    private readonly string _brandingPatchPrefix = "https://armstrongpowerhouse.com/free_download/Patches/";
    private readonly HttpClient _client;

    private readonly string _extraStockPrefix = "https://armstrongpowerhouse.com/free_download/";

    private readonly string _liveryPrefix = "https://armstrongpowerhouse.com/free_download/";

    private readonly string _productPrefix =
        "https://www.armstrongpowerhouse.com/index.php?route=account/download/download&download_id=";

    public HttpDataAccess(HttpClient client)
    {
        _client = client;
    }

    public async void GetDownloadInfo(IEnumerable<Product> products)
    {
        foreach (var product in products)
        {
            var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Head,
                "https://www.armstrongpowerhouse.com/index.php?route=account/download/download&download_id=" +
                product.ProductID + 1));
            var result = response.Content.Headers.ContentDisposition;
            if (result == null) break;
        }
    }
}