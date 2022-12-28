using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using BitWallpaper4.Models;

namespace BitWallpaper4.Models.APIClients;

public class PublicAPIClient : BaseClient
{
    private readonly Uri PublicAPIUri = new Uri("https://public.bitbank.cc");

    // コンストラクタ
    public PublicAPIClient()
    {
        Client.BaseAddress = PublicAPIUri;
    }

    // Ticker取得メソッド
    public async Task<Ticker> GetTicker(string pair)
    {
        Uri _endpoint = new Uri((Client.BaseAddress).ToString() + pair + "/ticker");

        var request = new HttpRequestMessage {Method = HttpMethod.Get, RequestUri = _endpoint};

        try
        {
            var response = await Client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine("GetTicker: HTTP Error " + response.StatusCode.ToString());
                return null;
            }

            string s = await response.Content.ReadAsStringAsync();

            var deserialized = JsonConvert.DeserializeObject<JsonTickerObject>(s);

            if (deserialized.success <= 0)
            {
                Debug.WriteLine("GetTicker: API returned failed response.");
                return null;
                /*
                    // 
                    ClientError er = new ClientError();
                    er.ErrType = "API";
                    er.ErrCode = jsonResult.data.code;
                    if (ApiErrorCodesDictionary.ContainsKey(jsonResult.data.code))
                    {
                        er.ErrText = "「" + ApiErrorCodesDictionary[jsonResult.data.code] + "」";
                    }
                    er.ErrDatetime = DateTime.Now;
                    er.ErrPlace = path.ToString();

                    ErrorOccured?.Invoke(this, er);

                    return null; 
                */
            }

            try
            {
                Ticker ticker = new();

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
            catch (Exception ex)
            {
                Debug.WriteLine("GetTicker: Exception - " + ex.Message);
                return null;
            }
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine("GetTicker: HttpRequestException - " + ex.Message + " + 内部例外: " + ex.InnerException.Message);
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GetTicker: Exception - " + ex.Message);
            return null;
        }
    }

    // 板情報取得メソッド
    public async Task<DepthResult> GetDepth(string pair)
    {
        Uri _endpoint = new Uri((Client.BaseAddress).ToString() + pair + "/depth");

        //System.Diagnostics.Debug.WriteLine("GettingDepth..." + _endpoint.ToString());

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = _endpoint,
        };

        try
        {
            var response = await Client.SendAsync(request);

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

                    Debug.WriteLine("■■■■■ GetDepth: API error code - " + jsonResult.data.code.ToString() + " ■■■■■");

                    return null;
                }
            }
            else
            {
                Debug.WriteLine("■■■■■ GetDepth: HTTP Error " + response.StatusCode.ToString());

                return null;
            }
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine("■■■■■ GetDepth: HttpRequestException - " + ex.Message + " + 内部例外: " + ex.InnerException.Message);

            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GetDepth: Exception - " + ex.Message);
            return null;
        }
    }

    // トランザクション(歩み値)取得メソッド
    public async Task<TransactionsResult> GetTransactions(string pair)
    {
        Uri _endpoint = new Uri((Client.BaseAddress).ToString() + pair + "/transactions");

        //System.Diagnostics.Debug.WriteLine("GettingDepth..." + _endpoint.ToString());

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = _endpoint,
        };

        try
        {
            var response = await Client.SendAsync(request);

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
                        Transaction dd = new BitWallpaper4.Models.Transaction();
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

                    Debug.WriteLine("■■■■■ GetTransactions: API error code - " + jsonResult.data.code.ToString() + " ■■■■■");

                    // TODO
                    return null;
                }
            }
            else
            {
                Debug.WriteLine("■■■■■ GetTransactions: HTTP Error " + response.StatusCode.ToString());

                //TODO
                return null;
            }
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine("■■■■■ GetTransactions: HttpRequestException - " + ex.Message + " + 内部例外: " + ex.InnerException.Message);

            //TODO
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GetTransactions: Exception - " + ex.Message);
            return null;
        }
    }

    // ろうそく足取得メソッド
    public async Task<CandlestickResult> GetCandlestick(string pair, string CandleType, string YYYYMMDDD)
    {
        Uri _endpoint = new Uri((Client.BaseAddress).ToString() + pair + "/candlestick/" + CandleType + "/" + YYYYMMDDD);

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = _endpoint,
        };

        try
        {
            var response = await Client.SendAsync(request);

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
                        //Candlestick cs = new Candlestick();

                        if (deserialized.Data.Candlestick[0].Type == "1min")
                        {
                            csr.CandleType = CandleTypes.OneMin;
                        }
                        else if (deserialized.Data.Candlestick[0].Type == "5min")
                        {
                            csr.CandleType = CandleTypes.FiveMin;
                        }
                        else if (deserialized.Data.Candlestick[0].Type == "15min")
                        {
                            csr.CandleType = CandleTypes.FifteenMin;
                        }
                        else if (deserialized.Data.Candlestick[0].Type == "30min")
                        {
                            csr.CandleType = CandleTypes.ThirtyMin;
                        }
                        else if (deserialized.Data.Candlestick[0].Type == "1hour")
                        {
                            csr.CandleType = CandleTypes.OneHour;
                        }
                        else if (deserialized.Data.Candlestick[0].Type == "4hour")
                        {
                            csr.CandleType = CandleTypes.FourHour;
                        }
                        else if (deserialized.Data.Candlestick[0].Type == "8hour")
                        {
                            csr.CandleType = CandleTypes.EightHour;
                        }
                        else if (deserialized.Data.Candlestick[0].Type == "12hour")
                        {
                            csr.CandleType = CandleTypes.TwelveHour;
                        }
                        else if (deserialized.Data.Candlestick[0].Type == "1day")
                        {
                            csr.CandleType = CandleTypes.OneDay;
                        }
                        else if (deserialized.Data.Candlestick[0].Type == "1week")
                        {
                            csr.CandleType = CandleTypes.OneWeek;
                        }
                        else if (deserialized.Data.Candlestick[0].Type == "1month")
                        {
                            csr.CandleType = CandleTypes.OneMonth;
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

                                csr.Candlesticks.Add(oh);

                            }
                            //csr.Candlesticks = cs.Ohlcvs;

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
        catch (Exception ex)
        {
            Debug.WriteLine("GetCandlestick: Exception - " + ex.Message);
            return null;
        }
    }
}


