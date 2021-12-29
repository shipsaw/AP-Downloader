using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using ApDownloader.Model;
using Dapper;
using Microsoft.Data.Sqlite;

namespace ApDownloader.DataAccess;

[SuppressMessage("ReSharper", "RedundantAnonymousTypePropertyName")]
public class SQLiteDataAccess
{
    public async Task<IEnumerable<Product>> GetProductsOnly()
    {
        using IDbConnection conn = new SqliteConnection("Data Source=./ProductsDb.db");
        var products = await conn.QueryAsync<Product>("SELECT * FROM Product", new DynamicParameters());
        return products;
    }

    public async Task<IEnumerable<Product>> GetDownloadedProductsOnly(IEnumerable<string>? productIds)
    {
        if (productIds == null) return new List<Product>();
        using IDbConnection conn = new SqliteConnection("Data Source=./ProductsDb.db");
        var products = await conn.QueryAsync<Product>("SELECT * FROM Product WHERE ProductID IN @productIds",
            new {productIds});
        return products;
    }

    public async Task<IEnumerable<string>> GetExtras(string dbName, IEnumerable<int> productList)
    {
        using IDbConnection conn = new SqliteConnection("Data Source=./ProductsDb.db");
        var products =
            await conn.QueryAsync<string>("SELECT Filename FROM @DbName WHERE ProductID IN @ProductList",
                new {DbName = dbName, ProductList = productList});
        return products;
    }

    public async Task<DownloadManifest> GetDownloadManifest(DownloadOption options,
        IEnumerable<int> productList)
    {
        var manifest = new DownloadManifest();
        using IDbConnection conn = new SqliteConnection("Data Source=./ProductsDb.db");
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

    public async Task<int> GetTotalFileCount(DownloadOption downloadOption, List<int> productIds)
    {
        var manifest = await GetDownloadManifest(downloadOption, productIds);
        return productIds.Count +
               (manifest.EsFilenames?.Count() ?? 0) +
               (manifest.BpFilenames?.Count() ?? 0) +
               (manifest.LpFilenames?.Count() ?? 0);
    }

    public async Task<DownloadOption> GetUserOptions()
    {
        using IDbConnection conn = new SqliteConnection("Data Source=./ProductsDb.db");
        var product =
            await conn.QueryFirstAsync<DownloadOption>("SELECT * FROM Options") ?? new DownloadOption();
        return product;
    }

    public async Task SetUserOptions(DownloadOption? downloadOption)
    {
        using IDbConnection conn = new SqliteConnection("Data Source=./ProductsDb.db");

        await conn.ExecuteAsync(@"UPDATE Options
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
}