namespace BitApps.Core.Models;

public class Ticker
{
    private decimal _ltp;
    public decimal LTP
    {
        get => _ltp;
        set
        {
            if (_ltp == value)
            {
                return;
            }

            _ltp = value;
        }
    }

    private DateTime _timestamp;
    public DateTime TimeStamp
    {
        get => _timestamp;
        set
        {
            if (_timestamp == value)
            {
                return;
            }

            _timestamp = value;
        }
    }

    private decimal _bid;
    public decimal Bid
    {
        get => _bid;
        set
        {
            if (_bid == value)
            {
                return;
            }

            _bid = value;
        }
    }

    private decimal _ask;
    public decimal Ask
    {
        get => _ask;
        set
        {
            if (_ask == value)
            {
                return;
            }

            _ask = value;
        }
    }

    //private decimal _high;
    public decimal High
    {
        get; set;
    }

    //private decimal _low;
    public decimal Low
    {
        get; set;
    }

    public Ticker()
    {

    }

}

