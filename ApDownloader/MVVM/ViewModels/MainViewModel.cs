using ApDownloader.Core;

namespace ApDownloader.MVVM.ViewModels;

public class MainViewModel : ObservableObject
{
    private object _currentView;

    public MainViewModel()
    {
        LoginVm = new LoginViewModel();
        CurrentView = LoginVm;
    }

    public LoginViewModel LoginVm { get; set; }

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