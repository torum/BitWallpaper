using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BitApps.Core.Models;

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
