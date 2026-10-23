using System;
using System.Collections.Generic;
using System.Text;

namespace BitWallpaper.Models;

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
