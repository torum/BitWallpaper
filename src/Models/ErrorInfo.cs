using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace BitWallpaper.Models;

public enum ErrType
{
    ConnectionError, HttpError, ResposeParseError, UserInputInvalid, Unexpected, Other
};

public partial class ErrorInfo(ErrType errType) : ObservableObject
{
    private Dictionary<ErrType, string> ErrTypeDictionary
    {
        get;
    } = new Dictionary<ErrType, string>()
        {
            {ErrType.ConnectionError, "Connection Error"},
            {ErrType.HttpError, "Http Error"},
            {ErrType.ResposeParseError, "Respose Parse Error"},
            {ErrType.UserInputInvalid, "Invalid Input Value"},
            {ErrType.Unexpected, "Unexpected Error"},
            {ErrType.Other, "Error"},
        };

    public ErrType ErrorType => errType;

    public string ErrorTitle
    {
        get => ErrTypeDictionary[errType];
    }

    public int ErrorCode
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public InfoBarSeverity ErrorSeverity
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    } = InfoBarSeverity.Warning;

    public string ErrorMessage
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

}

