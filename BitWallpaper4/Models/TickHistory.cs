using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitWallpaper4.Models;

public class TickHistory
{
    // 価格
    public decimal Price
    {
        get; set;
    }
    public String PriceString
    {
        get
        {
            return String.Format("{0:#,0}", Price);
        }
    }

    // 日時
    public DateTime TimeAt
    {
        get; set;
    }
    // タイムスタンプ文字列
    public string TimeStamp
    {
        get
        {
            return TimeAt.ToLocalTime().ToString("HH:mm:ss");
        }
    }

    //public Color TickHistoryPriceColor { get; set; }

    public bool TickHistoryPriceUp
    {
        get; set;
    }

    public TickHistory()
    {

    }
}
