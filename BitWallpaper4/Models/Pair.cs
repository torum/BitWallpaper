using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitWallpaper4.ViewModels;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using Microsoft.UI.Xaml.Data;
using SkiaSharp;
using System.Diagnostics;
using BitWallpaper4.Models.APIClients;
using System.Globalization;
using System.Runtime.Intrinsics.Arm;
using System.Collections;
using LiveChartsCore.Drawing;
using Windows.Services.Store;
using System.Reflection.Emit;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml;

namespace BitWallpaper4.Models;

public enum PairCodes
{
    btc_jpy, xrp_jpy, eth_jpy, ltc_jpy, bcc_jpy, mona_jpy, xlm_jpy, qtum_jpy, bat_jpy
}

public class Pair : ViewModelBase
{
    private PairCodes _p;
    public PairCodes PairCode
    {
        get
        {
            return _p;
        }
    }
    
    public string PairString // 表示用 通貨ペア名 "BTC/JPY";
    {
        get
        {
            return PairStrings[_p];
        }
    }

    private Dictionary<PairCodes, string> PairStrings
    {
        get; set;
    } = new Dictionary<PairCodes, string>()
    {
                {PairCodes.btc_jpy, "BTC/JPY"},
                {PairCodes.xrp_jpy, "XRP/JPY"},
                {PairCodes.eth_jpy, "ETH/JPY"},
                {PairCodes.ltc_jpy, "LTC/JPY"},
                {PairCodes.mona_jpy, "MONA/JPY"},
                {PairCodes.bcc_jpy, "BCH/JPY"},
                {PairCodes.xlm_jpy, "XLM/JPY"},
                {PairCodes.qtum_jpy, "QTUM/JPY"},
                {PairCodes.bat_jpy, "BAT/JPY"},
            };

    public Dictionary<string, PairCodes> GetPairs { get; set; } = new Dictionary<string, PairCodes>()
        {
            {"btc_jpy", PairCodes.btc_jpy},
            {"xrp_jpy", PairCodes.xrp_jpy},
            //{"eth_btc", Pairs.eth_btc},
            {"eth_jpy", PairCodes.eth_jpy},
            //{"ltc_btc", Pairs.ltc_btc},
            {"ltc_jpy", PairCodes.ltc_jpy},
            {"mona_jpy", PairCodes.mona_jpy},
            //{"mona_btc", Pairs.mona_btc},
            {"bcc_jpy", PairCodes.bcc_jpy},
            //{"bcc_btc", Pairs.bcc_btc},
            {"xlm_jpy", PairCodes.xlm_jpy},
            {"qtum_jpy", PairCodes.qtum_jpy},
            {"bat_jpy", PairCodes.bat_jpy},
        };

    public string CurrencyUnitString { get => CurrentPairCoin[_p]; }

    public Dictionary<PairCodes, string> CurrentPairCoin { get; set; } = new Dictionary<PairCodes, string>()
        {
            {PairCodes.btc_jpy, "BTC"},
            {PairCodes.xrp_jpy, "XRP"},
            //{Pairs.eth_btc, "ETH"},
            {PairCodes.eth_jpy, "ETH"},
            //{Pairs.ltc_btc, "LTC"},
            {PairCodes.ltc_jpy, "LTC"},
            {PairCodes.mona_jpy, "Mona"},
            //{Pairs.mona_btc, "Mona"},
            {PairCodes.bcc_jpy, "BCH"},
            //{Pairs.bcc_btc, "BCH"},
            {PairCodes.xlm_jpy, "XLM"},
            {PairCodes.qtum_jpy, "QTUM"},
            {PairCodes.bat_jpy, "BAT"},
        };

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

            if (IsActive) // && Enabled
            {
                //Debug.WriteLine("_ltp" + _ltp.ToString());

                // test
                //series[1].Values = new double[] { (double)_ltp };
                //YAxes[1]

                if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return;
                (App.Current as App)?.CurrentDispatcherQueue.TryEnqueue(() =>
                {
                    Sections[0].Yi = (double)_ltp;
                    Sections[0].Yj = (double)_ltp;
                });
                /*
                Sections[0] = new RectangularSection
                {
                    Yi = (double)_ltp,
                    Yj = (double)_ltp,
                    Stroke = new SolidColorPaint
                    {
                        Color = SKColors.Silver,
                        StrokeThickness = 1,
                        PathEffect = new DashEffect(new float[] { 6, 6 })
                    }
                };
                */

                //this.NotifyPropertyChanged("Sections");
            }
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

    private string _ltpFormstString = "{0:#,0}";
    public string LtpFormstString
    {
        get
        {
            return _ltpFormstString;
        }
        set
        {
            if (_ltpFormstString == value)
                return;

            _ltpFormstString = value;
            this.NotifyPropertyChanged(nameof(LtpFormstString));
        }
    }

    private string _currencyFormstString = "C";

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
            return _tickTimeStamp.ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss");
        }
    }

    private ObservableCollection<TickHistory> _tickHistory = new ObservableCollection<TickHistory>();
    public ObservableCollection<TickHistory> TickHistories
    {
        get
        {
            return this._tickHistory;
        }
    }
    
    // TODO: LTPのチェックをするかどうか（省エネ目的）
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

    // Selected or not.
    private bool _isActive;
    public bool IsActive
    {
        get
        {
            return _isActive;
        }
        set
        {
            if (_isActive == value)
                return;

            _isActive = value;
            this.NotifyPropertyChanged(nameof(IsActive));
        }
    }

    private bool _isChartInitAndLoaded;
    public bool IsChartInitAndLoaded
    {
        get=> _isChartInitAndLoaded;
        set
        {
            if (_isChartInitAndLoaded == value)
                return;

            _isChartInitAndLoaded = value;
            this.NotifyPropertyChanged(nameof(IsChartInitAndLoaded));
        }
    }

    #region == アラーム ==

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

    #region == 統計情報ィ ==

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

    #region == 板情報と歩み値 ==

    private ObservableCollection<Transaction> _transactions = new ObservableCollection<Transaction>();
    public ObservableCollection<Transaction> Transactions
    {
        get { return this._transactions; }
        set
        {
            if (_transactions == value)
                return;

            _transactions = value;
            this.NotifyPropertyChanged(nameof(Transactions));
        }
    }

    private ObservableCollection<Depth> _depth = new ObservableCollection<Depth>();
    public ObservableCollection<Depth> Depth
    {
        get { return this._depth; }
        set
        {
            if (_depth == value)
                return;

            _depth = value;
            this.NotifyPropertyChanged(nameof(Depth));
        }
    }

    private bool _isDeepthGroupingChanged;
    public bool IsDepthGroupingChanged
    {
        get
        {
            return _isDeepthGroupingChanged;
        }
        set
        {
            if (_isDeepthGroupingChanged == value)
                return;

            _isDeepthGroupingChanged = value;

            this.NotifyPropertyChanged("IsDepthGroupingChanged");
        }
    }

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

    #region == チャート ==

    //RectangularSection
    public Section<LiveChartsCore.SkiaSharpView.Drawing.SkiaSharpDrawingContext>[] Sections { get; set; } =
    {
        new RectangularSection
        {
            Yi = 0,
            Yj = 0,
            ScalesYAt = 1,
            Stroke = new SolidColorPaint
            {
                Color = SKColors.Silver,
                StrokeThickness = 1,
                PathEffect = new DashEffect(new float[] { 6, 6 })
            }
        }
    };

    public ICartesianAxis[] XAxes
    {
        get; set;

    } =
        {
        new Axis()
            {
                LabelsRotation = -15,
                //LabelsPaint = new SolidColorPaint(SKColors.Wheat),
                Labeler = value => new DateTime((long) value).ToString("MM/dd"),
                UnitWidth = TimeSpan.FromHours(0.5).Ticks,

                //4294727256 UpFill = new SolidColorPaint(SKColors.Blue), 
    
                //Labeler =(p) => (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)).AddMilliseconds((long)p).ToString("dd"),
                //Labeler = (s) => new DateTime((long)s).ToString("yyyy MMM dd"),
                //Labeler = value => new DateTime((long) value).ToString("yyyy MMM dd"),
                //Labeler = p => new DateTime((long)p).ToString("yyyy-MMM-dd"),
                // set the unit width of the axis to "days"
                // since our X axis is of type date time and 
                // the interval between our points is in days
                //UnitWidth = TimeSpan.FromMinutes(1).Ticks,
                //UnitWidth = TimeSpan.FromDays(1).Ticks,
                MaxLimit = null,
                //MinLimit = null,
                //MinLimit= DateTime.Now.Ticks - TimeSpan.FromDays(30).Ticks,
                MinLimit= DateTime.Now.Ticks - TimeSpan.FromDays(2.8).Ticks,
                //Position = AxisPosition.Start  
                //MinLimit = 0
                
                // The MinStep property forces the separator to be greater than 1 day.
                MinStep = TimeSpan.FromDays(1).Ticks,

                SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 1,
                                PathEffect = new DashEffect(new float[] { 3, 3 }) }

            }
    };

    public ICartesianAxis[] YAxes
    {
        get; set;

    } =
        {
        new Axis()
            {
                LabelsRotation = 0,
                //LabelsPaint = new SolidColorPaint(SKColors.Wheat),
                IsVisible = false,
                Position = LiveChartsCore.Measure.AxisPosition.Start,
                ShowSeparatorLines = false,
                //Labeler =(value) => (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)value).ToString("hh:mm"),
                MinLimit=0,
                //Labeler = Labelers.Currency,
                //Labeler = (value) => value.ToString("C", new System.Globalization.CultureInfo("ja-Jp")),
                //Labeler = value => new DateTime((long)value).ToString("yyyy-MMM-dd"),
                // set the unit width of the axis to "days"
                // since our X axis is of type date time and 
                // the interval between our points is in days
                //UnitWidth = TimeSpan.FromMinutes(1).Ticks,
                //UnitWidth = TimeSpan.FromMinutes(1).Ticks,
                //MaxLimit = DateTime.Now.Ticks,
                // The MinStep property forces the separator to be greater than 1 day.
                //MinStep = TimeSpan.FromDays(1).Ticks // mark
            },
        new Axis()
            {
                LabelsRotation = 0,
                //LabelsPaint = new SolidColorPaint(SKColors.Wheat),
                Position = LiveChartsCore.Measure.AxisPosition.End,
                //Labeler =(value) => (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)value).ToString("hh:mm"),
                SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
                            {
                                StrokeThickness = 1,
                                PathEffect = new DashEffect(new float[] { 3, 3 })
                            },
                //Labeler = Labelers.Currency,
                Labeler = (value) => value.ToString("C", new System.Globalization.CultureInfo("ja-Jp")),
                //Labeler = value => new DateTime((long)value).ToString("yyyy-MMM-dd"),
                // set the unit width of the axis to "days"
                // since our X axis is of type date time and 
                // the interval between our points is in days
                //UnitWidth = TimeSpan.FromMinutes(1).Ticks,
                //UnitWidth = TimeSpan.FromMinutes(1).Ticks,
                //MaxLimit = DateTime.Now.Ticks,
                // The MinStep property forces the separator to be greater than 1 day.
                //MinStep = TimeSpan.FromDays(1).Ticks // mark
            }
    };

    /*
        public ISeries[] Series
        {
            get; set;
        } = { new CandlesticksSeries<FinancialPoint>
            {
                Values = new ObservableCollection<FinancialPoint>
                {
                    //                      date, high, open, close, low
                    new(new DateTime(2021, 1, 1), 523, 500, 450, 400),
                    new(new DateTime(2021, 1, 2), 500, 450, 425, 400),
                    new(new DateTime(2021, 1, 3), 490, 425, 400, 380),
                    new(new DateTime(2021, 1, 4), 420, 400, 420, 380),
                    new(new DateTime(2021, 1, 5), 520, 420, 490, 400),
                    new(new DateTime(2021, 1, 6), 580, 490, 560, 440),
                    new(new DateTime(2021, 1, 7), 570, 560, 350, 340),
                    new(new DateTime(2021, 1, 8), 380, 350, 380, 330),
                    new(new DateTime(2021, 1, 9), 440, 380, 420, 350),
                    new(new DateTime(2021, 1, 10), 490, 420, 460, 400),
                    new(new DateTime(2021, 1, 11), 520, 460, 510, 460),
                    new(new DateTime(2021, 1, 12), 580, 510, 560, 500),
                    new(new DateTime(2021, 1, 13), 600, 560, 540, 510),
                    new(new DateTime(2021, 1, 14), 580, 540, 520, 500),
                    new(new DateTime(2021, 1, 15), 580, 520, 560, 520),
                    new(new DateTime(2021, 1, 16), 590, 560, 580, 520),
                    new(new DateTime(2021, 1, 17), 650, 580, 630, 550),
                    new(new DateTime(2021, 1, 18), 680, 630, 650, 600),
                    new(new DateTime(2021, 1, 19), 670, 650, 600, 570),
                    new(new DateTime(2021, 1, 20), 640, 600, 610, 560),
                    new(new DateTime(2021, 1, 21), 630, 610, 630, 590),
                    
                }
            }};
  */
    /*
     
        UpFill = new SolidColorPaint(SKColors.CadetBlue),
        UpStroke = new SolidColorPaint(SKColors.CadetBlue) { StrokeThickness = 1 },
        DownFill = new SolidColorPaint(SKColors.IndianRed),
        DownStroke = new SolidColorPaint(SKColors.Orange) { StrokeThickness = 1 },
    */

    private ISeries[] _series = 
    {
        new ColumnSeries<DateTimePoint>
        {
            Name = "Volume",
            ScalesYAt = 0,
            //Stroke = new SolidColorPaint((new SKColor(198, 167, 0)), 0),
            Fill =  new SolidColorPaint((new SKColor(127, 127, 127)), 1),
            TooltipLabelFormatter = (chartPoint) =>
                $"Volume, {new DateTime((long) chartPoint.SecondaryValue):yyy/MM/dd HH}: {chartPoint.PrimaryValue}",
            Values = new ObservableCollection<DateTimePoint>
            {
                new DateTimePoint(DateTime.Now, 0)
            }
        },
        new CandlesticksSeries<FinancialPoint>
        {
            Name = "Price",
            ScalesYAt = 1,
            //TooltipLabelFormatter = (chartPoint) => $"Price: {new DateTime((long) chartPoint.SecondaryValue):yyy/MM/dd HH}: {chartPoint.PrimaryValue}",
            Values = new ObservableCollection<FinancialPoint>
            {
                //new(DateTime.Now, 100, 25, 75, 0),
                new(DateTime.Now, 0, 0, 0, 0)
            }
        }
        /*
        new ColumnSeries<double>
        {
            Name = "Volume",
            Values = new double[] { 10 }
        }
        */

    };

    //{ new CandlesticksSeries<FinancialPoint>() };// = new CandlesticksSeries<FinancialPoint>[1] ;
    public ISeries[] Series
    {
        get => _series;
        set
        {
            if (_series == value)
                return;

            _series = value;
            NotifyPropertyChanged(nameof(Series));
        }
    }// = { new CandlesticksSeries<FinancialPoint>()};

    /*
    private ISeries[] series = new CandlesticksSeries<FinancialPoint>[1];
    public ISeries[] Series
    {
        get => series;
        set
        {
            if (series == value)
                return;

            series = value;
            NotifyPropertyChanged(nameof(Series));
        }
    }
    */

    private CandleTypes _selectedCandleType = CandleTypes.OneHour;
    public CandleTypes SelectedCandleType
    {
        get => _selectedCandleType;
        set
        {
            if (_selectedCandleType == value)
                return;

            _selectedCandleType = value;
            this.NotifyPropertyChanged(nameof(SelectedCandleType));
        }
    }

    // 一時間単位 
    private List<Ohlcv> _ohlcvsOneHour = new List<Ohlcv>();
    public List<Ohlcv> OhlcvsOneHour
    {
        get
        {
            return _ohlcvsOneHour;
        }
        set
        {
            _ohlcvsOneHour = value;
            //this.NotifyPropertyChanged("OhlcvsOneHour");
        }
    }

    // 一分単位 
    private List<Ohlcv> _ohlcvsOneMin = new List<Ohlcv>();
    public List<Ohlcv> OhlcvsOneMin
    {
        get
        {
            return _ohlcvsOneMin;
        }
        set
        {
            _ohlcvsOneMin = value;
            //this.NotifyPropertyChanged("OhlcvsOneMin");
        }
    }

    // 一日単位 
    private List<Ohlcv> _ohlcvsOneDay = new List<Ohlcv>();
    public List<Ohlcv> OhlcvsOneDay
    {
        get
        {
            return _ohlcvsOneDay;
        }
        set
        {
            _ohlcvsOneDay = value;
            //this.NotifyPropertyChanged("OhlcvsOneDay");
        }
    }

    #endregion

    // HTTP Clients
    readonly PublicAPIClient _pubCandlestickApi = new();
    readonly PublicAPIClient _pubTransactionsApi = new();
    readonly PublicAPIClient _pubDepthApi = new();

    // Timer
    readonly DispatcherTimer _dispatcherTimerDepth = new();
    readonly DispatcherTimer _dispatcherTimerTransaction = new();

    // コンストラクタ
    public Pair(PairCodes p, double fontSize, string ltpFormstString, string currencyFormstString, decimal grouping100, decimal grouping1000)
    {
        this._p = p;
        _ltpFontSize = fontSize;
        _ltpFormstString = ltpFormstString;
        _currencyFormstString = currencyFormstString;

        _depthGrouping100 = grouping100;
        _depthGrouping1000 = grouping1000;


        // Ticker update timer
        _dispatcherTimerDepth.Tick += TickerTimerDepth;
        _dispatcherTimerDepth.Interval = new TimeSpan(0, 0, 1);
        _dispatcherTimerDepth.Start();

        // Ticker update timer
        _dispatcherTimerTransaction.Tick += TickerTimerTransaction;
        _dispatcherTimerTransaction.Interval = new TimeSpan(0, 0, 2);
        _dispatcherTimerTransaction.Start();

        //InitializeAndGetChartData(CandleTypes.OneHour);
    }

    private void TickerTimerDepth(object source, object e)
    {
        if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return;
        
        UpdateDepth();
    }

    private void TickerTimerTransaction(object source, object e)
    {
        if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return;
        
        UpdateTransactions();
    }

    public async void InitializeAndGetChartData(CandleTypes ct)
    {
        if (IsChartInitAndLoaded)
            return;


        SelectedCandleType = ct;

        bool bln = await GetCandlesticks(PairCode, ct);

        if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return;

        if (bln == true)
        {
            LoadChart(PairCode, ct);

            if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return;
            (App.Current as App)?.CurrentDispatcherQueue.TryEnqueue(() =>
            {
                Sections[0].Yi = (double)_ltp;
                Sections[0].Yj = (double)_ltp;
            });


            //this.NotifyPropertyChanged("Sections");

            IsChartInitAndLoaded = true;
        }


        // temp

        //UpdateDepth();

        //UpdateTransactions();

    }

    private void LoadChart(PairCodes pair, CandleTypes ct)
    {

        if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return;
        (App.Current as App)?.CurrentDispatcherQueue.TryEnqueue(() =>
        {

        });
        // TODO: temp hack
        if (_currencyFormstString.Equals("C3"))
        {
            YAxes[1].Labeler = (value) => value.ToString("C3", new System.Globalization.CultureInfo("ja-Jp"));
        }
        else
        {
            YAxes[1].Labeler = (value) => value.ToString("C", new System.Globalization.CultureInfo("ja-Jp"));
        }

        var fuga = new CandlesticksSeries<FinancialPoint>();
        var test = new ObservableCollection<FinancialPoint>();

        var vols = new ObservableCollection<DateTimePoint>();//new ColumnSeries<double>();

        //Ohlcv asdf = new Ohlcv();

        foreach (var hoge in OhlcvsOneHour)
        {
            //Debug.WriteLine(hoge.TimeStamp.ToString());

            var asdf = new LiveChartsCore.Defaults.FinancialPoint(hoge.TimeStamp, (double)hoge.High, (double)hoge.Open, (double)hoge.Close, (double)hoge.Low);
            //asdf.TimeStamp = hoge.TimeStamp;
            //asdf.Volume = hoge.Volume;
            //asdf.Close = hoge.Close;
            //asdf.Low = hoge.Low;
            //asdf.High = hoge.High;
            test.Add(asdf);

            //test.Add(hoge);

            vols.Add(new DateTimePoint(hoge.TimeStamp, (double)hoge.Volume));
            //vols.Add(new FinancialVolume { Date=hoge.TimeStamp, Volume=(double)hoge.Volume});
        }


        //var stockData = test.ToArray();
        /*
        var fuga = new CandlesticksSeries<Ohlcv>
        {
            Mapping = (ohlcv, point) =>
            {
                point.SecondaryValue = (double)ohlcv.TimeStamp.Ticks;
                point.PrimaryValue = (double)ohlcv.High;
                point.TertiaryValue = (double)ohlcv.Open;
                point.QuaternaryValue = (double)ohlcv.Close;
                point.QuinaryValue = (double)ohlcv.Low;
            },
            Values = test.ToArray()
        };
        */

        //fuga.Values = test.ToArray();

        //Series[0].Values = test;

        //Series = new CandlesticksSeries<Ohlcv>[1];

        //Series[0] = fuga;
        /*
        test = new ObservableCollection<FinancialPoint>
                {
                    //                      date, high, open, close, low
                    new(new DateTime(2021, 1, 1,0,1,56), 523, 500, 450, 400),
                    new(new DateTime(2021, 1, 2), 500, 450, 425, 400),
                    new(new DateTime(2021, 1, 3), 490, 425, 400, 380),
                    new(new DateTime(2021, 1, 4), 420, 400, 420, 380),
                    new(new DateTime(2021, 1, 5), 520, 420, 490, 400),
                    new(new DateTime(2021, 1, 6), 580, 490, 560, 440),
                    new(new DateTime(2021, 1, 7), 570, 560, 350, 340),
                    new(new DateTime(2021, 1, 8), 380, 350, 380, 330),
                    new(new DateTime(2021, 1, 9), 440, 380, 420, 350),
                    new(new DateTime(2021, 1, 10), 490, 420, 460, 400),
                    new(new DateTime(2021, 1, 11), 520, 460, 510, 460),
                    new(new DateTime(2021, 1, 12), 580, 510, 560, 500),
                    new(new DateTime(2021, 1, 13), 600, 560, 540, 510),
                    new(new DateTime(2021, 1, 14), 580, 540, 520, 500),
                    new(new DateTime(2021, 1, 15), 580, 520, 560, 520),
                    new(new DateTime(2021, 1, 16), 590, 560, 580, 520),
                    new(new DateTime(2021, 1, 17), 650, 580, 630, 550),
                    new(new DateTime(2021, 1, 18), 680, 630, 650, 600),
                    new(new DateTime(2021, 1, 19), 670, 650, 600, 570),
                    new(new DateTime(2021, 1, 20), 640, 600, 610, 560),
                    new(new DateTime(2021, 1, 21), 630, 610, 630, 590),
                };
        */
        //fuga.Values = test;
        //////Series[0].Values = test;
        //await Task.Delay(1000);

        /*
        var vv=new ColumnSeries<double>
        {
            Values = vols.ToArray()
        };
        Series.Append(vv);
        */



        //await Task.Delay(1000);



        //Series[1] = new ColumnSeries<double> { Values= vols };
        //Series[1].Values = vols.ToArray();

        if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return;
        (App.Current as App)?.CurrentDispatcherQueue.TryEnqueue(() =>
        {
        });

        Series[0].Values = vols;
        Series[1].Values = test;

        //await Task.Delay(100);

        //Series = new ISeries[1] { fuga };
        //this.NotifyPropertyChanged("Series");
        //Thread.Sleep(2000);
        //await Task.Delay(100);
    }

    #region == チャート ==

    // 初回に各種Candlestickをまとめて取得
    private async Task<bool> GetCandlesticks(PairCodes pair, CandleTypes ct)
    {
        //ChartLoadingInfo = "チャートデータを取得中....";

        Debug.WriteLine("チャートデータを取得中.... " + pair.ToString());

        // 今日の日付セット。UTCで。
        DateTime dtToday = DateTime.Now.ToUniversalTime();

        // データは、ローカルタイムで、朝9:00 から翌8:59分まで。8:59分までしか取れないので、 9:00過ぎていたら 最新のデータとるには日付を１日追加する

        if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return false;

        #region == OhlcvsOneHour 1hour毎のデータ ==

        List<Ohlcv> ListOhlcvsOneHour = new List<Ohlcv>();

        if (ct == CandleTypes.OneHour)
        {
            //Debug.WriteLine("今日の1hour取得開始 " + pair.ToString());

            // 一時間のロウソク足タイプなら今日、昨日、一昨日、その前の１週間分の1hourデータを取得する必要あり。
            ListOhlcvsOneHour = await GetCandlestick(pair, CandleTypes.OneHour, dtToday);
            if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return false;

            if (ListOhlcvsOneHour != null)
            {
                // 逆順にする
                ListOhlcvsOneHour.Reverse();
            }
            else
            {
                ListOhlcvsOneHour = new List<Ohlcv>();
            }

            //Debug.WriteLine("昨日の1hour取得開始 " + pair.ToString());
            await Task.Delay(200);
            // 昨日
            DateTime dtTarget = dtToday.AddDays(-1);

            List<Ohlcv> res = await GetCandlestick(pair, CandleTypes.OneHour, dtTarget);
            if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return false;

            if (res != null)
            {
                // 逆順にする
                res.Reverse();

                foreach (var r in res)
                {
                    ListOhlcvsOneHour.Add(r);
                }

                //Debug.WriteLine("一昨日の1hour取得開始 " + pair.ToString());
                await Task.Delay(200);
                // 一昨日
                dtTarget = dtTarget.AddDays(-1);
                List<Ohlcv> last2 = await GetCandlestick(pair, CandleTypes.OneHour, dtTarget);
                if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return false;

                if (last2 != null)
                {
                    // 逆順にする
                    last2.Reverse();

                    foreach (var l in last2)
                    {
                        ListOhlcvsOneHour.Add(l);
                    }

                    //Debug.WriteLine("３日前の1hour取得開始 " + pair.ToString());
                    await Task.Delay(200);
                    // ３日前
                    dtTarget = dtTarget.AddDays(-1);
                    List<Ohlcv> last3 = await GetCandlestick(pair, CandleTypes.OneHour, dtTarget);
                    if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return false;

                    if (last3 != null)
                    {
                        // 逆順にする
                        last3.Reverse();

                        foreach (var l in last3)
                        {
                            ListOhlcvsOneHour.Add(l);
                        }


                        //Debug.WriteLine("４日前の1hour取得開始 " + pair.ToString());
                        await Task.Delay(300);
                        // 4日前
                        dtTarget = dtTarget.AddDays(-1);
                        List<Ohlcv> last4 = await GetCandlestick(pair, CandleTypes.OneHour, dtTarget);
                        if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return false;

                        if (last4 != null)
                        {
                            // 逆順にする
                            last4.Reverse();

                            foreach (var l in last4)
                            {
                                ListOhlcvsOneHour.Add(l);
                            }
                            /*
                            await Task.Delay(300);
                            // 5日前
                            dtTarget = dtTarget.AddDays(-1);
                            List<Ohlcv> last5 = await GetCandlestick(pair, CandleTypes.OneHour, dtTarget);
                            if (last5 != null)
                            {
                                // 逆順にする
                                last5.Reverse();

                                foreach (var l in last5)
                                {
                                    ListOhlcvsOneHour.Add(l);
                                }

                                await Task.Delay(300);
                                // 6日前
                                dtTarget = dtTarget.AddDays(-1);
                                List<Ohlcv> last6 = await GetCandlestick(pair, CandleTypes.OneHour, dtTarget);
                                if (last6 != null)
                                {
                                    // 逆順にする
                                    last6.Reverse();

                                    foreach (var l in last6)
                                    {
                                        ListOhlcvsOneHour.Add(l);
                                    }

                                    await Task.Delay(300);
                                    // 7日前
                                    dtTarget = dtTarget.AddDays(-1);
                                    List<Ohlcv> last7 = await GetCandlestick(pair, CandleTypes.OneHour, dtTarget);
                                    if (last7 != null)
                                    {
                                        // 逆順にする
                                        last7.Reverse();

                                        foreach (var l in last7)
                                        {
                                            ListOhlcvsOneHour.Add(l);
                                        }
                                    }


                                }

                            }
                            */

                        }

                    }

                }

            }

        }

        #endregion

        #region == OhlcvsOneMin 1min毎のデータ ==

        List<Ohlcv> ListOhlcvsOneMin = new List<Ohlcv>();

        if (ct == CandleTypes.OneMin)
        {
            //Debug.WriteLine("今日の1min取得開始 " + pair.ToString());

            // 一分毎のロウソク足タイプなら今日と昨日の1minデータを取得する必要あり。
            ListOhlcvsOneMin = await GetCandlestick(pair, CandleTypes.OneMin, dtToday);
            if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return false;

            if (ListOhlcvsOneMin != null)
            {
                // 逆順にする
                ListOhlcvsOneMin.Reverse();
            }
            else
            {
                ListOhlcvsOneMin = new List<Ohlcv>();
            }

            // 00:00:00から23:59:59分までしか取れないので、 3時間分取るには、00:00:00から3:00までは 最新のデータとるには日付を１日マイナスする
            if (dtToday.Hour <= 2) // 3時間欲しい場合 2am までは昨日の分も。
            {
                Debug.WriteLine("昨日の1min取得開始");

                await Task.Delay(200);

                // 昨日
                DateTime dtTarget = dtToday.AddDays(-1);

                List<Ohlcv> res = await GetCandlestick(pair, CandleTypes.OneMin, dtTarget);
                if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return false;
                if (res != null)
                {
                    // 逆順にする
                    res.Reverse();

                    foreach (var r in res)
                    {
                        ListOhlcvsOneMin.Add(r);
                    }
                }
            }
            else
            {
                //Debug.WriteLine("昨日の1min取得スキップ " + dtToday.Hour.ToString());
            }

        }

        #endregion

        #region == OhlcvsOneDay 1day毎のデータ ==

        List<Ohlcv> ListOhlcvsOneDay = new List<Ohlcv>();

        if (ct == CandleTypes.OneDay)
        {
            // 1日のロウソク足タイプなら今年、去年、２年前、３年前、４年前、５年前の1hourデータを取得する必要あり。(５年前は止めた)

            //Debug.WriteLine("今年のOneDay取得開始 " + pair.ToString());

            ListOhlcvsOneDay = await GetCandlestick(pair, CandleTypes.OneDay, dtToday);
            if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return false;

            if (ListOhlcvsOneDay != null)
            {
                // 逆順にする
                ListOhlcvsOneDay.Reverse();
            }
            else
            {
                ListOhlcvsOneDay = new List<Ohlcv>();
            }

            // BitWallpaperの場合は最大２か月表示なので。
            if (dtToday.Month <= 3) // 2?
            {
                //Debug.WriteLine("去年のOneDay取得開始 " + pair.ToString());

                await Task.Delay(300);
                // 去年
                DateTime dtTarget = dtToday.AddYears(-1);

                List<Ohlcv> res = await GetCandlestick(pair, CandleTypes.OneDay, dtTarget);
                if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return false;

                if (res != null)
                {
                    // 逆順にする
                    res.Reverse();

                    foreach (var r in res)
                    {
                        ListOhlcvsOneDay.Add(r);
                    }

                    /*
                    Debug.WriteLine("一昨年のOneDay取得開始 " + pair.ToString());

                    await Task.Delay(300);
                    // 一昨年
                    dtTarget = dtTarget.AddYears(-1);
                    List<Ohlcv> last = await GetCandlestick(pair, CandleTypes.OneDay, dtTarget);
                    if (last != null)
                    {
                        // 逆順にする
                        last.Reverse();

                        foreach (var l in last)
                        {
                            ListOhlcvsOneDay.Add(l);
                        }


                        // (５年前は止めた)
                    }
                    */

                }

            }

        }

        #endregion

        //ChartLoadingInfo = "";

        if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return false;

        if (ListOhlcvsOneHour != null)
            OhlcvsOneHour = ListOhlcvsOneHour;
        if (ListOhlcvsOneMin != null)
            OhlcvsOneMin = ListOhlcvsOneMin;
        if (ListOhlcvsOneDay != null)
            OhlcvsOneDay = ListOhlcvsOneDay;
        /*
        if (pair == PairCodes.btc_jpy)
        {
            if (ListOhlcvsOneHour != null)
                OhlcvsOneHourBtc = ListOhlcvsOneHour;
            if (ListOhlcvsOneMin != null)
                OhlcvsOneMinBtc = ListOhlcvsOneMin;
            if (ListOhlcvsOneDay != null)
                OhlcvsOneDayBtc = ListOhlcvsOneDay;
        }
        else if (pair == PairCodes.xrp_jpy)
        {
            if (ListOhlcvsOneHour != null)
                OhlcvsOneHourXrp = ListOhlcvsOneHour;
            if (ListOhlcvsOneMin != null)
                OhlcvsOneMinXrp = ListOhlcvsOneMin;
            if (ListOhlcvsOneDay != null)
                OhlcvsOneDayXrp = ListOhlcvsOneDay;
        }
        else if (pair == PairCodes.eth_jpy)
        {
            if (ListOhlcvsOneHour != null)
                OhlcvsOneHourEth = ListOhlcvsOneHour;
            if (ListOhlcvsOneMin != null)
                OhlcvsOneMinEth = ListOhlcvsOneMin;
            if (ListOhlcvsOneDay != null)
                OhlcvsOneDayEth = ListOhlcvsOneDay;
        }
        else if (pair == PairCodes.mona_jpy)
        {
            if (ListOhlcvsOneHour != null)
                OhlcvsOneHourMona = ListOhlcvsOneHour;
            if (ListOhlcvsOneMin != null)
                OhlcvsOneMinMona = ListOhlcvsOneMin;
            if (ListOhlcvsOneDay != null)
                OhlcvsOneDayMona = ListOhlcvsOneDay;
        }
        else if (pair == PairCodes.ltc_jpy)
        {
            if (ListOhlcvsOneHour != null)
                OhlcvsOneHourLtc = ListOhlcvsOneHour;
            if (ListOhlcvsOneMin != null)
                OhlcvsOneMinLtc = ListOhlcvsOneMin;
            if (ListOhlcvsOneDay != null)
                OhlcvsOneDayLtc = ListOhlcvsOneDay;
        }
        else if (pair == PairCodes.bcc_jpy)
        {
            if (ListOhlcvsOneHour != null)
                OhlcvsOneHourBch = ListOhlcvsOneHour;
            if (ListOhlcvsOneMin != null)
                OhlcvsOneMinBch = ListOhlcvsOneMin;
            if (ListOhlcvsOneDay != null)
                OhlcvsOneDayBch = ListOhlcvsOneDay;
        }
        else if (pair == PairCodes.xlm_jpy)
        {
            if (ListOhlcvsOneHour != null)
                OhlcvsOneHourXlm = ListOhlcvsOneHour;
            if (ListOhlcvsOneMin != null)
                OhlcvsOneMinXlm = ListOhlcvsOneMin;
            if (ListOhlcvsOneDay != null)
                OhlcvsOneDayXlm = ListOhlcvsOneDay;
        }
        else if (pair == PairCodes.qtum_jpy)
        {
            if (ListOhlcvsOneHour != null)
                OhlcvsOneHourQtum = ListOhlcvsOneHour;
            if (ListOhlcvsOneMin != null)
                OhlcvsOneMinQtum = ListOhlcvsOneMin;
            if (ListOhlcvsOneDay != null)
                OhlcvsOneDayQtum = ListOhlcvsOneDay;
        }
        else if (pair == PairCodes.bat_jpy)
        {
            if (ListOhlcvsOneHour != null)
                OhlcvsOneHourBat = ListOhlcvsOneHour;
            if (ListOhlcvsOneMin != null)
                OhlcvsOneMinBat = ListOhlcvsOneMin;
            if (ListOhlcvsOneDay != null)
                OhlcvsOneDayBat = ListOhlcvsOneDay;
        }
        */

        return true;

    }

    // ロウソク足 Candlestick取得メソッド
    private async Task<List<Ohlcv>> GetCandlestick(PairCodes pair, CandleTypes ct, DateTime dtTarget)
    {

        string ctString = "";
        string dtString = "";
        if (ct == CandleTypes.OneMin)
        {
            ctString = "1min";
            dtString = dtTarget.ToString("yyyyMMdd");
        }
        else if (ct == CandleTypes.OneHour)
        {
            ctString = "1hour";
            dtString = dtTarget.ToString("yyyyMMdd");
        }
        else if (ct == CandleTypes.OneDay)
        {
            ctString = "1day";
            dtString = dtTarget.ToString("yyyy");
        }
        else
        {
            throw new System.InvalidOperationException("サポートしてないロウソク足単位");
            //return null;
        }
        // 1min 5min 15min 30min 1hour 4hour 8hour 12hour 1day 1week

        CandlestickResult csr = await _pubCandlestickApi.GetCandlestick(pair.ToString(), ctString, dtString);
        if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return null;

        if (csr != null)
        {
            if (csr.IsSuccess == true)
            {
                if (csr.Candlesticks.Count > 0)
                {
                    // ロウソク足タイプが同じかどうか一応確認
                    if (csr.Candlesticks[0].Type.ToString() == ct.ToString())
                    {
                        return csr.Candlesticks[0].Ohlcvs;

                    }
                }
            }
            else
            {

                Debug.WriteLine("■■■■■ GetCandlestick: GetCandlestick returned error");
            }
        }
        else
        {

            Debug.WriteLine("■■■■■ GetCandlestick: GetCandlestick returned null");
        }

        return null;
    }

    // タイマーで、最新のロウソク足データを取得して追加する。
    private async void UpdateCandlestick(PairCodes pair, CandleTypes ct)
    {
        //ChartLoadingInfo = "チャートデータの更新中....";
        //await Task.Delay(600);

        // 今日の日付セット。UTCで。
        DateTime dtToday = DateTime.Now.ToUniversalTime();

        DateTime dtLastUpdate;


        List<Ohlcv> ListOhlcvsOneMin = OhlcvsOneMin;
        List<Ohlcv> ListOhlcvsOneHour = OhlcvsOneHour;
        List<Ohlcv> ListOhlcvsOneDay = OhlcvsOneDay;


        /*
        if (pair == PairCodes.btc_jpy)
        {
            ListOhlcvsOneHour = OhlcvsOneHourBtc;
            ListOhlcvsOneMin = OhlcvsOneMinBtc;
            ListOhlcvsOneDay = OhlcvsOneDayBtc;
        }
        else if (pair == PairCodes.xrp_jpy)
        {
            ListOhlcvsOneHour = OhlcvsOneHourXrp;
            ListOhlcvsOneMin = OhlcvsOneMinXrp;
            ListOhlcvsOneDay = OhlcvsOneDayXrp;
        }
        else if (pair == PairCodes.eth_jpy)
        {
            ListOhlcvsOneHour = OhlcvsOneHourEth;
            ListOhlcvsOneMin = OhlcvsOneMinEth;
            ListOhlcvsOneDay = OhlcvsOneDayEth;
        }
        else if (pair == PairCodes.mona_jpy)
        {
            ListOhlcvsOneHour = OhlcvsOneHourMona;
            ListOhlcvsOneMin = OhlcvsOneMinMona;
            ListOhlcvsOneDay = OhlcvsOneDayMona;
        }
        else if (pair == PairCodes.ltc_jpy)
        {
            ListOhlcvsOneHour = OhlcvsOneHourLtc;
            ListOhlcvsOneMin = OhlcvsOneMinLtc;
            ListOhlcvsOneDay = OhlcvsOneDayLtc;
        }
        else if (pair == PairCodes.bcc_jpy)
        {
            ListOhlcvsOneHour = OhlcvsOneHourBch;
            ListOhlcvsOneMin = OhlcvsOneMinBch;
            ListOhlcvsOneDay = OhlcvsOneDayBch;
        }
        else if (pair == PairCodes.xlm_jpy)
        {
            ListOhlcvsOneHour = OhlcvsOneHourXlm;
            ListOhlcvsOneMin = OhlcvsOneMinXlm;
            ListOhlcvsOneDay = OhlcvsOneDayXlm;
        }
        else if (pair == PairCodes.qtum_jpy)
        {
            ListOhlcvsOneHour = OhlcvsOneHourQtum;
            ListOhlcvsOneMin = OhlcvsOneMinQtum;
            ListOhlcvsOneDay = OhlcvsOneDayQtum;
        }
        else if (pair == PairCodes.bat_jpy)
        {
            ListOhlcvsOneHour = OhlcvsOneHourBat;
            ListOhlcvsOneMin = OhlcvsOneMinBat;
            ListOhlcvsOneDay = OhlcvsOneDayBat;
        }
        */

        #region == １分毎のデータ ==

        if (ct == CandleTypes.OneMin)
        {
            if (ListOhlcvsOneMin.Count > 0)
            {
                dtLastUpdate = ListOhlcvsOneMin[0].TimeStamp;

                //Debug.WriteLine(dtLastUpdate.ToString());

                List<Ohlcv> latestOneMin = new List<Ohlcv>();

                latestOneMin = await GetCandlestick(pair, CandleTypes.OneMin, dtToday);

                if (latestOneMin != null)
                {
                    //latestOneMin.Reverse();

                    if (latestOneMin.Count > 0)
                    {
                        foreach (var hoge in latestOneMin)
                        {

                            //Debug.WriteLine(hoge.TimeStamp.ToString()+" : "+ dtLastUpdate.ToString());

                            if (hoge.TimeStamp >= dtLastUpdate)
                            {

                                // 全てのポイントが同じ場合、スキップする。変なデータ？ 本家もスキップしている。
                                if ((hoge.Open == hoge.High) && (hoge.Open == hoge.Low) && (hoge.Open == hoge.Close) && (hoge.Volume == 0))
                                {
                                    Debug.WriteLine("■ UpdateCandlestick 全てのポイントが同じ " + pair.ToString());
                                    //continue;
                                }

                                if (hoge.TimeStamp == dtLastUpdate)
                                {
                                    // 更新前の最後のポイントを更新する。最終データは中途半端なので。

                                    Debug.WriteLine("１分毎のチャートデータ更新: " + hoge.TimeStamp.ToString() + " " + pair.ToString());

                                    ListOhlcvsOneMin[0].Open = hoge.Open;
                                    ListOhlcvsOneMin[0].High = hoge.High;
                                    ListOhlcvsOneMin[0].Low = hoge.Low;
                                    ListOhlcvsOneMin[0].Close = hoge.Close;
                                    ListOhlcvsOneMin[0].TimeStamp = hoge.TimeStamp;

                                    UpdateLastCandle(pair, CandleTypes.OneMin, hoge);

                                    //Debug.WriteLine(hoge.TimeStamp.ToString()+" : "+ dtLastUpdate.ToString());
                                }
                                else
                                {
                                    // 新規ポイントを追加する。

                                    Debug.WriteLine("１分毎のチャートデータ追加: " + hoge.TimeStamp.ToString() + " " + pair.ToString());

                                    ListOhlcvsOneMin.Insert(0, hoge);

                                    AddCandle(pair, CandleTypes.OneMin, hoge);

                                    dtLastUpdate = hoge.TimeStamp;
                                }


                            }

                        }


                    }

                }
            }
        }

        #endregion

        #region == １時間毎のデータ ==

        if (ct == CandleTypes.OneHour)
        {
            if (ListOhlcvsOneHour.Count > 0)
            {
                dtLastUpdate = ListOhlcvsOneHour[0].TimeStamp;

                //TimeSpan ts = dtLastUpdate - dtToday;

                //if (ts.TotalHours >= 1)
                //{
                //Debug.WriteLine(dtLastUpdate.ToString());

                List<Ohlcv> latestOneHour = new List<Ohlcv>();

                latestOneHour = await GetCandlestick(pair, CandleTypes.OneHour, dtToday);

                if (latestOneHour != null)
                {
                    //latestOneMin.Reverse();

                    if (latestOneHour.Count > 0)
                    {
                        foreach (var hoge in latestOneHour)
                        {

                            // 全てのポイントが同じ場合、スキップする。変なデータ？ 本家もスキップしている。
                            if ((hoge.Open == hoge.High) && (hoge.Open == hoge.Low) && (hoge.Open == hoge.Close) && (hoge.Volume == 0))
                            {
                                Debug.WriteLine("■ UpdateCandlestick 全てのポイントが同じ " + pair.ToString());
                                //continue;
                            }

                            //Debug.WriteLine(hoge.TimeStamp.ToString()+" : "+ dtLastUpdate.ToString());

                            if (hoge.TimeStamp >= dtLastUpdate)
                            {

                                if (hoge.TimeStamp == dtLastUpdate)
                                {
                                    // 更新前の最後のポイントを更新する。最終データは中途半端なので。

                                    Debug.WriteLine("１時間チャートデータ更新: " + hoge.TimeStamp.ToString() + " " + pair.ToString());

                                    ListOhlcvsOneHour[0].Open = hoge.Open;
                                    ListOhlcvsOneHour[0].High = hoge.High;
                                    ListOhlcvsOneHour[0].Low = hoge.Low;
                                    ListOhlcvsOneHour[0].Close = hoge.Close;
                                    ListOhlcvsOneHour[0].TimeStamp = hoge.TimeStamp;

                                    UpdateLastCandle(pair, CandleTypes.OneHour, hoge);

                                    //Debug.WriteLine(hoge.TimeStamp.ToString() + " : " + dtLastUpdate.ToString());
                                }
                                else
                                {
                                    // 新規ポイントを追加する。

                                    Debug.WriteLine("１時間チャートデータ追加: " + hoge.TimeStamp.ToString());

                                    ListOhlcvsOneHour.Insert(0, hoge);

                                    AddCandle(pair, CandleTypes.OneHour, hoge);

                                    dtLastUpdate = hoge.TimeStamp;
                                }

                            }

                        }

                    }

                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("■■■■■ UpdateCandlestick GetCandlestick One hour returned null " + pair.ToString());
                }


                //}

            }

        }

        #endregion

        #region == １日毎のデータ ==

        if (ct == CandleTypes.OneDay)
        {
            if (ListOhlcvsOneDay.Count > 0)
            {
                dtLastUpdate = ListOhlcvsOneDay[0].TimeStamp;

                //TimeSpan ts = dtLastUpdate - dtToday;

                //if (ts.TotalDays >= 1)
                //{
                //Debug.WriteLine(dtLastUpdate.ToString());

                List<Ohlcv> latestOneDay = new List<Ohlcv>();

                latestOneDay = await GetCandlestick(pair, CandleTypes.OneDay, dtToday);

                if (latestOneDay != null)
                {
                    //latestOneMin.Reverse();

                    if (latestOneDay.Count > 0)
                    {
                        foreach (var hoge in latestOneDay)
                        {

                            // 全てのポイントが同じ場合、スキップする。変なデータ？ 本家もスキップしている。
                            if ((hoge.Open == hoge.High) && (hoge.Open == hoge.Low) && (hoge.Open == hoge.Close) && (hoge.Volume == 0))
                            {
                                //continue;
                            }

                            //Debug.WriteLine(hoge.TimeStamp.ToString()+" : "+ dtLastUpdate.ToString());

                            if (hoge.TimeStamp >= dtLastUpdate)
                            {

                                if (hoge.TimeStamp == dtLastUpdate)
                                {
                                    // 更新前の最後のポイントを更新する。最終データは中途半端なので。

                                    Debug.WriteLine("１日チャートデータ更新: " + hoge.TimeStamp.ToString());

                                    ListOhlcvsOneDay[0].Open = hoge.Open;
                                    ListOhlcvsOneDay[0].High = hoge.High;
                                    ListOhlcvsOneDay[0].Low = hoge.Low;
                                    ListOhlcvsOneDay[0].Close = hoge.Close;
                                    ListOhlcvsOneDay[0].TimeStamp = hoge.TimeStamp;

                                    UpdateLastCandle(pair, CandleTypes.OneDay, hoge);

                                    //Debug.WriteLine(hoge.TimeStamp.ToString() + " : " + dtLastUpdate.ToString());
                                }
                                else
                                {
                                    // 新規ポイントを追加する。

                                    Debug.WriteLine("１日チャートデータ追加: " + hoge.TimeStamp.ToString());

                                    ListOhlcvsOneDay.Insert(0, hoge);

                                    AddCandle(pair, CandleTypes.OneDay, hoge);

                                    dtLastUpdate = hoge.TimeStamp;
                                }

                            }

                        }

                    }

                }


                //}

            }
        }

        #endregion

    }

    // チャートの最後に最新ポイントを追加して更新表示する。
    private void AddCandle(PairCodes pair, CandleTypes ct, Ohlcv newData)
    {
        // 表示されているのだけ更新。それ以外は不要。
        //if (SelectedCandleType != ct) return;

        //Debug.WriteLine("チャートの更新 追加: "+ newData.TimeStamp.ToString());

        /*
        SeriesCollection chartSeries = null;
        AxesCollection chartAxisX = null;
        AxesCollection chartAxisY = null;

        if (pair == PairCodes.btc_jpy)
        {
            chartSeries = ChartSeriesBtcJpy;
            chartAxisX = ChartAxisXBtcJpy;
            chartAxisY = ChartAxisYBtcJpy;
        }
        else if (pair == PairCodes.xrp_jpy)
        {
            chartSeries = ChartSeriesXrpJpy;
            chartAxisX = ChartAxisXXrpJpy;
            chartAxisY = ChartAxisYXrpJpy;
        }
        else if (pair == PairCodes.eth_jpy)
        {
            chartSeries = ChartSeriesEthJpy;
            chartAxisX = ChartAxisXEthJpy;
            chartAxisY = ChartAxisYEthJpy;
        }
        else if (pair == PairCodes.mona_jpy)
        {
            chartSeries = ChartSeriesMonaJpy;
            chartAxisX = ChartAxisXMonaJpy;
            chartAxisY = ChartAxisYMonaJpy;
        }
        else if (pair == PairCodes.ltc_jpy)
        {
            chartSeries = ChartSeriesLtcJpy;
            chartAxisX = ChartAxisXLtcJpy;
            chartAxisY = ChartAxisYLtcJpy;
        }
        else if (pair == PairCodes.bcc_jpy)
        {
            chartSeries = ChartSeriesBchJpy;
            chartAxisX = ChartAxisXBchJpy;
            chartAxisY = ChartAxisYBchJpy;
        }
        else if (pair == PairCodes.xlm_jpy)
        {
            chartSeries = ChartSeriesXlmJpy;
            chartAxisX = ChartAxisXXlmJpy;
            chartAxisY = ChartAxisYXlmJpy;
        }
        else if (pair == PairCodes.qtum_jpy)
        {
            chartSeries = ChartSeriesQtumJpy;
            chartAxisX = ChartAxisXQtumJpy;
            chartAxisY = ChartAxisYQtumJpy;
        }
        else if (pair == PairCodes.bat_jpy)
        {
            chartSeries = ChartSeriesBatJpy;
            chartAxisX = ChartAxisXBatJpy;
            chartAxisY = ChartAxisYBatJpy;
        }

        if (chartSeries == null)
            return;

        if (chartSeries[0].Values != null)
        {
            if (chartSeries[0].Values.Count > 0)
            {
                if (Application.Current == null) return;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        // 一番古いの削除
                        chartSeries[0].Values.RemoveAt(0);
                        chartSeries[1].Values.RemoveAt(0);
                        chartAxisX[0].Labels.RemoveAt(0);

                        // ポイント作成
                        OhlcPoint p = new OhlcPoint((double)newData.Open, (double)newData.High, (double)newData.Low, (double)newData.Close);

                        // ポイント追加
                        chartSeries[0].Values.Add(p);

                        // 出来高追加
                        chartSeries[1].Values.Add((double)newData.Volume);

                        // ラベル追加
                        if (ct == CandleTypes.OneMin)
                        {
                            chartAxisX[0].Labels.Add(newData.TimeStamp.ToString("HH:mm"));
                        }
                        else if (ct == CandleTypes.OneHour)
                        {
                            chartAxisX[0].Labels.Add(newData.TimeStamp.ToString("dd日 HH:mm"));

                        }
                        else if (ct == CandleTypes.OneDay)
                        {
                            chartAxisX[0].Labels.Add(newData.TimeStamp.ToString("MM月dd日"));
                        }
                        else
                        {
                            throw new System.InvalidOperationException("UpdateChart: 不正な CandleTypes");
                        }

                    }
                    catch (Exception ex)
                    {

                        ChartLoadingInfo = pair.ToString() + " チャートの追加中にエラーが発生しました 1 ";

                        Debug.WriteLine("■■■■■ " + pair.ToString() + " Chart adding error: " + ex.ToString());
                    }

                }, DispatcherPriority.Normal);

            }
        }
        */
    }

    // チャートの最後のポイントを最新情報に更新表示する。
    private void UpdateLastCandle(PairCodes pair, CandleTypes ct, Ohlcv newData)
    {
        // 表示されているのだけ更新。それ以外は不要。
        //if (SelectedCandleType != ct) return;

        //Debug.WriteLine("チャートの更新 追加: "+ newData.TimeStamp.ToString());

        /*
        SeriesCollection chartSeries = null;
        AxesCollection chartAxisX = null;
        AxesCollection chartAxisY = null;

        if (pair == PairCodes.btc_jpy)
        {
            chartSeries = ChartSeriesBtcJpy;
            chartAxisX = ChartAxisXBtcJpy;
            chartAxisY = ChartAxisYBtcJpy;
        }
        else if (pair == PairCodes.xrp_jpy)
        {
            chartSeries = ChartSeriesXrpJpy;
            chartAxisX = ChartAxisXXrpJpy;
            chartAxisY = ChartAxisYXrpJpy;
        }
        else if (pair == PairCodes.eth_jpy)
        {
            chartSeries = ChartSeriesEthJpy;
            chartAxisX = ChartAxisXEthJpy;
            chartAxisY = ChartAxisYEthJpy;
        }
        else if (pair == PairCodes.mona_jpy)
        {
            chartSeries = ChartSeriesMonaJpy;
            chartAxisX = ChartAxisXMonaJpy;
            chartAxisY = ChartAxisYMonaJpy;
        }
        else if (pair == PairCodes.ltc_jpy)
        {
            chartSeries = ChartSeriesLtcJpy;
            chartAxisX = ChartAxisXLtcJpy;
            chartAxisY = ChartAxisYLtcJpy;
        }
        else if (pair == PairCodes.bcc_jpy)
        {
            chartSeries = ChartSeriesBchJpy;
            chartAxisX = ChartAxisXBchJpy;
            chartAxisY = ChartAxisYBchJpy;
        }
        else if (pair == PairCodes.xlm_jpy)
        {
            chartSeries = ChartSeriesXlmJpy;
            chartAxisX = ChartAxisXXlmJpy;
            chartAxisY = ChartAxisYXlmJpy;
        }
        else if (pair == PairCodes.qtum_jpy)
        {
            chartSeries = ChartSeriesQtumJpy;
            chartAxisX = ChartAxisXQtumJpy;
            chartAxisY = ChartAxisYQtumJpy;
        }
        else if (pair == PairCodes.bat_jpy)
        {
            chartSeries = ChartSeriesBatJpy;
            chartAxisX = ChartAxisXBatJpy;
            chartAxisY = ChartAxisYBatJpy;
        }

        if (chartSeries == null)
            return;

        if (chartSeries[0].Values != null)
        {
            if (chartSeries[0].Values.Count > 0)
            {
                if (Application.Current == null) return;

                if (Application.Current.Dispatcher.CheckAccess())
                {
                    Debug.WriteLine("Dispatcher.CheckAccess()");
                    ((OhlcPoint)chartSeries[0].Values[chartSeries[0].Values.Count - 1]).Open = (double)newData.Open;
                    ((OhlcPoint)chartSeries[0].Values[chartSeries[0].Values.Count - 1]).High = (double)newData.High;
                    ((OhlcPoint)chartSeries[0].Values[chartSeries[0].Values.Count - 1]).Low = (double)newData.Low;
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ((OhlcPoint)chartSeries[0].Values[chartSeries[0].Values.Count - 1]).Open = (double)newData.Open;
                        ((OhlcPoint)chartSeries[0].Values[chartSeries[0].Values.Count - 1]).High = (double)newData.High;
                        ((OhlcPoint)chartSeries[0].Values[chartSeries[0].Values.Count - 1]).Low = (double)newData.Low;
                        ((OhlcPoint)chartSeries[0].Values[chartSeries[0].Values.Count - 1]).Close = (double)newData.Close;

                    }, DispatcherPriority.Normal);
                }
            }
        }
        */
    }

    #endregion

    #region == 板情報 ==

    // 板情報 取得
    private async Task<bool> GetDepth(PairCodes pair)
    {

        // まとめグルーピング単位 
        decimal unit = DepthGrouping;

        // リスト数 （基本 上売り200、下買い200）
        int half = 200;
        int listCount = (half * 2) + 1;

        if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return false;
        (App.Current as App)?.CurrentDispatcherQueue.TryEnqueue(() =>
        {
            // 初期化
            if (_depth.Count == 0)
            {
                for (int i = 0; i < listCount; i++)
                {
                    Depth dd = new Depth(this._ltpFormstString);
                    dd.DepthPrice = 0;
                    dd.DepthBid = 0;
                    dd.DepthAsk = 0;
                    _depth.Add(dd);
                }
            }
            else
            {
                if (IsDepthGroupingChanged)
                {
                    //グルーピング単位が変わったので、一旦クリアする。

                    for (int i = 0; i < listCount; i++)
                    {
                        Depth dd = _depth[i];//new Depth();
                        dd.DepthPrice = 0;
                        dd.DepthBid = 0;
                        dd.DepthAsk = 0;
                        //_depth.Add(dd);
                    }

                    IsDepthGroupingChanged = false;
                }
            }
        });


        if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return false;
        (App.Current as App)?.CurrentDispatcherQueue.TryEnqueue(() =>
        {
            // LTP を追加
            //Depth ddd = new Depth();
            _depth[half].DepthPrice = Ltp;
            //_depth[half].DepthBid = 0;
            //_depth[half].DepthAsk = 0;
            _depth[half].IsLTP = true;
            //_depth[half] = ddd;

        });


        if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return false;

        try
        {
            DepthResult dpr = await _pubDepthApi.GetDepth(pair.ToString());

            if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return false;

            if (dpr != null)
            {
                if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return false;
                (App.Current as App)?.CurrentDispatcherQueue.TryEnqueue(() =>
                {
                    if (_depth.Count != 0)
                    {

                        int i = 1;

                        // 100円単位でまとめる
                        // まとめた時の価格
                        decimal c2 = 0;
                        // 100単位ごとにまとめたAsk数量を保持
                        decimal t = 0;
                        // 先送りするAsk
                        decimal d = 0;
                        // 先送りする価格
                        decimal e = 0;

                        // ask をループ
                        foreach (var dp in dpr.DepthAskList)
                        {
                            // まとめ表示On
                            if (unit > 0)
                            {

                                if (c2 == 0) c2 = System.Math.Ceiling(dp.DepthPrice / unit);

                                // 100円単位でまとめる
                                if (System.Math.Ceiling(dp.DepthPrice / unit) == c2)
                                {
                                    t = t + dp.DepthAsk;
                                }
                                else
                                {
                                    //Debug.WriteLine(System.Math.Ceiling(dp.DepthPrice / unit).ToString() + " " + System.Math.Ceiling(c / unit).ToString());

                                    // 一時保存
                                    e = dp.DepthPrice;
                                    dp.DepthPrice = (c2 * unit);

                                    // 一時保存
                                    d = dp.DepthAsk;
                                    dp.DepthAsk = t;

                                    _depth[half - i].DepthAsk = dp.DepthAsk;
                                    _depth[half - i].DepthBid = dp.DepthBid;
                                    _depth[half - i].DepthPrice = dp.DepthPrice;
                                    _depth[half - i].PriceFormat = this._ltpFormstString;

                                    // 今回のAskは先送り
                                    t = d;
                                    // 今回のPriceが基準になる
                                    c2 = System.Math.Ceiling(e / unit);

                                    i++;

                                }

                            }
                            else
                            {
                                //dp.PriceFormat = this._ltpFormstString;
                                //_depth[half - i] = dp;
                                _depth[half - i].DepthPrice = dp.DepthPrice;
                                _depth[half - i].DepthBid = dp.DepthBid;
                                _depth[half - i].DepthAsk = dp.DepthAsk;
                                _depth[half - i].PriceFormat= this._ltpFormstString;

                                i++;
                            }

                        }

                        _depth[half - 1].IsAskBest = true;

                        i = half + 1;

                        // 100円単位でまとめる
                        // まとめた時の価格
                        decimal c = 0;
                        // 100単位ごとにまとめた数量を保持
                        t = 0;
                        // 先送りするBid
                        d = 0;
                        // 先送りする価格
                        e = 0;

                        // bid をループ
                        foreach (var dp in dpr.DepthBidList)
                        {

                            if (unit > 0)
                            {

                                if (c == 0) c = System.Math.Ceiling(dp.DepthPrice / unit);

                                // 100円単位でまとめる
                                if (System.Math.Ceiling(dp.DepthPrice / unit) == c)
                                {
                                    t = t + dp.DepthBid;
                                }
                                else
                                {

                                    // 一時保存
                                    e = dp.DepthPrice;
                                    dp.DepthPrice = (c * unit);

                                    // 一時保存
                                    d = dp.DepthBid;
                                    dp.DepthBid = t;

                                    // 追加
                                    _depth[i].DepthAsk = dp.DepthAsk;
                                    _depth[i].DepthBid = dp.DepthBid;
                                    _depth[i].DepthPrice = dp.DepthPrice;

                                    // 今回のBidは先送り
                                    t = d;
                                    // 今回のPriceが基準になる
                                    c = System.Math.Ceiling(e / unit);

                                    i++;

                                }
                            }
                            else
                            {
                                //dp.PriceFormat = this._ltpFormstString;
                                //_depth[i] = dp;

                                _depth[i].DepthPrice = dp.DepthPrice;
                                _depth[i].DepthBid = dp.DepthBid;
                                _depth[i].DepthAsk = dp.DepthAsk;
                                _depth[i].PriceFormat = this._ltpFormstString;
                                i++;
                            }

                        }

                        _depth[half + 1].IsBidBest = true;

                    }

                });

                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("■■■■■ GetDepth returned null");
                return false;
            }
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine("■■■■■ GetDepth Exception: " + e);
            return false;
        }

    }

    // 板情報の更新ループ
    private async void UpdateDepth()
    {
        if (IsActive)
            await GetDepth(PairCode);
        /*
        while (true)
        {
            if (IsActive == false)
            {
                await Task.Delay(2000);
                continue;
            }
            // 間隔 1/2
            await Task.Delay(600);

            if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return;
            try
            {
                await GetDepth(PairCode);

                if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("■■■■■ UpdateDepth Exception: " + e);
            }

            // 間隔 1/2
            await Task.Delay(600);
        }
        */
    }

    #endregion

    #region == 歩み値 ==

    // トランザクションの取得
    private async Task<bool> GetTransactions(PairCodes pair)
    {
        if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return false;

        try
        {
            TransactionsResult trs = await _pubTransactionsApi.GetTransactions(pair.ToString());

            if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return false;

            if (trs != null)
            {
                //Debug.WriteLine(trs.Trans.Count.ToString());

                if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return false;
                (App.Current as App)?.CurrentDispatcherQueue.TryEnqueue(() =>
                {

                    if (_transactions.Count == 0)
                    {
                        // 60 で初期化
                        for (int i = 0; i < 60; i++)
                        {
                            Transaction dd = new Transaction(_ltpFormstString);
                            //
                            _transactions.Add(dd);
                        }
                    }

                    if (trs.Trans != null)
                    {
                        int v = 0;
                        foreach (var tr in trs.Trans)
                        {
                            //_transactions[v] = tr;

                            _transactions[v].Amount = tr.Amount;
                            _transactions[v].ExecutedAt = tr.ExecutedAt;
                            _transactions[v].Price = tr.Price;
                            _transactions[v].Side = tr.Side;
                            _transactions[v].TransactionId = tr.TransactionId;

                            //_transactions[v].ExecutedAtFormated= tr.ExecutedAtFormated;

                            //Debug.WriteLine(_transactions[v].ExecutedAtFormated);

                            v++;

                            if (v >= 60)
                                break;
                        }
                    }

                });

                /*
                _transactions.Clear();

                foreach (var tr in trs.Trans)
                {
                    _transactions.Add(tr);
                }
                */

                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("■■■■■ GetTransactions returned null");
                return false;
            }
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine("■■■■■ GetTransactions Exception: " + e);
            return false;
        }
    }

    // トランザクションの更新ループ
    private async void UpdateTransactions()
    {
        if (IsActive)
            await GetTransactions(this.PairCode);

        /*
        while (true)
        {
            if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return;

            if (IsActive == false)
            {
                await Task.Delay(2000);
                continue;
            }
            // 間隔 1/2
            await Task.Delay(1300);

            if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return;

            try
            {
                await GetTransactions(this.PairCode);

                if ((App.Current == null) || ((App.Current as App)?.CurrentDispatcherQueue == null)) return;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("■■■■■ UpdateTransactions Exception: " + e);
            }

            // 間隔 1/2
            await Task.Delay(1300);
        }
        */
    }

    #endregion

}
