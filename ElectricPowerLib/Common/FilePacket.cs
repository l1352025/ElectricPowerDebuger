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
        private byte[] _pktMissBitFlags;

        public string FileName { get; private set; }
        public int FileSize { get; private set; }
        public int FileKbSize { get; private set; }
        public int FileCrc16 { get; private set; }
        public int PacketSize { get; private set; }
        public int PacketCount { get; private set; }
        public int LastPktSize { get; private set; }
        public int PktMissingCnt { get; private set; }
        public List<int> PktMissingList { get; private set; }
        public int PktCurrIndex { get; set; }
        public int PktSendCnt { get; set; }
        public string Version { get; set; }
        public int VersionCrc16 { get; set; }

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
                throw new Exception("文件长度 <= 起始位置" + packetStartIndex );
            }

            FileName = filePath;
            FileSize = (int)(fs.Length - packetStartIndex);
            FileKbSize = (FileSize + 1023) / 1024;
            PacketSize = packetSize;
            PacketCount = (FileSize + packetSize - 1)/packetSize;
            LastPktSize = ((FileSize % packetSize != 0) ? (FileSize % packetSize) : packetSize);
            PktMissingCnt = PacketCount;

            _pktMissBitFlags = new byte[(PacketCount + 7) / 8];
            ClearPacketMissingBitFlags();

            _dataBuffer = new byte[FileSize];
            fs.Seek(packetStartIndex, SeekOrigin.Begin);
            fs.Read(_dataBuffer, 0, _dataBuffer.Length);
            fs.Close();

            FileCrc16 = Util.GetCRC16(_dataBuffer, 0, _dataBuffer.Length);
        }

        public int CopyPacketToBuffer(byte[] dstBuffer, int dstIndex, int packetIndex)
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

        public void ClearPacketMissingBitFlags()
        {
            for(int i = 0; i < _pktMissBitFlags.Length; i++)
            {
                _pktMissBitFlags[i] = 0xFF;
            }
        }

        public void AddPacketMissingBitFlags(byte[] bitFlags)
        {
            if (_pktMissBitFlags.Length != bitFlags.Length) throw new Exception("缺包数位标记数组长度错误！");

            for (int i = 0; i < _pktMissBitFlags.Length; i++)
            {
                _pktMissBitFlags[i] &= bitFlags[i];
            }
        }

        public List<int> GetPacketMissingList()
        {
            List<int> list = new List<int>();
            byte aByte;
            int pktIdx = 0;

            if(PktMissingCnt == PacketCount)
            {
                for(int i = 0; i < PacketCount; i++)
                {
                    list.Add(i);
                }
                return list;
            }

            for (int i = 0; i < _pktMissBitFlags.Length; i++)
            {
                aByte = _pktMissBitFlags[i++];

                if (aByte == 0xFF)
                {
                    pktIdx += 8;
                }
                else
                {
                    for (int j = 0; j < 8 && pktIdx < PacketCount; j++)
                    {
                        if ((aByte & 0x01) == 0)
                        {
                            list.Add(pktIdx);
                        }
                        aByte >>= 1;
                        pktIdx++;
                    }
                }
            }

            PktMissingCnt = list.Count;

            return list;
        }
        public int GetPacketMissingList(byte[] bitFlags)
        {
            byte aByte;
            int pktIdx = 0;
            int cnt = 0;

            for (int i = 0; i < bitFlags.Length; i++)
            {
                aByte = bitFlags[i++];

                if (aByte == 0xFF)
                {
                    pktIdx += 8;
                }
                else
                {
                    for (int j = 0; j < 8 && pktIdx < PacketCount; j++)
                    {
                        if ((aByte & 0x01) == 0)
                        {
                            cnt++;
                        }
                        aByte >>= 1;
                        pktIdx++;
                    }
                }
            }

            return cnt;
        }

        // 方法二
        public void GetCurrPacketMissingCnt(byte[] bitFlags, out int sefMissCnt, out int currMissCnt, byte missFlag = 0)
        {
            List<int> list = new List<int>();
            byte aByte;
            int pktIdx = 0;

            for (int i = 0; i < bitFlags.Length; i++)
            {
                aByte = bitFlags[i++];

                if (missFlag == 0 && aByte == 0xFF)
                {
                    pktIdx += 8;
                }
                else if (missFlag == 1 && aByte == 0x00)
                {
                    pktIdx += 8;
                }
                else
                {
                    for (int j = 0; j < 8 && pktIdx < PacketCount; j++)
                    {
                        if ((aByte & 0x01) == missFlag)
                        {
                            list.Add(pktIdx);
                        }
                        aByte >>= 1;
                        pktIdx++;
                    }
                }
            }

            PktMissingList = (List<int>)PktMissingList.Union(list);

            sefMissCnt = list.Count;
            currMissCnt = PktMissingList.Count;
        }

    }
}
