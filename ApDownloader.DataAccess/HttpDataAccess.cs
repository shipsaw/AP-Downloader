using System;
using System.Collections;
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

    private readonly string _extraStockPrefix = "https://armstrongpowerhouse.com/free_download/";

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
            tasks.Add(_client.SendAsync(new HttpRequestMessage(HttpMethod.Head,
                "https://www.armstrongpowerhouse.com/index.php?route=account/download/download&download_id=" +
                product.ProductID)));

        var result = (await Task.WhenAll(tasks)).ToList();
        if (result.Count() != products.Count()) throw new Exception("Error Checking purchases");

        for (var i = 0; i < products.Count(); i++)
            if (result[i].Content.Headers.ContentDisposition != null)
                retProducts.Add(productsList[i]);

        return retProducts;
    }

    public async Task Download(IList products, bool getStock, bool getPatch, bool getLivery, IProgress<int> progress)
    {
        var productIds = new List<int>();
        var allTasks = new List<Task>();
        var throttler = new SemaphoreSlim(3);
        var lockTarget = new object();
        foreach (Cell cell in products)
            if (cell.ProductID != null)
                productIds.Add(cell.ProductID.Value);

        var dbAccess = new SQLiteDataAccess();
        var extraStockUrls = await dbAccess.GetExtras("ExtraStock", productIds);
        var brandingPatchUrls = await dbAccess.GetExtras("BrandingPatch", productIds);
        var liveryUrls = await dbAccess.GetExtras("LiveryPack", productIds);
        // Get Base Products
        foreach (var id in productIds)
        {
            await throttler.WaitAsync();
            allTasks.Add(
                Task.Run(async () =>
                {
                    try
                    {
                        progress.Report(1);
                        var response = await _client.GetAsync(_productPrefix + id);
                        await using var stream = await response.Content.ReadAsStreamAsync();
                        var filename = response.Content.Headers.ContentDisposition.FileName.Trim('"');
                        var fileInfo = new FileInfo("Downloads/Products/" + filename);
                        await using var fileStream = fileInfo.OpenWrite();
                        await stream.CopyToAsync(fileStream);
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }));
        }

        // Get Extra Stock
        foreach (var filename in extraStockUrls)
        {
            await throttler.WaitAsync();
            allTasks.Add(
                Task.Run(async () =>
                {
                    try
                    {
                        progress.Report(1);
                        var response = await _client.GetAsync(_extraStockPrefix + filename);
                        await using var stream = await response.Content.ReadAsStreamAsync();
                        var fileInfo = new FileInfo("Downloads/ExtraStock/" + filename);
                        await using var fileStream = fileInfo.OpenWrite();
                        await stream.CopyToAsync(fileStream);
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }));
        }

        // Get Branding Patch
        foreach (var filename in brandingPatchUrls)
        {
            await throttler.WaitAsync();
            allTasks.Add(
                Task.Run(async () =>
                {
                    try
                    {
                        progress.Report(1);
                        var response = await _client.GetAsync(_brandingPatchPrefix + filename);
                        await using var stream = await response.Content.ReadAsStreamAsync();
                        var fileInfo = new FileInfo("Downloads/BrandingPatch/" + filename);
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
        foreach (var filename in liveryUrls)
        {
            await throttler.WaitAsync();
            allTasks.Add(
                Task.Run(async () =>
                {
                    try
                    {
                        progress.Report(1);
                        var response = await _client.GetAsync(_productPrefix + filename);
                        await using var stream = await response.Content.ReadAsStreamAsync();
                        var fileInfo = new FileInfo("Downloads/LiveryPack/" + filename);
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
}