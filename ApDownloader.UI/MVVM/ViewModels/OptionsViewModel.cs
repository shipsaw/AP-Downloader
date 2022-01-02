using System.IO;
using System.Linq;
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
    private bool _isInstallFolderInvalid;
    private string _selectedDownloadPath;

    public OptionsViewModel(bool isInstallFolderValid)
    {
        IsInstallFolderInValid = !isInstallFolderValid;
        _dataAccess = new SQLiteDataAccess(MainViewModel.Config["DbConnectionString"]);
        _getExtraStock = MainViewModel.DlOption.GetExtraStock;
        _getBrandingPatch = MainViewModel.DlOption.GetBrandingPatch;
        _getLiveryPack = MainViewModel.DlOption.GetLiveryPack;
        _downloadFilepath = MainViewModel.DlOption.DownloadFilepath;
        _installFilepath = MainViewModel.DlOption.InstallFilePath;

        OrganizeDownloadFolderCommand = new RelayCommand(clickEvent => OrganizeDownloadFolder());
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
        ApplySettingsCommand = new RelayCommand(clickEvent => ApplySettings(), clickEvent => CanApply);
    }

    public bool IsInstallFolderInValid
    {
        get => _isInstallFolderInvalid;
        set
        {
            _isInstallFolderInvalid = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand ApplySettingsCommand { get; set; }
    public RelayCommand SetDownloadFilepathCommand { get; set; }
    public RelayCommand SetInstallFilepathCommand { get; set; }
    public RelayCommand OrganizeDownloadFolderCommand { get; set; }

    public bool CanApply
    {
        get => _canApply && !IsInstallFolderInValid;
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
            _downloadFilepath = value;
            OnPropertyChanged();
            CanApply = true;
            MainViewModel.IsDownloadDataDirty = true;
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

    private async void OrganizeDownloadFolder()
    {
        if (!Directory.Exists(Path.Combine(MainViewModel.DlOption.DownloadFilepath))) return;
        var productsSet = await _dataAccess.GetFilesFolders();

        Directory.CreateDirectory(MainViewModel.DlOption.DownloadFilepath + @"\Products");
        Directory.CreateDirectory(MainViewModel.DlOption.DownloadFilepath + @"\ExtraStock");
        Directory.CreateDirectory(MainViewModel.DlOption.DownloadFilepath + @"\BrandingPatches");
        Directory.CreateDirectory(MainViewModel.DlOption.DownloadFilepath + @"\LiveryPacks");

        var allFiles = Directory
            .EnumerateFiles(Path.Combine(MainViewModel.DlOption.DownloadFilepath), "*.zip",
                SearchOption.TopDirectoryOnly)
            .Select(file => new FileInfo(file).Name);
        foreach (var filename in allFiles)
            File.Move(Path.Combine(MainViewModel.DlOption.DownloadFilepath, filename),
                Path.Combine(MainViewModel.DlOption.DownloadFilepath, productsSet[filename], filename));
    }

    private async void ApplySettings()
    {
        MainViewModel.DlOption.GetExtraStock = GetExtraStock;
        MainViewModel.DlOption.GetBrandingPatch = GetBrandingPatch;
        MainViewModel.DlOption.GetLiveryPack = GetLiveryPack;
        MainViewModel.DlOption.DownloadFilepath = DownloadFilepath;
        MainViewModel.DlOption.InstallFilePath = InstallFilepath;
        Directory.CreateDirectory(MainViewModel.DlOption.DownloadFilepath + @"\Products");
        Directory.CreateDirectory(MainViewModel.DlOption.DownloadFilepath + @"\ExtraStock");
        Directory.CreateDirectory(MainViewModel.DlOption.DownloadFilepath + @"\BrandingPatches");
        Directory.CreateDirectory(MainViewModel.DlOption.DownloadFilepath + @"\LiveryPacks");
        await _dataAccess.SetUserOptions(MainViewModel.DlOption);
        ApplyResponseVisibility = true;
        CanApply = false;
    }
}