using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BitWallpaper4.Models;

public class JsonErrorData
{
    public int code
    {
        get; set;
    }
    public string ja
    {
        get; set;
    }
}

public class JsonErrorObject
{
    public int success
    {
        get; set;
    }
    public JsonErrorData data
    {
        get; set;
    }
}

public partial class JsonDepthObject
{
    [JsonProperty("success")]
    public long Success
    {
        get; set;
    }

    [JsonProperty("data")]
    public JsonDepthData Data
    {
        get; set;
    }
}

public partial class JsonDepthData
{
    [JsonProperty("asks")]
    public List<List<string>> Asks
    {
        get; set;
    }

    [JsonProperty("bids")]
    public List<List<string>> Bids
    {
        get; set;
    }

    [JsonProperty("timestamp")]
    public long Timestamp
    {
        get; set;
    }
}

#region == JsonTickerClass Json デシリアライズ用 ==

// Ticker
public class JsonTickerData
{
    public string sell
    {
        get; set;
    }
    public string buy
    {
        get; set;
    }
    public string high
    {
        get; set;
    }
    public string low
    {
        get; set;
    }
    public string last
    {
        get; set;
    }
    public string vol
    {
        get; set;
    }
    public long timestamp
    {
        get; set;
    }
}

public class JsonTickerObject
{
    public int success
    {
        get; set;
    }
    public JsonTickerData data
    {
        get; set;
    }
}

// Transaction
public partial class JsonTransactions
{
    [JsonProperty("success")]
    public long Success
    {
        get; set;
    }

    [JsonProperty("data")]
    public JsonData Data
    {
        get; set;
    }
}

public partial class JsonData
{
    [JsonProperty("transactions")]
    public List<JsonTransaction> Transactions
    {
        get; set;
    }
}

public partial class JsonTransaction
{
    [JsonProperty("transaction_id")]
    public long TransactionId
    {
        get; set;
    }

    [JsonProperty("side")]
    public string Side
    {
        get; set;
    }

    [JsonProperty("price")]
    public string Price
    {
        get; set;
    }

    [JsonProperty("amount")]
    public string Amount
    {
        get; set;
    }

    [JsonProperty("executed_at")]
    public long ExecutedAt
    {
        get; set;
    }
}


// Candlestick
public partial class JsonCandlestick
{
    [JsonProperty("success")]
    public long Success
    {
        get; set;
    }

    [JsonProperty("data")]
    public JsonCandlestickData Data
    {
        get; set;
    }
}

public partial class JsonCandlestickData
{
    [JsonProperty("candlestick")]
    public List<JsonCandlestickElement> Candlestick
    {
        get; set;
    }

    [JsonProperty("timestamp")]
    public long Timestamp
    {
        get; set;
    }
}

public partial class JsonCandlestickElement
{
    [JsonProperty("type")]
    public string Type
    {
        get; set;
    }

    [JsonProperty("ohlcv")]
    public List<List<JsonOhlcv>> Ohlcv
    {
        get; set;
    }
}

public partial struct JsonOhlcv
{
    public long? Long;
    public string String;

    public static implicit operator JsonOhlcv(long Long) => new JsonOhlcv { Long = Long };
    public static implicit operator JsonOhlcv(string String) => new JsonOhlcv { String = String };
}


#endregion

