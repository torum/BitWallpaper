using System.Text.Json;
using System.Text.Json.Serialization;

namespace BitWallpaper.Models;

public record BitbankDepthRoot(
    [property: JsonPropertyName("room_name")] string RoomName,
    [property: JsonPropertyName("message")] BitbankDepthMessage Message
);

public record BitbankDepthMessage(
    [property: JsonPropertyName("data")] BitbankDepthData Data
);

// The main record representing the structured order book data
public record BitbankDepthData(
    [property: JsonPropertyName("asks")] List<OrderBookEntry> Asks,
    [property: JsonPropertyName("bids")] List<OrderBookEntry> Bids,
    [property: JsonPropertyName("timestamp")] long Timestamp,
    PairCodes DepthPair = PairCodes.undefined
);

// The child record using a custom converter to instantly map ["string", "string"] -> decimals
[JsonConverter(typeof(OrderBookEntryConverter))]
public record OrderBookEntry(
    decimal Price,
    decimal Amount
);

// The high-performance converter that handles Bitbank's string-array format
public class OrderBookEntryConverter : JsonConverter<OrderBookEntry>
{
    public override OrderBookEntry? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected start of array for OrderBookEntry.");

        // Read Price string
        reader.Read();
        string priceStr = reader.GetString() ?? "0";

        // Read Amount string
        reader.Read();
        string amountStr = reader.GetString() ?? "0";

        // Consume the EndArray token
        reader.Read();
        if (reader.TokenType != JsonTokenType.EndArray)
            throw new JsonException("Expected end of array for OrderBookEntry.");

        // Parse with invariant culture safely
        decimal price = decimal.Parse(priceStr, System.Globalization.CultureInfo.InvariantCulture);
        decimal amount = decimal.Parse(amountStr, System.Globalization.CultureInfo.InvariantCulture);

        return new OrderBookEntry(price, amount);
    }

    public override void Write(Utf8JsonWriter writer, OrderBookEntry value, JsonSerializerOptions options)
    {
        // Optional: Implement if you ever need to serialize back to the Bitbank format
        writer.WriteStartArray();
        writer.WriteStringValue(value.Price.ToString(System.Globalization.CultureInfo.InvariantCulture));
        writer.WriteStringValue(value.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture));
        writer.WriteEndArray();
    }
}


