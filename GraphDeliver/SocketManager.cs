using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GraphDeliver
{
    internal class SocketManager
    {

        private const string _head = "CLMQHEAD";
        private const string _tail = "CLMQTAIL";
        private const int _offsetHead = 11;
        private const int _offsetTail = 8;
        private readonly bool _stickyControl = true;
        private readonly SocketTCPClient _clientA;
        private readonly SocketTCPClient _clientB;
        private readonly CancellationTokenSource _cancellation;
        private readonly Task _task;

        private readonly IPAddress _ipAddressA;
        private readonly int _portA = 1000;
        private readonly IPAddress _ipAddressB;
        private readonly int _portB = 1000;
        private readonly int _idleCount = 5;

        private int _receiveCountA = 0;
        private int _receiveCountB = 0;
        private int _errorCountA = 0;
        private int _errorCountB = 0;
        private readonly bool _isEnabledA = false;
        private readonly bool _isEnabledB = false;
        private bool _isConnectedA = false;
        private bool _isConnectedB = false;

        public event Action ClientConnected;
        public event Action ClientDisconnected;
        public event Action<string> ErrorOccured;
        public event Action<int, byte[]> DeviceStatusReceived;
        public event Action<string> MessageReceived;
        public event Action<int, byte[]> BoardStatusReceived;
        public event Action<byte[]> RollingDataReceived;

        public bool IsEnabledA => _isEnabledA;
        public bool IsEnabledB => _isEnabledB;
        public bool IsConnectedA => _clientA != null && _clientA.IsConnected && _isConnectedA;
        public bool IsConnectedB => _clientB != null && _clientB.IsConnected && _isConnectedB;
        public string NameA => _clientB != null && _ipAddressA != null ? $"{_ipAddressA}:{_portA}" : "未启用";
        public string NameB => _clientB != null && _ipAddressB != null ? $"{_ipAddressB}:{_portB}" : "未启用";
        public bool IsConnected { get; set; }

        public SocketManager(IPAddress ipAddressA, IPAddress ipAddressB, int portA = 1000, int portB = 1000, int idleCount = 5)
        {
            _clientA = new SocketTCPClient();
            _clientB = new SocketTCPClient();

            _isEnabledA = ipAddressA != null;
            _isEnabledB = ipAddressB != null;

            _ipAddressA = ipAddressA;
            _ipAddressB = ipAddressB;

            _portA = portA;
            _portB = portB;

            _idleCount = idleCount;

            SetClient();

            _cancellation = new CancellationTokenSource();
            _task = new Task(SocketReceivingProcedure, _cancellation.Token);
            _task.Start();
        }

        private void SetClient()
        {
            _clientA.SetConnectHandler((bool flag) =>
            {
                _isConnectedA = flag;
                if (flag && !IsConnected)
                {
                    IsConnected = true;
                    ClientConnected?.Invoke();
                    SendGraphRequest(_clientA);
                }
            });

            _clientA.SetErrorHandler((object sender, Exception e, SocketError error, string message) =>
            {
                _errorCountA++;
                message = message ?? e?.Message ?? error.ToString();
                ErrorOccured?.Invoke("A机:" + message);
            });

            _clientA.SetReceiveHandler((EndPoint endPoint, byte[] buffer, int offset, int count) =>
            {
                if (_clientA.IsConnected)
                {
                    _receiveCountA += 1;
                    PushData(buffer, offset, count);
                    SendDataResponse(_clientA, buffer[_offsetHead - 2]);
                }
            });

            _clientB.SetConnectHandler((bool flag) =>
            {
                _isConnectedB = flag;
                if (flag && !IsConnected)
                {
                    IsConnected = true;
                    ClientConnected?.Invoke();
                    SendGraphRequest(_clientB);
                }
            });

            _clientB.SetErrorHandler((object sender, Exception e, SocketError error, string message) =>
            {
                _errorCountB++;
                message = message ?? e?.Message ?? error.ToString();
                ErrorOccured?.Invoke("B机:" + message);
            });

            _clientB.SetReceiveHandler((EndPoint endPoint, byte[] buffer, int offset, int count) =>
            {
                if (_clientB.IsConnected)
                {
                    _receiveCountB += 1;
                    PushData(buffer, offset, count);
                    SendDataResponse(_clientB, buffer[_offsetHead - 2]);
                }
            });
        }

        private readonly HashSet<int> _packageIndexSet = new HashSet<int>();

        private bool CheckIfRepeatPackage(int index)
        {
            if (_packageIndexSet.Contains(index))
            {
                return true;
            }
            else
            {
                _packageIndexSet.Add(index);

                while (_packageIndexSet.Count > 500)
                {
                    _packageIndexSet.Remove(_packageIndexSet.First());
                }

                return false;
            }
        }

        private byte[] _answerData;
        private void SendDataResponse(SocketTCPClient client, byte id)
        {
            try
            {
                if (_answerData == null)
                {
                    byte[] buffer = new byte[10];
                    byte[] packData = new byte[buffer.Length + _head.Length + _tail.Length + 2];
                    Encoding.ASCII.GetBytes(_head).CopyTo(packData, 0);
                    packData[_head.Length] = 0x04;
                    packData[_head.Length + 1] = 0x00;
                    buffer.CopyTo(packData, _head.Length + 2);
                    Encoding.ASCII.GetBytes(_tail).CopyTo(packData, _head.Length + buffer.Length + 2);
                    _answerData = packData;
                }
                _answerData[_head.Length + 1] = id;

                _ = client.SendSync(_answerData);
            }
            catch { }
        }

        private void SendGraphRequest(SocketTCPClient client)
        {
            try
            {
                if (client == null || !client.IsConnected)
                {
                    return;
                }
                byte[] buffer = new byte[18];
                string command = "RF";
                ushort stationId = 0x0001;
                ushort commandId = 0x0002;
                ushort deviceId = 0x0000;
                ushort length = 10;
                BitConverter.GetBytes((uint)length).CopyTo(buffer, 4);
                BitConverter.GetBytes(stationId).CopyTo(buffer, 8);
                BitConverter.GetBytes(commandId).CopyTo(buffer, 8 + 2);
                BitConverter.GetBytes(deviceId).CopyTo(buffer, 8 + 4);
                BitConverter.GetBytes(length).CopyTo(buffer, 8 + 6);
                Encoding.ASCII.GetBytes(command).CopyTo(buffer, 8 + 8);

                byte[] packData = new byte[buffer.Length + _head.Length + _tail.Length + 2];
                Encoding.ASCII.GetBytes(_head).CopyTo(packData, 0);
                packData[_head.Length] = 0x03;
                packData[_head.Length + 1] = 0x00;
                buffer.CopyTo(packData, _head.Length + 2);
                Encoding.ASCII.GetBytes(_tail).CopyTo(packData, _head.Length + buffer.Length + 2);

                _ = client.SendSync(packData);
            }
            catch { }
        }

        private void PushData(byte[] buffer, int offset, int count)
        {
            if (!_stickyControl)
            {
                DataReceived(buffer, offset + _offsetHead, count - _offsetHead - _offsetTail);
                return;
            }

            while (true)
            {
                int head = FindInBuffer(_head, buffer, offset);
                int tail = FindInBuffer(_tail, buffer, offset);
                if (head >= 0 && tail >= head && count >= tail)
                {
                    int length = tail - head + _tail.Length;
                    byte[] data = new byte[length];
                    Buffer.BlockCopy(buffer, head, data, 0, length);
                    DataReceived(data, 0 + _offsetHead, length - _offsetHead - _offsetTail);
                    offset = tail + _tail.Length;
                }
                else
                {
                    break;
                }
            }
        }
        private int FindInBuffer(string target, byte[] buffer, int offset)
        {
            int index = -1;
            for (int i = offset; i < buffer.Length; i++)
            {
                if (buffer[i] == target[0])
                {
                    bool flag = true;
                    for (int j = 1; j < target.Length; j++)
                    {
                        if (buffer[i + j] != target[j])
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        index = i;
                        break;
                    }
                }
            }
            return index;
        }

        private void DataReceived(byte[] buffer, int offset, int count)
        {
            if (buffer == null || buffer.Length == 0 || count > buffer.Length || count < 16)
            {
                return;
            }

            byte[] data = new byte[count];
            Buffer.BlockCopy(buffer, offset, data, 0, count);

            int index = BitConverter.ToInt32(data, 0);
            int length = BitConverter.ToInt32(data, 4);
            ushort type = BitConverter.ToUInt16(data, 10);
            int deviceId = BitConverter.ToUInt16(data, 12);
            int dataLength = BitConverter.ToUInt16(data, 14) - 8;

            if (length <= 0)
            {
                return;
            }

            if (CheckIfRepeatPackage(index))
            {
                return;
            }

            switch (type)
            {
                case 0x0001:
                    {
                        if (count < 16)
                        {
                            break;
                        }
                        byte[] status = new byte[dataLength];
                        Buffer.BlockCopy(data, 16, status, 0, dataLength);
                        DeviceStatusReceived?.Invoke(deviceId, status);
                    }
                    break;
                case 0x0008:
                    {
                        if (count < 16)
                        {
                            break;
                        }
                        int hostId = deviceId;
                        byte[] status = new byte[dataLength];
                        Buffer.BlockCopy(data, 16, status, 0, dataLength);
                        BoardStatusReceived?.Invoke(hostId, status);
                    }
                    break;
                case 0x00f0:
                    {
                        if (count < 16)
                        {
                            break;
                        }
                        byte messageType = data[12];
                        string typeHex = messageType.ToString("X2");
                        string message = Encoding.Default.GetString(data, 16, dataLength) + typeHex;
                        MessageReceived?.Invoke(message);
                    }
                    break;
                case 0x0005:
                    {
                        if (count < 62 * 2 + 20)
                        {
                            break;
                        }
                        byte[] status = new byte[62 * 2];
                        Buffer.BlockCopy(data, 20, status, 0, 62 * 2);
                        RollingDataReceived?.Invoke(status);
                    }
                    break;
                default:
                    break;
            }
        }

        private void SocketReceivingProcedure()
        {
            if (_isEnabledA)
            {
                _clientA.Start(_ipAddressA, _portA);
            }

            if (_isEnabledB)
            {
                _clientB.Start(_ipAddressB, _portB);
            }

            int receiveCountA = 0;
            int receiveCountB = 0;

            while (!_cancellation.Token.IsCancellationRequested)
            {
                if (_isEnabledA)
                {
                    if (_receiveCountA == receiveCountA)
                    {
                        _errorCountA += 1;
                    }
                    else
                    {
                        receiveCountA = _receiveCountA;
                        _errorCountA = 0;
                    }

                    if (_errorCountA > _idleCount)
                    {
                        _errorCountA = 0;
                        if (_isConnectedA)
                        {
                            _isConnectedA = false;
                            ErrorOccured?.Invoke("A机:" + "数据接收超时");
                        }
                        _isConnectedA = false;
                        _clientA.Restart(_ipAddressA, _portA);

                        if (!_isConnectedB && IsConnected)
                        {
                            IsConnected = false;
                            ClientDisconnected?.Invoke();
                        }
                    }
                }

                if (_isEnabledB)
                {
                    if (_receiveCountB == receiveCountB)
                    {
                        _errorCountB += 1;
                    }
                    else
                    {
                        receiveCountB = _receiveCountB;
                        _errorCountB = 0;
                    }

                    if (_errorCountB > _idleCount)
                    {
                        _errorCountB = 0;
                        if(_isConnectedB)
                        {
                            _isConnectedB = false;
                            ErrorOccured?.Invoke("B机:" + "数据接收超时");
                        }
                        _clientB.Restart(_ipAddressB, _portB);

                        if (!_isConnectedA && IsConnected)
                        {
                            IsConnected = false;
                            ClientDisconnected?.Invoke();
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }
        public void Dispose()
        {
            _cancellation.Cancel();

            _clientA.Dispose();
            _clientB.Dispose();
        }
    }
}
