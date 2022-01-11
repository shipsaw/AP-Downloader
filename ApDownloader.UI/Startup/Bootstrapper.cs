using ApDownloader.UI.MVVM.Views;
using Autofac;

namespace ApDownloader.UI.Startup;

public class Bootstrapper
{
    public IContainer Bootstrap()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<DownloadView>().AsSelf();
        builder.RegisterType<MainWindow>().AsSelf();
        return builder.Build();
    }
}