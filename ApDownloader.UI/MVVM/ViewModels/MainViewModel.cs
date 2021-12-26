using ApDownloader.UI.Core;

namespace ApDownloader.UI.MVVM.ViewModels;

public class MainViewModel : ObservableObject
{
    private object _currentView;

    public MainViewModel()
    {
        LoginVm = new LoginViewModel();
        DownloadVM = new DownloadViewModel();
        OptionsVM = new OptionsViewModel();

        LoginViewCommand = new RelayCommand(clickEvent => CurrentView = LoginVm);
        DownloadViewCommand = new RelayCommand(clickEvent => CurrentView = DownloadVM);
        OptionsViewCommand = new RelayCommand(clickEvent => CurrentView = OptionsVM);
        CurrentView = LoginVm;
    }

    public RelayCommand LoginViewCommand { get; set; }
    public RelayCommand DownloadViewCommand { get; set; }
    public RelayCommand OptionsViewCommand { get; set; }

    public LoginViewModel LoginVm { get; set; }
    public DownloadViewModel DownloadVM { get; set; }
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