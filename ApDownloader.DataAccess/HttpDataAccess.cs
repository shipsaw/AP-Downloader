using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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

    public async void Download()
    {
    }
}