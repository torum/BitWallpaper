using System.Text.Json;
using System.Text.Json.Serialization;

namespace BitWallpaper.Models;

[JsonSerializable(typeof(CandlestickOhlcv))]
[JsonSerializable(typeof(List<CandlestickOhlcv>))]
[JsonSerializable(typeof(JsonCandlestickElement))]
[JsonSerializable(typeof(List<JsonCandlestickElement>))]
[JsonSerializable(typeof(JsonCandlestickData))]
[JsonSerializable(typeof(JsonCandlestick))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
internal partial class CandlestickJsonSerializerContext : JsonSerializerContext
{
}

// OHLCV配列のカスタムコンバーター
public class CandlestickOhlcvConverter : JsonConverter<CandlestickOhlcv>
{
    public override CandlestickOhlcv? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected start of array.");
        }

        // 配列の開始を読み進める
        reader.Read();

        var open = reader.GetString();
        reader.Read();

        var high = reader.GetString();
        reader.Read();

        var low = reader.GetString();
        reader.Read();

        var close = reader.GetString();
        reader.Read();

        var volume = reader.GetString();
        reader.Read();

        var timestamp = reader.GetInt64();
        reader.Read();

        if (reader.TokenType != JsonTokenType.EndArray)
        {
            throw new JsonException("Expected end of array.");
        }

        return new CandlestickOhlcv
        {
            Open = open,
            High = high,
            Low = low,
            Close = close,
            Volume = volume,
            Timestamp = timestamp
        };
    }

    public override void Write(Utf8JsonWriter writer, CandlestickOhlcv value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteStringValue(value.Open);
        writer.WriteStringValue(value.High);
        writer.WriteStringValue(value.Low);
        writer.WriteStringValue(value.Close);
        writer.WriteStringValue(value.Volume);
        writer.WriteNumberValue(value.Timestamp);
        writer.WriteEndArray();
    }
}

// CandlestickOhlcvにカスタムコンバーターを適用
[JsonConverter(typeof(CandlestickOhlcvConverter))]
public class CandlestickOhlcv
{
    public string? Open
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
    public string? Close
    {
        get; set;
    }
    public string? Volume
    {
        get; set;
    }
    public long Timestamp
    {
        get; set;
    }
}

public class JsonCandlestickElement
{
    [JsonPropertyName("type")]
    public string? Type
    {
        get; set;
    }

    [JsonPropertyName("ohlcv")]
    public List<CandlestickOhlcv>? Ohlcv
    {
        get; set;
    }
}

public partial class JsonCandlestickData
{
    [JsonPropertyName("candlestick")]
    public List<JsonCandlestickElement>? Candlestick
    {
        get; set;
    }

    [JsonPropertyName("timestamp")]
    public long? Timestamp
    {
        get; set;
    }
}

public partial class JsonCandlestick
{
    [JsonPropertyName("success")]
    public int? Success
    {
        get; set;
    }

    [JsonPropertyName("data")]
    public JsonCandlestickData? Data
    {
        get; set;
    }
}

/*
{
  "success": 1,
  "data": {
    "candlestick": [
      {
        "type": "1hour",
        "ohlcv": [
          [
            "17028635",
            "17150000",
            "17028634",
            "17095001",
            "12.3165",
            1757635200000
          ],
          [
            "17145536",
            "17167933",
            "17145535",
            "17153688",
            "2.9748",
            1757718000000
          ]
        ]
      }
    ],
    "timestamp": 1757721592557
  }
}
*/