using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace GraphDeliver
{
    internal class DataManager
    {
        public const int DeviceStatusSize = 105;
        public const int BoardStatusSize = 10;

        private byte _hostOnline = 0x74;
        private readonly List<byte[]> _deviceStatusList = new List<byte[]>();
        private readonly List<byte[]> _boardStatusList = new List<byte[]>(2) { new byte[BoardStatusSize], new byte[BoardStatusSize] };
        private readonly List<string> _messageList = new List<string>();
        private readonly List<string> _rollingDataList = new List<string>(2) { "", "" };
        private readonly List<byte[]> _rollingRawDataList = new List<byte[]>(2) { new byte[62], new byte[62] };

        public DateTime UpdateTime { get; set; }

        public void ClearData()
        {
            _deviceStatusList.Clear();
            _boardStatusList.Clear();
            _messageList.Clear();

            UpdateTime = DateTime.Now;
        }

        public void ApplyDeviceStatusData(int deviceID, byte[] data)
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

            UpdateTime = DateTime.Now;
        }

        public byte[] GetAllDeviceData()
        {
            int count = _deviceStatusList.Count;
            byte[] data = new byte[DeviceStatusSize * count];
            for (int i = 0; i < count; i++)
            {
                Buffer.BlockCopy(_deviceStatusList[i], 0, data, i * DeviceStatusSize, DeviceStatusSize);
            }

            return data;
        }

        public void PushMessage(string message)
        {
            if (_messageList.Count > 100)
            {
                return;
            }
            _messageList.Add(message);

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

            return Encoding.UTF8.GetBytes(stringBuilder.ToString());
        }

        public void ApplyBoardStatusData(int hostID, byte[] data)
        {
            int offset = 0;
            int online, backup;
            if (hostID == 0x74)
            {
                online = 0;
                backup = 1;
                _hostOnline = 0x74;
            }
            else if (hostID == 0x75)
            {
                online = 1;
                backup = 0;
                _hostOnline = 0x75;
            }
            else
            {
                return;
            }

            if (data.Length < 106)
            {
                return;
            }

            Buffer.BlockCopy(data, offset + 50, _boardStatusList[online], 0, 3);
            Buffer.BlockCopy(data, offset + 103, _boardStatusList[backup], 0, 3);
            new BitArray(data.Skip(offset).Take(50).Select(bit => bit == 0x01).ToArray()).CopyTo(_boardStatusList[online], 3);
            new BitArray(data.Skip(offset + 53).Take(50).Select(bit => bit == 0x01).ToArray()).CopyTo(_boardStatusList[backup], 3);

            UpdateTime = DateTime.Now;
        }

        public byte[] GetAllBoardData()
        {
            byte[] data = new byte[BoardStatusSize * _boardStatusList.Count + 1];
            data[0] = _hostOnline;
            for (int i = 0; i < _boardStatusList.Count; i++)
            {
                Buffer.BlockCopy(_boardStatusList[i], 0, data, i * BoardStatusSize + 1, BoardStatusSize);
            }

            return data;
        }

        public void ApplyRollingData(byte[] buffer)
        {
            for (int i = 0; i < 2; i++)
            {
                if (buffer.Length < i * 62 + 62)
                {
                    continue;
                }
                byte[] data = new byte[62];
                Buffer.BlockCopy(buffer, i * 62, data, 0, 62);
                if (data.SequenceEqual(_rollingRawDataList[i]))
                {
                    continue;
                }
                int offset = i * 62;
                string state = buffer[offset] == 0xc0 ? "正在溜放" : buffer[offset] == 0xc8 ? "等待溜放" : "溜放结束";
                byte cutCount = buffer[offset + 3];
                string trainName = Encoding.Default.GetString(buffer, offset + 26, 13).Trim();
                string message = $"车次:{trainName} ${state}，勾序:${cutCount}";
                _rollingDataList[i] = message;
                _rollingRawDataList[i] = data;
            }
        }

        public byte[] GetAllRollingData()
        {
            return Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, _rollingDataList));
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
            byte[] rollingData = GetAllRollingData();

            byte[] data = new byte[deviceData.Length + boardData.Length + messageData.Length + rollingData.Length + 8];

            Buffer.BlockCopy(BitConverter.GetBytes((ushort)deviceData.Length), 0, data, 0, 2);
            Buffer.BlockCopy(deviceData, 0, data, 2, deviceData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)boardData.Length), 0, data, deviceData.Length + 2, 2);
            Buffer.BlockCopy(boardData, 0, data, deviceData.Length + 4, boardData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)messageData.Length), 0, data, deviceData.Length + boardData.Length + 4, 2);
            Buffer.BlockCopy(messageData, 0, data, deviceData.Length + boardData.Length + 6, messageData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)rollingData.Length), 0, data, deviceData.Length + boardData.Length + messageData.Length + 6, 2);
            Buffer.BlockCopy(rollingData, 0, data, deviceData.Length + boardData.Length + messageData.Length + 8, rollingData.Length);

            byte[] compressedData = CompressByteArray(data);

            ushort length = (ushort)(compressedData.Length + 2);
            uint timestamp = (uint)(UpdateTime - new DateTime(1970, 1, 1)).TotalSeconds;
            byte[] packedData = new byte[length + 7];
            BitConverter.GetBytes(timestamp).CopyTo(packedData, 0);
            packedData[4] = 0x04;
            Buffer.BlockCopy(BitConverter.GetBytes(length), 0, packedData, 5, 2);
            packedData[7] = 0x78;
            packedData[8] = 0x9C;
            Buffer.BlockCopy(compressedData, 0, packedData, 9, compressedData.Length);

            return packedData;
        }
    }
}
