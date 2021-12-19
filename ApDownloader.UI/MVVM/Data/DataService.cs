using System.Collections.Generic;
using ApDownloader.Model;

namespace ApDownloader.UI.MVVM.Data;

public class DataService : IDataService
{
    public IEnumerable<Product> GetAll()
    {
        yield return new Product
        {
            Id = 201,
            Name = "Class 205",
            FileName = "Class205.zip",
            IsAvailable = true
        };
        yield return new Product
        {
            Id = 223,
            Name = "Class 37",
            FileName = "Class37.zip",
            IsAvailable = true
        };
        yield return new Product
        {
            Id = 348,
            Name = "Weather Enhancement Pack",
            FileName = "WeatherEnhance.zip",
            IsAvailable = false
        };
        yield return new Product
        {
            Id = 482,
            Name = "Wherry Lines",
            FileName = "WherryLines.zip",
            IsAvailable = true
        };
    }
}