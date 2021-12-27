﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ApDownloader.Model;
using Dapper;
using Microsoft.Data.Sqlite;

namespace ApDownloader.DataAccess;

public class SQLiteDataAccess
{
    public async Task<IEnumerable<Product>> GetProductsOnly()
    {
        using IDbConnection conn = new SqliteConnection("Data Source=./ProductsDb.db");
        var products = await conn.QueryAsync<Product>("SELECT * FROM Product", new DynamicParameters());
        return products;
    }

    public async Task<IEnumerable<string>> GetExtras(string dbName, IEnumerable<int> productList)
    {
        using IDbConnection conn = new SqliteConnection("Data Source=./ProductsDb.db");
        var products =
            await conn.QueryAsync<string>($"SELECT Filename FROM {dbName} WHERE ProductID IN @productList",
                new {productList});
        return products;
    }

    public async Task<int> GetTotalFileCount(DownloadOption downloadOption, List<int> productIds)
    {
        var dbNames = new List<string>();
        if (downloadOption.GetExtraStock) dbNames.Add("ExtraStock");
        if (downloadOption.GetBrandingPatch) dbNames.Add("BrandingPatch");
        if (downloadOption.GetLiveryPack) dbNames.Add("LiveryPack");
        var total = productIds.Count();
        var tasks = new List<Task<IEnumerable<string>>>();
        foreach (var dbName in dbNames) tasks.Add(GetExtras(dbName, productIds));
        var lists = await Task.WhenAll(tasks);
        return total + lists.SelectMany(x => x).ToList().Count;
    }
}