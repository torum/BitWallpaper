using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitWallpaper4.ViewModels;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI.Xaml.Data;

namespace BitWallpaper4.Models;

public enum Pairs
{
    btc_jpy, xrp_jpy, eth_jpy, ltc_jpy, bcc_jpy, mona_jpy, xlm_jpy, qtum_jpy, bat_jpy
}

public class Pair : ViewModelBase
{

    // 通貨フォーマット用
    private string _ltpFormstString = "{0:#,0}";
    // 通貨ペア
    private Pairs _p;
    public Pairs ThisPair
    {
        get
        {
            return _p;
        }
    }

    // 表示用 通貨ペア名 "BTC/JPY";
    public string PairString
    {
        get
        {
            return PairStrings[_p];
        }
    }

    public Dictionary<Pairs, string> PairStrings
    {
        get; set;
    } = new Dictionary<Pairs, string>()
    {
                {Pairs.btc_jpy, "BTC/JPY"},
                {Pairs.xrp_jpy, "XRP/JPY"},
                {Pairs.eth_jpy, "ETH/JPY"},
                {Pairs.ltc_jpy, "LTC/JPY"},
                {Pairs.mona_jpy, "MONA/JPY"},
                {Pairs.bcc_jpy, "BCH/JPY"},
                {Pairs.xlm_jpy, "XLM/JPY"},
                {Pairs.qtum_jpy, "QTUM/JPY"},
                {Pairs.bat_jpy, "BAT/JPY"},
            };

    // 最終取引価格
    private decimal _ltp;
    public decimal Ltp
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
            this.NotifyPropertyChanged("Ltp");
            this.NotifyPropertyChanged("LtpString");

            if (_ltp > BasePrice)
            {
                BasePriceUpFlag = true;
            }
            else if (_ltp < BasePrice)
            {
                BasePriceUpFlag = false;
            }
            this.NotifyPropertyChanged("BasePriceIcon");

            if (_ltp > MiddleInitPrice)
            {
                MiddleInitPriceUpFlag = true;
            }
            else if (_ltp < MiddleInitPrice)
            {
                MiddleInitPriceUpFlag = false;
            }
            this.NotifyPropertyChanged("MiddleInitPriceIcon");

            if (_ltp > MiddleLast24Price)
            {
                MiddleLast24PriceUpFlag = true;
            }
            else if (_ltp < MiddleLast24Price)
            {
                MiddleLast24PriceUpFlag = false;
            }
            this.NotifyPropertyChanged("MiddleLast24PriceIcon");

            if (_ltp > AveragePrice)
            {
                AveragePriceUpFlag = true;
            }
            else if (_ltp < AveragePrice)
            {
                AveragePriceUpFlag = false;
            }
            this.NotifyPropertyChanged("AveragePriceIcon");

        }
    }

    public string LtpString
    {
        get
        {
            if (_ltp == 0)
            {
                return "";
            }
            else
            {
                return String.Format(_ltpFormstString, _ltp);

            }
        }
    }

    private bool _ltpUpFlag;
    public bool LtpUpFlag
    {
        get
        {
            return _ltpUpFlag;
        }
        set
        {
            if (_ltpUpFlag == value)
                return;

            _ltpUpFlag = value;
            this.NotifyPropertyChanged("LtpUpFlag");
        }
    }

    private double _ltpFontSize = 28;
    public double LtpFontSize
    {
        get
        {
            return _ltpFontSize;
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
            this.NotifyPropertyChanged("Bid");
            this.NotifyPropertyChanged("BidString");

        }
    }
    public string BidString
    {
        get
        {
            return String.Format("{0:#,0}", _bid);
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
            this.NotifyPropertyChanged("Ask");
            this.NotifyPropertyChanged("AskString");

        }
    }
    public string AskString
    {
        get
        {
            return String.Format("{0:#,0}", _ask);
        }
    }

    private DateTime _tickTimeStamp;
    public DateTime TickTimeStamp
    {
        get
        {
            return _tickTimeStamp;
        }
        set
        {
            if (_tickTimeStamp == value)
                return;

            _tickTimeStamp = value;
            this.NotifyPropertyChanged("TickTimeStamp");
            this.NotifyPropertyChanged("TickTimeStampString");

        }
    }
    public string TickTimeStampString
    {
        get
        {
            return PairString + " - " + _tickTimeStamp.ToLocalTime().ToString("yyyy/MM/dd/HH:mm:ss");
        }
    }

    // LTPのチェックをするかどうか（省エネ目的）
    private bool _enabled;
    public bool Enabled
    {
        get
        {
            return _enabled;
        }
        set
        {
            if (_enabled == value)
                return;

            _enabled = value;
            this.NotifyPropertyChanged("Enabled");

        }
    }

    #region == アラーム用のプロパティ ==

    // カスタム値 アラーム
    private decimal _alarmPlus;
    public decimal AlarmPlus
    {
        get
        {
            return _alarmPlus;
        }
        set
        {
            if (_alarmPlus == value)
                return;

            _alarmPlus = value;
            this.NotifyPropertyChanged("AlarmPlus");
            this.NotifyPropertyChanged("AlarmPlusString");
        }
    }
    public string AlarmPlusString
    {
        get
        {
            return String.Format(_ltpFormstString, AlarmPlus);
        }
    }

    private decimal _alarmMinus;
    public decimal AlarmMinus
    {
        get
        {
            return _alarmMinus;
        }
        set
        {
            if (_alarmMinus == value)
                return;

            _alarmMinus = value;
            this.NotifyPropertyChanged("AlarmMinus");
            this.NotifyPropertyChanged("AlarmMinusString");
        }
    }
    public string AlarmMinusString
    {
        get
        {
            return String.Format(_ltpFormstString, AlarmMinus);
        }
    }

    // 起動後　最安値　最高値　アラーム情報表示
    private string _highLowInfoText;
    public string HighLowInfoText
    {
        get
        {
            return _highLowInfoText;
        }
        set
        {
            if (_highLowInfoText == value)
                return;

            _highLowInfoText = value;
            this.NotifyPropertyChanged("HighLowInfoText");
        }
    }

    private bool _highLowInfoTextColorFlag;
    public bool HighLowInfoTextColorFlag
    {
        get
        {
            return _highLowInfoTextColorFlag;
        }
        set
        {
            if (_highLowInfoTextColorFlag == value)
                return;

            _highLowInfoTextColorFlag = value;
            this.NotifyPropertyChanged("HighLowInfoTextColorFlag");
        }
    }

    // アラーム音
    // 起動後
    private bool _playSoundLowest = false;
    public bool PlaySoundLowest
    {
        get
        {
            return _playSoundLowest;
        }
        set
        {
            if (_playSoundLowest == value)
                return;

            _playSoundLowest = value;
            this.NotifyPropertyChanged("PlaySoundLowest");
        }
    }

    private bool _playSoundHighest = false;
    public bool PlaySoundHighest
    {
        get
        {
            return _playSoundHighest;
        }
        set
        {
            if (_playSoundHighest == value)
                return;

            _playSoundHighest = value;
            this.NotifyPropertyChanged("PlaySoundHighest");
        }
    }

    // last 24h
    private bool _playSoundLowest24h = false;
    public bool PlaySoundLowest24h
    {
        get
        {
            return _playSoundLowest24h;
        }
        set
        {
            if (_playSoundLowest24h == value)
                return;

            _playSoundLowest24h = value;
            this.NotifyPropertyChanged("PlaySoundLowest24h");
        }
    }

    private bool _playSoundHighest24h = false;
    public bool PlaySoundHighest24h
    {
        get
        {
            return _playSoundHighest24h;
        }
        set
        {
            if (_playSoundHighest24h == value)
                return;

            _playSoundHighest24h = value;
            this.NotifyPropertyChanged("PlaySoundHighest24h");
        }
    }

    #endregion

    #region == 統計情報のプロパティ ==

    // 起動時初期価格
    private decimal _basePrice = 0;
    public decimal BasePrice
    {
        get
        {
            return _basePrice;
        }
        set
        {
            if (_basePrice == value)
                return;

            _basePrice = value;

            this.NotifyPropertyChanged("BasePrice");
            this.NotifyPropertyChanged("BasePriceIcon");
            this.NotifyPropertyChanged("BasePriceString");

        }
    }

    public string BasePriceString
    {
        get
        {
            return String.Format(_ltpFormstString, BasePrice);
        }
    }

    public string BasePriceIcon
    {
        get
        {
            if (_ltp > BasePrice)
            {
                return "▲";
            }
            else if (_ltp < BasePrice)
            {
                return "▼";
            }
            else
            {
                return "＝";
            }
        }
    }

    private bool _basePriceUpFlag;
    public bool BasePriceUpFlag
    {
        get
        {
            return _basePriceUpFlag;
        }
        set
        {
            if (_basePriceUpFlag == value)
                return;

            _basePriceUpFlag = value;
            this.NotifyPropertyChanged("BasePriceUpFlag");
        }
    }

    // 過去1分間の平均値
    private decimal _averagePrice;
    public decimal AveragePrice
    {
        get
        {
            return _averagePrice;
        }
        set
        {
            if (_averagePrice == value)
                return;

            _averagePrice = value;
            this.NotifyPropertyChanged("AveragePrice");
            this.NotifyPropertyChanged("AveragePriceIcon");
            //this.NotifyPropertyChanged("AveragePriceIconColor");
            this.NotifyPropertyChanged("AveragePriceString");
        }
    }

    public string AveragePriceString
    {
        get
        {
            return String.Format(_ltpFormstString, _averagePrice); ;
        }
    }

    public string AveragePriceIcon
    {
        get
        {
            if (_ltp > _averagePrice)
            {
                return "▲";
            }
            else if (_ltp < _averagePrice)
            {
                return "▼";
            }
            else
            {
                return "＝";
            }
        }
    }

    private bool _averagePriceUpFlag;
    public bool AveragePriceUpFlag
    {
        get
        {
            return _averagePriceUpFlag;
        }
        set
        {
            if (_averagePriceUpFlag == value)
                return;

            _averagePriceUpFlag = value;
            this.NotifyPropertyChanged("AveragePriceUpFlag");
        }
    }

    // 過去２４時間の中央値
    public decimal MiddleLast24Price
    {
        get
        {
            return ((_lowestIn24Price + _highestIn24Price) / 2);
        }
    }
    public string MiddleLast24PriceString
    {
        get
        {
            return String.Format(_ltpFormstString, MiddleLast24Price); ;
        }
    }

    public string MiddleLast24PriceIcon
    {
        get
        {
            if (_ltp > MiddleLast24Price)
            {
                return "▲";
            }
            else if (_ltp < MiddleLast24Price)
            {
                return "▼";
            }
            else
            {
                return "＝";
            }
        }
    }

    private bool _middleLast24PriceUpFlag;
    public bool MiddleLast24PriceUpFlag
    {
        get
        {
            return _middleLast24PriceUpFlag;
        }
        set
        {
            if (_middleLast24PriceUpFlag == value)
                return;

            _middleLast24PriceUpFlag = value;
            this.NotifyPropertyChanged("MiddleLast24PriceUpFlag");
        }
    }

    // 起動後の中央値
    public decimal MiddleInitPrice
    {
        get
        {
            return ((_lowestPrice + _highestPrice) / 2);
        }
    }
    public string MiddleInitPriceString
    {
        get
        {
            return String.Format(_ltpFormstString, MiddleInitPrice); ;
        }
    }

    public string MiddleInitPriceIcon
    {
        get
        {
            if (_ltp > MiddleInitPrice)
            {
                return "▲";
            }
            else if (_ltp < MiddleInitPrice)
            {
                return "▼";
            }
            else
            {
                return "＝";
            }
        }
    }

    private bool _middleInitPriceUpFlag;
    public bool MiddleInitPriceUpFlag
    {
        get
        {
            return _middleInitPriceUpFlag;
        }
        set
        {
            if (_middleInitPriceUpFlag == value)
                return;

            _middleInitPriceUpFlag = value;
            this.NotifyPropertyChanged("MiddleInitPriceUpFlag");
        }
    }

    // 24時間の最高値 
    private decimal _highestIn24Price;
    public decimal HighestIn24Price
    {
        get
        {
            return _highestIn24Price;
        }
        set
        {
            if (_highestIn24Price == value)
                return;

            _highestIn24Price = value;
            this.NotifyPropertyChanged("HighestIn24Price");
            this.NotifyPropertyChanged("High24String");

            this.NotifyPropertyChanged("MiddleLast24Price");
            this.NotifyPropertyChanged("MiddleLast24PriceString");

            //if (MinMode) return;
            // チャートの最高値をセット
            //ChartAxisY[0].MaxValue = (double)_highestIn24Price + 3000;
            // チャートの２４最高値ポイントを更新
            //(ChartSeries[1].Values[0] as ObservableValue).Value = (double)_highestIn24Price;

        }
    }
    public string High24String
    {
        get
        {
            return String.Format(_ltpFormstString, _highestIn24Price);
        }
    }

    // 24時間の最高値 アラートOn/Offフラグ
    private bool _highestIn24PriceAlart;
    public bool HighestIn24PriceAlart
    {
        get
        {
            return _highestIn24PriceAlart;
        }
        set
        {
            if (_highestIn24PriceAlart == value)
                return;

            _highestIn24PriceAlart = value;
            this.NotifyPropertyChanged("HighestIn24PriceAlart");
            this.NotifyPropertyChanged("PriceAlart");
        }
    }

    // 24時間の最安値 
    private decimal _lowestIn24Price;
    public decimal LowestIn24Price
    {
        get
        {
            return _lowestIn24Price;
        }
        set
        {
            if (_lowestIn24Price == value)
                return;

            _lowestIn24Price = value;
            this.NotifyPropertyChanged("LowestIn24Price");
            this.NotifyPropertyChanged("Low24String");

            this.NotifyPropertyChanged("MiddleLast24Price");
            this.NotifyPropertyChanged("MiddleLast24PriceString");

            //if (MinMode) return;
            // チャートの最低値をセット
            //ChartAxisY[0].MinValue = (double)_lowestIn24Price - 3000;
            // チャートの２４最低値ポイントを更新
            //(ChartSeries[2].Values[0] as ObservableValue).Value = (double)_lowestIn24Price;
        }
    }
    public string Low24String
    {
        get
        {
            return String.Format(_ltpFormstString, _lowestIn24Price);
        }
    }

    // 24時間の最安値 アラートOn/Offフラグ
    private bool _lowestIn24PriceAlart;
    public bool LowestIn24PriceAlart
    {
        get
        {
            return _lowestIn24PriceAlart;
        }
        set
        {
            if (_lowestIn24PriceAlart == value)
                return;

            _lowestIn24PriceAlart = value;
            this.NotifyPropertyChanged("LowestIn24PriceAlart");
            this.NotifyPropertyChanged("PriceAlart");
        }
    }

    // 起動後 最高値
    private decimal _highestPrice;
    public decimal HighestPrice
    {
        get
        {
            return _highestPrice;
        }
        set
        {
            if (_highestPrice == value)
                return;

            _highestPrice = value;
            this.NotifyPropertyChanged("HighestPrice");
            this.NotifyPropertyChanged("HighestPriceString");

            this.NotifyPropertyChanged("MiddleInitPrice");
            this.NotifyPropertyChanged("MiddleInitPriceString");
            this.NotifyPropertyChanged("MiddleInitPriceIcon");

            //if (MinMode) return;
            //(ChartSeries[1].Values[0] as ObservableValue).Value = (double)_highestPrice;
        }
    }
    public string HighestPriceString
    {
        get
        {
            return String.Format(_ltpFormstString, _highestPrice); ;
        }
    }

    // 起動後 最高値 アラートOn/Offフラグ
    private bool _highestPriceAlart;
    public bool HighestPriceAlart
    {
        get
        {
            return _highestPriceAlart;
        }
        set
        {
            if (_highestPriceAlart == value)
                return;

            _highestPriceAlart = value;
            this.NotifyPropertyChanged("HighestPriceAlart");
            this.NotifyPropertyChanged("PriceAlart");
        }
    }

    // 起動後 最低値
    private decimal _lowestPrice;
    public decimal LowestPrice
    {
        get
        {
            return _lowestPrice;
        }
        set
        {
            if (_lowestPrice == value)
                return;

            _lowestPrice = value;
            this.NotifyPropertyChanged("LowestPrice");
            this.NotifyPropertyChanged("LowestPriceString");

            this.NotifyPropertyChanged("MiddleInitPrice");
            this.NotifyPropertyChanged("MiddleInitPriceString");
            this.NotifyPropertyChanged("MiddleInitPriceIcon");

            //if (MinMode) return;
            // (ChartSeries[2].Values[0] as ObservableValue).Value = (double)_lowestPrice;
        }
    }
    public string LowestPriceString
    {
        get
        {
            return String.Format(_ltpFormstString, _lowestPrice); ;
        }
    }

    // 起動後 最低値アラートOn/Offフラグ
    private bool _lowestPriceAlart;
    public bool LowestPriceAlart
    {
        get
        {
            return _lowestPriceAlart;
        }
        set
        {
            if (_lowestPriceAlart == value)
                return;

            _lowestPriceAlart = value;
            this.NotifyPropertyChanged("LowestPriceAlart");
            this.NotifyPropertyChanged("PriceAlart");
        }
    }

    // タブの点滅用
    public bool PriceAlart
    {
        get
        {
            if (LowestPriceAlart || HighestPriceAlart || LowestIn24PriceAlart || HighestIn24PriceAlart)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    #endregion

    #region == 板情報のプロパティ ==

    private decimal _depthGrouping = 0;
    public decimal DepthGrouping
    {
        get
        {
            return _depthGrouping;
        }
        set
        {
            if (_depthGrouping == value)
                return;

            _depthGrouping = value;

            if (DepthGrouping100 == _depthGrouping)
            {
                IsDepthGrouping100 = true;
                IsDepthGrouping1000 = false;
                IsDepthGroupingOff = false;
            }
            if (DepthGrouping1000 == _depthGrouping)
            {
                IsDepthGrouping100 = false;
                IsDepthGrouping1000 = true;
                IsDepthGroupingOff = false;
            }

            if (0 == _depthGrouping)
            {
                IsDepthGrouping100 = false;
                IsDepthGrouping1000 = false;
                IsDepthGroupingOff = true;
            }

            this.NotifyPropertyChanged("DepthGrouping");

        }
    }

    private bool _isDepthGroupingOff = true;
    public bool IsDepthGroupingOff
    {
        get
        {
            return _isDepthGroupingOff;
        }
        set
        {
            if (_isDepthGroupingOff == value)
                return;

            _isDepthGroupingOff = value;
            this.NotifyPropertyChanged("IsDepthGroupingOff");
        }
    }

    private decimal _depthGrouping100 = 100;
    public decimal DepthGrouping100
    {
        get
        {
            return _depthGrouping100;
        }
        set
        {
            if (_depthGrouping100 == value)
                return;

            _depthGrouping100 = value;
            this.NotifyPropertyChanged("DepthGrouping100");

        }
    }

    private bool _isDepthGrouping100;
    public bool IsDepthGrouping100
    {
        get
        {
            return _isDepthGrouping100;
        }
        set
        {
            if (_isDepthGrouping100 == value)
                return;

            _isDepthGrouping100 = value;
            this.NotifyPropertyChanged("IsDepthGrouping100");
        }
    }

    private decimal _depthGrouping1000 = 1000;
    public decimal DepthGrouping1000
    {
        get
        {
            return _depthGrouping1000;
        }
        set
        {
            if (_depthGrouping1000 == value)
                return;

            _depthGrouping1000 = value;
            this.NotifyPropertyChanged("DepthGrouping1000");

        }
    }

    private bool _isDepthGrouping1000;
    public bool IsDepthGrouping1000
    {
        get
        {
            return _isDepthGrouping1000;
        }
        set
        {
            if (_isDepthGrouping1000 == value)
                return;

            _isDepthGrouping1000 = value;
            this.NotifyPropertyChanged("IsDepthGrouping1000");
        }
    }

    #endregion

    #region == コレクション ==

    // TickHistoryクラス リスト
    private ObservableCollection<TickHistory> _tickHistory = new ObservableCollection<TickHistory>();
    public ObservableCollection<TickHistory> TickHistories
    {
        get
        {
            return this._tickHistory;
        }
    }

    #endregion

    // コンストラクタ
    public Pair(Pairs p, double fontSize, string ltpFormstString, decimal grouping100, decimal grouping1000)
    {
        this._p = p;
        _ltpFontSize = fontSize;
        _ltpFormstString = ltpFormstString;

        _depthGrouping100 = grouping100;
        _depthGrouping1000 = grouping1000;

        //BindingOperations.EnableCollectionSynchronization(this._tickHistory, new object());
    }

}
