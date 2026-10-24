using BitWallpaper.Models;
using BitWallpaper.Services;
using BitWallpaper.Services.Contracts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using Microsoft.UI.Xaml;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.Intrinsics.Arm;

namespace BitWallpaper.ViewModels;

public partial class PairViewModel : ObservableObject, IDisposable
{
    private DateTime lastChartLoadedDateTime = DateTime.MinValue;
    public event EventHandler<bool>? DepthScrollCenter;
    private bool _isChartLoading;
    private readonly List<DateTime> _chartTimes = [];
    private bool _isChartRefreshRunning;
    private readonly DispatcherTimer _dispatcherTimerChart = new();

    private readonly IBitbankHttpSearvice _bitbankHttpSearvice;
    private readonly IBitbankStreamService _bitbankStreamService;
    private readonly IDispatcherService _dispatcherService;

    public PairViewModel(PairCodes p, double fontSize, string ltpFormstString, string currencyFormstString, decimal grouping100, decimal grouping1000)
    {
        _bitbankHttpSearvice = App.GetService<IBitbankHttpSearvice>();
        _bitbankStreamService = App.GetService<IBitbankStreamService>();    
        _dispatcherService = App.GetService<IDispatcherService>();

        _p = p;
        _ltpFontSize = fontSize;
        _ltpFormstString = ltpFormstString;
        _currencyFormstString = currencyFormstString;

        _depthGrouping100 = grouping100;
        _depthGrouping1000 = grouping1000;

        // Temp testing.
        //_ = ChangeCandleType(CandleTypes.OneHour);

        IsChartTooltipVisible = false;

        // Chart update timer
        _dispatcherTimerChart.Tick += OnTimerChart;
        _dispatcherTimerChart.Interval = chartUpdateInterval;  // Temp testing. new(0, 5, 0);
        _dispatcherTimerChart.Start(); // Testing without updating the chart.. using ticker as we go.

        //_bitbankStreamService

    }

    #region == Properties ==

    #region == Pair ==

    private readonly PairCodes _p;
    public PairCodes PairCode => _p;

    // 表示用 通貨ペア名 "BTC/JPY";
    public string PairString => PairStrings[_p];

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
                {PairCodes.omg_jpy, "OMG/JPY"},
                {PairCodes.xym_jpy, "XYM/JPY"},
                {PairCodes.link_jpy, "LINK/JPY"},
                {PairCodes.mkr_jpy, "MKR/JPY"},
                {PairCodes.boba_jpy, "BOBA/JPY"},
                {PairCodes.enj_jpy, "ENJ/JPY"},
                {PairCodes.matic_jpy, "MATIC/JPY"},
                {PairCodes.dot_jpy, "DOT/JPY"},
                {PairCodes.doge_jpy, "DOGE/JPY"},
                {PairCodes.astr_jpy, "ASTR/JPY"},
                {PairCodes.ada_jpy, "ADA/JPY"},
                {PairCodes.avax_jpy, "AVAX/JPY"},
                {PairCodes.axs_jpy, "AXS/JPY"},
                {PairCodes.flr_jpy, "FLR/JPY"},
                {PairCodes.sand_jpy, "SAND/JPY"},
            };

    public string CurrencyUnitString => CurrentPairCoin[_p];

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
            {PairCodes.omg_jpy, "OMG"},
            {PairCodes.xym_jpy, "XYM"},
            {PairCodes.link_jpy, "LINK"},
            {PairCodes.mkr_jpy, "MKR"},
            {PairCodes.boba_jpy, "BOBA"},
            {PairCodes.enj_jpy, "ENJ"},
            {PairCodes.matic_jpy, "MATIC"},
            {PairCodes.dot_jpy, "DOT"},
            {PairCodes.doge_jpy, "DOGE"},
            {PairCodes.astr_jpy, "ASTR"},
            {PairCodes.ada_jpy, "ADA"},
            {PairCodes.avax_jpy, "AVAX"},
            {PairCodes.axs_jpy, "AXS"},
            {PairCodes.flr_jpy, "FLR"},
            {PairCodes.sand_jpy, "SAND"},
        };

    private decimal _ltp;
    public decimal Ltp
    {
        get => _ltp;
        set
        {
            if (_ltp == value)
            {
                return;
            }

            _ltp = value;

            OnPropertyChanged(nameof(Ltp));
            OnPropertyChanged(nameof(LtpString));

            /*
            if (IsSelectedActive && IsEnabled) // 
            {
                if (Sections[0] is RectangularSection secn)
                {
                    secn.Yi = (double)_ltp;
                    secn.Yj = (double)_ltp;
                }

                // a little hack to update Section...
                App.CurrentDispatcherQueue?.TryEnqueue(() =>
                {
                    if (Series[1].Values == null)
                    {
                        return;
                    }

                    if (Series[1].Values is ObservableCollection<FinancialPoint> oc)
                    {
                        if (oc.Count < 1)
                        {
                            return;
                        }

                        oc[0].Close =
                            oc[0].Close = (double)_ltp;
                    }
                });
            }
            */
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
        get => _ltpFormstString;
        set
        {
            if (_ltpFormstString == value)
            {
                return;
            }

            _ltpFormstString = value;
            OnPropertyChanged(nameof(LtpFormstString));
        }
    }

    private readonly string _currencyFormstString = "C";
    
    public string PercentChange
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;

            OnPropertyChanged();
        }
    } = string.Empty;

    private bool _ltpUpFlag;
    public bool LtpUpFlag
    {
        get => _ltpUpFlag;
        set
        {
            if (_ltpUpFlag == value)
            {
                return;
            }

            _ltpUpFlag = value;
            OnPropertyChanged(nameof(LtpUpFlag));
        }
    }

    private readonly double _ltpFontSize = 28;
    public double LtpFontSize => _ltpFontSize;

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
            OnPropertyChanged(nameof(Bid));
            OnPropertyChanged(nameof(BidString));

        }
    }
    public string BidString => string.Format("{0:#,0}", _bid);

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
            OnPropertyChanged(nameof(Ask));
            OnPropertyChanged(nameof(AskString));

        }
    }
    public string AskString => string.Format("{0:#,0}", _ask);

    private DateTime _tickTimeStamp;
    public DateTime TickTimeStamp
    {
        get => _tickTimeStamp;
        set
        {
            if (_tickTimeStamp == value)
            {
                return;
            }

            _tickTimeStamp = value;
            OnPropertyChanged(nameof(TickTimeStamp));
            OnPropertyChanged(nameof(TickTimeStampString));

        }
    }
    public string TickTimeStampString => _tickTimeStamp.ToLocalTime().ToString("G", System.Globalization.CultureInfo.CurrentUICulture);//"yyyy/MM/dd HH:mm:ss"

    #endregion

    #region == Flags ==

    private bool _isEnabled = true;
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled == value)
            {
                return;
            }

            _isEnabled = value;

            // When disabled, reset the chart loaded flag so that it will be reloaded when enabled again.
            if (!_isEnabled)
            {
                IsChartInitAndLoaded = false;
            }

            OnPropertyChanged(nameof(IsEnabled));
        }
    }

    // Selected or not.
    private bool _isSelectedActive;
    public bool IsSelectedActive
    {
        get => _isSelectedActive;
        set
        {
            if (_isSelectedActive == value)
            {
                return;
            }

            _isSelectedActive = value;
            OnPropertyChanged(nameof(IsSelectedActive));

            if (_isSelectedActive)
            {
                _bitbankStreamService.JoinDepthRoomAsync(_p);
            }
            else
            {
                _bitbankStreamService.LeaveDepthRoomAsync(_p);
            }
        }
    }

    private bool _isChartInitAndLoaded;
    public bool IsChartInitAndLoaded
    {
        get => _isChartInitAndLoaded;
        set
        {
            if (_isChartInitAndLoaded == value)
            {
                return;
            }

            _isChartInitAndLoaded = value;
            OnPropertyChanged(nameof(IsChartInitAndLoaded));
        }
    }

    private bool _isPaneOpen = true;
    public bool IsPaneOpen
    {
        get => _isPaneOpen;
        set
        {
            if (_isPaneOpen == value)
            {
                return;
            }

            _isPaneOpen = value;
            OnPropertyChanged(nameof(IsPaneOpen));
        }
    }

    #endregion

    #region == Options ==

    private bool _isChartTooltipVisible = false;
    public bool IsChartTooltipVisible
    {
        get => _isChartTooltipVisible;
        set
        {
            if (_isChartTooltipVisible == value)
            {
                return;
            }

            _isChartTooltipVisible = value;
            OnPropertyChanged(nameof(IsChartTooltipVisible));

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

    private LiveChartsCore.Measure.TooltipPosition _isChartTooltipVisibleTemp = LiveChartsCore.Measure.TooltipPosition.Hidden;
    public LiveChartsCore.Measure.TooltipPosition IsChartTooltipVisibleTemp
    {
        get => _isChartTooltipVisibleTemp;
        set
        {
            if (_isChartTooltipVisibleTemp == value)
            {
                return;
            }

            _isChartTooltipVisibleTemp = value;
            OnPropertyChanged(nameof(IsChartTooltipVisibleTemp));
        }
    }

    #endregion

    #region == Alarm ==

    private decimal _alarmPlus;
    public decimal AlarmPlus
    {
        get => _alarmPlus;
        set
        {
            if (_alarmPlus == value)
            {
                return;
            }

            if (value != 0)
            {
                if (value <= _ltp)
                {
                    return;
                }
            }

            _alarmPlus = value;
            OnPropertyChanged(nameof(AlarmPlus));
            OnPropertyChanged(nameof(AlarmPlusString));
            OnPropertyChanged(nameof(AlarmLabel));
        }
    }
    public string AlarmPlusString => string.Format(_ltpFormstString, AlarmPlus);

    private decimal _alarmMinus;
    public decimal AlarmMinus
    {
        get => _alarmMinus;
        set
        {
            if (_alarmMinus == value)
            {
                return;
            }

            if (value != 0)
            {
                if (value >= _ltp)
                {
                    return;
                }
            }

            _alarmMinus = value;
            OnPropertyChanged(nameof(AlarmMinus));
            OnPropertyChanged(nameof(AlarmMinusString));
            OnPropertyChanged(nameof(AlarmLabel));
        }
    }
    public string AlarmMinusString => string.Format(_ltpFormstString, AlarmMinus);

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
    private string _highLowInfoText = "";
    public string HighLowInfoText
    {
        get => _highLowInfoText;
        set
        {
            if (_highLowInfoText == value)
            {
                return;
            }

            _highLowInfoText = value;
            OnPropertyChanged(nameof(HighLowInfoText));
        }
    }

    private bool _highLowInfoTextColorFlag;
    public bool HighLowInfoTextColorFlag
    {
        get => _highLowInfoTextColorFlag;
        set
        {
            if (_highLowInfoTextColorFlag == value)
            {
                return;
            }

            _highLowInfoTextColorFlag = value;
            OnPropertyChanged(nameof(HighLowInfoTextColorFlag));
        }
    }

    // アラーム音
    // 起動後
    private bool _playSoundLowest = false;
    public bool PlaySoundLowest
    {
        get => _playSoundLowest;
        set
        {
            if (_playSoundLowest == value)
            {
                return;
            }

            _playSoundLowest = value;
            OnPropertyChanged(nameof(PlaySoundLowest));
        }
    }

    private bool _playSoundHighest = false;
    public bool PlaySoundHighest
    {
        get => _playSoundHighest;
        set
        {
            if (_playSoundHighest == value)
            {
                return;
            }

            _playSoundHighest = value;
            OnPropertyChanged(nameof(PlaySoundHighest));
        }
    }

    // last 24h
    private bool _playSoundLowest24h = false;
    public bool PlaySoundLowest24h
    {
        get => _playSoundLowest24h;
        set
        {
            if (_playSoundLowest24h == value)
            {
                return;
            }

            _playSoundLowest24h = value;
            OnPropertyChanged(nameof(PlaySoundLowest24h));
        }
    }

    private bool _playSoundHighest24h = false;
    public bool PlaySoundHighest24h
    {
        get => _playSoundHighest24h;
        set
        {
            if (_playSoundHighest24h == value)
            {
                return;
            }

            _playSoundHighest24h = value;
            OnPropertyChanged(nameof(PlaySoundHighest24h));
        }
    }

    #endregion

    #region == Stat ==

    // Initial price at startup.
    private decimal _basePrice = 0;
    public decimal BasePrice
    {
        get => _basePrice;
        set
        {
            if (_basePrice == value)
            {
                return;
            }

            _basePrice = value;

            OnPropertyChanged(nameof(BasePrice));
            OnPropertyChanged(nameof(BasePriceIcon));
            OnPropertyChanged(nameof(BasePriceString));

        }
    }

    public string BasePriceString => string.Format(_ltpFormstString, BasePrice);

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
        get => _basePriceUpFlag;
        set
        {
            if (_basePriceUpFlag == value)
            {
                return;
            }

            _basePriceUpFlag = value;
            OnPropertyChanged(nameof(BasePriceUpFlag));
        }
    }

    // 過去1分間の平均値
    private decimal _averagePrice;
    public decimal AveragePrice
    {
        get => _averagePrice;
        set
        {
            if (_averagePrice == value)
            {
                return;
            }

            _averagePrice = value;
            OnPropertyChanged(nameof(AveragePrice));
            OnPropertyChanged(nameof(AveragePriceIcon));
            //this.OnPropertyChanged("AveragePriceIconColor");
            OnPropertyChanged(nameof(AveragePriceString));
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
        get => _averagePriceUpFlag;
        set
        {
            if (_averagePriceUpFlag == value)
            {
                return;
            }

            _averagePriceUpFlag = value;
            OnPropertyChanged(nameof(AveragePriceUpFlag));
        }
    }

    // 過去２４時間の中央値
    public decimal MiddleLast24Price => (_lowestIn24Price + _highestIn24Price) / 2;
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
        get => _middleLast24PriceUpFlag;
        set
        {
            if (_middleLast24PriceUpFlag == value)
            {
                return;
            }

            _middleLast24PriceUpFlag = value;
            OnPropertyChanged(nameof(MiddleLast24PriceUpFlag));
        }
    }

    // 起動後の中央値
    public decimal MiddleInitPrice => (_lowestPrice + _highestPrice) / 2;
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
        get => _middleInitPriceUpFlag;
        set
        {
            if (_middleInitPriceUpFlag == value)
            {
                return;
            }

            _middleInitPriceUpFlag = value;
            OnPropertyChanged(nameof(MiddleInitPriceUpFlag));
        }
    }

    // 24時間の最高値 
    private decimal _highestIn24Price;
    public decimal HighestIn24Price
    {
        get => _highestIn24Price;
        set
        {
            if (_highestIn24Price == value)
            {
                return;
            }

            _highestIn24Price = value;
            OnPropertyChanged(nameof(HighestIn24Price));
            OnPropertyChanged(nameof(High24String));

            OnPropertyChanged(nameof(MiddleLast24Price));
            OnPropertyChanged(nameof(MiddleLast24PriceString));

            //if (MinMode) return;
            // チャートの最高値をセット
            //ChartAxisY[0].MaxValue = (double)_highestIn24Price + 3000;
            // チャートの２４最高値ポイントを更新
            //(ChartSeries[1].Values[0] as ObservableValue).Value = (double)_highestIn24Price;

        }
    }
    public string High24String => string.Format(_ltpFormstString, _highestIn24Price);

    // 24時間の最高値 アラートOn/Offフラグ
    private bool _highestIn24PriceAlart;
    public bool HighestIn24PriceAlart
    {
        get => _highestIn24PriceAlart;
        set
        {
            if (_highestIn24PriceAlart == value)
            {
                return;
            }

            _highestIn24PriceAlart = value;
            OnPropertyChanged(nameof(HighestIn24PriceAlart));
            OnPropertyChanged(nameof(PriceAlart));
        }
    }

    // 24時間の最安値 
    private decimal _lowestIn24Price;
    public decimal LowestIn24Price
    {
        get => _lowestIn24Price;
        set
        {
            if (_lowestIn24Price == value)
            {
                return;
            }

            _lowestIn24Price = value;
            OnPropertyChanged(nameof(LowestIn24Price));
            OnPropertyChanged(nameof(Low24String));

            OnPropertyChanged(nameof(MiddleLast24Price));
            OnPropertyChanged(nameof(MiddleLast24PriceString));

            //if (MinMode) return;
            // チャートの最低値をセット
            //ChartAxisY[0].MinValue = (double)_lowestIn24Price - 3000;
            // チャートの２４最低値ポイントを更新
            //(ChartSeries[2].Values[0] as ObservableValue).Value = (double)_lowestIn24Price;
        }
    }
    public string Low24String => string.Format(_ltpFormstString, _lowestIn24Price);

    // 24時間の最安値 アラートOn/Offフラグ
    private bool _lowestIn24PriceAlart;
    public bool LowestIn24PriceAlart
    {
        get => _lowestIn24PriceAlart;
        set
        {
            if (_lowestIn24PriceAlart == value)
            {
                return;
            }

            _lowestIn24PriceAlart = value;
            OnPropertyChanged(nameof(LowestIn24PriceAlart));
            OnPropertyChanged(nameof(PriceAlart));
        }
    }

    // 起動後 最高値
    private decimal _highestPrice;
    public decimal HighestPrice
    {
        get => _highestPrice;
        set
        {
            if (_highestPrice == value)
            {
                return;
            }

            _highestPrice = value;
            OnPropertyChanged(nameof(HighestPrice));
            OnPropertyChanged(nameof(HighestPriceString));

            OnPropertyChanged(nameof(MiddleInitPrice));
            OnPropertyChanged(nameof(MiddleInitPriceString));
            OnPropertyChanged(nameof(MiddleInitPriceIcon));

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
        get => _highestPriceAlart;
        set
        {
            if (_highestPriceAlart == value)
            {
                return;
            }

            _highestPriceAlart = value;
            OnPropertyChanged(nameof(HighestPriceAlart));
            OnPropertyChanged(nameof(PriceAlart));
        }
    }

    // 起動後 最低値
    private decimal _lowestPrice;
    public decimal LowestPrice
    {
        get => _lowestPrice;
        set
        {
            if (_lowestPrice == value)
            {
                return;
            }

            _lowestPrice = value;
            OnPropertyChanged(nameof(LowestPrice));
            OnPropertyChanged(nameof(LowestPriceString));

            OnPropertyChanged(nameof(MiddleInitPrice));
            OnPropertyChanged(nameof(MiddleInitPriceString));
            OnPropertyChanged(nameof(MiddleInitPriceIcon));

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
        get => _lowestPriceAlart;
        set
        {
            if (_lowestPriceAlart == value)
            {
                return;
            }

            _lowestPriceAlart = value;
            OnPropertyChanged(nameof(LowestPriceAlart));
            OnPropertyChanged(nameof(PriceAlart));
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

    #region == Depth and Transaction ==

    private ObservableCollection<BitWallpaper.Models.Transaction> _transactions = [];
    public ObservableCollection<BitWallpaper.Models.Transaction> Transactions
    {
        get => _transactions;
        set
        {
            if (_transactions == value)
            {
                return;
            }

            _transactions = value;
            OnPropertyChanged(nameof(Transactions));
        }
    }

    private ObservableCollection<BitWallpaper.Models.Depth> _depth = [];
    public ObservableCollection<BitWallpaper.Models.Depth> Depth
    {
        get => _depth;
        set
        {
            if (_depth == value)
            {
                return;
            }

            _depth = value;
            OnPropertyChanged(nameof(Depth));
        }
    }

    private bool _isDeepthGroupingChanged;
    public bool IsDepthGroupingChanged
    {
        get => _isDeepthGroupingChanged;
        set
        {
            if (_isDeepthGroupingChanged == value)
            {
                return;
            }

            _isDeepthGroupingChanged = value;

            OnPropertyChanged(nameof(IsDepthGroupingChanged));
        }
    }

    private decimal _depthGrouping = 0;
    public decimal DepthGrouping
    {
        get => _depthGrouping;
        set
        {
            if (_depthGrouping == value)
            {
                return;
            }

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

            OnPropertyChanged(nameof(DepthGrouping));

        }
    }

    private bool _isDepthGroupingOff = true;
    public bool IsDepthGroupingOff
    {
        get => _isDepthGroupingOff;
        set
        {
            if (_isDepthGroupingOff == value)
            {
                return;
            }

            _isDepthGroupingOff = value;
            OnPropertyChanged(nameof(IsDepthGroupingOff));
        }
    }

    private decimal _depthGrouping100 = 100;
    public decimal DepthGrouping100
    {
        get => _depthGrouping100;
        set
        {
            if (_depthGrouping100 == value)
            {
                return;
            }

            _depthGrouping100 = value;
            OnPropertyChanged(nameof(DepthGrouping100));

        }
    }

    private bool _isDepthGrouping100;
    public bool IsDepthGrouping100
    {
        get => _isDepthGrouping100;
        set
        {
            if (_isDepthGrouping100 == value)
            {
                return;
            }

            _isDepthGrouping100 = value;
            OnPropertyChanged(nameof(IsDepthGrouping100));
        }
    }

    private decimal _depthGrouping1000 = 1000;
    public decimal DepthGrouping1000
    {
        get => _depthGrouping1000;
        set
        {
            if (_depthGrouping1000 == value)
            {
                return;
            }

            _depthGrouping1000 = value;
            OnPropertyChanged(nameof(DepthGrouping1000));

        }
    }

    private bool _isDepthGrouping1000;
    public bool IsDepthGrouping1000
    {
        get => _isDepthGrouping1000;
        set
        {
            if (_isDepthGrouping1000 == value)
            {
                return;
            }

            _isDepthGrouping1000 = value;
            OnPropertyChanged(nameof(IsDepthGrouping1000));
        }
    }

    #endregion

    #region == Charts ==

    private IChartElement[] _sections =
[
    new RectangularSection
        {
            Yi = 1,
            Yj = 1,
            ScalesYAt = 1,
            Stroke = new SolidColorPaint
            {
                Color = SKColors.OrangeRed.WithAlpha(80),
                StrokeThickness = 1,
                PathEffect = new DashEffect(new float[] { 6, 6 })
            }
        }
];
    public IChartElement[] Sections
    {
        get => _sections;
        set
        {
            if (_sections == value)
            {
                return;
            }

            _sections = value;
            OnPropertyChanged(nameof(Sections));
        }
    }

    public ICartesianAxis[] XAxes { get; set; } =
    [
        new Axis()
        {
            TextSize = 12.5,
            LabelsRotation = -15,
            LabelsPaint = new SolidColorPaint(SKColors.Gray.WithAlpha(80)),
            Labeler = value => new DateTime((long)value).ToString("MM/dd"), //TODO: localize aware
            UnitWidth = TimeSpan.FromHours(0.5).Ticks,
            MinStep = TimeSpan.FromDays(0.4).Ticks,
            MaxLimit = null,
            MinLimit= DateTime.Now.Ticks - TimeSpan.FromDays(2.8).Ticks,
            SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray.WithAlpha(80)) { StrokeThickness = 1,PathEffect = new DashEffect([3, 3]) }
        }
    ];

    public ICartesianAxis[] YAxes
    {
        get; set;

    } =
        [
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
            TextSize = 12.5,
                LabelsRotation = 0,
                LabelsPaint = new SolidColorPaint(SKColors.Gray.WithAlpha(80)),
                Position = LiveChartsCore.Measure.AxisPosition.End,
                SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray.WithAlpha(80))
                            {
                                StrokeThickness = 1,
                                PathEffect = new DashEffect([3, 3])
                            },
                //Labeler = Labelers.Currency,
                //Labeler = (value) => value.ToString("C", new System.Globalization.CultureInfo("ja-Jp")),
                Labeler = (value) => value.ToString("N", new CultureInfo("ja-Jp")),
            }
    ];

    private ISeries[] _series =
    [
        new ColumnSeries<DateTimePoint>
        {
            Name = "Depth",
            ScalesYAt = 0,
            ZIndex = -10,
            //Stroke = new SolidColorPaint((new SKColor(198, 167, 0)), 0),
            Fill =  new SolidColorPaint(new SKColor(127, 127, 127).WithAlpha(80), 1),
            /*
             * CS0618	'Series<DateTimePoint, RoundedRectangleGeometry, LabelGeometry, SkiaSharpDrawingContext>.TooltipLabelFormatter' is deprecated ('You must now use XToolTipLabelFormatter or YToolTipLabelFormatter instead.')
            TooltipLabelFormatter = (chartPoint) =>
                $"Depth, {new DateTime((long) chartPoint.SecondaryValue):yyy/MM/dd HH}: {chartPoint.PrimaryValue}",
            Values = new ObservableCollection<DateTimePoint>
            {
                //new DateTimePoint(DateTime.Now, 1)
            }
            */
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
    ];
    public ISeries[] Series
    {
        get => _series;
        set
        {
            if (_series == value)
            {
                return;
            }

            _series = value;
            OnPropertyChanged(nameof(Series));
        }
    }

    private TimeSpan chartUpdateInterval = new(1, 0, 0);

    private CandleTypes _selectedCandleType = CandleTypes.OneHour;
    public CandleTypes SelectedCandleType
    {
        get => _selectedCandleType;
        set
        {
            if (_selectedCandleType == value)
            {
                return;
            }

            _selectedCandleType = value;
            OnPropertyChanged(nameof(SelectedCandleType));
            OnPropertyChanged(nameof(SelectedCandleTypeLabelString));

            if (_selectedCandleType == CandleTypes.OneMin)
            {
                chartUpdateInterval = new TimeSpan(0, 1, 0);
            }
            else if (_selectedCandleType == CandleTypes.FiveMin)
            {
                chartUpdateInterval = new TimeSpan(0, 5, 0);
            }
            else if (_selectedCandleType == CandleTypes.FifteenMin)
            {
                chartUpdateInterval = new TimeSpan(0, 15, 0);
            }
            else if (_selectedCandleType == CandleTypes.ThirtyMin)
            {
                chartUpdateInterval = new TimeSpan(0, 30, 0);
            }
            else if (_selectedCandleType == CandleTypes.OneHour)
            {
                chartUpdateInterval = new TimeSpan(1, 0, 0);
            }
            else if (_selectedCandleType == CandleTypes.FourHour)
            {
                chartUpdateInterval = new TimeSpan(4, 0, 0);
            }
            else if (_selectedCandleType == CandleTypes.EightHour)
            {
                chartUpdateInterval = new TimeSpan(8, 0, 0);
            }
            else if (_selectedCandleType == CandleTypes.TwelveHour)
            {
                chartUpdateInterval = new TimeSpan(12, 0, 0);
            }
            else if (_selectedCandleType == CandleTypes.OneDay)
            {
                chartUpdateInterval = new TimeSpan(24, 0, 0);
            }
            else if (_selectedCandleType == CandleTypes.OneWeek)
            {
                chartUpdateInterval = new TimeSpan(168, 0, 0);
            }
            else if (_selectedCandleType == CandleTypes.OneMonth)
            {
                chartUpdateInterval = new TimeSpan(720, 0, 0);
            }

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
            var candleTypeText = "";
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

    #endregion

    #region == Public Methods ==

    // Called from MainViewModel.
    public async Task InitializeAndLoadAsync()
    {
        //Debug.WriteLine($"InitializeAndLoadAsync() called for {PairCode} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

        if (!IsEnabled)
        {
            Debug.WriteLine($"InitializeAndLoadAsync() skipped for {PairCode} because IsEnabled is false.");
            return;
        }

        if (IsChartInitAndLoaded)
        {
            Debug.WriteLine($"InitializeAndLoadAsync() skipped for {PairCode} because chart is already initialized and loaded.");

            return;
        }

        if (_isChartLoading)
        {
            Debug.WriteLine($"InitializeAndLoadAsync() skipped for {PairCode} because chart is already loading.");
            return;
        }

        _dispatcherService.TryEnqueue(async () =>
        {
            _isChartLoading = true;

            try
            {
                Debug.WriteLine($"InitializeAndLoadAsync() starting LoadChartAsync() for {PairCode} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                if (Sections[0] is RectangularSection secn)
                {
                    secn.Yi = (double)_ltp;
                    secn.Yj = (double)_ltp;
                }

                // Little hack to init. This is required after upgrading Livechart2 to 2.0 rc.
                // clear chart data.
                Series[0].Values = new ObservableCollection<DateTimePoint> { new(DateTime.Now, 1) };
                Series[1].Values = new ObservableCollection<FinancialPoint> { new(DateTime.Now, 100, 0, 0, 0) };

                Debug.WriteLine($"InitializeAndLoadAsync() calling LoadChartAsync() for {PairCode} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                await LoadChartAsync(SelectedCandleType);
                Debug.WriteLine($"InitializeAndLoadAsync() completed LoadChartAsync() for {PairCode} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InitializeAndLoadAsync() exception for {PairCode}: {ex}");
            }
            finally
            {
                _isChartLoading = false;
            }

        });

        //Sections[0].Yi = (double)_ltp;
        //Sections[0].Yj = (double)_ltp;
    }

    // Called from MainViewModel.
    public async Task UpdateTickerDataAsync(BitbankTickerData data)
    {
        if (!IsEnabled)
        {
            //Debug.WriteLine($"UpdateTickerData() skipped for {data.TickerPair} at {DateTimeOffset.FromUnixTimeMilliseconds(data.Timestamp).LocalDateTime:yyyy-MM-dd HH:mm:ss} because IsEnabled is false.");
            return;
        }

        // We need to update the charts.
        /*
        if (!IsSelectedActive)
        {
            //Debug.WriteLine($"UpdateTickerData() skipped for {data.TickerPair} at {DateTimeOffset.FromUnixTimeMilliseconds(data.Timestamp).LocalDateTime:yyyy-MM-dd HH:mm:ss} because IsSelectedActive is false.");
            return;
        }
        */

        /*
        PairCodes pair = data.TickerPair;
        DateTime timeStamp = DateTimeOffset.FromUnixTimeMilliseconds(data.Timestamp).LocalDateTime;

        // Calculate a quick math metric (e.g., daily percentage change)
        decimal priceChange = data.Last - data.Open;
        decimal percentChange = data.Open != 0 ? (priceChange / data.Open) * 100 : 0;

        // Print out your typed metrics using standard number formatting
        Debug.WriteLine($"=== {pair} NUMERIC DATA [{timeStamp:yyyy-MM-dd HH:mm:ss}] ===");
        Debug.WriteLine($"  Last Price : {data.Last:N0} JPY ({percentChange:+0.00;-0.00;0.00}%)");
        Debug.WriteLine($"  Bid (Buy)  : {data.Buy:N0}   | Ask (Sell) : {data.Sell:N0}");
        Debug.WriteLine($"  24h High   : {data.High:N0}  | 24h Low    : {data.Low:N0}");
        Debug.WriteLine($"  24h Volume : {data.Vol:N4}");
        Debug.WriteLine(new string('=', 45));
        */

        if (data.TickerPair != PairCode)
        {
            return;
        }

        // Calculate a quick math metric (e.g., daily percentage change)
        decimal priceChange = data.Last - data.Open;
        decimal percentChange = data.Open != 0 ? (priceChange / data.Open) * 100 : 0;

        _dispatcherService.TryEnqueue(() =>
        {
            try
            {
                Ltp = data.Last;
                PercentChange = $"{percentChange:+0.00;-0.00;0.00}%";
                Bid = data.Buy;
                Ask = data.Sell;

                HighestIn24Price = data.High;
                LowestIn24Price = data.Low;
                TickTimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(data.Timestamp).LocalDateTime;

                if (!IsChartInitAndLoaded)
                {
                    //Debug.WriteLine($"UpdateTickerData() skipped for {data.TickerPair} at {DateTimeOffset.FromUnixTimeMilliseconds(data.Timestamp).LocalDateTime:yyyy-MM-dd HH:mm:ss} because chart is not initialized and loaded.");
                    return;
                }

                if (Sections[0] is RectangularSection section)
                {
                    section.Yi = (double)data.Last;
                    section.Yj = (double)data.Last;
                }

                //DateTime timeStamp = DateTimeOffset.FromUnixTimeMilliseconds(data.Timestamp).LocalDateTime;
                UpdateCandleUsingTickerData(TickTimeStamp, data); ;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateTickerDataAsync() exception for {PairCode}: {ex}");
            }
        });
    }

    // Called from MainViewModel.
    public async Task UpdateDepthDataAsync(BitbankDepthData data)
    {
        if (!IsSelectedActive || !IsEnabled)
        {
            // Let's update even if the pane is not open.
            // || !IsPaneOpen 
            Debug.WriteLine($"UpdateDepthDataAsync() skipped for {PairCode} because IsSelectedActive={IsSelectedActive}, IsEnabled={IsEnabled}");
            return;
        }

        if (data.DepthPair != PairCode)
        {
            return;
        }

        if (_depth == null)
        {
            return;
        }

        _dispatcherService.TryEnqueue(() =>
        {
            try
            {
                DepthResult dpr = new();

                if (data.Bids is not null)
                {
                    foreach (var dp in data.Bids)
                    {
                        if (dp is not null)
                        {
                            Depth dd = new()
                            {
                                DepthPrice = dp.Price,//decimal.Parse(dp[0]),
                                DepthBid = dp.Amount,//decimal.Parse(dp[1]),
                                DepthAsk = 0
                            };
                            dpr.DepthBidList.Add(dd);
                        }
                    }
                }

                if (data.Asks is not null)
                {
                    foreach (var dp in data.Asks)
                    {
                        if (dp is not null)
                        {
                            Depth dd = new()
                            {
                                DepthPrice = dp.Price,//decimal.Parse(dp[0]),
                                DepthBid = 0,
                                DepthAsk = dp.Amount//decimal.Parse(dp[1])
                            };
                            dpr.DepthAskList.Add(dd);
                        }
                    }
                }
                
                dpr.ErrorCode = 0;
                dpr.IsSuccess = true;

                UpdateDepth(dpr);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateDepthDataAsync() exception for {PairCode}: {ex}");
            }
        });
    }

    public void Dispose()
    {
        _dispatcherTimerChart.Stop();
        _dispatcherTimerChart.Tick -= OnTimerChart;

        // TODO: Dispose other resources if necessary
    }

    #endregion

    #region == Private Methods ==

    #region == Chart ==

    private async Task LoadChartAsync(CandleTypes ct)
    {
        try
        {
            if (Sections[0] is RectangularSection secn)
            {
                secn.Yi = (double)_ltp;
                secn.Yj = (double)_ltp;
            }

            if (!IsEnabled)
            {
                return;
            }

            Debug.WriteLine($"LoadChartAsync() called for {PairCode} at {DateTime.Now:yyyy-MM-dd HH:mm:ss} with candle type {ct}");

            // gets new data.
            var res = await GetCandlesticksAsync(PairCode, ct);

            if (res == null)
            {
                Debug.WriteLine($"LoadChartAsync() failed to get candlestick data for {PairCode} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                return;
            }

            if (res.Count > 0)
            {
                Debug.WriteLine($"LoadChartAsync() successfully retrieved {res.Count} candlestick data points for {PairCode} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                DoLoadChart(res, ct);
                Debug.WriteLine($"LoadChartAsync() completed loading chart for {PairCode} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadChartAsync() exception for {PairCode}: {ex}");
        }
    }

    private void DoLoadChart(List<Ohlcv> list, CandleTypes ct)
    {
        try
        {
            // Need to be here. not static and all.
            if (_currencyFormstString.Equals("C3"))
            {
                YAxes[1].Labeler = (value) => value.ToString("C3", new CultureInfo("ja-JP"));
            }
            else if (_currencyFormstString.Equals("C2"))
            {
                YAxes[1].Labeler = (value) => value.ToString("C2", new CultureInfo("ja-JP"));
            }
            else if (_currencyFormstString.Equals("C1"))
            {
                YAxes[1].Labeler = (value) => value.ToString("C1", new CultureInfo("ja-JP"));
            }
            else
            {
                YAxes[1].Labeler = (value) => value.ToString("C", new CultureInfo("ja-JP"));
            }

            //TODO: localize aware
            // キャンドルタイプにあわせてチャートXAxes[0]表示tweak
            if (ct == CandleTypes.OneMin)
            {
                XAxes[0].Labeler = value => new DateTime((long)value).ToString("H:mm");
                XAxes[0].UnitWidth = TimeSpan.FromMinutes(0.4).Ticks;
                //XAxes[0].MinStep = TimeSpan.FromMinutes(10).Ticks;
                XAxes[0].MinLimit = DateTime.Now.Ticks - TimeSpan.FromMinutes(60).Ticks;
                XAxes[0].MaxLimit = null;
            }
            else if (ct == CandleTypes.FiveMin)
            {
                XAxes[0].Labeler = value => new DateTime((long)value).ToString("H:mm");
                XAxes[0].UnitWidth = TimeSpan.FromMinutes(2.5).Ticks;
                //XAxes[0].MinStep = TimeSpan.FromMinutes(10).Ticks;
                XAxes[0].MinLimit = DateTime.Now.Ticks - TimeSpan.FromMinutes(300).Ticks;
                XAxes[0].MaxLimit = null;
            }
            else if (ct == CandleTypes.FifteenMin)
            {
                XAxes[0].Labeler = value => new DateTime((long)value).ToString("M/d H:mm");
                XAxes[0].UnitWidth = TimeSpan.FromMinutes(7).Ticks;
                //XAxes[0].MinStep = TimeSpan.FromMinutes(15).Ticks;
                XAxes[0].MinLimit = DateTime.Now.Ticks - TimeSpan.FromMinutes(750).Ticks;
                XAxes[0].MaxLimit = null;
            }
            else if (ct == CandleTypes.ThirtyMin)
            {
                XAxes[0].Labeler = value => new DateTime((long)value).ToString("M/d H:mm");
                XAxes[0].UnitWidth = TimeSpan.FromMinutes(15).Ticks;
                //XAxes[0].MinStep = TimeSpan.FromMinutes(30).Ticks;
                XAxes[0].MinLimit = DateTime.Now.Ticks - TimeSpan.FromMinutes(1500).Ticks;
                XAxes[0].MaxLimit = null;
            }
            else if (ct == CandleTypes.OneHour)
            {
                XAxes[0].Labeler = value => new DateTime((long)value).ToString("M/d H:mm");
                //XAxes[0].Labeler = value => new DateTime((long)value).ToString("yyyy MMM dd");
                XAxes[0].UnitWidth = TimeSpan.FromHours(0.5).Ticks;
                XAxes[0].MinStep = 0;
                XAxes[0].MinStep = TimeSpan.FromDays(0.4).Ticks;
                XAxes[0].MinLimit = DateTime.Now.Ticks - TimeSpan.FromDays(3).Ticks;
                XAxes[0].MaxLimit = null;
            }
            else if (ct == CandleTypes.FourHour)
            {
                XAxes[0].Labeler = value => new DateTime((long)value).ToString("M/d HH");
                XAxes[0].UnitWidth = TimeSpan.FromHours(2).Ticks;
                //XAxes[0].MinStep = TimeSpan.FromHours(4).Ticks;
                XAxes[0].MinLimit = DateTime.Now.Ticks - TimeSpan.FromDays(6).Ticks;
                XAxes[0].MaxLimit = null;
            }
            else if (ct == CandleTypes.EightHour)
            {
                XAxes[0].Labeler = value => new DateTime((long)value).ToString("M/d HH");
                XAxes[0].UnitWidth = TimeSpan.FromHours(4).Ticks;
                //XAxes[0].MinStep = TimeSpan.FromHours(8).Ticks;
                XAxes[0].MinLimit = DateTime.Now.Ticks - TimeSpan.FromDays(12).Ticks;
                XAxes[0].MaxLimit = null;
            }
            else if (ct == CandleTypes.TwelveHour)
            {
                XAxes[0].Labeler = value => new DateTime((long)value).ToString("yyyy M/d");
                XAxes[0].UnitWidth = TimeSpan.FromHours(6).Ticks;
                //XAxes[0].MinStep = TimeSpan.FromDays(0.5).Ticks;
                XAxes[0].MinLimit = DateTime.Now.Ticks - TimeSpan.FromDays(24).Ticks;
                XAxes[0].MaxLimit = null;
            }
            else if (ct == CandleTypes.OneDay)
            {
                XAxes[0].Labeler = value => new DateTime((long)value).ToString("yyyy M/d");
                XAxes[0].UnitWidth = TimeSpan.FromDays(0.4).Ticks;
                //XAxes[0].MinStep = TimeSpan.FromDays(1).Ticks;
                XAxes[0].MinLimit = DateTime.Now.Ticks - TimeSpan.FromDays(90).Ticks;
                XAxes[0].MaxLimit = null;
            }
            else if (ct == CandleTypes.OneWeek)
            {
                XAxes[0].Labeler = value => new DateTime((long)value).ToString("yyyy M/d");
                XAxes[0].UnitWidth = TimeSpan.FromDays(2).Ticks;
                //XAxes[0].MinStep = TimeSpan.FromDays(1).Ticks;
                XAxes[0].MinLimit = DateTime.Now.Ticks - TimeSpan.FromDays(300).Ticks;
                XAxes[0].MaxLimit = null;
            }
            else if (ct == CandleTypes.OneMonth)
            {
                XAxes[0].Labeler = value => new DateTime((long)value).ToString("yyyy/M");
                XAxes[0].UnitWidth = TimeSpan.FromDays(21).Ticks;
                //XAxes[0].MinStep = TimeSpan.FromDays(30).Ticks;
                XAxes[0].MinLimit = DateTime.Now.Ticks - TimeSpan.FromDays(360 * 3).Ticks;
                XAxes[0].MaxLimit = null;
            }

            var ohlcs = new ObservableCollection<FinancialPoint>();
            var vols = new ObservableCollection<DateTimePoint>();

            list.Sort((a, b) => a.TimeStamp.CompareTo(b.TimeStamp));
            _chartTimes.Clear();

            foreach (var hoge in list)
            {
                _chartTimes.Add(hoge.TimeStamp);

                vols.Add(new DateTimePoint(hoge.TimeStamp, (double)hoge.Volume));

                ohlcs.Add(new FinancialPoint(hoge.TimeStamp, (double)hoge.High, (double)hoge.Open, (double)hoge.Close, (double)hoge.Low));

                //Debug.WriteLine($"GetCandlestick: {hoge.TimeStamp:yyyy-MM-dd HH:mm:ss} Open: {hoge.Open}, High: {hoge.High}, Low: {hoge.Low}, Close: {hoge.Close}, Volume: {hoge.Volume}");
            }

            Series[0].Values = vols;
            Series[1].Values = ohlcs;

            IsChartInitAndLoaded = true;

            lastChartLoadedDateTime = DateTime.Now;

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"DoLoadChartAsync() exception for {PairCode}: {ex}");
        }
    }

    private async Task<List<Ohlcv>?> GetCandlesticksAsync(PairCodes pair, CandleTypes ct)
    {
        List<Ohlcv>? OhlcvList = [];

        try
        {
            //Debug.WriteLine("チャートデータを取得中.... " + pair.ToString());

            // 今日の日付セット。UTCで。// TODO: why UTC?
            var dtToday = DateTime.Now.ToUniversalTime();

            // データは、UTCで朝9:00 から翌8:59分まで。8:59分までしか取れないので、 9:00過ぎていたら 最新のデータとるには日付を１日追加する
            TimeSpan currentTime = DateTime.Now.TimeOfDay;
            TimeSpan targetTime = new TimeSpan(9, 0, 0); // 09:00:00
            var isPastNineAm = currentTime >= targetTime;

            var daysOrYearsCountToGetData = 0;

            var isYearly = false;

            if (ct == CandleTypes.OneMin)
            {
                if (isPastNineAm)
                {
                    daysOrYearsCountToGetData = 2;
                }
                else
                {
                    daysOrYearsCountToGetData = 1;
                }
            }
            else if (ct == CandleTypes.FiveMin)
            {
                if (isPastNineAm)
                {
                    daysOrYearsCountToGetData = 2;
                }
                else
                {
                    daysOrYearsCountToGetData = 1;
                }
            }
            else if (ct == CandleTypes.FifteenMin)
            {
                if (isPastNineAm)
                {
                    daysOrYearsCountToGetData = 4;
                }
                else
                {
                    daysOrYearsCountToGetData = 3;
                }
            }
            else if (ct == CandleTypes.ThirtyMin)
            {
                if (isPastNineAm)
                {
                    daysOrYearsCountToGetData = 4;
                }
                else
                {
                    daysOrYearsCountToGetData = 3;
                }
            }
            else if (ct == CandleTypes.OneHour)
            {
                if (isPastNineAm)
                {
                    daysOrYearsCountToGetData = 6;
                }
                else
                {
                    daysOrYearsCountToGetData = 5;
                }
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


            int i;

            for (i = 0; i < daysOrYearsCountToGetData;)
            {
                if (i <= 0)
                {
                    OhlcvList = await GetCandlestickAsync(pair, ct, dtToday);

                    if (OhlcvList != null)
                    {
                        OhlcvList.Reverse();
                    }
                    else
                    {
                        Debug.WriteLine($"1 failed to get candlestic. {pair}");
                        return OhlcvList;//  = new List<Ohlcv>(); 
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

                    var responseOhlcvList = await GetCandlestickAsync(pair, ct, dateTarget);

                    if (responseOhlcvList != null)
                    {
                        responseOhlcvList.Reverse();

                        foreach (var r in responseOhlcvList)
                        {
                            OhlcvList ??= [];
                            OhlcvList.Add(r);
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"2 failed to get candlestic. {pair}");
                        //OhlcvList.Clear();
                        return OhlcvList;
                    }
                }

                await Task.Delay(100);

                i++;
            }

            return OhlcvList;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetCandlesticksAsync() exception for {pair}: {ex}");
            return OhlcvList;
        }
    }

    private async Task<List<Ohlcv>?> GetCandlestickAsync(PairCodes pair, CandleTypes ct, DateTime dtTarget)
    {
        try
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

            //
            var csr = await _bitbankHttpSearvice.GetCandlestickAsync(pair.ToString(), ctString, dtString);

            if (!IsEnabled)
            {
                return null;
            }

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
                    Debug.WriteLine($"GetCandlestick for {pair}: GetCandlestick returned error");
                }
            }
            else
            {
                Debug.WriteLine($"GetCandlestick for {pair}: GetCandlestick returned null");
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetCandlestickAsync() exception for {pair}: {ex}");
            return null;
        }
    }

    // From Timer
    private async void OnTimerChart(object? source, object e)
    {
        _dispatcherService.TryEnqueue(async () =>
        {
            try
            {
                await UpdateChartAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnTimerChart() exception for {PairCode}: {ex}");
            }
        });
    }

    private async Task UpdateChartAsync()
    {
        if (!IsChartInitAndLoaded || !IsEnabled || _isChartRefreshRunning) 
        {
            // || !IsSelectedActive 
            // Let's update the chart even if it's not selected,
            // so that when the user switches to it, it's already updated.

            return;
        }

        _isChartRefreshRunning = true;

        try
        {
            // Replace this with a request for only the latest API period.
            // GetCandlesticksAsync currently downloads multiple days/years.
            var refreshed = await _bitbankHttpSearvice.GetLatestCandlesticksAsync(PairCode, SelectedCandleType);

            if (refreshed is null ||
                Series[1].Values is not ObservableCollection<FinancialPoint> prices ||
                Series[0].Values is not ObservableCollection<DateTimePoint> volumes)
            {
                return;
            }

            refreshed.Sort((a, b) => a.TimeStamp.CompareTo(b.TimeStamp));

            foreach (var candle in refreshed)
            {
                var index = _chartTimes.BinarySearch(candle.TimeStamp);

                var price = new FinancialPoint(
                    candle.TimeStamp,
                    (double)candle.High,
                    (double)candle.Open,
                    (double)candle.Close,
                    (double)candle.Low);

                var volume = new DateTimePoint(
                    candle.TimeStamp,
                    (double)candle.Volume);

                if (index >= 0)
                {
                    // Same timestamp: update the existing candle if it (close) has changed.
                    if ((prices[index].Close != price.Close) ||
                        (prices[index].Low != price.Low) ||
                        (prices[index].High != price.High) ||
                        (prices[index].Open != price.Open))
                    {
                        // Same timestamp: update the in-progress candle.

                        //prices[index] = price;
                        //volumes[index] = volume;
                        /*
                        prices[index].Low = price.Low;
                        prices[index].High = price.High;
                        prices[index].Open = price.Open ;
                        prices[index].Close = price.Close;
                        prices[index].Date = candle.TimeStamp;
                        */

                        // Let's just not update it because it does not cleanly update the value. It leaves visual glitch.

                        //Debug.WriteLine($"UpdateChartAsync Updated Candlestick for {PairCode}: {candle.TimeStamp:yyyy-MM-dd HH:mm:ss} Open: {candle.Open}, High: {candle.High}, Low: {candle.Low}, Close: {candle.Close}, Volume: {candle.Volume}");
                    }

                    if (volumes[index].Value != volume.Value)
                    {
                        volumes[index].Value = volume.Value;
                        //volumes[index].DateTime = volume.DateTime;
                        //volumes[index] = volume;
                    }
                }
                else
                {
                    // New timestamp: insert at its sorted position.
                    index = ~index;
                    _chartTimes.Insert(index, candle.TimeStamp);
                    prices.Insert(index, price);
                    volumes.Insert(index, volume);

                    //Debug.WriteLine($"Inserted new chart candle for {PairCode} at {candle.TimeStamp:yyyy-MM-dd HH:mm:ss}");
                    Debug.WriteLine($"UpdateChartAsync Inserted Candlestick for {PairCode}: {candle.TimeStamp:yyyy-MM-dd HH:mm:ss} Open: {candle.Open}, High: {candle.High}, Low: {candle.Low}, Close: {candle.Close}, Volume: {candle.Volume}");

                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"UpdateChartAsync() exception for {PairCode}: {ex}");
        }
        finally
        {
            _isChartRefreshRunning = false;
        }
    }

    // From Streaming Ticker Data
    private void UpdateCandleUsingTickerData(DateTime tickTime, BitbankTickerData data)
    {
        try
        {
            if (Series[1].Values is not ObservableCollection<FinancialPoint> candles) { return; }

            var candleStart = GetCandleStartDate(tickTime, SelectedCandleType);

            // Search from newest to oldest; the active candle should be near the end.
            for (var i = candles.Count - 1; i >= 0; i--)
            {
                var candle = candles[i];

                if (candle.Date != candleStart)
                {
                    //Debug.WriteLine($"UpdateCurrentCandle() skipping candle for {PairCode} at {candle.Date:yyyy-MM-dd HH:mm:ss} because it does not match the current candle start time {candleStart:yyyy-MM-dd HH:mm:ss}");
                    continue;
                }

                //Debug.WriteLine($"UpdateCurrentCandle() found existing candle for {PairCode} at {candleStart:yyyy-MM-dd HH:mm:ss} with price {lastPrice:N0}");

                var price = (double)data.Last;
                var high = Math.Max(candle.High, price);
                var low = Math.Min(candle.Low, price);
                var open = candle.Open;

                // Update the chart only if it has changed for performance reasons like high CPU usage.
                if ((candle.Close == price) && (candle.High == high) && (candle.Low == low) && (candle.Open == open))
                {
                    //Debug.WriteLine($"UpdateCurrentCandle() skipping candle update for {PairCode} at {candleStart:yyyy-MM-dd HH:mm:ss} because the close price is already {data.Last:N0}");
                    return;
                }

                Debug.WriteLine($"UpdateCurrentCandle() updating candle for {PairCode} at {candleStart:yyyy-MM-dd HH:mm:ss} with price {data.Last:N0}");
                //candles[i] = new FinancialPoint(candleStart, high, open, price, low);
                candle.Close = price;
                candle.High = high;
                candle.Low = low;
                //candle.Open = open;//candle.Open == 0 ? price : candle.Open;

                return;
            }

            Debug.WriteLine($"If we reach here, no existing candle matched; creating a new one. {PairCode} at {candleStart:yyyy-MM-dd HH:mm:ss} with price {data.Last:N0}");

            //DateTime timestamp = DateTimeOffset.FromUnixTimeMilliseconds(data.Timestamp).LocalDateTime;

            if (Series[0].Values is ObservableCollection<DateTimePoint> volumes)
            {
                // TODO: we need to load chart independently after all.
                // data.Vol is 24-hour rolling volume. So, we can't use it here.
                //volumes.Add(new DateTimePoint(candleStart, (double)data.Vol));
                // For now, just add a placeholder value of 0 for the volume.
                volumes.Add(new DateTimePoint(candleStart, 0));
            }
            if (Series[1].Values is ObservableCollection<FinancialPoint>)
            {
                candles.Add(new FinancialPoint(candleStart, (double)data.Last, (double)data.Last, (double)data.Last, (double)data.Last));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"UpdateCurrentCandle() exception for {PairCode}: {ex}");
            return;
        }

    }

    private static DateTime GetCandleStartDate(DateTime time, CandleTypes candleType)
    {
        var interval = candleType switch
        {
            CandleTypes.OneMin => TimeSpan.FromMinutes(1),
            CandleTypes.FiveMin => TimeSpan.FromMinutes(5),
            CandleTypes.FifteenMin => TimeSpan.FromMinutes(15),
            CandleTypes.ThirtyMin => TimeSpan.FromMinutes(30),
            CandleTypes.OneHour => TimeSpan.FromHours(1),
            CandleTypes.FourHour => TimeSpan.FromHours(4),
            CandleTypes.EightHour => TimeSpan.FromHours(8),
            CandleTypes.TwelveHour => TimeSpan.FromHours(12),
            _ => TimeSpan.Zero
        };

        if (interval != TimeSpan.Zero)
        {
            var startTicks = time.Ticks / interval.Ticks * interval.Ticks;
            return new DateTime(startTicks, time.Kind);
        }

        if (candleType == CandleTypes.OneDay)
        {
            return time.Date;
        }

        if (candleType == CandleTypes.OneWeek)
        {
            var daysSinceMonday = ((int)time.DayOfWeek + 6) % 7;
            return time.Date.AddDays(-daysSinceMonday);
        }

        if (candleType == CandleTypes.OneMonth)
        {
            return new DateTime(time.Year, time.Month, 1, 0, 0, 0, time.Kind);
        }

        return time;
    }

    #endregion

    #region == Depth ==

    private void UpdateDepth(DepthResult dpr)
    {
        if (!IsSelectedActive|| !IsEnabled)
        {
            // || !IsPaneOpen 
            Debug.WriteLine($"UpdateDepth() skipped for {PairCode} because IsSelectedActive={IsSelectedActive}, IsEnabled={IsEnabled}");
            return;
        }

        // まとめグルーピング単位 
        var unit = DepthGrouping;

        // リスト数 （基本 上売り200、下買い200）
        var half = 200;
        var listCount = (half * 2) + 1;

        if (_depth == null)
        {
            return;
        }

        // 初期化
        if ((_depth.Count == 0) || (_depth.Count < listCount))
        {
            _depth.Clear();
            for (var x = 0; x < listCount; x++)
            {
                Depth dd = new(_ltpFormstString)
                {
                    DepthPrice = x,
                    DepthBid = 0,
                    DepthAsk = 0
                };
                //if (i == (half-1)) dd.IsLTP = true;
                _depth.Add(dd);
            }

            // scroll center
            _dispatcherService?.TryEnqueue(() =>
            {
                DepthScrollCenter?.Invoke(this, true);
            });
        }
        else
        {
            if (IsDepthGroupingChanged)
            {
                //グルーピング単位が変わったので、一旦クリアする。
                for (var v = 0; v < _depth.Count - 1; v++)
                {
                    _depth[v].DepthPrice = 0;
                    _depth[v].DepthBid = 0;
                    _depth[v].DepthAsk = 0;
                }

                IsDepthGroupingChanged = false;
            }
        }

        // LTP を追加
        _depth[half].DepthPrice = Ltp;
        _depth[half].IsLTP = true;

        if (!IsEnabled)
        {
            return;
        }

        if (_depth == null)
        {
            return;
        }

        if (_depth.Count == 0)
        {
            return;
        }

        var i = 1;

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
            if (i > listCount)
            {
                break;
            }

            if (i > half)
            {
                break;
            }

            // まとめ表示On
            if (unit > 0)
            {
                if (c2 == 0)
                {
                    c2 = Math.Ceiling(dp.DepthPrice / unit);
                }

                // 100円単位でまとめる
                if (Math.Ceiling(dp.DepthPrice / unit) == c2)
                {
                    t += dp.DepthAsk;
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
            if (i >= listCount)
            {
                break;
            }

            if (unit > 0)
            {
                if (c == 0)
                {
                    c = Math.Ceiling(dp.DepthPrice / unit);
                }

                // 100円単位でまとめる
                if (Math.Ceiling(dp.DepthPrice / unit) == c)
                {
                    t += dp.DepthBid;
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

    #endregion

    #region == Transaction ==

    #endregion

    #endregion

    #region == Commands ==
    /*
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
    public static bool ChangeCandleTypeCommand_CanExecute()
    {
        return true;
    }
    public void ChangeCandleTypeCommand_Execute(CandleTypes candleType)
    {
        ChangeCandleType(candleType);
    }
    */

    [RelayCommand]
    private void TogglePaneVisibility()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    [RelayCommand]
    private async Task ChangeCandleType(CandleTypes candleType)
    {
        if (candleType == SelectedCandleType)
        {
            return;
        }

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

        await LoadChartAsync(SelectedCandleType);
    }

    #endregion

}
