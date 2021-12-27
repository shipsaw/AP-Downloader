using ApDownloader.UI.Core;

namespace ApDownloader.UI.MVVM.ViewModels;

public class MainViewModel : ObservableObject
{
    private object _currentView;

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
    }

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
}