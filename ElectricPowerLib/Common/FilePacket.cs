using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ElectricPowerLib.Common
{
    /// <summary>
    /// 文件分包处理类
    /// </summary>
    public class FilePacket
    {
        private readonly byte[] _headerBuffer;
        private byte[] _dataBuffer;
        private byte[] _pktMissBitFlags;
        private List<int> _pktMissList;
        
#pragma warning disable

        public string FileName { get; private set; }
        public int FileSize { get; private set; }
        public int FileKbSize { get; private set; }
        public int FileCrc16 { get; private set; }
        public int FileSum16 { get; private set; }
        public int PacketSize { get; private set; }
        public int PacketCount { get; private set; }
        public int LastPktSize { get; private set; }
        public int PktMissCnt { get; private set; }
        public int PktCurrIndex { get; set; }
        public int PktSendCnt { get; set; }
        public string Version { get; set; }
        public int VersionCrc16 { get; set; }
        public byte[] FileHeader { get { return _headerBuffer; } }
        public byte[] FileBuffer { get { return _dataBuffer; } }

#pragma warning restore

        /// <summary>
        /// 查找模式：begin - 从头部开始，end - 从尾部开始
        /// </summary>
        public enum FindMode
        {
            /// <summary>
            /// 从头部开始查找
            /// </summary>
            Begin       = 0,

            /// <summary>
            /// 从尾部开始查找
            /// </summary>
            End         = 1,
        }

        /// <summary>
        /// FilePacket 类实例化
        /// </summary>
        /// <param name="filePath">文件名</param>
        /// <param name="packetSize">文件分包的固定大小</param>
        /// <param name="packetStartIndex">文件分包的起始位置</param>
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
            PktMissCnt = PacketCount;
            _pktMissList = new List<int>();
            _pktMissBitFlags = new byte[(PacketCount + 7) / 8];
            ClearPacketMissingBitFlags();

            _headerBuffer = new byte[packetStartIndex];
            fs.Seek(0, SeekOrigin.Begin);
            fs.Read(_headerBuffer, 0, _headerBuffer.Length);

            _dataBuffer = new byte[FileSize];
            fs.Seek(packetStartIndex, SeekOrigin.Begin);
            fs.Read(_dataBuffer, 0, _dataBuffer.Length);
            fs.Close();

            FileCrc16 = Util.GetCRC16(_dataBuffer, 0, _dataBuffer.Length);
            FileSum16 = Util.GetChecksum16(_dataBuffer, 0, _dataBuffer.Length);
        }

        /// <summary>
        /// 拷贝当前文件缓存的一包数据
        /// </summary>
        /// <param name="dstBuffer">目的byte缓存</param>
        /// <param name="dstIndex">目的byte缓存索引</param>
        /// <param name="packetIndex">源文件缓存的包序号</param>
        /// <returns>实际拷贝的长度，除了最后一包，其他都是固定长度</returns>
        public int CopyPacketToBuffer(byte[] dstBuffer, int dstIndex, int packetIndex)
        {
            int size = 0;

            size = (packetIndex == PacketCount - 1 ? LastPktSize : PacketSize);

            if (dstIndex + size <= dstBuffer.Length)
            {
                Array.Copy(_dataBuffer, PacketSize * packetIndex, dstBuffer, dstIndex, size);
            }

            return size;
        }

        /// <summary>
        /// 拷贝当前文件缓存的指定长度数据
        /// </summary>
        /// <param name="dstBuffer">目的byte缓存</param>
        /// <param name="dstIndex">目的byte缓存索引</param>
        /// <param name="srcIndex">源文件缓存索引</param>
        /// <param name="size">拷贝的长度</param>
        /// <returns>实际拷贝的长度</returns>
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

        /// <summary>
        /// 在当前文件中查找字符串
        /// </summary>
        /// <param name="strPrefix">字符串前缀，如"SRWF-"</param>
        /// <param name="strSuffix">字符串后缀，如以" " 或 "" 结束</param>
        /// <param name="findMode">查找模式 FindMode, 从头部或尾部查找</param>
        /// <param name="offset">要检索的长度，如 1024 byte</param>
        /// <returns>若找到返回该字符串，否则返回空字符串""</returns>
        public string GetStringFromDataBuffer(string strPrefix, string strSuffix, FindMode findMode, int offset)
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

        /// <summary>
        /// 清空总累计缺包缓存
        /// </summary>
        public void ClearPacketMissingBitFlags()
        {
            for(int i = 0; i < _pktMissBitFlags.Length; i++)
            {
                _pktMissBitFlags[i] = 0xFF;
            }
            PktMissCnt = PacketCount;
        }

        /// <summary>
        /// 添加当前缺包位标记 到 总累计缺包缓存
        /// </summary>
        /// <param name="bitFlags">当前缺包位标记缓存</param>
        /// <param name="index">位标记起始位置</param>
        /// <param name="byteCnt">位标记字节数</param>
        public void AddPacketMissingBitFlags(byte[] bitFlags, int index, int byteCnt)
        {
            if (_pktMissBitFlags.Length != byteCnt
                || bitFlags.Length < index + byteCnt)
            {
                throw new Exception("缺包数位标记数组长度错误！");
            }

            for (int i = 0; i < _pktMissBitFlags.Length; i++)
            {
                _pktMissBitFlags[i] &= bitFlags[i + index];
            }

            PktMissCnt = 0xFFFF;   // cnt unknown
        }

        /// <summary>
        /// 获取当前缺包序号列表
        /// </summary>
        /// <returns></returns>
        public List<int> GetPacketMissingList()
        {
            List<int> list = new List<int>();
            byte aByte;
            int pktIdx = 0;

            if(PktMissCnt == PacketCount)
            {
                for(int i = 0; i < PacketCount; i++)
                {
                    list.Add(i);
                }
                return list;
            }

            for (int i = 0; i < _pktMissBitFlags.Length; i++)
            {
                aByte = _pktMissBitFlags[i];

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

            PktMissCnt = list.Count;

            return list;
        }
       
        // 方法二
        /// <summary>
        /// 获取当前包缺包数
        /// </summary>
        /// <param name="bitFlags">位标记所在缓存</param>
        /// <param name="index">位标记在缓存中的起始索引</param>
        /// <param name="currMissCnt">当前缓存中缺包数</param>
        /// <param name="totalMissList">总累计缺包序号列表</param>
        /// <param name="missFlag">缺包标记为 bit '0 或 ‘1’</param>
        public void GetCurrPacketMissingCnt(byte[] bitFlags, int index, out int currMissCnt, out List<int> totalMissList, byte missFlag = 0)
        {
            List<int> list = new List<int>();
            byte aByte;
            int pktIdx = 0;

            for (int i = index; i < bitFlags.Length ; i++)
            {
                aByte = bitFlags[i];

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

            _pktMissList = (List<int>)_pktMissList.Union(list);

            currMissCnt = list.Count;
            totalMissList = _pktMissList;

            PktMissCnt = _pktMissList.Count;
        }

    }
}
