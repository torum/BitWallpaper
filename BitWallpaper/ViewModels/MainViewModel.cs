using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using BitWallpaper.Models.Clients;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.IO;
using System.ComponentModel;

namespace BitWallpaper.ViewModels
{

    public class MainViewModel : ViewModelBase
    {

        // Application version
        private string _appVer = "0.0.0.1";

        // Application name
        private string _appName = "BitWallpaper";

        // Application config file folder
        private string _appDeveloper = "torum";

        // Application Window Title
        public string AppTitle
        {
            get
            {
                return _appName + " " + _appVer;
            }
        }


        #region == 通貨ペア切り替え用のプロパティ ==

        // 通貨ペア
        public enum Pairs
        {
            btc_jpy, xrp_jpy, ltc_btc, eth_btc, mona_jpy, mona_btc, bcc_jpy, bcc_btc
        }

        // チャート表示期間
        public enum ChartSpans
        {
            OneHour, ThreeHour, OneDay, ThreeDay, OneWeek, OneMonth, TwoMonth, OneYear, FiveYear
        }

        // 注文　指値・成行
        public enum OrderTypes
        {
            limit, market
        }

        // 現在の通貨ペア
        private Pairs _currentPair = Pairs.btc_jpy;//"btc_jpy";
        public Pairs CurrentPair
        {
            get
            {
                return _currentPair;
            }
            set
            {
                if (_currentPair == value)
                    return;

                _currentPair = value;

                this.NotifyPropertyChanged("CurrentPair");
                this.NotifyPropertyChanged("CurrentPairString");

                this.NotifyPropertyChanged("CoinString");


            }
        }

        // 表示用 通貨ペア名 "BTC/JPY";
        public string CurrentPairString
        {
            get
            {
                return PairStrings[CurrentPair];
            }
        }

        public Dictionary<Pairs, decimal> Ltps { get; set; } = new Dictionary<Pairs, decimal>()
        {
            {Pairs.btc_jpy, 0},
            {Pairs.xrp_jpy, 0},
            {Pairs.ltc_btc, 0},
            {Pairs.eth_btc, 0},
            {Pairs.mona_jpy, 0},
            {Pairs.mona_btc, 0},
            {Pairs.bcc_jpy, 0},
            {Pairs.bcc_btc, 0},
        };

        public Dictionary<Pairs, string> PairStrings { get; set; } = new Dictionary<Pairs, string>()
        {
            {Pairs.btc_jpy, "BTC/JPY"},
            {Pairs.xrp_jpy, "XRP/JPY"},
            {Pairs.eth_btc, "ETH/BTC"},
            {Pairs.ltc_btc, "LTC/BTC"},
            {Pairs.mona_jpy, "MONA/JPY"},
            {Pairs.mona_btc, "MONA/BTC"},
            {Pairs.bcc_jpy, "BCH/JPY"},
            {Pairs.bcc_btc, "BCH/BTC"},
        };

        public Dictionary<string, Pairs> GetPairs { get; set; } = new Dictionary<string, Pairs>()
        {
            {"btc_jpy", Pairs.btc_jpy},
            {"xrp_jpy", Pairs.xrp_jpy},
            {"eth_btc", Pairs.eth_btc},
            {"ltc_btc", Pairs.ltc_btc},
            {"mona_jpy", Pairs.mona_jpy},
            {"mona_btc", Pairs.mona_btc},
            {"bcc_jpy", Pairs.bcc_jpy},
            {"bcc_btc", Pairs.bcc_btc},
        };

        // 表示用 通貨 単位
        public string CoinString
        {
            get
            {
                return CurrentPairUnits[CurrentPair].ToUpper();//_coin.ToUpper();
            }
        }

        public Dictionary<Pairs, string> CurrentPairUnits { get; set; } = new Dictionary<Pairs, string>()
        {
            {Pairs.btc_jpy, "btc"},
            {Pairs.xrp_jpy, "xrp"},
            {Pairs.eth_btc, "eth"},
            {Pairs.ltc_btc, "ltc"},
            {Pairs.mona_jpy, "mona"},
            {Pairs.mona_btc, "mona"},
            {Pairs.bcc_jpy, "bch"},
            {Pairs.bcc_btc, "bch"},
        };

        // アルトコインの最新取引価格

        public decimal LtpBtc
        {
            get
            {
                return Ltps[Pairs.btc_jpy];
            }
        }

        public decimal LtpXrp
        {
            get
            {
                return Ltps[Pairs.xrp_jpy];
            }
        }

        public decimal LtpEthBtc
        {
            get
            {
                return Ltps[Pairs.eth_btc];
            }
        }

        public decimal LtpLtcBtc
        {
            get
            {
                return Ltps[Pairs.ltc_btc];
            }
        }

        public decimal LtpMonaJpy
        {
            get
            {
                return Ltps[Pairs.mona_jpy];
            }
        }

        public decimal LtpMonaBtc
        {
            get
            {
                return Ltps[Pairs.mona_btc];
            }
        }

        public decimal LtpBchJpy
        {
            get
            {
                return Ltps[Pairs.bcc_jpy];
            }
        }

        public decimal LtpBchBtc
        {
            get
            {
                return Ltps[Pairs.bcc_btc];
            }
        }

        #endregion

        #region == クライアントのプロパティ ==

        // 公開API ロウソク足 クライアント
        PublicAPIClient _pubCandlestickApi = new PublicAPIClient();

        // 公開API Ticker クライアント
        PublicAPIClient _pubTickerApi = new PublicAPIClient();

        #endregion

        #region == チャート用のプロパティ ==

        // ロウソク足タイプ
        public enum CandleTypes
        {
            OneMin, FiveMin, FifteenMin, ThirteenMin, OneHour, FourHour, EightHour, TwelveHour, OneDay, OneWeek
        }

        // ロウソク足タイプ　コンボボックス表示用
        public Dictionary<CandleTypes, string> CandleTypesDictionary { get; } = new Dictionary<CandleTypes, string>()
        {
            {CandleTypes.OneMin, "1m"},
            //{CandleTypes.FiveMin, "５分" },
            //{CandleTypes.FifteenMin, "１５分"},
            //{CandleTypes.ThirteenMin, "３０分" },
            {CandleTypes.OneHour, "1h" },
            //{CandleTypes.FourHour, "４時間"},
            //{CandleTypes.EightHour, "８時間" },
            //{CandleTypes.TwelveHour, "１２時間"},
            {CandleTypes.OneDay, "1d" },
            //{CandleTypes.OneWeek, "１週間"},

        };

        // 選択されたロウソク足タイプ
        public CandleTypes _selectedCandleType = CandleTypes.OneHour; // デフォ //CandleTypes.OneMin;
        public CandleTypes SelectedCandleType
        {
            get
            {
                return _selectedCandleType;
            }
            set
            {
                if (_selectedCandleType == value)
                    return;

                _selectedCandleType = value;
                this.NotifyPropertyChanged("SelectedCandleType");
                //this.NotifyPropertyChanged("SelectedCandleType");

                // 
                if (_selectedCandleType == CandleTypes.OneMin)
                {
                    // 一分毎のロウソク足タイプなら

                    if ((SelectedChartSpan != ChartSpans.OneHour) && (SelectedChartSpan != ChartSpans.ThreeHour))
                    {
                        // デフォルト 一時間の期間で表示
                        SelectedChartSpan = ChartSpans.OneHour;
                        //return;
                    }

                    // または、3時間
                    //SelectedChartSpan = ChartSpans.ThreeHour;

                    // 負荷掛かり過ぎなので無し　>または、１日の期間
                    //SelectedChartSpan = ChartSpans.OneDay;

                    // つまり、今日と昨日の1minデータを取得する必要あり。

                }
                else if (_selectedCandleType == CandleTypes.OneHour)
                {
                    // 一時間のロウソク足タイプなら

                    if ((SelectedChartSpan != ChartSpans.ThreeDay) && (SelectedChartSpan != ChartSpans.OneDay) && (SelectedChartSpan != ChartSpans.OneWeek))
                    {
                        // デフォルト 3日の期間で表示
                        SelectedChartSpan = ChartSpans.ThreeDay;
                        //return;
                    }


                    // または、１日の期間
                    //SelectedChartSpan = ChartSpans.OneDay;
                    // または、１週間の期間
                    //SelectedChartSpan = ChartSpans.OneWeek;

                    // つまり、今日、昨日、一昨日、その前の1hourデータを取得する必要あり。

                }
                else if (_selectedCandleType == CandleTypes.OneDay)
                {
                    // 1日のロウソク足タイプなら

                    if ((SelectedChartSpan != ChartSpans.OneMonth) && (SelectedChartSpan != ChartSpans.TwoMonth) && (SelectedChartSpan != ChartSpans.OneYear) && (SelectedChartSpan != ChartSpans.FiveYear))
                    {
                        // デフォルト 1ヵ月の期間で表示
                        SelectedChartSpan = ChartSpans.TwoMonth;
                        //return;
                    }

                    //
                    //SelectedChartSpan = ChartSpans.TwoMonth;

                    //SelectedChartSpan = ChartSpans.OneYear;

                    //SelectedChartSpan = ChartSpans.FiveYear;

                    // つまり、今年、去年、２年前、３年前、４年前、５年前の1hourデータを取得する必要あり。

                }
                else
                {
                    return;

                    // デフォルト 1日の期間で表示
                    //SelectedChartSpan = ChartSpans.OneDay;
                    //Debug.WriteLine("デフォルト Oops");

                }


                // チャート表示
                //LoadChart();
                DisplayCharts();

            }
        }

        private string _chartLoadingInfo;
        public string ChartLoadingInfo
        {
            get
            {
                return _chartLoadingInfo;
            }
            set
            {
                if (_chartLoadingInfo == value)
                    return;

                _chartLoadingInfo = value;

                this.NotifyPropertyChanged("ChartLoadingInfo");

            }
        }

        // 選択されたチャート表示期間 
        private ChartSpans _chartSpan = ChartSpans.ThreeDay; // デフォ //ChartSpans.OneHour;
        public ChartSpans SelectedChartSpan
        {
            get
            {
                return _chartSpan;
            }
            set
            {
                if (_chartSpan == value)
                    return;

                _chartSpan = value;
                this.NotifyPropertyChanged("SelectedChartSpan");

                // チャート表示アップデート >// TODO ダブルアップデートになってしまう。
                //LoadChart();

                // チャート表示アップデート
                //ChangeChartSpan();

            }
        }

        private ZoomingOptions _zoomingMode = ZoomingOptions.X;
        public ZoomingOptions ZoomingMode
        {
            get { return _zoomingMode; }
            set
            {
                _zoomingMode = value;
                this.NotifyPropertyChanged("ZoomingMode");
            }
        }

        /*
        // チャートデータ保持
        // 一日単位 今年、去年、２年前、３年前、４年前、５年前の1hourデータを取得する必要あり。
        private List<Ohlcv> _ohlcvsOneDay = new List<Ohlcv>();
        public List<Ohlcv> OhlcvsOneDay
        {
            get { return _ohlcvsOneDay; }
            set
            {
                _ohlcvsOneDay = value;
                this.NotifyPropertyChanged("Ohlcvs");
            }
        }

        // 一時間単位 今日、昨日、一昨日、その前の1hourデータを取得する必要あり。
        private List<Ohlcv> _ohlcvsOneHour = new List<Ohlcv>();
        public List<Ohlcv> OhlcvsOneHour
        {
            get { return _ohlcvsOneHour; }
            set
            {
                _ohlcvsOneHour = value;
                this.NotifyPropertyChanged("OhlcvsOneHour");
            }
        }

        // 一分単位 今日と昨日の1minデータを取得する必要あり。
        private List<Ohlcv> _ohlcvsOneMin = new List<Ohlcv>();
        public List<Ohlcv> OhlcvsOneMin
        {
            get { return _ohlcvsOneMin; }
            set
            {
                _ohlcvsOneMin = value;
                this.NotifyPropertyChanged("OhlcvsOneMin");
            }
        }

        private SeriesCollection _chartSeries;
        public SeriesCollection ChartSeries
        {
            get
            {
                return _chartSeries;
            }
            set
            {
                if (_chartSeries == value)
                    return;

                _chartSeries = value;
                this.NotifyPropertyChanged("ChartSeries");
            }
        }

        private AxesCollection _chartAxisX = new AxesCollection();
        public AxesCollection ChartAxisX
        {
            get
            {
                return _chartAxisX;
            }
            set
            {
                if (_chartAxisX == value)
                    return;

                _chartAxisX = value;
                this.NotifyPropertyChanged("ChartAxisX");
            }
        }

        private AxesCollection _chartAxisY = new AxesCollection();
        public AxesCollection ChartAxisY
        {
            get
            {
                return _chartAxisY;
            }
            set
            {
                if (_chartAxisY == value)
                    return;

                _chartAxisY = value;
                this.NotifyPropertyChanged("ChartAxisY");
            }
        }
        */

        // === BTC === 
        private SeriesCollection _chartSeriesBtc;
        public SeriesCollection ChartSeriesBtc
        {
            get
            {
                return _chartSeriesBtc;
            }
            set
            {
                if (_chartSeriesBtc == value)
                    return;

                _chartSeriesBtc = value;
                this.NotifyPropertyChanged("ChartSeriesBtc");
            }
        }

        private AxesCollection _chartAxisXBtc;
        public AxesCollection ChartAxisXBtc
        {
            get
            {
                return _chartAxisXBtc;
            }
            set
            {
                if (_chartAxisXBtc == value)
                    return;

                _chartAxisXBtc = value;
                this.NotifyPropertyChanged("ChartAxisXBtc");
            }
        }

        private AxesCollection _chartAxisYBtc;
        public AxesCollection ChartAxisYBtc
        {
            get
            {
                return _chartAxisYBtc;
            }
            set
            {
                if (_chartAxisYBtc == value)
                    return;

                _chartAxisYBtc = value;
                this.NotifyPropertyChanged("ChartAxisYBtc");
            }
        }

        // 一時間単位 
        private List<Ohlcv> _ohlcvsOneHourBtc = new List<Ohlcv>();
        public List<Ohlcv> OhlcvsOneHourBtc
        {
            get { return _ohlcvsOneHourBtc; }
            set
            {
                _ohlcvsOneHourBtc = value;
                this.NotifyPropertyChanged("OhlcvsOneHourBtc");
            }
        }

        // 一分単位 
        private List<Ohlcv> _ohlcvsOneMinBtc = new List<Ohlcv>();
        public List<Ohlcv> OhlcvsOneMinBtc
        {
            get { return _ohlcvsOneMinBtc; }
            set
            {
                _ohlcvsOneMinBtc = value;
                this.NotifyPropertyChanged("OhlcvsOneMinBtc");
            }
        }

        // 一日単位 
        private List<Ohlcv> _ohlcvsOneDayBtc = new List<Ohlcv>();
        public List<Ohlcv> OhlcvsOneDayBtc
        {
            get { return _ohlcvsOneDayBtc; }
            set
            {
                _ohlcvsOneDayBtc = value;
                this.NotifyPropertyChanged("OhlcvsOneDayBtc");
            }
        }

        // === LTC === 
        private SeriesCollection _chartSeriesLtc;
        public SeriesCollection ChartSeriesLtc
        {
            get
            {
                return _chartSeriesLtc;
            }
            set
            {
                if (_chartSeriesLtc == value)
                    return;

                _chartSeriesLtc = value;
                this.NotifyPropertyChanged("ChartSeriesLtc");
            }
        }

        private AxesCollection _chartAxisXLtc;
        public AxesCollection ChartAxisXLtc
        {
            get
            {
                return _chartAxisXLtc;
            }
            set
            {
                if (_chartAxisXLtc == value)
                    return;

                _chartAxisXLtc = value;
                this.NotifyPropertyChanged("ChartAxisXLtc");
            }
        }

        private AxesCollection _chartAxisYLtc;
        public AxesCollection ChartAxisYLtc
        {
            get
            {
                return _chartAxisYLtc;
            }
            set
            {
                if (_chartAxisYLtc == value)
                    return;

                _chartAxisYLtc = value;
                this.NotifyPropertyChanged("ChartAxisYLtc");
            }
        }

        // 一時間単位 
        private List<Ohlcv> _ohlcvsOneHourLtc = new List<Ohlcv>();
        public List<Ohlcv> OhlcvsOneHourLtc
        {
            get { return _ohlcvsOneHourLtc; }
            set
            {
                _ohlcvsOneHourLtc = value;
                this.NotifyPropertyChanged("OhlcvsOneHourLtc");
            }
        }

        // 一分単位 
        private List<Ohlcv> _ohlcvsOneMinLtc = new List<Ohlcv>();
        public List<Ohlcv> OhlcvsOneMinLtc
        {
            get { return _ohlcvsOneMinLtc; }
            set
            {
                _ohlcvsOneMinLtc = value;
                this.NotifyPropertyChanged("OhlcvsOneMinLtc");
            }
        }

        // 一日単位 
        private List<Ohlcv> _ohlcvsOneDayLtc = new List<Ohlcv>();
        public List<Ohlcv> OhlcvsOneDayLtc
        {
            get { return _ohlcvsOneDayLtc; }
            set
            {
                _ohlcvsOneDayLtc = value;
                this.NotifyPropertyChanged("OhlcvsOneDayLtc");
            }
        }

        // === XRP === 
        private SeriesCollection _chartSeriesXrp;
        public SeriesCollection ChartSeriesXrp
        {
            get
            {
                return _chartSeriesXrp;
            }
            set
            {
                if (_chartSeriesXrp == value)
                    return;

                _chartSeriesXrp = value;
                this.NotifyPropertyChanged("ChartSeriesXrp");
            }
        }

        private AxesCollection _chartAxisXXrp;
        public AxesCollection ChartAxisXXrp
        {
            get
            {
                return _chartAxisXXrp;
            }
            set
            {
                if (_chartAxisXXrp == value)
                    return;

                _chartAxisXXrp = value;
                this.NotifyPropertyChanged("ChartAxisXXrp");
            }
        }

        private AxesCollection _chartAxisYXrp;
        public AxesCollection ChartAxisYXrp
        {
            get
            {
                return _chartAxisYXrp;
            }
            set
            {
                if (_chartAxisYXrp == value)
                    return;

                _chartAxisYXrp = value;
                this.NotifyPropertyChanged("ChartAxisYXrp");
            }
        }

        // 一時間単位 
        private List<Ohlcv> _ohlcvsOneHourXrp = new List<Ohlcv>();
        public List<Ohlcv> OhlcvsOneHourXrp
        {
            get { return _ohlcvsOneHourXrp; }
            set
            {
                _ohlcvsOneHourXrp = value;
                this.NotifyPropertyChanged("OhlcvsOneHourXrp");
            }
        }

        // 一分単位 
        private List<Ohlcv> _ohlcvsOneMinXrp = new List<Ohlcv>();
        public List<Ohlcv> OhlcvsOneMinXrp
        {
            get { return _ohlcvsOneMinXrp; }
            set
            {
                _ohlcvsOneMinXrp = value;
                this.NotifyPropertyChanged("OhlcvsOneMinXrp");
            }
        }

        // 一日単位 
        private List<Ohlcv> _ohlcvsOneDayXrp = new List<Ohlcv>();
        public List<Ohlcv> OhlcvsOneDayXrp
        {
            get { return _ohlcvsOneDayXrp; }
            set
            {
                _ohlcvsOneDayXrp = value;
                this.NotifyPropertyChanged("OhlcvsOneDayXrp");
            }
        }

        // === Eth === 
        private SeriesCollection _chartSeriesEth;
        public SeriesCollection ChartSeriesEth
        {
            get
            {
                return _chartSeriesEth;
            }
            set
            {
                if (_chartSeriesEth == value)
                    return;

                _chartSeriesEth = value;
                this.NotifyPropertyChanged("ChartSeriesEth");
            }
        }

        private AxesCollection _chartAxisXEth;
        public AxesCollection ChartAxisXEth
        {
            get
            {
                return _chartAxisXEth;
            }
            set
            {
                if (_chartAxisXEth == value)
                    return;

                _chartAxisXEth = value;
                this.NotifyPropertyChanged("ChartAxisXEth");
            }
        }

        private AxesCollection _chartAxisYEth;
        public AxesCollection ChartAxisYEth
        {
            get
            {
                return _chartAxisYEth;
            }
            set
            {
                if (_chartAxisYEth == value)
                    return;

                _chartAxisYEth = value;
                this.NotifyPropertyChanged("ChartAxisYEth");
            }
        }

        // 一時間単位 
        private List<Ohlcv> _ohlcvsOneHourEth = new List<Ohlcv>();
        public List<Ohlcv> OhlcvsOneHourEth
        {
            get { return _ohlcvsOneHourEth; }
            set
            {
                _ohlcvsOneHourEth = value;
                this.NotifyPropertyChanged("OhlcvsOneHourEth");
            }
        }

        // 一分単位 
        private List<Ohlcv> _ohlcvsOneMinEth = new List<Ohlcv>();
        public List<Ohlcv> OhlcvsOneMinEth
        {
            get { return _ohlcvsOneMinEth; }
            set
            {
                _ohlcvsOneMinEth = value;
                this.NotifyPropertyChanged("OhlcvsOneMinEth");
            }
        }

        // 一日単位 
        private List<Ohlcv> _ohlcvsOneDayEth = new List<Ohlcv>();
        public List<Ohlcv> OhlcvsOneDayEth
        {
            get { return _ohlcvsOneDayEth; }
            set
            {
                _ohlcvsOneDayEth = value;
                this.NotifyPropertyChanged("OhlcvsOneDayEth");
            }
        }

        // === Mona === 
        private SeriesCollection _chartSeriesMona;
        public SeriesCollection ChartSeriesMona
        {
            get
            {
                return _chartSeriesMona;
            }
            set
            {
                if (_chartSeriesMona == value)
                    return;

                _chartSeriesMona = value;
                this.NotifyPropertyChanged("ChartSeriesMona");
            }
        }

        private AxesCollection _chartAxisXMona;
        public AxesCollection ChartAxisXMona
        {
            get
            {
                return _chartAxisXMona;
            }
            set
            {
                if (_chartAxisXMona == value)
                    return;

                _chartAxisXMona = value;
                this.NotifyPropertyChanged("ChartAxisXMona");
            }
        }

        private AxesCollection _chartAxisYMona;
        public AxesCollection ChartAxisYMona
        {
            get
            {
                return _chartAxisYMona;
            }
            set
            {
                if (_chartAxisYMona == value)
                    return;

                _chartAxisYMona = value;
                this.NotifyPropertyChanged("ChartAxisYMona");
            }
        }

        // 一時間単位 
        private List<Ohlcv> _ohlcvsOneHourMona = new List<Ohlcv>();
        public List<Ohlcv> OhlcvsOneHourMona
        {
            get { return _ohlcvsOneHourMona; }
            set
            {
                _ohlcvsOneHourMona = value;
                this.NotifyPropertyChanged("OhlcvsOneHourMona");
            }
        }

        // 一分単位 
        private List<Ohlcv> _ohlcvsOneMinMona = new List<Ohlcv>();
        public List<Ohlcv> OhlcvsOneMinMona
        {
            get { return _ohlcvsOneMinMona; }
            set
            {
                _ohlcvsOneMinMona = value;
                this.NotifyPropertyChanged("OhlcvsOneMinMona");
            }
        }

        // 一日単位 
        private List<Ohlcv> _ohlcvsOneDayMona = new List<Ohlcv>();
        public List<Ohlcv> OhlcvsOneDayMona
        {
            get { return _ohlcvsOneDayMona; }
            set
            {
                _ohlcvsOneDayMona = value;
                this.NotifyPropertyChanged("OhlcvsOneDayMona");
            }
        }

        // === Bch === 
        private SeriesCollection _chartSeriesBch;
        public SeriesCollection ChartSeriesBch
        {
            get
            {
                return _chartSeriesBch;
            }
            set
            {
                if (_chartSeriesBch == value)
                    return;

                _chartSeriesBch = value;
                this.NotifyPropertyChanged("ChartSeriesBch");
            }
        }

        private AxesCollection _chartAxisXBch;
        public AxesCollection ChartAxisXBch
        {
            get
            {
                return _chartAxisXBch;
            }
            set
            {
                if (_chartAxisXBch == value)
                    return;

                _chartAxisXBch = value;
                this.NotifyPropertyChanged("ChartAxisXBch");
            }
        }

        private AxesCollection _chartAxisYBch;
        public AxesCollection ChartAxisYBch
        {
            get
            {
                return _chartAxisYBch;
            }
            set
            {
                if (_chartAxisYBch == value)
                    return;

                _chartAxisYBch = value;
                this.NotifyPropertyChanged("ChartAxisYBch");
            }
        }

        // 一時間単位 
        private List<Ohlcv> _ohlcvsOneHourBch = new List<Ohlcv>();
        public List<Ohlcv> OhlcvsOneHourBch
        {
            get { return _ohlcvsOneHourBch; }
            set
            {
                _ohlcvsOneHourBch = value;
                this.NotifyPropertyChanged("OhlcvsOneHourBch");
            }
        }

        // 一分単位 
        private List<Ohlcv> _ohlcvsOneMinBch = new List<Ohlcv>();
        public List<Ohlcv> OhlcvsOneMinBch
        {
            get { return _ohlcvsOneMinBch; }
            set
            {
                _ohlcvsOneMinBch = value;
                this.NotifyPropertyChanged("OhlcvsOneMinBch");
            }
        }

        // 一日単位 
        private List<Ohlcv> _ohlcvsOneDayBch = new List<Ohlcv>();
        public List<Ohlcv> OhlcvsOneDayBch
        {
            get { return _ohlcvsOneDayBch; }
            set
            {
                _ohlcvsOneDayBch = value;
                this.NotifyPropertyChanged("OhlcvsOneDayBch");
            }
        }


        #endregion

        System.Windows.Threading.DispatcherTimer dispatcherTimerTickOtherPairs = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer dispatcherChartTimer = new System.Windows.Threading.DispatcherTimer();

        /// <summary>
        /// メインのビューモデル
        /// </summary>
        public MainViewModel()
        {

            #region == チャートのイニシャライズ ==

            // 各通貨ペアをループ
            foreach (Pairs pair in Enum.GetValues(typeof(Pairs)))
            {
                // Axes
                AxesCollection chartAxisX = new AxesCollection();
                AxesCollection chartAxisY = new AxesCollection();

                // 日時 X
                Axis caX = new Axis();
                caX.Name = "AxisX";
                caX.Title = "";
                caX.MaxValue = 60;
                caX.MinValue = 0;
                caX.Labels = new List<string>();
                caX.Separator.StrokeThickness = 0.1;
                caX.Separator.StrokeDashArray = new DoubleCollection { 4 };
                caX.Separator.IsEnabled = false;
                caX.IsMerged = false;
                caX.DisableAnimations = true;

                //ChartAxisX.Add(caX);
                chartAxisX.Add(caX);

                // 価格 Y
                Axis caY = new Axis();
                caY.Name = "Price";
                caY.Title = "";
                caY.MaxValue = double.NaN;
                caY.MinValue = double.NaN;
                caY.Position = AxisPosition.RightTop;
                caY.Separator.StrokeThickness = 0.1;
                caY.Separator.StrokeDashArray = new DoubleCollection { 4 };
                caY.IsMerged = false;
                caY.Separator.Stroke = System.Windows.Media.Brushes.WhiteSmoke;
                caY.DisableAnimations = true;

                //ChartAxisY.Add(caY);
                chartAxisY.Add(caY);

                // 出来高 Y
                Axis vaY = new Axis();
                vaY.Name = "出来高";
                vaY.Title = "";
                vaY.ShowLabels = false;
                vaY.Labels = null;
                vaY.MaxValue = double.NaN;
                vaY.MinValue = double.NaN;
                vaY.Position = AxisPosition.RightTop;
                vaY.Separator.IsEnabled = false;
                vaY.Separator.StrokeThickness = 0;
                vaY.IsMerged = true;
                vaY.DisableAnimations = true;

                //ChartAxisY.Add(vaY);
                chartAxisY.Add(vaY);

                // sections

                // 現在値セクション
                AxisSection axs = new AxisSection();
                axs.Value = double.NaN;//(double)_ltp;
                axs.Width = 0;
                //axs.SectionWidth = 0;
                axs.StrokeThickness = 0.4;
                axs.StrokeDashArray = new DoubleCollection { 4 };
                axs.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(150, 172, 206));
                axs.DataLabel = false;
                //axs.DataLabelForeground = new SolidColorBrush(Colors.Black);
                axs.DisableAnimations = true;

                //ChartAxisY[0].Sections.Add(axs);
                chartAxisY[0].Sections.Add(axs);

                // 色
                SolidColorBrush yellowBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 0));
                yellowBrush.Opacity = 0.1;

                // Lines
                SeriesCollection chartSeries = new SeriesCollection()
                {

                    new CandleSeries()
                    {
                        Title = PairStrings[pair],
                        Values = new ChartValues<OhlcPoint>{},
                        Fill = Brushes.Transparent,
                        ScalesYAt = 0,
                        //IncreaseBrush = new SolidColorBrush(_priceUpColor),//System.Windows.Media.Brushes.Aqua,
                        //DecreaseBrush = new SolidColorBrush(_priceDownColor),//System.Windows.Media.Brushes.Pink,
                        //IncreaseBrush = new SolidColorBrush(_chartIncreaseColor)
                        //DecreaseBrush = new SolidColorBrush(_chartDecreaseColor),
                        
                    },

                     new ColumnSeries
                    {
                        Title = "出来高",
                        Values = new ChartValues<double> {},
                        ScalesYAt = 1,
                        Fill = yellowBrush,

                    }

                };

                if (pair == Pairs.btc_jpy)
                {
                    ChartSeriesBtc = chartSeries;
                    ChartAxisXBtc = chartAxisX;
                    ChartAxisYBtc = chartAxisY;
                }
                else if (pair == Pairs.xrp_jpy)
                {
                    ChartSeriesXrp = chartSeries;
                    ChartAxisXXrp = chartAxisX;
                    ChartAxisYXrp = chartAxisY;
                }
                else if (pair == Pairs.eth_btc)
                {
                    ChartSeriesEth = chartSeries;
                    ChartAxisXEth = chartAxisX;
                    ChartAxisYEth = chartAxisY;
                }
                else if (pair == Pairs.mona_jpy)
                {
                    ChartSeriesMona = chartSeries;
                    ChartAxisXMona = chartAxisX;
                    ChartAxisYMona = chartAxisY;
                }
                else if (pair == Pairs.mona_btc)
                {
                    //
                }
                else if (pair == Pairs.ltc_btc)
                {
                    ChartSeriesLtc = chartSeries;
                    ChartAxisXLtc = chartAxisX;
                    ChartAxisYLtc = chartAxisY;
                }
                else if (pair == Pairs.bcc_btc)
                {
                    //
                }
                else if (pair == Pairs.bcc_jpy)
                {
                    ChartSeriesBch = chartSeries;
                    ChartAxisXBch = chartAxisX;
                    ChartAxisYBch = chartAxisY;
                }

            }


            #endregion

            // Ticker（他の通貨用）のタイマー起動
            dispatcherTimerTickOtherPairs.Tick += new EventHandler(TickerTimerOtherPairs);
            dispatcherTimerTickOtherPairs.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimerTickOtherPairs.Start();
            
            
            // Chart表示のタイマー起動
            dispatcherChartTimer.Tick += new EventHandler(ChartTimer);
            dispatcherChartTimer.Interval = new TimeSpan(0, 1, 0);
            //dispatcherChartTimer.Start();
            

            
        }

        #region == イベント・タイマー系 ==

        // チャート表示 タイマー
        private void ChartTimer(object source, EventArgs e)
        {
            try
            {
                // 各通貨ペアをループ
                foreach (Pairs pair in Enum.GetValues(typeof(Pairs)))
                {
                    if ((pair == Pairs.mona_btc) || pair == Pairs.bcc_btc)
                    {
                        continue;
                    }

                    UpdateCandlestick(pair, SelectedCandleType);

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("■■■■■ ChartTimer Exception: " + ex);
            }

        }

        // 現在価格取得 Tickerタイマー
        private async void TickerTimerOtherPairs(object source, EventArgs e)
        {
            // 各通貨ペアをループ
            foreach (string pair in Enum.GetNames(typeof(Pairs)))
            {
                if ((pair == "mona_btc") || pair == "bcc_btc")
                {
                    continue;
                }

                Ticker tick = await _pubTickerApi.GetTicker(pair);

                if (tick != null)
                {
                    try
                    {
                        Ltps[GetPairs[pair]] = tick.LTP;

                        // チャートの現在値をセット
                        if (pair == "btc_jpy")
                        {
                            this.NotifyPropertyChanged("LtpBtc");
                            if (ChartAxisYBtc[0].Sections.Count > 0)
                            {
                                ChartAxisYBtc[0].Sections[0].Value = (double)tick.LTP;
                            }
                            /*
                            if (ChartSeriesBtc[0].Values != null)
                            {
                                int c = ChartSeriesBtc[0].Values.Count;

                                if (c > 0)
                                {
                                    double l = ((OhlcPoint)ChartSeriesBtc[0].Values[c - 1]).Low;
                                    double h = ((OhlcPoint)ChartSeriesBtc[0].Values[c - 1]).High;

                                    if (Application.Current == null) return;
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {

                                        ((OhlcPoint)ChartSeriesBtc[0].Values[c - 1]).Close = (double)tick.LTP;

                                        if (l > (double)tick.LTP)
                                        {
                                            ((OhlcPoint)ChartSeriesBtc[0].Values[c - 1]).Low = (double)tick.LTP;
                                        }

                                        if (h < (double)tick.LTP)
                                        {
                                            ((OhlcPoint)ChartSeriesBtc[0].Values[c - 1]).High = (double)tick.LTP;
                                        }

                                    });

                                }
                            }
                            */
                        }
                        else if (pair == "xrp_jpy")
                        {
                            this.NotifyPropertyChanged("LtpXrp");
                            if (ChartAxisYXrp[0].Sections.Count > 0)
                            {
                                ChartAxisYXrp[0].Sections[0].Value = (double)tick.LTP;
                            }
                            /*
                            if (ChartSeriesXrp[0].Values != null)
                            {
                                int c = ChartSeriesXrp[0].Values.Count;

                                if (c > 0)
                                {
                                    double l = ((OhlcPoint)ChartSeriesXrp[0].Values[c - 1]).Low;
                                    double h = ((OhlcPoint)ChartSeriesXrp[0].Values[c - 1]).High;

                                    if (Application.Current == null) return;
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {

                                        ((OhlcPoint)ChartSeriesXrp[0].Values[c - 1]).Close = (double)tick.LTP;

                                        if (l > (double)tick.LTP)
                                        {
                                            ((OhlcPoint)ChartSeriesXrp[0].Values[c - 1]).Low = (double)tick.LTP;
                                        }

                                        if (h < (double)tick.LTP)
                                        {
                                            ((OhlcPoint)ChartSeriesXrp[0].Values[c - 1]).High = (double)tick.LTP;
                                        }

                                    });

                                }
                            }
                            */
                        }
                        else if (pair == "eth_btc")
                        {
                            this.NotifyPropertyChanged("LtpEthBtc");
                            if (ChartAxisYEth[0].Sections.Count > 0)
                            {
                                ChartAxisYEth[0].Sections[0].Value = (double)tick.LTP;
                            }
                            /*
                            if (ChartSeriesEth[0].Values != null)
                            {
                                int c = ChartSeriesEth[0].Values.Count;

                                if (c > 0)
                                {
                                    double l = ((OhlcPoint)ChartSeriesEth[0].Values[c - 1]).Low;
                                    double h = ((OhlcPoint)ChartSeriesEth[0].Values[c - 1]).High;

                                    if (Application.Current == null) return;
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {

                                        ((OhlcPoint)ChartSeriesEth[0].Values[c - 1]).Close = (double)tick.LTP;

                                        if (l > (double)tick.LTP)
                                        {
                                            ((OhlcPoint)ChartSeriesEth[0].Values[c - 1]).Low = (double)tick.LTP;
                                        }

                                        if (h < (double)tick.LTP)
                                        {
                                            ((OhlcPoint)ChartSeriesEth[0].Values[c - 1]).High = (double)tick.LTP;
                                        }

                                    });

                                }
                            }
                            */
                        }
                        else if (pair == "mona_jpy")
                        {
                            this.NotifyPropertyChanged("LtpMonaJpy");
                            if (ChartAxisYMona[0].Sections.Count > 0)
                            {
                                ChartAxisYMona[0].Sections[0].Value = (double)tick.LTP;
                            }
                            /*
                            if (ChartSeriesMona[0].Values != null)
                            {
                                int c = ChartSeriesMona[0].Values.Count;

                                if (c > 0)
                                {
                                    double l = ((OhlcPoint)ChartSeriesMona[0].Values[c - 1]).Low;
                                    double h = ((OhlcPoint)ChartSeriesMona[0].Values[c - 1]).High;

                                    if (Application.Current == null) return;
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {

                                        ((OhlcPoint)ChartSeriesMona[0].Values[c - 1]).Close = (double)tick.LTP;

                                        if (l > (double)tick.LTP)
                                        {
                                            ((OhlcPoint)ChartSeriesMona[0].Values[c - 1]).Low = (double)tick.LTP;
                                        }

                                        if (h < (double)tick.LTP)
                                        {
                                            ((OhlcPoint)ChartSeriesMona[0].Values[c - 1]).High = (double)tick.LTP;
                                        }

                                    });

                                }
                            }
                            */
                        }
                        else if (pair == "mona_btc")
                        {
                            //
                        }
                        else if (pair == "ltc_btc")
                        {
                            this.NotifyPropertyChanged("LtpLtcBtc");
                            if (ChartAxisYLtc[0].Sections.Count > 0)
                            {
                                ChartAxisYLtc[0].Sections[0].Value = (double)tick.LTP;
                            }
                            /*
                            if (ChartSeriesLtc[0].Values != null)
                            {
                                int c = ChartSeriesLtc[0].Values.Count;

                                if (c > 0)
                                {
                                    double l = ((OhlcPoint)ChartSeriesLtc[0].Values[c - 1]).Low;
                                    double h = ((OhlcPoint)ChartSeriesLtc[0].Values[c - 1]).High;

                                    if (Application.Current == null) return;
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {

                                        ((OhlcPoint)ChartSeriesLtc[0].Values[c - 1]).Close = (double)tick.LTP;

                                        if (l > (double)tick.LTP)
                                        {
                                            ((OhlcPoint)ChartSeriesLtc[0].Values[c - 1]).Low = (double)tick.LTP;
                                        }

                                        if (h < (double)tick.LTP)
                                        {
                                            ((OhlcPoint)ChartSeriesLtc[0].Values[c - 1]).High = (double)tick.LTP;
                                        }

                                    });

                                }
                            }
                            */
                        }
                        else if (pair == "bcc_btc")
                        {
                            //
                        }
                        else if (pair == "bcc_jpy")
                        {
                            this.NotifyPropertyChanged("LtpBchJpy");
                            if (ChartAxisYBch[0].Sections.Count > 0)
                            {
                                ChartAxisYBch[0].Sections[0].Value = (double)tick.LTP;
                            }
                            /*
                            if (ChartSeriesBch[0].Values != null)
                            {
                                int c = ChartSeriesBch[0].Values.Count;

                                if (c > 0)
                                {
                                    double l = ((OhlcPoint)ChartSeriesBch[0].Values[c - 1]).Low;
                                    double h = ((OhlcPoint)ChartSeriesBch[0].Values[c - 1]).High;

                                    if (Application.Current == null) return;
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {

                                        ((OhlcPoint)ChartSeriesBch[0].Values[c - 1]).Close = (double)tick.LTP;

                                        if (l > (double)tick.LTP)
                                        {
                                            ((OhlcPoint)ChartSeriesBch[0].Values[c - 1]).Low = (double)tick.LTP;
                                        }

                                        if (h < (double)tick.LTP)
                                        {
                                            ((OhlcPoint)ChartSeriesBch[0].Values[c - 1]).High = (double)tick.LTP;
                                        }

                                    });

                                }
                            }
                            */
                        }

                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("■■■■■ TickerTimerOtherPairs: Exception1 - " + ex.Message);
                        break;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("■■■■■ TickerTimerOtherPairs: GetTicker returned null");
                    break;
                }
            }
        }

        // 起動時の処理
        public void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // データ保存フォルダの取得
            var AppDataFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            AppDataFolder = AppDataFolder + System.IO.Path.DirectorySeparatorChar + _appDeveloper + System.IO.Path.DirectorySeparatorChar + _appName;
            // 存在していなかったら作成
            System.IO.Directory.CreateDirectory(AppDataFolder);

            #region == アプリ設定のロード  ==

            // 設定ファイルのパス
            var AppConfigFilePath = AppDataFolder + System.IO.Path.DirectorySeparatorChar + _appName + ".config";

            // アプリ設定情報の読み込み
            if (File.Exists(AppConfigFilePath))
            {
                XDocument xdoc = XDocument.Load(AppConfigFilePath);

                #region == ウィンドウ関連 ==

                if (sender is Window)
                {
                    // Main Window element
                    var mainWindow = xdoc.Root.Element("MainWindow");
                    if (mainWindow != null)
                    {
                        var hoge = mainWindow.Attribute("top");
                        if (hoge != null)
                        {
                            (sender as Window).Top = double.Parse(hoge.Value);
                        }

                        hoge = mainWindow.Attribute("left");
                        if (hoge != null)
                        {
                            (sender as Window).Left = double.Parse(hoge.Value);
                        }

                        hoge = mainWindow.Attribute("height");
                        if (hoge != null)
                        {
                            (sender as Window).Height = double.Parse(hoge.Value);
                        }

                        hoge = mainWindow.Attribute("width");
                        if (hoge != null)
                        {
                            (sender as Window).Width = double.Parse(hoge.Value);
                        }

                        hoge = mainWindow.Attribute("state");
                        if (hoge != null)
                        {
                            if (hoge.Value == "Maximized")
                            {
                                (sender as Window).WindowState = WindowState.Maximized;
                            }
                            else if (hoge.Value == "Normal")
                            {
                                (sender as Window).WindowState = WindowState.Normal;
                            }
                            else if (hoge.Value == "Minimized")
                            {
                                (sender as Window).WindowState = WindowState.Normal;
                            }
                        }

                    }

                }

                #endregion

                #region == チャート関連 ==

                var chartSetting = xdoc.Root.Element("Chart");
                if (chartSetting != null)
                {
                    var hoge = chartSetting.Attribute("candleType");
                    if (hoge != null)
                    {
                        if (hoge.Value == CandleTypes.OneMin.ToString())
                        {
                            SelectedCandleType = CandleTypes.OneMin;
                        }
                        else if (hoge.Value == CandleTypes.OneHour.ToString())
                        {
                            SelectedCandleType = CandleTypes.OneHour;
                        }
                        else if (hoge.Value == CandleTypes.OneDay.ToString())
                        {
                            SelectedCandleType = CandleTypes.OneDay;
                        }
                        else
                        {
                            // TODO other candle types

                            SelectedCandleType = CandleTypes.OneHour;
                        }
                    }

                }
                else
                {
                    // デフォのチャート、キャンドルタイプ指定
                    SelectedCandleType = CandleTypes.OneHour;
                }

                #endregion
            }
            else
            {
                // デフォのチャート、キャンドルタイプ指定
                SelectedCandleType = CandleTypes.OneHour;
            }

            #endregion

            // チャート更新のタイマー起動
            dispatcherChartTimer.Start();

            /*
             * 
             *  SelectedCandleType = で表示できるので、これは不要。
             * 
            // チャートの表示
            Task.Run(async () =>
            {
                await Task.Run(() => DisplayCharts());

            });
            */

        }

        // 終了時の処理
        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // データ保存フォルダの取得
            var AppDataFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            AppDataFolder = AppDataFolder + System.IO.Path.DirectorySeparatorChar + _appDeveloper + System.IO.Path.DirectorySeparatorChar + _appName;
            // 存在していなかったら作成
            System.IO.Directory.CreateDirectory(AppDataFolder);

            #region == アプリ設定の保存 ==

            // 設定ファイルのパス
            var AppConfigFilePath = AppDataFolder + System.IO.Path.DirectorySeparatorChar + _appName + ".config";

            // 設定ファイル用のXMLオブジェクト
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.InsertBefore(xmlDeclaration, doc.DocumentElement);

            // Root Document Element
            XmlElement root = doc.CreateElement(string.Empty, "App", string.Empty);
            doc.AppendChild(root);

            XmlAttribute attrs = doc.CreateAttribute("Version");
            attrs.Value = _appVer;
            root.SetAttributeNode(attrs);

            #region == ウィンドウ関連 ==

            if (sender is Window)
            {
                // Main Window element
                XmlElement mainWindow = doc.CreateElement(string.Empty, "MainWindow", string.Empty);

                // Main Window attributes
                attrs = doc.CreateAttribute("height");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Height.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Height.ToString();
                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("width");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Width.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Width.ToString();

                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("top");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Top.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Top.ToString();
                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("left");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Left.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Left.ToString();
                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("state");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = "Maximized";
                }
                else if ((sender as Window).WindowState == WindowState.Normal)
                {
                    attrs.Value = "Normal";

                }
                else if ((sender as Window).WindowState == WindowState.Minimized)
                {
                    attrs.Value = "Minimized";
                }
                mainWindow.SetAttributeNode(attrs);

                // set Main Window element to root.
                root.AppendChild(mainWindow);

            }

            #endregion

            #region == チャート関連 ==

            XmlElement chartSetting = doc.CreateElement(string.Empty, "Chart", string.Empty);

            attrs = doc.CreateAttribute("candleType");

            if (SelectedCandleType == CandleTypes.OneMin)
            {
                attrs.Value = CandleTypes.OneMin.ToString();
            }
            else if (SelectedCandleType == CandleTypes.OneHour)
            {
                attrs.Value = CandleTypes.OneHour.ToString();
            }
            else if (SelectedCandleType == CandleTypes.OneDay)
            {
                attrs.Value = CandleTypes.OneDay.ToString();
            }
            else
            {
                // TODO
                attrs.Value = "";
            }

            chartSetting.SetAttributeNode(attrs);

            root.AppendChild(chartSetting);

            #endregion


            // 設定ファイルの保存
            doc.Save(AppConfigFilePath);

            #endregion

        }


        #endregion

        #region == メソッド ==

        #region == チャート関係のメソッド ==

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

                    System.Diagnostics.Debug.WriteLine("■■■■■ GetCandlestick: GetCandlestick returned error");
                }
            }
            else
            {

                System.Diagnostics.Debug.WriteLine("■■■■■ GetCandlestick: GetCandlestick returned null");
            }

            return null;
        }

        // 初回に各種Candlestickをまとめて取得
        private async Task<bool> GetCandlesticks(Pairs pair, CandleTypes ct)
        {
            //ChartLoadingInfo = "チャートデータを取得中....";

            // 今日の日付セット。UTCで。
            DateTime dtToday = DateTime.Now.ToUniversalTime();

            // データは、ローカルタイムで、朝9:00 から翌8:59分まで。8:59分までしか取れないので、 9:00過ぎていたら 最新のデータとるには日付を１日追加する

            #region == OhlcvsOneHour 1hour毎のデータ ==

            List<Ohlcv> ListOhlcvsOneHour = new List<Ohlcv>();

            if (ct == CandleTypes.OneHour)
            {
                // TODO 取得中フラグセット。

                Debug.WriteLine("今日の1hour取得開始 " + pair.ToString());

                // 一時間のロウソク足タイプなら今日、昨日、一昨日、その前の１週間分の1hourデータを取得する必要あり。
                ListOhlcvsOneHour = await GetCandlestick(pair, CandleTypes.OneHour, dtToday);
                if (ListOhlcvsOneHour != null)
                {
                    // 逆順にする
                    ListOhlcvsOneHour.Reverse();

                    Debug.WriteLine("昨日の1hour取得開始 " + pair.ToString());
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

                        Debug.WriteLine("一昨日の1hour取得開始 " + pair.ToString());
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

                            Debug.WriteLine("３日前の1hour取得開始 " + pair.ToString());
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
                                /*
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

                                }
                                */
                            }

                        }


                    }
                }

                // TODO 取得中フラグ解除。

            }

            #endregion

            await Task.Delay(200);

            #region == OhlcvsOneMin 1min毎のデータ ==

            List<Ohlcv> ListOhlcvsOneMin = new List<Ohlcv>();

            if (ct == CandleTypes.OneMin)
            {
                // TODO 取得中フラグセット。

                Debug.WriteLine("今日の1min取得開始 " + pair.ToString());

                // 一分毎のロウソク足タイプなら今日と昨日の1minデータを取得する必要あり。
                ListOhlcvsOneMin = await GetCandlestick(pair, CandleTypes.OneMin, dtToday);
                if (ListOhlcvsOneMin != null)
                {
                    // 逆順にする
                    ListOhlcvsOneMin.Reverse();

                    await Task.Delay(200);

                    // 00:00:00から23:59:59分までしか取れないので、 3時間分取るには、00:00:00から3:00までは 最新のデータとるには日付を１日マイナスする
                    if (dtToday.Hour <= 3) // < 3
                    {
                        Debug.WriteLine("昨日の1min取得開始");
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
                }

                // TODO 取得中フラグ解除。
            }

            #endregion

            await Task.Delay(200);

            #region == OhlcvsOneDay 1day毎のデータ ==

            List<Ohlcv> ListOhlcvsOneDay = new List<Ohlcv>();

            if (ct == CandleTypes.OneDay)
            {
                // TODO 取得中フラグセット。

                // 1日のロウソク足タイプなら今年、去年、２年前、３年前、４年前、５年前の1hourデータを取得する必要あり。(５年前は止めた)

                Debug.WriteLine("今年のOneDay取得開始 " + pair.ToString());

                ListOhlcvsOneDay = await GetCandlestick(pair, CandleTypes.OneDay, dtToday);
                if (ListOhlcvsOneDay != null)
                {
                    // 逆順にする
                    ListOhlcvsOneDay.Reverse();

                    if (dtToday.Month < 3)
                    {
                        Debug.WriteLine("去年のOneDay取得開始 " + pair.ToString());

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

                // TODO 取得中フラグ解除。
            }

            #endregion


            ChartLoadingInfo = "";

            if (pair == Pairs.btc_jpy)
            {
                if (ListOhlcvsOneHour != null)
                    OhlcvsOneHourBtc = ListOhlcvsOneHour;
                if (ListOhlcvsOneMin != null)
                    OhlcvsOneMinBtc = ListOhlcvsOneMin;
                if (ListOhlcvsOneDay != null)
                    OhlcvsOneDayBtc = ListOhlcvsOneDay;
            }
            else if (pair == Pairs.xrp_jpy)
            {
                if (ListOhlcvsOneHour != null)
                    OhlcvsOneHourXrp = ListOhlcvsOneHour;
                if (ListOhlcvsOneMin != null)
                    OhlcvsOneMinXrp = ListOhlcvsOneMin;
                if (ListOhlcvsOneDay != null)
                    OhlcvsOneDayXrp = ListOhlcvsOneDay;
            }
            else if (pair == Pairs.eth_btc)
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
            else if (pair == Pairs.mona_btc)
            {
                //
            }
            else if (pair == Pairs.ltc_btc)
            {
                if (ListOhlcvsOneHour != null)
                    OhlcvsOneHourLtc = ListOhlcvsOneHour;
                if (ListOhlcvsOneMin != null)
                    OhlcvsOneMinLtc = ListOhlcvsOneMin;
                if (ListOhlcvsOneDay != null)
                    OhlcvsOneDayLtc = ListOhlcvsOneDay;
            }
            else if (pair == Pairs.bcc_btc)
            {
                //
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

            return true;

        }

        // チャートを読み込み表示する。
        private void LoadChart(Pairs pair, CandleTypes ct)
        {
            // TODO 個別にロードして、初期表示を早くする。

            //ChartLoadingInfo = "チャートをロード中....";
            //Debug.WriteLine("LoadChart... " + pair.ToString());

            //CandleTypes ct = SelectedCandleType;

            List<Ohlcv> lst = null;
            int span = 0;

            if (ct == CandleTypes.OneMin)
            {
                // 一分毎のロウソク足タイプなら
                //lst = OhlcvsOneMin;
                if (pair == Pairs.btc_jpy)
                {
                    lst = OhlcvsOneMinBtc;
                }
                else if (pair == Pairs.xrp_jpy)
                {
                    lst = OhlcvsOneMinXrp;
                }
                else if (pair == Pairs.eth_btc)
                {
                    lst = OhlcvsOneMinEth;
                }
                else if (pair == Pairs.mona_jpy)
                {
                    lst = OhlcvsOneMinMona;
                }
                else if (pair == Pairs.mona_btc)
                {
                    //
                }
                else if (pair == Pairs.ltc_btc)
                {
                    lst = OhlcvsOneMinLtc;
                }
                else if (pair == Pairs.bcc_btc)
                {
                    //
                }
                else if (pair == Pairs.bcc_jpy)
                {
                    lst = OhlcvsOneMinBch;
                }

                // 一時間の期間か１日の期間
                if (_chartSpan == ChartSpans.OneHour)
                {
                    span = 60;
                }
                else if (_chartSpan == ChartSpans.ThreeHour)
                {
                    span = 60 * 3;
                }
                else
                {
                    throw new System.InvalidOperationException("一分毎のロウソク足タイプなら、負荷掛かり過ぎなので、１日以上は無し");
                }

                // 負荷掛かり過ぎなので、１日は無し
                /*
                else if (_chartSpan == ChartSpans.OneDay)
                {
                    span = 60*24;
                }
                */

                //Debug.WriteLine("OneMin数" + test.Count.ToString());
            }
            else if (ct == CandleTypes.OneHour)
            {
                // 一時間のロウソク足タイプなら
                //lst = OhlcvsOneHour;
                if (pair == Pairs.btc_jpy)
                {
                    lst = OhlcvsOneHourBtc;
                }
                else if (pair == Pairs.xrp_jpy)
                {
                    lst = OhlcvsOneHourXrp;
                }
                else if (pair == Pairs.eth_btc)
                {
                    lst = OhlcvsOneHourEth;
                }
                else if (pair == Pairs.mona_jpy)
                {
                    lst = OhlcvsOneHourMona;
                }
                else if (pair == Pairs.mona_btc)
                {
                    //
                }
                else if (pair == Pairs.ltc_btc)
                {
                    lst = OhlcvsOneHourLtc;
                }
                else if (pair == Pairs.bcc_btc)
                {
                    //
                }
                else if (pair == Pairs.bcc_jpy)
                {
                    lst = OhlcvsOneHourBch;
                }

                // １日の期間か3日か１週間の期間
                if (_chartSpan == ChartSpans.OneDay)
                {
                    span = 24;
                }
                else if (_chartSpan == ChartSpans.ThreeDay)
                {
                    span = 24 * 3;
                }
                else if (_chartSpan == ChartSpans.OneWeek)
                {
                    span = 24 * 7;
                }
                else
                {
                    throw new System.InvalidOperationException("時間毎のロウソク足タイプなら、負荷掛かり過ぎなので、1週間以上は無し。一日未満もなし");
                }

                // Debug.WriteLine("OneHour数" + test.Count.ToString());
            }
            else if (ct == CandleTypes.OneDay)
            {
                // 1日のロウソク足タイプなら
                //lst = OhlcvsOneDay;
                if (pair == Pairs.btc_jpy)
                {
                    lst = OhlcvsOneDayBtc;
                }
                else if (pair == Pairs.xrp_jpy)
                {
                    lst = OhlcvsOneDayXrp;
                }
                else if (pair == Pairs.eth_btc)
                {
                    lst = OhlcvsOneDayEth;
                }
                else if (pair == Pairs.mona_jpy)
                {
                    lst = OhlcvsOneDayMona;
                }
                else if (pair == Pairs.mona_btc)
                {
                    //
                }
                else if (pair == Pairs.ltc_btc)
                {
                    lst = OhlcvsOneDayLtc;
                }
                else if (pair == Pairs.bcc_btc)
                {
                    //
                }
                else if (pair == Pairs.bcc_jpy)
                {
                    lst = OhlcvsOneDayBch;
                }

                // 1ヵ月、2ヵ月、１年、５年の期間
                if (_chartSpan == ChartSpans.OneMonth)
                {
                    span = 30;//.44
                }
                else if (_chartSpan == ChartSpans.TwoMonth)
                {
                    span = 30 * 2;
                }
                else if (_chartSpan == ChartSpans.OneYear)
                {
                    span = 365;//.2425
                }
                else if (_chartSpan == ChartSpans.FiveYear)
                {
                    span = 365 * 5;
                }
                else
                {
                    throw new System.InvalidOperationException("1日のロウソク足タイプなら、一月以上");
                }

                //Debug.WriteLine("OneDay数" + test.Count.ToString());
            }
            else
            {
                throw new System.InvalidOperationException("Not impl...");
                //return;
            }

            //Debug.WriteLine("スパン：" + span.ToString());

            if (span == 0)
            {
                Debug.WriteLine("スパン 0");
                return;
            }

            if (lst == null)
            {
                Debug.WriteLine("リスト Null " + pair.ToString());
                return;
            }

            if (lst.Count < span - 1)
            {
                Debug.WriteLine("ロード中？ " + pair.ToString() + " " + lst.Count.ToString() + " " + span.ToString());
                return;
            }

            try
            {

                SeriesCollection chartSeries = null;
                AxesCollection chartAxisX = null;
                AxesCollection chartAxisY = null;

                if (pair == Pairs.btc_jpy)
                {
                    chartSeries = ChartSeriesBtc;
                    chartAxisX = ChartAxisXBtc;
                    chartAxisY = ChartAxisYBtc;
                }
                else if (pair == Pairs.xrp_jpy)
                {
                    chartSeries = ChartSeriesXrp;
                    chartAxisX = ChartAxisXXrp;
                    chartAxisY = ChartAxisYXrp;

                }
                else if (pair == Pairs.eth_btc)
                {
                    chartSeries = ChartSeriesEth;
                    chartAxisX = ChartAxisXEth;
                    chartAxisY = ChartAxisYEth;
                }
                else if (pair == Pairs.mona_jpy)
                {
                    chartSeries = ChartSeriesMona;
                    chartAxisX = ChartAxisXMona;
                    chartAxisY = ChartAxisYMona;
                }
                else if (pair == Pairs.mona_btc)
                {
                    //
                }
                else if (pair == Pairs.ltc_btc)
                {
                    chartSeries = ChartSeriesLtc;
                    chartAxisX = ChartAxisXLtc;
                    chartAxisY = ChartAxisYLtc;
                }
                else if (pair == Pairs.bcc_btc)
                {
                    //
                }
                else if (pair == Pairs.bcc_jpy)
                {
                    chartSeries = ChartSeriesBch;
                    chartAxisX = ChartAxisXBch;
                    chartAxisY = ChartAxisYBch;
                }


                if (Application.Current == null) return;
                Application.Current.Dispatcher.Invoke(() =>
                {

                    // チャート OHLCVのロード
                    if (lst.Count > 0)
                    {
                        // Candlestickクリア
                        chartSeries[0].Values.Clear();

                        // 出来高クリア
                        //ChartSeries[1].Values.Clear();
                        // https://github.com/Live-Charts/Live-Charts/issues/76
                        for (int v = 0; v < chartSeries[1].Values.Count - 1; v++)
                        {
                            chartSeries[1].Values[v] = (double)0;
                        }

                        // ラベル表示クリア
                        chartAxisX[0].Labels.Clear();

                        // 期間設定
                        chartAxisX[0].MaxValue = span - 1;
                        chartAxisX[0].MinValue = 0;

                        // Temp を作って、後でまとめて追加する。
                        // https://lvcharts.net/App/examples/v1/wpf/Performance%20Tips

                        //var temporalCv = new OhlcPoint[test.Count];
                        var temporalCv = new OhlcPoint[span - 1];
                        //var temporalOV = new ObservableValue[span - 1];

                        var tempVol = new double[span - 1];

                        // チャート最低値、最高値の設定
                        double HighMax = 0;
                        double LowMax = 999999999;

                        int i = 0;
                        int c = span;
                        foreach (var oh in lst)
                        {
                            // 全てのポイントが同じ場合、スキップする。変なデータ？ 本家もスキップしている。
                            if ((oh.Open == oh.High) && (oh.Open == oh.Low) && (oh.Open == oh.Close) && (oh.Volume == 0))
                            {
                                continue;
                            }

                            // 表示数を限る 直近のspan本
                            //if (i < (span - 1))
                            if (i < (span - 1))
                            {
                                // 最高値と最低値を探る
                                if ((double)oh.High > HighMax)
                                {
                                    HighMax = (double)oh.High;
                                }

                                if ((double)oh.Low < LowMax)
                                {
                                    LowMax = (double)oh.Low;
                                }

                                //Debug.WriteLine(oh.TimeStamp.ToString("dd日 hh時mm分"));

                                // ラベル
                                if (ct == CandleTypes.OneMin)
                                {
                                    chartAxisX[0].Labels.Insert(0, oh.TimeStamp.ToString("H:mm"));
                                }
                                else if (ct == CandleTypes.OneHour)
                                {
                                    chartAxisX[0].Labels.Insert(0, oh.TimeStamp.ToString("d日 H:mm"));

                                }
                                else if (ct == CandleTypes.OneDay)
                                {
                                    chartAxisX[0].Labels.Insert(0, oh.TimeStamp.ToString("M月d日"));
                                }
                                else
                                {
                                    throw new System.InvalidOperationException("LoadChart: 不正な CandleType");
                                }
                                //ChartAxisX[0].Labels.Add(oh.TimeStamp.ToShortTimeString());


                                // ポイント作成
                                OhlcPoint p = new OhlcPoint((double)oh.Open, (double)oh.High, (double)oh.Low, (double)oh.Close);


                                // 直接追加しないで、
                                //ChartSeries[0].Values.Add(p);
                                // 一旦、Tempに追加して、あとでまとめてAddRange
                                temporalCv[c - 2] = p;


                                tempVol[c - 2] = (double)oh.Volume;
                                //ChartSeries[3].Values.Add((double)oh.Volume);

                                c = c - 1;

                            }

                            i = i + 1;
                        }

                        // チャート最低値、最高値のセット
                        chartAxisY[0].MaxValue = HighMax;
                        chartAxisY[0].MinValue = LowMax;

                        // まとめて追加

                        // OHLCV
                        chartSeries[0].Values.AddRange(temporalCv);

                        // volume
                        var cv = new ChartValues<double>();
                        cv.AddRange(tempVol);
                        chartSeries[1].Values = cv;


                        // TODO what is this? not working.
                        if (chartAxisY[0].Sections.Count > 0)
                        {
                            //ChartAxisY[0].Sections[0].Width = span;
                            //ChartAxisY[0].Sections[0].SectionWidth = span;
                        }


                    }

                });

            }
            catch (Exception ex)
            {
                ChartLoadingInfo = "チャートのロード中にエラーが発生しました";

                Debug.WriteLine("■■■■■ Chart loading error: " + ex.ToString());
            }

            ChartLoadingInfo = "";

        }

        // 初回、データロードを確認して、チャートをロードする。
        private async void DisplayChart(Pairs pair)
        {
            bool bln = await GetCandlesticks(pair, SelectedCandleType);

            if (bln == true)
            {
                LoadChart(pair, SelectedCandleType);
            }
        }

        private async void DisplayCharts()
        {
            //foreach (DayOfWeek value in Enum.GetValues(typeof(DayOfWeek)))
            foreach (Pairs p in Enum.GetValues(typeof(Pairs)))
            {
                //Debug.WriteLine(p.ToString());

                if ((p == Pairs.mona_btc) || p == Pairs.bcc_btc)
                {
                    //Debug.WriteLine(p.ToString() + " skipping.");
                    continue;
                }
                else
                {
                    bool bln = await GetCandlesticks(p, SelectedCandleType);

                    if (bln == true)
                    {
                        LoadChart(p, SelectedCandleType);
                    }

                }

            }

        }

        // チャート表示期間を変えた時にチャートを読み込み直す。
        private void ChangeChartSpan(Pairs pair)
        {
            // enum の期間選択からチャートを更新させる。コンボボックスとダブルアップデートにならないようにするため。

            if (_chartSpan == ChartSpans.OneHour)
            {
                if (SelectedCandleType != CandleTypes.OneMin)
                {
                    SelectedCandleType = CandleTypes.OneMin;
                }
                else
                {
                    LoadChart(pair, SelectedCandleType);
                }
            }
            else if (_chartSpan == ChartSpans.ThreeHour)
            {
                if (SelectedCandleType != CandleTypes.OneMin)
                {
                    SelectedCandleType = CandleTypes.OneMin;
                }
                else
                {
                    LoadChart(pair, SelectedCandleType);
                }
            }
            else if (_chartSpan == ChartSpans.OneDay)
            {
                if (SelectedCandleType != CandleTypes.OneHour)
                {
                    SelectedCandleType = CandleTypes.OneHour;
                }
                else
                {
                    LoadChart(pair, SelectedCandleType);
                }
            }
            else if (_chartSpan == ChartSpans.ThreeDay)
            {
                if (SelectedCandleType != CandleTypes.OneHour)
                {
                    SelectedCandleType = CandleTypes.OneHour;
                }
                else
                {
                    LoadChart(pair, SelectedCandleType);
                }
            }
            else if (_chartSpan == ChartSpans.OneWeek)
            {
                if (SelectedCandleType != CandleTypes.OneHour)
                {
                    SelectedCandleType = CandleTypes.OneHour;
                }
                else
                {
                    LoadChart(pair, SelectedCandleType);
                }
            }
            else if (_chartSpan == ChartSpans.OneMonth)
            {
                if (SelectedCandleType != CandleTypes.OneDay)
                {
                    SelectedCandleType = CandleTypes.OneDay;
                }
                else
                {
                    LoadChart(pair, SelectedCandleType);
                }
            }
            else if (_chartSpan == ChartSpans.TwoMonth)
            {
                if (SelectedCandleType != CandleTypes.OneDay)
                {
                    SelectedCandleType = CandleTypes.OneDay;
                }
                else
                {
                    LoadChart(pair, SelectedCandleType);
                }
            }
            else if (_chartSpan == ChartSpans.OneYear)
            {
                if (SelectedCandleType != CandleTypes.OneDay)
                {
                    SelectedCandleType = CandleTypes.OneDay;
                }
                else
                {
                    LoadChart(pair, SelectedCandleType);
                }
            }
            else if (_chartSpan == ChartSpans.FiveYear)
            {
                if (SelectedCandleType != CandleTypes.OneDay)
                {
                    SelectedCandleType = CandleTypes.OneDay;
                }
                else
                {
                    LoadChart(pair, SelectedCandleType);
                }
            }

        }

        // タイマーで、最新のロウソク足データを取得して追加する。
        private async void UpdateCandlestick(Pairs pair, CandleTypes ct)
        {
            //ChartLoadingInfo = "チャートデータの更新中....";
            await Task.Delay(600);

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
            else if (pair == Pairs.xrp_jpy)
            {
                ListOhlcvsOneHour = OhlcvsOneHourXrp;
                ListOhlcvsOneMin = OhlcvsOneMinXrp;
                ListOhlcvsOneDay = OhlcvsOneDayXrp;
            }
            else if (pair == Pairs.eth_btc)
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
            else if (pair == Pairs.mona_btc)
            {
                //
            }
            else if (pair == Pairs.ltc_btc)
            {
                ListOhlcvsOneHour = OhlcvsOneHourLtc;
                ListOhlcvsOneMin = OhlcvsOneMinLtc;
                ListOhlcvsOneDay = OhlcvsOneDayLtc;
            }
            else if (pair == Pairs.bcc_btc)
            {
                //
            }
            else if (pair == Pairs.bcc_jpy)
            {
                ListOhlcvsOneHour = OhlcvsOneHourBch;
                ListOhlcvsOneMin = OhlcvsOneMinBch;
                ListOhlcvsOneDay = OhlcvsOneDayBch;
            }

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

                                // 全てのポイントが同じ場合、スキップする。変なデータ？ 本家もスキップしている。
                                if ((hoge.Open == hoge.High) && (hoge.Open == hoge.Low) && (hoge.Open == hoge.Close) && (hoge.Volume == 0))
                                {
                                    continue;
                                }

                                //Debug.WriteLine(hoge.TimeStamp.ToString()+" : "+ dtLastUpdate.ToString());

                                if (hoge.TimeStamp >= dtLastUpdate)
                                {

                                    if (hoge.TimeStamp == dtLastUpdate)
                                    {
                                        // 更新前の最後のポイントを更新する。最終データは中途半端なので。

                                        //Debug.WriteLine("１分毎のチャートデータ更新: " + hoge.TimeStamp.ToString());

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

                                        //Debug.WriteLine("１分毎のチャートデータ追加: " + hoge.TimeStamp.ToString());

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

            if (ct == CandleTypes.OneMin)
            {
                if (ListOhlcvsOneHour.Count > 0)
                {
                    dtLastUpdate = ListOhlcvsOneHour[0].TimeStamp;

                    TimeSpan ts = dtLastUpdate - dtToday;

                    if (ts.TotalHours >= 1)
                    {
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
                                        Debug.WriteLine("■ UpdateCandlestick 全てのポイントが同じのためスキップ " + pair.ToString());
                                        continue;
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


                    }

                }

            }

            #endregion


            #region == １日毎のデータ ==

            if (ct == CandleTypes.OneDay)
            {
                if (ListOhlcvsOneDay.Count > 0)
                {
                    dtLastUpdate = ListOhlcvsOneDay[0].TimeStamp;

                    TimeSpan ts = dtLastUpdate - dtToday;

                    if (ts.TotalDays >= 1)
                    {
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
                                        continue;
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


                    }

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

            SeriesCollection chartSeries = null;
            AxesCollection chartAxisX = null;
            AxesCollection chartAxisY = null;

            if (pair == Pairs.btc_jpy)
            {
                chartSeries = ChartSeriesBtc;
                chartAxisX = ChartAxisXBtc;
                chartAxisY = ChartAxisYBtc;
            }
            else if (pair == Pairs.xrp_jpy)
            {
                chartSeries = ChartSeriesXrp;
                chartAxisX = ChartAxisXXrp;
                chartAxisY = ChartAxisYXrp;
            }
            else if (pair == Pairs.eth_btc)
            {
                chartSeries = ChartSeriesEth;
                chartAxisX = ChartAxisXEth;
                chartAxisY = ChartAxisYEth;
            }
            else if (pair == Pairs.mona_jpy)
            {
                chartSeries = ChartSeriesMona;
                chartAxisX = ChartAxisXMona;
                chartAxisY = ChartAxisYMona;
            }
            else if (pair == Pairs.mona_btc)
            {
                //
            }
            else if (pair == Pairs.ltc_btc)
            {
                chartSeries = ChartSeriesLtc;
                chartAxisX = ChartAxisXLtc;
                chartAxisY = ChartAxisYLtc;
            }
            else if (pair == Pairs.bcc_btc)
            {
                //
            }
            else if (pair == Pairs.bcc_jpy)
            {
                chartSeries = ChartSeriesBch;
                chartAxisX = ChartAxisXBch;
                chartAxisY = ChartAxisYBch;
            }

            if (chartSeries[0].Values != null)
            {
                if (chartSeries[0].Values.Count > 0)
                {
                    if (Application.Current == null) return;
                    Application.Current.Dispatcher.Invoke(() =>
                    {

                        // ポイント作成
                        OhlcPoint p = new OhlcPoint((double)newData.Open, (double)newData.High, (double)newData.Low, (double)newData.Close);
                        // 追加
                        chartSeries[0].Values.Add(p);
                        // 一番古いの削除
                        chartSeries[0].Values.RemoveAt(0);

                        // 出来高
                        chartSeries[1].Values.Add((double)newData.Volume);
                        chartSeries[1].Values.RemoveAt(0);

                        // ラベル
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
                        chartAxisX[0].Labels.RemoveAt(0);

                        // チャート最低値、最高値のセット
                        if (chartAxisY[0].MaxValue < (double)newData.High)
                        {
                            chartAxisY[0].MaxValue = (double)newData.High + 1000;
                        }

                        if (chartAxisY[0].MinValue > (double)newData.Low)
                        {
                            chartAxisY[0].MinValue = (double)newData.Low - 1000;
                        }


                    });

                }
            }

        }

        // チャートの最後のポイントを最新情報に更新表示する。
        private void UpdateLastCandle(Pairs pair, CandleTypes ct, Ohlcv newData)
        {
            // 表示されているのだけ更新。それ以外は不要。
            //if (SelectedCandleType != ct) return;

            //Debug.WriteLine("チャートの更新 追加: "+ newData.TimeStamp.ToString());

            SeriesCollection chartSeries = null;
            AxesCollection chartAxisX = null;
            AxesCollection chartAxisY = null;

            if (pair == Pairs.btc_jpy)
            {
                chartSeries = ChartSeriesBtc;
                chartAxisX = ChartAxisXBtc;
                chartAxisY = ChartAxisYBtc;
            }
            else if (pair == Pairs.xrp_jpy)
            {
                chartSeries = ChartSeriesXrp;
                chartAxisX = ChartAxisXXrp;
                chartAxisY = ChartAxisYXrp;
            }
            else if (pair == Pairs.eth_btc)
            {
                chartSeries = ChartSeriesEth;
                chartAxisX = ChartAxisXEth;
                chartAxisY = ChartAxisYEth;
            }
            else if (pair == Pairs.mona_jpy)
            {
                chartSeries = ChartSeriesMona;
                chartAxisX = ChartAxisXMona;
                chartAxisY = ChartAxisYMona;
            }
            else if (pair == Pairs.mona_btc)
            {
                //
            }
            else if (pair == Pairs.ltc_btc)
            {
                chartSeries = ChartSeriesLtc;
                chartAxisX = ChartAxisXLtc;
                chartAxisY = ChartAxisYLtc;
            }
            else if (pair == Pairs.bcc_btc)
            {
                //
            }
            else if (pair == Pairs.bcc_jpy)
            {
                chartSeries = ChartSeriesBch;
                chartAxisX = ChartAxisXBch;
                chartAxisY = ChartAxisYBch;
            }

            if (chartSeries[0].Values != null)
            {
                if (chartSeries[0].Values.Count > 0)
                {

                    if (Application.Current == null) return;
                    Application.Current.Dispatcher.Invoke(() =>
                    {

                        ((OhlcPoint)chartSeries[0].Values[chartSeries[0].Values.Count - 1]).Open = (double)newData.Open;
                        ((OhlcPoint)chartSeries[0].Values[chartSeries[0].Values.Count - 1]).High = (double)newData.High;
                        ((OhlcPoint)chartSeries[0].Values[chartSeries[0].Values.Count - 1]).Low = (double)newData.Low;
                        ((OhlcPoint)chartSeries[0].Values[chartSeries[0].Values.Count - 1]).Close = (double)newData.Close;

                    });

                }
            }

        }

        #endregion

        #endregion

    }
}
