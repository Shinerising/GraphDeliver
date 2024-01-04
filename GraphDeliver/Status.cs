using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace GraphDeliver
{
    public class Status : CustomINotifyPropertyChanged
    {
        private readonly SerialManager _serialManager;
        private readonly SocketManager _socketManager;
        private readonly DataManager _dataManager;
        public Status()
        {
            _serialManager = new SerialManager();

            CommunicationClient.GetAddressFromString(AppSettings.HostAddressA, out IPAddress ipAddressA, out int portA);
            CommunicationClient.GetAddressFromString(AppSettings.HostAddressB, out IPAddress ipAddressB, out int portB);
            _socketManager = new SocketManager(ipAddressA, ipAddressB, portA, portB);

            _dataManager = new DataManager();

            _serialManager.ClientConnected += SerialManager_ClientConnected;
            _serialManager.ClientDisconnected += SerialManager_ClientDisconnected;
            _serialManager.ErrorOccured += SerialManager_ErrorOccured;
            _serialManager.DataRequested += SerialManager_DataRequested;
            _serialManager.DataSended += SerialManager_DataSended;

            _socketManager.ClientConnected += SocketManager_ClientConnected;
            _socketManager.ClientDisconnected += SocketManager_ClientDisconnected;
            _socketManager.ErrorOccured += SocketManager_ErrorOccured;
            _socketManager.DeviceStatusReceived += SocketManager_DeviceStatusReceived;
            _socketManager.BoardStatusReceived += SocketManager_BoardStatusReceived;
            _socketManager.MessageReceived += SocketManager_MessageReceived;
        }

        public void Dispose()
        {
            _serialManager.Dispose();
            _socketManager.Dispose();
        }

        private void SerialManager_ClientConnected()
        {
            AddLog("串口通信", "串口通信客户端已启动连接！");
        }

        private void SerialManager_ClientDisconnected()
        {
            AddLog("串口通信", "串口通信客户端已断开连接！");
        }

        private void SocketManager_ErrorOccured(string message)
        {
            AddLog("网络异常", message);
        }

        private void SerialManager_ErrorOccured(string message)
        {
            AddLog("串口异常", message);
        }

        private void SerialManager_DataSended(byte[] buffer)
        {
            string message = $"数据发送成功！数据长度：{buffer.Length}";
            AddRecord(message);
        }

        private void SerialManager_DataRequested(SerialManager.DataContext context)
        {
            if (_dataManager.UpdateTime == context.UpdateTime)
            {
                return;
            }

            context.Data = _dataManager.PackAllData();
            context.UpdateTime = _dataManager.UpdateTime;
        }

        private void SocketManager_ClientConnected()
        {
            AddLog("网络通信", "网络通信客户端已启动连接！");
        }

        private void SocketManager_ClientDisconnected()
        {
            _dataManager.ClearData();

            AddLog("网络通信", "网络通信客户端已断开连接！");
        }

        private void SocketManager_DeviceStatusReceived(int deviceId, byte[] buffer)
        {
            _dataManager.ApplyDeviceStatusData(deviceId, buffer);
        }

        private void SocketManager_BoardStatusReceived(int hostId, byte[] buffer)
        {
            _dataManager.ApplyBoardStatusData(hostId, buffer);
        }

        private void SocketManager_MessageReceived(string message)
        {
            _dataManager.PushMessage(message);
        }

        public void DelayOpen(int delay, int wait = 1000, int retry = 0)
        {
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(delay);

                if (_serialManager.IsOpen)
                {
                    return;
                }

                AddLog("操作记录", "正在自动打开串行端口：" + _serialManager.PortInfo);
                _serialManager.OpenPort();

                while (!_serialManager.IsOpen && retry > 0)
                {
                    AddLog("操作记录", string.Format("串口打开失败，{0}毫秒后自动重试", wait));

                    Thread.Sleep(wait);

                    AddLog("操作记录", "正在重试打开串行端口：" + _serialManager.PortInfo);
                    _serialManager.OpenPort();

                    retry -= 1;
                }
            });
        }

        /// <summary>
        /// 工作日志显示上限
        /// </summary>
        private const int LogLimit = 200;
        /// <summary>
        /// 工作日志列表
        /// </summary>
        public ObservableCollection<string> LogList { get; set; } = new ObservableCollection<string>();
        /// <summary>
        /// 传输记录显示上限
        /// </summary>
        private const int RecordLimit = 100;
        /// <summary>
        /// 传输记录列表
        /// </summary>
        public ObservableCollection<string> RecordList { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// 添加一条日志
        /// </summary>
        /// <param name="message">日志文本</param>
        public void AddLog(string message)
        {
            try
            {
                Application.Current.Dispatcher?.Invoke((Action)(() =>
                {
                    LogList.Add(message);

                    while (LogList.Count > LogLimit)
                    {
                        LogList.RemoveAt(0);
                    }
                }));
            }
            catch
            {

            }
        }
        /// <summary>
        /// 添加一条工作日志信息
        /// </summary>
        /// <param name="brief">信息摘要</param>
        /// <param name="message">信息文本</param>
        public void AddLog(string brief, string message)
        {
            string log = string.Format("[{0}] {1} {2}", DateTime.Now.ToString("MM-dd HH:mm:ss"), brief, message);

            try
            {
                Application.Current.Dispatcher?.Invoke((Action)(() =>
                {
                    LogList.Add(log);

                    while (LogList.Count > LogLimit)
                    {
                        LogList.RemoveAt(0);
                    }
                }));

                LogHelper.PushLog("SYSTEM", log);
            }
            catch
            {

            }
        }

        /// <summary>
        /// 添加一条记录
        /// </summary>
        /// <param name="message">记录文本</param>
        public void AddRecord(string message)
        {
            string record = string.Format("[{0}] {1}", DateTime.Now.ToString("MM-dd HH:mm:ss"), message);

            try
            {
                Application.Current.Dispatcher?.Invoke((Action)(() =>
                {
                    RecordList.Add(record);

                    while (RecordList.Count > RecordLimit)
                    {
                        RecordList.RemoveAt(0);
                    }
                }));

                LogHelper.PushLog("RECORD", record);
            }
            catch
            {

            }
        }

        /// <summary>
        /// 清除工作日志
        /// </summary>
        public void ClearLog()
        {
            LogList.Clear();
        }

        /// <summary>
        /// 清除传输记录
        /// </summary>
        public void ClearRecord()
        {
            RecordList.Clear();
        }
    }
}
