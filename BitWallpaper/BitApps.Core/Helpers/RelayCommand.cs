using System.Windows.Input;

namespace BitApps.Core.Helpers;

public partial class GenericRelayCommand<T>(Action<T> execute, Predicate<T> canExecuteFunc) : ICommand
{
    private readonly Action<T> execute = execute;

    public GenericRelayCommand(Action<T> execute) : this(execute, p => true)
    {
    }

    public event EventHandler? CanExecuteChanged;
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }


    public Predicate<T> CanExecuteFunc
    {
        get; private set;
    } = canExecuteFunc;

    public bool CanExecute(object? parameter)
    {
        if (parameter != null)
        {
            var canExecute = CanExecuteFunc((T)parameter);
            return canExecute;
        }
        else
        {
            return false;
        }
    }

    public void Execute(object? parameter)
    {
        if (parameter is not null)
        {
            execute((T)parameter);
        }
    }
}

