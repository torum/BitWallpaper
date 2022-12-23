using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitWallpaper4.Models;

public class Ticker
{
    private decimal _ltp;
    public decimal LTP
    {
        get
        {
            return _ltp;
        }
        set
        {
            if (_ltp == value)
                return;

            _ltp = value;

        }
    }

    private DateTime _timestamp;
    public DateTime TimeStamp
    {
        get
        {
            return _timestamp;
        }
        set
        {
            if (_timestamp == value)
                return;

            _timestamp = value;

        }
    }

    private decimal _bid;
    public decimal Bid
    {
        get
        {
            return _bid;
        }
        set
        {
            if (_bid == value)
                return;

            _bid = value;

        }
    }

    private decimal _ask;
    public decimal Ask
    {
        get
        {
            return _ask;
        }
        set
        {
            if (_ask == value)
                return;

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

