﻿using System;
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
    private readonly HttpClient _client;

    private const string BrandingPatchPrefix = "https://www.armstrongpowerhouse.com/free_download/Patches/";
    private const string ExtraStockPrefix = "https://www.armstrongpowerhouse.com/free_download/";
    private const string LiveryPrefix = "https://www.armstrongpowerhouse.com/free_download/";
    private const string PreviewImagePrefix = "https://www.armstrongpowerhouse.com/image/cache/catalog/";
    private const string ProductPrefix = "https://www.armstrongpowerhouse.com/index.php?route=account/download/download&download_id=";


    public HttpDataAccess(HttpClient client, int concurrentDownloads = 1)
    {
        _client = client;
        _allTasks = new List<Task>();
        _throttler = new SemaphoreSlim(concurrentDownloads);
    }

    /// <summary>
    /// Checks download headers for Product Information, gets content length
    /// </summary>
    /// <param name="products"> List of all AP products we want to check for purchased and populate with content length info</param>
    /// <returns>Purchased products with website content length filled in</returns>
    /// <exception cref="ErrorCheckingPurchasesException"></exception>
    public async Task<IEnumerable<Product>> GetPurchasedProducts(IEnumerable<Product> products)
    {
        var productsList = products.ToList();
        var retProducts = new List<Product>();
        var tasks = new List<Task<HttpResponseMessage>>();

        try
        {
            foreach (var product in productsList)
            {
                await Task.Delay(30);
                tasks.Add(_client.SendAsync(new HttpRequestMessage(HttpMethod.Head,
                    ProductPrefix +
                    product.ProductID)));
            }

            var httpResponses = await Task.WhenAll(tasks.ToList());

            for (var i = 0; i < productsList.Count; i++)
                if (httpResponses[i].Content.Headers.ContentDisposition != null)
                {
                    productsList[i].CurrentContentLength = httpResponses[i].Content.Headers.ContentLength ?? 0;
                    retProducts.Add(productsList[i]);
                }
        }
        catch (Exception)
        {
            throw new ErrorCheckingPurchasesException();
        }

        return retProducts;
    }

    /// <summary>
    /// Calls all download methods for products, extrastock, brandingpatches, and liverypacks
    /// </summary>
    /// <param name="downloadManifest">file ids to download</param>
    /// <param name="downloadOption"></param>
    /// <param name="progress"></param>
    public async Task Download(DownloadManifest downloadManifest, ApDownloaderConfig downloadOption,
        IProgress<int> progress, IProgress<float> downloadProgress)
    {
        // Get Extra Stock
        if (downloadOption.GetExtraStock)
            await DownloadFile(downloadManifest.EsFilenames, ExtraStockPrefix, progress, downloadProgress, downloadOption, "ExtraStock");

        // Get Branding Patch
        if (downloadOption.GetBrandingPatch)
            await DownloadFile(downloadManifest.BpFilenames, BrandingPatchPrefix, progress, downloadProgress, downloadOption,
                "BrandingPatches");

        // Get Liveries
        if (downloadOption.GetLiveryPack)
            await DownloadFile(downloadManifest.LpFilenames, LiveryPrefix, progress, downloadProgress, downloadOption, "LiveryPacks");

        // Get Base Products
        await DownloadFile(downloadManifest.ProductIds, ProductPrefix, progress, downloadProgress, downloadOption, "Products");

        await Task.WhenAll(_allTasks);
    }

    /// <summary>
    /// Method that sets up and adds to throttler. Actual downloads done in SaveFile which is called by this method
    /// </summary>
    /// <param name="products"></param>
    /// <param name="prefix"></param>
    /// <param name="progress"></param>
    /// <param name="downloadOption"></param>
    /// <param name="saveLoc"></param>
    private async Task DownloadFile(IEnumerable<string> products, string prefix, IProgress<int> progress, IProgress<float> downloadProgress,
        ApDownloaderConfig downloadOption, string saveLoc)
    {
        foreach (var filename in products)
        {
            var uri = new Uri(prefix + filename);
            _allTasks.Add(
                Task.Run(async () =>
                {
                    try
                    {
                        // We don't have filenames for products, only the product ID.  So for product prefix we have
                        // to get this info from the server in the SaveFile call
                        progress.Report(1);
                        await SaveFile(downloadOption, uri, saveLoc,
                            prefix == ProductPrefix ? "" : filename, downloadProgress);
                    }
                    finally
                    {
                        _throttler.Release();
                    }
                }));
            if (_allTasks.Count == 1)
            {
                var completed = await Task.WhenAny(_allTasks);
                _allTasks.Remove(completed);
            }
        }
    }

    public async Task DownloadPreviewImages(IEnumerable<string> productImageNames, string imagesPath)
    {
        Directory.CreateDirectory(imagesPath);
        foreach (var filename in productImageNames)
        {
            var uri = new Uri(PreviewImagePrefix + filename);
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

    private async Task SaveFile(ApDownloaderConfig downloadOption, Uri uri, string saveLoc, string filename, IProgress<float> downloadProgress = null)
    {
        using (var response = await _client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead))
        {
            var contentLength = response.Content.Headers.ContentLength;
            if (filename == "")
            {
                var responseFilename = response.Content.Headers.ContentDisposition?.FileName?.Trim('"');
                if (responseFilename == null) Logger.Log("Unable to get filename for" + uri);
                filename = responseFilename ?? "ERROR:";
            }
            using (var streamToReadFrom = await response.Content.ReadAsStreamAsync())
            {
                var fileToWriteTo = Path.Combine(downloadOption.DownloadFilepath, saveLoc, filename);
                
                using (Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create))
                {
                    if (contentLength == null)
                    {
                        await streamToReadFrom.CopyToAsync(streamToWriteTo, 81920);
                    }

                    var relativeProgress = new Progress<long>(totalBytes => downloadProgress.Report((float)totalBytes / contentLength.Value));
                    await streamToReadFrom.CopyToAsync(streamToWriteTo, 81920, relativeProgress);
                    downloadProgress.Report(1);
                }
            }
        }
    }

    private async Task SavePreviewImage(Uri uri, string saveLoc, string filename)
    {
        using (var response = await _client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead))
        {
            if (filename == "")
            {
                var responseFilename = response.Content.Headers.ContentDisposition?.FileName?.Trim('"');
                if (responseFilename == null) Logger.Log("Unable to get filename for" + uri);
                filename = responseFilename ?? "ERROR:";
            }
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
    
    public async Task UpdateProductsDb(string dbUri)
    {
        var fileToWriteTo = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "ApDownloader\\ProductsDb.db");
        using (var response = await _client.GetAsync(dbUri))
        {
            using (var streamToReadFrom = await response.Content.ReadAsStreamAsync())
            {
                using (Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create))
                {
                    await streamToReadFrom.CopyToAsync(streamToWriteTo);
                }
            }
        }
    }
}