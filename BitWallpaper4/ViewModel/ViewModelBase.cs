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
    public ViewModelBase()
    {
    }

    #region == INotifyPropertyChanged ==

    public event PropertyChangedEventHandler PropertyChanged;

    protected void NotifyPropertyChanged(string propertyName)
    {
        //this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return;
        (App.Current as App)?.CurrentDispatcherQueue.TryEnqueue(() =>
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
