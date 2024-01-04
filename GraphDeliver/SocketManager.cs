using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GraphDeliver
{
    internal class SocketManager
    {
        private readonly SocketTCPClient _clientA;
        private readonly SocketTCPClient _clientB;
        private readonly CancellationTokenSource _cancellation;
        private readonly Task _task;

        private IPAddress _ipAddressA = null;
        private const int _portA = 1000;
        private IPAddress _ipAddressB = null;
        private const int _portB = 1000;

        private int _receiveCountA = 0;
        private int _receiveCountB = 0;
        private int _errorCountA = 0;
        private int _errorCountB = 0;
        private bool _isEnabledA = false;
        private bool _isEnabledB = false;
        private bool _isConnectedA = false;
        private bool _isConnectedB = false;

        public event Action ClientConnected;
        public event Action ClientDisconnected;
        public event Action<string> ErrorOccured;
        public event Action<int, byte[]> DeviceStatusReceived;
        public event Action<string> MessageReceived;
        public event Action<int, byte[]> BoardStatusReceived;

        public bool IsEnabledA => _isEnabledA;
        public bool IsEnabledB => _isEnabledB;
        public bool IsConnectedA => _clientA != null && _clientA.IsConnected && _isConnectedA;
        public bool IsConnectedB => _clientB != null && _clientB.IsConnected && _isConnectedB;
        public string NameA => _clientB != null ? $"{_ipAddressA}:{_portA}" : "未启用";
        public string NameB => _clientB != null ? $"{_ipAddressB}:{_portB}" : "未启用";
        public bool IsConnected { get; set; }

        public SocketManager()
        {
            _clientA = new SocketTCPClient();
            _clientB = new SocketTCPClient();

            SetClient();

            _cancellation = new CancellationTokenSource();
            _task = new Task(SocketReceivingProcedure, _cancellation.Token);
            _task.Start();
        }

        private void SetClient()
        {
            string ipAddressA = "";
            string ipAddressB = "";

            if (!string.IsNullOrEmpty(ipAddressA))
            {
                _isEnabledA = IPAddress.TryParse(ipAddressA, out _ipAddressA);
            }

            if (!string.IsNullOrEmpty(ipAddressB))
            {
                _isEnabledB = IPAddress.TryParse(ipAddressB, out _ipAddressB);
            }

            _clientA.SetConnectHandler((bool flag) =>
            {
                _isConnectedA = flag;
                if (flag && !IsConnected)
                {
                    IsConnected = true;
                    ClientConnected?.Invoke();
                }
            });

            _clientA.SetErrorHandler((object sender, Exception e, SocketError error, string message) =>
            {
                _errorCountA++;
                message = message ?? e?.Message ?? error.ToString();
                ErrorOccured?.Invoke(message);
            });

            _clientA.SetReceiveHandler((EndPoint endPoint, byte[] buffer, int offset, int count) =>
            {
                if (_clientA.IsConnected)
                {
                    _receiveCountA += 1;
                    DataReceived(buffer, offset, count);
                }
            });

            _clientB.SetConnectHandler((bool flag) =>
            {
                _isConnectedB = flag;
                if (flag && !IsConnected)
                {
                    IsConnected = true;
                    ClientConnected?.Invoke();
                }
            });

            _clientB.SetErrorHandler((object sender, Exception e, SocketError error, string message) =>
            {
                _errorCountB++;
                message = message ?? e?.Message ?? error.ToString();
                ErrorOccured?.Invoke(message);
            });

            _clientB.SetReceiveHandler((EndPoint endPoint, byte[] buffer, int offset, int count) =>
            {
                if (_clientB.IsConnected)
                {
                    _receiveCountB += 1;
                    DataReceived(buffer, offset, count);
                }
            });
        }

        private void DataReceived(byte[] buffer, int offset, int count)
        {
            if (buffer == null || buffer.Length == 0 || count == 0 || count > buffer.Length || count < 2)
            {
                return;
            }

            byte[] data = new byte[count];
            Buffer.BlockCopy(buffer, offset, data, 0, count);

            ushort type = BitConverter.ToUInt16(data, 2);
            switch (type)
            {
                case 0x0001:
                    {
                        if (count < 8)
                        {
                            break;
                        }
                        int deviceId = data[4];
                        byte[] status = new byte[count - 8];
                        Buffer.BlockCopy(data, 8, status, 0, count - 8);
                        DeviceStatusReceived?.Invoke(deviceId, status);
                    }
                    break;
                case 0x0008:
                    {
                        if (count < 8)
                        {
                            break;
                        }
                        int hostId = data[4];
                        byte[] status = new byte[count - 8];
                        Buffer.BlockCopy(data, 8, status, 0, count - 8);
                        BoardStatusReceived?.Invoke(hostId, status);
                    }
                    break;
                case 0x00f0:
                    {
                        if (count < 8)
                        {
                            break;
                        }
                        bool isCritical = false;
                        switch (data[4])
                        {
                            case 9:
                            case 11:
                            case 12:
                            case 14:
                            case 15:
                            case 21:
                            case 29:
                            case 30:
                            case 34:
                            case 40:
                            case 41:
                            case 42:
                            case 43:
                            case 55:
                            case 83:
                                isCritical = true;
                                break;
                            default:
                                break;
                        }
                        string message = Encoding.ASCII.GetString(data, 8, count - 8) + (isCritical ? "$" : "");
                        MessageReceived?.Invoke(message);
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

                    if (_errorCountA > 5)
                    {
                        _errorCountA = 0;
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

                    if (_errorCountB > 5)
                    {
                        _errorCountA = 0;
                        _isConnectedB = false;
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
