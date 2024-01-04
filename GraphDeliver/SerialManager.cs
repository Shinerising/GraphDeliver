using System;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Ports;

namespace GraphDeliver
{
    internal class SerialManager
    {
        public class DataContext
        {
            public byte[] Data { get; set; } = new byte[0];
            public DateTime UpdateTime { get; set; }
        }

        private readonly SerialClient _serialClient;

        private readonly CancellationTokenSource _cancellation;
        private readonly Task _task;
        private readonly DataContext _dataContext;
        public bool IsOpen { get; set; }
        public string PortInfo { get; set; }

        public event Action ClientConnected;
        public event Action ClientDisconnected;
        public event Action<string> ErrorOccured;
        public event Action<DataContext> DataRequested;
        public event Action<byte[]> DataSended;
        public SerialManager()
        {
            _serialClient = new SerialClient(new byte[0], new byte[0], SerialClient.WorkingMode.Default);

            _serialClient.SetErrorHandler((object sender, SerialError error, Exception e, string message) =>
            {
                message = message ?? e?.Message ?? error.ToString();
                ErrorOccured?.Invoke(message);
            });

            _dataContext = new DataContext();

            _cancellation = new CancellationTokenSource();
            _task = new Task(SendingProcedure, _cancellation.Token);
            _task.Start();
        }
        public void OpenPort()
        {
            bool result = _serialClient.Start("", 0);

            if (result && !IsOpen)
            {
                ClientConnected?.Invoke();
            }
            else if (!result && IsOpen)
            {
                ClientDisconnected?.Invoke();
            }

            IsOpen = result;
        }
        private void SendingProcedure()
        {
            while (!_cancellation.Token.IsCancellationRequested)
            {
                DataRequested?.Invoke(_dataContext);

                var data = _dataContext.Data;

                SendData(data);

                Thread.Sleep(1000);
            }
        }
        private void SendData(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return;
            }

            bool result = _serialClient.Send(data);

            if (result)
            {
                DataSended?.Invoke(data);
            }

            if (result && !IsOpen)
            {
                ClientConnected?.Invoke();
            }
            else if (!result && IsOpen)
            {
                ClientDisconnected?.Invoke();
            }

            IsOpen = result;
        }
        public void Dispose()
        {
            _cancellation.Cancel();
        }

    }
}
