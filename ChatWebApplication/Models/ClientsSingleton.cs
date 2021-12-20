using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace ChatWebApplication.Models
{
    public class ClientsSingleton
    {
        private static ClientsSingleton _instance;
        private List<WebSocket> webSockets = new List<WebSocket>();
        private ClientsSingleton() { }
        public static ClientsSingleton GetInstance()
        {
            if(_instance == null)
            {
                _instance = new ClientsSingleton();
            }
            return _instance;
        }
        public void AddClient(WebSocket client)
        {
            webSockets.Add(client);
        }
        public void RemoveClient(WebSocket client)
        {
            webSockets.Remove(client);
        }
        public async Task SendMessageToAll(byte[] buffer)
        {
            foreach (var client in webSockets)
            {
                await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
            }
        }
        public int Count
        {
            get => webSockets.Count;
        }
    }
}
