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
    internal class Status
    {
        private readonly SocketTCPClient _clientA;
        private readonly SocketTCPClient _clientB;
        private readonly CancellationTokenSource _cancellation;
        private readonly Task _task;

        private IPAddress _ipAddressA = null;
        private const int _portA = 1000;
        private IPAddress _ipAddressB = null;
        private const int _portB = 1000;

        private int _errorCountA = 0;
        private int _errorCountB = 0;
        private bool _isEnabledA = false;
        private bool _isEnabledB = false;
        private bool _isConnectedA = false;
        private bool _isConnectedB = false;

        public Status()
        {
            _clientA = new SocketTCPClient();
            _clientB = new SocketTCPClient();

            SetClient();

            _cancellation = new CancellationTokenSource();
            _task = new Task(SocketReceivingProcedure, _cancellation.Token);
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
            });

            _clientA.SetErrorHandler((object sender, Exception e, SocketError error, string message) =>
            {
                _errorCountA++;
            });

            _clientA.SetReceiveHandler((EndPoint endPoint, byte[] buffer, int offset, int count) =>
            {
                if (_isConnectedA && _clientA.IsConnected)
                {
                    DataReceived(buffer, offset, count);
                }
            });

            _clientB.SetConnectHandler((bool flag) =>
            {
                _isConnectedB = flag;
            });

            _clientB.SetErrorHandler((object sender, Exception e, SocketError error, string message) =>
            {
                _errorCountB++;
            });

            _clientB.SetReceiveHandler((EndPoint endPoint, byte[] buffer, int offset, int count) =>
            {
                if (_isConnectedB && _clientB.IsConnected)
                {
                    DataReceived(buffer, offset, count);
                }
            });
        }

        private void DataReceived(byte[] buffer, int offset, int count)
        {
            byte[] data = new byte[count];
            Buffer.BlockCopy(buffer, offset, data, 0, count);

            ushort type = BitConverter.ToUInt16(data, 2);
            switch (type)
            {
                case 0x0001:
                    {
                        
                    }
                    break;
                case 0x00f0:
                    {
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

            while (!_cancellation.Token.IsCancellationRequested)
            {
                if (_errorCountA > 5)
                {
                    _errorCountA = 0;
                    _isConnectedA = false;
                    _clientA.Restart(_ipAddressA, _portA);
                }

                if (_errorCountB > 5)
                {
                    _errorCountA = 0;
                    _isConnectedB = false;
                    _clientB.Restart(_ipAddressB, _portB);
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
