using System.IO;
using ApDownloader.DataAccess;
using ApDownloader.UI.Core;

namespace ApDownloader.UI.MVVM.ViewModels;

public class OptionsViewModel : ObservableObject
{
    private readonly SQLiteDataAccess _dataAccess;
    private string _actualDownloadFolderLoc;
    private object _applyResponseVisibility = false;
    private bool _canApply;
    private string _downloadFilepath;
    private bool _getBrandingPatch;
    private bool _getExtraStock;

    private bool _getLiveryPack;
    private string _installFilepath;
    private string _selectedDownloadPath;

    public OptionsViewModel()
    {
        _dataAccess = new SQLiteDataAccess();
        _getExtraStock = MainViewModel.DlOption.GetExtraStock;
        _getBrandingPatch = MainViewModel.DlOption.GetBrandingPatch;
        _getLiveryPack = MainViewModel.DlOption.GetLiveryPack;
        _downloadFilepath = MainViewModel.DlOption.DownloadFilepath;
        _installFilepath = MainViewModel.DlOption.InstallFilePath;

        SetDownloadFilepathCommand = new RelayCommand(path =>
        {
            DownloadFilepath = (string) path;
            CanApply = true;
        });
        SetInstallFilepathCommand = new RelayCommand(path =>
        {
            InstallFilepath = (string) path;
            CanApply = true;
        });
        ApplySettingsCommand = new RelayCommand(clickEvent => ApplySettings());
    }

    public RelayCommand ApplySettingsCommand { get; set; }
    public RelayCommand SetDownloadFilepathCommand { get; set; }
    public RelayCommand SetInstallFilepathCommand { get; set; }

    public bool CanApply
    {
        get => _canApply;
        set
        {
            _canApply = value;
            if (value)
                ApplyResponseVisibility = false;
            OnPropertyChanged();
        }
    }

    public bool GetExtraStock
    {
        get => _getExtraStock;
        set
        {
            _getExtraStock = value;
            OnPropertyChanged();
            CanApply = true;
        }
    }

    public bool GetBrandingPatch
    {
        get => _getBrandingPatch;
        set
        {
            _getBrandingPatch = value;
            OnPropertyChanged();
            CanApply = true;
        }
    }

    public bool GetLiveryPack
    {
        get => _getLiveryPack;
        set
        {
            _getLiveryPack = value;
            OnPropertyChanged();
            CanApply = true;
        }
    }

    public string DownloadFilepath
    {
        get => _downloadFilepath;
        set
        {
            // If selecting existing Downloads folder, use that, else create one
            if (value.EndsWith(@"\ApDownloads"))
                _downloadFilepath = value.Remove(value.LastIndexOf(@"ApDownloads"));
            else if (value.EndsWith('\\'))
                _downloadFilepath = value;
            else
                _downloadFilepath = value + '\\';
            OnPropertyChanged();
            CanApply = true;
        }
    }

    public string InstallFilepath
    {
        get => _installFilepath;
        set
        {
            _installFilepath = value;
            OnPropertyChanged();
            CanApply = true;
        }
    }

    public object ApplyResponseVisibility
    {
        get => _applyResponseVisibility;
        set
        {
            _applyResponseVisibility = value;
            OnPropertyChanged();
        }
    }

    private async void ApplySettings()
    {
        MainViewModel.DlOption.GetExtraStock = GetExtraStock;
        MainViewModel.DlOption.GetBrandingPatch = GetBrandingPatch;
        MainViewModel.DlOption.GetLiveryPack = GetLiveryPack;
        MainViewModel.DlOption.DownloadFilepath = DownloadFilepath;
        MainViewModel.DlOption.InstallFilePath = InstallFilepath;
        Directory.CreateDirectory(MainViewModel.DlOption.DownloadFilepath + @"\ApDownloads\");
        await _dataAccess.SetUserOptions(MainViewModel.DlOption);
        ApplyResponseVisibility = true;
        CanApply = false;
    }
}