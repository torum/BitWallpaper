using BitWallpaper.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BitWallpaper.Services.Contracts;

public interface IBitbankStreamService
{
    event EventHandler<BitbankTickerData>? TickerUpdated;

    void Initialize(List<PairCodes> pairCodes);

    Task StartAsync();
    Task StopAsync();

    bool IsConnected { get; }

    Task LeaveRoomAsync(PairCodes pairCode);
    Task JoinRoomAsync(PairCodes pairCode);
}