﻿using System;
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
using LiveChartsCore.Themes;
using Newtonsoft.Json.Linq;

namespace BitWallpaper4.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    //
    private ObservableCollection<Pair> _pairs = new() 
    {
        new Pair(PairCodes.btc_jpy, 24, "{0:#,0}", "C", 100M, 1000M),
        new Pair(PairCodes.xrp_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
        new Pair(PairCodes.eth_jpy, 24, "{0:#,0}", "C", 100M, 1000M),
        new Pair(PairCodes.ltc_jpy, 24, "{0:#,0.0}", "C", 100M, 1000M),
        new Pair(PairCodes.mona_jpy, 24, "{0:#,0.000}", "C", 0.1M, 1M),
        new Pair(PairCodes.bcc_jpy, 24, "{0:#,0}", "C", 10M, 100M),
        new Pair(PairCodes.xlm_jpy, 24, "{0:#,0.000}", "C", 0.1M, 0.01M),
        new Pair(PairCodes.qtum_jpy, 24, "{0:#,0.000}", "C", 0.1M, 0.01M),
        new Pair(PairCodes.bat_jpy, 24, "{0:#,0.000}", "C", 0.1M, 0.01M),
    };
    public ObservableCollection<Pair> Pairs
    {
        get => _pairs;
        set
        {
            if (_pairs == value) return;

            _pairs = value;
            NotifyPropertyChanged(nameof(Pairs));
        }
    }

    private Pair _selectedPair;
    public Pair SelectedPair
    {
        get => _selectedPair;
        set
        {
            if (_selectedPair == value) return;

            _selectedPair = value;

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

            Debug.WriteLine("SelectedPair "+_selectedPair.PairCode);
            NotifyPropertyChanged(nameof(SelectedPair));
        }
    }

    //
    public PairCodes SetSelectedPairFromCode
    {
        set
        {
            Debug.WriteLine("SetSelectedPairFromCode " + value.ToString());

            if (_selectedPair?.PairCode == value) return;

            var hoge = Pairs.FirstOrDefault(x => x.PairCode == value);
            SelectedPair = hoge;

            NotifyPropertyChanged(nameof(SelectedPair));
        }
    }

    //
    private string _ltpJpyBtc = "...";
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

    private string _ltpXrpJpy = "...";
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

    // HTTP Clients
    readonly PublicAPIClient _pubTickerApi = new();

    // Timer
    readonly DispatcherTimer _dispatcherTimerTickAllPairs = new();
    //readonly DispatcherTimer _dispatcherChartTimer = new();

    public MainViewModel()
    {
        #region == チャートのイニシャライズ ==
        // TODO:
        #endregion


        // Ticker update timer
        _dispatcherTimerTickAllPairs.Tick += TickerTimerAllPairs;
        _dispatcherTimerTickAllPairs.Interval = new TimeSpan(0, 0, 1);
        _dispatcherTimerTickAllPairs.Start();

        /*
        // Chart update timer
        _dispatcherChartTimer.Tick += ChartTimer;
        _dispatcherChartTimer.Interval = new TimeSpan(0, 1, 0);
        */

        // temp
        //DisplayCharts(CandleTypes.OneHour);

    }

    private async void TickerTimerAllPairs(object? source, object e)
    {
        // 起動直後アラームを鳴らさない秒数
        int waitTime = 10;

        foreach (var hoge in Pairs)
        {
            Ticker tick = await _pubTickerApi.GetTicker(hoge.PairCode.ToString());

            if (tick != null)
            {

                //hoge.Ltp = tick.LTP;
                //hoge.TickTimeStamp = tick.TimeStamp;

                // more?

                // 一旦前の値を保存
                var prevLtp = hoge.Ltp;

                if (tick.LTP > prevLtp)
                {
                    hoge.LtpUpFlag = true;
                }
                else if (tick.LTP < prevLtp)
                {
                    hoge.LtpUpFlag = false;
                }


                // 最新の価格をセット
                if (hoge.PairCode == PairCodes.btc_jpy)
                {
                    LtpBtcJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                }
                else if (hoge.PairCode == PairCodes.xrp_jpy)
                {
                    LtpXrpJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                }
                //todo more





                // 最新の価格をセット
                hoge.Ltp = tick.LTP;
                hoge.Bid = tick.Bid;
                hoge.Ask = tick.Ask;
                hoge.TickTimeStamp = tick.TimeStamp;

                hoge.LowestIn24Price = tick.Low;
                hoge.HighestIn24Price = tick.High;

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

                #region == チック履歴 ==

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

                #endregion

                #region == アラーム ==

                bool isPlayed = false;

                // カスタムアラーム
                if (hoge.AlarmPlus > 0)
                {
                    if (tick.LTP >= hoge.AlarmPlus)
                    {
                        hoge.HighLowInfoTextColorFlag = true;
                        hoge.HighLowInfoText = hoge.PairString + " ⇑⇑⇑　高値アラーム ";
                        /*
                        ShowBalloonEventArgs ag = new ShowBalloonEventArgs
                        {
                            Title = PairBtcJpy.PairString + " 高値アラーム",
                            Text = PairBtcJpy.AlarmPlus.ToString("#,0") + " に達しました。"
                        };
                        // バルーン表示
                        ShowBalloon?.Invoke(this, ag);
                        */
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
                        /*
                        ShowBalloonEventArgs ag = new ShowBalloonEventArgs
                        {
                            Title = hoge.PairString + " 安値アラーム",
                            Text = hoge.AlarmMinus.ToString("#,0") + " に達しました。"
                        };
                        // バルーン表示
                        ShowBalloon?.Invoke(this, ag);
                        */
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

                            /*
                            if (PlaySound)
                            {
                                SystemSounds.Hand.Play();
                                isPlayed = true;
                            }
                            */
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
                            /*
                            if (PlaySound)
                            {
                                SystemSounds.Beep.Play();
                                isPlayed = true;
                            }
                            */
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
                        /*
                        if (PlaySound)
                        {
                            SystemSounds.Hand.Play();
                            isPlayed = true;
                        }
                        */
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
                        /*
                        if (PlaySound)
                        {
                            SystemSounds.Hand.Play();
                            isPlayed = true;
                        }
                        */
                    }
                }
                else
                {
                    hoge.LowestIn24PriceAlart = false;
                }

                #endregion

                }
                else
            {
                // TODO:
                //APIResultTicker = "<<取得失敗>>";
                System.Diagnostics.Debug.WriteLine("■■■■■ TickerTimerAllPairs: GetTicker returned null");
                break;
            }

            await Task.Delay(3);
        }
    }



}

