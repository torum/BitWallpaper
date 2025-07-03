namespace BitApps.Core.Models;

public class TickHistory
{
    public decimal Price
    {
        get; set;
    }

    public string PriceString => string.Format("{0:#,0}", Price);

    public DateTime TimeAt
    {
        get; set;
    }

    public string TimeStamp => TimeAt.ToLocalTime().ToString("HH:mm:ss");

    public bool TickHistoryPriceUp
    {
        get; set;
    }
}
