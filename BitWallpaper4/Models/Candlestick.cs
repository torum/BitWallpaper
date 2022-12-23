using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitWallpaper4.Models;

public enum CandleTypes
{
    OneMin, FiveMin, FifteenMin, ThirteenMin, OneHour, FourHour, EightHour, TwelveHour, OneDay, OneWeek

    // 1min 5min 15min 30min 1hour 4hour 8hour 12hour 1day 1week
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

    public Ohlcv()
    {

    }

}

public class Candlestick
{
    public CandleTypes Type
    {
        get; set;
    }

    public List<Ohlcv> Ohlcvs;

    public Candlestick()
    {
        Ohlcvs = new List<Ohlcv>();
    }
}

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

    public List<Candlestick> Candlesticks;

    public CandlestickResult()
    {
        Candlesticks = new List<Candlestick>();
    }
}

