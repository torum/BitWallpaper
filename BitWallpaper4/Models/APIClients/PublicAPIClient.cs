using System.Transactions;
using Newtonsoft.Json;
using BitWallpaper4.ViewModels;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using System.Diagnostics;

namespace BitWallpaper4.Models.APIClients;

public class PublicAPIClient : BaseClient
{
    private readonly Uri PublicAPIUri = new Uri("https://public.bitbank.cc");

    // コンストラクタ
    public PublicAPIClient()
    {
        _HTTPConn.Client.BaseAddress = PublicAPIUri;
    }

    // Ticker取得メソッド
    public async Task<Ticker> GetTicker(string pair)
    {
        Uri _endpoint = new Uri((_HTTPConn.Client.BaseAddress).ToString() + pair + "/ticker");

        //System.Diagnostics.Debug.WriteLine("GettingTicker..." + _endpoint.ToString());

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = _endpoint,
        };

        //System.Diagnostics.Debug.WriteLine("GettingTicker...");

        try
        {

            var response = await _HTTPConn.Client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string s = await response.Content.ReadAsStringAsync();
                //System.Diagnostics.Debug.WriteLine("GetTicker: " + s);

                var deserialized = JsonConvert.DeserializeObject<JsonTickerObject>(s);

                if (deserialized.success > 0)
                {

                    Ticker ticker = new Ticker();

                    ticker.LTP = decimal.Parse(deserialized.data.last);
                    ticker.Bid = decimal.Parse(deserialized.data.buy);
                    ticker.Ask = decimal.Parse(deserialized.data.sell);
                    ticker.Low = decimal.Parse(deserialized.data.low);
                    ticker.High = decimal.Parse(deserialized.data.high);

                    DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    DateTime date = start.AddMilliseconds(deserialized.data.timestamp).ToLocalTime();
                    ticker.TimeStamp = date;

                    return ticker;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("GetTicker: API returned failed response.");

                    return null;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("GetTicker: HTTP Error " + response.StatusCode.ToString());

                return null;
            }
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine("GetTicker: HttpRequestException - " + ex.Message + " + 内部例外: " + ex.InnerException.Message);

            return null;
        }
    }

    // 板情報取得メソッド
    public async Task<DepthResult> GetDepth(string pair)
    {
        Uri _endpoint = new Uri((_HTTPConn.Client.BaseAddress).ToString() + pair + "/depth");

        //System.Diagnostics.Debug.WriteLine("GettingDepth..." + _endpoint.ToString());

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = _endpoint,
        };

        try
        {
            var response = await _HTTPConn.Client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                //System.Diagnostics.Debug.WriteLine("GetTicker: " + s);

                var deserialized = JsonConvert.DeserializeObject<JsonDepthObject>(json);

                if (deserialized.Success > 0)
                {
                    DepthResult dpr = new DepthResult();


                    foreach (var dp in deserialized.Data.Bids)
                    {
                        if (dp.Count > 1)
                        {

                            Depth dd = new Depth();
                            dd.DepthPrice = Decimal.Parse(dp[0]);
                            dd.DepthBid = Decimal.Parse(dp[1]);
                            dd.DepthAsk = 0;
                            dpr.DepthBidList.Add(dd);

                        }
                    }

                    foreach (var dp in deserialized.Data.Asks)
                    {
                        if (dp.Count > 1)
                        {

                            Depth dd = new Depth();
                            dd.DepthPrice = Decimal.Parse(dp[0]);
                            dd.DepthBid = 0;
                            dd.DepthAsk = Decimal.Parse(dp[1]);
                            dpr.DepthAskList.Add(dd);

                        }
                    }

                    dpr.ErrorCode = 0;
                    dpr.IsSuccess = true;

                    return dpr;
                }
                else
                {
                    var jsonResult = JsonConvert.DeserializeObject<JsonErrorObject>(json);

                    System.Diagnostics.Debug.WriteLine("■■■■■ GetDepth: API error code - " + jsonResult.data.code.ToString() + " ■■■■■");

                    return null;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("■■■■■ GetDepth: HTTP Error " + response.StatusCode.ToString());

                return null;
            }
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine("■■■■■ GetDepth: HttpRequestException - " + ex.Message + " + 内部例外: " + ex.InnerException.Message);

            return null;
        }


    }

    // トランザクション(歩み値)取得メソッド
    public async Task<TransactionsResult> GetTransactions(string pair)
    {
        Uri _endpoint = new Uri((_HTTPConn.Client.BaseAddress).ToString() + pair + "/transactions");

        //System.Diagnostics.Debug.WriteLine("GettingDepth..." + _endpoint.ToString());

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = _endpoint,
        };

        try
        {
            var response = await _HTTPConn.Client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                //System.Diagnostics.Debug.WriteLine("GetTicker: " + s);

                var deserialized = JsonConvert.DeserializeObject<JsonTransactions>(json);

                if (deserialized.Success > 0)
                {
                    TransactionsResult trs = new TransactionsResult();

                    foreach (var tr in deserialized.Data.Transactions)
                    {
                        Transaction dd = new Transaction();
                        dd.TransactionId = tr.TransactionId;
                        dd.Side = tr.Side;
                        if (!string.IsNullOrEmpty(tr.Price)) dd.Price = decimal.Parse(tr.Price);
                        if (!string.IsNullOrEmpty(tr.Amount)) dd.Amount = decimal.Parse(tr.Amount);

                        dd.ExecutedAt = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds(tr.ExecutedAt).ToLocalTime();

                        trs.Trans.Add(dd);

                        //Debug.WriteLine(tr.ExecutedAt);
                    }

                    trs.ErrorCode = 0;
                    trs.IsSuccess = true;

                    return trs;
                }
                else
                {
                    var jsonResult = JsonConvert.DeserializeObject<JsonErrorObject>(json);

                    System.Diagnostics.Debug.WriteLine("■■■■■ GetTransactions: API error code - " + jsonResult.data.code.ToString() + " ■■■■■");

                    // TODO
                    return null;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("■■■■■ GetTransactions: HTTP Error " + response.StatusCode.ToString());

                //TODO
                return null;
            }
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine("■■■■■ GetTransactions: HttpRequestException - " + ex.Message + " + 内部例外: " + ex.InnerException.Message);

            //TODO
            return null;
        }


    }

    // ろうそく足取得メソッド
    public async Task<CandlestickResult> GetCandlestick(string pair, string CandleType, string YYYYMMDDD)
    {

        Uri _endpoint = new Uri((_HTTPConn.Client.BaseAddress).ToString() + pair + "/candlestick/" + CandleType + "/" + YYYYMMDDD);

        //Uri _endpoint = new Uri((_HTTPConn.Client.BaseAddress).ToString() + "btc_jpy/candlestick/1day/2018");

        //Uri _endpoint = new Uri((_HTTPConn.Client.BaseAddress).ToString() + "btc_jpy/candlestick/1hour/20180825");

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = _endpoint,
        };

        try
        {
            var response = await _HTTPConn.Client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                //System.Diagnostics.Debug.WriteLine("GetCandlestick: " + json);

                var deserialized = JsonConvert.DeserializeObject<JsonCandlestick>(json);

                if (deserialized.Success > 0)
                {
                    CandlestickResult csr = new CandlestickResult();

                    if (deserialized.Data.Candlestick.Count > 0)
                    {
                        Candlestick cs = new Candlestick();

                        if (deserialized.Data.Candlestick[0].Type == "1min")
                        {
                            cs.Type = CandleTypes.OneMin;
                        }
                        else if (deserialized.Data.Candlestick[0].Type == "5min")
                        {
                            cs.Type = CandleTypes.FiveMin;
                        }
                        else if (deserialized.Data.Candlestick[0].Type == "15min")
                        {
                            cs.Type = CandleTypes.FifteenMin;
                        }
                        else if (deserialized.Data.Candlestick[0].Type == "30min")
                        {
                            cs.Type = CandleTypes.ThirteenMin;
                        }
                        else if (deserialized.Data.Candlestick[0].Type == "1hour")
                        {
                            cs.Type = CandleTypes.OneHour;
                        }
                        else if (deserialized.Data.Candlestick[0].Type == "4hour")
                        {
                            cs.Type = CandleTypes.FourHour;
                        }
                        else if (deserialized.Data.Candlestick[0].Type == "8hour")
                        {
                            cs.Type = CandleTypes.EightHour;
                        }
                        else if (deserialized.Data.Candlestick[0].Type == "12hour")
                        {
                            cs.Type = CandleTypes.TwelveHour;
                        }
                        else if (deserialized.Data.Candlestick[0].Type == "1day")
                        {
                            cs.Type = CandleTypes.OneDay;
                        }
                        else if (deserialized.Data.Candlestick[0].Type == "1week")
                        {
                            cs.Type = CandleTypes.OneWeek;
                        }

                        if (deserialized.Data.Candlestick[0].Ohlcv.Count > 0)
                        {

                            foreach (var jcs in deserialized.Data.Candlestick[0].Ohlcv)
                            {

                                Ohlcv oh = new Ohlcv();

                                if (jcs.Count >= 6)
                                {
                                    oh.Open = decimal.Parse(jcs[0].String);
                                    oh.High = decimal.Parse(jcs[1].String);
                                    oh.Low = decimal.Parse(jcs[2].String);
                                    oh.Close = decimal.Parse(jcs[3].String);
                                    oh.Volume = decimal.Parse(jcs[4].String);
                                    //oh.TimeStamp = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)).AddMilliseconds((double)jcs[5].Double);
                                    oh.TimeStamp = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((double)jcs[5].Long).ToLocalTime();
                                    //oh.TimeStamp = new DateTime(2022,1,1);

                                }

                                //System.Diagnostics.Debug.WriteLine("GetCandlestick: " + oh.TimeStamp.ToString("dd日 hh:mm:ss"));
                                //System.Diagnostics.Debug.WriteLine(jcs[4].String);

                                cs.Ohlcvs.Add(oh);

                            }
                            csr.Candlesticks.Add(cs);

                        }

                    }

                    csr.ErrorCode = 0;
                    csr.IsSuccess = true;

                    return csr;
                }
                else
                {
                    var jsonResult = JsonConvert.DeserializeObject<JsonErrorObject>(json);

                    System.Diagnostics.Debug.WriteLine("■■■■■ GetCandlestick: API error code - " + jsonResult.data.code.ToString() + " ■■■■■");

                    // TODO
                    return null;
                }
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine("■■■■■ GetCandlestick: HTTP Error " + response.StatusCode.ToString());
                System.Diagnostics.Debug.WriteLine("■■■■■ GetCandlestick: HTTP Error" + response.StatusCode.ToString() + " URL: " + _endpoint.ToString());

                //TODO
                return null;
            }
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine("■■■■■ GetCandlestick: HttpRequestException - " + ex.Message + " + 内部例外: " + ex.InnerException.Message);

            //TODO
            return null;
        }

    }

}


