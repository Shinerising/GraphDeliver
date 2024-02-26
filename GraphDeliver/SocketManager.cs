﻿using System;
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
        private readonly SocketTCPClient _clientA;
        private readonly SocketTCPClient _clientB;
        private readonly CancellationTokenSource _cancellation;
        private readonly Task _task;

        private readonly IPAddress _ipAddressA;
        private readonly int _portA = 1000;
        private readonly IPAddress _ipAddressB;
        private readonly int _portB = 1000;

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

        public SocketManager(IPAddress ipAddressA, IPAddress ipAddressB, int portA = 1000, int portB = 1000)
        {
            _clientA = new SocketTCPClient();
            _clientB = new SocketTCPClient();

            _isEnabledA = ipAddressA != null;
            _isEnabledB = ipAddressB != null;

            _ipAddressA = ipAddressA;
            _ipAddressB = ipAddressB;

            _portA = portA;
            _portB = portB;

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
                    DataReceived(buffer, offset + 11, count - 11);
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
                    DataReceived(buffer, offset + 11, count - 11);
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

                while (_packageIndexSet.Count > 200)
                {
                    _packageIndexSet.Remove(_packageIndexSet.First());
                }

                return false;
            }
        }

        private void DataReceived(byte[] buffer, int offset, int count)
        {
            if (buffer == null || buffer.Length == 0 || count == 0 || count > buffer.Length || count < 18)
            {
                return;
            }

            byte[] data = new byte[count];
            Buffer.BlockCopy(buffer, offset, data, 0, count);

            int index = BitConverter.ToInt32(data, 0);
            int length = BitConverter.ToInt32(data, 4);
            ushort type = BitConverter.ToUInt16(data, 10);
            int deviceId = BitConverter.ToUInt16(data, 12);
            int dataLength = BitConverter.ToUInt16(data, 14);

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
                        bool isCritical = false;
                        switch (data[12])
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
                        string message = Encoding.ASCII.GetString(data, 16, dataLength) + (isCritical ? "$" : "");
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
                        _errorCountB = 0;
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
