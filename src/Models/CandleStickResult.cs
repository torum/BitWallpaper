using System;
using System.Collections.Generic;
using System.Text;

namespace BitWallpaper.Models;

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
        Candlesticks = [];
    }
}
