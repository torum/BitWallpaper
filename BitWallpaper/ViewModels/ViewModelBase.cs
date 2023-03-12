using System.ComponentModel;
using System.Diagnostics;

namespace BitWallpaper.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged, IDataErrorInfo
{
    #region == INotifyPropertyChanged ==

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void NotifyPropertyChanged(string propertyName)
    {
        //this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        bool? uithread = App.CurrentDispatcherQueue?.HasThreadAccess;

        if (uithread != null)
        {
            if (uithread == true)
            {
                DoNotifyPropertyChanged(propertyName);
            }
            else
            {
                App.CurrentDispatcherQueue?.TryEnqueue(() =>
                {
                    DoNotifyPropertyChanged(propertyName);
                });
            }
        }

        /*
        (App.Current as App)?.TheSynchronizationContext?.Post(d => {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }, null);
        */
    }

    private void DoNotifyPropertyChanged(string propertyName)
    {
        try
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception at NotifyPropertyChanged ({propertyName}): " + ex.Message);

            (App.Current as App)?.AppendErrorLog($"Exception at NotifyPropertyChanged ({propertyName}): ", ex.Message);
        }
    }

    #endregion

    #region == IDataErrorInfo ==

    private readonly Dictionary<string, string> _ErrorMessages = new();

    string IDataErrorInfo.Error => (_ErrorMessages.Count > 0) ? "Has Error" : "";

    string IDataErrorInfo.this[string columnName]
    {
        get
        {
            if (_ErrorMessages.ContainsKey(columnName))
                return _ErrorMessages[columnName];
            else
                return "";
        }
    }

    protected void SetError(string propertyName, string errorMessage)
    {
        _ErrorMessages[propertyName] = errorMessage;
    }

    protected void ClearErrror(string propertyName)
    {
        if (_ErrorMessages.ContainsKey(propertyName))
            //_ErrorMessages.Remove(propertyName);
            _ErrorMessages[propertyName] = "";
    }


    #endregion

}
