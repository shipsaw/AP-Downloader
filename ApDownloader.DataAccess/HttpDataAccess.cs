using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApDownloader.Model;

namespace ApDownloader.DataAccess;

public class HttpDataAccess
{
    private readonly List<Task> _allTasks;
    private readonly string _brandingPatchPrefix = "https://www.armstrongpowerhouse.com/free_download/Patches/";
    private readonly HttpClient? _client;

    private readonly string _extraStockPrefix = "https://www.armstrongpowerhouse.com/free_download/";

    private readonly string _liveryPrefix = "https://www.armstrongpowerhouse.com/free_download/";

    private readonly string _productPrefix =
        "https://www.armstrongpowerhouse.com/index.php?route=account/download/download&download_id=";

    private readonly SemaphoreSlim _throttler;

    public HttpDataAccess(HttpClient? client)
    {
        _client = client;
        _allTasks = new List<Task>();
        _throttler = new SemaphoreSlim(3);
    }

    public async Task<IEnumerable<Product>> GetPurchasedProducts(IEnumerable<Product> products)
    {
        var productsList = new List<Product>(products);
        var retProducts = new List<Product>();
        var tasks = new List<Task<HttpResponseMessage>>();
        foreach (var product in products)
        {
            await Task.Delay(30);
            tasks.Add(_client.SendAsync(new HttpRequestMessage(HttpMethod.Head,
                "https://www.armstrongpowerhouse.com/index.php?route=account/download/download&download_id=" +
                product.ProductID)));
        }

        var result = await Task.WhenAll(tasks.ToList());
        if (result.Count() != products.Count()) throw new Exception("Error Checking purchases");

        for (var i = 0; i < products.Count(); i++)
            if (result[i].Content.Headers.ContentDisposition != null)
            {
                productsList[i].ContentLength = result[i].Content.Headers.ContentLength ?? 0;
                retProducts.Add(productsList[i]);
            }

        return retProducts;
    }

    public async Task Download(DownloadManifest downloadManifest, DownloadOption downloadOption,
        IProgress<int> progress)
    {
        // Create Download Directories
        var result = Directory.CreateDirectory(downloadOption.DownloadFilepath + @"\ApDownloads\");
        Directory.CreateDirectory(downloadOption.DownloadFilepath + @"\ApDownloads\Products\");
        Directory.CreateDirectory(downloadOption.DownloadFilepath + @"\ApDownloads\ExtraStock\");
        Directory.CreateDirectory(downloadOption.DownloadFilepath + @"\ApDownloads\BrandingPatches\");
        Directory.CreateDirectory(downloadOption.DownloadFilepath + @"\ApDownloads\LiveryPacks\");
        // Get Base Products
        await DownloadFile(downloadManifest.ProductIds, _productPrefix, progress, downloadOption, "Products");

        // Get Extra Stock
        if (downloadOption.GetExtraStock)
            await DownloadFile(downloadManifest.EsFilenames, _extraStockPrefix, progress, downloadOption, "ExtraStock");

        // Get Branding Patch
        if (downloadOption.GetBrandingPatch)
            await DownloadFile(downloadManifest.BpFilenames, _brandingPatchPrefix, progress, downloadOption,
                "BrandingPatches");

        // Get Liveries
        if (downloadOption.GetLiveryPack)
            await DownloadFile(downloadManifest.LpFilenames, _liveryPrefix, progress, downloadOption, "LiveryPacks");

        await Task.WhenAll(_allTasks);
    }

    private async Task DownloadFile(IEnumerable<string> products, string prefix, IProgress<int> progress,
        DownloadOption downloadOption, string saveLoc)
    {
        foreach (var filename in products)
        {
            var uri = new Uri(prefix + filename);
            await _throttler.WaitAsync();
            await Task.Delay(100);
            _allTasks.Add(
                Task.Run(async () =>
                {
                    try
                    {
                        progress.Report(1);
                        await SaveFile(downloadOption, uri, "ApDownloads/" + saveLoc,
                            prefix == _productPrefix ? "" : filename);
                    }
                    finally
                    {
                        _throttler.Release();
                    }
                }));
        }
    }

    private async Task SaveFile(DownloadOption downloadOption, Uri uri, string saveLoc, string filename)
    {
        var response = await _client.GetAsync(uri);
        if (filename == "") filename = response.Content.Headers.ContentDisposition.FileName.Trim('"');
        using (var stream = await response.Content.ReadAsStreamAsync())
        {
            var fileInfo = new FileInfo(Path.Combine(downloadOption.DownloadFilepath,
                saveLoc, filename));
            using (var fileStream = fileInfo.OpenWrite())
            {
                await stream.CopyToAsync(fileStream);
            }
        }
    }
}