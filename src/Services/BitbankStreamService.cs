using BitWallpaper.Models;
using BitWallpaper.Services.Contracts;
using CommunityToolkit.Mvvm.DependencyInjection;
using SocketIOClient;
using SocketIOClient.Common;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BitWallpaper.Services;

[JsonSerializable(typeof(BitbankTickerRoot))]
[JsonSerializable(typeof(BitbankDepthRoot))]
//[JsonSerializable(typeof(BitbankTransactionsRoot))] 
[JsonSerializable(typeof(JsonElement?))]
[JsonSerializable(typeof(JsonDocument))]
[JsonSerializable(typeof(string))] // Primitive types if needed
[JsonSerializable(typeof(object[]))] // This tells the AOT source generator how to handle outgoing event payloads.
[JsonSerializable(typeof(PairCodes))]
internal partial class BitbankJsonContext : JsonSerializerContext {}

public class BitbankStreamService : IBitbankStreamService
{
    private SocketIOClient.SocketIO? _client;
    private string[] _tickerRooms = new[] { "ticker_btc_jpy", "ticker_eth_jpy", "ticker_xrp_jpy" };
    private string _depthRoom = string.Empty;
    private bool _isInitialized;
    private bool _isConnected;
    private readonly Lock _lock = new();
    private readonly IDispatcherService _dispatcherService;

    public event EventHandler<BitbankTickerData>? TickerUpdated;
    public event EventHandler<BitbankDepthData>? DepthUpdated;

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
        /*
        _client = new SocketIOClient.SocketIO(uri, options, services =>
        {
            services.AddSystemTextJson(new JsonSerializerOptions
            {
                TypeInfoResolver = BitbankJsonContext.Default
            });
        });
        */

        _client.OnError += (sender, errorDescription) =>
        {
            Debug.WriteLine($"Socket.IO Error occurred: {errorDescription}");

            _dispatcherService.TryEnqueue(() =>
            {
                (App.Current as App)?.AppendErrorLog($"Socket.IO Error occurred @BitbankStreamService: {errorDescription}", "");
                (App.Current as App)?.SaveErrorLog();
            });
        };

        _client.OnConnected += OnClientConnected;

        _client.OnDisconnected += (sender, e) =>
        {
            Debug.WriteLine($"--> Disconnected: {e}");
        };

        _client.On("message", response =>
        {
            // Another way. Both are valid and not safe for AOT.
            
            string rawJsonArray = response.RawText ?? string.Empty;
            // Parse the text directly into a DOM Document (100% Native AOT safe)
            using var document = JsonDocument.Parse(rawJsonArray);
            JsonElement rootArray = document.RootElement;

            // Ensure it is a valid array containing at least the event name and your payload argument
            if (rootArray.ValueKind == JsonValueKind.Array && rootArray.GetArrayLength() > 1)
            {
                // Index 0 is the event name ("message"). Index 1 is the actual object payload.
                JsonElement firstArgument = rootArray[1];

                // Safely check 'room_name' via reflection-free DOM APIs
                if (firstArgument.TryGetProperty("room_name", out JsonElement roomProperty) && roomProperty.ValueKind == JsonValueKind.String)
                {
                    string roomName = roomProperty.GetString() ?? string.Empty;

                    //Debug.WriteLine($"--> Received message for room: {roomName}");

                    if (roomName.StartsWith("ticker_"))
                    {
                        var root = firstArgument.Deserialize(BitbankJsonContext.Default.BitbankTickerRoot);//response.GetValue<BitbankTickerRoot>(0);

                        if (root?.Message?.Data != null)
                        {
                            ProcessTickerData(root);
                        }
                        else
                        {
                            Debug.WriteLine("--> Received ticker message with null data. Ignoring.");
                        }
                    }
                    else if (roomName.StartsWith("depth_whole_"))
                    {
                        var root = firstArgument.Deserialize(BitbankJsonContext.Default.BitbankDepthRoot);//response.GetValue<BitbankDepthRoot>(0);

                        if (root?.Message?.Data != null)
                        {
                            ProcessDepthData(root);
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"--> Received data for an unhandled room: {roomName}");

                        _dispatcherService.TryEnqueue(() => {
                            (App.Current as App)?.AppendErrorLog($"--> Received data for an unhandled room: {roomName}", "");
                            (App.Current as App)?.SaveErrorLog();
                        });

                        return Task.CompletedTask;
                    }
                }
                else
                {
                    Debug.WriteLine("--> Received message without a valid 'room_name'. Ignoring.");
                    _dispatcherService.TryEnqueue(() => {
                        (App.Current as App)?.AppendErrorLog($"--> Received message without a valid 'room_name'", "");
                        (App.Current as App)?.SaveErrorLog();
                    });
                    return Task.CompletedTask;
                }
            }
            else
            {
                Debug.WriteLine("--> Received message that is not a valid array or has insufficient elements. Ignoring.");
                _dispatcherService.TryEnqueue(() => {
                    (App.Current as App)?.AppendErrorLog($"--> Received message that is not a valid array or has insufficient elements", "");
                    (App.Current as App)?.SaveErrorLog();
                });
                return Task.CompletedTask;
            }

            // Another way. Both are valid and not safe for AOT. 
            /*
            var element = response.GetValue<System.Text.Json.JsonElement?>(0);
            if (!element.HasValue) return Task.CompletedTask;

            // Prints the raw JSON string for debugging purposes
            //string rawJsonString = element.Value.GetRawText();
            //Debug.WriteLine($"[RAW STREAM DATA]: {rawJsonString}");

            if (!element.HasValue || element.Value.ValueKind != JsonValueKind.Object)
            {
                Debug.WriteLine("--> Received message that is not a JSON object. Ignoring.");
                return Task.CompletedTask;
            }

            // Access the "room_name" property directly from the root object
            if (!element.Value.TryGetProperty("room_name", out JsonElement roomProperty) || roomProperty.ValueKind != JsonValueKind.String)
            {
                Debug.WriteLine("--> Received message without a valid 'room_name'. Ignoring.");
                return Task.CompletedTask;
            }

            string roomName = roomProperty.GetString() ?? string.Empty;

            if (roomName.StartsWith("ticker_"))
            {
                var root = response.GetValue<BitbankTickerRoot>(0);

                if (root?.Message?.Data != null)
                {
                    ProcessTickerData(root);
                }
            }
            else if (roomName.StartsWith("depth_whole_"))
            {
                var root = response.GetValue<BitbankDepthRoot>(0);

                if (root?.Message?.Data != null)
                {
                    ProcessDepthData(root);
                }
            }
            else
            {
                Debug.WriteLine($"--> Received data for an unhandled room: {roomName}");
                return Task.CompletedTask;
            }
            */
            return Task.CompletedTask;
        });

        try
        {
            await _client.ConnectAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to connect initially @BitbankStreamService: {ex}");

            _dispatcherService.TryEnqueue(() =>
            {
                (App.Current as App)?.AppendErrorLog($"Failed to connect initially @BitbankStreamService: {ex}", "");
                (App.Current as App)?.SaveErrorLog();
            });
        }
    }

    private void ProcessTickerData(BitbankTickerRoot root)
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

        if (string.IsNullOrWhiteSpace(root.RoomName))
        {
            Debug.WriteLine("--> Received message with empty RoomName. Ignoring.");
            return;
        }

        if (root.RoomName.StartsWith("ticker_"))
        {
            string pair = (root.RoomName ?? "unknown").Replace("ticker_", "");
            if (Enum.TryParse(pair, true, out PairCodes pairCode))
            {
                var data = root.Message.Data with { TickerPair = pairCode };
                OnTickerUpdated(data);
            }
            else
            {
                Debug.WriteLine($"--> Unknown pair code received: {pair}");
                return;
            }
        }
        else
        {
            Debug.WriteLine($"--> Received message for unknown room type: {root.RoomName}. Ignoring.");
            return;
        }
    }

    private void ProcessDepthData(BitbankDepthRoot root)
    {
        if (string.IsNullOrWhiteSpace(root.RoomName))
        {
            Debug.WriteLine("--> Received message with empty RoomName. Ignoring.");
            return;
        }

        if (root.RoomName.StartsWith("depth_whole_"))
        {
            string pair = (root.RoomName ?? "unknown").Replace("depth_whole_", "");
            if (Enum.TryParse(pair, true, out PairCodes pairCode))
            {
                var data = root.Message.Data with { DepthPair = pairCode };
                OnDepthUpdated(data);
            }
            else
            {
                Debug.WriteLine($"--> Unknown pair code received: {pair}");
                return;
            }
        }
        else
        {
            Debug.WriteLine($"--> Received message for unknown room type: {root.RoomName}. Ignoring.");
            return;
        }
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

    public async Task JoinDepthRoomAsync(PairCodes pairCode)
    {
        if (_client == null || !_isConnected) return;

        if (!_isInitialized) return;

        if (string.IsNullOrWhiteSpace(pairCode.ToString())) return;

        if (_depthRoom.Equals($"depth_whole_{pairCode.ToString().ToLower()}", StringComparison.OrdinalIgnoreCase)) 
        { 
            Debug.WriteLine($"--> Already joined depth room: {_depthRoom}. No action taken.");
            return; 
        }

        if (!string.IsNullOrWhiteSpace(_depthRoom))
        {
            Debug.WriteLine($"--> Already joined depth room: {_depthRoom}. Leaving it first...");

            string pair = (_depthRoom ?? "unknown").Replace("depth_whole_", "");
            if (!string.IsNullOrWhiteSpace(pair))
            {
                if (Enum.TryParse(pair, true, out PairCodes currentPairCode))
                {
                    await LeaveDepthRoomAsync(currentPairCode);
                }
                else
                {
                    Debug.WriteLine($"--> Unknown pair code received for depth room: {pair}");
                }
            }
        }

        Debug.WriteLine($"--> Joining Bitbank Stream rooms...{_depthRoom}");

        // Wrap the room string in an array, matching your subscription pattern
        var roomArray = new[] { $"depth_whole_{pairCode.ToString().ToLower()}" };

        // Emit 'join-room' to tell the Bitbank server to start streaming this data
        await _client.EmitAsync("join-room", roomArray);

        _depthRoom = $"depth_whole_{pairCode.ToString().ToLower()}";

        Debug.WriteLine($"--> Joined room: {_depthRoom}");
    }

    public async Task LeaveDepthRoomAsync(PairCodes pairCode)
    {
        if (_client == null || !_isConnected) return;

        if (!_isInitialized) return;

        string room = $"depth_whole_{pairCode.ToString().ToLower()}";

        if (string.IsNullOrWhiteSpace(room)) return;

        if (!_depthRoom.Equals(room, StringComparison.OrdinalIgnoreCase))
        {
            Debug.WriteLine($"--> Not currently in depth room: {room}. Current depth room: {_depthRoom}");
            return;
        }

        Debug.WriteLine("--> Leaving Bitbank Stream rooms...");

        // Wrap the room string in an array, matching your subscription pattern
        var roomArray = new[] { room };

        // Emit 'leave-room' to tell the Bitbank server to stop streaming this data
        await _client.EmitAsync("leave-room", roomArray);

        _depthRoom = string.Empty;

        Debug.WriteLine($"--> Left room: {room}");
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

    protected virtual void OnDepthUpdated(BitbankDepthData data)
    {
        DepthUpdated?.Invoke(this, data);
    }
}
