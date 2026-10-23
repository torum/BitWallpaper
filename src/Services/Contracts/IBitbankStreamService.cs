using BitWallpaper.Models;

namespace BitWallpaper.Services.Contracts;

public interface IBitbankStreamService
{
    event EventHandler<BitbankTickerData>? TickerUpdated;

    void Initialize(List<PairCodes> pairCodes);

    Task StartAsync();
    Task StopAsync();

    bool IsConnected { get; }

    Task LeaveTickerRoomAsync(PairCodes pairCode);
    Task JoinTickerRoomAsync(PairCodes pairCode);
}