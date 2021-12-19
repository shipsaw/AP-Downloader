using System.Collections.Generic;
using ApDownloader.Model;

namespace ApDownloader.UI.MVVM.Data;

public interface IDataService
{
    IEnumerable<Product> GetAll();
}