using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace BitWallpaper.Models.Clients
{
    /// <summary>
    /// Base HTTP client
    /// </summary>
    public abstract class BaseClient
    {
        protected HTTPConnection _HTTPConn;

        public BaseClient()
        {
            //_HTTPConn = HTTPConnection.Instance;
            _HTTPConn = new HTTPConnection();

        }


        #region == Events ==

        public delegate void ClientDebugOutput(BaseClient sender, string data);

        public event ClientDebugOutput DebugOutput;

        #endregion

        protected async void ToDebugWindow(string data)
        {
            await Task.Run(() => { DebugOutput?.Invoke(this, data); });
        }

    }

    public class HTTPConnection
    {
        public HttpClient Client { get; }

        public HTTPConnection()
        {
            //System.Diagnostics.Debug.WriteLine("HttpClient");

            Client = new HttpClient();
        }
    }


}
