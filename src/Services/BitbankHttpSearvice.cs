using BitWallpaper.Models;
using BitWallpaper.Services.Contracts;
using System.Text.Json;

namespace BitWallpaper.Services;

public class BitbankHttpSearvice : IBitbankHttpSearvice
{
    private readonly HttpClient? _client;

    private static readonly Uri _publicApiEndpoint = new("https://public.bitbank.cc");

    private readonly IHttpClientFactory _httpClientFactory;

    public BitbankHttpSearvice(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _client = _httpClientFactory.CreateClient("BitDeskX");
        _client.BaseAddress = _publicApiEndpoint;
    }

    public async Task<CandlestickResult?> GetCandlestick(string pair, string CandleType, string YYYYMMDDD)
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

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                //System.Diagnostics.Debug.WriteLine("GetCandlestick: " + json);

                //var deserialized = JsonConvert.DeserializeObject<JsonCandlestick>(json);
                var deserialized = JsonSerializer.Deserialize(json, CandlestickJsonSerializerContext.Default.JsonCandlestick);

                if (deserialized != null)
                {
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
                        //var jsonResult = JsonConvert.DeserializeObject<JsonErrorObject>(json);
                        var jsonResult = JsonSerializer.Deserialize(json, ErrorJsonSerializerContext.Default.JsonErrorObject);
                        System.Diagnostics.Debug.WriteLine("GetCandlestick: API error code - " + jsonResult?.Data?.Code.ToString());

                        // TODO
                        return null;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("GetCandlestick: API error. deserialized is null.");
                    //TODO
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
        return GetCandlestick(pairString, candleTypeString, yyyymmdd).ContinueWith(task =>
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
}
