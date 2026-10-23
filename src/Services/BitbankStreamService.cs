using BitWallpaper.Models;
using BitWallpaper.Services.Contracts;
using SocketIOClient;
using SocketIOClient.Common;

namespace BitWallpaper.Services;

public class BitbankStreamService : IBitbankStreamService
{
    private SocketIOClient.SocketIO? _client;
    private string[] _tickerRooms = new[] { "ticker_btc_jpy", "ticker_eth_jpy", "ticker_xrp_jpy" };

    private bool _isInitialized;
    private bool _isConnected;
    private readonly Lock _lock = new();
    private readonly IDispatcherService _dispatcherService;

    public event EventHandler<BitbankTickerData>? TickerUpdated;

    public BitbankStreamService(IDispatcherService dispatcherService)
    {
        _dispatcherService = dispatcherService;
    }

    public bool IsConnected => _isConnected;

    public void Initialize(List<PairCodes> pairCodes)
    {
        _isInitialized = true;
        _tickerRooms = pairCodes.Select(pc => $"ticker_{pc.ToString().ToLower()}").ToArray();
    }

    public async Task StartAsync()
    {
        if (!_isInitialized)
        {
            Debug.WriteLine("--> BitbankStreamService is not initialized. Call Initialize() before StartAsync().");
            return;
        }

        // Bitbank Socket.IO endpoint (Engine.IO v4)
        var uri = new Uri("wss://stream.bitbank.cc");
        var options = new SocketIOOptions
        {
            EIO = EngineIO.V4,
            Transport = TransportProtocol.WebSocket
        };

        _client = new SocketIOClient.SocketIO(uri, options);

        _client.OnConnected += OnClientConnected;

        _client.OnDisconnected += (sender, e) =>
        {
            Debug.WriteLine($"--> Disconnected: {e}");
        };

        _client.On("message", response =>
        {
            var element = response.GetValue<System.Text.Json.JsonElement?>(0);
            if (!element.HasValue) return Task.CompletedTask;

            // Prints the raw JSON string for debugging purposes
            //string rawJsonString = element.Value.GetRawText();
            //Debug.WriteLine($"[RAW STREAM DATA]: {rawJsonString}");

            var root = response.GetValue<BitbankTickerRoot>(0);

            // Ensure data nodes exist before reading values
            if (root?.Message?.Data != null)
            {
                /*
                var data = root.Message.Data;
                string pair = (root.RoomName ?? "unknown").Replace("ticker_", "").ToUpper();
                DateTime timeStamp = DateTimeOffset.FromUnixTimeMilliseconds(data.Timestamp).LocalDateTime;

                // Calculate a quick math metric (e.g., daily percentage change)
                decimal priceChange = data.Last - data.Open;
                decimal percentChange = data.Open != 0 ? (priceChange / data.Open) * 100 : 0;

                // Print out your typed metrics using standard number formatting
                Debug.WriteLine($"=== {pair} NUMERIC DATA [{timeStamp:yyyy-MM-dd HH:mm:ss}] ===");
                Debug.WriteLine($"  Last Price : {data.Last:N0} JPY ({percentChange:+0.00;-0.00;0.00}%)");
                Debug.WriteLine($"  Bid (Buy)  : {data.Buy:N0}   | Ask (Sell) : {data.Sell:N0}");
                Debug.WriteLine($"  24h High   : {data.High:N0}  | 24h Low    : {data.Low:N0}");
                Debug.WriteLine($"  24h Volume : {data.Vol:N4}");
                Debug.WriteLine(new string('=', 45));
                */

                string pair = (root.RoomName ?? "unknown").Replace("ticker_", "");

                if (Enum.TryParse(pair, true, out PairCodes pairCode))
                {
                    var data = root.Message.Data with { TickerPair = pairCode };
                    OnTickerUpdated(data);
                }
                else
                {
                    Debug.WriteLine($"--> Unknown pair code received: {pair}");
                }
            }

            return Task.CompletedTask;
        });

        await _client.ConnectAsync();
    }

    private async void OnClientConnected(object? sender, EventArgs e)
    {
        if (_client == null) return;
        _isConnected = true;

        Debug.WriteLine("--> Successfully connected to Bitbank Stream. Subscribing now..." + string.Join(", ", _tickerRooms));

        //var rooms = new[] { "ticker_btc_jpy", "ticker_eth_jpy", "ticker_xrp_jpy" };

        if (_tickerRooms.Length == 0) return;

        foreach (var room in _tickerRooms)
        {
            var roomArray = new[] { room };
            await _client.EmitAsync("join-room", roomArray);
        }
    }

    public async Task LeaveTickerRoomAsync(PairCodes pairCode)
    {
        if (_client == null || !_isConnected) return;

        if (!_isInitialized) return;

        string room = $"ticker_{pairCode.ToString().ToLower()}";

        if (string.IsNullOrWhiteSpace(room)) return;

        if (!_tickerRooms.Contains(room, StringComparer.OrdinalIgnoreCase)) return;

        Debug.WriteLine("--> Leaving Bitbank Stream rooms...");

        // Wrap the room string in an array, matching your subscription pattern
        var roomArray = new[] { room };

        // Emit 'leave-room' to tell the Bitbank server to stop streaming this data
        await _client.EmitAsync("leave-room", roomArray);

        lock (_lock)
        {
            List<string> roomList = [.. _tickerRooms];
            roomList.Remove(room);
            _tickerRooms = [.. roomList];

        }
        Debug.WriteLine($"--> Left room: {room}");
    }

    public async Task JoinTickerRoomAsync(PairCodes pairCode)
    {
        if (_client == null || !_isConnected) return;
        
        if (!_isInitialized) return;

        if (string.IsNullOrWhiteSpace(pairCode.ToString())) return;

        if (_tickerRooms.Contains($"ticker_{pairCode.ToString().ToLower()}", StringComparer.OrdinalIgnoreCase)) return;

        Debug.WriteLine("--> Joining Bitbank Stream rooms...");

        // Wrap the room string in an array, matching your subscription pattern
        var roomArray = new[] { $"ticker_{pairCode.ToString().ToLower()}" };

        // Emit 'join-room' to tell the Bitbank server to start streaming this data
        await _client.EmitAsync("join-room", roomArray);

        lock (_lock)
        {
            List<string> roomList = [.. _tickerRooms];
            roomList.Add($"ticker_{pairCode.ToString().ToLower()}");
            _tickerRooms = [.. roomList];
        }

        Debug.WriteLine($"--> Joined room: {pairCode}");
    }

    public async Task StopAsync()
    {
        _client?.OnConnected -= OnClientConnected;

        if (_client is not null && _isConnected == true)
        {
            try
            {
                if (_client.Connected)
                {
                    Debug.WriteLine("--> Disconnecting from Bitbank Stream...");
                    await _client.DisconnectAsync(); // 1. Signal graceful disconnect to server
                }
            }
            catch (System.Net.WebSockets.WebSocketException)
            {
                /* Swallow any concurrent connection drops */
            }
            _client.Dispose();
        }
    }

    protected virtual void OnTickerUpdated(BitbankTickerData data)
    {
        TickerUpdated?.Invoke(this, data);
    }
}
