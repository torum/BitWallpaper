using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BitApps.Core.Models.APIClients;

public abstract class BaseSingletonClient : IDisposable
{
    private static readonly object _locker = new();
    private static volatile HttpClient? _client;

    protected static HttpClient Client
    {
        get
        {
            if (_client == null)
            {
                lock (_locker)
                {
                    _client ??= new HttpClient();
                }
            }

            return _client;
        }
    }

    #region == Events ==

    public delegate void ClientDebugOutput(BaseSingletonClient sender, string data);

    public event ClientDebugOutput? DebugOutput;

    protected async void ToDebugWindow(string data)
    {
        // TODO:
        await Task.Run(() => { DebugOutput?.Invoke(this, data); });
    }

    #endregion

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _client?.Dispose();

            _client = null;
        }
    }
}