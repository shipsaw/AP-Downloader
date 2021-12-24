using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ApDownloader.Model;
using Dapper;
using Microsoft.Data.Sqlite;

namespace ApDownloader.DataAccess;

public class SQLiteDataAccess
{
    public async Task<IEnumerable<Product>> GetProductsOnly()
    {
        using (IDbConnection conn = new SqliteConnection("Data Source=./ProductsDb.db"))
        {
            var products = await conn.QueryAsync<Product>("SELECT * FROM Product", new DynamicParameters());
            return products;
        }
    }
}