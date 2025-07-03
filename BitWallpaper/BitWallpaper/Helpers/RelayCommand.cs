using System.Windows.Input;

namespace BitWallpaper.Helpers;

public partial class RelayCommand(Action methodToExecute, Func<bool>? canExecuteEvaluator) : ICommand
{
    private readonly Action methodToExecute = methodToExecute;
    private readonly Func<bool>? canExecuteEvaluator = canExecuteEvaluator;

    public RelayCommand(Action methodToExecute)
        : this(methodToExecute, null)
    {
    }

    /*
    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
    */
    public event EventHandler? CanExecuteChanged;
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool CanExecute(object? parameter)
    {
        if (canExecuteEvaluator == null)
        {
            return true;
        }
        else
        {
            var result = canExecuteEvaluator.Invoke();
            return result;
        }
    }

    public void Execute(object? parameter)
    {
        methodToExecute.Invoke();
    }
}
