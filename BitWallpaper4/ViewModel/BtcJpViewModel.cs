using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using Microsoft.UI.Xaml;
using System.Threading;
using Windows.UI.Core;
using BitWallpaper4.Models;
using BitWallpaper4.Models.APIClients;
using System.Diagnostics;
using System.Windows.Markup;
using LiveChartsCore.Measure;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Linq;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.Painting.Effects;

namespace BitWallpaper4.ViewModels;

public partial class BtcJpViewModel : INotifyPropertyChanged
{

    readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
    public event PropertyChangedEventHandler PropertyChanged;

    protected void NotifyPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        _dispatcherQueue.TryEnqueue(() =>
        {
            //this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        });
    }

    public ICartesianAxis[] XAxes
    {
        get; set;

    } =
    {
    new Axis()
    {
        LabelsRotation = -15,
        LabelsPaint = new SolidColorPaint(SKColors.Wheat),
        Labeler = value => new DateTime((long) value).ToString("MM/dd HH:mm"),
        UnitWidth = TimeSpan.FromMinutes(15).Ticks,
        MinStep = TimeSpan.FromHours(1).Ticks, 

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
        MinLimit= DateTime.Now.Ticks - TimeSpan.FromDays(3).Ticks,
        //Position = AxisPosition.Start  
        //MinLimit = 0

        // The MinStep property forces the separator to be greater than 1 day.
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
        LabelsPaint = new SolidColorPaint(SKColors.Wheat),
        Position = LiveChartsCore.Measure.AxisPosition.End,
        //Labeler =(value) => (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)value).ToString("hh:mm"),
        SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
                    {
                        StrokeThickness = 1,
                        PathEffect = new DashEffect(new float[] { 3, 3 })
                    },
        //Labeler = Labelers.Currency,
        Labeler = (value) => value.ToString("C"),
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

    private ISeries[] series =  { new CandlesticksSeries<FinancialPoint>
            {
                Values = new ObservableCollection<FinancialPoint>
                {
                    new(DateTime.Now, 523, 500, 450, 400),
                    /*
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
                    */
                }
            }
    };


        //{ new CandlesticksSeries<FinancialPoint>() };// = new CandlesticksSeries<FinancialPoint>[1] ;
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

    #region == BTCチャートデータ用のプロパティ ==

    // === BTC === 
    private ObservableCollection<FinancialPoint> _chartSeriesBtcJpy = new();
    public ObservableCollection<FinancialPoint> ChartSeriesBtcJpy
    {
        get
        {
            return _chartSeriesBtcJpy;
        }
        set
        {
            if (_chartSeriesBtcJpy == value)
                return;

            _chartSeriesBtcJpy = value;
            //this.NotifyPropertyChanged("ChartSeriesBtcJpy");
        }
    }

    /*
    private AxesCollection _chartAxisXBtcJpy = new AxesCollection();
    public AxesCollection ChartAxisXBtcJpy
    {
        get
        {
            return _chartAxisXBtcJpy;
        }
        set
        {
            if (_chartAxisXBtcJpy == value)
                return;

            _chartAxisXBtcJpy = value;
            //this.NotifyPropertyChanged("ChartAxisXBtcJpy");
        }
    }

    private AxesCollection _chartAxisYBtcJpy = new AxesCollection();
    public AxesCollection ChartAxisYBtcJpy
    {
        get
        {
            return _chartAxisYBtcJpy;
        }
        set
        {
            if (_chartAxisYBtcJpy == value)
                return;

            _chartAxisYBtcJpy = value;
            //this.NotifyPropertyChanged("ChartAxisYBtcJpy");
        }
    }
    */

    // 一時間単位 
    private List<Ohlcv> _ohlcvsOneHourBtc = new List<Ohlcv>();
    public List<Ohlcv> OhlcvsOneHourBtc
    {
        get
        {
            return _ohlcvsOneHourBtc;
        }
        set
        {
            _ohlcvsOneHourBtc = value;
            //this.NotifyPropertyChanged("OhlcvsOneHourBtc");
        }
    }

    // 一分単位 
    private List<Ohlcv> _ohlcvsOneMinBtc = new List<Ohlcv>();
    public List<Ohlcv> OhlcvsOneMinBtc
    {
        get
        {
            return _ohlcvsOneMinBtc;
        }
        set
        {
            _ohlcvsOneMinBtc = value;
            //this.NotifyPropertyChanged("OhlcvsOneMinBtc");
        }
    }

    // 一日単位 
    private List<Ohlcv> _ohlcvsOneDayBtc = new List<Ohlcv>();
    public List<Ohlcv> OhlcvsOneDayBtc
    {
        get
        {
            return _ohlcvsOneDayBtc;
        }
        set
        {
            _ohlcvsOneDayBtc = value;
            //this.NotifyPropertyChanged("OhlcvsOneDayBtc");
        }
    }

    #endregion




    // 公開API ロウソク足 クライアント
    readonly PublicAPIClient _pubCandlestickApi = new();

    // 公開API Ticker クライアント
    //PublicAPIClient _pubTickerApi = new PublicAPIClient();

    // 公開API Depth クライアント
    readonly PublicAPIClient _pubDepthApi = new();

    // 公開API Transactions クライアント
    readonly PublicAPIClient _pubTransactionsApi = new();

    readonly DispatcherTimer _dispatcherChartTimer = new();

    public BtcJpViewModel()
    {

        // Chart更新のタイマー
        _dispatcherChartTimer.Tick += ChartTimer;
        _dispatcherChartTimer.Interval = new TimeSpan(0, 1, 0);


        // temp

        //UpdateCandlestick(Pairs.btc_jpy, CandleTypes.OneDay);
        //GetCandlesticks(Pairs.btc_jpy, CandleTypes.OneDay);


        DisplayCharts(CandleTypes.OneHour);
        //Debug.WriteLine("sasadf");
    }

    private void ChartTimer(object? source, object e)
    {
        try
        {
            // 各通貨ペアをループ
            foreach (Pairs pair in Enum.GetValues(typeof(Pairs)))
            {

                //UpdateCandlestick(pair, SelectedCandleType);

            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("■■■■■ ChartTimer Exception: " + ex);
        }
    }

    private async void DisplayCharts(CandleTypes selectedCandleType)
    {
        //foreach (DayOfWeek value in Enum.GetValues(typeof(DayOfWeek)))
        foreach (Pairs p in Enum.GetValues(typeof(Pairs)))
        {
            bool bln = await GetCandlesticks(p, selectedCandleType);

            if (bln == true)
            {
                LoadChart(p, selectedCandleType);
            }

            //temp
            break;
        }

    }

    // 初回に各種Candlestickをまとめて取得
    private async Task<bool> GetCandlesticks(Pairs pair, CandleTypes ct)
    {
        //ChartLoadingInfo = "チャートデータを取得中....";

        Debug.WriteLine("チャートデータを取得中.... " + pair.ToString());

        // 今日の日付セット。UTCで。
        DateTime dtToday = DateTime.Now.ToUniversalTime();

        // データは、ローカルタイムで、朝9:00 から翌8:59分まで。8:59分までしか取れないので、 9:00過ぎていたら 最新のデータとるには日付を１日追加する

        #region == OhlcvsOneHour 1hour毎のデータ ==

        List<Ohlcv> ListOhlcvsOneHour = new List<Ohlcv>();

        if (ct == CandleTypes.OneHour)
        {
            //Debug.WriteLine("今日の1hour取得開始 " + pair.ToString());

            // 一時間のロウソク足タイプなら今日、昨日、一昨日、その前の１週間分の1hourデータを取得する必要あり。
            ListOhlcvsOneHour = await GetCandlestick(pair, CandleTypes.OneHour, dtToday);
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

        if (pair == Pairs.btc_jpy)
        {
            if (ListOhlcvsOneHour != null)
                OhlcvsOneHourBtc = ListOhlcvsOneHour;
            if (ListOhlcvsOneMin != null)
                OhlcvsOneMinBtc = ListOhlcvsOneMin;
            if (ListOhlcvsOneDay != null)
                OhlcvsOneDayBtc = ListOhlcvsOneDay;
        }
        /*
        else if (pair == Pairs.xrp_jpy)
        {
            if (ListOhlcvsOneHour != null)
                OhlcvsOneHourXrp = ListOhlcvsOneHour;
            if (ListOhlcvsOneMin != null)
                OhlcvsOneMinXrp = ListOhlcvsOneMin;
            if (ListOhlcvsOneDay != null)
                OhlcvsOneDayXrp = ListOhlcvsOneDay;
        }
        else if (pair == Pairs.eth_jpy)
        {
            if (ListOhlcvsOneHour != null)
                OhlcvsOneHourEth = ListOhlcvsOneHour;
            if (ListOhlcvsOneMin != null)
                OhlcvsOneMinEth = ListOhlcvsOneMin;
            if (ListOhlcvsOneDay != null)
                OhlcvsOneDayEth = ListOhlcvsOneDay;
        }
        else if (pair == Pairs.mona_jpy)
        {
            if (ListOhlcvsOneHour != null)
                OhlcvsOneHourMona = ListOhlcvsOneHour;
            if (ListOhlcvsOneMin != null)
                OhlcvsOneMinMona = ListOhlcvsOneMin;
            if (ListOhlcvsOneDay != null)
                OhlcvsOneDayMona = ListOhlcvsOneDay;
        }
        else if (pair == Pairs.ltc_jpy)
        {
            if (ListOhlcvsOneHour != null)
                OhlcvsOneHourLtc = ListOhlcvsOneHour;
            if (ListOhlcvsOneMin != null)
                OhlcvsOneMinLtc = ListOhlcvsOneMin;
            if (ListOhlcvsOneDay != null)
                OhlcvsOneDayLtc = ListOhlcvsOneDay;
        }
        else if (pair == Pairs.bcc_jpy)
        {
            if (ListOhlcvsOneHour != null)
                OhlcvsOneHourBch = ListOhlcvsOneHour;
            if (ListOhlcvsOneMin != null)
                OhlcvsOneMinBch = ListOhlcvsOneMin;
            if (ListOhlcvsOneDay != null)
                OhlcvsOneDayBch = ListOhlcvsOneDay;
        }
        else if (pair == Pairs.xlm_jpy)
        {
            if (ListOhlcvsOneHour != null)
                OhlcvsOneHourXlm = ListOhlcvsOneHour;
            if (ListOhlcvsOneMin != null)
                OhlcvsOneMinXlm = ListOhlcvsOneMin;
            if (ListOhlcvsOneDay != null)
                OhlcvsOneDayXlm = ListOhlcvsOneDay;
        }
        else if (pair == Pairs.qtum_jpy)
        {
            if (ListOhlcvsOneHour != null)
                OhlcvsOneHourQtum = ListOhlcvsOneHour;
            if (ListOhlcvsOneMin != null)
                OhlcvsOneMinQtum = ListOhlcvsOneMin;
            if (ListOhlcvsOneDay != null)
                OhlcvsOneDayQtum = ListOhlcvsOneDay;
        }
        else if (pair == Pairs.bat_jpy)
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
    private async Task<List<Ohlcv>> GetCandlestick(Pairs pair, CandleTypes ct, DateTime dtTarget)
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

    // チャートのロード
    private async void LoadChart(Pairs pair, CandleTypes ct)
    {

        var fuga = new CandlesticksSeries<FinancialPoint>();
        var test = new ObservableCollection<FinancialPoint>();


        //Ohlcv asdf = new Ohlcv();

        foreach (var hoge in OhlcvsOneHourBtc)
        {
            Debug.WriteLine(hoge.TimeStamp.ToString());

            var asdf = new FinancialPoint(hoge.TimeStamp, (double)hoge.High, (double)hoge.Open, (double)hoge.Close, (double)hoge.Low);
            //asdf.TimeStamp = hoge.TimeStamp;
            //asdf.Volume = hoge.Volume;
            //asdf.Close = hoge.Close;
            //asdf.Low = hoge.Low;
            //asdf.High = hoge.High;
            test.Add(asdf);
            
            //test.Add(hoge);
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
        Series[0].Values = test;
        //await Task.Delay(1000);

        //Thread.Sleep(2000);
        _dispatcherQueue.TryEnqueue(() =>
        {
            //Series[0] = fuga;

            //Series= new ISeries[1];
            //Series[0] = fuga;
            //Series = new ISeries[1] { fuga };
            //Series = new ISeries[1] { fuga };
        });

        //await Task.Delay(100);

        //Series = new ISeries[1] { fuga };
        this.NotifyPropertyChanged("Series");
        //Thread.Sleep(2000);
        //await Task.Delay(100);
    }

    // タイマーで、最新のロウソク足データを取得して追加する。
    private async void UpdateCandlestick(Pairs pair, CandleTypes ct)
    {
        //ChartLoadingInfo = "チャートデータの更新中....";
        //await Task.Delay(600);

        // 今日の日付セット。UTCで。
        DateTime dtToday = DateTime.Now.ToUniversalTime();

        DateTime dtLastUpdate;


        List<Ohlcv> ListOhlcvsOneMin = null;
        List<Ohlcv> ListOhlcvsOneHour = null;
        List<Ohlcv> ListOhlcvsOneDay = null;

        if (pair == Pairs.btc_jpy)
        {
            ListOhlcvsOneHour = OhlcvsOneHourBtc;
            ListOhlcvsOneMin = OhlcvsOneMinBtc;
            ListOhlcvsOneDay = OhlcvsOneDayBtc;
        }
        /*
        else if (pair == Pairs.xrp_jpy)
        {
            ListOhlcvsOneHour = OhlcvsOneHourXrp;
            ListOhlcvsOneMin = OhlcvsOneMinXrp;
            ListOhlcvsOneDay = OhlcvsOneDayXrp;
        }
        else if (pair == Pairs.eth_jpy)
        {
            ListOhlcvsOneHour = OhlcvsOneHourEth;
            ListOhlcvsOneMin = OhlcvsOneMinEth;
            ListOhlcvsOneDay = OhlcvsOneDayEth;
        }
        else if (pair == Pairs.mona_jpy)
        {
            ListOhlcvsOneHour = OhlcvsOneHourMona;
            ListOhlcvsOneMin = OhlcvsOneMinMona;
            ListOhlcvsOneDay = OhlcvsOneDayMona;
        }
        else if (pair == Pairs.ltc_jpy)
        {
            ListOhlcvsOneHour = OhlcvsOneHourLtc;
            ListOhlcvsOneMin = OhlcvsOneMinLtc;
            ListOhlcvsOneDay = OhlcvsOneDayLtc;
        }
        else if (pair == Pairs.bcc_jpy)
        {
            ListOhlcvsOneHour = OhlcvsOneHourBch;
            ListOhlcvsOneMin = OhlcvsOneMinBch;
            ListOhlcvsOneDay = OhlcvsOneDayBch;
        }
        else if (pair == Pairs.xlm_jpy)
        {
            ListOhlcvsOneHour = OhlcvsOneHourXlm;
            ListOhlcvsOneMin = OhlcvsOneMinXlm;
            ListOhlcvsOneDay = OhlcvsOneDayXlm;
        }
        else if (pair == Pairs.qtum_jpy)
        {
            ListOhlcvsOneHour = OhlcvsOneHourQtum;
            ListOhlcvsOneMin = OhlcvsOneMinQtum;
            ListOhlcvsOneDay = OhlcvsOneDayQtum;
        }
        else if (pair == Pairs.bat_jpy)
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
    private void AddCandle(Pairs pair, CandleTypes ct, Ohlcv newData)
    {
        // 表示されているのだけ更新。それ以外は不要。
        //if (SelectedCandleType != ct) return;

        //Debug.WriteLine("チャートの更新 追加: "+ newData.TimeStamp.ToString());

        /*
        SeriesCollection chartSeries = null;
        AxesCollection chartAxisX = null;
        AxesCollection chartAxisY = null;

        if (pair == Pairs.btc_jpy)
        {
            chartSeries = ChartSeriesBtcJpy;
            chartAxisX = ChartAxisXBtcJpy;
            chartAxisY = ChartAxisYBtcJpy;
        }
        else if (pair == Pairs.xrp_jpy)
        {
            chartSeries = ChartSeriesXrpJpy;
            chartAxisX = ChartAxisXXrpJpy;
            chartAxisY = ChartAxisYXrpJpy;
        }
        else if (pair == Pairs.eth_jpy)
        {
            chartSeries = ChartSeriesEthJpy;
            chartAxisX = ChartAxisXEthJpy;
            chartAxisY = ChartAxisYEthJpy;
        }
        else if (pair == Pairs.mona_jpy)
        {
            chartSeries = ChartSeriesMonaJpy;
            chartAxisX = ChartAxisXMonaJpy;
            chartAxisY = ChartAxisYMonaJpy;
        }
        else if (pair == Pairs.ltc_jpy)
        {
            chartSeries = ChartSeriesLtcJpy;
            chartAxisX = ChartAxisXLtcJpy;
            chartAxisY = ChartAxisYLtcJpy;
        }
        else if (pair == Pairs.bcc_jpy)
        {
            chartSeries = ChartSeriesBchJpy;
            chartAxisX = ChartAxisXBchJpy;
            chartAxisY = ChartAxisYBchJpy;
        }
        else if (pair == Pairs.xlm_jpy)
        {
            chartSeries = ChartSeriesXlmJpy;
            chartAxisX = ChartAxisXXlmJpy;
            chartAxisY = ChartAxisYXlmJpy;
        }
        else if (pair == Pairs.qtum_jpy)
        {
            chartSeries = ChartSeriesQtumJpy;
            chartAxisX = ChartAxisXQtumJpy;
            chartAxisY = ChartAxisYQtumJpy;
        }
        else if (pair == Pairs.bat_jpy)
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
    private void UpdateLastCandle(Pairs pair, CandleTypes ct, Ohlcv newData)
    {
        // 表示されているのだけ更新。それ以外は不要。
        //if (SelectedCandleType != ct) return;

        //Debug.WriteLine("チャートの更新 追加: "+ newData.TimeStamp.ToString());

        /*
        SeriesCollection chartSeries = null;
        AxesCollection chartAxisX = null;
        AxesCollection chartAxisY = null;

        if (pair == Pairs.btc_jpy)
        {
            chartSeries = ChartSeriesBtcJpy;
            chartAxisX = ChartAxisXBtcJpy;
            chartAxisY = ChartAxisYBtcJpy;
        }
        else if (pair == Pairs.xrp_jpy)
        {
            chartSeries = ChartSeriesXrpJpy;
            chartAxisX = ChartAxisXXrpJpy;
            chartAxisY = ChartAxisYXrpJpy;
        }
        else if (pair == Pairs.eth_jpy)
        {
            chartSeries = ChartSeriesEthJpy;
            chartAxisX = ChartAxisXEthJpy;
            chartAxisY = ChartAxisYEthJpy;
        }
        else if (pair == Pairs.mona_jpy)
        {
            chartSeries = ChartSeriesMonaJpy;
            chartAxisX = ChartAxisXMonaJpy;
            chartAxisY = ChartAxisYMonaJpy;
        }
        else if (pair == Pairs.ltc_jpy)
        {
            chartSeries = ChartSeriesLtcJpy;
            chartAxisX = ChartAxisXLtcJpy;
            chartAxisY = ChartAxisYLtcJpy;
        }
        else if (pair == Pairs.bcc_jpy)
        {
            chartSeries = ChartSeriesBchJpy;
            chartAxisX = ChartAxisXBchJpy;
            chartAxisY = ChartAxisYBchJpy;
        }
        else if (pair == Pairs.xlm_jpy)
        {
            chartSeries = ChartSeriesXlmJpy;
            chartAxisX = ChartAxisXXlmJpy;
            chartAxisY = ChartAxisYXlmJpy;
        }
        else if (pair == Pairs.qtum_jpy)
        {
            chartSeries = ChartSeriesQtumJpy;
            chartAxisX = ChartAxisXQtumJpy;
            chartAxisY = ChartAxisYQtumJpy;
        }
        else if (pair == Pairs.bat_jpy)
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


}

