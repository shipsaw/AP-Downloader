using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ApDownloader.Model;
using Dapper;
using Microsoft.Data.Sqlite;

namespace ApDownloader.DataAccess;

[SuppressMessage("ReSharper", "RedundantAnonymousTypePropertyName")]
public class SQLiteDataAccess
{
    private readonly string _ProdDbConnectionString;
    private readonly string _SettingsDbConnectionString;

    public SQLiteDataAccess(string appFilepath)
    {
        _ProdDbConnectionString = Path.Combine(appFilepath, Path.GetFileName("./ProductsDb.db"));
        _SettingsDbConnectionString = Path.Combine(appFilepath, Path.GetFileName("./Settings.db"));
    }

    public async Task<IEnumerable<Product>> GetProductsOnly()
    {
        using IDbConnection conn = new SqliteConnection($"Data Source={_ProdDbConnectionString}");
        var products = await conn.QueryAsync<Product>("SELECT * FROM Product", new DynamicParameters());
        return products;
    }

    public async Task<IEnumerable<Product>> GetDownloadedProductsOnly(IEnumerable<string>? productIds)
    {
        if (productIds == null) return new List<Product>();
        using IDbConnection conn = new SqliteConnection($"Data Source={_ProdDbConnectionString}");
        var products = await conn.QueryAsync<Product>("SELECT * FROM Product WHERE ProductID IN @productIds",
            new {productIds});
        return products;
    }

    public async Task<IEnumerable<Product>> GetDownloadedProductsByName(IEnumerable<string> productNames)
    {
        using IDbConnection conn = new SqliteConnection($"Data Source={_ProdDbConnectionString}");
        var products = await conn.QueryAsync<Product>("SELECT * FROM Product WHERE Filename IN @productNames",
            new {productNames});
        return products;
    }

    public async Task<IEnumerable<string>> GetExtras(string dbName, IEnumerable<int> productList)
    {
        using IDbConnection conn = new SqliteConnection($"Data Source={_ProdDbConnectionString}");
        var products =
            await conn.QueryAsync<string>("SELECT Filename FROM @DbName WHERE ProductID IN @ProductList",
                new {DbName = dbName, ProductList = productList});
        return products;
    }


    public async Task<DownloadManifest> GetDownloadManifest(ApDownloaderConfig options,
        IEnumerable<int> productList)
    {
        var manifest = new DownloadManifest();
        using IDbConnection conn = new SqliteConnection($"Data Source={_ProdDbConnectionString}");
        if (options.GetExtraStock)
            manifest.EsFilenames =
                await conn.QueryAsync<string>("SELECT Filename FROM ExtraStock WHERE ProductID IN @ProductList",
                    new {ProductList = productList});
        if (options.GetBrandingPatch)
            manifest.BpFilenames =
                await conn.QueryAsync<string>("SELECT Filename FROM BrandingPatch WHERE ProductID IN @ProductList",
                    new {ProductList = productList});
        if (options.GetLiveryPack)
            manifest.LpFilenames =
                await conn.QueryAsync<string>("SELECT Filename FROM LiveryPack WHERE ProductID IN @ProductList",
                    new {ProductList = productList});
        manifest.PrFilenames =
            await conn.QueryAsync<string>("SELECT Filename FROM Product WHERE ProductID IN @ProductList",
                new {ProductList = productList});
        manifest.ProductIds = productList.Select(p => p.ToString());
        return manifest;
    }

    public async Task<int> GetTotalFileCount(ApDownloaderConfig downloadOption, List<int> productIds)
    {
        var manifest = await GetDownloadManifest(downloadOption, productIds);
        return productIds.Count +
               (manifest.EsFilenames?.Count() ?? 0) +
               (manifest.BpFilenames?.Count() ?? 0) +
               (manifest.LpFilenames?.Count() ?? 0);
    }

    public ApDownloaderConfig GetUserOptions()
    {
        using IDbConnection conn = new SqliteConnection($"Data Source={_SettingsDbConnectionString}");
        var product =
            conn.QueryFirst<ApDownloaderConfig>("SELECT * FROM Settings") ?? new ApDownloaderConfig();
        return product;
    }

    public async Task SetUserOptions(ApDownloaderConfig downloadOption)
    {
        using IDbConnection conn = new SqliteConnection($"Data Source={_SettingsDbConnectionString}");

        await conn.ExecuteAsync(@"UPDATE Settings
                                        SET GetExtraStock = @GetExtraStock,
                                            GetLiveryPack = @GetLiveryPack,
                                            GetBrandingPatch = @GetBrandingPatch,
                                            DownloadFilepath = @DownloadFilepath,
                                            InstallFilepath = @InstallFilepath
                                            WHERE 1 = 1", new
        {
            GetExtraStock = downloadOption.GetExtraStock,
            GetLiveryPack = downloadOption.GetLiveryPack,
            GetBrandingPatch = downloadOption.GetBrandingPatch,
            DownloadFilepath = downloadOption.DownloadFilepath,
            InstallFilepath = downloadOption.InstallFilePath
        });
    }

    public async Task<Dictionary<string, string>> GetFilesFolders()
    {
        var fileSet = new Dictionary<string, string>();
        using IDbConnection conn = new SqliteConnection($"Data Source={_ProdDbConnectionString}");
        var products = await conn.QueryAsync<string>("SELECT Filename FROM Product");
        foreach (var file in products) fileSet.Add(file, "Products");
        var extraStock = await conn.QueryAsync<string>("SELECT Filename FROM ExtraStock");
        foreach (var file in extraStock) fileSet.Add(file, "ExtraStock");
        var brandingPatches = await conn.QueryAsync<string>("SELECT Filename FROM BrandingPatch");
        foreach (var file in brandingPatches) fileSet.Add(file, "BrandingPatches");
        var liveryPacks = await conn.QueryAsync<string>("SELECT Filename FROM LiveryPack");
        foreach (var file in liveryPacks) fileSet.Add(file, "LiveryPacks");
        return fileSet;
    }

    public void ImportProductDb(string filename)
    {
        File.Copy(filename, _ProdDbConnectionString, true);
    }
}