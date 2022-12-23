using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace BitWallpaper4.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged, IDataErrorInfo
{

    readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

    public ViewModelBase()
    {
    }

    #region == INotifyPropertyChanged ==

    public event PropertyChangedEventHandler PropertyChanged;

    protected void NotifyPropertyChanged(string propertyName)
    {
        //this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        _dispatcherQueue.TryEnqueue(() =>
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        });
    }

    #endregion

    #region == IDataErrorInfo ==

    private Dictionary<string, string> _ErrorMessages = new Dictionary<string, string>();

    string IDataErrorInfo.Error
    {
        get
        {
            return (_ErrorMessages.Count > 0) ? "Has Error" : null;
        }
    }

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
