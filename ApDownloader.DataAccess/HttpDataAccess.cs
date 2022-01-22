using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApDownloader.Model;
using ApDownloader.Model.Exceptions;

namespace ApDownloader.DataAccess;

public class HttpDataAccess
{
    private readonly List<Task> _allTasks;
    private readonly SemaphoreSlim _throttler;
    private readonly HttpClient? _client;

    private readonly string _brandingPatchPrefix = "https://www.armstrongpowerhouse.com/free_download/Patches/";
    private readonly string _extraStockPrefix = "https://www.armstrongpowerhouse.com/free_download/";
    private readonly string _liveryPrefix = "https://www.armstrongpowerhouse.com/free_download/";
    private readonly string _previewImagePrefix = "https://www.armstrongpowerhouse.com/image/cache/catalog/";
    private readonly string _productPrefix = "https://www.armstrongpowerhouse.com/index.php?route=account/download/download&download_id=";


    public HttpDataAccess(HttpClient? client, int concurrentDownloads = 1)
    {
        _client = client;
        _allTasks = new List<Task>();
        _throttler = new SemaphoreSlim(concurrentDownloads);
    }

    /// <summary>
    ///     Checkes download headers for Product Information, gets content length
    /// </summary>
    /// <param name="products"> List of all AP products we want to check for purchased and populate with content length info</param>
    /// <returns>Purchased products with website content length filled in</returns>
    /// <exception cref="ErrorCheckingPurchasesException"></exception>
    public async Task<IEnumerable<Product>> GetPurchasedProducts(IEnumerable<Product> products)
    {
        var productsList = new List<Product>(products);
        var retProducts = new List<Product>();
        var tasks = new List<Task<HttpResponseMessage>>();

        foreach (var product in products)
        {
            await Task.Delay(30);
            tasks.Add(_client.SendAsync(new HttpRequestMessage(HttpMethod.Head,
                _productPrefix +
                product.ProductID)));
        }

        try
        {
            var httpResponses = await Task.WhenAll(tasks.ToList());

            for (var i = 0; i < products.Count(); i++)
                if (httpResponses[i].Content.Headers.ContentDisposition != null)
                {
                    productsList[i].CurrentContentLength = httpResponses[i].Content.Headers.ContentLength ?? 0;
                    retProducts.Add(productsList[i]);
                }
        }
        catch (Exception e)
        {
            throw new ErrorCheckingPurchasesException();
        }

        return retProducts;
    }

    /// <summary>
    ///     Calls all download methods for products, extrastock, brandingpatches, and liverypacks
    /// </summary>
    /// <param name="downloadManifest">file ids to download</param>
    /// <param name="downloadOption"></param>
    /// <param name="progress"></param>
    public async Task Download(DownloadManifest downloadManifest, ApDownloaderConfig downloadOption,
        IProgress<int> progress)
    {
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

        // Get Base Products
        await DownloadFile(downloadManifest.ProductIds, _productPrefix, progress, downloadOption, "Products");

        await Task.WhenAll(_allTasks);
    }

    private async Task DownloadFile(IEnumerable<string> products, string prefix, IProgress<int> progress,
        ApDownloaderConfig downloadOption, string saveLoc)
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
                        await SaveFile(downloadOption, uri, saveLoc,
                            prefix == _productPrefix ? "" : filename);
                    }
                    finally
                    {
                        _throttler.Release();
                    }
                }));
        }
    }

    public async Task DownloadPreviewImages(IEnumerable<string> productImageNames, string imagesPath)
    {
        Directory.CreateDirectory(imagesPath);
        foreach (var filename in productImageNames)
        {
            var uri = new Uri(_previewImagePrefix + filename);
            await _throttler.WaitAsync();
            await Task.Delay(100);
            _allTasks.Add(
                Task.Run(async () =>
                {
                    try
                    {
                        await SavePreviewImage(uri, imagesPath, filename);
                    }
                    finally
                    {
                        _throttler.Release();
                    }
                }));
        }
    }

    private async Task SaveFile(ApDownloaderConfig downloadOption, Uri uri, string saveLoc, string filename)
    {
        using (var response = await _client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead))
        {
            if (filename == "") filename = response.Content.Headers.ContentDisposition.FileName.Trim('"');
            using (var streamToReadFrom = await response.Content.ReadAsStreamAsync())
            {
                var fileToWriteTo = Path.Combine(downloadOption.DownloadFilepath, saveLoc, filename);
                using (Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create))
                {
                    await streamToReadFrom.CopyToAsync(streamToWriteTo);
                }
            }
        }
    }

    private async Task SavePreviewImage(Uri uri, string saveLoc, string filename)
    {
        using (var response = await _client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead))
        {
            if (filename == "") filename = response.Content.Headers.ContentDisposition.FileName.Trim('"');
            using (var streamToReadFrom = await response.Content.ReadAsStreamAsync())
            {
                var fileToWriteTo = Path.Combine(saveLoc, filename);
                using (Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create))
                {
                    await streamToReadFrom.CopyToAsync(streamToWriteTo);
                }
            }
        }
    }
}