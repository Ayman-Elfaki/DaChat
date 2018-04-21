using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace DaStream
{
    public class DaStream:IDisposable
    {
        private ClientWebSocket clientWebSocket;


        public event EventHandler<EventArgs> Connected;
        public event EventHandler<EventArgs> Disconnected;
        public event EventHandler<byte[]> DataRecived;

        public async Task ConnectAsync(string uri)
        {
            try
            {
                clientWebSocket = new ClientWebSocket();
                clientWebSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(120);
                await clientWebSocket.ConnectAsync(new Uri(uri), CancellationToken.None);

                if (clientWebSocket.State == WebSocketState.Open)
                {
                    Connected?.Invoke(this, EventArgs.Empty);
                }

                while (clientWebSocket.State == WebSocketState.Open)
                {
                    var data = new ArraySegment<byte>(new byte[4 * 1024]);
                    await clientWebSocket.ReceiveAsync(data, CancellationToken.None);
                    if (data != null)
                    {
                        DataRecived?.Invoke(this, data.Array);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SendAsync(byte[] data)
        {
            try
            {
                if (clientWebSocket.State == WebSocketState.Open)
                {
                    await clientWebSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (clientWebSocket.State != WebSocketState.Closed)
                {
                   await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    Disconnected?.Invoke(this, EventArgs.Empty);
                    clientWebSocket.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Dispose()
        {
            if(clientWebSocket != null)
            {
                clientWebSocket.Dispose();
                clientWebSocket = null;
            }
        }
    }
}
