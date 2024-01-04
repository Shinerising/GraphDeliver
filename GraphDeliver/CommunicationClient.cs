using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace GraphDeliver
{
    /// <summary>
    /// Socket数据通信接口
    /// </summary>
    public interface ISocketCommunication
    {
        /// <summary>
        /// 客户端名称
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// 通信地址
        /// </summary>
        EndPoint EndPoint { get; set; }
        /// <summary>
        /// 数据发送计数
        /// </summary>
        long SendCount { get; }
        /// <summary>
        /// 数据接收计数
        /// </summary>
        long ReceiveCount { get; }
        /// <summary>
        /// 数据发送量
        /// </summary>
        long SendSize { get; }
        /// <summary>
        /// 数据接收量
        /// </summary>
        long ReceiveSize { get; }
        /// <summary>
        /// 是否已连接
        /// </summary>
        bool IsConnected { get; }
        /// <summary>
        /// 启动数据通信接口
        /// </summary>
        /// <param name="iPAddress">IP地址</param>
        /// <param name="port">网络端口</param>
        /// <returns>是否启动成功</returns>
        bool Start(IPAddress iPAddress, int port);
        /// <summary>
        /// 重启数据通信接口
        /// </summary>
        /// <param name="iPAddress">IP地址</param>
        /// <param name="port">网络端口</param>
        /// <returns>是否重启成功</returns>
        bool Restart(IPAddress iPAddress, int port);
        /// <summary>
        /// 断开数据通信接口
        /// </summary>
        /// <param name="isReusable">是否可重用</param>
        /// <returns>是否断开成功</returns>
        bool Disconnect(bool isReusable);
        /// <summary>
        /// 向目标网络地址发送数据
        /// </summary>
        /// <param name="endpoint">远端网络地址</param>
        /// <param name="buffer">数据字节</param>
        /// <param name="index">偏移量</param>
        /// <param name="length">数据长度</param>
        /// <returns>是否发送成功</returns>
        bool Send(EndPoint endpoint, byte[] buffer, int index = 0, int length = -1);
        /// <summary>
        /// 向默认网络地址发送数据
        /// </summary>
        /// <param name="buffer">数据字节</param>
        /// <param name="index">偏移量</param>
        /// <param name="length">数据长度</param>
        /// <returns>是否发送成功</returns>
        bool Send(byte[] buffer, int index = 0, int length = -1);
        /// <summary>
        /// 设置数据接收处理方法
        /// </summary>
        /// <param name="action">数据处理方法</param>
        /// <remarks>方法参数说明：终端地址、数据字节、偏移量、数据长度</remarks>
        void SetReceiveHandler(Action<EndPoint, byte[], int, int> action);
        /// <summary>
        /// 设置错误及异常处理方法
        /// </summary>
        /// <param name="action">错误处理方法</param>
        /// <remarks>方法参数说明：目标对象、异常对象、网络错误值、错误说明</remarks>
        void SetErrorHandler(Action<object, Exception, SocketError, string> action);
        /// <summary>
        /// 释放托管资源
        /// </summary>
        void Dispose();
    }

    /// <summary>
    /// 串口数据通信接口
    /// </summary>
    public interface ISerialCommunication
    {
        /// <summary>
        /// 客户端名称
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// 数据发送计数
        /// </summary>
        long SendCount { get; }
        /// <summary>
        /// 数据接收计数
        /// </summary>
        long ReceiveCount { get; }
        /// <summary>
        /// 数据发送量
        /// </summary>
        long SendSize { get; }
        /// <summary>
        /// 数据接收量
        /// </summary>
        long ReceiveSize { get; }
        /// <summary>
        /// 是否已连接
        /// </summary>
        bool IsConnected { get; }
        /// <summary>
        /// 启动数据通信接口
        /// </summary>
        /// <param name="portName">串口名称</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">奇偶校验位</param>
        /// <param name="databits">数据位</param>
        /// <param name="stopBits">停止位</param>
        /// <returns>是否启动成功</returns>
        bool Start(string portName, int baudRate, Parity parity = Parity.None, int databits = 8, StopBits stopBits = StopBits.One);
        /// <summary>
        /// 重启数据通信接口
        /// </summary>
        /// <param name="portName">串口名称</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">奇偶校验位</param>
        /// <param name="databits">数据位</param>
        /// <param name="stopBits">停止位</param>
        /// <returns>是否重启成功</returns>
        bool Restart(string portName, int baudRate, Parity parity = Parity.None, int databits = 8, StopBits stopBits = StopBits.One);
        /// <summary>
        /// 断开数据通信接口
        /// </summary>
        /// <returns>是否断开成功</returns>
        bool Disconnect();
        /// <summary>
        /// 向默认网络地址发送数据
        /// </summary>
        /// <param name="buffer">数据字节</param>
        /// <param name="index">偏移量</param>
        /// <param name="length">数据长度</param>
        /// <returns>是否发送成功</returns>
        bool Send(byte[] buffer, int index = 0, int length = -1);
        /// <summary>
        /// 设置数据接收处理方法
        /// </summary>
        /// <param name="action">数据处理方法</param>
        /// <remarks>方法参数说明：数据字节、偏移量、数据长度</remarks>
        void SetReceiveHandler(Action<byte[], int, int> action);
        /// <summary>
        /// 设置错误及异常处理方法
        /// </summary>
        /// <param name="action">错误处理方法</param>
        /// <remarks>方法参数说明：目标对象、串口通讯错误、异常对象、错误说明</remarks>
        void SetErrorHandler(Action<object, SerialError, Exception, string> action);
        /// <summary>
        /// 释放托管资源
        /// </summary>
        void Dispose();
    }

    /// <summary>
    /// 串口数据通讯客户端
    /// </summary>
    internal class SerialClient : ISerialCommunication
    {
        /// <summary>
        /// 默认缓冲区大小
        /// </summary>
        private const int BufferSize = 102400;
        /// <summary>
        /// 异步任务对象
        /// </summary>
        private readonly Task task;
        /// <summary>
        /// 任务取消标记
        /// </summary>
        private readonly CancellationTokenSource cancellation;
        /// <summary>
        /// 串口通信对象
        /// </summary>
        private readonly SerialPort port;
        /// <summary>
        /// 循环处理时间间隔
        /// </summary>
        private readonly int interval;
        /// <summary>
        /// 包头标记
        /// </summary>
        private readonly byte headLabel;
        /// <summary>
        /// 包尾标记
        /// </summary>
        private readonly byte tailLabel;
        /// <summary>
        /// 转义标记
        /// </summary>
        private readonly byte escapeLabel;
        /// <summary>
        /// 转义字典
        /// </summary>
        private readonly Dictionary<byte, byte> escapeDictionary;
        /// <summary>
        /// 反转义字典
        /// </summary>
        private readonly Dictionary<byte, byte> unescapeDictionary;
        /// <summary>
        /// 包头识别数据
        /// </summary>
        private readonly byte[] headData;
        /// <summary>
        /// 包尾识别数据
        /// </summary>
        private readonly byte[] tailData;
        /// <summary>
        /// 取消转义
        /// </summary>
        public bool IsEscapeDisabled { get; private set; } = false;
        public enum WorkingMode
        {
            Default,
            CIPS,
        }
        /// <summary>
        /// 特殊模式
        /// </summary>
        public WorkingMode Mode { get; set; } = WorkingMode.Default;
        /// <summary>
        /// 通信超时毫秒数
        /// </summary>
        private readonly int TimeoutMilliseconds;

        /// <summary>
        /// 接收数据处理方法
        /// </summary>
        private Action<byte[], int, int> ReceiveHandler;
        /// <summary>
        /// 错误处理方法
        /// </summary>
        private Action<object, SerialError, Exception, string> ErrorHandler;
        /// <summary>
        /// 对象名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 数据发送计数
        /// </summary>
        public long SendCount { get; private set; }
        /// <summary>
        /// 数据接收计数
        /// </summary>
        public long ReceiveCount { get; private set; }
        /// <summary>
        /// 数据发送量
        /// </summary>
        public long SendSize { get; private set; }
        /// <summary>
        /// 数据接收量
        /// </summary>
        public long ReceiveSize { get; private set; }
        /// <summary>
        /// 是否正在通信
        /// </summary>
        public bool IsConnected => port.IsOpen && !isTimeout;
        /// <summary>
        /// 是否已打开端口
        /// </summary>
        public bool IsOpen => port.IsOpen;
        /// <summary>
        /// 通信是否超时
        /// </summary>
        private bool isTimeout = true;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="head">包头标志</param>
        /// <param name="tail">包尾标志</param>
        /// <param name="escape">转义标志</param>
        /// <param name="escapeDict">转义字典</param>
        /// <param name="timeout">超时时间(超过此时间间隔未收到合法数据视为通讯断开)</param>
        /// <param name="delay">循环间隔</param>
        /// <remarks>
        /// 转义字典按此定义：0x0A->0x0C01, 0x0B->0x0C02, 0x0C->0x0C03 => {0x0A, 0x01}, {0x0B, 0x02}, {0x0C, 0x03}
        /// </remarks>
        public SerialClient(byte head, byte tail, byte escape, Dictionary<byte, byte> escapeDict, int timeout = 1000, int delay = 100)
        {
            IsEscapeDisabled = false;
            headLabel = head;
            tailLabel = tail;
            escapeLabel = escape;
            escapeDictionary = escapeDict;
            unescapeDictionary = escapeDict.GroupBy(x => x.Value, x => x.Key).ToDictionary(g => g.Key, g => g.ToList().FirstOrDefault());

            TimeoutMilliseconds = timeout;

            interval = delay;
            port = new SerialPort();
            port.ErrorReceived += Port_ErrorReceived;

            cancellation = new CancellationTokenSource();
            task = new Task(ReceiveDataProcudure, cancellation.Token, TaskCreationOptions.LongRunning);
            task.Start();
        }

        public SerialClient(byte[] head, byte[] tail, WorkingMode mode, int timeout = 1000, int delay = 100)
        {
            IsEscapeDisabled = true;
            headData = head ?? new byte[0];
            tailData = tail ?? new byte[0];
            Mode = mode;

            TimeoutMilliseconds = timeout;

            interval = delay;
            port = new SerialPort();
            port.ErrorReceived += Port_ErrorReceived;

            cancellation = new CancellationTokenSource();
            if (mode == WorkingMode.CIPS)
            {
                task = new Task(ReceiveDataProcudure_CIPS, cancellation.Token, TaskCreationOptions.LongRunning);
            }
            else
            {
                task = new Task(ReceiveDataProcudureWithoutEscape, cancellation.Token, TaskCreationOptions.LongRunning);
            }
            task.Start();
        }

        /// <summary>
        /// 用于收取数据的子线程过程
        /// </summary>
        private void ReceiveDataProcudure()
        {
            byte currentData = 0;

            byte[] buffer = new byte[BufferSize];
            int bufferIndex = -1;
            int bufferCount = 0;

            byte[] readBuffer = new byte[BufferSize];

            long timestamp = 0;
            Stopwatch watch = new Stopwatch();
            watch.Start();

            while (!cancellation.Token.IsCancellationRequested)
            {
                if (port.IsOpen && port.BytesToRead > 0)
                {
                    int read = -1;
                    try
                    {
                        read = port.Read(readBuffer, 0, BufferSize);
                    }
                    catch (Exception e)
                    {
                        ErrorHandler?.Invoke(this, 0, e, e.Message);
                    }
                    if (read != -1)
                    {
                        int index = 0;
                        while (index < read && !cancellation.Token.IsCancellationRequested)
                        {
                            byte previousData = currentData;
                            currentData = readBuffer[index];
                            index += 1;

                            bool isNewData = false;
                            byte newData = 0;

                            if (currentData == headLabel)
                            {
                                bufferIndex = 0;
                                bufferCount = 0;
                            }
                            else if (bufferIndex != -1)
                            {
                                if (currentData == tailLabel)
                                {
                                    ReceiveCount += 1;
                                    ReceiveSize += bufferCount;
                                    ReceiveHandler?.Invoke(buffer, 0, bufferCount);

                                    timestamp = watch.ElapsedMilliseconds;

                                    buffer = new byte[BufferSize];
                                    bufferIndex = -1;
                                    bufferCount = 0;
                                }
                                else if (previousData == escapeLabel)
                                {
                                    newData = currentData;
                                    isNewData = true;
                                    if (unescapeDictionary.ContainsKey(newData))
                                    {
                                        newData = unescapeDictionary[newData];
                                    }
                                }
                                else if (currentData != escapeLabel)
                                {
                                    newData = currentData;
                                    isNewData = true;
                                }
                            }

                            if (isNewData)
                            {
                                if (bufferIndex == BufferSize)
                                {
                                    bufferIndex = -1;
                                    bufferCount = 0;
                                    ErrorHandler?.Invoke(this, 0, new IndexOutOfRangeException(), "数据超出缓冲区长度");
                                }
                                else
                                {
                                    buffer[bufferIndex] = newData;
                                    bufferIndex += 1;
                                    bufferCount += 1;
                                }
                            }
                        }
                    }
                }

                long timespan = watch.ElapsedMilliseconds - timestamp;
                if (isTimeout && timespan <= TimeoutMilliseconds && timestamp != 0)
                {
                    isTimeout = false;
                }
                else if (!isTimeout && timespan > TimeoutMilliseconds)
                {
                    isTimeout = true;
                }

                Thread.Sleep(interval);
            }

            watch.Stop();
        }

        /// <summary>
        /// 用于收取数据的子线程过程
        /// </summary>
        private void ReceiveDataProcudureWithoutEscape()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 用于收取数据的子线程过程(CIPS)
        /// </summary>
        private void ReceiveDataProcudure_CIPS()
        {
            byte[] buffer = new byte[BufferSize];
            int bufferCount = 0;
            int targetHead = -1;
            int targetCount = -1;

            byte[] readBuffer = new byte[BufferSize];

            long timestamp = 0;
            Stopwatch watch = new Stopwatch();
            watch.Start();

            while (!cancellation.Token.IsCancellationRequested)
            {
                if (port.IsOpen && port.BytesToRead > 0)
                {
                    int read = -1;
                    try
                    {
                        read = port.Read(readBuffer, 0, BufferSize);
                    }
                    catch (Exception e)
                    {
                        ErrorHandler?.Invoke(this, 0, e, e.Message);
                    }
                    if (read != -1)
                    {
                        int index = 0;
                        while (index < read && !cancellation.Token.IsCancellationRequested)
                        {
                            if (bufferCount == BufferSize)
                            {
                                bufferCount = 0;
                                ErrorHandler?.Invoke(this, 0, new IndexOutOfRangeException(), "数据超出缓冲区长度");
                            }

                            buffer[bufferCount] = readBuffer[index];
                            bufferCount += 1;
                            index += 1;

                            if (targetHead != -1)
                            {
                                if (targetCount >= 0)
                                {
                                    if (bufferCount >= targetHead + 18 + targetCount)
                                    {
                                        ReceiveCount += 1;
                                        ReceiveSize += bufferCount;
                                        ReceiveHandler?.Invoke(buffer, targetHead, 18 + targetCount);

                                        timestamp = watch.ElapsedMilliseconds;

                                        buffer = new byte[BufferSize];
                                        bufferCount = 0;
                                        targetHead = -1;
                                        targetCount = -1;
                                    }
                                }
                                else if (bufferCount >= targetHead + 9)
                                {
                                    targetCount = BitConverter.ToUInt16(buffer, targetHead + 7);
                                }
                            }
                            else if (bufferCount >= headData.Length)
                            {
                                bool flag = true;
                                for (int i = 0; i < headData.Length; i += 1)
                                {
                                    if (buffer[bufferCount - 1 - i] != headData[headData.Length - 1 - i])
                                    {
                                        flag = false;
                                        break;
                                    }
                                }
                                if (flag)
                                {
                                    targetHead = bufferCount;
                                    targetCount = -1;
                                }
                            }
                        }
                    }
                }

                long timespan = watch.ElapsedMilliseconds - timestamp;
                if (isTimeout && timespan <= TimeoutMilliseconds && timestamp != 0)
                {
                    isTimeout = false;
                }
                else if (!isTimeout && timespan > TimeoutMilliseconds)
                {
                    isTimeout = true;
                }

                Thread.Sleep(interval);
            }

            watch.Stop();
        }

        /// <summary>
        /// 串口通信错误事件响应
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        private void Port_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            ErrorHandler?.Invoke(sender, e.EventType, null, e.ToString());
        }
        /// <summary>
        /// 启动数据通信接口
        /// </summary>
        /// <param name="portName">串口名称</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">奇偶校验位</param>
        /// <param name="databits">数据位</param>
        /// <param name="stopBits">停止位</param>
        /// <returns>是否启动成功</returns>
        public bool Start(string portName, int baudRate, Parity parity = Parity.None, int databits = 8, StopBits stopBits = StopBits.One)
        {
            try
            {
                port.PortName = portName;
                port.BaudRate = baudRate;
                port.Parity = parity;
                port.DataBits = databits;
                port.StopBits = stopBits;
                port.Open();
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, 0, e, e.Message);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 重启数据通信接口
        /// </summary>
        /// <param name="portName">串口名称</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">奇偶校验位</param>
        /// <param name="databits">数据位</param>
        /// <param name="stopBits">停止位</param>
        /// <returns>是否重启成功</returns>
        public bool Restart(string portName, int baudRate, Parity parity = Parity.None, int databits = 8, StopBits stopBits = StopBits.One)
        {
            try
            {
                port.Close();
                port.PortName = portName;
                port.BaudRate = baudRate;
                port.Parity = parity;
                port.DataBits = databits;
                port.StopBits = stopBits;
                port.Open();
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, 0, e, e.Message);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 断开数据通信接口
        /// </summary>
        /// <returns>是否断开成功</returns>
        public bool Disconnect()
        {
            try
            {
                port.Close();
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, 0, e, e.Message);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 设置错误及异常处理方法
        /// </summary>
        /// <param name="action">错误处理方法</param>
        public void SetErrorHandler(Action<object, SerialError, Exception, string> action)
        {
            ErrorHandler = action;
        }
        /// <summary>
        /// 设置数据接收处理方法
        /// </summary>
        /// <param name="action">数据处理方法</param>
        public void SetReceiveHandler(Action<byte[], int, int> action)
        {
            ReceiveHandler = action;
        }
        /// <summary>
        /// 对数据按规定协议进行转义
        /// </summary>
        /// <param name="buffer">原始数据对象</param>
        /// <param name="data">转义后数据对象</param>
        /// <param name="index">偏移量</param>
        /// <param name="length">数据长度</param>
        /// <returns>转义后数据长度</returns>
        private int EscapeData(byte[] buffer, out byte[] data, int index = 0, int length = -1)
        {
            if (IsEscapeDisabled)
            {
                if (length == -1)
                {
                    length = buffer.Length;
                }
                data = new byte[length + headData.Length + tailData.Length];
                if (buffer == null)
                {
                    return -1;
                }

                Buffer.BlockCopy(headData, 0, data, 0, headData.Length);
                Buffer.BlockCopy(buffer, index, data, headData.Length, length);
                Buffer.BlockCopy(tailData, 0, data, length + headData.Length, tailData.Length);

                return length + headData.Length + tailData.Length;
            }

            data = new byte[BufferSize];
            if (buffer == null)
            {
                return -1;
            }
            if (length == -1)
            {
                length = buffer.Length;
            }
            data[0] = headLabel;
            int count = 1;
            for (int offset = index; offset < length; offset += 1)
            {
                byte currentData = buffer[offset];
                if (escapeDictionary.ContainsKey(currentData))
                {
                    data[count] = escapeLabel;
                    data[count + 1] = escapeDictionary[currentData];
                    count += 2;
                }
                else
                {
                    data[count] = currentData;
                    count += 1;
                }
            }
            data[count] = tailLabel;
            count += 1;
            return count;
        }
        /// <summary>
        /// 使用通信接口发送数据
        /// </summary>
        /// <param name="buffer">数据字节</param>
        /// <param name="index">偏移量</param>
        /// <param name="length">数据长度</param>
        /// <returns>是否发送成功</returns>
        public bool Send(byte[] buffer, int index = 0, int length = -1)
        {
            try
            {
                int count = EscapeData(buffer, out byte[] data, index, length);
                port.Write(data, 0, count);
                SendCount += 1;
                SendSize += count;
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, 0, e, e.Message);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 释放托管资源
        /// </summary>
        public void Dispose()
        {
            cancellation.Cancel();
            port?.Dispose();
        }
        /// <summary>
        /// 返回表示当前对象的字符串
        /// </summary>
        /// <returns>当前对象字符串</returns>
        public override string ToString()
        {
            return string.Format("{0}|{1}", base.ToString(), port.PortName.ToString());
        }
    }

    /// <summary>
    /// 数据通信客户端基类
    /// </summary>
    internal abstract class CommunicationClient : ISocketCommunication, IDisposable
    {
        /// <summary>
        /// 客户端名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 断线计数
        /// </summary>
        public int ErrorCount { get; set; }
        /// <summary>
        /// 缓冲区大小
        /// </summary>
        protected const int BufferSize = 102400;
        /// <summary>
        /// 通信地址
        /// </summary>
        public EndPoint EndPoint { get; set; }
        /// <summary>
        /// Socket通信接口
        /// </summary>
        protected Socket socket;
        /// <summary>
        /// Socket协议类型
        /// </summary>
        protected ProtocolType Type;
        /// <summary>
        /// 接收数据处理方法
        /// </summary>
        protected Action<EndPoint, byte[], int, int> ReceiveHandler;
        /// <summary>
        /// 错误处理方法
        /// </summary>
        protected Action<object, Exception, SocketError, string> ErrorHandler;

        /// <summary>
        /// 数据发送计数
        /// </summary>
        protected long sendCount;
        /// <summary>
        /// 数据接收计数
        /// </summary>
        protected long receiveCount;
        /// <summary>
        /// 数据发送计数
        /// </summary>
        public long SendCount => sendCount;
        /// <summary>
        /// 数据接收计数
        /// </summary>
        public long ReceiveCount => receiveCount;
        /// <summary>
        /// 数据发送量
        /// </summary>
        protected long sendSize;
        /// <summary>
        /// 数据接收量
        /// </summary>
        protected long receiveSize;
        /// <summary>
        /// 数据发送量
        /// </summary>
        public long SendSize => sendSize;
        /// <summary>
        /// 数据接收量
        /// </summary>
        public long ReceiveSize => receiveSize;

        /// <summary>
        /// 是否已连接
        /// </summary>
        public virtual bool IsConnected => socket.Connected;
        /// <summary>
        /// 启动数据通信接口
        /// </summary>
        /// <param name="iPAddress">IP地址</param>
        /// <param name="port">网络端口</param>
        /// <returns>是否启动成功</returns>
        public abstract bool Start(IPAddress iPAddress, int port);
        /// <summary>
        /// 重启数据通信接口
        /// </summary>
        /// <param name="iPAddress">IP地址</param>
        /// <param name="port">网络端口</param>
        /// <returns>是否重启成功</returns>
        public abstract bool Restart(IPAddress iPAddress, int port);
        /// <summary>
        /// 断开数据通信接口
        /// </summary>
        /// <param name="isReusable">是否可重用</param>
        /// <returns>是否断开成功</returns>
        public abstract bool Disconnect(bool isReusable);
        /// <summary>
        /// 使用通信接口发送数据
        /// </summary>
        /// <param name="endpoint">远端网络地址</param>
        /// <param name="buffer">数据字节</param>
        /// <param name="index">偏移量</param>
        /// <param name="length">数据长度</param>
        /// <returns>是否发送成功</returns>
        public abstract bool Send(EndPoint endpoint, byte[] buffer, int index = 0, int length = -1);
        /// <summary>
        /// 释放托管资源
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// 返回表示当前对象的字符串
        /// </summary>
        /// <returns>当前对象字符串</returns>
        public override string ToString()
        {
            return string.Format("{0}|{1}", base.ToString(), EndPoint?.ToString());
        }

        /// <summary>
        /// 使用通信接口发送数据
        /// </summary>
        /// <param name="buffer">数据字节</param>
        /// <param name="index">偏移量</param>
        /// <param name="length">数据长度</param>
        /// <returns>是否发送成功</returns>
        public bool Send(byte[] buffer, int index = 0, int length = -1)
        {
            return Send(null, buffer, index, length);
        }

        /// <summary>
        /// 设置数据接收处理方法
        /// </summary>
        /// <param name="action">数据处理方法</param>
        public void SetReceiveHandler(Action<EndPoint, byte[], int, int> action)
        {
            ReceiveHandler = action;
        }

        /// <summary>
        /// 设置错误及异常处理方法
        /// </summary>
        /// <param name="action">错误处理方法</param>
        public void SetErrorHandler(Action<object, Exception, SocketError, string> action)
        {
            ErrorHandler = action;
        }

        /// <summary>
        /// IP地址是否为组播地址
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <returns>是否为组播地址</returns>
        public static bool IsMulticast(IPAddress ip)
        {
            switch (ip.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    return ip.GetAddressBytes()[0] >= 224 && ip.GetAddressBytes()[0] <= 239;
                case AddressFamily.InterNetworkV6:
                    return ip.IsIPv6Multicast;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 从连接地址获取IP地址以及端口号
        /// </summary>
        /// <param name="address">网络连接地址</param>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        /// <returns>是否成功获取</returns>
        /// <example>"localhost" => 127.0.0.1, 80</example>
        /// <example>"http://192.168.1.1:8888" => 192.168.1.1, 8888</example>
        public static bool GetAddressFromString(string address, out IPAddress ip, out int port)
        {
            try
            {
                UriBuilder uriBuilder = new UriBuilder(address);
                ip = Dns.GetHostAddresses(uriBuilder.Uri.Host).Where(item => item.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault();
                port = uriBuilder.Uri.Port;
                return true;
            }
            catch
            {
                ip = null;
                port = 0;
                return false;
            }
        }

        /// <summary>
        /// 获取可用的网络接口IP地址列表
        /// </summary>
        /// <returns>IP地址</returns>
        public static IEnumerable<IPAddress> GetInterfaceIPAddress()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            yield return ip.Address;
                        }
                    }
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// Socket通信TCP协议服务端
    /// </summary>
    internal class SocketTCPServer : CommunicationClient
    {
        /// <summary>
        /// 客户端连接处理参数
        /// </summary>
        private SocketAsyncEventArgs AcceptEventArgs;
        /// <summary>
        /// 客户端列表
        /// </summary>
        private readonly List<RemoteClient> ClientList;
        /// <summary>
        /// 客户端连接处理
        /// </summary>
        private Action<EndPoint> AcceptHandler;
        /// <summary>
        /// 最大同时连接数目
        /// </summary>
        private const int MaxConnection = 1024;

        /// <summary>
        /// 查询任务
        /// </summary>
        private readonly Task task;
        /// <summary>
        /// 任务取消标记
        /// </summary>
        private readonly CancellationTokenSource cancellation;

        /// <summary>
        /// 活动状态的网络客户端数量
        /// </summary>
        public int ClientCount => ClientList == null ? 0 : ClientList.Count;

        /// <summary>
        /// 用于记录网络客户端信息的私有类
        /// </summary>
        private class RemoteClient : IDisposable
        {
            /// <summary>
            /// 网络地址
            /// </summary>
            public EndPoint EndPoint;
            /// <summary>
            /// Socket接口对象
            /// </summary>
            public Socket Socket;
            /// <summary>
            /// 是否需要断开
            /// </summary>
            public bool IsDisconnecting;
            /// <summary>
            /// 数据接收处理参数
            /// </summary>
            public SocketAsyncEventArgs ReceiveEventArgs;
            /// <summary>
            /// 数据发送处理参数
            /// </summary>
            public SocketAsyncEventArgs SendEventArgs;
            /// <summary>
            /// 当前网络地址是否与某地址一致
            /// </summary>
            /// <param name="endpoint">目标网络地址</param>
            /// <returns>是否一致</returns>
            public bool IsEndPointEquals(EndPoint endpoint)
            {
                return EndPoint != null && EndPoint.Equals(endpoint);
            }
            /// <summary>
            /// 释放托管资源
            /// </summary>
            public void Dispose()
            {
                try
                {
                    Socket?.Disconnect(false);
                    Socket?.Dispose();
                    ReceiveEventArgs?.Dispose();
                    SendEventArgs?.Dispose();
                }
                catch
                {

                }
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public SocketTCPServer()
        {
            ClientList = new List<RemoteClient>();

            InitializeEventArgs();

            Type = ProtocolType.Tcp;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, Type);

            cancellation = new CancellationTokenSource();
            task = new Task(() =>
            {
                List<RemoteClient> removeList = new List<RemoteClient>();
                while (!cancellation.Token.IsCancellationRequested)
                {
                    if (removeList.Count > 0)
                    {
                        removeList.Clear();
                    }
                    foreach (RemoteClient client in ClientList)
                    {
                        if (client.IsDisconnecting || !client.Socket.Connected)
                        {
                            removeList.Add(client);
                        }
                    }
                    foreach (RemoteClient client in removeList)
                    {
                        try
                        {
                            ClientList.Remove(client);
                            client.Dispose();
                        }
                        catch (Exception e)
                        {
                            ErrorHandler?.Invoke(this, e, 0, null);
                        }
                    }
                    Thread.Sleep(3000);
                }
            }, cancellation.Token, TaskCreationOptions.LongRunning);
            task.Start();
        }
        /// <summary>
        /// 初始化处理参数
        /// </summary>
        private void InitializeEventArgs()
        {
            AcceptEventArgs = new SocketAsyncEventArgs();
            AcceptEventArgs.Completed += Accept_Completed;
        }

        /// <summary>
        /// 设置客户端连接处理方法
        /// </summary>
        /// <param name="action">事件处理方法</param>
        public void SetAcceptHandler(Action<EndPoint> action)
        {
            AcceptHandler = action;
        }

        /// <summary>
        /// 客户端连接处理
        /// </summary>
        /// <param name="args">操作参数</param>
        private void AcceptClient(SocketAsyncEventArgs args)
        {
            try
            {
                Socket client = args.AcceptSocket;
                if (client != null && client.Connected)
                {
                    SocketAsyncEventArgs receiveEventArgs = new SocketAsyncEventArgs();
                    receiveEventArgs.SetBuffer(new byte[BufferSize], 0, BufferSize);
                    receiveEventArgs.Completed += Receive_Completed;
                    SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
                    sendEventArgs.SetBuffer(new byte[BufferSize], 0, BufferSize);
                    sendEventArgs.Completed += Send_Completed;
                    RemoteClient remoteClient = ClientList.Where(item => item.IsEndPointEquals(client.RemoteEndPoint)).FirstOrDefault();
                    if (remoteClient != null)
                    {
                        remoteClient.EndPoint = null;
                        remoteClient.IsDisconnecting = true;
                    }
                    if (ClientList.Count > MaxConnection)
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                    ClientList.Add(new RemoteClient() { EndPoint = client.RemoteEndPoint, Socket = client, ReceiveEventArgs = receiveEventArgs, SendEventArgs = sendEventArgs });

                    AcceptHandler?.Invoke(client.RemoteEndPoint);

                    if (!client.ReceiveAsync(receiveEventArgs))
                    {
                        Receive_Completed(client, receiveEventArgs);
                    }
                }
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, e, 0, null);
            }
        }

        /// <summary>
        /// 客户端连接完成事件
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="args">事件参数</param>
        private void Accept_Completed(object sender, SocketAsyncEventArgs args)
        {
            try
            {
                do
                {
                    if (args.SocketError == SocketError.OperationAborted || args.SocketError == SocketError.ConnectionAborted)
                    {
                        return;
                    }
                    else if (args.SocketError != SocketError.Success)
                    {
                        ErrorHandler?.Invoke(this, null, args.SocketError, null);
                    }
                    else
                    {
                        AcceptClient(args);
                    }
                    args.AcceptSocket = null;
                } while (!socket.AcceptAsync(args));
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, e, 0, null);
            }
        }

        /// <summary>
        /// 数据发送完成事件
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="args">事件参数</param>
        private void Send_Completed(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.OperationAborted || args.SocketError == SocketError.ConnectionAborted)
            {
                return;
            }
            else if (args.SocketError != SocketError.Success)
            {
                ErrorHandler?.Invoke(this, null, args.SocketError, null);
            }
            else
            {
                sendCount += 1;
                sendSize += args.BytesTransferred;
            }
        }

        /// <summary>
        /// 数据接收完成事件
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="args">事件参数</param>
        private void Receive_Completed(object sender, SocketAsyncEventArgs args)
        {
            try
            {
                Socket client = sender as Socket;
                do
                {
                    if (args.SocketError == SocketError.OperationAborted || args.SocketError == SocketError.ConnectionAborted)
                    {
                        return;
                    }
                    else if (args.SocketError != SocketError.Success)
                    {
                        ErrorHandler?.Invoke(this, null, args.SocketError, null);
                    }
                    else
                    {
                        receiveCount += 1;
                        receiveSize += args.BytesTransferred;
                        try
                        {
                            ReceiveHandler?.Invoke(args.RemoteEndPoint, args.Buffer, args.Offset, args.BytesTransferred);
                        }
                        catch (Exception e)
                        {
                            ErrorHandler?.Invoke(this, e, 0, null);
                        }
                    }
                    if (!client.Connected)
                    {
                        return;
                    }
                } while (!client.ReceiveAsync(args));
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, e, 0, null);
            }
        }

        /// <summary>
        /// 启动网络监听
        /// </summary>
        /// <param name="iPAddress">IP地址</param>
        /// <param name="port">网络端口</param>
        /// <returns>是否启动成功</returns>
        public override bool Start(IPAddress iPAddress, int port)
        {
            bool result = true;
            try
            {
                EndPoint = new IPEndPoint(iPAddress, port);
                socket.Bind(EndPoint);
                socket.Listen(MaxConnection);
                if (!socket.AcceptAsync(AcceptEventArgs))
                {
                    Accept_Completed(socket, AcceptEventArgs);
                }
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, e, 0, null);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// 重新启动网络监听
        /// </summary>
        /// <param name="iPAddress">IP地址</param>
        /// <param name="port">网络端口</param>
        /// <returns>是否启动成功</returns>
        public override bool Restart(IPAddress iPAddress, int port)
        {
            try
            {
                if (socket != null && socket.Connected)
                {
                    socket.Disconnect(false);
                }

                DisconnectAll();

                socket?.Dispose();
                AcceptEventArgs?.Dispose();

                InitializeEventArgs();

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, Type);
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, e, 0, null);
                return false;
            }

            return Start(iPAddress, port);
        }

        /// <summary>
        /// 断开网络连接
        /// </summary>
        /// <param name="isReusable">是否允许重用</param>
        /// <returns>是否操作成功</returns>
        public override bool Disconnect(bool isReusable)
        {
            bool result = true;
            try
            {
                socket.Disconnect(isReusable);
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, e, 0, null);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// 断开客户端网络连接
        /// </summary>
        /// <param name="endpoint">客户端网络地址</param>
        /// <returns>是否成功</returns>
        public bool Disconnect(EndPoint endpoint)
        {
            RemoteClient remoteClient = ClientList.Where(item => item.IsEndPointEquals(endpoint)).FirstOrDefault();
            if (remoteClient == null)
            {
                return false;
            }
            else
            {
                remoteClient.IsDisconnecting = true;
                return true;
            }
        }

        /// <summary>
        /// 断开所有客户端连接
        /// </summary>
        public void DisconnectAll()
        {
            ClientList.ForEach(client => client.IsDisconnecting = true);
        }

        /// <summary>
        /// 向目标客户端发送数据
        /// </summary>
        /// <param name="endpoint">客户端网络地址</param>
        /// <param name="buffer">数据字节</param>
        /// <param name="offset">偏移量</param>
        /// <param name="count">数据长度</param>
        /// <returns>是否成功</returns>
        public override bool Send(EndPoint endpoint, byte[] buffer, int offset = 0, int count = -1)
        {
            if (buffer == null || endpoint == null)
            {
                return false;
            }
            if (count == -1)
            {
                count = buffer.Length;
            }
            RemoteClient remoteClient = ClientList.Where(item => item.IsEndPointEquals(endpoint)).FirstOrDefault();
            if (remoteClient == null)
            {
                return false;
            }
            Socket client = remoteClient.Socket;
            if (!client.Connected)
            {
                ErrorHandler?.Invoke(this, null, SocketError.NotConnected, null);
                return false;
            }
            try
            {
                SocketAsyncEventArgs sendEventArgs = remoteClient.SendEventArgs;
                Buffer.BlockCopy(buffer, offset, sendEventArgs.Buffer, 0, count);
                sendEventArgs.SetBuffer(0, count);
                if (!client.SendAsync(sendEventArgs))
                {
                    Send_Completed(client, sendEventArgs);
                }
                return true;
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, e, 0, null);
                return false;
            }
        }

        /// <summary>
        /// 释放托管资源
        /// </summary>
        public override void Dispose()
        {
            try
            {
                task.ContinueWith((task) =>
                {
                    cancellation.Dispose();
                    task.Dispose();
                });
                cancellation.Cancel();

                DisconnectAll();

                socket?.Dispose();
                AcceptEventArgs?.Dispose();
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, e, 0, null);
            }
        }
    }

    /// <summary>
    /// Socket通信TCP协议服客户端
    /// </summary>
    internal class SocketTCPClient : CommunicationClient
    {
        /// <summary>
        /// 网络连接处理参数
        /// </summary>
        private SocketAsyncEventArgs ConnectEventArgs;
        /// <summary>
        /// 数据发送处理参数
        /// </summary>
        private SocketAsyncEventArgs SendEventArgs;
        /// <summary>
        /// 数据接收处理参数
        /// </summary>
        private SocketAsyncEventArgs ReceiveEventArgs;
        /// <summary>
        /// 连接结束处理
        /// </summary>
        private Action<bool> ConnectHandler;
        /// <summary>
        /// 是否正在连接
        /// </summary>
        private bool isConnecting;

        /// <summary>
        /// 构造函数
        /// </summary>
        public SocketTCPClient()
        {
            InitializeEventArgs();

            Type = ProtocolType.Tcp;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, Type);
        }
        /// <summary>
        /// 初始化处理参数
        /// </summary>
        private void InitializeEventArgs()
        {
            ConnectEventArgs = new SocketAsyncEventArgs();
            ConnectEventArgs.Completed += Connect_Completed;
            SendEventArgs = new SocketAsyncEventArgs();
            SendEventArgs.SetBuffer(new byte[BufferSize], 0, BufferSize);
            SendEventArgs.Completed += Send_Completed;
            ReceiveEventArgs = new SocketAsyncEventArgs();
            ReceiveEventArgs.SetBuffer(new byte[BufferSize], 0, BufferSize);
            ReceiveEventArgs.Completed += Receive_Completed;
        }
        /// <summary>
        /// 设置连接完毕处理方法
        /// </summary>
        /// <param name="action">事件处理方法</param>
        public void SetConnectHandler(Action<bool> action)
        {
            ConnectHandler = action;
        }

        /// <summary>
        /// 网络连接完成事件
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="args">事件参数</param>
        private void Connect_Completed(object sender, SocketAsyncEventArgs args)
        {
            isConnecting = false;
            if (args.SocketError == SocketError.OperationAborted || args.SocketError == SocketError.ConnectionAborted)
            {
                ConnectHandler?.Invoke(false);
                ErrorHandler?.Invoke(this, null, args.SocketError, null);
                return;
            }
            else if (args.SocketError != SocketError.Success && args.SocketError != SocketError.IsConnected)
            {
                ConnectHandler?.Invoke(false);
                ErrorHandler?.Invoke(this, null, args.SocketError, null);
            }
            else
            {
                try
                {
                    if (socket.Connected)
                    {
                        ConnectHandler?.Invoke(true);
                        if (!socket.ReceiveAsync(ReceiveEventArgs))
                        {
                            Receive_Completed(socket, ReceiveEventArgs);
                        }
                    }
                    else
                    {
                        ConnectHandler?.Invoke(false);
                    }
                }
                catch (Exception e)
                {
                    ConnectHandler?.Invoke(false);
                    ErrorHandler?.Invoke(this, e, 0, null);
                }
            }
        }
        /// <summary>
        /// 数据发送完成事件
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="args">事件参数</param>
        private void Send_Completed(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.OperationAborted || args.SocketError == SocketError.ConnectionAborted)
            {
                return;
            }
            else if (args.SocketError != SocketError.Success)
            {
                ErrorHandler?.Invoke(this, null, args.SocketError, null);
            }
            else
            {
                sendCount += 1;
                sendSize += args.BytesTransferred;
            }
        }
        /// <summary>
        /// 数据接收完成事件
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="args">事件参数</param>
        private void Receive_Completed(object sender, SocketAsyncEventArgs args)
        {
            try
            {
                do
                {
                    if (args.SocketError == SocketError.OperationAborted || args.SocketError == SocketError.ConnectionAborted)
                    {
                        return;
                    }
                    else if (args.SocketError != SocketError.Success)
                    {
                        ErrorHandler?.Invoke(this, null, args.SocketError, null);
                    }
                    else
                    {
                        receiveCount += 1;
                        receiveSize += args.BytesTransferred;
                        try
                        {
                            ReceiveHandler?.Invoke(args.RemoteEndPoint, args.Buffer, args.Offset, args.BytesTransferred);
                        }
                        catch (Exception e)
                        {
                            ErrorHandler?.Invoke(this, e, 0, null);
                        }
                    }
                    if (!socket.Connected)
                    {
                        return;
                    }
                } while (!socket.ReceiveAsync(args));
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, e, 0, null);
            }
        }

        /// <summary>
        /// 启动网络连接
        /// <remarks>该方法不会等待网络连接就绪，须主动检查<c>IsConnected</c>状态或调用异步方法<c>StartAsync()</c>，或使用<c>SetConnectHandler()</c>方法设置连接结束事件处理</remarks>
        /// </summary>
        /// <param name="iPAddress">IP地址</param>
        /// <param name="port">网络端口</param>
        /// <returns>是否成功</returns>
        public override bool Start(IPAddress iPAddress, int port)
        {
            try
            {
                EndPoint = new IPEndPoint(iPAddress, port);
                ConnectEventArgs.RemoteEndPoint = EndPoint;

                if (!isConnecting)
                {
                    isConnecting = true;
                    if (!socket.ConnectAsync(ConnectEventArgs))
                    {
                        Connect_Completed(socket, ConnectEventArgs);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, e, 0, null);
                return false;
            }
        }

        /// <summary>
        /// 重新启动网络连接
        /// </summary>
        /// <param name="iPAddress">IP地址</param>
        /// <param name="port">网络端口</param>
        /// <returns>是否成功</returns>
        public override bool Restart(IPAddress iPAddress, int port)
        {
            if (socket != null && socket.Connected)
            {
                socket.Disconnect(false);
            }
            socket?.Dispose();
            ConnectEventArgs?.Dispose();
            SendEventArgs?.Dispose();
            ReceiveEventArgs?.Dispose();

            InitializeEventArgs();

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, Type);
            return Start(iPAddress, port);
        }

        /// <summary>
        /// 断开网络连接
        /// </summary>
        /// <param name="isReusable">是否允许重用</param>
        /// <returns>是否成功</returns>
        public override bool Disconnect(bool isReusable)
        {
            bool result = true;
            try
            {
                socket.Disconnect(isReusable);
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, e, 0, null);
                result = false;
            }
            return result;
        }
        /// <summary>
        /// 向目标服务端发送数据
        /// </summary>
        /// <param name="endpoint">远端地址</param>
        /// <param name="buffer">数据字节</param>
        /// <param name="offset">偏移量</param>
        /// <param name="count">数据长度</param>
        /// <returns>是否发送成功</returns>
        public override bool Send(EndPoint endpoint, byte[] buffer, int offset = 0, int count = -1)
        {
            if (buffer == null)
            {
                return false;
            }
            if (count == -1)
            {
                count = buffer.Length;
            }
            if (!socket.Connected)
            {
                ErrorHandler?.Invoke(this, null, SocketError.NotConnected, null);

                socket.Dispose();
                if (ConnectEventArgs.RemoteEndPoint is IPEndPoint point)
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, Type);
                    Start(point.Address, point.Port);
                }

                return false;
            }
            try
            {
                Buffer.BlockCopy(buffer, offset, SendEventArgs.Buffer, 0, count);
                SendEventArgs.SetBuffer(0, count);
                if (!socket.SendAsync(SendEventArgs))
                {
                    Send_Completed(socket, SendEventArgs);
                }
                return true;
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, e, 0, null);
                return false;
            }
        }
        /// <summary>
        /// 释放托管资源
        /// </summary>
        public override void Dispose()
        {
            try
            {
                if (socket != null && socket.Connected)
                {
                    socket.Disconnect(false);
                }
                socket?.Dispose();
                ConnectEventArgs?.Dispose();
                SendEventArgs?.Dispose();
                ReceiveEventArgs?.Dispose();
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, e, 0, null);
            }
        }
    }

    /// <summary>
    /// Socket通信UDP协议客户端
    /// </summary>
    internal class SocketUDPClient : CommunicationClient
    {
        /// <summary>
        /// 数据发送处理参数
        /// </summary>
        private SocketAsyncEventArgs SendEventArgs;
        /// <summary>
        /// 数据接收处理参数
        /// </summary>
        private SocketAsyncEventArgs ReceiveEventArgs;

        /// <summary>
        /// 构造函数
        /// </summary>
        public SocketUDPClient()
        {
            InitializeEventArgs();

            Type = ProtocolType.Udp;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, Type);
        }
        /// <summary>
        /// 初始化处理参数
        /// </summary>
        private void InitializeEventArgs()
        {
            SendEventArgs = new SocketAsyncEventArgs();
            SendEventArgs.SetBuffer(new byte[BufferSize], 0, BufferSize);
            SendEventArgs.Completed += Send_Completed;
            ReceiveEventArgs = new SocketAsyncEventArgs();
            ReceiveEventArgs.SetBuffer(new byte[BufferSize], 0, BufferSize);
            ReceiveEventArgs.Completed += Receive_Completed;
        }
        /// <summary>
        /// 数据发送结束事件
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="args">事件参数</param>
        private void Send_Completed(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.OperationAborted || args.SocketError == SocketError.ConnectionAborted)
            {
                return;
            }
            else if (args.SocketError != SocketError.Success)
            {
                ErrorHandler?.Invoke(this, null, args.SocketError, null);
            }
            else
            {
                sendCount += 1;
                sendSize += args.BytesTransferred;
            }
        }
        /// <summary>
        /// 数据接收结束事件
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="args">事件参数</param>
        private void Receive_Completed(object sender, SocketAsyncEventArgs args)
        {
            try
            {
                do
                {
                    if (args.SocketError == SocketError.OperationAborted || args.SocketError == SocketError.ConnectionAborted)
                    {
                        return;
                    }
                    else if (args.SocketError != SocketError.Success)
                    {
                        ErrorHandler?.Invoke(this, null, args.SocketError, null);
                    }
                    else
                    {
                        receiveCount += 1;
                        receiveSize += args.BytesTransferred;
                        try
                        {
                            ReceiveHandler?.Invoke(args.RemoteEndPoint, args.Buffer, args.Offset, args.BytesTransferred);
                        }
                        catch (Exception e)
                        {
                            ErrorHandler?.Invoke(this, e, 0, null);
                        }
                    }
                } while (!socket.ReceiveFromAsync(args));
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, e, 0, null);
            }
        }

        /// <summary>
        /// 启动网络连接
        /// </summary>
        /// <param name="iPAddress">IP地址</param>
        /// <param name="port">网络端口</param>
        /// <returns>是否成功</returns>
        public override bool Start(IPAddress iPAddress, int port)
        {
            bool result = true;
            try
            {
                EndPoint = new IPEndPoint(iPAddress, port);
                SendEventArgs.RemoteEndPoint = EndPoint;
                ReceiveEventArgs.RemoteEndPoint = EndPoint;
                socket.Connect(EndPoint);
                if (!socket.ReceiveFromAsync(ReceiveEventArgs))
                {
                    Receive_Completed(socket, ReceiveEventArgs);
                }
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, e, 0, null);
                result = false;
            }
            return result;
        }
        /// <summary>
        /// 重新启动网络连接
        /// </summary>
        /// <param name="iPAddress">IP地址</param>
        /// <param name="port">网络端口</param>
        /// <returns>是否成功</returns>
        public override bool Restart(IPAddress iPAddress, int port)
        {
            socket?.Shutdown(SocketShutdown.Both);
            socket?.Dispose();
            SendEventArgs?.Dispose();
            ReceiveEventArgs?.Dispose();

            InitializeEventArgs();

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, Type);
            return Start(iPAddress, port);
        }
        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="isReusable">是否允许重用</param>
        /// <returns>是否断开成功</returns>
        public override bool Disconnect(bool isReusable)
        {
            bool result = true;
            try
            {
                socket.Disconnect(isReusable);
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, e, 0, null);
                result = false;
            }
            return result;
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="endpoint">远端网络地址</param>
        /// <param name="buffer">数据字节</param>
        /// <param name="offset">偏移量</param>
        /// <param name="count">数据长度</param>
        /// <returns>是否成功</returns>
        public override bool Send(EndPoint endpoint, byte[] buffer, int offset = 0, int count = -1)
        {
            if (buffer == null)
            {
                return false;
            }
            if (count == -1)
            {
                count = buffer.Length;
            }
            try
            {
                SendEventArgs.SetBuffer(0, count);
                Buffer.BlockCopy(buffer, offset, SendEventArgs.Buffer, 0, count);
                if (!socket.SendToAsync(SendEventArgs))
                {
                    Send_Completed(socket, SendEventArgs);
                }
                return true;
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, e, 0, null);
                return false;
            }
        }
        /// <summary>
        /// 释放托管资源
        /// </summary>
        public override void Dispose()
        {
            try
            {
                socket?.Shutdown(SocketShutdown.Both);
                socket?.Dispose();
                SendEventArgs?.Dispose();
                ReceiveEventArgs?.Dispose();
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, e, 0, null);
            }
        }
    }
    /// <summary>
    /// Socket通信UDP协议服务端
    /// </summary>
    internal class SocketUDPServer : CommunicationClient
    {
        /// <summary>
        /// 数据发送处理参数
        /// </summary>
        private SocketAsyncEventArgs SendEventArgs;
        /// <summary>
        /// 数据接收处理参数
        /// </summary>
        private SocketAsyncEventArgs ReceiveEventArgs;
        /// <summary>
        /// 构造函数
        /// </summary>
        public SocketUDPServer()
        {
            InitializeEventArgs();

            Type = ProtocolType.Udp;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, Type);
        }
        /// <summary>
        /// 初始化处理参数
        /// </summary>
        private void InitializeEventArgs()
        {
            SendEventArgs = new SocketAsyncEventArgs();
            SendEventArgs.SetBuffer(new byte[BufferSize], 0, BufferSize);
            SendEventArgs.Completed += Send_Completed;
            ReceiveEventArgs = new SocketAsyncEventArgs();
            ReceiveEventArgs.SetBuffer(new byte[BufferSize], 0, BufferSize);
            ReceiveEventArgs.Completed += Receive_Completed;
        }
        /// <summary>
        /// 数据发送结束事件
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="args">事件参数</param>
        private void Send_Completed(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.OperationAborted || args.SocketError == SocketError.ConnectionAborted)
            {
                return;
            }
            else if (args.SocketError != SocketError.Success)
            {
                ErrorHandler?.Invoke(this, null, args.SocketError, null);
            }
            else
            {
                sendCount += 1;
                sendSize += args.BytesTransferred;
            }
        }
        /// <summary>
        /// 数据接收结束事件
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="args">事件参数</param>
        private void Receive_Completed(object sender, SocketAsyncEventArgs args)
        {
            try
            {
                do
                {
                    if (args.SocketError == SocketError.OperationAborted || args.SocketError == SocketError.ConnectionAborted)
                    {
                        return;
                    }
                    else if (args.SocketError != SocketError.Success)
                    {
                        ErrorHandler?.Invoke(this, null, args.SocketError, null);
                    }
                    else
                    {
                        receiveCount += 1;
                        receiveSize += args.BytesTransferred;
                        try
                        {
                            ReceiveHandler?.Invoke(args.RemoteEndPoint, args.Buffer, args.Offset, args.BytesTransferred);
                        }
                        catch (Exception e)
                        {
                            ErrorHandler?.Invoke(this, e, 0, null);
                        }
                    }
                } while (!socket.ReceiveFromAsync(args));
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, e, 0, null);
            }
        }
        /// <summary>
        /// 组播网络绑定地址
        /// </summary>
        public IPAddress BindAddress { get; set; }
        /// <summary>
        /// 启动网络服务监听
        /// </summary>
        /// <param name="iPAddress">IP地址</param>
        /// <param name="port">网络端口</param>
        /// <returns>是否启动成功</returns>
        public override bool Start(IPAddress iPAddress, int port)
        {
            bool result = true;
            try
            {
                EndPoint = new IPEndPoint(iPAddress, port);

                if (iPAddress.Equals(IPAddress.Broadcast))
                {
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                }
                if (IsMulticast(iPAddress))
                {
                    EndPoint = new IPEndPoint(IPAddress.Any, port);
                    socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(iPAddress, BindAddress ?? IPAddress.Any));
                    socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, true);
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                }

                ReceiveEventArgs.RemoteEndPoint = EndPoint;
                socket.Bind(EndPoint);
                if (!socket.ReceiveFromAsync(ReceiveEventArgs))
                {
                    Receive_Completed(socket, ReceiveEventArgs);
                }
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, e, 0, null);
                result = false;
            }
            return result;
        }
        /// <summary>
        /// 重新启动网络服务监听
        /// </summary>
        /// <param name="iPAddress">IP地址</param>
        /// <param name="port">网络端口</param>
        /// <returns>是否重启成功</returns>
        public override bool Restart(IPAddress iPAddress, int port)
        {
            socket?.Shutdown(SocketShutdown.Both);
            socket?.Dispose();
            SendEventArgs?.Dispose();
            ReceiveEventArgs?.Dispose();

            InitializeEventArgs();

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, Type);
            return Start(iPAddress, port);
        }
        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="isReusable">是否允许重用</param>
        /// <returns>是否断开成功</returns>
        public override bool Disconnect(bool isReusable)
        {
            bool result = true;
            try
            {
                socket.Disconnect(isReusable);
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, e, 0, null);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="endpoint">远端网络地址</param>
        /// <param name="buffer">数据字节</param>
        /// <param name="offset">偏移量</param>
        /// <param name="count">数据长度</param>
        /// <returns>是否成功</returns>
        public override bool Send(EndPoint endpoint, byte[] buffer, int offset = 0, int count = -1)
        {
            if (buffer == null)
            {
                return false;
            }
            if (count == -1)
            {
                count = buffer.Length;
            }
            try
            {
                Buffer.BlockCopy(buffer, offset, SendEventArgs.Buffer, 0, count);
                SendEventArgs.SetBuffer(0, count);
                SendEventArgs.RemoteEndPoint = endpoint;
                if (!socket.SendToAsync(SendEventArgs))
                {
                    Send_Completed(socket, SendEventArgs);
                }
                return true;
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, e, 0, null);
                return false;
            }
        }
        /// <summary>
        /// 释放托管资源
        /// </summary>
        public override void Dispose()
        {
            try
            {
                socket?.Shutdown(SocketShutdown.Both);
                socket?.Dispose();
                SendEventArgs?.Dispose();
                ReceiveEventArgs?.Dispose();
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, e, 0, null);
            }
        }
    }
}