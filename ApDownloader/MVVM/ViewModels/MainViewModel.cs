using ApDownloader.Core;

namespace ApDownloader.MVVM.ViewModels;

public class MainViewModel : ObservableObject
{
    private object _currentView;

    public MainViewModel()
    {
        LoginVm = new LoginViewModel();
        DownloadVM = new DownloadViewModel();

        LoginViewCommand = new RelayCommand(clickEvent => CurrentView = LoginVm);
        DownloadViewCommand = new RelayCommand(clickEvent => CurrentView = DownloadVM);
        CurrentView = LoginVm;
    }

    public RelayCommand LoginViewCommand { get; set; }
    public RelayCommand DownloadViewCommand { get; set; }

    public LoginViewModel LoginVm { get; set; }
    public DownloadViewModel DownloadVM { get; set; }

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