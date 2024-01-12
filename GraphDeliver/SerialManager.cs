using System;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Ports;
using System.Collections.Generic;

namespace GraphDeliver
{
    internal class SerialManager
    {
        public class DataContext
        {
            public byte[] Data { get; set; } = new byte[0];
            public DateTime UpdateTime { get; set; }
        }
        private static class Config
        {
            /// <summary>
            /// 帧头定义
            /// </summary>
            public const byte Head = 0x7D;
            /// <summary>
            /// 帧尾定义
            /// </summary>
            public const byte Tail = 0x7E;
            /// <summary>
            /// 转义标志
            /// </summary>
            public const byte Label = 0x7F;
            /// <summary>
            /// 转义后帧头
            /// </summary>
            public const byte EscapedHead = 0xFD;
            /// <summary>
            /// 转义后帧尾
            /// </summary>
            public const byte EscapedTail = 0xFE;
            /// <summary>
            /// 转义后标志
            /// </summary>
            public const byte EscapedLabel = 0xFF;
        }
        private readonly string _portName = "COM1";
        private readonly int _baudRate = 9600;
        private readonly int _dataBits = 8;
        private readonly Parity _parity = Parity.None;
        private readonly StopBits _stopBits = StopBits.One;
        private readonly int _sendInterval = 1000;

        private readonly SerialClient _serialClient;

        private readonly CancellationTokenSource _cancellation;
        private readonly Task _task;
        private readonly DataContext _dataContext;
        public bool IsOpen { get; set; }
        public bool Status_Open => _serialClient.IsOpen;
        public bool Status_Connect => _serialClient.IsConnected;
        /// <summary>
        /// 串口参数信息
        /// </summary>
        public string PortInfo => $"{_portName} {_baudRate}";
        public bool Status_CTS => _serialClient.Status_CTS;
        public bool Status_DSR => _serialClient.Status_DSR;
        public bool Status_CD => _serialClient.Status_CD;
        public bool Status_DTR => _serialClient.Status_DTR;
        public bool Status_RTS => _serialClient.Status_RTS;

        public event Action ClientConnected;
        public event Action ClientDisconnected;
        public event Action<string> ErrorOccured;
        public event Action<DataContext> DataRequested;
        public event Action<byte[]> DataSended;
        public SerialManager(string portName, int baudRate, string parity, int dataBits, int stopBits, int interval)
        {
            _sendInterval = interval;

            _portName = portName;
            _baudRate = baudRate;
            _dataBits = dataBits;
            switch (parity?.ToUpper())
            {
                case "NONE": _parity = Parity.None; break;
                case "ODD": _parity = Parity.Odd; break;
                case "EVEN": _parity = Parity.Even; break;
                case "MARK": _parity = Parity.Mark; break;
                case "SPACE": _parity = Parity.Space; break;
                default: _parity = Parity.None; break;
            }
            _stopBits = (StopBits)stopBits;
            _serialClient = new SerialClient(Config.Head, Config.Tail, Config.Label, new Dictionary<byte, byte>() { { Config.Head, Config.EscapedHead }, { Config.Tail, Config.EscapedTail }, { Config.Label, Config.EscapedLabel } });

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
        public bool OpenPort()
        {
            bool result = _serialClient.Start(_portName, _baudRate, _parity, _dataBits, _stopBits);

            if (result && !IsOpen)
            {
                ClientConnected?.Invoke();
            }
            else if (!result && IsOpen)
            {
                ClientDisconnected?.Invoke();
            }

            IsOpen = result;
            return result;
        }
        public bool ClosePort()
        {
            bool result = _serialClient.Disconnect();

            IsOpen = !result;
            return result;
        }
        private void SendingProcedure()
        {
            while (!_cancellation.Token.IsCancellationRequested)
            {
                DataRequested?.Invoke(_dataContext);

                var data = _dataContext.Data;

                SendData(data);

                Thread.Sleep(_sendInterval);
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
