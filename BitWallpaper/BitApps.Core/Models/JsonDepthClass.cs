using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BitApps.Core.Models;

public partial class JsonDepthObject
{
    [JsonProperty("success")]
    public long Success { get; set; }

    [JsonProperty("data")]
    public JsonDepthData? Data { get; set; }
}

public partial class JsonDepthData
{
    [JsonProperty("asks")]
    public List<List<string>>? Asks { get; set; }

    [JsonProperty("bids")]
    public List<List<string>>? Bids { get; set; }

    [JsonProperty("timestamp")]
    public long Timestamp { get; set; }
}
