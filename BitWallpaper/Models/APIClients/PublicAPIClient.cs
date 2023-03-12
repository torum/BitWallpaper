using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace BitWallpaper.Models.APIClients;

public class PublicAPIClient : BaseClient
{
    private readonly Uri PublicAPIUri = new("https://public.bitbank.cc");

    // コンストラクタ
    public PublicAPIClient()
    {
        Client.BaseAddress = PublicAPIUri;

        Client.DefaultRequestHeaders.Clear();
        Client.DefaultRequestHeaders.ConnectionClose = false;
        //Client.DefaultRequestHeaders.ConnectionClose = true;
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    // Ticker取得メソッド
    public static async Task<Ticker?> GetTicker(string pair)
    {
        if (Client is null) return null;
        if (Client.BaseAddress is null) return null;

        Uri _endpoint = new(Client.BaseAddress.ToString() + pair + "/ticker");

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

            if (deserialized.Success <= 0)
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
                if (deserialized.Data is not null)
                {
                    if (deserialized.Data.Last is not null)
                    {
                        ticker.LTP = decimal.Parse(deserialized.Data.Last);
                    }
                    if (deserialized.Data.Buy is not null)
                    {
                        ticker.Bid = decimal.Parse(deserialized.Data.Buy);
                    }
                    if (deserialized.Data.Sell is not null)
                    {
                        ticker.Ask = decimal.Parse(deserialized.Data.Sell);
                    }
                    if (deserialized.Data.Low is not null)
                    {
                        ticker.Low = decimal.Parse(deserialized.Data.Low);
                    }
                    if (deserialized.Data.High is not null)
                    {
                        ticker.High = decimal.Parse(deserialized.Data.High);
                    }

                    DateTime start = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    DateTime date = start.AddMilliseconds(deserialized.Data.Timestamp).ToLocalTime();
                    ticker.TimeStamp = date;

                    return ticker;

                }
                else
                {
                    Debug.WriteLine("GetTicker: Data is null.");
                    return null;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetTicker: Exception - " + ex.Message);
                return null;
            }
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine("GetTicker: HttpRequestException - " + ex.Message + " + 内部例外: " + ex.InnerException?.Message);
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GetTicker: Exception - " + ex.Message);
            return null;
        }
    }

    // 板情報取得メソッド
    public static async Task<DepthResult?> GetDepth(string pair)
    {
        if (Client is null) return null;
        if (Client.BaseAddress is null) return null;

        Uri _endpoint = new(Client.BaseAddress.ToString() + pair + "/depth");

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
                    if (deserialized.Data is not null)
                    {
                        DepthResult dpr = new();

                        if (deserialized.Data.Bids is not null)
                        {
                            foreach (var dp in deserialized.Data.Bids)
                            {
                                if (dp.Count > 1)
                                {
                                    Depth dd = new()
                                    {
                                        DepthPrice = Decimal.Parse(dp[0]),
                                        DepthBid = Decimal.Parse(dp[1]),
                                        DepthAsk = 0
                                    };
                                    dpr.DepthBidList.Add(dd);
                                }
                            }
                        }

                        if (deserialized.Data.Asks is not null)
                        {
                            foreach (var dp in deserialized.Data.Asks)
                            {
                                if (dp.Count > 1)
                                {
                                    Depth dd = new()
                                    {
                                        DepthPrice = Decimal.Parse(dp[0]),
                                        DepthBid = 0,
                                        DepthAsk = Decimal.Parse(dp[1])
                                    };
                                    dpr.DepthAskList.Add(dd);
                                }
                            }
                        }

                        dpr.ErrorCode = 0;
                        dpr.IsSuccess = true;

                        return dpr;
                    }
                    else
                    {
                        Debug.WriteLine("GetDepth: API error. Data is null.");
                        return null;
                    }
                }
                else
                {
                    var jsonResult = JsonConvert.DeserializeObject<JsonErrorObject>(json);
                    if (jsonResult.Data is not null)
                    {
                        Debug.WriteLine("GetDepth: API error code - " + jsonResult.Data.Code.ToString());
                    }
                    else
                    {
                        Debug.WriteLine("GetDepth: API error. (No error code given.)");
                    }
                    return null;
                }
            }
            else
            {
                Debug.WriteLine("GetDepth: HTTP Error " + response.StatusCode.ToString());
                return null;
            }
        }
        catch (HttpRequestException ex)
        {
            if (ex.InnerException is not null)
            {
                Debug.WriteLine("GetDepth: HttpRequestException - " + ex.Message + " + InnerException: " + ex.InnerException.Message);
            }
            else
            {
                Debug.WriteLine("GetDepth: HttpRequestException - " + ex.Message);
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GetDepth: Exception - " + ex.Message);
            return null;
        }
    }

    // トランザクション(歩み値)取得メソッド
    public static async Task<TransactionsResult?> GetTransactions(string pair)
    {
        if (Client is null) return null;
        if (Client.BaseAddress is null) return null;

        Uri _endpoint = new((Client.BaseAddress).ToString() + pair + "/transactions");

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
                    if (deserialized.Data?.Transactions is not null)
                    {
                        TransactionsResult trs = new();

                        foreach (var tr in deserialized.Data.Transactions)
                        {
                            BitWallpaper.Models.Transaction dd = new()
                            {
                                TransactionId = tr.TransactionId
                            };
                            if (tr.Side is not null)
                            {
                                dd.Side = tr.Side;
                            }
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
                        Debug.WriteLine("GetTransactions: API error: deserialized.Data?.Transactions is null.");

                        // TODO
                        return null;
                    }
                }
                else
                {
                    var jsonResult = JsonConvert.DeserializeObject<JsonErrorObject>(json);
                    if (jsonResult.Data is not null)
                    {
                        Debug.WriteLine("GetTransactions: API error code - " + jsonResult.Data.Code.ToString());
                    }
                    else
                    {
                        Debug.WriteLine("GetTransactions: API error. No error code given.");
                    }

                    // TODO
                    return null;
                }
            }
            else
            {
                Debug.WriteLine("GetTransactions: HTTP Error " + response.StatusCode.ToString());

                //TODO
                return null;
            }
        }
        catch (HttpRequestException ex)
        {
            if (ex.InnerException is not null)
            {
                Debug.WriteLine("GetTransactions: HttpRequestException - " + ex.Message + " + InnerException: " + ex.InnerException.Message);
            }
            else
            {
                Debug.WriteLine("GetTransactions: HttpRequestException - " + ex.Message);
            }

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
    public static async Task<CandlestickResult?> GetCandlestick(string pair, string CandleType, string YYYYMMDDD)
    {
        if (Client is null) return null;
        if (Client.BaseAddress is null) return null;

        Uri _endpoint = new(Client.BaseAddress.ToString() + pair + "/candlestick/" + CandleType + "/" + YYYYMMDDD);

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
                    if (deserialized.Data is not null)
                    {
                        if (deserialized.Data.Candlestick is not null)
                        {
                            CandlestickResult csr = new();

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

                                if (deserialized.Data.Candlestick[0].Ohlcv is not null)
                                {
                                    if (deserialized.Data.Candlestick[0]?.Ohlcv?.Count > 0)
                                    {
                                        foreach (var (jcs, oh) in from jcs in deserialized.Data.Candlestick[0].Ohlcv
                                                                  let oh = new Ohlcv()
                                                                  select (jcs, oh))
                                        {
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
                                    }
                                }
                            }

                            csr.ErrorCode = 0;
                            csr.IsSuccess = true;

                            return csr;
                        }
                        else
                        {

                            System.Diagnostics.Debug.WriteLine("GetCandlestick: API error. deserialized.Data.Candlestick is null.");

                            // TODO
                            return null;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("GetCandlestick: API error. Data is null.");

                        // TODO
                        return null;
                    }
                }
                else
                {
                    var jsonResult = JsonConvert.DeserializeObject<JsonErrorObject>(json);
                    System.Diagnostics.Debug.WriteLine("GetCandlestick: API error code - " + jsonResult.Data?.Code.ToString());

                    // TODO
                    return null;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("GetCandlestick: HTTP Error" + response.StatusCode.ToString() + " URL: " + _endpoint.ToString());

                //TODO
                return null;
            }
        }
        catch (HttpRequestException ex)
        {
            if (ex.InnerException is not null)
            {
                System.Diagnostics.Debug.WriteLine("GetCandlestick: HttpRequestException - " + ex.Message + " + InnerException: " + ex.InnerException.Message);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("GetCandlestick: HttpRequestException - " + ex.Message);
            }

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


