using ApDownloader.Model;
using Microsoft.EntityFrameworkCore;

namespace ApDownloader.DataAccess;

public class ApDownloaderDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=ApDownloader;Integrated Security=True;");
    }
}