using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ElectricPowerLib.Common
{
    public class FilePacket
    {
        private byte[] _dataBuffer;

        public string FileName { get; private set; }
        public int FileSize { get; private set; }
        public int FileKbSize { get; private set; }
        public int PacketSize { get; private set; }
        public int PacketCount { get; private set; }
        public int LastPktSize { get; private set; }
        public int CurrPktIndex { get; private set; }

        public enum FindMode
        {
            Begin       = 0,
            End         = 1,
        }

        public FilePacket(string filePath, int packetSize, int packetStartIndex = 0)
        {
            if (!File.Exists(filePath)) throw new Exception("文件不存在：" + filePath);

            FileStream fs = File.OpenRead(filePath);

            if (fs.Length < packetStartIndex)
            {
                fs.Close();
                throw new Exception($"文件长度 <= 起始位置 {packetStartIndex}");
            }

            FileName = filePath;
            FileSize = (int)(fs.Length - packetStartIndex);
            FileKbSize = (FileSize + 1023) / 1024;
            PacketSize = packetSize;
            PacketCount = (FileSize + packetSize - 1)/packetSize;
            LastPktSize = ((FileSize % packetSize != 0) ? (FileSize % packetSize) : packetSize);

            _dataBuffer = new byte[FileSize];
            fs.Seek(packetStartIndex, SeekOrigin.Begin);
            fs.Read(_dataBuffer, 0, _dataBuffer.Length);
            fs.Close();
        }

        public int PacketToBuffer(byte[] dstBuffer, int dstIndex, int packetIndex)
        {
            int size = 0;

            if (dstIndex + PacketSize <= dstBuffer.Length)
            {
                size = (packetIndex == PacketCount - 1 ? LastPktSize : PacketSize);
                Array.Copy(_dataBuffer, PacketSize * packetIndex, dstBuffer, dstIndex, size);
                return PacketSize;
            }

            return size;
        }

        public int CopyToBuffer(byte[] dstBuffer, int dstIndex, int srcIndex, int size)
        {
            if (dstIndex + size <= dstBuffer.Length)
            {
                if(size + srcIndex > FileSize)
                {
                    size = FileSize - srcIndex;
                }
                Array.Copy(_dataBuffer, srcIndex, dstBuffer, dstIndex, size);
                return size;
            }
            else
            {
                return 0;
            }
        }

        public string GetString(string strPrefix, string strSuffix, FindMode findMode, int offset)
        {
            string strFind = "";
            int indexStart = -1, indexEnd = -1;

            if (findMode == FindMode.Begin)
            {
                indexStart = Util.IndexOf(_dataBuffer, strPrefix, 0, offset);

                if (indexStart >= 0)
                    indexEnd = Util.IndexOf(_dataBuffer, strSuffix, indexStart, (offset - indexStart));

                if (indexEnd >= 0)
                    strFind = Encoding.UTF8.GetString(_dataBuffer, indexStart, (indexEnd - indexStart));
            }
            else if (findMode == FindMode.End)
            {
                indexStart = Util.IndexOf(_dataBuffer, strPrefix, (FileSize - offset), offset);

                if (indexStart >= 0)
                    indexEnd = Util.IndexOf(_dataBuffer, strSuffix, indexStart, (FileSize - indexStart));

                if (indexEnd >= 0)
                    strFind = Encoding.UTF8.GetString(_dataBuffer, indexStart, (indexEnd - indexStart));
            }

            return strFind;
        }
    }
}
