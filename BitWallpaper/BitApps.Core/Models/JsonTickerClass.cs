using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BitApps.Core.Models;

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
