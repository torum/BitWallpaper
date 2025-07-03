
namespace BitApps.Core.Models.APIClients;

public class ClientError
{
    public string? ErrType
    {
        get; set;
    }
    public int ErrCode
    {
        get; set;
    }
    public string? ErrText
    {
        get; set;
    }
    public string? ErrPlace
    {
        get; set;
    }
    public string? ErrPlaceParent
    {
        get; set;
    }
    public DateTime ErrDatetime
    {
        get; set;
    }
    public string? ErrEx
    {
        get; set;
    }

    public ClientError()
    {

    }
}

