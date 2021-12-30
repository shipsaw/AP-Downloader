using System.Collections.Generic;
using System.Net.Http;
using System.Security.Principal;
using ApDownloader.DataAccess;
using ApDownloader.Model;
using ApDownloader.UI.Core;

namespace ApDownloader.UI.MVVM.ViewModels;

public class MainViewModel : ObservableObject
{
    private static readonly HttpClientHandler _handler = new() {AllowAutoRedirect = false};
    public static DownloadOption DlOption = new();
    public static DownloadManifest DlManifest = new();
    private readonly HttpClient _client = new(_handler);
    private readonly SQLiteDataAccess _dataAccess;
    private object _currentView;
    private bool _isAdmin;

    public MainViewModel()
    {
        LoginVm = new LoginViewModel();
        DownloadVM = new DownloadViewModel();
        InstallVM = new InstallViewModel();
        OptionsVM = new OptionsViewModel();

        LoginViewCommand = new RelayCommand(clickEvent => CurrentView = LoginVm);
        DownloadViewCommand = new RelayCommand(clickEvent => CurrentView = DownloadVM);
        InstallViewCommand = new RelayCommand(clickEvent => CurrentView = InstallVM);
        OptionsViewCommand = new RelayCommand(clickEvent => CurrentView = OptionsVM);
        CurrentView = LoginVm;

        CheckAdmin();
        _dataAccess = new SQLiteDataAccess();
        DlOption = _dataAccess.GetUserOptions();
    }

    public static IEnumerable<Product> Products { get; set; }

    public RelayCommand LoginViewCommand { get; set; }
    public RelayCommand DownloadViewCommand { get; set; }
    public RelayCommand InstallViewCommand { get; set; }
    public RelayCommand OptionsViewCommand { get; set; }

    public LoginViewModel LoginVm { get; set; }
    public DownloadViewModel DownloadVM { get; set; }
    public InstallViewModel InstallVM { get; set; }
    public OptionsViewModel OptionsVM { get; set; }

    public object CurrentView
    {
        get => _currentView;
        set
        {
            _currentView = value;
            OnPropertyChanged();
        }
    }

    public bool IsAdmin
    {
        get => !_isAdmin;
        set
        {
            _isAdmin = value;
            OnPropertyChanged();
        }
    }

    private void CheckAdmin()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        IsAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}