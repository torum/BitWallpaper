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
using BitWallpaper.Common;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Windows.Threading;

namespace BitWallpaper.ViewModels
{
    // 通貨ペア
    public enum Pairs
    {
        btc_jpy, xrp_jpy, ltc_btc, eth_btc, mona_jpy, mona_btc, bcc_jpy, bcc_btc
    }

    #region == チック履歴 TickHistory クラス ==

    // TickHistoryクラス 
    public class TickHistory
    {
        // 価格
        public decimal Price { get; set; }
        public String PriceString
        {
            get { return String.Format("{0:#,0}", Price); }
        }

        // 日時
        public DateTime TimeAt { get; set; }
        // タイムスタンプ文字列
        public string TimeStamp
        {
            get { return TimeAt.ToLocalTime().ToString("HH:mm:ss"); }
        }

        //public Color TickHistoryPriceColor { get; set; }

        public bool TickHistoryPriceUp { get; set; }

        public TickHistory()
        {

        }
    }

    #endregion

    #region == テーマ用のクラス ==

    /// <summary>
    /// テーマ用のクラス
    /// </summary>
    public class Theme
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string IconData { get; set; }
    }

    #endregion

    #region == イベント定義 ==

    public class ShowBalloonEventArgs : EventArgs
    {
        public string Title { get; set; }
        public string Text { get; set; }
    }

    #endregion

    public class MainViewModel : ViewModelBase
    {

        #region == 基本 ==

        /// 0.0.0.8  (1.5.3)
        /// カスタムアラート値が保存されないバグ修正、タイトルメニューのアイコン余白調整、午前9時問題でバグがあった、ウィンドウ最大時余白修正。
        /// 0.0.0.7  (1.5.2)
        /// システムトレイへ最小化、カスタムアラートの追加、バルーン通知
        /// 0.0.0.6  (1.5.1)
        /// アラーム（24時間最高・最低値の場合も、起動後10秒待つように変更し、デフォルトもオフに）
        /// 0.0.0.5
        /// アラーム機能を見直し(デフォルトロウソク足タイプ変更、ピン止め追加、初期画面サイズ変更、投げ銭追加、非フォーカス時透過変更)
        /// 0.0.0.4
        /// 一覧機能を削除し、BitDeskを元にしつつ取引機能を省いた、BitDesk機能限定バージョンに。
        /// 0.0.0.3
        /// テーマ切替と背景透過率の変更機能を追加。
        /// 0.0.0.2
        /// ロウソク足とチャート表示期間のアイコンを付けて、チャート表示期間を選択で切り替えられるようにしました。
        /// 

        // Application version
        private string _appVer = "0.0.0.8";

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

        #endregion

        #region == 表示切替 ==

        // 設定画面表示フラグ
        private bool _showSettings = false;
        public bool ShowSettings
        {
            get
            {
                return _showSettings;
            }
            set
            {
                if (_showSettings == value) return;

                _showSettings = value;
                this.NotifyPropertyChanged("ShowSettings");

                if (_showSettings)
                    ShowMainContents = false;
                else
                    ShowMainContents = true;
            }
        }

        // 設定画面表示中にメインの内容を隠すフラグ
        private bool _showMainContents = true;
        public bool ShowMainContents
        {
            get
            {
                return _showMainContents;
            }
            set
            {
                if (_showMainContents == value) return;

                _showMainContents = value;
                this.NotifyPropertyChanged("ShowMainContents");
            }
        }

        #region == 省エネ用のプロパティ ==

        // 省エネモード
        private bool _minMode = false;
        public bool MinMode
        {
            get
            {
                return _minMode;
            }
            set
            {
                if (_minMode == value) return;

                _minMode = value;

                this.NotifyPropertyChanged("MinMode");
                this.NotifyPropertyChanged("NotMinMode");
            }
        }

        // 省エネモードにおける、コントロールの表示・非表示フラグ
        public bool NotMinMode
        {
            get
            {
                return !_minMode;
            }
        }

        #endregion

        #endregion

        // 設定

        // グローバルアラーム音設定
        private bool _playSound = true;
        public bool PlaySound
        {
            get
            {
                return _playSound;
            }
            set
            {
                if (_playSound == value)
                    return;

                _playSound = value;
                this.NotifyPropertyChanged("PlaySound");
            }
        }

        // APIエラー結果文字列 
        private string _aPIResultTicker;
        public string APIResultTicker
        {
            get
            {
                return _aPIResultTicker;
            }
            set
            {
                if (_aPIResultTicker == value) return;

                _aPIResultTicker = value;
                this.NotifyPropertyChanged("APIResultTicker");
            }
        }

        #region == テーマ関係 ==

        // テーマ一覧
        private ObservableCollection<Theme> _themes;
        public ObservableCollection<Theme> Themes
        {
            get { return _themes; }
            set { _themes = value; }
        }

        // テーマ切替
        private Theme _currentTheme;
        public Theme CurrentTheme
        {
            get
            {
                return _currentTheme;
            }
            set
            {
                if (_currentTheme == value) return;

                _currentTheme = value;
                this.NotifyPropertyChanged("CurrentTheme");

                (Application.Current as App).ChangeTheme(_currentTheme.Name);

            }
        }

        // 画面透過率
        private double _windowOpacity = 0.3;
        public double WindowOpacity
        {
            get
            {
                return _windowOpacity;
            }
            set
            {
                if (_windowOpacity == value) return;

                _windowOpacity = value;
                this.NotifyPropertyChanged("WindowOpacity");
            }
        }

        #endregion

        #region == 各通貨ペア用のクラス ==

        /// <summary>
        /// 各通貨ペア毎の情報を保持するクラス
        /// </summary>
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
                get { return _ltpFontSize; }
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
                get { return String.Format("{0:#,0}", _bid); }
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
                get { return String.Format("{0:#,0}", _ask); }
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
                get { return PairString + " - " + _tickTimeStamp.ToLocalTime().ToString("yyyy/MM/dd/HH:mm:ss"); }
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
                get { return _averagePrice; }
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
                get { return _highestIn24Price; }
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
                get { return String.Format(_ltpFormstString, _highestIn24Price); }
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
                get { return _lowestIn24Price; }
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
                get { return String.Format(_ltpFormstString, _lowestIn24Price); }
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
                get { return _highestPrice; }
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
                get { return _lowestPrice; }
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
                get { return this._tickHistory; }
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

                BindingOperations.EnableCollectionSynchronization(this._tickHistory, new object());
            }

        }

        #endregion

        #region == 通貨ペア切り替え用のプロパティ ==

        // 左メニュータブの選択インデックス
        private int _activePairIndex = 0;
        public int ActivePairIndex
        {
            get
            {
                return _activePairIndex;
            }
            set
            {
                if (_activePairIndex == value)
                    return;

                _activePairIndex = value;
                this.NotifyPropertyChanged("ActivePairIndex");

                if (_activePairIndex == 0)
                {
                    CurrentPair = Pairs.btc_jpy;
                    ActivePair = PairBtcJpy;

                    // 主にチャートの切替
                    IsBtcJpyVisible = true;
                    IsXrpJpyVisible = false;
                    IsEthBtcVisible = false;
                    IsLtcBtcVisible = false;
                    IsMonaJpyVisible = false;
                    IsBchJpyVisible = false;

                    DepthGroupingChanged = true;
                }
                else if (_activePairIndex == 1)
                {
                    CurrentPair = Pairs.xrp_jpy;
                    ActivePair = PairXrpJpy;

                    IsBtcJpyVisible = false;
                    IsXrpJpyVisible = true;
                    IsEthBtcVisible = false;
                    IsLtcBtcVisible = false;
                    IsMonaJpyVisible = false;
                    IsBchJpyVisible = false;

                    DepthGroupingChanged = true;
                }
                else if (_activePairIndex == 2)
                {
                    CurrentPair = Pairs.ltc_btc;
                    ActivePair = PairLtcBtc;

                    IsBtcJpyVisible = false;
                    IsXrpJpyVisible = false;
                    IsEthBtcVisible = false;
                    IsLtcBtcVisible = true;
                    IsMonaJpyVisible = false;
                    IsBchJpyVisible = false;

                    DepthGroupingChanged = true;
                }
                else if (_activePairIndex == 3)
                {
                    CurrentPair = Pairs.eth_btc;
                    ActivePair = PairEthBtc;

                    IsBtcJpyVisible = false;
                    IsXrpJpyVisible = false;
                    IsEthBtcVisible = true;
                    IsLtcBtcVisible = false;
                    IsMonaJpyVisible = false;
                    IsBchJpyVisible = false;

                    DepthGroupingChanged = true;
                }
                else if (_activePairIndex == 4)
                {
                    CurrentPair = Pairs.mona_jpy;
                    ActivePair = PairMonaJpy;

                    IsBtcJpyVisible = false;
                    IsXrpJpyVisible = false;
                    IsEthBtcVisible = false;
                    IsLtcBtcVisible = false;
                    IsMonaJpyVisible = true;
                    IsBchJpyVisible = false;

                    DepthGroupingChanged = true;
                }
                else if (_activePairIndex == 5)
                {
                    CurrentPair = Pairs.bcc_jpy;
                    ActivePair = PairBchJpy;

                    IsBtcJpyVisible = false;
                    IsXrpJpyVisible = false;
                    IsEthBtcVisible = false;
                    IsLtcBtcVisible = false;
                    IsMonaJpyVisible = false;
                    IsBchJpyVisible = true;

                    DepthGroupingChanged = true;
                }

                // チャートの表示
                //DisplayChart(CurrentPair);

            }
        }

        // 現在の通貨ペア
        private Pairs _currentPair = Pairs.btc_jpy;
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
                this.NotifyPropertyChanged("CurrentCoinString");
                this.NotifyPropertyChanged("CurrentPairUnitString");
                this.NotifyPropertyChanged("TickTimeStampString");

            }
        }

        // 表示用 通貨 
        public string CurrentCoinString
        {
            get
            {
                return CurrentPairCoin[CurrentPair];//.ToUpper();
            }
        }

        // 表示用 円/BTC 単位
        public string CurrentPairUnitString
        {
            get
            {
                if ((CurrentPair == Pairs.btc_jpy) || (CurrentPair == Pairs.xrp_jpy) || (CurrentPair == Pairs.mona_jpy) || (CurrentPair == Pairs.bcc_jpy))
                {
                    return "円";
                }
                else if ((CurrentPair == Pairs.eth_btc) || (CurrentPair == Pairs.ltc_btc))
                {
                    return "BTC";
                }
                else
                {
                    return "?";
                }

            }
        }

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

        public Dictionary<Pairs, string> CurrentPairCoin { get; set; } = new Dictionary<Pairs, string>()
        {
            {Pairs.btc_jpy, "BTC"},
            {Pairs.xrp_jpy, "XRP"},
            {Pairs.eth_btc, "ETH"},
            {Pairs.ltc_btc, "LTC"},
            {Pairs.mona_jpy, "Mona"},
            {Pairs.mona_btc, "Mona"},
            {Pairs.bcc_jpy, "BCH"},
            {Pairs.bcc_btc, "BCH"},
        };

        // デフォの通貨ペアクラス
        private Pair _activePair = new Pair(Pairs.btc_jpy, 28, "{0:#,0}", 100M, 1000M);
        public Pair ActivePair
        {
            get
            {
                return _activePair;
            }
            set
            {
                if (_activePair == value)
                    return;

                _activePair = value;
                this.NotifyPropertyChanged("ActivePair");
            }
        }

        private Pair _pairBtcJpy = new Pair(Pairs.btc_jpy, 24, "{0:#,0}", 100M, 1000M);
        public Pair PairBtcJpy
        {
            get
            {
                return _pairBtcJpy;
            }
        }

        private bool _isBtcJpyVisible;
        public bool IsBtcJpyVisible
        {
            get
            {
                return _isBtcJpyVisible;
            }
            set
            {
                if (_isBtcJpyVisible == value)
                    return;

                _isBtcJpyVisible = value;
                this.NotifyPropertyChanged("IsBtcJpyVisible");
            }
        }

        private Pair _pairXrpJpy = new Pair(Pairs.xrp_jpy, 24, "{0:#,0.000}", 0.1M, 0.01M);
        public Pair PairXrpJpy
        {
            get
            {
                return _pairXrpJpy;
            }
        }

        private bool _isXrpJpyVisible;
        public bool IsXrpJpyVisible
        {
            get
            {
                return _isXrpJpyVisible;
            }
            set
            {
                if (_isXrpJpyVisible == value)
                    return;

                _isXrpJpyVisible = value;
                this.NotifyPropertyChanged("IsXrpJpyVisible");
            }
        }

        private Pair _pairEthBtc = new Pair(Pairs.eth_btc, 24, "{0:#,0.00000000}", 0.0001M, 0.00001M);
        public Pair PairEthBtc
        {
            get
            {
                return _pairEthBtc;
            }
        }

        private bool _isEthBtcVisible;
        public bool IsEthBtcVisible
        {
            get
            {
                return _isEthBtcVisible;
            }
            set
            {
                if (_isEthBtcVisible == value)
                    return;

                _isEthBtcVisible = value;
                this.NotifyPropertyChanged("IsEthBtcVisible");
            }
        }

        private Pair _pairLtcBtc = new Pair(Pairs.ltc_btc, 24, "{0:#,0.00000000}", 0.0001M, 0.00001M);
        public Pair PairLtcBtc
        {
            get
            {
                return _pairLtcBtc;
            }
        }

        private bool _isLtcBtcVisible;
        public bool IsLtcBtcVisible
        {
            get
            {
                return _isLtcBtcVisible;
            }
            set
            {
                if (_isLtcBtcVisible == value)
                    return;

                _isLtcBtcVisible = value;
                this.NotifyPropertyChanged("IsLtcBtcVisible");
            }
        }

        private Pair _pairMonaJpy = new Pair(Pairs.mona_jpy, 24, "{0:#,0.000}", 0.1M, 1M);
        public Pair PairMonaJpy
        {
            get
            {
                return _pairMonaJpy;
            }
        }

        private bool _isMonaJpyVisible;
        public bool IsMonaJpyVisible
        {
            get
            {
                return _isMonaJpyVisible;
            }
            set
            {
                if (_isMonaJpyVisible == value)
                    return;

                _isMonaJpyVisible = value;
                this.NotifyPropertyChanged("IsMonaJpyVisible");
            }
        }

        private Pair _pairBchJpy = new Pair(Pairs.bcc_jpy, 24, "{0:#,0}", 10M, 100M);
        public Pair PairBchJpy
        {
            get
            {
                return _pairBchJpy;
            }
        }

        private bool _isBchJpyVisible;
        public bool IsBchJpyVisible
        {
            get
            {
                return _isBchJpyVisible;
            }
            set
            {
                if (_isBchJpyVisible == value)
                    return;

                _isBchJpyVisible = value;
                this.NotifyPropertyChanged("IsBchJpyVisible");
            }
        }

        #endregion

        #region == 板情報のプロパティ ==

        // 板情報
        private ObservableCollection<Depth> _depth = new ObservableCollection<Depth>();
        public ObservableCollection<Depth> Depths
        {
            get { return this._depth; }
        }

        private bool _depthGroupingChanged;
        public bool DepthGroupingChanged
        {
            get
            {
                return _depthGroupingChanged;
            }
            set
            {
                if (_depthGroupingChanged == value)
                    return;

                _depthGroupingChanged = value;

                this.NotifyPropertyChanged("DepthGroupingChanged");
            }
        }

        #endregion
        
        #region == トランザクション（歩み値）のプロパティ ==

        private ObservableCollection<Transactions> _transactions = new ObservableCollection<Transactions>();
        public ObservableCollection<Transactions> Transactions
        {
            get { return this._transactions; }
        }

        #endregion

        #region == クライアントのプロパティ ==

        // 公開API ロウソク足 クライアント
        PublicAPIClient _pubCandlestickApi = new PublicAPIClient();

        // 公開API Ticker クライアント
        PublicAPIClient _pubTickerApi = new PublicAPIClient();

        // 公開API Depth クライアント
        PublicAPIClient _pubDepthApi = new PublicAPIClient();

        // 公開API Transactions クライアント
        PublicAPIClient _pubTransactionsApi = new PublicAPIClient();

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
            {CandleTypes.OneMin, "1分足"},
            //{CandleTypes.FiveMin, "５分" },
            //{CandleTypes.FifteenMin, "１５分"},
            //{CandleTypes.ThirteenMin, "３０分" },
            {CandleTypes.OneHour, "1時間" },
            //{CandleTypes.FourHour, "４時間"},
            //{CandleTypes.EightHour, "８時間" },
            //{CandleTypes.TwelveHour, "１２時間"},
            {CandleTypes.OneDay, "日足" },
            //{CandleTypes.OneWeek, "週足"},

        };

        // 選択されたロウソク足タイプ
        public CandleTypes _selectedCandleType = CandleTypes.OneMin; // デフォ。変更注意。起動時のロードと合わせる。
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

                // 
                if (_selectedCandleType == CandleTypes.OneMin)
                {
                    // 一分毎のロウソク足タイプなら

                    if ((SelectedChartSpan != ChartSpans.OneHour) && (SelectedChartSpan != ChartSpans.ThreeHour))
                    {
                        // デフォルト 3時間の期間で表示
                        SelectedChartSpan = ChartSpans.ThreeHour;
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

                    Debug.WriteLine("■" + _selectedCandleType.ToString() + " チャート表示、設定範囲外");

                    return;

                    // デフォルト 1日の期間で表示
                    //SelectedChartSpan = ChartSpans.OneDay;
                    //Debug.WriteLine("デフォルト Oops");

                }

                Debug.WriteLine(_selectedCandleType.ToString() + " チャート表示");

                // チャート表示
                //LoadChart();
                DisplayCharts();

            }
        }

        // 読み込み中状態を知らせる文字列
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

        // チャート表示期間
        public enum ChartSpans
        {
            OneHour, ThreeHour, OneDay, ThreeDay, OneWeek, OneMonth, TwoMonth, OneYear, FiveYear
        }

        // チャート表示期間　コンボボックス表示用
        public Dictionary<ChartSpans, string> ChartSpansDictionary { get; } = new Dictionary<ChartSpans, string>()
        {
            {ChartSpans.OneHour, "1時間"},
            {ChartSpans.ThreeHour, "3時間"},
            {ChartSpans.ThreeDay, "3日間" },
            {ChartSpans.TwoMonth, "2ヵ月間" },

        };

        // 選択されたチャート表示期間 
        private ChartSpans _chartSpan = ChartSpans.ThreeHour; 
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

                ChangeChartSpans();
                //DisplayCharts();

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

        #region == チャートデータ用のプロパティ ==

        #region == BTCチャートデータ用のプロパティ ==

        // === BTC === 
        private SeriesCollection _chartSeriesBtcJpy = new SeriesCollection();
        public SeriesCollection ChartSeriesBtcJpy
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
                this.NotifyPropertyChanged("ChartSeriesBtcJpy");
            }
        }

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
                this.NotifyPropertyChanged("ChartAxisXBtcJpy");
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
                this.NotifyPropertyChanged("ChartAxisYBtcJpy");
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

        #endregion

        #region == LTCャートデータ用のプロパティ ==

        // === LTC === 
        private SeriesCollection _chartSeriesLtcBtc = new SeriesCollection();
        public SeriesCollection ChartSeriesLtcBtc
        {
            get
            {
                return _chartSeriesLtcBtc;
            }
            set
            {
                if (_chartSeriesLtcBtc == value)
                    return;

                _chartSeriesLtcBtc = value;
                this.NotifyPropertyChanged("ChartSeriesLtcBtc");
            }
        }

        private AxesCollection _chartAxisXLtcBtc = new AxesCollection();
        public AxesCollection ChartAxisXLtcBtc
        {
            get
            {
                return _chartAxisXLtcBtc;
            }
            set
            {
                if (_chartAxisXLtcBtc == value)
                    return;

                _chartAxisXLtcBtc = value;
                this.NotifyPropertyChanged("ChartAxisXLtcBtc");
            }
        }

        private AxesCollection _chartAxisYLtcBtc = new AxesCollection();
        public AxesCollection ChartAxisYLtcBtc
        {
            get
            {
                return _chartAxisYLtcBtc;
            }
            set
            {
                if (_chartAxisYLtcBtc == value)
                    return;

                _chartAxisYLtcBtc = value;
                this.NotifyPropertyChanged("ChartAxisYLtcBtc");
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

        #endregion

        #region == XRPャートデータ用のプロパティ ==

        // === XRP === 
        private SeriesCollection _chartSeriesXrpJpy;
        public SeriesCollection ChartSeriesXrpJpy
        {
            get
            {
                return _chartSeriesXrpJpy;
            }
            set
            {
                if (_chartSeriesXrpJpy == value)
                    return;

                _chartSeriesXrpJpy = value;
                this.NotifyPropertyChanged("ChartSeriesXrp");
            }
        }

        private AxesCollection _chartAxisXXrpJpy;
        public AxesCollection ChartAxisXXrpJpy
        {
            get
            {
                return _chartAxisXXrpJpy;
            }
            set
            {
                if (_chartAxisXXrpJpy == value)
                    return;

                _chartAxisXXrpJpy = value;
                this.NotifyPropertyChanged("ChartAxisXXrpJpy");
            }
        }

        private AxesCollection _chartAxisYXrpJpy;
        public AxesCollection ChartAxisYXrpJpy
        {
            get
            {
                return _chartAxisYXrpJpy;
            }
            set
            {
                if (_chartAxisYXrpJpy == value)
                    return;

                _chartAxisYXrpJpy = value;
                this.NotifyPropertyChanged("ChartAxisYXrpJpy");
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

        #endregion

        #region == Ethャートデータ用のプロパティ ==

        // === Eth === 
        private SeriesCollection _chartSeriesEthBtc;
        public SeriesCollection ChartSeriesEthBtc
        {
            get
            {
                return _chartSeriesEthBtc;
            }
            set
            {
                if (_chartSeriesEthBtc == value)
                    return;

                _chartSeriesEthBtc = value;
                this.NotifyPropertyChanged("ChartSeriesEthBtc");
            }
        }

        private AxesCollection _chartAxisXEthBtc;
        public AxesCollection ChartAxisXEthBtc
        {
            get
            {
                return _chartAxisXEthBtc;
            }
            set
            {
                if (_chartAxisXEthBtc == value)
                    return;

                _chartAxisXEthBtc = value;
                this.NotifyPropertyChanged("ChartAxisXEthBtc");
            }
        }

        private AxesCollection _chartAxisYEthBtc;
        public AxesCollection ChartAxisYEthBtc
        {
            get
            {
                return _chartAxisYEthBtc;
            }
            set
            {
                if (_chartAxisYEthBtc == value)
                    return;

                _chartAxisYEthBtc = value;
                this.NotifyPropertyChanged("ChartAxisYEthBtc");
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

        #endregion

        #region == Monaャートデータ用のプロパティ ==

        // === Mona === 
        private SeriesCollection _chartSeriesMonaJpy;
        public SeriesCollection ChartSeriesMonaJpy
        {
            get
            {
                return _chartSeriesMonaJpy;
            }
            set
            {
                if (_chartSeriesMonaJpy == value)
                    return;

                _chartSeriesMonaJpy = value;
                this.NotifyPropertyChanged("ChartSeriesMonaJpy");
            }
        }

        private AxesCollection _chartAxisXMonaJpy;
        public AxesCollection ChartAxisXMonaJpy
        {
            get
            {
                return _chartAxisXMonaJpy;
            }
            set
            {
                if (_chartAxisXMonaJpy == value)
                    return;

                _chartAxisXMonaJpy = value;
                this.NotifyPropertyChanged("ChartAxisXMonaJpy");
            }
        }

        private AxesCollection _chartAxisYMonaJpy;
        public AxesCollection ChartAxisYMonaJpy
        {
            get
            {
                return _chartAxisYMonaJpy;
            }
            set
            {
                if (_chartAxisYMonaJpy == value)
                    return;

                _chartAxisYMonaJpy = value;
                this.NotifyPropertyChanged("ChartAxisYMonaJpy");
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

        #endregion

        #region == Bchャートデータ用のプロパティ ==

        // === Bch === 
        private SeriesCollection _chartSeriesBchJpy;
        public SeriesCollection ChartSeriesBchJpy
        {
            get
            {
                return _chartSeriesBchJpy;
            }
            set
            {
                if (_chartSeriesBchJpy == value)
                    return;

                _chartSeriesBchJpy = value;
                this.NotifyPropertyChanged("ChartSeriesBchJpy");
            }
        }

        private AxesCollection _chartAxisXBchJpy;
        public AxesCollection ChartAxisXBchJpy
        {
            get
            {
                return _chartAxisXBchJpy;
            }
            set
            {
                if (_chartAxisXBchJpy == value)
                    return;

                _chartAxisXBchJpy = value;
                this.NotifyPropertyChanged("ChartAxisXBchJpy");
            }
        }

        private AxesCollection _chartAxisYBchJpy;
        public AxesCollection ChartAxisYBchJpy
        {
            get
            {
                return _chartAxisYBchJpy;
            }
            set
            {
                if (_chartAxisYBchJpy == value)
                    return;

                _chartAxisYBchJpy = value;
                this.NotifyPropertyChanged("ChartAxisYBchJpy");
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

        #endregion

        /*
        #region == チャートデータ用のプロパティ ==

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
        */

        #endregion

        #region == タイマー ==

        System.Windows.Threading.DispatcherTimer dispatcherTimerTickOtherPairs = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer dispatcherChartTimer = new System.Windows.Threading.DispatcherTimer();

        #endregion

        // イベント
        public event EventHandler<ShowBalloonEventArgs> ShowBalloon;


        /// <summary>
        /// メインのビューモデル
        /// </summary>
        public MainViewModel()
        {
            // コマンドのイニシャライズ
            ShowSettingsCommand = new RelayCommand(ShowSettingsCommand_Execute, ShowSettingsCommand_CanExecute);
            SettingsCancelCommand = new RelayCommand(SettingsCancelCommand_Execute, SettingsCancelCommand_CanExecute);
            SettingsOKCommand = new RelayCommand(SettingsOKCommand_Execute, SettingsOKCommand_CanExecute);

            DepthGroupingCommand = new GenericRelayCommand<object>(
                param => DepthGroupingCommand_Execute(param),
                param => DepthGroupingCommand_CanExecute());

            // ObservableCollection collections 
            BindingOperations.EnableCollectionSynchronization(this._depth, new object());
            BindingOperations.EnableCollectionSynchronization(this._transactions, new object());

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
                Style styleX = Application.Current.FindResource("ChartAxisStyle") as Style;
                caX.Style = styleX;

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
                //caY.Separator.Stroke = System.Windows.Media.Brushes.WhiteSmoke;
                Style styleYSec = Application.Current.FindResource("ChartSeparatorStyle") as Style;
                caY.Separator.Style = styleYSec;
                Style styleY = Application.Current.FindResource("ChartAxisStyle") as Style;
                caY.Style = styleY;
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
                //axs.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(150, 172, 206));
                Style styleSection = Application.Current.FindResource("ChartSectionStyle") as Style;
                axs.Style = styleSection;
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
                    },

                     new ColumnSeries
                    {
                        Title = "出来高",
                        Values = new ChartValues<double> {},
                        ScalesYAt = 1,
                        //Fill = yellowBrush,
                        Style = (Application.Current.FindResource("ChartVolumeStyle") as Style),
                    }

                };

                if (pair == Pairs.btc_jpy)
                {
                    ChartSeriesBtcJpy = chartSeries;
                    ChartAxisXBtcJpy = chartAxisX;
                    ChartAxisYBtcJpy = chartAxisY;
                }
                else if (pair == Pairs.xrp_jpy)
                {
                    ChartSeriesXrpJpy = chartSeries;
                    ChartAxisXXrpJpy = chartAxisX;
                    ChartAxisYXrpJpy = chartAxisY;
                }
                else if (pair == Pairs.eth_btc)
                {
                    ChartSeriesEthBtc = chartSeries;
                    ChartAxisXEthBtc = chartAxisX;
                    ChartAxisYEthBtc = chartAxisY;
                }
                else if (pair == Pairs.mona_jpy)
                {
                    ChartSeriesMonaJpy = chartSeries;
                    ChartAxisXMonaJpy = chartAxisX;
                    ChartAxisYMonaJpy = chartAxisY;
                }
                else if (pair == Pairs.mona_btc)
                {
                    //
                }
                else if (pair == Pairs.ltc_btc)
                {
                    ChartSeriesLtcBtc = chartSeries;
                    ChartAxisXLtcBtc = chartAxisX;
                    ChartAxisYLtcBtc = chartAxisY;
                }
                else if (pair == Pairs.bcc_btc)
                {
                    //
                }
                else if (pair == Pairs.bcc_jpy)
                {
                    ChartSeriesBchJpy = chartSeries;
                    ChartAxisXBchJpy = chartAxisX;
                    ChartAxisYBchJpy = chartAxisY;
                }

            }


            #endregion

            #region == テーマのイニシャライズ ==

            // テーマの選択コンボボックスのイニシャライズ
            _themes = new ObservableCollection<Theme>()
            {
                new Theme() { Id = 1, Name = "DefaultTheme", Label = "Dark", IconData="M17.75,4.09L15.22,6.03L16.13,9.09L13.5,7.28L10.87,9.09L11.78,6.03L9.25,4.09L12.44,4L13.5,1L14.56,4L17.75,4.09M21.25,11L19.61,12.25L20.2,14.23L18.5,13.06L16.8,14.23L17.39,12.25L15.75,11L17.81,10.95L18.5,9L19.19,10.95L21.25,11M18.97,15.95C19.8,15.87 20.69,17.05 20.16,17.8C19.84,18.25 19.5,18.67 19.08,19.07C15.17,23 8.84,23 4.94,19.07C1.03,15.17 1.03,8.83 4.94,4.93C5.34,4.53 5.76,4.17 6.21,3.85C6.96,3.32 8.14,4.21 8.06,5.04C7.79,7.9 8.75,10.87 10.95,13.06C13.14,15.26 16.1,16.22 18.97,15.95M17.33,17.97C14.5,17.81 11.7,16.64 9.53,14.5C7.36,12.31 6.2,9.5 6.04,6.68C3.23,9.82 3.34,14.64 6.35,17.66C9.37,20.67 14.19,20.78 17.33,17.97Z"},
                new Theme() { Id = 2, Name = "LightTheme", Label = "Light", IconData="M12,7A5,5 0 0,1 17,12A5,5 0 0,1 12,17A5,5 0 0,1 7,12A5,5 0 0,1 12,7M12,9A3,3 0 0,0 9,12A3,3 0 0,0 12,15A3,3 0 0,0 15,12A3,3 0 0,0 12,9M12,2L14.39,5.42C13.65,5.15 12.84,5 12,5C11.16,5 10.35,5.15 9.61,5.42L12,2M3.34,7L7.5,6.65C6.9,7.16 6.36,7.78 5.94,8.5C5.5,9.24 5.25,10 5.11,10.79L3.34,7M3.36,17L5.12,13.23C5.26,14 5.53,14.78 5.95,15.5C6.37,16.24 6.91,16.86 7.5,17.37L3.36,17M20.65,7L18.88,10.79C18.74,10 18.47,9.23 18.05,8.5C17.63,7.78 17.1,7.15 16.5,6.64L20.65,7M20.64,17L16.5,17.36C17.09,16.85 17.62,16.22 18.04,15.5C18.46,14.77 18.73,14 18.87,13.21L20.64,17M12,22L9.59,18.56C10.33,18.83 11.14,19 12,19C12.82,19 13.63,18.83 14.37,18.56L12,22Z"}
            };
            // デフォルトにセット
            _currentTheme = _themes[0];

            #endregion

            // Tickerのタイマー起動
            dispatcherTimerTickOtherPairs.Tick += new EventHandler(TickerTimerOtherPairs);
            dispatcherTimerTickOtherPairs.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimerTickOtherPairs.Start();
            
            // Chart更新のタイマー
            dispatcherChartTimer.Tick += new EventHandler(ChartTimer);
            dispatcherChartTimer.Interval = new TimeSpan(0, 1, 0);
            //dispatcherChartTimer.Start();

            // 初期値
            ActivePairIndex = 0;
            CurrentPair = Pairs.btc_jpy;
            ActivePair = PairBtcJpy;
            ActivePair.Ltp = PairBtcJpy.Ltp;

            IsBtcJpyVisible = true;
            IsXrpJpyVisible = false;
            IsEthBtcVisible = false;
            IsLtcBtcVisible = false;
            IsMonaJpyVisible = false;
            IsBchJpyVisible = false;

            UpdateDepth();
            UpdateTransactions();

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

            foreach (Pairs pair in Enum.GetValues(typeof(Pairs)))
            {
                if ((pair == Pairs.mona_btc) || pair == Pairs.bcc_btc)
                {
                    continue;
                }

                // 起動直後アラームを鳴らさない秒数
                int waitTime = 10;

                Ticker tick = await _pubTickerApi.GetTicker(pair.ToString());

                if (tick != null)
                {
                    // Ticker 取得エラー表示をクリア
                    APIResultTicker = "";

                    try
                    {

                        if (pair == CurrentPair)
                        {
                            if (tick.LTP > ActivePair.Ltp)
                            {
                                ActivePair.LtpUpFlag = true;
                            }
                            else if (tick.LTP < ActivePair.Ltp)
                            {
                                ActivePair.LtpUpFlag = false;
                            }
                            else if (tick.LTP == ActivePair.Ltp)
                            {
                                //ActivePair.LtpColor = Colors.Gainsboro;
                            }
                        }

                        if (pair == Pairs.btc_jpy)
                        {

                            // 一旦前の値を保存
                            var prevLtp = PairBtcJpy.Ltp;

                            // 最新の価格をセット
                            PairBtcJpy.Ltp = tick.LTP;
                            PairBtcJpy.Bid = tick.Bid;
                            PairBtcJpy.Ask = tick.Ask;
                            PairBtcJpy.TickTimeStamp = tick.TimeStamp;

                            PairBtcJpy.LowestIn24Price = tick.Low;
                            PairBtcJpy.HighestIn24Price = tick.High;

                            // 起動時価格セット
                            if (PairBtcJpy.BasePrice == 0) PairBtcJpy.BasePrice = tick.LTP;

                            // 最安値登録
                            if (PairBtcJpy.LowestPrice == 0)
                            {
                                PairBtcJpy.LowestPrice = tick.LTP;
                            }
                            if (tick.LTP < PairBtcJpy.LowestPrice)
                            {
                                PairBtcJpy.LowestPrice = tick.LTP;
                            }

                            // 最高値登録
                            if (PairBtcJpy.HighestPrice == 0)
                            {
                                PairBtcJpy.HighestPrice = tick.LTP;
                            }
                            if (tick.LTP > PairBtcJpy.HighestPrice)
                            {
                                PairBtcJpy.HighestPrice = tick.LTP;
                            }

                            #region == チック履歴 ==

                            TickHistory aym = new TickHistory();
                            aym.Price = tick.LTP;
                            aym.TimeAt = tick.TimeStamp;
                            if (PairBtcJpy.TickHistories.Count > 0)
                            {
                                if (PairBtcJpy.TickHistories[0].Price > aym.Price)
                                {
                                    aym.TickHistoryPriceUp = true;
                                    PairBtcJpy.TickHistories.Insert(0, aym);

                                }
                                else if (PairBtcJpy.TickHistories[0].Price < aym.Price)
                                {
                                    aym.TickHistoryPriceUp = false;
                                    PairBtcJpy.TickHistories.Insert(0, aym);
                                }
                                else
                                {
                                    //aym.TickHistoryPriceColor = Colors.Gainsboro;
                                    PairBtcJpy.TickHistories.Insert(0, aym);
                                }
                            }
                            else
                            {
                                //aym.TickHistoryPriceColor = Colors.Gainsboro;
                                PairBtcJpy.TickHistories.Insert(0, aym);
                            }

                            // limit the number of the list.
                            if (PairBtcJpy.TickHistories.Count > 60)
                            {
                                PairBtcJpy.TickHistories.RemoveAt(60);
                            }

                            // 60(1分)の平均値を求める
                            decimal aSum = 0;
                            int c = 0;
                            if (PairBtcJpy.TickHistories.Count > 0)
                            {

                                if (PairBtcJpy.TickHistories.Count > 60)
                                {
                                    c = 59;
                                }
                                else
                                {
                                    c = PairBtcJpy.TickHistories.Count - 1;
                                }

                                if (c == 0)
                                {
                                    PairBtcJpy.AveragePrice = PairBtcJpy.TickHistories[0].Price;
                                }
                                else
                                {
                                    for (int i = 0; i < c; i++)
                                    {
                                        aSum = aSum + PairBtcJpy.TickHistories[i].Price;
                                    }
                                    PairBtcJpy.AveragePrice = aSum / c;
                                }

                            }
                            else if (PairBtcJpy.TickHistories.Count == 1)
                            {
                                PairBtcJpy.AveragePrice = PairBtcJpy.TickHistories[0].Price;
                            }

                            #endregion

                            #region == アラーム ==
                            
                            bool isPlayed = false;

                            // カスタムアラーム
                            if (PairBtcJpy.AlarmPlus > 0)
                            {
                                if (tick.LTP >= PairBtcJpy.AlarmPlus)
                                {
                                    PairBtcJpy.HighLowInfoTextColorFlag = true;
                                    PairBtcJpy.HighLowInfoText = PairBtcJpy.PairString + " ⇑⇑⇑　高値アラーム ";

                                    ShowBalloonEventArgs ag = new ShowBalloonEventArgs
                                    {
                                        Title = PairBtcJpy.PairString + " 高値アラーム",
                                        Text = PairBtcJpy.AlarmPlus.ToString("#,0") + " に達しました。"
                                    };
                                    // バルーン表示
                                    ShowBalloon?.Invoke(this, ag);
                                    // クリア
                                    PairBtcJpy.AlarmPlus = 0;

                                }
                            }

                            if (PairBtcJpy.AlarmMinus > 0)
                            {
                                if (tick.LTP <= PairBtcJpy.AlarmMinus)
                                {
                                    PairBtcJpy.HighLowInfoTextColorFlag = false;
                                    PairBtcJpy.HighLowInfoText = PairBtcJpy.PairString + " ⇓⇓⇓　安値アラーム ";

                                    ShowBalloonEventArgs ag = new ShowBalloonEventArgs
                                    {
                                        Title = PairBtcJpy.PairString + " 安値アラーム",
                                        Text = PairBtcJpy.AlarmMinus.ToString("#,0") + " に達しました。"
                                    };
                                    // バルーン表示
                                    ShowBalloon?.Invoke(this, ag);
                                    // クリア
                                    PairBtcJpy.AlarmMinus = 0;

                                }
                            }

                            // 起動後最高値
                            if ((tick.LTP >= PairBtcJpy.HighestPrice) && (prevLtp != tick.LTP))
                            {
                                if ((PairBtcJpy.TickHistories.Count > waitTime) && ((PairBtcJpy.BasePrice + 100M) < tick.LTP))
                                {

                                    // 値を点滅させるフラグ
                                    if (PairBtcJpy.PlaySoundHighest)
                                        PairBtcJpy.HighestPriceAlart = true;

                                    // 音を鳴らす
                                    if ((isPlayed == false) && (PairBtcJpy.PlaySoundHighest == true))
                                    {

                                        // 色を変える
                                        PairBtcJpy.HighLowInfoTextColorFlag = true;
                                        // テキストを点滅させる為、一旦クリア
                                        PairBtcJpy.HighLowInfoText = "";
                                        PairBtcJpy.HighLowInfoText = PairBtcJpy.PairString + " ⇑⇑⇑　起動後最高値 ";

                                        if (PlaySound)
                                        {
                                            SystemSounds.Hand.Play();
                                            isPlayed = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                PairBtcJpy.HighestPriceAlart = false;
                            }

                            // 起動後最安値
                            if ((tick.LTP <= PairBtcJpy.LowestPrice) && (prevLtp != tick.LTP))
                            {
                                if ((PairBtcJpy.TickHistories.Count > waitTime) && ((PairBtcJpy.BasePrice - 100M) > tick.LTP))
                                {

                                    if (PairBtcJpy.PlaySoundLowest)
                                        PairBtcJpy.LowestPriceAlart = true;

                                    if ((isPlayed == false) && (PairBtcJpy.PlaySoundLowest == true))
                                    {
                                        PairBtcJpy.HighLowInfoTextColorFlag = false;
                                        PairBtcJpy.HighLowInfoText = "";
                                        PairBtcJpy.HighLowInfoText = PairBtcJpy.PairString + " ⇓⇓⇓　起動後最安値 ";

                                        if (PlaySound)
                                        {
                                            SystemSounds.Beep.Play();
                                            isPlayed = true;
                                        }
                                    }

                                }
                            }
                            else
                            {
                                PairBtcJpy.LowestPriceAlart = false;
                            }

                            // 過去24時間最高値
                            if ((tick.LTP >= PairBtcJpy.HighestIn24Price) && (prevLtp != tick.LTP) && (PairBtcJpy.TickHistories.Count > waitTime))
                            {
                                if (PairBtcJpy.PlaySoundHighest24h)
                                    PairBtcJpy.HighestIn24PriceAlart = true;

                                if ((isPlayed == false) && (PairBtcJpy.PlaySoundHighest24h == true))
                                {
                                    PairBtcJpy.HighLowInfoTextColorFlag = true;
                                    PairBtcJpy.HighLowInfoText = "";
                                    PairBtcJpy.HighLowInfoText = PairBtcJpy.PairString + " ⇑⇑⇑⇑⇑⇑　24時間最高値 ";

                                    if (PlaySound)
                                    {
                                        SystemSounds.Hand.Play();
                                        isPlayed = true;
                                    }
                                }
                            }
                            else
                            {
                                PairBtcJpy.HighestIn24PriceAlart = false;
                            }

                            // 過去24時間最安値
                            if ((tick.LTP <= PairBtcJpy.LowestIn24Price) && (prevLtp != tick.LTP) && (PairBtcJpy.TickHistories.Count > waitTime))
                            {

                                if (PairBtcJpy.PlaySoundLowest24h)
                                    PairBtcJpy.LowestIn24PriceAlart = true;

                                if ((isPlayed == false) && (PairBtcJpy.PlaySoundLowest24h == true))
                                {
                                    PairBtcJpy.HighLowInfoTextColorFlag = false;
                                    PairBtcJpy.HighLowInfoText = "";
                                    PairBtcJpy.HighLowInfoText = PairBtcJpy.PairString + " ⇓⇓⇓⇓⇓⇓　24時間最安値 ";

                                    if (PlaySound)
                                    {
                                        SystemSounds.Hand.Play();
                                        isPlayed = true;
                                    }
                                }
                            }
                            else
                            {
                                PairBtcJpy.LowestIn24PriceAlart = false;
                            }

                            #endregion

                            // 省エネモードでなかったら
                            if ((MinMode == false) && (pair == CurrentPair))
                            {
                                // 最新取引価格のラインを更新
                                if (ChartAxisYBtcJpy != null)
                                {
                                    if (ChartAxisYBtcJpy.Count > 0)
                                    {
                                        if (ChartAxisYBtcJpy[0].Sections.Count > 0)
                                        {
                                            ChartAxisYBtcJpy[0].Sections[0].Value = (double)tick.LTP;
                                        }
                                    }

                                    // 最新のロウソク足を更新する。
                                    //＞＞＞重すぎ。負荷掛かり過ぎなので止め。
                                    /*
                                    if (ChartSeriesBtcJpy != null)
                                    {
                                        if (ChartSeriesBtcJpy[0].Values != null)
                                        {
                                            int cb = ChartSeriesBtcJpy[0].Values.Count;

                                            if (cb > 0)
                                            {
                                                double l = ((OhlcPoint)ChartSeriesBtcJpy[0].Values[cb - 1]).Low;
                                                double h = ((OhlcPoint)ChartSeriesBtcJpy[0].Values[cb - 1]).High;

                                                if (Application.Current == null) return;
                                                Application.Current.Dispatcher.Invoke(() =>
                                                {

                                                    ((OhlcPoint)ChartSeriesBtcJpy[0].Values[cb - 1]).Close = (double)tick.LTP;

                                                    if (l > (double)tick.LTP)
                                                    {
                                                        ((OhlcPoint)ChartSeriesBtcJpy[0].Values[cb - 1]).Low = (double)tick.LTP;
                                                    }

                                                    if (h < (double)tick.LTP)
                                                    {
                                                        ((OhlcPoint)ChartSeriesBtcJpy[0].Values[cb - 1]).High = (double)tick.LTP;
                                                    }

                                                });

                                            }
                                        }

                                    }
                                    */

                                }

                            }

                        }
                        else if (pair == Pairs.xrp_jpy)
                        {

                            // 一旦前の値を保存
                            var prevLtp = PairXrpJpy.Ltp;

                            // 最新の価格をセット
                            PairXrpJpy.Ltp = tick.LTP;
                            PairXrpJpy.Bid = tick.Bid;
                            PairXrpJpy.Ask = tick.Ask;
                            PairXrpJpy.TickTimeStamp = tick.TimeStamp;

                            PairXrpJpy.LowestIn24Price = tick.Low;
                            PairXrpJpy.HighestIn24Price = tick.High;

                            // 起動時価格セット
                            if (PairXrpJpy.BasePrice == 0) PairXrpJpy.BasePrice = tick.LTP;

                            // 最安値登録
                            if (PairXrpJpy.LowestPrice == 0)
                            {
                                PairXrpJpy.LowestPrice = tick.LTP;
                            }
                            if (tick.LTP < PairXrpJpy.LowestPrice)
                            {
                                //SystemSounds.Beep.Play();
                                PairXrpJpy.LowestPrice = tick.LTP;
                            }

                            // 最高値登録
                            if (PairXrpJpy.HighestPrice == 0)
                            {
                                PairXrpJpy.HighestPrice = tick.LTP;
                            }
                            if (tick.LTP > PairXrpJpy.HighestPrice)
                            {
                                //SystemSounds.Asterisk.Play();
                                PairXrpJpy.HighestPrice = tick.LTP;
                            }

                            #region == チック履歴 ==

                            TickHistory aym = new TickHistory();
                            aym.Price = tick.LTP;
                            aym.TimeAt = tick.TimeStamp;
                            if (PairXrpJpy.TickHistories.Count > 0)
                            {
                                if (PairXrpJpy.TickHistories[0].Price > aym.Price)
                                {
                                    //aym.TickHistoryPriceColor = _priceUpColor;
                                    aym.TickHistoryPriceUp = true;
                                    PairXrpJpy.TickHistories.Insert(0, aym);

                                }
                                else if (PairXrpJpy.TickHistories[0].Price < aym.Price)
                                {
                                    //aym.TickHistoryPriceColor = _priceDownColor;
                                    aym.TickHistoryPriceUp = false;
                                    PairXrpJpy.TickHistories.Insert(0, aym);
                                }
                                else
                                {
                                    //aym.TickHistoryPriceColor = Colors.Gainsboro;
                                    PairXrpJpy.TickHistories.Insert(0, aym);
                                }
                            }
                            else
                            {
                                //aym.TickHistoryPriceColor = Colors.Gainsboro;
                                PairXrpJpy.TickHistories.Insert(0, aym);
                            }

                            // limit the number of the list.
                            if (PairXrpJpy.TickHistories.Count > 60)
                            {
                                PairXrpJpy.TickHistories.RemoveAt(60);
                            }

                            // 60(1分)の平均値を求める
                            decimal aSum = 0;
                            int c = 0;
                            if (PairXrpJpy.TickHistories.Count > 0)
                            {

                                if (PairXrpJpy.TickHistories.Count > 60)
                                {
                                    c = 59;
                                }
                                else
                                {
                                    c = PairXrpJpy.TickHistories.Count - 1;
                                }

                                if (c == 0)
                                {
                                    PairXrpJpy.AveragePrice = PairXrpJpy.TickHistories[0].Price;
                                }
                                else
                                {
                                    for (int i = 0; i < c; i++)
                                    {
                                        aSum = aSum + PairXrpJpy.TickHistories[i].Price;
                                    }
                                    PairXrpJpy.AveragePrice = aSum / c;
                                }

                            }
                            else if (PairXrpJpy.TickHistories.Count == 1)
                            {
                                PairXrpJpy.AveragePrice = PairXrpJpy.TickHistories[0].Price;
                            }

                            #endregion

                            #region == アラーム ==

                            bool isPlayed = false;

                            // カスタムアラーム
                            if (PairXrpJpy.AlarmPlus > 0)
                            {
                                if (tick.LTP >= PairXrpJpy.AlarmPlus)
                                {
                                    PairXrpJpy.HighLowInfoTextColorFlag = true;
                                    PairXrpJpy.HighLowInfoText = PairXrpJpy.PairString + " ⇑⇑⇑　高値アラーム ";

                                    ShowBalloonEventArgs ag = new ShowBalloonEventArgs
                                    {
                                        Title = PairXrpJpy.PairString + " 高値アラーム",
                                        Text = PairXrpJpy.AlarmPlus.ToString("#,0") + " に達しました。"
                                    };
                                    // バルーン表示
                                    ShowBalloon?.Invoke(this, ag);
                                    // クリア
                                    PairXrpJpy.AlarmPlus = 0;

                                }
                            }

                            if (PairXrpJpy.AlarmMinus > 0)
                            {
                                if (tick.LTP <= PairXrpJpy.AlarmMinus)
                                {
                                    PairXrpJpy.HighLowInfoTextColorFlag = false;
                                    PairXrpJpy.HighLowInfoText = PairBtcJpy.PairString + " ⇓⇓⇓　安値アラーム ";

                                    ShowBalloonEventArgs ag = new ShowBalloonEventArgs
                                    {
                                        Title = PairXrpJpy.PairString + " 安値アラーム",
                                        Text = PairXrpJpy.AlarmMinus.ToString("#,0") + " に達しました。"
                                    };
                                    // バルーン表示
                                    ShowBalloon?.Invoke(this, ag);
                                    // クリア
                                    PairXrpJpy.AlarmMinus = 0;

                                }
                            }

                            // 起動後最高値
                            if ((tick.LTP >= PairXrpJpy.HighestPrice) && (prevLtp != tick.LTP))
                            {
                                if ((PairXrpJpy.TickHistories.Count > waitTime) && ((PairXrpJpy.BasePrice + 0.05M) < tick.LTP))
                                {
                                    if (PairXrpJpy.PlaySoundHighest)
                                        PairXrpJpy.HighestPriceAlart = true;

                                    if ((isPlayed == false) && (PairXrpJpy.PlaySoundHighest == true))
                                    {
                                        PairXrpJpy.HighLowInfoTextColorFlag = true;
                                        PairXrpJpy.HighLowInfoText = "";
                                        PairXrpJpy.HighLowInfoText = PairXrpJpy.PairString + " ⇑⇑⇑　起動後最高値 ";

                                        if (PlaySound)
                                        {
                                            SystemSounds.Hand.Play();
                                            isPlayed = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                PairXrpJpy.HighestPriceAlart = false;
                            }

                            // 起動後最安値
                            if ((tick.LTP <= PairXrpJpy.LowestPrice) && (prevLtp != tick.LTP))
                            {
                                if ((PairXrpJpy.TickHistories.Count > waitTime) && ((PairXrpJpy.BasePrice - 0.05M) > tick.LTP))
                                {
                                    if (PairXrpJpy.PlaySoundLowest)
                                        PairXrpJpy.LowestPriceAlart = true;

                                    if ((isPlayed == false) && (PairXrpJpy.PlaySoundLowest == true))
                                    {
                                        PairXrpJpy.HighLowInfoTextColorFlag = false;
                                        PairXrpJpy.HighLowInfoText = "";
                                        PairXrpJpy.HighLowInfoText = PairXrpJpy.PairString + " ⇓⇓⇓　起動後最安値 ";

                                        if (PlaySound)
                                        {
                                            SystemSounds.Beep.Play();
                                            isPlayed = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                PairXrpJpy.LowestPriceAlart = false;
                            }

                            // 過去24時間最高値
                            if ((tick.LTP >= PairXrpJpy.HighestIn24Price) && (prevLtp != tick.LTP) && (PairXrpJpy.TickHistories.Count > waitTime))
                            {
                                if (PairXrpJpy.PlaySoundHighest24h)
                                    PairXrpJpy.HighestIn24PriceAlart = true;

                                if ((isPlayed == false) && (PairXrpJpy.PlaySoundHighest24h == true))
                                {
                                    PairXrpJpy.HighLowInfoTextColorFlag = true;
                                    PairXrpJpy.HighLowInfoText = "";
                                    PairXrpJpy.HighLowInfoText = PairXrpJpy.PairString + " ⇑⇑⇑⇑⇑⇑　24時間最高値 ";

                                    if (PlaySound)
                                    {
                                        SystemSounds.Hand.Play();
                                        isPlayed = true;
                                    }
                                }
                            }
                            else
                            {
                                PairXrpJpy.HighestIn24PriceAlart = false;
                            }

                            // 過去24時間最安値
                            if ((tick.LTP <= PairXrpJpy.LowestIn24Price) && (prevLtp != tick.LTP) && (PairXrpJpy.TickHistories.Count > waitTime))
                            {

                                if (PairXrpJpy.PlaySoundLowest24h)
                                    PairXrpJpy.LowestIn24PriceAlart = true;

                                if ((isPlayed == false) && (PairXrpJpy.PlaySoundLowest24h == true))
                                {
                                    PairXrpJpy.HighLowInfoTextColorFlag = false;
                                    PairXrpJpy.HighLowInfoText = "";
                                    PairXrpJpy.HighLowInfoText = PairXrpJpy.PairString + " ⇓⇓⇓⇓⇓⇓　24時間最安値 ";

                                    if (PlaySound)
                                    {
                                        SystemSounds.Hand.Play();
                                        isPlayed = true;
                                    }
                                }
                            }
                            else
                            {
                                PairXrpJpy.LowestIn24PriceAlart = false;
                            }

                            #endregion

                            // 省エネモードでなかったら
                            if ((MinMode == false) && (pair == CurrentPair))
                            {
                                // 最新取引価格のラインを更新
                                if (ChartAxisYXrpJpy != null)
                                {
                                    if (ChartAxisYXrpJpy[0].Sections.Count > 0)
                                    {
                                        ChartAxisYXrpJpy[0].Sections[0].Value = (double)tick.LTP;
                                    }
                                }

                                // 最新のロウソク足を更新する。＞＞＞重すぎ。負荷掛かり過ぎなので止め。
                                /*
                                if (ChartSeriesBtcJpy[0].Values != null)
                                {
                                    int c = ChartSeriesBtcJpy[0].Values.Count;

                                    if (c > 0)
                                    {
                                        double l = ((OhlcPoint)ChartSeriesBtcJpy[0].Values[c - 1]).Low;
                                        double h = ((OhlcPoint)ChartSeriesBtcJpy[0].Values[c - 1]).High;

                                        if (Application.Current == null) return;
                                        Application.Current.Dispatcher.Invoke(() =>
                                        {

                                            ((OhlcPoint)ChartSeriesBtcJpy[0].Values[c - 1]).Close = (double)tick.LTP;

                                            if (l > (double)tick.LTP)
                                            {
                                                ((OhlcPoint)ChartSeriesBtcJpy[0].Values[c - 1]).Low = (double)tick.LTP;
                                            }

                                            if (h < (double)tick.LTP)
                                            {
                                                ((OhlcPoint)ChartSeriesBtcJpy[0].Values[c - 1]).High = (double)tick.LTP;
                                            }

                                        });

                                    }
                                }
                                */
                            }

                        }
                        else if (pair == Pairs.eth_btc)
                        {

                            // 一旦前の値を保存
                            var prevLtp = PairEthBtc.Ltp;

                            // 最新の価格をセット
                            PairEthBtc.Ltp = tick.LTP;
                            PairEthBtc.Bid = tick.Bid;
                            PairEthBtc.Ask = tick.Ask;
                            PairEthBtc.TickTimeStamp = tick.TimeStamp;

                            PairEthBtc.LowestIn24Price = tick.Low;
                            PairEthBtc.HighestIn24Price = tick.High;

                            // 起動時価格セット
                            if (PairEthBtc.BasePrice == 0) PairEthBtc.BasePrice = tick.LTP;

                            // 最安値登録
                            if (PairEthBtc.LowestPrice == 0)
                            {
                                PairEthBtc.LowestPrice = tick.LTP;
                            }
                            if (tick.LTP < PairEthBtc.LowestPrice)
                            {
                                PairEthBtc.LowestPrice = tick.LTP;
                            }

                            // 最高値登録
                            if (PairEthBtc.HighestPrice == 0)
                            {
                                PairEthBtc.HighestPrice = tick.LTP;
                            }
                            if (tick.LTP > PairEthBtc.HighestPrice)
                            {
                                //SystemSounds.Asterisk.Play();
                                PairEthBtc.HighestPrice = tick.LTP;
                            }

                            #region == チック履歴 ==

                            TickHistory aym = new TickHistory();
                            aym.Price = tick.LTP;
                            aym.TimeAt = tick.TimeStamp;
                            if (PairEthBtc.TickHistories.Count > 0)
                            {
                                if (PairEthBtc.TickHistories[0].Price > aym.Price)
                                {
                                    //aym.TickHistoryPriceColor = _priceUpColor;
                                    aym.TickHistoryPriceUp = true;
                                    PairEthBtc.TickHistories.Insert(0, aym);

                                }
                                else if (PairEthBtc.TickHistories[0].Price < aym.Price)
                                {
                                    //aym.TickHistoryPriceColor = _priceDownColor;
                                    aym.TickHistoryPriceUp = false;
                                    PairEthBtc.TickHistories.Insert(0, aym);
                                }
                                else
                                {
                                    //aym.TickHistoryPriceColor = Colors.Gainsboro;
                                    PairEthBtc.TickHistories.Insert(0, aym);
                                }
                            }
                            else
                            {
                                //aym.TickHistoryPriceColor = Colors.Gainsboro;
                                PairEthBtc.TickHistories.Insert(0, aym);
                            }

                            // limit the number of the list.
                            if (PairEthBtc.TickHistories.Count > 60)
                            {
                                PairEthBtc.TickHistories.RemoveAt(60);
                            }

                            // 60(1分)の平均値を求める
                            decimal aSum = 0;
                            int c = 0;
                            if (PairEthBtc.TickHistories.Count > 0)
                            {

                                if (PairEthBtc.TickHistories.Count > 60)
                                {
                                    c = 59;
                                }
                                else
                                {
                                    c = PairEthBtc.TickHistories.Count - 1;
                                }

                                if (c == 0)
                                {
                                    PairEthBtc.AveragePrice = PairEthBtc.TickHistories[0].Price;
                                }
                                else
                                {
                                    for (int i = 0; i < c; i++)
                                    {
                                        aSum = aSum + PairEthBtc.TickHistories[i].Price;
                                    }
                                    PairEthBtc.AveragePrice = aSum / c;
                                }

                            }
                            else if (PairEthBtc.TickHistories.Count == 1)
                            {
                                PairEthBtc.AveragePrice = PairEthBtc.TickHistories[0].Price;
                            }

                            #endregion

                            #region == アラーム ==

                            bool isPlayed = false;

                            // カスタムアラーム
                            if (PairEthBtc.AlarmPlus > 0)
                            {
                                if (tick.LTP >= PairEthBtc.AlarmPlus)
                                {
                                    PairEthBtc.HighLowInfoTextColorFlag = true;
                                    PairEthBtc.HighLowInfoText = PairEthBtc.PairString + " ⇑⇑⇑　高値アラーム ";

                                    ShowBalloonEventArgs ag = new ShowBalloonEventArgs
                                    {
                                        Title = PairEthBtc.PairString + " 高値アラーム",
                                        Text = PairEthBtc.AlarmPlus.ToString("#,0") + " に達しました。"
                                    };
                                    // バルーン表示
                                    ShowBalloon?.Invoke(this, ag);
                                    // クリア
                                    PairEthBtc.AlarmPlus = 0;

                                }
                            }

                            if (PairEthBtc.AlarmMinus > 0)
                            {
                                if (tick.LTP <= PairEthBtc.AlarmMinus)
                                {
                                    PairEthBtc.HighLowInfoTextColorFlag = false;
                                    PairEthBtc.HighLowInfoText = PairEthBtc.PairString + " ⇓⇓⇓　安値アラーム ";

                                    ShowBalloonEventArgs ag = new ShowBalloonEventArgs
                                    {
                                        Title = PairEthBtc.PairString + " 安値アラーム",
                                        Text = PairEthBtc.AlarmMinus.ToString("#,0") + " に達しました。"
                                    };
                                    // バルーン表示
                                    ShowBalloon?.Invoke(this, ag);
                                    // クリア
                                    PairEthBtc.AlarmMinus = 0;

                                }
                            }

                            // 起動後最高値
                            if ((tick.LTP >= PairEthBtc.HighestPrice) && (prevLtp != tick.LTP))
                            {
                                if ((PairEthBtc.TickHistories.Count > waitTime) && ((PairEthBtc.BasePrice + 0.000001M) < tick.LTP))
                                {

                                    if (PairEthBtc.PlaySoundHighest)
                                        PairEthBtc.HighestPriceAlart = true;

                                    if ((isPlayed == false) && (PairEthBtc.PlaySoundHighest == true))
                                    {
                                        PairEthBtc.HighLowInfoTextColorFlag = true;
                                        PairEthBtc.HighLowInfoText = "";
                                        PairEthBtc.HighLowInfoText = PairEthBtc.PairString + " ⇑⇑⇑　起動後最高値 ";

                                        if (PlaySound)
                                        {
                                            SystemSounds.Hand.Play();
                                            isPlayed = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                PairEthBtc.HighestPriceAlart = false;
                            }

                            // 起動後最安値
                            if ((tick.LTP <= PairEthBtc.LowestPrice) && (prevLtp != tick.LTP))
                            {
                                if ((PairEthBtc.TickHistories.Count > waitTime) && ((PairEthBtc.BasePrice - 0.000001M) > tick.LTP))
                                {

                                    if (PairEthBtc.PlaySoundLowest)
                                        PairEthBtc.LowestPriceAlart = true;

                                    if ((isPlayed == false) && (PairEthBtc.PlaySoundLowest == true))
                                    {
                                        PairEthBtc.HighLowInfoTextColorFlag = false;
                                        PairEthBtc.HighLowInfoText = "";
                                        PairEthBtc.HighLowInfoText = PairEthBtc.PairString + "⇓⇓⇓　起動後最安値 ";

                                        if (PlaySound)
                                        {
                                            SystemSounds.Beep.Play();
                                            isPlayed = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                PairEthBtc.LowestPriceAlart = false;
                            }

                            // 過去24時間最高値
                            if ((tick.LTP >= PairEthBtc.HighestIn24Price) && (prevLtp != tick.LTP) && (PairEthBtc.TickHistories.Count > waitTime))
                            {

                                if (PairEthBtc.PlaySoundHighest24h)
                                    PairEthBtc.HighestIn24PriceAlart = true;

                                if ((isPlayed == false) && (PairEthBtc.PlaySoundHighest24h == true))
                                {
                                    PairEthBtc.HighLowInfoTextColorFlag = true;
                                    PairEthBtc.HighLowInfoText = "";
                                    PairEthBtc.HighLowInfoText = PairEthBtc.PairString + " ⇑⇑⇑⇑⇑⇑　24時間最高値 ";

                                    if (PlaySound)
                                    {
                                        SystemSounds.Hand.Play();
                                        isPlayed = true;
                                    }
                                }
                            }
                            else
                            {
                                PairEthBtc.HighestIn24PriceAlart = false;
                            }

                            // 過去24時間最安値
                            if ((tick.LTP <= PairEthBtc.LowestIn24Price) && (prevLtp != tick.LTP) && (PairEthBtc.TickHistories.Count > waitTime))
                            {

                                if (PairEthBtc.PlaySoundLowest24h)
                                    PairEthBtc.LowestIn24PriceAlart = true;

                                if ((isPlayed == false) && (PairEthBtc.PlaySoundLowest24h == true))
                                {
                                    PairEthBtc.HighLowInfoTextColorFlag = false;
                                    PairEthBtc.HighLowInfoText = "";
                                    PairEthBtc.HighLowInfoText = PairEthBtc.PairString + " ⇓⇓⇓⇓⇓⇓　24時間最安値 ";

                                    if (PlaySound)
                                    {
                                        SystemSounds.Hand.Play();
                                        isPlayed = true;
                                    }
                                }
                            }
                            else
                            {
                                PairEthBtc.LowestIn24PriceAlart = false;
                            }

                            #endregion

                            // 省エネモードでなかったら
                            if ((MinMode == false) && (pair == CurrentPair))
                            {
                                // 最新取引価格のラインを更新
                                if (ChartAxisYEthBtc != null)
                                {
                                    if (ChartAxisYEthBtc[0].Sections.Count > 0)
                                    {
                                        ChartAxisYEthBtc[0].Sections[0].Value = (double)tick.LTP;
                                    }
                                }

                                // 最新のロウソク足を更新する。＞＞＞重すぎ。負荷掛かり過ぎなので止め。
                                /*
                                if (ChartSeriesEthBtc[0].Values != null)
                                {
                                    int c = ChartSeriesEthBtc[0].Values.Count;

                                    if (c > 0)
                                    {
                                        double l = ((OhlcPoint)ChartSeriesEthBtc[0].Values[c - 1]).Low;
                                        double h = ((OhlcPoint)ChartSeriesEthBtc[0].Values[c - 1]).High;

                                        if (Application.Current == null) return;
                                        Application.Current.Dispatcher.Invoke(() =>
                                        {

                                            ((OhlcPoint)ChartSeriesEthBtc[0].Values[c - 1]).Close = (double)tick.LTP;

                                            if (l > (double)tick.LTP)
                                            {
                                                ((OhlcPoint)ChartSeriesEthBtc[0].Values[c - 1]).Low = (double)tick.LTP;
                                            }

                                            if (h < (double)tick.LTP)
                                            {
                                                ((OhlcPoint)ChartSeriesEthBtc[0].Values[c - 1]).High = (double)tick.LTP;
                                            }

                                        });

                                    }
                                }
                                */
                            }

                        }
                        else if (pair == Pairs.mona_jpy)
                        {

                            // 一旦前の値を保存
                            var prevLtp = PairMonaJpy.Ltp;

                            // 最新の価格をセット
                            PairMonaJpy.Ltp = tick.LTP;
                            PairMonaJpy.Bid = tick.Bid;
                            PairMonaJpy.Ask = tick.Ask;
                            PairMonaJpy.TickTimeStamp = tick.TimeStamp;

                            PairMonaJpy.LowestIn24Price = tick.Low;
                            PairMonaJpy.HighestIn24Price = tick.High;

                            // 起動時価格セット
                            if (PairMonaJpy.BasePrice == 0) PairMonaJpy.BasePrice = tick.LTP;

                            // 最安値登録
                            if (PairMonaJpy.LowestPrice == 0)
                            {
                                PairMonaJpy.LowestPrice = tick.LTP;
                            }
                            if (tick.LTP < PairMonaJpy.LowestPrice)
                            {
                                //SystemSounds.Beep.Play();
                                PairMonaJpy.LowestPrice = tick.LTP;
                            }

                            // 最高値登録
                            if (PairMonaJpy.HighestPrice == 0)
                            {
                                PairMonaJpy.HighestPrice = tick.LTP;
                            }
                            if (tick.LTP > PairMonaJpy.HighestPrice)
                            {
                                //SystemSounds.Asterisk.Play();
                                PairMonaJpy.HighestPrice = tick.LTP;
                            }

                            #region == チック履歴 ==

                            TickHistory aym = new TickHistory();
                            aym.Price = tick.LTP;
                            aym.TimeAt = tick.TimeStamp;
                            if (PairMonaJpy.TickHistories.Count > 0)
                            {
                                if (PairMonaJpy.TickHistories[0].Price > aym.Price)
                                {
                                    //aym.TickHistoryPriceColor = _priceUpColor;
                                    aym.TickHistoryPriceUp = true;
                                    PairMonaJpy.TickHistories.Insert(0, aym);

                                }
                                else if (PairMonaJpy.TickHistories[0].Price < aym.Price)
                                {
                                    //aym.TickHistoryPriceColor = _priceDownColor;
                                    aym.TickHistoryPriceUp = false;
                                    PairMonaJpy.TickHistories.Insert(0, aym);
                                }
                                else
                                {
                                    //aym.TickHistoryPriceColor = Colors.Gainsboro;
                                    PairMonaJpy.TickHistories.Insert(0, aym);
                                }
                            }
                            else
                            {
                                //aym.TickHistoryPriceColor = Colors.Gainsboro;
                                PairMonaJpy.TickHistories.Insert(0, aym);
                            }

                            // limit the number of the list.
                            if (PairMonaJpy.TickHistories.Count > 60)
                            {
                                PairMonaJpy.TickHistories.RemoveAt(60);
                            }

                            // 60(1分)の平均値を求める
                            decimal aSum = 0;
                            int c = 0;
                            if (PairMonaJpy.TickHistories.Count > 0)
                            {

                                if (PairMonaJpy.TickHistories.Count > 60)
                                {
                                    c = 59;
                                }
                                else
                                {
                                    c = PairMonaJpy.TickHistories.Count - 1;
                                }

                                if (c == 0)
                                {
                                    PairMonaJpy.AveragePrice = PairMonaJpy.TickHistories[0].Price;
                                }
                                else
                                {
                                    for (int i = 0; i < c; i++)
                                    {
                                        aSum = aSum + PairMonaJpy.TickHistories[i].Price;
                                    }
                                    PairMonaJpy.AveragePrice = aSum / c;
                                }

                            }
                            else if (PairMonaJpy.TickHistories.Count == 1)
                            {
                                PairMonaJpy.AveragePrice = PairMonaJpy.TickHistories[0].Price;
                            }

                            #endregion

                            #region == アラーム ==

                            bool isPlayed = false;

                            // カスタムアラーム
                            if (PairMonaJpy.AlarmPlus > 0)
                            {
                                if (tick.LTP >= PairMonaJpy.AlarmPlus)
                                {
                                    PairMonaJpy.HighLowInfoTextColorFlag = true;
                                    PairMonaJpy.HighLowInfoText = PairMonaJpy.PairString + " ⇑⇑⇑　高値アラーム ";

                                    ShowBalloonEventArgs ag = new ShowBalloonEventArgs
                                    {
                                        Title = PairMonaJpy.PairString + " 高値アラーム",
                                        Text = PairMonaJpy.AlarmPlus.ToString("#,0") + " に達しました。"
                                    };
                                    // バルーン表示
                                    ShowBalloon?.Invoke(this, ag);
                                    // クリア
                                    PairMonaJpy.AlarmPlus = 0;

                                }
                            }

                            if (PairMonaJpy.AlarmMinus > 0)
                            {
                                if (tick.LTP <= PairMonaJpy.AlarmMinus)
                                {
                                    PairMonaJpy.HighLowInfoTextColorFlag = false;
                                    PairMonaJpy.HighLowInfoText = PairMonaJpy.PairString + " ⇓⇓⇓　安値アラーム ";

                                    ShowBalloonEventArgs ag = new ShowBalloonEventArgs
                                    {
                                        Title = PairMonaJpy.PairString + " 安値アラーム",
                                        Text = PairMonaJpy.AlarmMinus.ToString("#,0") + " に達しました。"
                                    };
                                    // バルーン表示
                                    ShowBalloon?.Invoke(this, ag);
                                    // クリア
                                    PairMonaJpy.AlarmMinus = 0;

                                }
                            }

                            // 起動後最高値
                            if ((tick.LTP >= PairMonaJpy.HighestPrice) && (prevLtp != tick.LTP))
                            {
                                if ((PairMonaJpy.TickHistories.Count > waitTime) && ((PairMonaJpy.BasePrice + 0.1M) < tick.LTP))
                                {
                                    if (PairMonaJpy.PlaySoundHighest)
                                        PairMonaJpy.HighestPriceAlart = true;

                                    if ((isPlayed == false) && (PairMonaJpy.PlaySoundHighest == true))
                                    {
                                        PairMonaJpy.HighLowInfoTextColorFlag = true;
                                        PairMonaJpy.HighLowInfoText = "";
                                        PairMonaJpy.HighLowInfoText = PairMonaJpy.PairString + " ⇑⇑⇑　起動後最高値 ";

                                        if (PlaySound)
                                        {
                                            SystemSounds.Hand.Play();
                                            isPlayed = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                PairMonaJpy.HighestPriceAlart = false;
                            }

                            // 起動後最安値
                            if ((tick.LTP <= PairMonaJpy.LowestPrice) && (prevLtp != tick.LTP))
                            {
                                if ((PairMonaJpy.TickHistories.Count > waitTime) && ((PairMonaJpy.BasePrice - 0.1M) > tick.LTP))
                                {

                                    if (PairMonaJpy.PlaySoundLowest)
                                        PairMonaJpy.LowestPriceAlart = true;

                                    if ((isPlayed == false) && (PairMonaJpy.PlaySoundLowest == true))
                                    {
                                        PairMonaJpy.HighLowInfoTextColorFlag = false;
                                        PairMonaJpy.HighLowInfoText = "";
                                        PairMonaJpy.HighLowInfoText = PairMonaJpy.PairString + " ⇓⇓⇓　起動後最安値 ";

                                        if (PlaySound)
                                        {
                                            SystemSounds.Beep.Play();
                                            isPlayed = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                PairMonaJpy.LowestPriceAlart = false;
                            }

                            // 過去24時間最高値
                            if ((tick.LTP >= PairMonaJpy.HighestIn24Price) && (prevLtp != tick.LTP) && (PairMonaJpy.TickHistories.Count > waitTime))
                            {
                                if (PairMonaJpy.PlaySoundHighest24h)
                                    PairMonaJpy.HighestIn24PriceAlart = true;

                                if ((isPlayed == false) && (PairMonaJpy.PlaySoundHighest24h == true))
                                {
                                    PairMonaJpy.HighLowInfoTextColorFlag = true;
                                    PairMonaJpy.HighLowInfoText = "";
                                    PairMonaJpy.HighLowInfoText = PairMonaJpy.PairString + " ⇑⇑⇑⇑⇑⇑　24時間最高値 ";

                                    if (PlaySound)
                                    {
                                        SystemSounds.Hand.Play();
                                        isPlayed = true;
                                    }
                                }
                            }
                            else
                            {
                                PairMonaJpy.HighestIn24PriceAlart = false;
                            }

                            // 過去24時間最安値
                            if ((tick.LTP <= PairMonaJpy.LowestIn24Price) && (prevLtp != tick.LTP) && (PairMonaJpy.TickHistories.Count > waitTime))
                            {

                                if (PairMonaJpy.PlaySoundLowest24h)
                                    PairMonaJpy.LowestIn24PriceAlart = true;

                                if ((isPlayed == false) && (PairMonaJpy.PlaySoundLowest24h == true))
                                {
                                    PairMonaJpy.HighLowInfoTextColorFlag = false;
                                    PairMonaJpy.HighLowInfoText = "";
                                    PairMonaJpy.HighLowInfoText = PairMonaJpy.PairString + " ⇓⇓⇓⇓⇓⇓　24時間最安値 ";

                                    if (PlaySound)
                                    {
                                        SystemSounds.Hand.Play();
                                        isPlayed = true;
                                    }
                                }
                            }
                            else
                            {
                                PairMonaJpy.LowestIn24PriceAlart = false;
                            }

                            #endregion

                            // 省エネモードでなかったら
                            if ((MinMode == false) && (pair == CurrentPair))
                            {
                                // 最新取引価格のラインを更新
                                if (ChartAxisYMonaJpy != null)
                                {
                                    if (ChartAxisYMonaJpy[0].Sections.Count > 0)
                                    {
                                        ChartAxisYMonaJpy[0].Sections[0].Value = (double)tick.LTP;
                                    }
                                }

                                // 最新のロウソク足を更新する。＞＞＞重すぎ。負荷掛かり過ぎなので止め。
                                /*
                                if (ChartSeriesMonaJpy[0].Values != null)
                                {
                                    int c = ChartSeriesMonaJpy[0].Values.Count;

                                    if (c > 0)
                                    {
                                        double l = ((OhlcPoint)ChartSeriesMonaJpy[0].Values[c - 1]).Low;
                                        double h = ((OhlcPoint)ChartSeriesMonaJpy[0].Values[c - 1]).High;

                                        if (Application.Current == null) return;
                                        Application.Current.Dispatcher.Invoke(() =>
                                        {

                                            ((OhlcPoint)ChartSeriesMonaJpy[0].Values[c - 1]).Close = (double)tick.LTP;

                                            if (l > (double)tick.LTP)
                                            {
                                                ((OhlcPoint)ChartSeriesMonaJpy[0].Values[c - 1]).Low = (double)tick.LTP;
                                            }

                                            if (h < (double)tick.LTP)
                                            {
                                                ((OhlcPoint)ChartSeriesMonaJpy[0].Values[c - 1]).High = (double)tick.LTP;
                                            }

                                        });

                                    }
                                }
                                */
                            }

                        }
                        else if (pair == Pairs.mona_btc)
                        {
                            //
                        }
                        else if (pair == Pairs.ltc_btc)
                        {

                            // 一旦前の値を保存
                            var prevLtp = PairLtcBtc.Ltp;

                            // 最新の価格をセット
                            PairLtcBtc.Ltp = tick.LTP;
                            PairLtcBtc.Bid = tick.Bid;
                            PairLtcBtc.Ask = tick.Ask;
                            PairLtcBtc.TickTimeStamp = tick.TimeStamp;

                            PairLtcBtc.LowestIn24Price = tick.Low;
                            PairLtcBtc.HighestIn24Price = tick.High;

                            // 起動時価格セット
                            if (PairLtcBtc.BasePrice == 0) PairLtcBtc.BasePrice = tick.LTP;

                            // 最安値登録
                            if (PairLtcBtc.LowestPrice == 0)
                            {
                                PairLtcBtc.LowestPrice = tick.LTP;
                            }
                            if (tick.LTP < PairLtcBtc.LowestPrice)
                            {
                                //SystemSounds.Beep.Play();
                                PairLtcBtc.LowestPrice = tick.LTP;
                            }

                            // 最高値登録
                            if (PairLtcBtc.HighestPrice == 0)
                            {
                                PairLtcBtc.HighestPrice = tick.LTP;
                            }
                            if (tick.LTP > PairLtcBtc.HighestPrice)
                            {
                                //SystemSounds.Asterisk.Play();
                                PairLtcBtc.HighestPrice = tick.LTP;
                            }

                            #region == チック履歴 ==

                            TickHistory aym = new TickHistory();
                            aym.Price = tick.LTP;
                            aym.TimeAt = tick.TimeStamp;
                            if (PairLtcBtc.TickHistories.Count > 0)
                            {
                                if (PairLtcBtc.TickHistories[0].Price > aym.Price)
                                {
                                    //aym.TickHistoryPriceColor = _priceUpColor;
                                    aym.TickHistoryPriceUp = true;
                                    PairLtcBtc.TickHistories.Insert(0, aym);

                                }
                                else if (PairLtcBtc.TickHistories[0].Price < aym.Price)
                                {
                                    //aym.TickHistoryPriceColor = _priceDownColor;
                                    aym.TickHistoryPriceUp = false;
                                    PairLtcBtc.TickHistories.Insert(0, aym);
                                }
                                else
                                {
                                    //aym.TickHistoryPriceColor = Colors.Gainsboro;
                                    PairLtcBtc.TickHistories.Insert(0, aym);
                                }
                            }
                            else
                            {
                                //aym.TickHistoryPriceColor = Colors.Gainsboro;
                                PairLtcBtc.TickHistories.Insert(0, aym);
                            }

                            // limit the number of the list.
                            if (PairLtcBtc.TickHistories.Count > 60)
                            {
                                PairLtcBtc.TickHistories.RemoveAt(60);
                            }

                            // 60(1分)の平均値を求める
                            decimal aSum = 0;
                            int c = 0;
                            if (PairLtcBtc.TickHistories.Count > 0)
                            {

                                if (PairLtcBtc.TickHistories.Count > 60)
                                {
                                    c = 59;
                                }
                                else
                                {
                                    c = PairLtcBtc.TickHistories.Count - 1;
                                }

                                if (c == 0)
                                {
                                    PairLtcBtc.AveragePrice = PairLtcBtc.TickHistories[0].Price;
                                }
                                else
                                {
                                    for (int i = 0; i < c; i++)
                                    {
                                        aSum = aSum + PairLtcBtc.TickHistories[i].Price;
                                    }
                                    PairLtcBtc.AveragePrice = aSum / c;
                                }

                            }
                            else if (PairLtcBtc.TickHistories.Count == 1)
                            {
                                PairLtcBtc.AveragePrice = PairLtcBtc.TickHistories[0].Price;
                            }

                            #endregion

                            #region == アラーム ==

                            bool isPlayed = false;

                            // カスタムアラーム
                            if (PairLtcBtc.AlarmPlus > 0)
                            {
                                if (tick.LTP >= PairLtcBtc.AlarmPlus)
                                {
                                    PairLtcBtc.HighLowInfoTextColorFlag = true;
                                    PairLtcBtc.HighLowInfoText = PairLtcBtc.PairString + " ⇑⇑⇑　高値アラーム ";

                                    ShowBalloonEventArgs ag = new ShowBalloonEventArgs
                                    {
                                        Title = PairLtcBtc.PairString + " 高値アラーム",
                                        Text = PairLtcBtc.AlarmPlus.ToString("#,0") + " に達しました。"
                                    };
                                    // バルーン表示
                                    ShowBalloon?.Invoke(this, ag);
                                    // クリア
                                    PairLtcBtc.AlarmPlus = 0;

                                }
                            }

                            if (PairLtcBtc.AlarmMinus > 0)
                            {
                                if (tick.LTP <= PairLtcBtc.AlarmMinus)
                                {
                                    PairLtcBtc.HighLowInfoTextColorFlag = false;
                                    PairLtcBtc.HighLowInfoText = PairLtcBtc.PairString + " ⇓⇓⇓　安値アラーム ";

                                    ShowBalloonEventArgs ag = new ShowBalloonEventArgs
                                    {
                                        Title = PairLtcBtc.PairString + " 安値アラーム",
                                        Text = PairLtcBtc.AlarmMinus.ToString("#,0") + " に達しました。"
                                    };
                                    // バルーン表示
                                    ShowBalloon?.Invoke(this, ag);
                                    // クリア
                                    PairLtcBtc.AlarmMinus = 0;

                                }
                            }

                            // 起動後最高値
                            if ((tick.LTP >= PairLtcBtc.HighestPrice) && (prevLtp != tick.LTP))
                            {
                                if ((PairLtcBtc.TickHistories.Count > waitTime) && ((PairLtcBtc.BasePrice + 0.000001M) < tick.LTP))
                                {

                                    if (PairLtcBtc.PlaySoundHighest)
                                        PairLtcBtc.HighestPriceAlart = true;

                                    if ((isPlayed == false) && (PairLtcBtc.PlaySoundHighest == true))
                                    {
                                        PairLtcBtc.HighLowInfoTextColorFlag = true;
                                        PairLtcBtc.HighLowInfoText = "";
                                        PairLtcBtc.HighLowInfoText = PairLtcBtc.PairString + " ⇑⇑⇑　起動後最高値 ";

                                        if (PlaySound)
                                        {
                                            SystemSounds.Hand.Play();
                                            isPlayed = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                PairLtcBtc.HighestPriceAlart = false;
                            }
                            // 起動後最安値
                            if ((tick.LTP <= PairLtcBtc.LowestPrice) && (prevLtp != tick.LTP))
                            {
                                if ((PairLtcBtc.TickHistories.Count > waitTime) && ((PairLtcBtc.BasePrice - 0.000001M) > tick.LTP))
                                {

                                    if (PairLtcBtc.PlaySoundLowest)
                                        PairLtcBtc.LowestPriceAlart = true;

                                    if ((isPlayed == false) && (PairLtcBtc.PlaySoundLowest == true))
                                    {
                                        PairLtcBtc.HighLowInfoTextColorFlag = false;
                                        PairLtcBtc.HighLowInfoText = "";
                                        PairLtcBtc.HighLowInfoText = PairLtcBtc.PairString + " ⇓⇓⇓　起動後最安値 ";

                                        if (PlaySound)
                                        {
                                            SystemSounds.Beep.Play();
                                            isPlayed = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                PairLtcBtc.LowestPriceAlart = false;
                            }

                            // 過去24時間最高値
                            if ((tick.LTP >= PairLtcBtc.HighestIn24Price) && (prevLtp != tick.LTP) && (PairLtcBtc.TickHistories.Count > waitTime))
                            {
                                if (PairLtcBtc.PlaySoundHighest24h)
                                    PairLtcBtc.HighestIn24PriceAlart = true;

                                if ((isPlayed == false) && (PairLtcBtc.PlaySoundHighest24h == true))
                                {
                                    PairLtcBtc.HighLowInfoTextColorFlag = true;
                                    PairLtcBtc.HighLowInfoText = "";
                                    PairLtcBtc.HighLowInfoText = PairLtcBtc.PairString + " ⇑⇑⇑⇑⇑⇑　24時間最高値 ";

                                    if (PlaySound)
                                    {
                                        SystemSounds.Hand.Play();
                                        isPlayed = true;
                                    }
                                }
                            }
                            else
                            {
                                PairLtcBtc.HighestIn24PriceAlart = false;
                            }

                            // 過去24時間最安値
                            if ((tick.LTP <= PairLtcBtc.LowestIn24Price) && (prevLtp != tick.LTP) && (PairLtcBtc.TickHistories.Count > waitTime))
                            {
                                if (PairLtcBtc.PlaySoundLowest24h)
                                    PairLtcBtc.LowestIn24PriceAlart = true;

                                if ((isPlayed == false) && (PairLtcBtc.PlaySoundLowest24h == true))
                                {
                                    PairLtcBtc.HighLowInfoTextColorFlag = false;
                                    PairLtcBtc.HighLowInfoText = "";
                                    PairLtcBtc.HighLowInfoText = PairLtcBtc.PairString + " ⇓⇓⇓⇓⇓⇓　24時間最安値 ";

                                    if (PlaySound)
                                    {
                                        SystemSounds.Hand.Play();
                                        isPlayed = true;
                                    }
                                }
                            }
                            else
                            {
                                PairLtcBtc.LowestIn24PriceAlart = false;
                            }

                            #endregion

                            // 省エネモードでなかったら
                            if ((MinMode == false) && (pair == CurrentPair))
                            {
                                // 最新取引価格のラインを更新
                                if (ChartAxisYLtcBtc != null)
                                {
                                    if (ChartAxisYLtcBtc[0].Sections.Count > 0)
                                    {
                                        ChartAxisYLtcBtc[0].Sections[0].Value = (double)tick.LTP;
                                    }
                                }

                                // 最新のロウソク足を更新する。＞＞＞重すぎ。負荷掛かり過ぎなので止め。
                                /*
                                if (ChartSeriesLtcBtc[0].Values != null)
                                {
                                    int c = ChartSeriesLtcBtc[0].Values.Count;

                                    if (c > 0)
                                    {
                                        double l = ((OhlcPoint)ChartSeriesLtcBtc[0].Values[c - 1]).Low;
                                        double h = ((OhlcPoint)ChartSeriesLtcBtc[0].Values[c - 1]).High;

                                        if (Application.Current == null) return;
                                        Application.Current.Dispatcher.Invoke(() =>
                                        {

                                            ((OhlcPoint)ChartSeriesLtcBtc[0].Values[c - 1]).Close = (double)tick.LTP;

                                            if (l > (double)tick.LTP)
                                            {
                                                ((OhlcPoint)ChartSeriesLtcBtc[0].Values[c - 1]).Low = (double)tick.LTP;
                                            }

                                            if (h < (double)tick.LTP)
                                            {
                                                ((OhlcPoint)ChartSeriesLtcBtc[0].Values[c - 1]).High = (double)tick.LTP;
                                            }

                                        });

                                    }
                                }
                                */
                            }

                        }
                        else if (pair == Pairs.bcc_btc)
                        {
                            //
                        }
                        else if (pair == Pairs.bcc_jpy)
                        {
                            // 一旦前の値を保存
                            var prevLtp = PairBchJpy.Ltp;

                            // 最新の価格をセット
                            PairBchJpy.Ltp = tick.LTP;
                            PairBchJpy.Bid = tick.Bid;
                            PairBchJpy.Ask = tick.Ask;
                            PairBchJpy.TickTimeStamp = tick.TimeStamp;

                            PairBchJpy.LowestIn24Price = tick.Low;
                            PairBchJpy.HighestIn24Price = tick.High;

                            // 起動時価格セット
                            if (PairBchJpy.BasePrice == 0) PairBchJpy.BasePrice = tick.LTP;

                            // 最安値登録
                            if (PairBchJpy.LowestPrice == 0)
                            {
                                PairBchJpy.LowestPrice = tick.LTP;
                            }
                            if (tick.LTP < PairBchJpy.LowestPrice)
                            {
                                //SystemSounds.Beep.Play();
                                PairBchJpy.LowestPrice = tick.LTP;
                            }

                            // 最高値登録
                            if (PairBchJpy.HighestPrice == 0)
                            {
                                PairBchJpy.HighestPrice = tick.LTP;
                            }
                            if (tick.LTP > PairBchJpy.HighestPrice)
                            {
                                //SystemSounds.Asterisk.Play();
                                PairBchJpy.HighestPrice = tick.LTP;
                            }

                            #region == チック履歴 ==

                            TickHistory aym = new TickHistory();
                            aym.Price = tick.LTP;
                            aym.TimeAt = tick.TimeStamp;
                            if (PairBchJpy.TickHistories.Count > 0)
                            {
                                if (PairBchJpy.TickHistories[0].Price > aym.Price)
                                {
                                    //aym.TickHistoryPriceColor = _priceUpColor;
                                    aym.TickHistoryPriceUp = true;
                                    PairBchJpy.TickHistories.Insert(0, aym);

                                }
                                else if (PairBchJpy.TickHistories[0].Price < aym.Price)
                                {
                                    //aym.TickHistoryPriceColor = _priceDownColor;
                                    aym.TickHistoryPriceUp = false;
                                    PairBchJpy.TickHistories.Insert(0, aym);
                                }
                                else
                                {
                                    //aym.TickHistoryPriceColor = Colors.Gainsboro;
                                    PairBchJpy.TickHistories.Insert(0, aym);
                                }
                            }
                            else
                            {
                                //aym.TickHistoryPriceColor = Colors.Gainsboro;
                                PairBchJpy.TickHistories.Insert(0, aym);
                            }

                            // limit the number of the list.
                            if (PairBchJpy.TickHistories.Count > 60)
                            {
                                PairBchJpy.TickHistories.RemoveAt(60);
                            }

                            // 60(1分)の平均値を求める
                            decimal aSum = 0;
                            int c = 0;
                            if (PairBchJpy.TickHistories.Count > 0)
                            {

                                if (PairBchJpy.TickHistories.Count > 60)
                                {
                                    c = 59;
                                }
                                else
                                {
                                    c = PairBchJpy.TickHistories.Count - 1;
                                }

                                if (c == 0)
                                {
                                    PairBchJpy.AveragePrice = PairBchJpy.TickHistories[0].Price;
                                }
                                else
                                {
                                    for (int i = 0; i < c; i++)
                                    {
                                        aSum = aSum + PairBchJpy.TickHistories[i].Price;
                                    }
                                    PairBchJpy.AveragePrice = aSum / c;
                                }

                            }
                            else if (PairLtcBtc.TickHistories.Count == 1)
                            {
                                PairBchJpy.AveragePrice = PairBchJpy.TickHistories[0].Price;
                            }

                            #endregion

                            #region == アラーム ==

                            bool isPlayed = false;

                            // カスタムアラーム
                            if (PairBchJpy.AlarmPlus > 0)
                            {
                                if (tick.LTP >= PairBchJpy.AlarmPlus)
                                {
                                    PairBchJpy.HighLowInfoTextColorFlag = true;
                                    PairBchJpy.HighLowInfoText = PairBchJpy.PairString + " ⇑⇑⇑　高値アラーム ";

                                    ShowBalloonEventArgs ag = new ShowBalloonEventArgs
                                    {
                                        Title = PairBchJpy.PairString + " 高値アラーム",
                                        Text = PairBchJpy.AlarmPlus.ToString("#,0") + " に達しました。"
                                    };
                                    // バルーン表示
                                    ShowBalloon?.Invoke(this, ag);
                                    // クリア
                                    PairBchJpy.AlarmPlus = 0;

                                }
                            }

                            if (PairBchJpy.AlarmMinus > 0)
                            {
                                if (tick.LTP <= PairBchJpy.AlarmMinus)
                                {
                                    PairBchJpy.HighLowInfoTextColorFlag = false;
                                    PairBchJpy.HighLowInfoText = PairBchJpy.PairString + " ⇓⇓⇓　安値アラーム ";

                                    ShowBalloonEventArgs ag = new ShowBalloonEventArgs
                                    {
                                        Title = PairBchJpy.PairString + " 安値アラーム",
                                        Text = PairBchJpy.AlarmMinus.ToString("#,0") + " に達しました。"
                                    };
                                    // バルーン表示
                                    ShowBalloon?.Invoke(this, ag);
                                    // クリア
                                    PairBchJpy.AlarmMinus = 0;

                                }
                            }

                            // 起動後最高値
                            if ((tick.LTP >= PairBchJpy.HighestPrice) && (prevLtp != tick.LTP))
                            {
                                if ((PairBchJpy.TickHistories.Count > waitTime) && ((PairBchJpy.BasePrice + 20M) < tick.LTP))
                                {
                                    if (PairBchJpy.PlaySoundHighest)
                                        PairBchJpy.HighestPriceAlart = true;

                                    if ((isPlayed == false) && (PairBchJpy.PlaySoundHighest == true))
                                    {
                                        PairBchJpy.HighLowInfoTextColorFlag = true;
                                        PairBchJpy.HighLowInfoText = "";
                                        PairBchJpy.HighLowInfoText = PairBchJpy.PairString + " ⇑⇑⇑　起動後最高値 ";

                                        if (PlaySound)
                                        {
                                            SystemSounds.Hand.Play();
                                            isPlayed = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                PairBchJpy.HighestPriceAlart = false;
                            }

                            // 起動後最安値
                            if ((tick.LTP <= PairBchJpy.LowestPrice) && (prevLtp != tick.LTP))
                            {
                                if ((PairBchJpy.TickHistories.Count > waitTime) && ((PairBchJpy.BasePrice - 20M) > tick.LTP))
                                {
                                    if (PairBchJpy.PlaySoundLowest)
                                        PairBchJpy.LowestPriceAlart = true;

                                    if ((isPlayed == false) && (PairBchJpy.PlaySoundLowest == true))
                                    {
                                        PairBchJpy.HighLowInfoTextColorFlag = false;
                                        PairBchJpy.HighLowInfoText = "";
                                        PairBchJpy.HighLowInfoText = PairBchJpy.PairString + " ⇓⇓⇓　起動後最安値 ";

                                        if (PlaySound)
                                        {
                                            SystemSounds.Beep.Play();
                                            isPlayed = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                PairBchJpy.LowestPriceAlart = false;
                            }

                            // 過去24時間最高値
                            if ((tick.LTP >= PairBchJpy.HighestIn24Price) && (prevLtp != tick.LTP) && (PairBchJpy.TickHistories.Count > waitTime))
                            {
                                if (PairBchJpy.PlaySoundHighest24h)
                                    PairBchJpy.HighestIn24PriceAlart = true;

                                if ((isPlayed == false) && (PairBchJpy.PlaySoundHighest24h == true))
                                {
                                    PairBchJpy.HighLowInfoTextColorFlag = true;
                                    PairBchJpy.HighLowInfoText = "";
                                    PairBchJpy.HighLowInfoText = PairBchJpy.PairString + " ⇑⇑⇑⇑⇑⇑　過去24時間最高値 ";

                                    if (PlaySound)
                                    {
                                        SystemSounds.Hand.Play();
                                        isPlayed = true;
                                    }
                                }
                            }
                            else
                            {
                                PairBchJpy.HighestIn24PriceAlart = false;
                            }

                            // 過去24時間最安値
                            if ((tick.LTP <= PairBchJpy.LowestIn24Price) && (prevLtp != tick.LTP) && (PairBchJpy.TickHistories.Count > waitTime))
                            {
                                if (PairBchJpy.PlaySoundLowest24h)
                                    PairBchJpy.LowestIn24PriceAlart = true;

                                if ((isPlayed == false) && (PairBchJpy.PlaySoundLowest24h == true))
                                {
                                    PairBchJpy.HighLowInfoTextColorFlag = false;
                                    PairBchJpy.HighLowInfoText = "";
                                    PairBchJpy.HighLowInfoText = PairBchJpy.PairString + " ⇓⇓⇓⇓⇓⇓　過去24時間最安値 ";

                                    if (PlaySound)
                                    {
                                        SystemSounds.Hand.Play();
                                        isPlayed = true;
                                    }
                                }
                            }
                            else
                            {
                                PairBchJpy.LowestIn24PriceAlart = false;
                            }

                            #endregion

                            // 省エネモードでなかったら
                            if ((MinMode == false) && (pair == CurrentPair))
                            {
                                // 最新取引価格のラインを更新
                                if (ChartAxisYBchJpy != null)
                                {
                                    if (ChartAxisYBchJpy[0].Sections.Count > 0)
                                    {
                                        ChartAxisYBchJpy[0].Sections[0].Value = (double)tick.LTP;
                                    }
                                }

                                // 最新のロウソク足を更新する。＞＞＞重すぎ。負荷掛かり過ぎなので止め。
                                /*
                                if (ChartSeriesBchJpy[0].Values != null)
                                {
                                    int c = ChartSeriesBchJpy[0].Values.Count;

                                    if (c > 0)
                                    {
                                        double l = ((OhlcPoint)ChartSeriesBchJpy[0].Values[c - 1]).Low;
                                        double h = ((OhlcPoint)ChartSeriesBchJpy[0].Values[c - 1]).High;

                                        if (Application.Current == null) return;
                                        Application.Current.Dispatcher.Invoke(() =>
                                        {

                                            ((OhlcPoint)ChartSeriesBchJpy[0].Values[c - 1]).Close = (double)tick.LTP;

                                            if (l > (double)tick.LTP)
                                            {
                                                ((OhlcPoint)ChartSeriesBchJpy[0].Values[c - 1]).Low = (double)tick.LTP;
                                            }

                                            if (h < (double)tick.LTP)
                                            {
                                                ((OhlcPoint)ChartSeriesBchJpy[0].Values[c - 1]).High = (double)tick.LTP;
                                            }

                                        });

                                    }
                                }
                                */
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("■■■■■ TickerTimerAllPairs: Exception1 - " + ex.Message);
                        break;
                    }
                }
                else
                {
                    APIResultTicker = "<<取得失敗>>";
                    System.Diagnostics.Debug.WriteLine("■■■■■ TickerTimerAllPairs: GetTicker returned null");
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

            try
            {
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

                            hoge = mainWindow.Attribute("opacity");
                            if (hoge != null)
                            {
                                WindowOpacity = double.Parse(hoge.Value);
                            }

                            hoge = mainWindow.Attribute("theme");
                            if (hoge != null)
                            {
                                // テーマをセット
                                SetCurrentTheme(hoge.Value);
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

                                SelectedCandleType = CandleTypes.OneMin;
                            }
                        }
                        else
                        {
                            // TODO other candle types

                            SelectedCandleType = CandleTypes.OneMin;
                        }

                    }
                    else
                    {
                        // デフォのチャート、キャンドルタイプ指定
                        SelectedCandleType = CandleTypes.OneMin;
                    }

                    #endregion

                    #region == アラーム音設定 ==

                    var alarmSetting = xdoc.Root.Element("Alarm");
                    if (alarmSetting != null)
                    {
                        var hoge = alarmSetting.Attribute("playSound");
                        if (hoge != null)
                        {
                            if (hoge.Value == "true")
                            {
                                PlaySound = true;
                            }
                            else
                            {
                                PlaySound = false;
                            }
                        }
                    }

                    #endregion

                    #region == 各通貨毎の設定 ==

                    var pairs = xdoc.Root.Element("Pairs");
                    if (pairs != null)
                    {
                        // PairBtcJpy
                        var pair = pairs.Element("BtcJpy");
                        if (pair != null)
                        {
                            var hoge = pair.Attribute("playSoundLowest");
                            if (hoge != null)
                            {
                                if (hoge.Value == "true")
                                {
                                    PairBtcJpy.PlaySoundLowest = true;
                                }
                                else
                                {
                                    PairBtcJpy.PlaySoundLowest = false;
                                }
                            }

                            hoge = pair.Attribute("playSoundHighest");
                            if (hoge != null)
                            {
                                if (hoge.Value == "true")
                                {
                                    PairBtcJpy.PlaySoundHighest = true;
                                }
                                else
                                {
                                    PairBtcJpy.PlaySoundHighest = false;
                                }
                            }

                            hoge = pair.Attribute("playSoundLowest24h");
                            if (hoge != null)
                            {
                                if (hoge.Value == "true")
                                {
                                    PairBtcJpy.PlaySoundLowest24h = true;
                                }
                                else
                                {
                                    PairBtcJpy.PlaySoundLowest24h = false;
                                }
                            }

                            hoge = pair.Attribute("playSoundHighest24h");
                            if (hoge != null)
                            {
                                if (hoge.Value == "true")
                                {
                                    PairBtcJpy.PlaySoundHighest24h = true;
                                }
                                else
                                {
                                    PairBtcJpy.PlaySoundHighest24h = false;
                                }
                            }

                            // 板グルーピング
                            hoge = pair.Attribute("depthGrouping");
                            if (hoge != null)
                            {
                                if (!string.IsNullOrEmpty(hoge.Value))
                                {
                                    try
                                    {
                                        PairBtcJpy.DepthGrouping = Decimal.Parse(hoge.Value);
                                    }
                                    catch
                                    {
                                        PairBtcJpy.DepthGrouping = 0;
                                    }

                                }
                            }

                            // カスタムアラート
                            hoge = pair.Attribute("alarmHigh");
                            if (hoge != null)
                            {
                                if (!string.IsNullOrEmpty(hoge.Value))
                                {
                                    try
                                    {
                                        PairBtcJpy.AlarmPlus = Decimal.Parse(hoge.Value);
                                    }
                                    catch
                                    {
                                        PairBtcJpy.AlarmPlus = 0;
                                    }

                                }
                            }
                            hoge = pair.Attribute("alarmLow");
                            if (hoge != null)
                            {
                                if (!string.IsNullOrEmpty(hoge.Value))
                                {
                                    try
                                    {
                                        PairBtcJpy.AlarmMinus = Decimal.Parse(hoge.Value);
                                    }
                                    catch
                                    {
                                        PairBtcJpy.AlarmMinus = 0;
                                    }

                                }
                            }
                            
                        }

                        // PairXrpJpy
                        pair = pairs.Element("XrpJpy");
                        if (pair != null)
                        {
                            var hoge = pair.Attribute("playSoundLowest");
                            if (hoge != null)
                            {
                                if (hoge.Value == "true")
                                {
                                    PairXrpJpy.PlaySoundLowest = true;
                                }
                                else
                                {
                                    PairXrpJpy.PlaySoundLowest = false;
                                }
                            }

                            hoge = pair.Attribute("playSoundHighest");
                            if (hoge != null)
                            {
                                if (hoge.Value == "true")
                                {
                                    PairXrpJpy.PlaySoundHighest = true;
                                }
                                else
                                {
                                    PairXrpJpy.PlaySoundHighest = false;
                                }
                            }

                            hoge = pair.Attribute("playSoundLowest24h");
                            if (hoge != null)
                            {
                                if (hoge.Value == "true")
                                {
                                    PairXrpJpy.PlaySoundLowest24h = true;
                                }
                                else
                                {
                                    PairXrpJpy.PlaySoundLowest24h = false;
                                }
                            }

                            hoge = pair.Attribute("playSoundHighest24h");
                            if (hoge != null)
                            {
                                if (hoge.Value == "true")
                                {
                                    PairXrpJpy.PlaySoundHighest24h = true;
                                }
                                else
                                {
                                    PairXrpJpy.PlaySoundHighest24h = false;
                                }
                            }

                            // 板グルーピング
                            hoge = pair.Attribute("depthGrouping");
                            if (hoge != null)
                            {
                                if (!string.IsNullOrEmpty(hoge.Value))
                                {
                                    try
                                    {
                                        PairXrpJpy.DepthGrouping = Decimal.Parse(hoge.Value);
                                    }
                                    catch
                                    {
                                        PairXrpJpy.DepthGrouping = 0;
                                    }

                                }
                            }

                            // カスタムアラート
                            hoge = pair.Attribute("alarmHigh");
                            if (hoge != null)
                            {
                                if (!string.IsNullOrEmpty(hoge.Value))
                                {
                                    try
                                    {
                                        PairXrpJpy.AlarmPlus = Decimal.Parse(hoge.Value);
                                    }
                                    catch
                                    {
                                        PairXrpJpy.AlarmPlus = 0;
                                    }

                                }
                            }
                            hoge = pair.Attribute("alarmLow");
                            if (hoge != null)
                            {
                                if (!string.IsNullOrEmpty(hoge.Value))
                                {
                                    try
                                    {
                                        PairXrpJpy.AlarmMinus = Decimal.Parse(hoge.Value);
                                    }
                                    catch
                                    {
                                        PairXrpJpy.AlarmMinus = 0;
                                    }

                                }
                            }
                        }

                        // PairEthBtc
                        pair = pairs.Element("EthBtc");
                        if (pair != null)
                        {
                            var hoge = pair.Attribute("playSoundLowest");
                            if (hoge != null)
                            {
                                if (hoge.Value == "true")
                                {
                                    PairEthBtc.PlaySoundLowest = true;
                                }
                                else
                                {
                                    PairEthBtc.PlaySoundLowest = false;
                                }
                            }

                            hoge = pair.Attribute("playSoundHighest");
                            if (hoge != null)
                            {
                                if (hoge.Value == "true")
                                {
                                    PairEthBtc.PlaySoundHighest = true;
                                }
                                else
                                {
                                    PairEthBtc.PlaySoundHighest = false;
                                }
                            }

                            hoge = pair.Attribute("playSoundLowest24h");
                            if (hoge != null)
                            {
                                if (hoge.Value == "true")
                                {
                                    PairEthBtc.PlaySoundLowest24h = true;
                                }
                                else
                                {
                                    PairEthBtc.PlaySoundLowest24h = false;
                                }
                            }

                            hoge = pair.Attribute("playSoundHighest24h");
                            if (hoge != null)
                            {
                                if (hoge.Value == "true")
                                {
                                    PairEthBtc.PlaySoundHighest24h = true;
                                }
                                else
                                {
                                    PairEthBtc.PlaySoundHighest24h = false;
                                }
                            }

                            // 板グルーピング
                            hoge = pair.Attribute("depthGrouping");
                            if (hoge != null)
                            {
                                if (!string.IsNullOrEmpty(hoge.Value))
                                {
                                    try
                                    {
                                        PairEthBtc.DepthGrouping = Decimal.Parse(hoge.Value);
                                    }
                                    catch
                                    {
                                        PairEthBtc.DepthGrouping = 0;
                                    }

                                }
                            }

                            // カスタムアラート
                            hoge = pair.Attribute("alarmHigh");
                            if (hoge != null)
                            {
                                if (!string.IsNullOrEmpty(hoge.Value))
                                {
                                    try
                                    {
                                        PairEthBtc.AlarmPlus = Decimal.Parse(hoge.Value);
                                    }
                                    catch
                                    {
                                        PairEthBtc.AlarmPlus = 0;
                                    }

                                }
                            }
                            hoge = pair.Attribute("alarmLow");
                            if (hoge != null)
                            {
                                if (!string.IsNullOrEmpty(hoge.Value))
                                {
                                    try
                                    {
                                        PairEthBtc.AlarmMinus = Decimal.Parse(hoge.Value);
                                    }
                                    catch
                                    {
                                        PairEthBtc.AlarmMinus = 0;
                                    }

                                }
                            }
                        }

                        // PairLtcBtc
                        pair = pairs.Element("LtcBtc");
                        if (pair != null)
                        {
                            var hoge = pair.Attribute("playSoundLowest");
                            if (hoge != null)
                            {
                                if (hoge.Value == "true")
                                {
                                    PairLtcBtc.PlaySoundLowest = true;
                                }
                                else
                                {
                                    PairLtcBtc.PlaySoundLowest = false;
                                }
                            }

                            hoge = pair.Attribute("playSoundHighest");
                            if (hoge != null)
                            {
                                if (hoge.Value == "true")
                                {
                                    PairLtcBtc.PlaySoundHighest = true;
                                }
                                else
                                {
                                    PairLtcBtc.PlaySoundHighest = false;
                                }
                            }

                            hoge = pair.Attribute("playSoundLowest24h");
                            if (hoge != null)
                            {
                                if (hoge.Value == "true")
                                {
                                    PairLtcBtc.PlaySoundLowest24h = true;
                                }
                                else
                                {
                                    PairLtcBtc.PlaySoundLowest24h = false;
                                }
                            }

                            hoge = pair.Attribute("playSoundHighest24h");
                            if (hoge != null)
                            {
                                if (hoge.Value == "true")
                                {
                                    PairLtcBtc.PlaySoundHighest24h = true;
                                }
                                else
                                {
                                    PairLtcBtc.PlaySoundHighest24h = false;
                                }
                            }

                            // 板グルーピング
                            hoge = pair.Attribute("depthGrouping");
                            if (hoge != null)
                            {
                                if (!string.IsNullOrEmpty(hoge.Value))
                                {
                                    try
                                    {
                                        PairLtcBtc.DepthGrouping = Decimal.Parse(hoge.Value);
                                    }
                                    catch
                                    {
                                        PairLtcBtc.DepthGrouping = 0;
                                    }

                                }
                            }

                            // カスタムアラート
                            hoge = pair.Attribute("alarmHigh");
                            if (hoge != null)
                            {
                                if (!string.IsNullOrEmpty(hoge.Value))
                                {
                                    try
                                    {
                                        PairLtcBtc.AlarmPlus = Decimal.Parse(hoge.Value);
                                    }
                                    catch
                                    {
                                        PairLtcBtc.AlarmPlus = 0;
                                    }

                                }
                            }
                            hoge = pair.Attribute("alarmLow");
                            if (hoge != null)
                            {
                                if (!string.IsNullOrEmpty(hoge.Value))
                                {
                                    try
                                    {
                                        PairLtcBtc.AlarmMinus = Decimal.Parse(hoge.Value);
                                    }
                                    catch
                                    {
                                        PairLtcBtc.AlarmMinus = 0;
                                    }

                                }
                            }
                        }

                        // PairMonaJpy
                        pair = pairs.Element("MonaJpy");
                        if (pair != null)
                        {
                            var hoge = pair.Attribute("playSoundLowest");
                            if (hoge != null)
                            {
                                if (hoge.Value == "true")
                                {
                                    PairMonaJpy.PlaySoundLowest = true;
                                }
                                else
                                {
                                    PairMonaJpy.PlaySoundLowest = false;
                                }
                            }

                            hoge = pair.Attribute("playSoundHighest");
                            if (hoge != null)
                            {
                                if (hoge.Value == "true")
                                {
                                    PairMonaJpy.PlaySoundHighest = true;
                                }
                                else
                                {
                                    PairMonaJpy.PlaySoundHighest = false;
                                }
                            }

                            hoge = pair.Attribute("playSoundLowest24h");
                            if (hoge != null)
                            {
                                if (hoge.Value == "true")
                                {
                                    PairMonaJpy.PlaySoundLowest24h = true;
                                }
                                else
                                {
                                    PairMonaJpy.PlaySoundLowest24h = false;
                                }
                            }

                            hoge = pair.Attribute("playSoundHighest24h");
                            if (hoge != null)
                            {
                                if (hoge.Value == "true")
                                {
                                    PairMonaJpy.PlaySoundHighest24h = true;
                                }
                                else
                                {
                                    PairMonaJpy.PlaySoundHighest24h = false;
                                }
                            }

                            // 板グルーピング
                            hoge = pair.Attribute("depthGrouping");
                            if (hoge != null)
                            {
                                if (!string.IsNullOrEmpty(hoge.Value))
                                {
                                    try
                                    {
                                        PairMonaJpy.DepthGrouping = Decimal.Parse(hoge.Value);
                                    }
                                    catch
                                    {
                                        PairMonaJpy.DepthGrouping = 0;
                                    }

                                }
                            }

                            // カスタムアラート
                            hoge = pair.Attribute("alarmHigh");
                            if (hoge != null)
                            {
                                if (!string.IsNullOrEmpty(hoge.Value))
                                {
                                    try
                                    {
                                        PairMonaJpy.AlarmPlus = Decimal.Parse(hoge.Value);
                                    }
                                    catch
                                    {
                                        PairMonaJpy.AlarmPlus = 0;
                                    }

                                }
                            }
                            hoge = pair.Attribute("alarmLow");
                            if (hoge != null)
                            {
                                if (!string.IsNullOrEmpty(hoge.Value))
                                {
                                    try
                                    {
                                        PairMonaJpy.AlarmMinus = Decimal.Parse(hoge.Value);
                                    }
                                    catch
                                    {
                                        PairMonaJpy.AlarmMinus = 0;
                                    }

                                }
                            }
                        }

                        // PairBchJpy
                        pair = pairs.Element("BchJpy");
                        if (pair != null)
                        {
                            var hoge = pair.Attribute("playSoundLowest");
                            if (hoge != null)
                            {
                                if (hoge.Value == "true")
                                {
                                    PairBchJpy.PlaySoundLowest = true;
                                }
                                else
                                {
                                    PairBchJpy.PlaySoundLowest = false;
                                }
                            }

                            hoge = pair.Attribute("playSoundHighest");
                            if (hoge != null)
                            {
                                if (hoge.Value == "true")
                                {
                                    PairBchJpy.PlaySoundHighest = true;
                                }
                                else
                                {
                                    PairBchJpy.PlaySoundHighest = false;
                                }
                            }

                            hoge = pair.Attribute("playSoundLowest24h");
                            if (hoge != null)
                            {
                                if (hoge.Value == "true")
                                {
                                    PairBchJpy.PlaySoundLowest24h = true;
                                }
                                else
                                {
                                    PairBchJpy.PlaySoundLowest24h = false;
                                }
                            }

                            hoge = pair.Attribute("playSoundHighest24h");
                            if (hoge != null)
                            {
                                if (hoge.Value == "true")
                                {
                                    PairBchJpy.PlaySoundHighest24h = true;
                                }
                                else
                                {
                                    PairBchJpy.PlaySoundHighest24h = false;
                                }
                            }

                            // 板グルーピング
                            hoge = pair.Attribute("depthGrouping");
                            if (hoge != null)
                            {
                                if (!string.IsNullOrEmpty(hoge.Value))
                                {
                                    try
                                    {
                                        PairBchJpy.DepthGrouping = Decimal.Parse(hoge.Value);
                                    }
                                    catch
                                    {
                                        PairBchJpy.DepthGrouping = 0;
                                    }

                                }
                            }

                            // カスタムアラート
                            hoge = pair.Attribute("alarmHigh");
                            if (hoge != null)
                            {
                                if (!string.IsNullOrEmpty(hoge.Value))
                                {
                                    try
                                    {
                                        PairBchJpy.AlarmPlus = Decimal.Parse(hoge.Value);
                                    }
                                    catch
                                    {
                                        PairBchJpy.AlarmPlus = 0;
                                    }

                                }
                            }
                            hoge = pair.Attribute("alarmLow");
                            if (hoge != null)
                            {
                                if (!string.IsNullOrEmpty(hoge.Value))
                                {
                                    try
                                    {
                                        PairBchJpy.AlarmMinus = Decimal.Parse(hoge.Value);
                                    }
                                    catch
                                    {
                                        PairBchJpy.AlarmMinus = 0;
                                    }

                                }
                            }
                        }

                    }

                    #endregion
                }
                else
                {
                    // デフォのチャート、キャンドルタイプ指定
                    SelectedCandleType = CandleTypes.OneMin;
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                System.Diagnostics.Debug.WriteLine("■■■■■ Error  設定ファイルの保存中 - FileNotFoundException : " + AppConfigFilePath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("■■■■■ Error  設定ファイルの保存中: " + ex + " while opening : " + AppConfigFilePath);
            }

            #endregion

            //SelectedCandleType = で表示できるので、これは不要だが、デフォと同じ場合のみ、手動で表示させる。
            if (SelectedCandleType == CandleTypes.OneMin) // デフォと揃えること。
            {
                Task.Run(async () =>
                {
                    await Task.Run(() => DisplayCharts());

                });
            }

            // チャート更新のタイマー起動
            dispatcherChartTimer.Start();

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

                attrs = doc.CreateAttribute("opacity");
                attrs.Value = WindowOpacity.ToString();
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("theme");
                attrs.Value = CurrentTheme.Name.ToString();
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

            #region == 各通貨毎の設定 ==

            XmlElement pairs = doc.CreateElement(string.Empty, "Pairs", string.Empty);

            // BtcJpy の設定
            XmlElement pairBtcJpy = doc.CreateElement(string.Empty, "BtcJpy", string.Empty);

            attrs = doc.CreateAttribute("playSoundLowest");
            if (PairBtcJpy.PlaySoundLowest)
            {
                attrs.Value = "true";
            }
            else
            {
                attrs.Value = "false";
            }
            pairBtcJpy.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("playSoundHighest");
            if (PairBtcJpy.PlaySoundHighest)
            {
                attrs.Value = "true";
            }
            else
            {
                attrs.Value = "false";
            }
            pairBtcJpy.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("playSoundLowest24h");
            if (PairBtcJpy.PlaySoundLowest24h)
            {
                attrs.Value = "true";
            }
            else
            {
                attrs.Value = "false";
            }
            pairBtcJpy.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("playSoundHighest24h");
            if (PairBtcJpy.PlaySoundHighest24h)
            {
                attrs.Value = "true";
            }
            else
            {
                attrs.Value = "false";
            }
            pairBtcJpy.SetAttributeNode(attrs);

            // カスタムアラート
            attrs = doc.CreateAttribute("alarmHigh");
            attrs.Value = PairBtcJpy.AlarmPlus.ToString();
            pairBtcJpy.SetAttributeNode(attrs);
            attrs = doc.CreateAttribute("alarmLow");
            attrs.Value = PairBtcJpy.AlarmMinus.ToString();
            pairBtcJpy.SetAttributeNode(attrs);

            // 板グルーピング
            attrs = doc.CreateAttribute("depthGrouping");
            attrs.Value = PairBtcJpy.DepthGrouping.ToString();
            pairBtcJpy.SetAttributeNode(attrs);

            //
            pairs.AppendChild(pairBtcJpy);

            // PairXrpJpy の設定
            XmlElement pairXrpJpy = doc.CreateElement(string.Empty, "XrpJpy", string.Empty);

            attrs = doc.CreateAttribute("playSoundLowest");
            if (PairXrpJpy.PlaySoundLowest)
            {
                attrs.Value = "true";
            }
            else
            {
                attrs.Value = "false";
            }
            pairXrpJpy.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("playSoundHighest");
            if (PairXrpJpy.PlaySoundHighest)
            {
                attrs.Value = "true";
            }
            else
            {
                attrs.Value = "false";
            }
            pairXrpJpy.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("playSoundLowest24h");
            if (PairXrpJpy.PlaySoundLowest24h)
            {
                attrs.Value = "true";
            }
            else
            {
                attrs.Value = "false";
            }
            pairXrpJpy.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("playSoundHighest24h");
            if (PairXrpJpy.PlaySoundHighest24h)
            {
                attrs.Value = "true";
            }
            else
            {
                attrs.Value = "false";
            }
            pairXrpJpy.SetAttributeNode(attrs);

            // カスタムアラート
            attrs = doc.CreateAttribute("alarmHigh");
            attrs.Value = PairXrpJpy.AlarmPlus.ToString();
            pairXrpJpy.SetAttributeNode(attrs);
            attrs = doc.CreateAttribute("alarmLow");
            attrs.Value = PairXrpJpy.AlarmMinus.ToString();
            pairXrpJpy.SetAttributeNode(attrs);

            // 板グルーピング
            attrs = doc.CreateAttribute("depthGrouping");
            attrs.Value = PairXrpJpy.DepthGrouping.ToString();
            pairXrpJpy.SetAttributeNode(attrs);

            //
            pairs.AppendChild(pairXrpJpy);

            // PairEthBtc の設定
            XmlElement pairEthBtc = doc.CreateElement(string.Empty, "EthBtc", string.Empty);

            attrs = doc.CreateAttribute("playSoundLowest");
            if (PairEthBtc.PlaySoundLowest)
            {
                attrs.Value = "true";
            }
            else
            {
                attrs.Value = "false";
            }
            pairEthBtc.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("playSoundHighest");
            if (PairEthBtc.PlaySoundHighest)
            {
                attrs.Value = "true";
            }
            else
            {
                attrs.Value = "false";
            }
            pairEthBtc.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("playSoundLowest24h");
            if (PairEthBtc.PlaySoundLowest24h)
            {
                attrs.Value = "true";
            }
            else
            {
                attrs.Value = "false";
            }
            pairEthBtc.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("playSoundHighest24h");
            if (PairEthBtc.PlaySoundHighest24h)
            {
                attrs.Value = "true";
            }
            else
            {
                attrs.Value = "false";
            }
            pairEthBtc.SetAttributeNode(attrs);

            // カスタムアラート
            attrs = doc.CreateAttribute("alarmHigh");
            attrs.Value = PairEthBtc.AlarmPlus.ToString();
            pairEthBtc.SetAttributeNode(attrs);
            attrs = doc.CreateAttribute("alarmLow");
            attrs.Value = PairEthBtc.AlarmMinus.ToString();
            pairEthBtc.SetAttributeNode(attrs);

            // 板グルーピング
            attrs = doc.CreateAttribute("depthGrouping");
            attrs.Value = PairEthBtc.DepthGrouping.ToString();
            pairEthBtc.SetAttributeNode(attrs);

            //
            pairs.AppendChild(pairEthBtc);

            // PairLtcBtc の設定
            XmlElement pairLtcBtc = doc.CreateElement(string.Empty, "LtcBtc", string.Empty);

            attrs = doc.CreateAttribute("playSoundLowest");
            if (PairLtcBtc.PlaySoundLowest)
            {
                attrs.Value = "true";
            }
            else
            {
                attrs.Value = "false";
            }
            pairLtcBtc.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("playSoundHighest");
            if (PairLtcBtc.PlaySoundHighest)
            {
                attrs.Value = "true";
            }
            else
            {
                attrs.Value = "false";
            }
            pairLtcBtc.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("playSoundLowest24h");
            if (PairLtcBtc.PlaySoundLowest24h)
            {
                attrs.Value = "true";
            }
            else
            {
                attrs.Value = "false";
            }
            pairLtcBtc.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("playSoundHighest24h");
            if (PairLtcBtc.PlaySoundHighest24h)
            {
                attrs.Value = "true";
            }
            else
            {
                attrs.Value = "false";
            }
            pairLtcBtc.SetAttributeNode(attrs);

            // カスタムアラート
            attrs = doc.CreateAttribute("alarmHigh");
            attrs.Value = PairLtcBtc.AlarmPlus.ToString();
            pairLtcBtc.SetAttributeNode(attrs);
            attrs = doc.CreateAttribute("alarmLow");
            attrs.Value = PairLtcBtc.AlarmMinus.ToString();
            pairLtcBtc.SetAttributeNode(attrs);

            // 板グルーピング
            attrs = doc.CreateAttribute("depthGrouping");
            attrs.Value = PairLtcBtc.DepthGrouping.ToString();
            pairLtcBtc.SetAttributeNode(attrs);

            //
            pairs.AppendChild(pairLtcBtc);

            // PairMonaJpy の設定
            XmlElement pairMonaJpy = doc.CreateElement(string.Empty, "MonaJpy", string.Empty);

            attrs = doc.CreateAttribute("playSoundLowest");
            if (PairMonaJpy.PlaySoundLowest)
            {
                attrs.Value = "true";
            }
            else
            {
                attrs.Value = "false";
            }
            pairMonaJpy.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("playSoundHighest");
            if (PairMonaJpy.PlaySoundHighest)
            {
                attrs.Value = "true";
            }
            else
            {
                attrs.Value = "false";
            }
            pairMonaJpy.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("playSoundLowest24h");
            if (PairMonaJpy.PlaySoundLowest24h)
            {
                attrs.Value = "true";
            }
            else
            {
                attrs.Value = "false";
            }
            pairMonaJpy.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("playSoundHighest24h");
            if (PairMonaJpy.PlaySoundHighest24h)
            {
                attrs.Value = "true";
            }
            else
            {
                attrs.Value = "false";
            }
            pairMonaJpy.SetAttributeNode(attrs);

            // カスタムアラート
            attrs = doc.CreateAttribute("alarmHigh");
            attrs.Value = PairMonaJpy.AlarmPlus.ToString();
            pairMonaJpy.SetAttributeNode(attrs);
            attrs = doc.CreateAttribute("alarmLow");
            attrs.Value = PairMonaJpy.AlarmMinus.ToString();
            pairMonaJpy.SetAttributeNode(attrs);

            // 板グルーピング
            attrs = doc.CreateAttribute("depthGrouping");
            attrs.Value = PairMonaJpy.DepthGrouping.ToString();
            pairMonaJpy.SetAttributeNode(attrs);

            //
            pairs.AppendChild(pairMonaJpy);

            // PairBchJpy の設定
            XmlElement pairBchJpy = doc.CreateElement(string.Empty, "BchJpy", string.Empty);

            attrs = doc.CreateAttribute("playSoundLowest");
            if (PairBchJpy.PlaySoundLowest)
            {
                attrs.Value = "true";
            }
            else
            {
                attrs.Value = "false";
            }
            pairBchJpy.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("playSoundHighest");
            if (PairBchJpy.PlaySoundHighest)
            {
                attrs.Value = "true";
            }
            else
            {
                attrs.Value = "false";
            }
            pairBchJpy.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("playSoundLowest24h");
            if (PairBchJpy.PlaySoundLowest24h)
            {
                attrs.Value = "true";
            }
            else
            {
                attrs.Value = "false";
            }
            pairBchJpy.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("playSoundHighest24h");
            if (PairBchJpy.PlaySoundHighest24h)
            {
                attrs.Value = "true";
            }
            else
            {
                attrs.Value = "false";
            }
            pairBchJpy.SetAttributeNode(attrs);

            // カスタムアラート
            attrs = doc.CreateAttribute("alarmHigh");
            attrs.Value = PairBchJpy.AlarmPlus.ToString();
            pairBchJpy.SetAttributeNode(attrs);
            attrs = doc.CreateAttribute("alarmLow");
            attrs.Value = PairBchJpy.AlarmMinus.ToString();
            pairBchJpy.SetAttributeNode(attrs);

            // 板グルーピング
            attrs = doc.CreateAttribute("depthGrouping");
            attrs.Value = PairBchJpy.DepthGrouping.ToString();
            pairBchJpy.SetAttributeNode(attrs);

            //
            pairs.AppendChild(pairBchJpy);



            // ////
            root.AppendChild(pairs);


            #endregion

            #region == アラーム音設定 ==

            XmlElement alarmSetting = doc.CreateElement(string.Empty, "Alarm", string.Empty);

            attrs = doc.CreateAttribute("playSound");
            if (PlaySound)
            {
                attrs.Value = "true";
            }
            else
            {
                attrs.Value = "false";
            }
            alarmSetting.SetAttributeNode(attrs);

            root.AppendChild(alarmSetting);

            #endregion

            try
            {
                // 設定ファイルの保存
                doc.Save(AppConfigFilePath);

            }
            //catch (System.IO.FileNotFoundException) { }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("■■■■■ Error  設定ファイルの保存中: " + ex + " while opening : " + AppConfigFilePath);
            }

            #endregion

        }

        #endregion

        #region == メソッド ==

        // テーマをセットするメソッド
        private void SetCurrentTheme(string themeName)
        {
            Theme test = _themes.FirstOrDefault(x => x.Name == themeName);
            if (test != null)
            {
                CurrentTheme = test;
            }
        }

        // 板情報 取得
        private async Task<bool> GetDepth(Pairs pair)
        {

            // まとめグルーピング単位 
            decimal unit = ActivePair.DepthGrouping;

            // リスト数 （基本 上売り200、下買い200）
            int half = 200;
            int listCount = (half * 2) + 1;

            // 初期化
            if (_depth.Count == 0)
            {
                for (int i = 0; i < listCount; i++)
                {
                    Depth dd = new Depth();
                    dd.DepthPrice = 0;
                    dd.DepthBid = 0;
                    dd.DepthAsk = 0;
                    _depth.Add(dd);
                }
            }
            else
            {
                if (DepthGroupingChanged)
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

                    DepthGroupingChanged = false;
                }
            }

            // LTP を追加
            //Depth ddd = new Depth();
            _depth[half].DepthPrice = ActivePair.Ltp;
            //_depth[half].DepthBid = 0;
            //_depth[half].DepthAsk = 0;
            _depth[half].IsLTP = true;
            //_depth[half] = ddd;

            try
            {
                DepthResult dpr = await _pubDepthApi.GetDepth(pair.ToString());

                if (dpr != null)
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

                                    // 今回のAskは先送り
                                    t = d;
                                    // 今回のPriceが基準になる
                                    c2 = System.Math.Ceiling(e / unit);

                                    i++;

                                }

                            }
                            else
                            {
                                _depth[half - i] = dp;
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
                                _depth[i] = dp;
                                i++;
                            }

                        }

                        _depth[half + 1].IsBidBest = true;

                    }

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
            while (true)
            {
                // 省エネモードならスルー。
                if (MinMode)
                {
                    await Task.Delay(2000);
                    continue;
                }

                // 間隔 1/2
                await Task.Delay(600);

                try
                {
                    await GetDepth(CurrentPair);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("■■■■■ UpdateDepth Exception: " + e);
                }

                // 間隔 1/2
                await Task.Delay(600);
            }

        }

        // トランザクションの取得
        private async Task<bool> GetTransactions(Pairs pair)
        {
            try
            {
                TransactionsResult trs = await _pubTransactionsApi.GetTransactions(pair.ToString());

                if (trs != null)
                {
                    //Debug.WriteLine(trs.Trans.Count.ToString());

                    if (_transactions.Count == 0)
                    {
                        // 60 で初期化
                        for (int i = 0; i < 60; i++)
                        {
                            Transactions dd = new Transactions();
                            //
                            _transactions.Add(dd);
                        }
                    }

                    int v = 0;
                    foreach (var tr in trs.Trans)
                    {
                        //_transactions[v] = tr;

                        _transactions[v].Amount = tr.Amount;
                        _transactions[v].ExecutedAt = tr.ExecutedAt;
                        _transactions[v].Price = tr.Price;
                        _transactions[v].Side = tr.Side;
                        _transactions[v].TransactionId = tr.TransactionId;

                        v++;

                        if (v >= 60)
                            break;
                    }

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
            while (true)
            {
                // 省エネモードならスルー。
                if (MinMode)
                {
                    await Task.Delay(2000);
                    continue;
                }

                // 間隔 1/2
                await Task.Delay(1300);

                try
                {
                    await GetTransactions(CurrentPair);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("■■■■■ UpdateTransactions Exception: " + e);
                }

                // 間隔 1/2
                await Task.Delay(1300);
            }

        }

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
            // TODO 個別にロードして、サーバー負荷を下げ、初期表示を早くする。

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
                    span = 60+1;
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
                    span = (24 * 3);
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

            //Debug.WriteLine("ロード中  " + pair.ToString() + " " + lst.Count.ToString() + " " + span.ToString());

            try
            {

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
                else if (pair == Pairs.eth_btc)
                {
                    chartSeries = ChartSeriesEthBtc;
                    chartAxisX = ChartAxisXEthBtc;
                    chartAxisY = ChartAxisYEthBtc;
                }
                else if (pair == Pairs.mona_jpy)
                {
                    chartSeries = ChartSeriesMonaJpy;
                    chartAxisX = ChartAxisXMonaJpy;
                    chartAxisY = ChartAxisYMonaJpy;
                }
                else if (pair == Pairs.mona_btc)
                {
                    //
                }
                else if (pair == Pairs.ltc_btc)
                {
                    chartSeries = ChartSeriesLtcBtc;
                    chartAxisX = ChartAxisXLtcBtc;
                    chartAxisY = ChartAxisYLtcBtc;
                }
                else if (pair == Pairs.bcc_btc)
                {
                    //
                }
                else if (pair == Pairs.bcc_jpy)
                {
                    chartSeries = ChartSeriesBchJpy;
                    chartAxisX = ChartAxisXBchJpy;
                    chartAxisY = ChartAxisYBchJpy;
                }

                try
                {
                    // チャート OHLCVのロード
                    if (lst.Count > 0)
                    {

                        // Temp を作って、後でまとめて追加する。
                        // https://lvcharts.net/App/examples/v1/wpf/Performance%20Tips

                        var tempCv = new OhlcPoint[span - 1];

                        var tempVol = new double[span - 1];

                        //var tempLabel = new string[span - 1];

                        int i = 0;
                        int c = span;
                        foreach (var oh in lst)
                        {
                            // 全てのポイントが同じ場合、スキップする。変なデータ？ 本家もスキップしている。＞逆に変になるからそのまま
                            //if ((oh.Open == oh.High) && (oh.Open == oh.Low) && (oh.Open == oh.Close) && (oh.Volume == 0))
                            //{
                                //continue;
                            //}

                            // 表示数を限る 直近のspan本
                            if (i < (span - 1))
                            {

                                // ポイント作成
                                OhlcPoint p = new OhlcPoint((double)oh.Open, (double)oh.High, (double)oh.Low, (double)oh.Close);

                                // 直接追加しないで、
                                //ChartSeries[0].Values.Add(p);
                                // 一旦、Tempに追加して、あとでまとめてAddRange
                                tempCv[c - 2] = p;

                                // 出来高
                                //ChartSeries[3].Values.Add((double)oh.Volume);
                                tempVol[c - 2] = (double)oh.Volume;

                                // ラベル
                                if (ct == CandleTypes.OneMin)
                                {
                                    chartAxisX[0].Labels.Insert(0, oh.TimeStamp.ToString("H:mm"));
                                    //tempLabel[c - 2] = oh.TimeStamp.ToString("H:mm");
                                }
                                else if (ct == CandleTypes.OneHour)
                                {
                                    chartAxisX[0].Labels.Insert(0, oh.TimeStamp.ToString("d日 H:mm"));
                                    //tempLabel[c - 2] = oh.TimeStamp.ToString("d日 H:mm");
                                }
                                else if (ct == CandleTypes.OneDay)
                                {
                                    chartAxisX[0].Labels.Insert(0, oh.TimeStamp.ToString("M月d日"));
                                    //tempLabel[c - 2] = oh.TimeStamp.ToString("M月d日");
                                }
                                else
                                {
                                    throw new System.InvalidOperationException("LoadChart: 不正な CandleType");
                                }
                                //Debug.WriteLine(oh.TimeStamp.ToString("dd日 hh時mm分"));

                                c = c - 1;

                            }

                            i = i + 1;
                        }


                        // Candlestickクリア
                        chartSeries[0].Values.Clear();

                        // 出来高クリア
                        //ChartSeries[1].Values.Clear();
                        // https://github.com/Live-Charts/Live-Charts/issues/76
                        for (int v = 0; v < chartSeries[1].Values.Count - 1; v++)
                        {
                            chartSeries[1].Values[v] = (double)0;
                        }

                        if (Application.Current == null) return;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            try
                            {

                                // ラベル表示クリア > エラる
                                //chartAxisX[0].Labels.Clear();

                                // 期間設定
                                chartAxisX[0].MaxValue = span - 1;
                                //chartAxisX[0].MinValue = 0;

                                // まとめて追加

                                // OHLCV
                                chartSeries[0].Values.AddRange(tempCv);

                                // Volume
                                var cv = new ChartValues<double>();
                                cv.AddRange(tempVol);
                                chartSeries[1].Values = cv;

                                // Label
                                //chartAxisX[0].Labels = tempLabel;

                            }
                            catch (Exception ex)
                            {

                                ChartLoadingInfo = pair.ToString() + " チャートのロード中にエラーが発生しました 1 ";

                                Debug.WriteLine("■■■■■ " + pair.ToString() + " Chart loading error: " + ex.ToString());
                            }

                        }, DispatcherPriority.Normal);
                    }

                }
                catch (Exception ex)
                {
                    ChartLoadingInfo = "チャートのロード中にエラーが発生しました 2 ";

                    Debug.WriteLine("■■■■■ Chart loading error: " + ex.ToString());
                }

            }
            catch (Exception ex)
            {
                ChartLoadingInfo = "チャートのロード中にエラーが発生しました 3";

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

        // チャート表示期間を変えた時に
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
                    //DisplayChart(pair);
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
                    //DisplayChart(pair);
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
                    //DisplayChart(pair);
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
                    //DisplayChart(pair);
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
                    //DisplayChart(pair);
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
                    //DisplayChart(pair);
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
                    //DisplayChart(pair);
                }
            }
            else if (_chartSpan == ChartSpans.OneYear)
            {
                if (SelectedCandleType != CandleTypes.OneDay)
                {
                    //SelectedCandleType = CandleTypes.OneDay;
                }
                else
                {
                    //LoadChart(pair, SelectedCandleType);
                    //DisplayChart(pair);
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
                    //LoadChart(pair, SelectedCandleType);
                    //DisplayChart(pair);
                }
            }

        }

        private void ChangeChartSpans()
        {
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
                    ChangeChartSpan(p);
                }
            }

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
            else if (pair == Pairs.eth_btc)
            {
                chartSeries = ChartSeriesEthBtc;
                chartAxisX = ChartAxisXEthBtc;
                chartAxisY = ChartAxisYEthBtc;
            }
            else if (pair == Pairs.mona_jpy)
            {
                chartSeries = ChartSeriesMonaJpy;
                chartAxisX = ChartAxisXMonaJpy;
                chartAxisY = ChartAxisYMonaJpy;
            }
            else if (pair == Pairs.mona_btc)
            {
                //
            }
            else if (pair == Pairs.ltc_btc)
            {
                chartSeries = ChartSeriesLtcBtc;
                chartAxisX = ChartAxisXLtcBtc;
                chartAxisY = ChartAxisYLtcBtc;
            }
            else if (pair == Pairs.bcc_btc)
            {
                //
            }
            else if (pair == Pairs.bcc_jpy)
            {
                chartSeries = ChartSeriesBchJpy;
                chartAxisX = ChartAxisXBchJpy;
                chartAxisY = ChartAxisYBchJpy;
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
            else if (pair == Pairs.eth_btc)
            {
                chartSeries = ChartSeriesEthBtc;
                chartAxisX = ChartAxisXEthBtc;
                chartAxisY = ChartAxisYEthBtc;
            }
            else if (pair == Pairs.mona_jpy)
            {
                chartSeries = ChartSeriesMonaJpy;
                chartAxisX = ChartAxisXMonaJpy;
                chartAxisY = ChartAxisYMonaJpy;
            }
            else if (pair == Pairs.mona_btc)
            {
                //
            }
            else if (pair == Pairs.ltc_btc)
            {
                chartSeries = ChartSeriesLtcBtc;
                chartAxisX = ChartAxisXLtcBtc;
                chartAxisY = ChartAxisYLtcBtc;
            }
            else if (pair == Pairs.bcc_btc)
            {
                //
            }
            else if (pair == Pairs.bcc_jpy)
            {
                chartSeries = ChartSeriesBchJpy;
                chartAxisX = ChartAxisXBchJpy;
                chartAxisY = ChartAxisYBchJpy;
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

        }

        #endregion

        #endregion

        #region == コマンド ==

        // 設定画面表示
        public ICommand ShowSettingsCommand { get; }
        public bool ShowSettingsCommand_CanExecute()
        {
            return true;
        }
        public void ShowSettingsCommand_Execute()
        {
            if (ShowSettings)
            {
                ShowSettings = false;
            }
            else
            {
                ShowSettings = true;
            }
        }

        // 設定画面キャンセルボタン
        public ICommand SettingsCancelCommand { get; }
        public bool SettingsCancelCommand_CanExecute()
        {
            return true;
        }
        public void SettingsCancelCommand_Execute()
        {
            ShowSettings = false;
        }

        // 設定画面OKボタン
        public ICommand SettingsOKCommand { get; }
        public bool SettingsOKCommand_CanExecute()
        {
            return true;
        }
        public void SettingsOKCommand_Execute()
        {
            ShowSettings = false;
        }

        // 板情報のグルーピングコマンド
        public ICommand DepthGroupingCommand { get; }
        public bool DepthGroupingCommand_CanExecute()
        {
            return true;
        }
        public void DepthGroupingCommand_Execute(object obj)
        {
            if (obj == null) return;

            Decimal numVal = Decimal.Parse(obj.ToString());

            if (ActivePair.DepthGrouping != numVal)
            {
                ActivePair.DepthGrouping = numVal;
                DepthGroupingChanged = true;
            }
        }

        #endregion

    }
}
