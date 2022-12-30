using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BitWallpaper.Models;

public enum CandleTypes
{
    OneMin, FiveMin, FifteenMin, ThirtyMin, OneHour, FourHour, EightHour, TwelveHour, OneDay, OneWeek, OneMonth

    // 1min 5min 15min 30min 1hour 4hour 8hour 12hour 1day 1week 1month
}

public class Ohlcv
{
    // [始値, 高値, 安値, 終値, 出来高, UnixTime]

    public decimal Open;
    public decimal High;
    public decimal Low;
    public decimal Close;
    public decimal Volume;
    public DateTime TimeStamp;

}
/*
public class CandlestickTemp
{
    public CandleTypes CandleType { get; set; }

    //public CandleSpanFormats CandleSpanFormat { get; set; }

    public List<Ohlcv> Ohlcvs;

    public CandlestickTemp()
    {
        /
        CandleSpanFormat = span;

        if (span == CandleSpanFormats.yealy)
        {
            if ((ct == CandleTypes.OneMin) || (ct == CandleTypes.FiveMin) || (ct == CandleTypes.FifteenMin) || (ct == CandleTypes.ThirtyMin) || (ct == CandleTypes.OneHour))
            {
                throw new ArgumentException("Not supported.");
            }
        }
        else if (span == CandleSpanFormats.daily)
        {
            if ((ct == CandleTypes.FourHour) || (ct == CandleTypes.EightHour) || (ct == CandleTypes.TwelveHour) || (ct == CandleTypes.OneDay) || (ct == CandleTypes.OneWeek) || (ct == CandleTypes.OneMonth))
            {
                throw new ArgumentException("Not supported.");
            }
        }
        CandleType = ct;
        /

        Ohlcvs = new List<Ohlcv>();
    }
}
*/

public class CandlestickResult
{
    public bool IsSuccess
    {
        get; set;
    }
    public int ErrorCode
    {
        get; set;
    }

    public CandleTypes CandleType { get; set; }

    public List<Ohlcv> Candlesticks;

    public CandlestickResult()
    {
        Candlesticks = new List<Ohlcv>();
    }
}

