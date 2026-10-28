using BitWallpaper.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BitWallpaper.Services.Contracts;

public interface IBitbankHttpService
{
    event EventHandler<ErrorInfo>? ErrorOccured;

    Task<CandlestickResult?> GetCandlestickAsync(string pair, string CandleType, string YYYYMMDDD);

    Task<List<Ohlcv>?> GetLatestCandlesticksAsync(PairCodes pair, CandleTypes ct);

    Task<DepthResult?> GetDepthAsync(string pair);

    Task<TransactionsResult?> GetTransactionsAsync(string pair);
}