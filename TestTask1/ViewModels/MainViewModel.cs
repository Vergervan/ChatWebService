using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;

namespace TestTask1.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private string _messagetext = string.Empty;
        private bool _isConnected = false;
        private bool _useDates = false;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private ObservableCollection<Message> _messagesList = new ObservableCollection<Message>();
        private ObservableCollection<Message> _sortedList = new ObservableCollection<Message>();
        private ClientWebSocket client;

        public string MessageText
        {
            get => _messagetext;
            set
            {
                _messagetext = value;
                OnPropertyChanged();
            }
        }

        public bool ConnectDisabled
        {
            get => !IsConnected;
        }

        public bool UseDates
        {
            get => _useDates;
            set
            {
                _useDates = value;
                if (_useDates)
                    SortMessages();
                OnPropertyChanged();
            }
        }

        private void SortMessages()
        {
            if (!_startDate.HasValue || !_endDate.HasValue) return;
            _sortedList = new ObservableCollection<Message>(_messagesList.Where(x => x.Date >= StartDate.Value && x.Date <= EndDate.Value.AddDays(1).Date));
            System.Diagnostics.Debug.WriteLine($"Sorted: {_sortedList.Count} | {StartDate.Value} | {EndDate.Value}");
            OnPropertyChanged(nameof(MessagesList));
        }

        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                if (!value.HasValue && value == DateTime.MinValue) return;
                _startDate = value;
                SortMessages();
                OnPropertyChanged();
            }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                if (!value.HasValue && value == DateTime.MinValue) return;
                _endDate = value;
                SortMessages();
                OnPropertyChanged();
            }
        }

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ConnectDisabled));
            }
        }

        public ObservableCollection<Message> MessagesList
        {
            get => UseDates ? _sortedList : _messagesList;
        }

        public void AddMessages(IEnumerable<Message> messages)
        {
            foreach (var m in messages) MessagesList.Add(m);
            OnPropertyChanged(nameof(MessagesList));
        }

        public void AddMessage(Message message)
        {
            MessagesList.Add(message);
            OnPropertyChanged(nameof(MessagesList));
        }

        public ICommand Connect
        {
            get => new ClickCommand(async (obj) =>
            {
                try
                {
                    client = new ClientWebSocket();
                    await client.ConnectAsync(new Uri("wss://localhost:5001/ws"), cts.Token);
                    IsConnected = true;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return;
                }
                byte[] buffer = new byte[1024 * 4];
                var bytesResult = new ArraySegment<byte>(buffer);
                var result = await client.ReceiveAsync(bytesResult, cts.Token);
                AddMessages(JsonConvert.DeserializeObject<List<Message>>(Encoding.UTF8.GetString(bytesResult.Array)));
                while (client.State == WebSocketState.Open)
                {
                    result = await client.ReceiveAsync(bytesResult, cts.Token);
                    AddMessage(JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(buffer, 0, result.Count)));
                }
            });
        }

        public ICommand SendMessage
        {
            get => new ClickCommand(async (obj) =>
            {
                if (string.IsNullOrEmpty(MessageText)) return;
                Message message = new Message() { Date = DateTime.Now, Nickname = Environment.MachineName, MessageText = this.MessageText };
                byte[] buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                MessageText = string.Empty;
                await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cts.Token);
            });
        }
        ~MainViewModel()
        {
            client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Destructor close", CancellationToken.None);
            client.Dispose();
        }
    }
}
