using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ElectricPowerDebuger.Protocol
{
    class ProtoLocal_South
    {
        public const byte FrameHeader = 0x68;           // 帧头
        public const byte FrameTail = 0x16;             // 帧尾
        public const byte FrameFixedLen = 12;           // 起始字符,长度(2),控制域,校验和,结束字符,AFN,SEQ,数据标识编码(4)
        public const byte FrameAddrLen = 12;            // 地址域的长度
        public const byte LongAddrSize = 6;             // 地址的长度

        // 通信报文格式
        public struct PacketFormat
        {
            public byte Header;                         // 帧头
            public int Length;                          // 长度
            public byte CtrlWord;                       // 控制字
            public byte[] SrcAddr;                      // 源地址
            public byte[] DstAddr;                      // 目标地址
            public Afn Afn;                            // 功能码
            public byte SerialNo;                       // 序列号
            public byte[] DataId;                       // 数据标识编码
            public byte[] DataBuf;                      // 数据域
            public byte Crc8;                           // Crc8校验
            public byte Tail;                           // 帧尾
        };

        // 应用功能码定义
        public enum Afn
        {
            Afn0_Ack = 0x00,                            // 应答
            Afn1_Initial,                               // 初始化模块
            Afn2_TaskManage,                            // 管理任务
            Afn3_ReadParams,                            // 读参数
            Afn4_WriteParams,                           // 写参数
            Afn5_ReportData,                            // 上报数据
            Afn6_RequestInfo,                           // 请求信息
            Afn7_FileTransfer,                          // 文件传输
            AfnF0_Internal_Debug = 0xF0,                // 内部调试
        };
       

        public static byte CalCRC8(byte[] dataBuf, int startIndex, int length)
        {
            byte crc = 0;

            for (int i = 0; i < length; i++)
            {
                crc += dataBuf[startIndex + i];
            }

            return crc;
        }

        public static PacketFormat ExplainRxPacket(byte[] rxBuf)
        {
            PacketFormat rxData = new PacketFormat();
        
            try
            {
                int index = 0;
                rxData.Header = rxBuf[index++];
                rxData.Length = rxBuf[index++] + rxBuf[index++] * 256;
                rxData.CtrlWord = rxBuf[index++];
                if ((rxData.CtrlWord & 0x20) == 0x20)
                {
                    rxData.SrcAddr = new byte[LongAddrSize];
                    Array.Copy(rxBuf, index, rxData.SrcAddr, 0, LongAddrSize);
                    index += LongAddrSize;
                    rxData.DstAddr = new byte[LongAddrSize];
                    Array.Copy(rxBuf, index, rxData.DstAddr, 0, LongAddrSize);
                    index += LongAddrSize;
                }
                else
                {
                    rxData.SrcAddr = null;
                    rxData.DstAddr = null;
                }
                rxData.Afn = (Afn)(rxBuf[index++]);
                rxData.SerialNo = rxBuf[index++];
                rxData.DataId = new byte[4];
                Array.Copy(rxBuf, index, rxData.DataId, 0, rxData.DataId.Length);
                index += rxData.DataId.Length;
                rxData.DataBuf = new byte[rxData.Length - index - 2];
                Array.Copy(rxBuf, index, rxData.DataBuf, 0, rxData.DataBuf.Length);
                index += rxData.DataBuf.Length;
                rxData.Crc8 = rxBuf[index++];
                rxData.Tail = rxBuf[index++];
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据解析错误" + ex.Message);
            }
            return rxData;
        }
    }
}
