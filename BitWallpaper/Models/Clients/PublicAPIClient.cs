using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;
using BitWallpaper.ViewModels;


namespace BitWallpaper.Models.Clients
{

    public class JsonErrorData
    {
        public int code { get; set; }
        public string ja { get; set; }
    }

    public class JsonErrorObject
    {
        public int success { get; set; }
        public JsonErrorData data { get; set; }
    }

    public partial class JsonDepthObject
    {
        [JsonProperty("success")]
        public long Success { get; set; }

        [JsonProperty("data")]
        public JsonDepthData Data { get; set; }
    }

    public partial class JsonDepthData
    {
        [JsonProperty("asks")]
        public List<List<string>> Asks { get; set; }

        [JsonProperty("bids")]
        public List<List<string>> Bids { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
    }


    #region == JsonTickerClass Json デシリアライズ用 ==

    // Ticker
    public class JsonTickerData
    {
        public string sell { get; set; }
        public string buy { get; set; }
        public string high { get; set; }
        public string low { get; set; }
        public string last { get; set; }
        public string vol { get; set; }
        public long timestamp { get; set; }
    }

    public class JsonTickerObject
    {
        public int success { get; set; }
        public JsonTickerData data { get; set; }
    }

    // Transaction
    public partial class JsonTransactions
    {
        [JsonProperty("success")]
        public long Success { get; set; }

        [JsonProperty("data")]
        public JsonData Data { get; set; }
    }

    public partial class JsonData
    {
        [JsonProperty("transactions")]
        public List<JsonTransaction> Transactions { get; set; }
    }

    public partial class JsonTransaction
    {
        [JsonProperty("transaction_id")]
        public long TransactionId { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("executed_at")]
        public long ExecutedAt { get; set; }
    }


    // Candlestick
    public partial class JsonCandlestick
    {
        [JsonProperty("success")]
        public long Success { get; set; }

        [JsonProperty("data")]
        public JsonCandlestickData Data { get; set; }
    }

    public partial class JsonCandlestickData
    {
        [JsonProperty("candlestick")]
        public List<JsonCandlestickElement> Candlestick { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
    }

    public partial class JsonCandlestickElement
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("ohlcv")]
        public List<List<JsonOhlcv>> Ohlcv { get; set; }
    }

    public partial struct JsonOhlcv
    {
        public long? Integer;
        public string String;

        public static implicit operator JsonOhlcv(long Integer) => new JsonOhlcv { Integer = Integer };
        public static implicit operator JsonOhlcv(string String) => new JsonOhlcv { String = String };
    }


    #endregion

    #region == クラス定義 ==

    #region == ティック ==

    class Ticker
    {
        private decimal _ltp;
        public decimal LTP
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

            }
        }

        private DateTime _timestamp;
        public DateTime TimeStamp
        {
            get
            {
                return _timestamp;
            }
            set
            {
                if (_timestamp == value)
                    return;

                _timestamp = value;

            }
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

            }
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

            }
        }

        //private decimal _high;
        public decimal High { get; set; }

        //private decimal _low;
        public decimal Low { get; set; }

        public Ticker()
        {

        }

    }

    #endregion

    #region == 板 ==

    public class Depth : ViewModelBase
    {
        private decimal _depthBid;
        public decimal DepthBid
        {
            get
            {
                return _depthBid;
            }
            set
            {
                if (_depthBid == value)
                    return;

                _depthBid = value;
                this.NotifyPropertyChanged("DepthBid");

            }
        }
        private decimal _depthPrice;
        public decimal DepthPrice
        {
            get
            {
                return _depthPrice;
            }
            set
            {
                if (_depthPrice == value)
                    return;

                _depthPrice = value;
                this.NotifyPropertyChanged("DepthPrice");

            }
        }
        private decimal _depthAsk;
        public decimal DepthAsk
        {
            get
            {
                return _depthAsk;
            }
            set
            {
                if (_depthAsk == value)
                    return;

                _depthAsk = value;
                this.NotifyPropertyChanged("DepthAsk");

            }
        }
        private bool _isLTP;
        public bool IsLTP
        {
            get
            {
                return _isLTP;
            }
            set
            {
                if (_isLTP == value)
                    return;

                _isLTP = value;
                this.NotifyPropertyChanged("IsLTP");

            }
        }

        private bool _isAskBest;
        public bool IsAskBest
        {
            get
            {
                return _isAskBest;
            }
            set
            {
                if (_isAskBest == value)
                    return;

                _isAskBest = value;

                this.NotifyPropertyChanged("IsAskBest");
            }
        }
        private bool _isBidBest;
        public bool IsBidBest
        {
            get
            {
                return _isBidBest;
            }
            set
            {
                if (_isBidBest == value)
                    return;

                _isBidBest = value;
                this.NotifyPropertyChanged("IsBidBest");

            }
        }

        public Depth()
        {

        }

    }

    public class DepthResult
    {
        public bool IsSuccess { get; set; }
        public int ErrorCode { get; set; }

        public List<Depth> DepthAskList;
        public List<Depth> DepthBidList;

        public DepthResult()
        {
            DepthAskList = new List<Depth>();
            DepthBidList = new List<Depth>();
        }
    }

    #endregion

    #region == 取引履歴 ==

    public class Transactions : ViewModelBase
    {
        private long _transactionId;
        public long TransactionId
        {
            get
            {
                return _transactionId;
            }
            set
            {
                if (_transactionId == value)
                    return;

                _transactionId = value;
                this.NotifyPropertyChanged("TransactionId");

            }
        }

        private string _side;
        public string Side
        {
            get
            {
                return _side;
            }
            set
            {
                if (_side == value)
                    return;

                _side = value;
                this.NotifyPropertyChanged("Side");

            }
        }

        public decimal _price;
        public decimal Price
        {
            get
            {
                return _price;
            }
            set
            {
                if (_price == value)
                    return;

                _price = value;
                this.NotifyPropertyChanged("Price");

            }
        }

        private decimal _amount;
        public decimal Amount
        {
            get
            {
                return _amount;
            }
            set
            {
                if (_amount == value)
                    return;

                _amount = value;
                this.NotifyPropertyChanged("Amount");

            }
        }

        private DateTime _executedAt;
        public DateTime ExecutedAt
        {
            get
            {
                return _executedAt;
            }
            set
            {
                if (_executedAt == value)
                    return;

                _executedAt = value;
                this.NotifyPropertyChanged("ExecutedAt");

            }
        }

        public Transactions()
        {

        }
    }

    public class TransactionsResult
    {
        public bool IsSuccess { get; set; }
        public int ErrorCode { get; set; }

        public List<Transactions> Trans;

        public TransactionsResult()
        {
            Trans = new List<Transactions>();
        }
    }

    #endregion

    #region == ロウソク足 ==

    public enum CandleTypes
    {
        OneMin, FiveMin, FifteenMin, ThirteenMin, OneHour, FourHour, EightHour, TwelveHour, OneDay, OneWeek

        // 1min 5min 15min 30min 1hour 4hour 8hour 12hour 1day 1week
    }

    public class Ohlcv
    {
        // [始値, 高値, 安値, 終値, 出来高, UnixTime]

        public decimal Open;
        public decimal High;
        public decimal Low;
        public decimal Close;
        public decimal Volume;
        public DateTime TimeStamp;

        public Ohlcv()
        {

        }
    }

    public class Candlestick
    {
        public CandleTypes Type { get; set; }

        public List<Ohlcv> Ohlcvs;

        public Candlestick()
        {
            Ohlcvs = new List<Ohlcv>();
        }
    }

    public class CandlestickResult
    {
        public bool IsSuccess { get; set; }
        public int ErrorCode { get; set; }

        public List<Candlestick> Candlesticks;

        public CandlestickResult()
        {
            Candlesticks = new List<Candlestick>();
        }
    }

    #endregion

    #endregion

    #region == パブリックAPIクラス ==

    class PublicAPIClient : BaseClient
    {
        private readonly Uri PublicAPIUri = new Uri("https://public.bitbank.cc");

        // コンストラクタ
        public PublicAPIClient()
        {
            //_endpoint = endpoint;

            _HTTPConn.Client.BaseAddress = PublicAPIUri;
        }

        // Ticker取得メソッド
        public async Task<Ticker> GetTicker(string pair)
        {
            //Uri _endpoint = new Uri ((_HTTPConn.Client.BaseAddress).ToString() + "btc_jpy/ticker");
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

                        /*
                        long l;
                        if (Int64.TryParse(deserialized.data.last, out l))
                        {
                            ticker.LTP = l;
                        }

                        if (Int64.TryParse(deserialized.data.buy, out l))
                        {
                            ticker.Bid = l;
                        }

                        if (Int64.TryParse(deserialized.data.sell, out l))
                        {
                            ticker.Ask = l;
                        }

                        if (Int64.TryParse(deserialized.data.low, out l))
                        {
                            ticker.Low = l;
                        }

                        if (Int64.TryParse(deserialized.data.high, out l))
                        {
                            ticker.High = l;
                        }
                        */

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
        public async Task<DepthResult> GetDepth()
        {
            Uri _endpoint = new Uri((_HTTPConn.Client.BaseAddress).ToString() + "btc_jpy/depth");

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
                                dd.DepthPrice = Decimal.Parse(dp[0]);//dp[0];
                                dd.DepthBid = Decimal.Parse(dp[1]);//dp[1];//
                                dd.DepthAsk = 0;
                                dpr.DepthBidList.Add(dd);


                            }
                        }

                        foreach (var dp in deserialized.Data.Asks)
                        {
                            if (dp.Count > 1)
                            {

                                Depth dd = new Depth();
                                dd.DepthPrice = Decimal.Parse(dp[0]); //dp[0];
                                dd.DepthBid = 0;
                                dd.DepthAsk = Decimal.Parse(dp[1]); //dp[1];//
                                dpr.DepthAskList.Add(dd);

                            }
                        }

                        dpr.ErrorCode = 0;
                        dpr.IsSuccess = true;


                        /*
                        long l;
                        if (Int64.TryParse(deserialized.Data.last, out l))
                        {
                            ticker.LTP = l;
                        }

                        if (Int64.TryParse(deserialized.data.buy, out l))
                        {
                            ticker.Bid = l;
                        }

                        if (Int64.TryParse(deserialized.data.sell, out l))
                        {
                            ticker.Ask = l;
                        }

                        if (Int64.TryParse(deserialized.data.low, out l))
                        {
                            ticker.Low = l;
                        }

                        if (Int64.TryParse(deserialized.data.high, out l))
                        {
                            ticker.High = l;
                        }



                        */

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
        public async Task<TransactionsResult> GetTransactions()
        {
            Uri _endpoint = new Uri((_HTTPConn.Client.BaseAddress).ToString() + "btc_jpy/transactions");

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
                            Transactions dd = new Transactions();
                            dd.TransactionId = tr.TransactionId;
                            dd.Side = tr.Side;
                            if (!string.IsNullOrEmpty(tr.Price)) dd.Price = decimal.Parse(tr.Price);
                            if (!string.IsNullOrEmpty(tr.Amount)) dd.Amount = decimal.Parse(tr.Amount);

                            dd.ExecutedAt = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds(tr.ExecutedAt).ToLocalTime();

                            trs.Trans.Add(dd);
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

            Uri _endpoint = new Uri((_HTTPConn.Client.BaseAddress).ToString() + pair  + "/candlestick/" + CandleType + "/" + YYYYMMDDD);

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
                                        oh.TimeStamp = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)jcs[5].Integer).ToLocalTime();
                                    }
                                    //dd.ExecutedAt = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds(tr.ExecutedAt).ToLocalTime();

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

    #endregion
}
