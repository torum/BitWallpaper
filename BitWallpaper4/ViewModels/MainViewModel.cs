﻿using BitWallpaper4.Models;
using BitWallpaper4.Models.APIClients;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BitWallpaper4.ViewModels;

public partial class MainViewModel : ViewModelBase
{

    public string VersionText { get => "v2.0.0.1"; }


    private ObservableCollection<PairViewModel> _pairs = new() 
    {
        new PairViewModel(PairCodes.btc_jpy, 24, "{0:#,0}", "C", 100M, 1000M),
        new PairViewModel(PairCodes.xrp_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
        new PairViewModel(PairCodes.eth_jpy, 24, "{0:#,0}", "C", 100M, 1000M),
        new PairViewModel(PairCodes.ltc_jpy, 24, "{0:#,0.0}", "C1", 100M, 1000M),
        new PairViewModel(PairCodes.mona_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 1M),
        new PairViewModel(PairCodes.bcc_jpy, 24, "{0:#,0}", "C", 10M, 100M),
        new PairViewModel(PairCodes.xlm_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
        new PairViewModel(PairCodes.qtum_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
        new PairViewModel(PairCodes.bat_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
    };
    public ObservableCollection<PairViewModel> Pairs
    {
        get => _pairs;
        set
        {
            if (_pairs == value) return;

            _pairs = value;
            NotifyPropertyChanged(nameof(Pairs));
        }
    }

    private PairViewModel _selectedPair;
    public PairViewModel SelectedPair
    {
        get => _selectedPair;
        set
        {
            if (_selectedPair == value) return;

            _selectedPair = value;

            if (_selectedPair != null)
            {
                foreach (var hoge in _pairs)
                {
                    if (hoge.PairCode == _selectedPair.PairCode)
                    {
                        hoge.IsActive = true;
                    }
                    else
                    {
                        hoge.IsActive = false;
                    }
                }
            }

            NotifyPropertyChanged(nameof(SelectedPair));
        }
    }

    public void SetSelectedPairFromCode(PairCodes pairCode)
    {
        if (_selectedPair?.PairCode == pairCode) return;

        var fuga = Pairs.FirstOrDefault(x => x.PairCode == pairCode);
        
        // little hack.
        _selectedPair = fuga;

        if (_selectedPair != null)
        {
            foreach (var hoge in _pairs)
            {
                if (hoge.PairCode == _selectedPair.PairCode)
                {
                    hoge.IsActive = true;
                }
                else
                {
                    hoge.IsActive = false;
                }
            }
        }

        NotifyPropertyChanged(nameof(SelectedPair));

        Task.Run(() =>
        {
            _selectedPair.InitializeAndStart();
        });

    }

    #region == （個別設定が必要） ==

    // TODO:
    private string _ltpJpyBtc = "";
    public string LtpBtcJpy
    {
        get => _ltpJpyBtc;
        set
        {
            if (_ltpJpyBtc == value)
                return;

            _ltpJpyBtc = value;
            NotifyPropertyChanged(nameof(LtpBtcJpy));
        }
    }

    private string _ltpXrpJpy = "";
    public string LtpXrpJpy
    {
        get => _ltpXrpJpy;
        set
        {
            if (_ltpXrpJpy == value)
                return;

            _ltpXrpJpy = value;
            NotifyPropertyChanged(nameof(LtpXrpJpy));
        }
    }

    private string _ltpEthJpy = "";
    public string LtpEthJpy
    {
        get => _ltpEthJpy;
        set
        {
            if (_ltpEthJpy == value)
                return;

            _ltpEthJpy = value;
            NotifyPropertyChanged(nameof(LtpEthJpy));
        }
    }

    private string _ltpLtcJpy = "";
    public string LtpLtcJpy
    {
        get => _ltpLtcJpy;
        set
        {
            if (_ltpLtcJpy == value)
                return;

            _ltpLtcJpy = value;
            NotifyPropertyChanged(nameof(LtpLtcJpy));
        }
    }

    private string _ltpMonaJpy = "";
    public string LtpMonaJpy
    {
        get => _ltpMonaJpy;
        set
        {
            if (_ltpMonaJpy == value)
                return;

            _ltpMonaJpy = value;
            NotifyPropertyChanged(nameof(LtpMonaJpy));
        }
    }

    private string _ltpBccJpy = "";
    public string LtpBccJpy
    {
        get => _ltpBccJpy;
        set
        {
            if (_ltpBccJpy == value)
                return;

            _ltpBccJpy = value;
            NotifyPropertyChanged(nameof(LtpBccJpy));
        }
    }

    private string _ltpXlmJpy = "";
    public string LtpXlmJpy
    {
        get => _ltpXlmJpy;
        set
        {
            if (_ltpXlmJpy == value)
                return;

            _ltpXlmJpy = value;
            NotifyPropertyChanged(nameof(LtpXlmJpy));
        }
    }

    private string _ltpQtumJpy = "";
    public string LtpQtumJpy
    {
        get => _ltpQtumJpy;
        set
        {
            if (_ltpQtumJpy == value)
                return;

            _ltpQtumJpy = value;
            NotifyPropertyChanged(nameof(LtpQtumJpy));
        }
    }

    private string _ltpBatJpy = "";
    public string LtpBatJpy
    {
        get => _ltpBatJpy;
        set
        {
            if (_ltpBatJpy == value)
                return;

            _ltpBatJpy = value;
            NotifyPropertyChanged(nameof(LtpBatJpy));
        }
    }

    #endregion

    // HTTP Clients
    readonly PublicAPIClient _pubTickerApi = new();
    //_pubTickerApi.ErrorOccured += new PrivateAPIClient.ClinetErrorEvent(OnError);

    // Timer
    readonly DispatcherTimer _dispatcherTimerTickAllPairs = new();

    public MainViewModel()
    {
        // Ticker update timer
        _dispatcherTimerTickAllPairs.Tick += TickerTimerAllPairs;
        _dispatcherTimerTickAllPairs.Interval = new TimeSpan(0, 0, 2);
        _dispatcherTimerTickAllPairs.Start();

        Task.Run(() =>
        {
            //TickerLoop();
        });
    }

    private async void TickerTimerAllPairs(object source, object e)
    {
        // 起動直後アラームを鳴らさない秒数
        //int waitTime = 60;

        foreach (var hoge in Pairs)
        {
            Ticker tick = await _pubTickerApi.GetTicker(hoge.PairCode.ToString());

            if (tick != null)
            {
                // TODO: （個別設定が必要）
                if (hoge.PairCode == PairCodes.btc_jpy)
                {
                    LtpBtcJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                }
                else if (hoge.PairCode == PairCodes.xrp_jpy)
                {
                    LtpXrpJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                }
                else if (hoge.PairCode == PairCodes.eth_jpy)
                {
                    LtpEthJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                }
                else if (hoge.PairCode == PairCodes.ltc_jpy)
                {
                    LtpLtcJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                }
                else if (hoge.PairCode == PairCodes.mona_jpy)
                {
                    LtpMonaJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                }
                else if (hoge.PairCode == PairCodes.bcc_jpy)
                {
                    LtpBccJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                }
                else if (hoge.PairCode == PairCodes.xlm_jpy)
                {
                    LtpXlmJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                }
                else if (hoge.PairCode == PairCodes.qtum_jpy)
                {
                    LtpQtumJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                }
                else if (hoge.PairCode == PairCodes.bat_jpy)
                {
                    LtpBatJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                }
                // TODO: more


                /*
                // Up/down flag. 一旦前の値を保存
                var prevLtp = hoge.Ltp;

                if (tick.LTP > prevLtp)
                {
                    hoge.LtpUpFlag = true;
                }
                else if (tick.LTP < prevLtp)
                {
                    hoge.LtpUpFlag = false;
                }
                */


                // 最新の価格をセット
                hoge.Ltp = tick.LTP;
                hoge.Bid = tick.Bid;
                hoge.Ask = tick.Ask;
                hoge.TickTimeStamp = tick.TimeStamp;
                hoge.LowestIn24Price = tick.Low;
                hoge.HighestIn24Price = tick.High;
                /*

                // 起動時価格セット
                if (hoge.BasePrice == 0) hoge.BasePrice = tick.LTP;

                // 最安値登録
                if (hoge.LowestPrice == 0)
                {
                    hoge.LowestPrice = tick.LTP;
                }
                if (tick.LTP < hoge.LowestPrice)
                {
                    hoge.LowestPrice = tick.LTP;
                }

                // 最高値登録
                if (hoge.HighestPrice == 0)
                {
                    hoge.HighestPrice = tick.LTP;
                }
                if (tick.LTP > hoge.HighestPrice)
                {
                    hoge.HighestPrice = tick.LTP;
                }
                */

                #region == チック履歴 ==
                /*
                TickHistory aym = new TickHistory();
                aym.Price = tick.LTP;
                aym.TimeAt = tick.TimeStamp;
                if (hoge.TickHistories.Count > 0)
                {
                    if (hoge.TickHistories[0].Price > aym.Price)
                    {
                        aym.TickHistoryPriceUp = true;
                        hoge.TickHistories.Insert(0, aym);

                    }
                    else if (hoge.TickHistories[0].Price < aym.Price)
                    {
                        aym.TickHistoryPriceUp = false;
                        hoge.TickHistories.Insert(0, aym);
                    }
                    else
                    {
                        //aym.TickHistoryPriceColor = Colors.Gainsboro;
                        hoge.TickHistories.Insert(0, aym);
                    }
                }
                else
                {
                    //aym.TickHistoryPriceColor = Colors.Gainsboro;
                    hoge.TickHistories.Insert(0, aym);
                }

                // limit the number of the list.
                if (hoge.TickHistories.Count > 60)
                {
                    hoge.TickHistories.RemoveAt(60);
                }

                // 60(1分)の平均値を求める
                decimal aSum = 0;
                int c = 0;
                if (hoge.TickHistories.Count > 0)
                {

                    if (hoge.TickHistories.Count > 60)
                    {
                        c = 59;
                    }
                    else
                    {
                        c = hoge.TickHistories.Count - 1;
                    }

                    if (c == 0)
                    {
                        hoge.AveragePrice = hoge.TickHistories[0].Price;
                    }
                    else
                    {
                        for (int i = 0; i < c; i++)
                        {
                            aSum = aSum + hoge.TickHistories[i].Price;
                        }
                        hoge.AveragePrice = aSum / c;
                    }

                }
                else if (hoge.TickHistories.Count == 1)
                {
                    hoge.AveragePrice = hoge.TickHistories[0].Price;
                }
                */
                #endregion

                #region == アラーム ==
                /*
                bool isPlayed = false;

                // カスタムアラーム
                if (hoge.AlarmPlus > 0)
                {
                    if (tick.LTP >= hoge.AlarmPlus)
                    {
                        hoge.HighLowInfoTextColorFlag = true;
                        hoge.HighLowInfoText = hoge.PairString + " ⇑⇑⇑　高値アラーム ";

                        //ShowBalloonEventArgs ag = new ShowBalloonEventArgs
                        //{
                        //    Title = PairBtcJpy.PairString + " 高値アラーム",
                        //    Text = PairBtcJpy.AlarmPlus.ToString("#,0") + " に達しました。"
                        //};
                        // バルーン表示
                        //ShowBalloon?.Invoke(this, ag);

                        // クリア
                        hoge.AlarmPlus = 0;

                    }
                }

                if (hoge.AlarmMinus > 0)
                {
                    if (tick.LTP <= hoge.AlarmMinus)
                    {
                        hoge.HighLowInfoTextColorFlag = false;
                        hoge.HighLowInfoText = hoge.PairString + " ⇓⇓⇓　安値アラーム ";

                        //ShowBalloonEventArgs ag = new ShowBalloonEventArgs
                        //{
                        //    Title = hoge.PairString + " 安値アラーム",
                        //    Text = hoge.AlarmMinus.ToString("#,0") + " に達しました。"
                        //};
                        // バルーン表示
                        //ShowBalloon?.Invoke(this, ag);

                        // クリア
                        hoge.AlarmMinus = 0;

                    }
                }

                // 起動後最高値
                if ((tick.LTP >= hoge.HighestPrice) && (prevLtp != tick.LTP))
                {
                    if ((hoge.TickHistories.Count > waitTime) && ((hoge.BasePrice + 100M) < tick.LTP))
                    {

                        // 値を点滅させるフラグ
                        if (hoge.PlaySoundHighest)
                            hoge.HighestPriceAlart = true;

                        // 音を鳴らす
                        if ((isPlayed == false) && (hoge.PlaySoundHighest == true))
                        {

                            // 色を変える
                            hoge.HighLowInfoTextColorFlag = true;
                            // テキストを点滅させる為、一旦クリア
                            hoge.HighLowInfoText = "";
                            hoge.HighLowInfoText = hoge.PairString + " ⇑⇑⇑　起動後最高値 ";


                            //if (PlaySound)
                            //{
                            //    SystemSounds.Hand.Play();
                            //    isPlayed = true;
                            //}

                        }
                    }
                }
                else
                {
                    hoge.HighestPriceAlart = false;
                }

                // 起動後最安値
                if ((tick.LTP <= hoge.LowestPrice) && (prevLtp != tick.LTP))
                {
                    if ((hoge.TickHistories.Count > waitTime) && ((hoge.BasePrice - 100M) > tick.LTP))
                    {

                        if (hoge.PlaySoundLowest)
                            hoge.LowestPriceAlart = true;

                        if ((isPlayed == false) && (hoge.PlaySoundLowest == true))
                        {
                            hoge.HighLowInfoTextColorFlag = false;
                            hoge.HighLowInfoText = "";
                            hoge.HighLowInfoText = hoge.PairString + " ⇓⇓⇓　起動後最安値 ";

                            //if (PlaySound)
                            //{
                            //    SystemSounds.Beep.Play();
                            //    isPlayed = true;
                            //}

                        }

                    }
                }
                else
                {
                    hoge.LowestPriceAlart = false;
                }

                // 過去24時間最高値
                if ((tick.LTP >= hoge.HighestIn24Price) && (prevLtp != tick.LTP) && (hoge.TickHistories.Count > waitTime))
                {
                    if (hoge.PlaySoundHighest24h)
                        hoge.HighestIn24PriceAlart = true;

                    if ((isPlayed == false) && (hoge.PlaySoundHighest24h == true))
                    {
                        hoge.HighLowInfoTextColorFlag = true;
                        hoge.HighLowInfoText = "";
                        hoge.HighLowInfoText = hoge.PairString + " ⇑⇑⇑⇑⇑⇑　24時間最高値 ";

                        //if (PlaySound)
                        //{
                        //    SystemSounds.Hand.Play();
                        //isPlayed = true;
                        //}

                    }
                }
                else
                {
                    hoge.HighestIn24PriceAlart = false;
                }

                // 過去24時間最安値
                if ((tick.LTP <= hoge.LowestIn24Price) && (prevLtp != tick.LTP) && (hoge.TickHistories.Count > waitTime))
                {

                    if (hoge.PlaySoundLowest24h)
                        hoge.LowestIn24PriceAlart = true;

                    if ((isPlayed == false) && (hoge.PlaySoundLowest24h == true))
                    {
                        hoge.HighLowInfoTextColorFlag = false;
                        hoge.HighLowInfoText = "";
                        hoge.HighLowInfoText = hoge.PairString + " ⇓⇓⇓⇓⇓⇓　24時間最安値 ";

                        //if (PlaySound)
                        //{
                        //    SystemSounds.Hand.Play();
                        //    isPlayed = true;
                        //}

                    }
                }
                else
                {
                    hoge.LowestIn24PriceAlart = false;
                }
                */
                #endregion

            }
            else
            {
                // TODO:
                //APIResultTicker = "<<取得失敗>>";
                Debug.WriteLine("■■■■■ TickerTimerAllPairs: GetTicker returned null");
                break;
            }

        }
    }

    private async void TickerLoop()
    {
        while (true)
        {
            foreach (var hoge in Pairs)
            {
                try
                {
                    Ticker tick = await _pubTickerApi.GetTicker(hoge.PairCode.ToString());

                    if (tick != null)
                    {
                        // TODO: （個別設定が必要）
                        if (hoge.PairCode == PairCodes.btc_jpy)
                        {
                            LtpBtcJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                        }
                        else if (hoge.PairCode == PairCodes.xrp_jpy)
                        {
                            LtpXrpJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                        }
                        else if (hoge.PairCode == PairCodes.eth_jpy)
                        {
                            LtpEthJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                        }
                        else if (hoge.PairCode == PairCodes.ltc_jpy)
                        {
                            LtpLtcJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                        }
                        else if (hoge.PairCode == PairCodes.mona_jpy)
                        {
                            LtpMonaJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                        }
                        else if (hoge.PairCode == PairCodes.bcc_jpy)
                        {
                            LtpBccJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                        }
                        else if (hoge.PairCode == PairCodes.xlm_jpy)
                        {
                            LtpXlmJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                        }
                        else if (hoge.PairCode == PairCodes.qtum_jpy)
                        {
                            LtpQtumJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                        }
                        else if (hoge.PairCode == PairCodes.bat_jpy)
                        {
                            LtpBatJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                        }
                        // TODO: more


                        /*
                        // Up/down flag. 一旦前の値を保存
                        var prevLtp = hoge.Ltp;

                        if (tick.LTP > prevLtp)
                        {
                            hoge.LtpUpFlag = true;
                        }
                        else if (tick.LTP < prevLtp)
                        {
                            hoge.LtpUpFlag = false;
                        }
                        */


                        // 最新の価格をセット
                        hoge.Ltp = tick.LTP;
                        hoge.Bid = tick.Bid;
                        hoge.Ask = tick.Ask;
                        hoge.TickTimeStamp = tick.TimeStamp;
                        hoge.LowestIn24Price = tick.Low;
                        hoge.HighestIn24Price = tick.High;
                        /*

                        // 起動時価格セット
                        if (hoge.BasePrice == 0) hoge.BasePrice = tick.LTP;

                        // 最安値登録
                        if (hoge.LowestPrice == 0)
                        {
                            hoge.LowestPrice = tick.LTP;
                        }
                        if (tick.LTP < hoge.LowestPrice)
                        {
                            hoge.LowestPrice = tick.LTP;
                        }

                        // 最高値登録
                        if (hoge.HighestPrice == 0)
                        {
                            hoge.HighestPrice = tick.LTP;
                        }
                        if (tick.LTP > hoge.HighestPrice)
                        {
                            hoge.HighestPrice = tick.LTP;
                        }
                        */

                        #region == チック履歴 ==
                        /*
                        TickHistory aym = new TickHistory();
                        aym.Price = tick.LTP;
                        aym.TimeAt = tick.TimeStamp;
                        if (hoge.TickHistories.Count > 0)
                        {
                            if (hoge.TickHistories[0].Price > aym.Price)
                            {
                                aym.TickHistoryPriceUp = true;
                                hoge.TickHistories.Insert(0, aym);

                            }
                            else if (hoge.TickHistories[0].Price < aym.Price)
                            {
                                aym.TickHistoryPriceUp = false;
                                hoge.TickHistories.Insert(0, aym);
                            }
                            else
                            {
                                //aym.TickHistoryPriceColor = Colors.Gainsboro;
                                hoge.TickHistories.Insert(0, aym);
                            }
                        }
                        else
                        {
                            //aym.TickHistoryPriceColor = Colors.Gainsboro;
                            hoge.TickHistories.Insert(0, aym);
                        }

                        // limit the number of the list.
                        if (hoge.TickHistories.Count > 60)
                        {
                            hoge.TickHistories.RemoveAt(60);
                        }

                        // 60(1分)の平均値を求める
                        decimal aSum = 0;
                        int c = 0;
                        if (hoge.TickHistories.Count > 0)
                        {

                            if (hoge.TickHistories.Count > 60)
                            {
                                c = 59;
                            }
                            else
                            {
                                c = hoge.TickHistories.Count - 1;
                            }

                            if (c == 0)
                            {
                                hoge.AveragePrice = hoge.TickHistories[0].Price;
                            }
                            else
                            {
                                for (int i = 0; i < c; i++)
                                {
                                    aSum = aSum + hoge.TickHistories[i].Price;
                                }
                                hoge.AveragePrice = aSum / c;
                            }

                        }
                        else if (hoge.TickHistories.Count == 1)
                        {
                            hoge.AveragePrice = hoge.TickHistories[0].Price;
                        }
                        */
                        #endregion

                        #region == アラーム ==
                        /*
                        bool isPlayed = false;

                        // カスタムアラーム
                        if (hoge.AlarmPlus > 0)
                        {
                            if (tick.LTP >= hoge.AlarmPlus)
                            {
                                hoge.HighLowInfoTextColorFlag = true;
                                hoge.HighLowInfoText = hoge.PairString + " ⇑⇑⇑　高値アラーム ";

                                //ShowBalloonEventArgs ag = new ShowBalloonEventArgs
                                //{
                                //    Title = PairBtcJpy.PairString + " 高値アラーム",
                                //    Text = PairBtcJpy.AlarmPlus.ToString("#,0") + " に達しました。"
                                //};
                                // バルーン表示
                                //ShowBalloon?.Invoke(this, ag);

                                // クリア
                                hoge.AlarmPlus = 0;

                            }
                        }

                        if (hoge.AlarmMinus > 0)
                        {
                            if (tick.LTP <= hoge.AlarmMinus)
                            {
                                hoge.HighLowInfoTextColorFlag = false;
                                hoge.HighLowInfoText = hoge.PairString + " ⇓⇓⇓　安値アラーム ";

                                //ShowBalloonEventArgs ag = new ShowBalloonEventArgs
                                //{
                                //    Title = hoge.PairString + " 安値アラーム",
                                //    Text = hoge.AlarmMinus.ToString("#,0") + " に達しました。"
                                //};
                                // バルーン表示
                                //ShowBalloon?.Invoke(this, ag);

                                // クリア
                                hoge.AlarmMinus = 0;

                            }
                        }

                        // 起動後最高値
                        if ((tick.LTP >= hoge.HighestPrice) && (prevLtp != tick.LTP))
                        {
                            if ((hoge.TickHistories.Count > waitTime) && ((hoge.BasePrice + 100M) < tick.LTP))
                            {

                                // 値を点滅させるフラグ
                                if (hoge.PlaySoundHighest)
                                    hoge.HighestPriceAlart = true;

                                // 音を鳴らす
                                if ((isPlayed == false) && (hoge.PlaySoundHighest == true))
                                {

                                    // 色を変える
                                    hoge.HighLowInfoTextColorFlag = true;
                                    // テキストを点滅させる為、一旦クリア
                                    hoge.HighLowInfoText = "";
                                    hoge.HighLowInfoText = hoge.PairString + " ⇑⇑⇑　起動後最高値 ";


                                    //if (PlaySound)
                                    //{
                                    //    SystemSounds.Hand.Play();
                                    //    isPlayed = true;
                                    //}

                                }
                            }
                        }
                        else
                        {
                            hoge.HighestPriceAlart = false;
                        }

                        // 起動後最安値
                        if ((tick.LTP <= hoge.LowestPrice) && (prevLtp != tick.LTP))
                        {
                            if ((hoge.TickHistories.Count > waitTime) && ((hoge.BasePrice - 100M) > tick.LTP))
                            {

                                if (hoge.PlaySoundLowest)
                                    hoge.LowestPriceAlart = true;

                                if ((isPlayed == false) && (hoge.PlaySoundLowest == true))
                                {
                                    hoge.HighLowInfoTextColorFlag = false;
                                    hoge.HighLowInfoText = "";
                                    hoge.HighLowInfoText = hoge.PairString + " ⇓⇓⇓　起動後最安値 ";

                                    //if (PlaySound)
                                    //{
                                    //    SystemSounds.Beep.Play();
                                    //    isPlayed = true;
                                    //}

                                }

                            }
                        }
                        else
                        {
                            hoge.LowestPriceAlart = false;
                        }

                        // 過去24時間最高値
                        if ((tick.LTP >= hoge.HighestIn24Price) && (prevLtp != tick.LTP) && (hoge.TickHistories.Count > waitTime))
                        {
                            if (hoge.PlaySoundHighest24h)
                                hoge.HighestIn24PriceAlart = true;

                            if ((isPlayed == false) && (hoge.PlaySoundHighest24h == true))
                            {
                                hoge.HighLowInfoTextColorFlag = true;
                                hoge.HighLowInfoText = "";
                                hoge.HighLowInfoText = hoge.PairString + " ⇑⇑⇑⇑⇑⇑　24時間最高値 ";

                                //if (PlaySound)
                                //{
                                //    SystemSounds.Hand.Play();
                                //isPlayed = true;
                                //}

                            }
                        }
                        else
                        {
                            hoge.HighestIn24PriceAlart = false;
                        }

                        // 過去24時間最安値
                        if ((tick.LTP <= hoge.LowestIn24Price) && (prevLtp != tick.LTP) && (hoge.TickHistories.Count > waitTime))
                        {

                            if (hoge.PlaySoundLowest24h)
                                hoge.LowestIn24PriceAlart = true;

                            if ((isPlayed == false) && (hoge.PlaySoundLowest24h == true))
                            {
                                hoge.HighLowInfoTextColorFlag = false;
                                hoge.HighLowInfoText = "";
                                hoge.HighLowInfoText = hoge.PairString + " ⇓⇓⇓⇓⇓⇓　24時間最安値 ";

                                //if (PlaySound)
                                //{
                                //    SystemSounds.Hand.Play();
                                //    isPlayed = true;
                                //}

                            }
                        }
                        else
                        {
                            hoge.LowestIn24PriceAlart = false;
                        }
                        */
                        #endregion

                    }
                    else
                    {
                        // TODO:
                        //APIResultTicker = "<<取得失敗>>";
                        Debug.WriteLine("■■■■■ TickerLoop: GetTicker returned null");
                        break;
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("TickerLoop: " + ex);
                }

                //await Task.Delay(100);
            }

            await Task.Delay(2000);
            Debug.WriteLine("getting ticker");
        }
    }
}

