using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BitWallpaper.Models;

public class JsonErrorData
{
    public int Code
    {
        get; set;
    }
    public string? Ja
    {
        get; set;
    }
}

public class JsonErrorObject
{
    public int Success
    {
        get; set;
    }
    public JsonErrorData? Data
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
    public JsonDepthData? Data
    {
        get; set;
    }
}

public partial class JsonDepthData
{
    [JsonProperty("asks")]
    public List<List<string>>? Asks
    {
        get; set;
    }

    [JsonProperty("bids")]
    public List<List<string>>? Bids
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
    public string? Sell
    {
        get; set;
    }
    public string? Buy
    {
        get; set;
    }
    public string? High
    {
        get; set;
    }
    public string? Low
    {
        get; set;
    }
    public string? Last
    {
        get; set;
    }
    public string? Vol
    {
        get; set;
    }
    public long Timestamp
    {
        get; set;
    }
}

public class JsonTickerObject
{
    public int Success
    {
        get; set;
    }
    public JsonTickerData? Data
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
    public JsonData? Data
    {
        get; set;
    }
}

public partial class JsonData
{
    [JsonProperty("transactions")]
    public List<JsonTransaction>? Transactions
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
    public string? Side
    {
        get; set;
    }

    [JsonProperty("price")]
    public string? Price
    {
        get; set;
    }

    [JsonProperty("amount")]
    public string? Amount
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
    public JsonCandlestickData? Data
    {
        get; set;
    }
}

public partial class JsonCandlestickData
{
    [JsonProperty("candlestick")]
    public List<JsonCandlestickElement>? Candlestick
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
    public string? Type
    {
        get; set;
    }

    [JsonProperty("ohlcv")]
    public List<List<JsonOhlcv>>? Ohlcv
    {
        get; set;
    }
}

public partial struct JsonOhlcv
{
    public long Long;
    public string String;

    public static implicit operator JsonOhlcv(long Long) => new() { Long = Long };
    public static implicit operator JsonOhlcv(string String) => new() { String = String };
}


#endregion

