using BitWallpaper.Helpers.UICommand1;
using BitWallpaper.Models;
using BitWallpaper.Models.APIClients;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BitWallpaper.ViewModels;

public enum PairCodes
{
    btc_jpy, xrp_jpy, eth_jpy, ltc_jpy, bcc_jpy, mona_jpy, xlm_jpy, qtum_jpy, bat_jpy
}

public class PairViewModel : ViewModelBase
{
    #region == Main properties ==

    private PairCodes _p;
    public PairCodes PairCode
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
                {PairCodes.bcc_jpy, "BCC/JPY"},
                {PairCodes.xlm_jpy, "XLM/JPY"},
                {PairCodes.qtum_jpy, "QTUM/JPY"},
                {PairCodes.bat_jpy, "BAT/JPY"},
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
            {PairCodes.bcc_jpy, "BCC"},
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
            if (_ltp == value) return;

            _ltp = value;

            NotifyPropertyChanged("Ltp");
            NotifyPropertyChanged("LtpString");
            /*
            if (_ltp > BasePrice)
            {
                BasePriceUpFlag = true;
            }
            else if (_ltp < BasePrice)
            {
                BasePriceUpFlag = false;
            }
            NotifyPropertyChanged("BasePriceIcon");

            if (_ltp > MiddleInitPrice)
            {
                MiddleInitPriceUpFlag = true;
            }
            else if (_ltp < MiddleInitPrice)
            {
                MiddleInitPriceUpFlag = false;
            }
            NotifyPropertyChanged("MiddleInitPriceIcon");

            if (_ltp > MiddleLast24Price)
            {
                MiddleLast24PriceUpFlag = true;
            }
            else if (_ltp < MiddleLast24Price)
            {
                MiddleLast24PriceUpFlag = false;
            }
            NotifyPropertyChanged("MiddleLast24PriceIcon");

            if (_ltp > AveragePrice)
            {
                AveragePriceUpFlag = true;
            }
            else if (_ltp < AveragePrice)
            {
                AveragePriceUpFlag = false;
            }
            NotifyPropertyChanged("AveragePriceIcon");
            */

            if (IsSelectedActive && IsEnabled) // 
            {
                App.CurrentDispatcherQueue?.TryEnqueue(() =>
                {
                    Sections[0].Yi = (double)_ltp;
                    Sections[0].Yj = (double)_ltp;
                });
                
                // a little hack to update Section...
                App.CurrentDispatcherQueue?.TryEnqueue(() =>
                {
                    if (Series[1].Values == null) return;
                    if (Series[1].Values is not ObservableCollection<FinancialPoint>) return;
                    if ((Series[1].Values as ObservableCollection<FinancialPoint>).Count < 1) return;

                    (Series[1].Values as ObservableCollection<FinancialPoint>)[0].Close =
                        (Series[1].Values as ObservableCollection<FinancialPoint>)[0].Close = (double)_ltp;
                });
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
                return string.Format(_ltpFormstString, _ltp);

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
            NotifyPropertyChanged(nameof(LtpFormstString));
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
            NotifyPropertyChanged("LtpUpFlag");
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
            NotifyPropertyChanged("Bid");
            NotifyPropertyChanged("BidString");

        }
    }
    public string BidString
    {
        get
        {
            return string.Format("{0:#,0}", _bid);
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
            NotifyPropertyChanged("Ask");
            NotifyPropertyChanged("AskString");

        }
    }
    public string AskString
    {
        get
        {
            return string.Format("{0:#,0}", _ask);
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
            NotifyPropertyChanged("TickTimeStamp");
            NotifyPropertyChanged("TickTimeStampString");

        }
    }
    public string TickTimeStampString
    {
        get
        {
            return _tickTimeStamp.ToLocalTime().ToString("G", System.Globalization.CultureInfo.CurrentCulture);//"yyyy/MM/dd HH:mm:ss"
        }
    }

    private ObservableCollection<TickHistory> _tickHistory = new ObservableCollection<TickHistory>();
    public ObservableCollection<TickHistory> TickHistories
    {
        get
        {
            return _tickHistory;
        }
    }

    #endregion

    #region == Flags ==

    private bool _isEnabled = true;
    public bool IsEnabled
    {
        get
        {
            return _isEnabled;
        }
        set
        {
            if (_isEnabled == value)
                return;

            _isEnabled = value;
            NotifyPropertyChanged(nameof(IsEnabled));
        }
    }

    // Selected or not.
    private bool _isSelectedActive;
    public bool IsSelectedActive
    {
        get
        {
            return _isSelectedActive;
        }
        set
        {
            if (_isSelectedActive == value)
                return;

            _isSelectedActive = value;
            NotifyPropertyChanged(nameof(IsSelectedActive));
        }
    }

    private bool _isChartInitAndLoaded;
    public bool IsChartInitAndLoaded
    {
        get => _isChartInitAndLoaded;
        set
        {
            if (_isChartInitAndLoaded == value)
                return;

            _isChartInitAndLoaded = value;
            NotifyPropertyChanged(nameof(IsChartInitAndLoaded));
        }
    }

    #endregion

    #region == Options ==

    private bool _isPaneVisible = true;
    public bool IsPaneVisible
    {
        get
        {
            return _isPaneVisible;
        }
        set
        {
            if (_isPaneVisible == value)
                return;

            _isPaneVisible = value;
            NotifyPropertyChanged(nameof(IsPaneVisible));
        }
    }

    private bool _isChartTooltipVisible = true;
    public bool IsChartTooltipVisible
    {
        get
        {
            return _isChartTooltipVisible;
        }
        set
        {
            if (_isChartTooltipVisible == value)
                return;

            _isChartTooltipVisible = value;
            NotifyPropertyChanged(nameof(IsChartTooltipVisible));

            if (_isChartTooltipVisible)
            {
                IsChartTooltipVisibleTemp = LiveChartsCore.Measure.TooltipPosition.Center;
            }
            else
            {
                IsChartTooltipVisibleTemp = LiveChartsCore.Measure.TooltipPosition.Hidden;
            }
        }
    }

    private LiveChartsCore.Measure.TooltipPosition _isChartTooltipVisibleTemp = LiveChartsCore.Measure.TooltipPosition.Center;
    public LiveChartsCore.Measure.TooltipPosition IsChartTooltipVisibleTemp
    {
        get
        {
            return _isChartTooltipVisibleTemp;
        }
        set
        {
            if (_isChartTooltipVisibleTemp == value)
                return;

            _isChartTooltipVisibleTemp = value;
            NotifyPropertyChanged(nameof(IsChartTooltipVisibleTemp));
        }
    }

    #endregion

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

            if (value != 0)
            {
                if (value <= _ltp)
                    return;
            }

            _alarmPlus = value;
            NotifyPropertyChanged("AlarmPlus");
            NotifyPropertyChanged("AlarmPlusString");
            NotifyPropertyChanged("AlarmLabel");
        }
    }
    public string AlarmPlusString
    {
        get
        {
            return string.Format(_ltpFormstString, AlarmPlus);
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

            if (value != 0)
            {
                if (value >= _ltp)
                    return;
            }

            _alarmMinus = value;
            NotifyPropertyChanged("AlarmMinus");
            NotifyPropertyChanged("AlarmMinusString");
            NotifyPropertyChanged("AlarmLabel");
        }
    }
    public string AlarmMinusString
    {
        get
        {
            return string.Format(_ltpFormstString, AlarmMinus);
        }
    }

    public string AlarmLabel
    {
        get
        {
            if ((AlarmPlus > 0) || (AlarmMinus > 0))
            {
                return "set";
            }
            else
            {
                return "none";
            }
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
            NotifyPropertyChanged("HighLowInfoText");
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
            NotifyPropertyChanged("HighLowInfoTextColorFlag");
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
            NotifyPropertyChanged("PlaySoundLowest");
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
            NotifyPropertyChanged("PlaySoundHighest");
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
            NotifyPropertyChanged("PlaySoundLowest24h");
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
            NotifyPropertyChanged("PlaySoundHighest24h");
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

            NotifyPropertyChanged("BasePrice");
            NotifyPropertyChanged("BasePriceIcon");
            NotifyPropertyChanged("BasePriceString");

        }
    }

    public string BasePriceString
    {
        get
        {
            return string.Format(_ltpFormstString, BasePrice);
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
            NotifyPropertyChanged("BasePriceUpFlag");
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
            NotifyPropertyChanged("AveragePrice");
            NotifyPropertyChanged("AveragePriceIcon");
            //this.NotifyPropertyChanged("AveragePriceIconColor");
            NotifyPropertyChanged("AveragePriceString");
        }
    }

    public string AveragePriceString
    {
        get
        {
            return string.Format(_ltpFormstString, _averagePrice); ;
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
            NotifyPropertyChanged("AveragePriceUpFlag");
        }
    }

    // 過去２４時間の中央値
    public decimal MiddleLast24Price
    {
        get
        {
            return (_lowestIn24Price + _highestIn24Price) / 2;
        }
    }
    public string MiddleLast24PriceString
    {
        get
        {
            return string.Format(_ltpFormstString, MiddleLast24Price); ;
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
            NotifyPropertyChanged("MiddleLast24PriceUpFlag");
        }
    }

    // 起動後の中央値
    public decimal MiddleInitPrice
    {
        get
        {
            return (_lowestPrice + _highestPrice) / 2;
        }
    }
    public string MiddleInitPriceString
    {
        get
        {
            return string.Format(_ltpFormstString, MiddleInitPrice); ;
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
            NotifyPropertyChanged("MiddleInitPriceUpFlag");
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
            NotifyPropertyChanged("HighestIn24Price");
            NotifyPropertyChanged("High24String");

            NotifyPropertyChanged("MiddleLast24Price");
            NotifyPropertyChanged("MiddleLast24PriceString");

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
            return string.Format(_ltpFormstString, _highestIn24Price);
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
            NotifyPropertyChanged("HighestIn24PriceAlart");
            NotifyPropertyChanged("PriceAlart");
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
            NotifyPropertyChanged("LowestIn24Price");
            NotifyPropertyChanged("Low24String");

            NotifyPropertyChanged("MiddleLast24Price");
            NotifyPropertyChanged("MiddleLast24PriceString");

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
            return string.Format(_ltpFormstString, _lowestIn24Price);
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
            NotifyPropertyChanged("LowestIn24PriceAlart");
            NotifyPropertyChanged("PriceAlart");
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
            NotifyPropertyChanged("HighestPrice");
            NotifyPropertyChanged("HighestPriceString");

            NotifyPropertyChanged("MiddleInitPrice");
            NotifyPropertyChanged("MiddleInitPriceString");
            NotifyPropertyChanged("MiddleInitPriceIcon");

            //if (MinMode) return;
            //(ChartSeries[1].Values[0] as ObservableValue).Value = (double)_highestPrice;
        }
    }
    public string HighestPriceString
    {
        get
        {
            return string.Format(_ltpFormstString, _highestPrice); ;
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
            NotifyPropertyChanged("HighestPriceAlart");
            NotifyPropertyChanged("PriceAlart");
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
            NotifyPropertyChanged("LowestPrice");
            NotifyPropertyChanged("LowestPriceString");

            NotifyPropertyChanged("MiddleInitPrice");
            NotifyPropertyChanged("MiddleInitPriceString");
            NotifyPropertyChanged("MiddleInitPriceIcon");

            //if (MinMode) return;
            // (ChartSeries[2].Values[0] as ObservableValue).Value = (double)_lowestPrice;
        }
    }
    public string LowestPriceString
    {
        get
        {
            return string.Format(_ltpFormstString, _lowestPrice); ;
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
            NotifyPropertyChanged("LowestPriceAlart");
            NotifyPropertyChanged("PriceAlart");
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
        get { return _transactions; }
        set
        {
            if (_transactions == value)
                return;

            _transactions = value;
            NotifyPropertyChanged(nameof(Transactions));
        }
    }

    private ObservableCollection<Depth> _depth = new ObservableCollection<Depth>();
    public ObservableCollection<Depth> Depth
    {
        get { return _depth; }
        set
        {
            if (_depth == value)
                return;

            _depth = value;
            NotifyPropertyChanged(nameof(Depth));
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

            NotifyPropertyChanged("IsDepthGroupingChanged");
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

            NotifyPropertyChanged("DepthGrouping");

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
            NotifyPropertyChanged("IsDepthGroupingOff");
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
            NotifyPropertyChanged("DepthGrouping100");

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
            NotifyPropertyChanged("IsDepthGrouping100");
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
            NotifyPropertyChanged("DepthGrouping1000");

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
            NotifyPropertyChanged("IsDepthGrouping1000");
        }
    }

    #endregion

    #region == チャート ==

    public Section<LiveChartsCore.SkiaSharpView.Drawing.SkiaSharpDrawingContext>[] Sections { get; set; } =
    {
        new RectangularSection
        {
            Yi = 1,
            Yj = 1,
            ScalesYAt = 1,
            Stroke = new SolidColorPaint
            {
                Color = SKColors.Silver,
                StrokeThickness = 1,
                //PathEffect = new DashEffect(new float[] { 6, 6 })
            }
        }
    };

    public ICartesianAxis[] XAxes {get; set;} =
    {
        new Axis()
        {
            LabelsRotation = -15,
            //LabelsPaint = new SolidColorPaint(SKColors.Wheat),
            Labeler = value => new DateTime((long) value).ToString("MM/dd"), //TODO: localize aware
            UnitWidth = TimeSpan.FromHours(0.5).Ticks,
            MinStep = TimeSpan.FromDays(1).Ticks,
            MaxLimit = null,
            MinLimit= DateTime.Now.Ticks - TimeSpan.FromDays(2.8).Ticks,
            SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 1,PathEffect = new DashEffect(new float[] { 3, 3 }) }
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
                MinLimit=0,
            },
        new Axis()
            {
                LabelsRotation = 0,
                //LabelsPaint = new SolidColorPaint(SKColors.Wheat),
                Position = LiveChartsCore.Measure.AxisPosition.End,
                SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
                            {
                                StrokeThickness = 1,
                                PathEffect = new DashEffect(new float[] { 3, 3 })
                            },
                //Labeler = Labelers.Currency,
                //Labeler = (value) => value.ToString("C", new System.Globalization.CultureInfo("ja-Jp")),
                Labeler = (value) => value.ToString("N", new CultureInfo("ja-Jp")),
            }
    };

    private ISeries[] _series =
    {
        new ColumnSeries<DateTimePoint>
        {
            Name = "Depth",
            ScalesYAt = 0,
            //Stroke = new SolidColorPaint((new SKColor(198, 167, 0)), 0),
            Fill =  new SolidColorPaint(new SKColor(127, 127, 127), 1),
            TooltipLabelFormatter = (chartPoint) =>
                $"Depth, {new DateTime((long) chartPoint.SecondaryValue):yyy/MM/dd HH}: {chartPoint.PrimaryValue}",
            Values = new ObservableCollection<DateTimePoint>
            {
                //new DateTimePoint(DateTime.Now, 1)
            }
        },
        new CandlesticksSeries<FinancialPoint>
        {
            Name = "Price",
            ScalesYAt = 1,
            //TooltipLabelFormatter = (chartPoint) => $"Price: {new DateTime((long) chartPoint.SecondaryValue):yyy/MM/dd HH}: {chartPoint.PrimaryValue}",
            Values = new ObservableCollection<FinancialPoint>
            {
                //new(DateTime.Now, 100, 0, 0, 0)
            }
        }
    };
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
    }

    TimeSpan chartUpdateInterval = new TimeSpan(1, 0, 0);

    private CandleTypes _selectedCandleType = CandleTypes.OneHour;
    public CandleTypes SelectedCandleType
    {
        get => _selectedCandleType;
        set
        {
            if (_selectedCandleType == value)
                return;

            _selectedCandleType = value;
            NotifyPropertyChanged(nameof(SelectedCandleType));
            NotifyPropertyChanged(nameof(SelectedCandleTypeLabelString));

            if (_selectedCandleType == CandleTypes.OneMin)
                chartUpdateInterval = new TimeSpan(0, 1, 0);
            else if (_selectedCandleType == CandleTypes.FiveMin)
                chartUpdateInterval = new TimeSpan(0, 5, 0);
            else if (_selectedCandleType == CandleTypes.FifteenMin)
                chartUpdateInterval = new TimeSpan(0, 15, 0);
            else if (_selectedCandleType == CandleTypes.ThirtyMin)
                chartUpdateInterval = new TimeSpan(0, 30, 0);
            else if (_selectedCandleType == CandleTypes.OneHour)
                chartUpdateInterval = new TimeSpan(1, 0, 0);
            else if (_selectedCandleType == CandleTypes.FourHour)
                chartUpdateInterval = new TimeSpan(4, 0, 0);
            else if (_selectedCandleType == CandleTypes.EightHour)
                chartUpdateInterval = new TimeSpan(8, 0, 0);
            else if (_selectedCandleType == CandleTypes.TwelveHour)
                chartUpdateInterval = new TimeSpan(12, 0, 0);
            else if (_selectedCandleType == CandleTypes.OneDay)
                chartUpdateInterval = new TimeSpan(24, 0, 0);
            else if (_selectedCandleType == CandleTypes.OneWeek)
                chartUpdateInterval = new TimeSpan(168, 0, 0);
            else if (_selectedCandleType == CandleTypes.OneMonth)
                chartUpdateInterval = new TimeSpan(720, 0, 0);

            _dispatcherTimerChart.Stop();
            _dispatcherTimerChart.Interval = chartUpdateInterval;
            _dispatcherTimerChart.Start();
        }
    }

    public string SelectedCandleTypeLabelString
    {
        get
        {
            var ct = _selectedCandleType;
            string candleTypeText = "";
            if (ct == CandleTypes.OneMin)
            {
                candleTypeText = "1 min";//"１分";
            }
            else if (ct == CandleTypes.FiveMin)
            {
                candleTypeText = "5 min";//"５分";
            }
            else if (ct == CandleTypes.FifteenMin)
            {
                candleTypeText = "15 min";//"１５分";
            }
            else if (ct == CandleTypes.ThirtyMin)
            {
                candleTypeText = "30 min";//"３０分";
            }
            else if (ct == CandleTypes.OneHour)
            {
                candleTypeText = "1 hour";//"１時間";
            }
            else if (ct == CandleTypes.FourHour)
            {
                candleTypeText = "4 hours";//"４時間";
            }
            else if (ct == CandleTypes.EightHour)
            {
                candleTypeText = "8 hours";//"８時間";
            }
            else if (ct == CandleTypes.TwelveHour)
            {
                candleTypeText = "12 hours";//"１２時間";
            }
            else if (ct == CandleTypes.OneDay)
            {
                candleTypeText = "1 day";//"１日";
            }
            else if (ct == CandleTypes.OneWeek)
            {
                candleTypeText = "1 week";//"１週間";
            }
            else if (ct == CandleTypes.OneMonth)
            {
                candleTypeText = "1 month";//"１ヵ月";
            }

            return candleTypeText;
            //return $"ロウソク足の選択（{candleTypeText}）";
        }
    }

    #endregion

    #region == HTTP Clients ==

    // HTTP Clients
    readonly PublicAPIClient _pubCandlestickApi = new();
    readonly PublicAPIClient _pubTransactionsApi = new();
    readonly PublicAPIClient _pubDepthApi = new();

    #endregion

    #region == Timers ==
    // Timer
    readonly DispatcherTimer _dispatcherTimerChart = new();
    readonly DispatcherTimer _dispatcherTimerDepth = new();
    readonly DispatcherTimer _dispatcherTimerTransaction = new();
    #endregion

    private DateTime lastChartLoadedDateTime= DateTime.MinValue;

    // 
    public PairViewModel(PairCodes p, double fontSize, string ltpFormstString, string currencyFormstString, decimal grouping100, decimal grouping1000)
    {
        _p = p;
        _ltpFontSize = fontSize;
        _ltpFormstString = ltpFormstString;
        _currencyFormstString = currencyFormstString;

        _depthGrouping100 = grouping100;
        _depthGrouping1000 = grouping1000;

        #region == RelayCommands ==

        //TogglePaneVisibilityCommand = new RelayCommand(new Action(TogglePaneVisibility), CanExecuteTogglePaneVisibilityCommand);
        TogglePaneVisibilityCommand = new RelayCommand(TogglePaneVisibilityCommand_Execute, TogglePaneVisibilityCommand_CanExecute);
        ChangeCandleTypeCommand = new GenericRelayCommand<CandleTypes>(param => ChangeCandleTypeCommand_Execute(param), param => ChangeCandleTypeCommand_CanExecute());
        TogglePairVisibilityCommand = new GenericRelayCommand<PairCodes>(param => TogglePairVisibilityCommand_Execute(param), param => TogglePairVisibilityCommand_CanExecute());

        #endregion

        #region == Timers ==

        // Depth update timer
        _dispatcherTimerDepth.Tick += TickerTimerDepth;
        _dispatcherTimerDepth.Interval = new TimeSpan(0, 0, 2);
        _dispatcherTimerDepth.Start();

        // Transaction update timer
        _dispatcherTimerTransaction.Tick += TickerTimerTransaction;
        _dispatcherTimerTransaction.Interval = new TimeSpan(0, 0, 3);
        _dispatcherTimerTransaction.Start();

        // Chart update timer
        _dispatcherTimerChart.Tick += TickerTimerChart;
        _dispatcherTimerChart.Interval = chartUpdateInterval;
        _dispatcherTimerChart.Start();

        #endregion

    }

    // Called from MainViewModel.
    public void InitializeAndLoad()
    {
        if (IsChartInitAndLoaded) 
        {
            if (lastChartLoadedDateTime.Add(chartUpdateInterval) < DateTime.Now)
            {
                LoadChart(SelectedCandleType);
                _dispatcherTimerChart.Stop();
                _dispatcherTimerChart.Start();
            }
        }
        else
        {
            LoadChart(SelectedCandleType);
        }

        /*
        Task.Run(async () =>
        {

        });
        */
        /*
        List<Ohlcv> res = await GetCandlesticks(this.PairCode, SelectedCandleType);

        if (res == null) return;

        if (res.Count > 0)
        {
            LoadChart(res, SelectedCandleType);

            Sections[0].Yi = (double)_ltp;
            Sections[0].Yj = (double)_ltp;
        }
        */
    }

    public void CleanUp()
    {
        try
        {
            _dispatcherTimerChart.Stop();
            _dispatcherTimerDepth.Stop();
            _dispatcherTimerTransaction.Stop();

            _pubCandlestickApi.Dispose();
            _pubTransactionsApi.Dispose();
            _pubDepthApi.Dispose();
        }
        catch(Exception ex)
        {
            Debug.WriteLine("Error while Shutdown() : " + ex);
        }
    }

    #region == チャート ==

    private void TickerTimerChart(object source, object e)
    {
        UpdateChart();
    }

    private void UpdateChart()
    {
        if (!IsChartInitAndLoaded) return;
        if (!IsEnabled) return;
        if (!IsSelectedActive) return;

        /*
        Task.Run(async () =>
        {

        });
        */
        /*
        List<Ohlcv> res = await GetCandlesticks(this.PairCode, SelectedCandleType);

        if (res == null) return;

        if (res.Count > 0)
        {
            LoadChart(res, SelectedCandleType);

            Sections[0].Yi = (double)_ltp;
            Sections[0].Yj = (double)_ltp;
        }
        */

        LoadChart(SelectedCandleType);
    }

    private void ChangeCandleType(CandleTypes candleType)
    {
        if (candleType == SelectedCandleType) return;

        // set new candle type
        SelectedCandleType = candleType;

        // clear chart data.
        Series[0].Values = new ObservableCollection<DateTimePoint>
        {
            //new DateTimePoint(DateTime.Now, 1)
        };
        Series[1].Values = new ObservableCollection<FinancialPoint>
        {
            //new(DateTime.Now, 100, 0, 0, 0)
        };

        LoadChart(SelectedCandleType);
    }

    private async void LoadChart(CandleTypes ct)
    {        
        // gets new data.
        List<Ohlcv> res = await GetCandlesticks(this.PairCode, ct);

        if (res == null) return;

        if (res.Count > 0)
        {
            DoLoadChart(res, ct);

            Sections[0].Yi = (double)_ltp;
            Sections[0].Yj = (double)_ltp;
        }
    }

    private void DoLoadChart(List<Ohlcv> list, CandleTypes ct)
    {
        //Debug.WriteLine("DoLoadChart: " + this.PairCode + ", " + ct.ToString());

        // Need to be here. not static and all.
        if (_currencyFormstString.Equals("C3"))
        {
            YAxes[1].Labeler = (value) => value.ToString("C3", new CultureInfo("ja-Jp"));
        }
        else if (_currencyFormstString.Equals("C2"))
        {
            YAxes[1].Labeler = (value) => value.ToString("C2", new CultureInfo("ja-Jp"));
        }
        else if (_currencyFormstString.Equals("C1"))
        {
            YAxes[1].Labeler = (value) => value.ToString("C1", new CultureInfo("ja-Jp"));
        }
        else
        {
            YAxes[1].Labeler = (value) => value.ToString("C", new CultureInfo("ja-Jp"));
        }

        //TODO: localize aware
        // キャンドルタイプにあわせてチャートXAxes[0]表示tweak
        if (ct == CandleTypes.OneMin)
        {
            XAxes[0].Labeler = value => new DateTime((long)value).ToString("HH:mm");
            XAxes[0].UnitWidth = TimeSpan.FromMinutes(0.4).Ticks;
            XAxes[0].MinStep = TimeSpan.FromMinutes(1).Ticks;
            XAxes[0].MinLimit = DateTime.Now.Ticks - TimeSpan.FromMinutes(60).Ticks;
        }
        else if (ct == CandleTypes.FiveMin)
        {
            XAxes[0].Labeler = value => new DateTime((long)value).ToString("HH:mm");
            XAxes[0].UnitWidth = TimeSpan.FromMinutes(2.5).Ticks;
            XAxes[0].MinStep = TimeSpan.FromMinutes(5).Ticks;
            XAxes[0].MinLimit = DateTime.Now.Ticks - TimeSpan.FromMinutes(300).Ticks;
        }
        else if (ct == CandleTypes.FifteenMin)
        {
            XAxes[0].Labeler = value => new DateTime((long)value).ToString("MM/dd HH:mm");
            XAxes[0].UnitWidth = TimeSpan.FromMinutes(7).Ticks;
            XAxes[0].MinStep = TimeSpan.FromMinutes(15).Ticks;
            XAxes[0].MinLimit = DateTime.Now.Ticks - TimeSpan.FromMinutes(750).Ticks;
        }
        else if (ct == CandleTypes.ThirtyMin)
        {
            XAxes[0].Labeler = value => new DateTime((long)value).ToString("MM/dd HH:mm");
            XAxes[0].UnitWidth = TimeSpan.FromMinutes(15).Ticks;
            XAxes[0].MinStep = TimeSpan.FromMinutes(30).Ticks;
            XAxes[0].MinLimit = DateTime.Now.Ticks - TimeSpan.FromMinutes(1500).Ticks;
        }
        else if (ct== CandleTypes.OneHour)
        {
            XAxes[0].Labeler = value => new DateTime((long)value).ToString("MM/dd HH");
            XAxes[0].UnitWidth = TimeSpan.FromHours(0.5).Ticks;
            XAxes[0].MinStep = TimeSpan.FromHours(1).Ticks;
            XAxes[0].MinLimit = DateTime.Now.Ticks - TimeSpan.FromDays(3).Ticks;
        }
        else if (ct == CandleTypes.FourHour)
        {
            XAxes[0].Labeler = value => new DateTime((long)value).ToString("MM/dd HH");
            XAxes[0].UnitWidth = TimeSpan.FromHours(2).Ticks;
            XAxes[0].MinStep = TimeSpan.FromHours(4).Ticks;
            XAxes[0].MinLimit = DateTime.Now.Ticks - TimeSpan.FromDays(6).Ticks;
        }
        else if (ct == CandleTypes.EightHour)
        {
            XAxes[0].Labeler = value => new DateTime((long)value).ToString("MM/dd HH");
            XAxes[0].UnitWidth = TimeSpan.FromHours(4).Ticks;
            XAxes[0].MinStep = TimeSpan.FromHours(8).Ticks;
            XAxes[0].MinLimit = DateTime.Now.Ticks - TimeSpan.FromDays(12).Ticks;
        }
        else if (ct == CandleTypes.TwelveHour)
        {
            XAxes[0].Labeler = value => new DateTime((long)value).ToString("yyyy MM/dd");
            XAxes[0].UnitWidth = TimeSpan.FromHours(6).Ticks;
            XAxes[0].MinStep = TimeSpan.FromDays(0.5).Ticks;
            XAxes[0].MinLimit = DateTime.Now.Ticks - TimeSpan.FromDays(24).Ticks;
        }
        else if (ct == CandleTypes.OneDay)
        {
            XAxes[0].Labeler = value => new DateTime((long)value).ToString("yyyy MM/dd");
            XAxes[0].UnitWidth = TimeSpan.FromDays(0.4).Ticks;
            XAxes[0].MinStep = TimeSpan.FromDays(1).Ticks;
            XAxes[0].MinLimit = DateTime.Now.Ticks - TimeSpan.FromDays(90).Ticks;
        }
        else if (ct == CandleTypes.OneWeek)
        {
            XAxes[0].Labeler = value => new DateTime((long)value).ToString("yyyy MM/dd");
            XAxes[0].UnitWidth = TimeSpan.FromDays(2).Ticks;
            XAxes[0].MinStep = TimeSpan.FromDays(1).Ticks;
            XAxes[0].MinLimit = DateTime.Now.Ticks - TimeSpan.FromDays(300).Ticks;

        }
        else if (ct == CandleTypes.OneMonth)
        {
            XAxes[0].Labeler = value => new DateTime((long)value).ToString("yyyy/MM");
            XAxes[0].UnitWidth = TimeSpan.FromDays(21).Ticks;
            XAxes[0].MinStep = TimeSpan.FromDays(30).Ticks;
            XAxes[0].MinLimit = DateTime.Now.Ticks - TimeSpan.FromDays(360*3).Ticks;
        }

        var ohlcs = new ObservableCollection<FinancialPoint>();
        var vols = new ObservableCollection<DateTimePoint>();

        foreach (var hoge in list)
        {
            vols.Add(new DateTimePoint(hoge.TimeStamp, (double)hoge.Volume));

            ohlcs.Add(new FinancialPoint(hoge.TimeStamp, (double)hoge.High, (double)hoge.Open, (double)hoge.Close, (double)hoge.Low));
        }

        Series[0].Values = vols;
        Series[1].Values = ohlcs;

        IsChartInitAndLoaded = true;

        lastChartLoadedDateTime = DateTime.Now;
    }

    private async Task<List<Ohlcv>> GetCandlesticks(PairCodes pair, CandleTypes ct)
    {
        List<Ohlcv> OhlcvList =  new List<Ohlcv>();

        //Debug.WriteLine("チャートデータを取得中.... " + pair.ToString());

        // 今日の日付セット。UTCで。
        DateTime dtToday = DateTime.Now.ToUniversalTime();

        // データは、ローカルタイムで、朝9:00 から翌8:59分まで。8:59分までしか取れないので、 9:00過ぎていたら 最新のデータとるには日付を１日追加する

        int daysOrYearsCountToGetData = 0;

        bool isYearly = false;

        if (ct == CandleTypes.OneMin)
        {
            daysOrYearsCountToGetData = 2;
        }
        else if (ct == CandleTypes.FiveMin)
        {
            daysOrYearsCountToGetData = 2;
        }
        else if (ct == CandleTypes.FifteenMin)
        {
            daysOrYearsCountToGetData = 4;
        }
        else if (ct == CandleTypes.ThirtyMin)
        {
            daysOrYearsCountToGetData = 4;
        }
        else if (ct == CandleTypes.OneHour)
        {
            daysOrYearsCountToGetData = 6;
        }
        else if (ct == CandleTypes.FourHour)
        {
            isYearly = true;
            daysOrYearsCountToGetData = 2;
        }
        else if (ct == CandleTypes.EightHour)
        {
            isYearly = true;
            daysOrYearsCountToGetData = 2;
        }
        else if (ct == CandleTypes.TwelveHour)
        {
            isYearly = true;
            daysOrYearsCountToGetData = 2;
        }
        else if (ct == CandleTypes.OneDay)
        {
            isYearly = true;
            daysOrYearsCountToGetData = 3;
        }
        else if (ct == CandleTypes.OneWeek)
        {
            isYearly = true;
            daysOrYearsCountToGetData = 5;
        }
        else if (ct == CandleTypes.OneMonth)
        {
            isYearly = true;
            daysOrYearsCountToGetData = 10;
        }

        int i = 0;

        for (i = 0; i < daysOrYearsCountToGetData;)
        {
            if (i <= 0)
            {
                OhlcvList = await GetCandlestick(pair, ct, dtToday);

                if (OhlcvList != null)
                {
                    OhlcvList.Reverse();
                }
                else
                {
                    Debug.WriteLine("failed to get candlestic.");
                    //return OhlcvList  = new List<Ohlcv>(); 
                }
            }
            else
            {
                DateTime dateTarget;

                if (isYearly)
                {
                    dateTarget = dtToday.AddDays(-(i * 365));
                }
                else
                {
                    dateTarget = dtToday.AddDays(-i);
                }

                List<Ohlcv> responseOhlcvList = await GetCandlestick(pair, ct, dateTarget);

                if (responseOhlcvList != null)
                {
                    responseOhlcvList.Reverse();

                    foreach (var r in responseOhlcvList)
                    {
                        if (OhlcvList == null)
                            OhlcvList = new List<Ohlcv>();
                        OhlcvList.Add(r);
                    }
                }
                else
                {
                    Debug.WriteLine("failed to get candlestic.");
                    //OhlcvList.Clear();
                    //return OhlcvList;
                }
            }

            await Task.Delay(100);

            i++;
        }

        return OhlcvList;
    }

    private async Task<List<Ohlcv>> GetCandlestick(PairCodes pair, CandleTypes ct, DateTime dtTarget)
    {
        string ctString;
        string dtString;

        if (ct == CandleTypes.OneMin)
        {
            ctString = "1min";
            dtString = dtTarget.ToString("yyyyMMdd");
        }
        else if (ct == CandleTypes.FiveMin)
        {
            ctString = "5min";
            dtString = dtTarget.ToString("yyyyMMdd");
        }
        else if (ct == CandleTypes.FifteenMin)
        {
            ctString = "15min";
            dtString = dtTarget.ToString("yyyyMMdd");
        }
        else if (ct == CandleTypes.ThirtyMin)
        {
            ctString = "30min";
            dtString = dtTarget.ToString("yyyyMMdd");
        }
        else if (ct == CandleTypes.OneHour)
        {
            ctString = "1hour";
            dtString = dtTarget.ToString("yyyyMMdd");
        }
        else if (ct == CandleTypes.FourHour)
        {
            ctString = "4hour";
            dtString = dtTarget.ToString("yyyy");
        }
        else if (ct == CandleTypes.EightHour)
        {
            ctString = "8hour";
            dtString = dtTarget.ToString("yyyy");
        }
        else if (ct == CandleTypes.TwelveHour)
        {
            ctString = "12hour";
            dtString = dtTarget.ToString("yyyy");
        }
        else if (ct == CandleTypes.OneDay)
        {
            ctString = "1day";
            dtString = dtTarget.ToString("yyyy");
        }
        else if (ct == CandleTypes.OneWeek)
        {
            ctString = "1week";
            dtString = dtTarget.ToString("yyyy");
        }
        else if (ct == CandleTypes.OneMonth)
        {
            ctString = "1month";
            dtString = dtTarget.ToString("yyyy");
        }
        else
        {
            throw new InvalidOperationException("Unsupported.");
            //return null;
        }

        CandlestickResult csr = await _pubCandlestickApi.GetCandlestick(pair.ToString(), ctString, dtString);

        if (!IsEnabled) return null;

        if (csr != null)
        {
            if (csr.IsSuccess == true)
            {
                if (csr.Candlesticks.Count > 0)
                {
                    // ロウソク足タイプが同じかどうか一応確認
                    if (csr.CandleType.ToString() == ct.ToString())
                    {
                        return csr.Candlesticks;
                    }
                }
            }
            else
            {
                Debug.WriteLine("GetCandlestick: GetCandlestick returned error");
            }
        }
        else
        {
            Debug.WriteLine("GetCandlestick: GetCandlestick returned null");
        }

        return null;
    }

    #endregion

    #region == 板情報 (Depth) ==

    private void TickerTimerDepth(object source, object e)
    {
        UpdateDepth();
    }

    private async void UpdateDepth()
    {
        // timer ver
        if (IsSelectedActive && IsPaneVisible && IsEnabled)
            await GetDepth(PairCode);
        /*
        while (true)
        {
            if ((IsSelectedActive == false) || (IsPaneVisible == false) || (IsEnabled == false))
            {
                await Task.Delay(1000);
                continue;
            }
            
            try
            {
                await GetDepth(PairCode);
            }
            catch (Exception e)
            {
                Debug.WriteLine("■■■■■ UpdateDepth Exception: " + e);
            }
            
            await Task.Delay(1500);
        }
        */
    }

    private async Task<bool> GetDepth(PairCodes pair)
    {
        // まとめグルーピング単位 
        decimal unit = DepthGrouping;

        // リスト数 （基本 上売り200、下買い200）
        int half = 200;
        int listCount = (half * 2) + 1;

        if (_depth == null) return false;
        /*
        (App.Current as App)?.CurrentDispatcherQueue?.TryEnqueue(() =>
        {
        });
        */
        // 初期化
        if ((_depth.Count == 0) || (_depth.Count < listCount))
        {
            //_depth.Clear();
            for (int i = 0; i < listCount; i++)
            {
                Depth dd = new Depth(_ltpFormstString);
                dd.DepthPrice = i;
                dd.DepthBid = 0;
                dd.DepthAsk = 0;
                //if (i == (half-1)) dd.IsLTP = true;
                _depth.Add(dd);
            }
        }
        else
        {
            if (IsDepthGroupingChanged)
            {
                //グルーピング単位が変わったので、一旦クリアする。
                for (int i = 0; i < _depth.Count - 1; i++)
                {
                    _depth[i].DepthPrice = 0;
                    _depth[i].DepthBid = 0;
                    _depth[i].DepthAsk = 0;
                }

                IsDepthGroupingChanged = false;
            }
        }

        // LTP を追加
        _depth[half].DepthPrice = Ltp;
        _depth[half].IsLTP = true;

        DepthResult dpr = await _pubDepthApi?.GetDepth(pair.ToString());

        if (!IsEnabled) return false;

        if (dpr != null)
        {
            if (_depth == null) return false;
            /*
            (App.Current as App)?.CurrentDispatcherQueue?.TryEnqueue(() =>
            {
            });
            */

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

                        if (c2 == 0) c2 = Math.Ceiling(dp.DepthPrice / unit);

                        // 100円単位でまとめる
                        if (Math.Ceiling(dp.DepthPrice / unit) == c2)
                        {
                            t = t + dp.DepthAsk;
                        }
                        else
                        {
                            //Debug.WriteLine(System.Math.Ceiling(dp.DepthPrice / unit).ToString() + " " + System.Math.Ceiling(c / unit).ToString());

                            // 一時保存
                            e = dp.DepthPrice;
                            dp.DepthPrice = c2 * unit;

                            // 一時保存
                            d = dp.DepthAsk;
                            dp.DepthAsk = t;

                            _depth[half - i].DepthAsk = dp.DepthAsk;
                            _depth[half - i].DepthBid = dp.DepthBid;
                            _depth[half - i].DepthPrice = dp.DepthPrice;
                            _depth[half - i].PriceFormat = _ltpFormstString;

                            // 今回のAskは先送り
                            t = d;
                            // 今回のPriceが基準になる
                            c2 = Math.Ceiling(e / unit);

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
                        _depth[half - i].PriceFormat = _ltpFormstString;

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

                        if (c == 0) c = Math.Ceiling(dp.DepthPrice / unit);

                        // 100円単位でまとめる
                        if (Math.Ceiling(dp.DepthPrice / unit) == c)
                        {
                            t = t + dp.DepthBid;
                        }
                        else
                        {
                            // 一時保存
                            e = dp.DepthPrice;
                            dp.DepthPrice = c * unit;

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
                            c = Math.Ceiling(e / unit);

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
                        _depth[i].PriceFormat = _ltpFormstString;
                        i++;
                    }
                }

                _depth[half + 1].IsBidBest = true;
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion

    #region == 歩み値 (Transaction) ==

    private void TickerTimerTransaction(object source, object e)
    {
        UpdateTransactions();
    }

    private async void UpdateTransactions()
    {
        // timer ver
        if (IsSelectedActive && IsPaneVisible && IsEnabled)
            await GetTransactions(PairCode);
        /*
        while (true)
        {
            if ((IsSelectedActive == false) || (IsPaneVisible == false) || (IsEnabled == false))
            {
                await Task.Delay(2000);
                continue;
            }
            else
            {
                try
                {
                    await GetTransactions(this.PairCode);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("■■■■■ UpdateTransactions Exception: " + e);
                }
            }

            await Task.Delay(2000);
        }
        */
    }

    private async Task<bool> GetTransactions(PairCodes pair)
    {
        TransactionsResult trs = await _pubTransactionsApi?.GetTransactions(pair.ToString());

        if (!IsEnabled) return false;

        if (trs != null)
        {
            App.CurrentDispatcherQueue?.TryEnqueue(() =>
            {
                if (_transactions == null) return;

                if (_transactions.Count == 0)
                {
                    // 60 で初期化
                    for (int i = 0; i < 60; i++)
                    {
                        Transaction dd = new Transaction(_ltpFormstString);

                        _transactions.Add(dd);
                    }
                }

                if (trs.Trans != null)
                {
                    int v = 0;
                    foreach (var tr in trs.Trans)
                    {
                        _transactions[v].Amount = tr.Amount;
                        _transactions[v].ExecutedAt = tr.ExecutedAt;
                        _transactions[v].Price = tr.Price;
                        _transactions[v].Side = tr.Side;
                        _transactions[v].TransactionId = tr.TransactionId;

                        v++;

                        if (v >= 60)
                            break;
                    }
                }
            });

            return true;
        }
        else
        {
            Debug.WriteLine("■■■■■ GetTransactions returned null");
            return false;
        }
    }

    #endregion

    #region == コマンド ==

    public RelayCommand TogglePaneVisibilityCommand { get; private set; }
    private bool TogglePaneVisibilityCommand_CanExecute()
    {
        return true;
    }

    public void TogglePaneVisibilityCommand_Execute()
    {
        IsPaneVisible = !IsPaneVisible;
    }

    public ICommand ChangeCandleTypeCommand { get; set; }
    public bool ChangeCandleTypeCommand_CanExecute()
    {
        return true;
    }
    public void ChangeCandleTypeCommand_Execute(CandleTypes candleType)
    {
        ChangeCandleType(candleType);
    }

    public ICommand TogglePairVisibilityCommand { get; private set; }
    private bool TogglePairVisibilityCommand_CanExecute()
    {
        return true;
    }

    public void TogglePairVisibilityCommand_Execute(PairCodes pairCode)
    {

    }

    #endregion
}
