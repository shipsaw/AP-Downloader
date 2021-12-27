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
    private readonly string _brandingPatchPrefix = "https://armstrongpowerhouse.com/free_download/Patches/";
    private readonly HttpClient? _client;

    private readonly string _extraStockPrefix = "https://www.armstrongpowerhouse.com/free_download/";

    private readonly string _liveryPrefix = "https://armstrongpowerhouse.com/free_download/";

    private readonly string _productPrefix =
        "https://www.armstrongpowerhouse.com/index.php?route=account/download/download&download_id=";

    public HttpDataAccess(HttpClient? client)
    {
        _client = client;
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
                retProducts.Add(productsList[i]);
            else
                Console.WriteLine("OOPS");

        return retProducts;
    }

    public async Task Download(DownloadManifest downloadManifest, DownloadOption downloadOption,
        IProgress<int> progress)
    {
        var allTasks = new List<Task>();
        var throttler = new SemaphoreSlim(3);

        // Create Download Directories
        var result = Directory.CreateDirectory(downloadOption.DownloadFilepath + @"\ApDownloads\");
        Directory.CreateDirectory(downloadOption.DownloadFilepath + @"\ApDownloads\Products\");
        Directory.CreateDirectory(downloadOption.DownloadFilepath + @"\ApDownloads\ExtraStock\");
        Directory.CreateDirectory(downloadOption.DownloadFilepath + @"\ApDownloads\BrandingPatches\");
        Directory.CreateDirectory(downloadOption.DownloadFilepath + @"\ApDownloads\LiveryPacks\");
        // Get Base Products
        foreach (var id in downloadManifest.ProductIds)
        {
            await throttler.WaitAsync();
            await Task.Delay(100);
            allTasks.Add(
                Task.Run(async () =>
                {
                    try
                    {
                        progress.Report(1);
                        var uri = new Uri(_productPrefix + id);
                        await SaveFile(downloadOption, uri, "ApDownloads/Products");
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }));
        }

        // Get Extra Stock
        if (downloadOption.GetExtraStock)
            foreach (var filename in downloadManifest.EsFilenames)
            {
                await throttler.WaitAsync();
                await Task.Delay(100);
                allTasks.Add(
                    Task.Run(async () =>
                    {
                        try
                        {
                            progress.Report(1);
                            var uri = new Uri(_extraStockPrefix + filename);
                            await SaveFile(downloadOption, uri, "ApDownloads/ExtraStock/", filename);
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    }));
            }

        // Get Branding Patch
        if (downloadOption.GetBrandingPatch)
            foreach (var filename in downloadManifest.BpFilenames)
            {
                await throttler.WaitAsync();
                await Task.Delay(100);
                allTasks.Add(
                    Task.Run(async () =>
                    {
                        try
                        {
                            progress.Report(1);
                            var response = await _client.GetAsync(_brandingPatchPrefix + filename);
                            await using var stream = await response.Content.ReadAsStreamAsync();
                            var fileInfo = new FileInfo(Path.Combine(downloadOption.DownloadFilepath,
                                "ApDownloads/BrandingPatches/", filename));
                            await using var fileStream = fileInfo.OpenWrite();
                            await stream.CopyToAsync(fileStream);
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    }));
            }

        // Get Liveries
        if (downloadOption.GetLiveryPack)
            foreach (var filename in downloadManifest.LpFilenames)
            {
                await throttler.WaitAsync();
                await Task.Delay(100);
                allTasks.Add(
                    Task.Run(async () =>
                    {
                        try
                        {
                            progress.Report(1);
                            var response = await _client.GetAsync(_productPrefix + filename);
                            await using var stream = await response.Content.ReadAsStreamAsync();
                            var fileInfo = new FileInfo(Path.Combine(downloadOption.DownloadFilepath,
                                "ApDownloads/LiveryPacks/",
                                filename));
                            await using var fileStream = fileInfo.OpenWrite();
                            await stream.CopyToAsync(fileStream);
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    }));
            }

        await Task.WhenAll(allTasks);
    }

    public async Task SaveFile(DownloadOption downloadOption, Uri uri, string saveLoc, string filename = "")
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