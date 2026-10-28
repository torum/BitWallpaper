using BitWallpaper.Models;

namespace BitWallpaper.Services.Contracts;

public interface IBitbankStreamService
{
    event EventHandler<BitbankTickerData>? TickerUpdated;
    event EventHandler<BitbankDepthData>? DepthUpdated;
    event EventHandler<ErrorInfo>? ErrorOccured;
    void Initialize(List<PairCodes> pairCodes);

    Task StartAsync();
    Task StopAsync();

    bool IsConnected { get; }

    Task LeaveTickerRoomAsync(PairCodes pairCode);
    Task JoinTickerRoomAsync(PairCodes pairCode);
    Task LeaveDepthRoomAsync(PairCodes pairCode);
    Task JoinDepthRoomAsync(PairCodes pairCode);
}