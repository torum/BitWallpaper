using BitWallpaper.Models;
using BitWallpaper.Services.Contracts;
using Microsoft.UI.Xaml.Controls;
using System.Text.Json;

namespace BitWallpaper.Services;

public class BitbankHttpService : IBitbankHttpService
{
    private readonly HttpClient? _client;

    private static readonly Uri _publicApiEndpoint = new("https://public.bitbank.cc");

    private readonly IHttpClientFactory _httpClientFactory;

    public event EventHandler<ErrorInfo>? ErrorOccured;

    public BitbankHttpService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _client = _httpClientFactory.CreateClient("BitDeskX");
        _client.BaseAddress = _publicApiEndpoint;
    }

    public async Task<CandlestickResult?> GetCandlestickAsync(string pair, string CandleType, string YYYYMMDDD)
    {
        if (_client is null)
        {
            return null;
        }

        if (_client.BaseAddress is null)
        {
            return null;
        }

        Uri _endpoint = new(_client.BaseAddress.ToString() + pair + "/candlestick/" + CandleType + "/" + YYYYMMDDD);

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = _endpoint,
        };

        try
        {
            var response = await _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine("GetCandlestick: HTTP Error" + response.StatusCode.ToString() + " URL: " + _endpoint.ToString());

                //TODO
                return null;
            }
            var json = await response.Content.ReadAsStringAsync();
            //System.Diagnostics.Debug.WriteLine("GetCandlestick: " + json);

            //var deserialized = JsonConvert.DeserializeObject<JsonCandlestick>(json);
            var deserialized = JsonSerializer.Deserialize(json, CandlestickJsonSerializerContext.Default.JsonCandlestick);

            if (deserialized == null)
            {
                System.Diagnostics.Debug.WriteLine("GetCandlestick: API error. deserialized is null.");
                //TODO
                return null;
            }
            if (!(deserialized.Success > 0))
            {
                //var jsonResult = JsonConvert.DeserializeObject<JsonErrorObject>(json);
                var jsonResult = JsonSerializer.Deserialize(json, ErrorJsonSerializerContext.Default.JsonErrorObject);
                System.Diagnostics.Debug.WriteLine("GetCandlestick: API error code - " + jsonResult?.Data?.Code.ToString());

                // TODO
                return null;
            }
            if (deserialized.Data is null)
            {
                System.Diagnostics.Debug.WriteLine("GetCandlestick: API error. Data is null.");

                // TODO
                return null;
            }
            if (deserialized.Data.Candlestick is null)
            {

                System.Diagnostics.Debug.WriteLine("GetCandlestick: API error. deserialized.Data.Candlestick is null.");

                // TODO
                return null;
            }

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

                if (deserialized?.Data?.Candlestick != null)
                {
                    foreach (var candlestick in deserialized.Data.Candlestick)
                    {
                        if (candlestick.Ohlcv != null)
                        {
                            foreach (var ohlcv in candlestick.Ohlcv)
                            {
                                //Debug.WriteLine($"  Open: {ohlcv.Open}, High: {ohlcv.High}, Low: {ohlcv.Low}, Close: {ohlcv.Close}, Volume: {ohlcv.Volume}, Timestamp: {ohlcv.Timestamp}");

                                var oh = new Ohlcv();
                                if (decimal.TryParse(ohlcv.Open, out var decimalOpen))
                                {
                                    oh.Open = decimalOpen;
                                }
                                if (decimal.TryParse(ohlcv.High, out var decimalHigh))
                                {
                                    oh.High = decimalHigh;
                                }
                                if (decimal.TryParse(ohlcv.Low, out var decimalLow))
                                {
                                    oh.Low = decimalLow;
                                }
                                if (decimal.TryParse(ohlcv.Close, out var decimalClose))
                                {
                                    oh.Close = decimalClose;
                                }
                                if (decimal.TryParse(ohlcv.Volume, out var decimalVolume))
                                {
                                    oh.Volume = decimalVolume;
                                }
                                oh.TimeStamp = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((double)ohlcv.Timestamp).ToLocalTime();

                                csr.Candlesticks.Add(oh);
                            }
                        }
                    }
                }
            }

            csr.ErrorCode = 0;
            csr.IsSuccess = true;

            return csr;
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

            var errorInfo = new ErrorInfo(ErrType.ConnectionError)
            {
                ErrorMessage = $"{ex.Message} @GetCandlestickAsync.",
                ErrorSeverity = InfoBarSeverity.Warning
            };

            ErrorOccured?.Invoke(this, errorInfo);

            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GetCandlestick: Exception - " + ex.Message);

            var errorInfo = new ErrorInfo(ErrType.Unexpected)
            {
                ErrorMessage = $"{ex.Message} ({ex.GetType().FullName}) @GetCandlestickAsync.",
                ErrorSeverity = InfoBarSeverity.Warning
            };
            ErrorOccured?.Invoke(this, errorInfo);

            return null;
        }

    }

    public Task<List<Ohlcv>?> GetLatestCandlesticksAsync(PairCodes pair, CandleTypes ct)
    {
        string pairString = pair.ToString().ToLower();
        string candleTypeString = ct switch
        {
            CandleTypes.OneMin => "1min",
            CandleTypes.FiveMin => "5min",
            CandleTypes.FifteenMin => "15min",
            CandleTypes.ThirtyMin => "30min",
            CandleTypes.OneHour => "1hour",
            CandleTypes.FourHour => "4hour",
            CandleTypes.EightHour => "8hour",
            CandleTypes.TwelveHour => "12hour",
            CandleTypes.OneDay => "1day",
            CandleTypes.OneWeek => "1week",
            CandleTypes.OneMonth => "1month",
            _ => throw new ArgumentOutOfRangeException(nameof(ct), ct, null)
        };
        string yyyymmdd = DateTime.UtcNow.ToString("yyyyMMdd");
        return GetCandlestickAsync(pairString, candleTypeString, yyyymmdd).ContinueWith(task =>
        {
            if (task.Result != null && task.Result.IsSuccess)
            {
                return task.Result.Candlesticks;
            }
            else
            {
                return null;
            }
        });
    }

    public async Task<DepthResult?> GetDepthAsync(string pair)
    {
        if (_client is null)
        {
            return null;
        }

        if (_client.BaseAddress is null)
        {
            return null;
        }

        Uri _endpoint = new(_client.BaseAddress.ToString() + pair + "/depth");

        //System.Diagnostics.Debug.WriteLine("GettingDepth..." + _endpoint.ToString());

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = _endpoint,
        };

        try
        {
            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                //System.Diagnostics.Debug.WriteLine("GetTicker: " + s);

                //var deserialized = JsonConvert.DeserializeObject<JsonDepthObject>(json);
                var deserialized = JsonSerializer.Deserialize(json, DepthJsonSerializerContext.Default.JsonDepthObject);

                if (deserialized == null)
                {
                    return null;
                }

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
                                        DepthPrice = decimal.Parse(dp[0]),
                                        DepthBid = decimal.Parse(dp[1]),
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
                                        DepthPrice = decimal.Parse(dp[0]),
                                        DepthBid = 0,
                                        DepthAsk = decimal.Parse(dp[1])
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
                    //var jsonResult = JsonConvert.DeserializeObject<JsonErrorObject>(json);
                    var jsonResult = JsonSerializer.Deserialize(json, ErrorJsonSerializerContext.Default.JsonErrorObject);

                    if (jsonResult?.Data is not null)
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

    public async Task<TransactionsResult?> GetTransactionsAsync(string pair)
    {
        if (_client is null)
        {
            return null;
        }

        if (_client.BaseAddress is null)
        {
            return null;
        }

        Uri _endpoint = new((_client.BaseAddress).ToString() + pair + "/transactions");

        //System.Diagnostics.Debug.WriteLine("GettingDepth..." + _endpoint.ToString());

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = _endpoint,
        };

        try
        {
            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                //System.Diagnostics.Debug.WriteLine("GetTicker: " + s);

                //var deserialized = JsonConvert.DeserializeObject<JsonTransactions>(json);
                var deserialized = JsonSerializer.Deserialize(json, TransactionsJsonSerializerContext.Default.JsonTransactions);

                if (deserialized?.Success > 0)
                {
                    if (deserialized.Data?.Transactions is not null)
                    {
                        TransactionsResult trs = new();

                        foreach (var tr in deserialized.Data.Transactions)
                        {
                            Transaction dd = new()
                            {
                                TransactionId = tr.TransactionId
                            };
                            if (tr.Side is not null)
                            {
                                dd.Side = tr.Side;
                            }
                            if (!string.IsNullOrEmpty(tr.Price))
                            {
                                dd.Price = decimal.Parse(tr.Price);
                            }

                            if (!string.IsNullOrEmpty(tr.Amount))
                            {
                                dd.Amount = decimal.Parse(tr.Amount);
                            }

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
                    //var jsonResult = JsonConvert.DeserializeObject<JsonErrorObject>(json);
                    var jsonResult = JsonSerializer.Deserialize(json, ErrorJsonSerializerContext.Default.JsonErrorObject);

                    if (jsonResult?.Data is not null)
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
}
