using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ApDownloader.UI.MVVM.Models;

public class Addon : INotifyPropertyChanged
{
    public Addon(string name, string filename, string fileNum, DateTime updatedDate)
    {
        Name = name;
        Filename = filename;
        FileNum = fileNum;
        UpdatedDate = updatedDate;
    }

    public int AddonId { get; set; }
    public string Name { get; set; }
    public string Filename { get; set; }
    public string FileNum { get; set; }
    public DateTime UpdatedDate { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}