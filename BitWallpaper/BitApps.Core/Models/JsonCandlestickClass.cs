using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BitApps.Core.Models;

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
