using BitWallpaper.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BitWallpaper.Services.Contracts;

public interface IBitbankHttpSearvice
{
    Task<CandlestickResult?> GetCandlestick(string pair, string CandleType, string YYYYMMDDD);

    Task<List<Ohlcv>?> GetLatestCandlesticksAsync(PairCodes pair, CandleTypes ct);
}