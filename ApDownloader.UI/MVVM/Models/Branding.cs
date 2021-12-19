using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ApDownloader.UI.MVVM.Models;

public class Branding : INotifyPropertyChanged
{
    public Branding(int addonId, string filename)
    {
        AddonId = addonId;
        Filename = filename;
    }

    public int AddonId { get; set; }
    public string Filename { get; set; }
    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}