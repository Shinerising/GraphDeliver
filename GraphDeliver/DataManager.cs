using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace GraphDeliver
{
    internal class DataManager
    {
        public const int DeviceStatusSize = 105;
        public const int BoardStatusSize = 16;

        private readonly List<byte[]> _deviceStatusList = new List<byte[]>();
        private readonly List<byte[]> _boardStatusList = new List<byte[]>(2);
        private readonly List<string> _messageList = new List<string>();

        public DateTime UpdateTime { get; set; }

        public void ClearData()
        {
            lock (_deviceStatusList)
            {
                _deviceStatusList.Clear();
            }

            lock (_boardStatusList)
            {
                _boardStatusList.Clear();
            }

            lock (_messageList)
            { 
                _messageList.Clear();
            }

            UpdateTime = DateTime.Now;
        }

        public void ApplyDeviceStatusData(int deviceID, byte[] data)
        {
            lock (_deviceStatusList)
            {
                while (deviceID >= _deviceStatusList.Count)
                {
                    _deviceStatusList.Add(new byte[DeviceStatusSize]);
                }

                if (data.Length < DeviceStatusSize)
                {
                    return;
                }

                Buffer.BlockCopy(data, 0, _deviceStatusList[deviceID], 0, DeviceStatusSize);
            }

            UpdateTime = DateTime.Now;
        }

        public byte[] GetAllDeviceData()
        {
            lock (_deviceStatusList)
            {
                byte[] data = new byte[DeviceStatusSize * _deviceStatusList.Count];
                for (int i = 0; i < _deviceStatusList.Count; i++)
                {
                    Buffer.BlockCopy(_deviceStatusList[i], 0, data, i * DeviceStatusSize, DeviceStatusSize);
                }

                return data;
            }
        }

        public void PushMessage(string message)
        {
            lock (_messageList)
            {
                if (_messageList.Count > 100)
                {
                    return;
                }
                _messageList.Add(message);
            }

            UpdateTime = DateTime.Now;
        }

        public byte[] PopAllMessageData()
        {
            if (_messageList.Count == 0)
            {
                return new byte[0];
            }

            StringBuilder stringBuilder = new StringBuilder();
            foreach (string message in _messageList)
            {
                stringBuilder.AppendLine(message);
            }

            _messageList.Clear();

            return Encoding.ASCII.GetBytes(stringBuilder.ToString());
        }

        public void ApplyBoardStatusData(int boardID, byte[] data)
        {
            lock (_boardStatusList)
            {
                int index = boardID - 1;

                if (data.Length < BoardStatusSize)
                {
                    return;
                }

                Buffer.BlockCopy(data, 0, _boardStatusList[index], 0, BoardStatusSize);
            }

            UpdateTime = DateTime.Now;
        }

        public byte[] GetAllBoardData()
        {
            lock (_boardStatusList)
            {
                byte[] data = new byte[BoardStatusSize * _boardStatusList.Count];
                for (int i = 0; i < _boardStatusList.Count; i++)
                {
                    Buffer.BlockCopy(_boardStatusList[i], 0, data, i * BoardStatusSize, BoardStatusSize);
                }

                return data;
            }
        }

        private byte[] CompressByteArray(byte[] data)
        {
            using (var inStream = new MemoryStream(data))
            {
                using (var outStream = new MemoryStream())
                {
                    using (var compressStream = new DeflateStream(outStream, CompressionMode.Compress))
                    {
                        inStream.CopyTo(compressStream);
                    }

                    return outStream.ToArray();
                }
            }
        }

        public byte[] PackAllData()
        {
            byte[] deviceData = GetAllDeviceData();
            byte[] boardData = GetAllBoardData();
            byte[] messageData = PopAllMessageData();

            byte[] data = new byte[deviceData.Length + boardData.Length + messageData.Length];

            Buffer.BlockCopy(deviceData, 0, data, 0, deviceData.Length);
            Buffer.BlockCopy(boardData, 0, data, deviceData.Length, boardData.Length);
            Buffer.BlockCopy(messageData, 0, data, deviceData.Length + boardData.Length, messageData.Length);

            byte[] compressedData = CompressByteArray(data);

            byte[] packedData = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(BitConverter.GetBytes(compressedData.Length), 0, packedData, 0, 4);
            Buffer.BlockCopy(compressedData, 0, packedData, 4, compressedData.Length);

            return packedData;
        }
    }
}
