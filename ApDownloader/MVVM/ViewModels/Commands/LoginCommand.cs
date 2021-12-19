using System;
using System.Windows.Input;

namespace ApDownloader.MVVM.ViewModels.Commands;

public class LoginCommand : ICommand
{
    private readonly Func<bool> _canExecute;
    private readonly Action _execute;


    public LoginCommand(Action execute, Func<bool> canExecute)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute();
    }

    public void Execute(object? parameter)
    {
        _execute.Invoke();
    }

    public event EventHandler? CanExecuteChanged;
}