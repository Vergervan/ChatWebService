using ChatWebApp.Models;
using ChatWebApp.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.Json;
using ChatWebApplication.Models;

namespace ChatWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageRepository _messageRepository;
        private ClientsSingleton clients;
        public MessagesController(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
            clients = ClientsSingleton.GetInstance();
        }

        [HttpGet("/ws")]
        public async Task NewClient()
        {
            Console.WriteLine("Someone is trying to connect...");
            Console.WriteLine(string.Join(", ", HttpContext.Request.Headers));
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                try
                {
                    clients.AddClient(webSocket);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                Console.WriteLine("Clients: " + clients.Count);
                Console.WriteLine(clients.GetHashCode());
                Console.WriteLine("New WebSocket connection");
                byte[] buffer = new byte[1024 * 4];
                WebSocketReceiveResult result = null;
                try
                {
                    var messages = await _messageRepository.Get();
                    string messagesJson = JsonConvert.SerializeObject(messages.Select(x => new { Date = x.Date, Nickname = x.Nickname, MessageText = x.MessageText }));

                    await webSocket.SendAsync(Encoding.UTF8.GetBytes(messagesJson), WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);

                    do
                    {
                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), System.Threading.CancellationToken.None);
                        Message newMessage = JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(buffer, 0, result.Count));
                        await clients.SendMessageToAll(buffer);
                        await PostMessage(newMessage);
                    } while (result != null
                        || !result.CloseStatus.HasValue
                        || !(result.Count == 0));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                clients.RemoveClient(webSocket);
                try
                {
                    if (result != null)
                        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, System.Threading.CancellationToken.None);
                }
                catch (Exception) { }
                webSocket.Dispose();
                Console.WriteLine("Connection closed");
                Console.WriteLine("Clients: " + clients.Count);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                Console.WriteLine("Not a websocket request");
            }
        }

        [HttpGet]
        public async Task<IEnumerable<Message>> GetMessages()
        {
            return await _messageRepository.Get();
        }

        [HttpPost]
        public async Task<ActionResult<Message>> PostMessage([FromBody] Message message)
        {
            message.Date = DateTime.Now;
            var newMessage = await _messageRepository.Create(message);
            return CreatedAtAction(nameof(GetMessages), new { Date = DateTime.Now }, newMessage);
        }
        [HttpDelete]
        public async Task<ActionResult> Clear()
        {
            await _messageRepository.Clear();
            return NoContent();
        }
    }
}
