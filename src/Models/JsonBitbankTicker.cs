using System.Text.Json;
using System.Text.Json.Serialization;

namespace BitWallpaper.Models;

public record BitbankTickerRoot(
    [property: JsonPropertyName("room_name")] string RoomName,
    [property: JsonPropertyName("message")] BitbankTickerMessage Message
);

public record BitbankTickerMessage(
    [property: JsonPropertyName("data")] BitbankTickerData Data
);

public record BitbankTickerData(

    // This attribute tells C# to read strings as numeric decimals automatically!
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    [property: JsonPropertyName("sell")] decimal Sell,

    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    [property: JsonPropertyName("buy")] decimal Buy,

    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    [property: JsonPropertyName("open")] decimal Open,

    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    [property: JsonPropertyName("high")] decimal High,

    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    [property: JsonPropertyName("low")] decimal Low,

    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    [property: JsonPropertyName("last")] decimal Last,

    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    [property: JsonPropertyName("vol")] decimal Vol,

    [property: JsonPropertyName("timestamp")] long Timestamp,

    PairCodes TickerPair = PairCodes.undefined
);

