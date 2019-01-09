#if ProtoVer_North          // 北网版本
#undef ProtoVer_North
#elif ProtoVer_NiBoEr       // 尼泊尔版本
#undef ProtoVer_NiBoEr
#elif ProtoVer_BaXi         // 巴西版本
#undef ProtoVer_BaXi
#else
#define ProtoVer_BaXi      // 当前版本
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using ElectricPowerDebuger.Common;

namespace ElectricPowerDebuger.Protocol
{
#if     ProtoVer_North
    class ProtoWireless_North
#elif   ProtoVer_NiBoEr
    class ProtoWireless_NiBoEr
#elif   ProtoVer_BaXi
    class ProtoWireless_BaXi
#endif
    {
        public const ushort FrameHeader = 0xAA55;       // 帧头 55AA
        public const byte FrameFixedLen = 8;            // 帧头，Rssi,长度,信道号,phy帧头校验,Mac帧(>18),phy载荷校验
        public const byte LongAddrSize = 6;             // 地址的长度

        public delegate TreeNode ExplainCallback(byte[] buf);
        public struct CmdExplain
        {
            public byte CmdId;
            public String CmdName;
            public Color CmdColor;
            public ExplainCallback CmdExplainFunc;
            public int PayloadMinLen;

            public CmdExplain(byte id, String name, Color color, ExplainCallback callback, int minPayload = 0)
            {
                CmdId = id;
                CmdName = name;
                CmdColor = color;
                CmdExplainFunc = callback;
                PayloadMinLen = minPayload;
            }
        }

        #region 协议帧格式
        // 通信报文格式
        public struct FrameFormat
        {
            public UInt16 Header;             // 帧头 固定值 AA55
            public byte Rssi;                 // 接收场强
#if     ProtoVer_North
            public byte Length;               // 长度
            public byte Chanel;                 // 信道号 
            public byte Version;                // 标准识别号
            public byte PhrCrc;                 // 帧头校验
#elif   ProtoVer_NiBoEr
            public UInt16 Length;               // 长度
            public byte Chanel;                 // 信道号 
            public byte Option;                 // 选项字 bit0 加密选项
#elif   ProtoVer_BaXi
            public UInt16 Length;               // 长度
            public byte Chanel;                 // 信道号 
            public byte PhrCrc;                 // 帧头校验
#endif
            public MacFrame Mac;                // Mac帧
            public NwkFrame Nwk;                    // Nwk帧
            public ApsFrame Aps;                        // Aps帧
            public UInt16 Crc16;                // Crc16校验
        };


        // Mac层
        public struct MacFrame
        {
            public MacCtrl CtrlWord;            // 控制字
            public byte FrameSn;                // 帧序号
            public UInt16 PanID;                // PanID
            public byte[] DstAddr;              // 目标地址
            public byte[] SrcAddr;              // 源地址
            public MacExtend Extend;            // 扩展域
            public byte[] Payload;              // 载荷
        };

        public struct MacCtrl
        {
            public MacFrameType FrameType;  // 帧类型  bit2-0
            public bool SecFlag;            // 安全使能标识 bit3
            public bool RemainFlag;         // 后续帧标识 bit4
            public bool AckFlag;            // 确认应答标识 bit5
            public bool PanIDFlag;          // PanID标识 bit6
            public bool FrameSNFlag;        // 帧序号标识 bit8
            public bool ExtendFlag;         // 扩展域标识 bit9
            public byte DstAddrMode;        // 地址模式 bit11-10
            public byte FrameVer;           // 帧版本 bit13-12
            public byte SrcAddrMode;        // 地址模式 bit15-14

            public UInt16 All;              // 整个控制域
        };

        public enum MacFrameType
        {
            Beacon = 0x00,
            Data,
            Ack,
            Cmd,
            WaterGasMeterScan,  /* 北网扩展 -- 查找水气表帧 */
            NetworkScan,        /* 北网扩展 -- 全网感知帧  */
            Reserve,            /* 保留 */
            LowPwrMeterCmd,     /* 北网扩展 -- 低功耗表命令帧 */
            Invalid = 0xFF
        };

        public struct MacExtend
        {
            public byte Length;             // 扩展域长度:(长度 = 厂家标识 + 扩展域数据区)
            public ushort OemInfo;          // 厂家标识
            public byte[] Data;             // 扩展域数据区
        };

        // Nwk层
        public struct NwkFrame
        {
            public NwkCtrl CtrlWord;            // 控制字
            public byte[] DstAddr;              // 目标地址
            public byte[] SrcAddr;              // 源地址
            public byte FrameSn;                // 帧序号
            public byte Radius;                 // 网络半径
            public NwkRoute Route;              // 路由域
            public byte[] Payload;              // 载荷
        };

        public struct NwkCtrl
        {
            public NwkFrameType FrameType;  // 帧类型  bit1-0
            public byte DstAddrMode;        // 目的地址模式 bit3-2
            public byte SrcAddrMode;        // 源地址模式 bit5-4
            public bool RouteFlag;          // 路由域标识 bit7

            public byte All;                // 整个控制域
        };

        public enum NwkFrameType
        {
            Data = 0x00,
            Cmd,
            Invalid = 0xFF
        }

        public struct NwkRoute
        {
            public byte RelayCount;         // 中继节点数 bit4-0
            public byte RelayIndex;         // 中继索引 bit9-5
            public UInt16 AddrMode;         // 中继地址模式 bit23-10
            public byte[] RelayList;        // 中继列表
        };

        // Aps层
        public struct ApsFrame
        {
            public ApsCtrl CtrlWord;            // 控制字
            public byte FrameSn;                // 帧序号
            public ApsExtend Extend;            // 扩展域
            public byte[] Payload;              // 载荷
        };

        public struct ApsCtrl
        {
            public ApsFrameType FrameType;  // 帧类型  bit2-0
            public bool ExtendFlag;         // 扩展标识 bit3

            public byte All;                // 整个控制域，byte格式
        };

        public enum ApsFrameType
        {
            Ack = 0x00,
            Cmd,
            DataTransfer,
            Report,
            Invalid = 0xFF
        };

        public struct ApsExtend
        {
            public byte Length;             // 扩展域长度:(长度 = 厂家标识 + 扩展域数据区)
            public ushort OemInfo;          // 厂家标识
            public byte[] Data;             // 扩展域数据区
        };

        #endregion

        #region 协议帧提取
        // 协议帧提取
        public static FrameFormat ExplainRxPacket(byte[] rxBuf)
        {
            FrameFormat rxData = new FrameFormat();

            try
            {
                int index = 0;
                int srcAddrLen = 0;
                int dstAddrLen = 0;

                if (rxBuf.Length < index + FrameFixedLen) throw new Exception("Mac层无效");

                // 如果有 帧头、Rssi
                if (rxBuf[0] == 0x55 && rxBuf[1] == 0xAA)
                {
                    rxData.Header = (ushort)(rxBuf[index] + rxBuf[index + 1] * 256);
                    index += 2;
                    rxData.Rssi = rxBuf[index++];
                }

                // Phy层提取
#if     ProtoVer_North
                rxData.Length = rxBuf[index++];
                rxData.Chanel = rxBuf[index++];
                rxData.Version = rxBuf[index++];
                rxData.PhrCrc = rxBuf[index++];
#elif   ProtoVer_NiBoEr
                rxData.Length = (ushort)(rxBuf[index] + (rxBuf[index + 1] << 8));
                index += 2;
                rxData.Chanel = rxBuf[index++];
                rxData.Option = rxBuf[index++];
#elif   ProtoVer_BaXi
                rxData.Length = (ushort)(rxBuf[index] + (rxBuf[index + 1] << 8));
                index += 2;
                rxData.Chanel = rxBuf[index++];
                rxData.PhrCrc = rxBuf[index++];
#endif

                rxData.Crc16 = (ushort)(rxBuf[rxBuf.Length - 2] + rxBuf[rxBuf.Length - 1] * 256);

                // 无线升级帧提取
                if (rxBuf[index] == 0x53 && rxBuf[index + 1] == 0x52)
                {
                    if (rxBuf.Length < index + 20) throw new Exception("Mac层无效");

                    rxData.Mac.CtrlWord.All = (ushort)(rxBuf[index] + (rxBuf[index + 1] << 8));
                    index += 2;
                    rxData.Mac.FrameSn = rxBuf[index++];
                    rxData.Mac.PanID = (ushort)(rxBuf[index] + (rxBuf[index + 1] << 8));
                    index += 2;
                    rxData.Mac.DstAddr = new byte[6];
                    Array.Copy(rxBuf, index, rxData.Mac.DstAddr, 0, 6);
                    index += 6;
                    rxData.Mac.SrcAddr = new byte[6];
                    Array.Copy(rxBuf, index, rxData.Mac.SrcAddr, 0, 6);
                    index += 6;
                    int PayloadLen = rxBuf.Length - index - 2;
                    if (PayloadLen > 0)
                    {
                        rxData.Mac.Payload = new byte[PayloadLen];
                        Array.Copy(rxBuf, index, rxData.Mac.Payload, 0, PayloadLen);
                    }

                    return rxData;
                }

                // Mac层提取
                if (rxBuf.Length < index + 5) throw new Exception("Mac层无效");

                rxData.Mac.CtrlWord.All = (ushort)(rxBuf[index] + (rxBuf[index + 1] << 8));
                rxData.Mac.CtrlWord.FrameType = (MacFrameType)(rxBuf[index] & 0x07);
                rxData.Mac.CtrlWord.SecFlag = (rxBuf[index] & 0x08) > 0 ? true : false;
                rxData.Mac.CtrlWord.RemainFlag = (rxBuf[index] & 0x10) > 0 ? true : false;
                rxData.Mac.CtrlWord.AckFlag = (rxBuf[index] & 0x20) > 0 ? true : false;
                rxData.Mac.CtrlWord.PanIDFlag = (rxBuf[index] & 0x40) > 0 ? true : false;
                index++;
                rxData.Mac.CtrlWord.FrameSNFlag = (rxBuf[index] & 0x01) > 0 ? true : false;
                rxData.Mac.CtrlWord.ExtendFlag = (rxBuf[index] & 0x02) > 0 ? true : false;
                rxData.Mac.CtrlWord.DstAddrMode = (byte)(rxBuf[index] >> 2 & 0x03);
                rxData.Mac.CtrlWord.FrameVer = (byte)(rxBuf[index] >> 4 & 0x03);
                rxData.Mac.CtrlWord.SrcAddrMode = (byte)(rxBuf[index] >> 6 & 0x03);
                index++;

                rxData.Mac.FrameSn = rxBuf[index++];
                rxData.Mac.PanID = (ushort)(rxBuf[index] + (rxBuf[index + 1] << 8));
                index += 2;

                if ((rxData.Mac.CtrlWord.FrameType > MacFrameType.LowPwrMeterCmd)	//无效帧 
                    || (rxData.Mac.CtrlWord.DstAddrMode != 2 && rxData.Mac.CtrlWord.DstAddrMode != 3))
                {
                    throw new Exception("Mac层无效");
                }

                if (rxData.Mac.CtrlWord.FrameType == MacFrameType.LowPwrMeterCmd)
                {
                    dstAddrLen = 7;
                    srcAddrLen = 7;
                }
                else
                {
                    dstAddrLen = rxData.Mac.CtrlWord.DstAddrMode == 0x02 ? 2 : 6;
                    srcAddrLen = rxData.Mac.CtrlWord.SrcAddrMode == 0x02 ? 2 : 6;
                }

                if (rxBuf.Length < index + dstAddrLen + srcAddrLen) throw new Exception("Mac层无效");

                rxData.Mac.DstAddr = new byte[dstAddrLen];
                Array.Copy(rxBuf, index, rxData.Mac.DstAddr, 0, dstAddrLen);
                index += dstAddrLen;
                rxData.Mac.SrcAddr = new byte[srcAddrLen];
                Array.Copy(rxBuf, index, rxData.Mac.SrcAddr, 0, srcAddrLen);
                index += srcAddrLen;

                if (true == rxData.Mac.CtrlWord.ExtendFlag)
                {
                    if (rxBuf.Length < index + 1) throw new Exception("Mac层无效");

                    rxData.Mac.Extend.Length = rxBuf[index++];

                    if (rxBuf.Length < index + rxData.Mac.Extend.Length + 2) throw new Exception("Mac层无效");

                    if (rxData.Mac.Extend.Length >= 2)
                    {
                        rxData.Mac.Extend.OemInfo = (ushort)((rxBuf[index] << 8) + rxBuf[index + 1]);  // ascii格式
                        rxData.Mac.Extend.Data = new byte[rxData.Mac.Extend.Length - 2];
                        Array.Copy(rxBuf, index, rxData.Mac.Extend.Data, 0, rxData.Mac.Extend.Length - 2);
                    }

                    index += rxData.Mac.Extend.Length;
                }

                if (rxData.Mac.CtrlWord.FrameType == MacFrameType.Ack)          // 0x01 Mac确认帧
                {
                    // do nothing
                }
                else if (rxData.Mac.CtrlWord.FrameType != MacFrameType.Data)     // Mac层 处理载荷  
                {
                    int PayloadLen = rxBuf.Length - index - 2;
                    if (PayloadLen <= 0)
                    {
                        throw new Exception("Mac层无效");
                    }
                    rxData.Mac.Payload = new byte[PayloadLen];
                    Array.Copy(rxBuf, index, rxData.Mac.Payload, 0, PayloadLen);
                }
                else                                                            // 0x01 Mac数据帧
                {
                    //Nwk层提取
                    if (rxBuf.Length < index + 8) throw new Exception("Nwk层无效");

                    rxData.Nwk.CtrlWord.All = rxBuf[index];
                    rxData.Nwk.CtrlWord.FrameType = (NwkFrameType)(rxBuf[index] & 0x03);
                    rxData.Nwk.CtrlWord.DstAddrMode = (byte)(rxBuf[index] >> 2 & 0x03);
                    rxData.Nwk.CtrlWord.SrcAddrMode = (byte)(rxBuf[index] >> 4 & 0x03);
                    rxData.Nwk.CtrlWord.RouteFlag = (rxBuf[index] & 0x80) > 0 ? true : false;
                    index++;

                    dstAddrLen = rxData.Nwk.CtrlWord.DstAddrMode == 0x02 ? 2 : 6;
                    srcAddrLen = rxData.Nwk.CtrlWord.SrcAddrMode == 0x02 ? 2 : 6;

                    if (rxBuf.Length < index + dstAddrLen + srcAddrLen + 3) throw new Exception("Nwk层无效");

                    rxData.Nwk.DstAddr = new byte[dstAddrLen];
                    Array.Copy(rxBuf, index, rxData.Nwk.DstAddr, 0, dstAddrLen);
                    index += dstAddrLen;
                    rxData.Nwk.SrcAddr = new byte[srcAddrLen];
                    Array.Copy(rxBuf, index, rxData.Nwk.SrcAddr, 0, srcAddrLen);
                    index += srcAddrLen;

                    rxData.Nwk.Radius = (byte)(rxBuf[index] & 0x0F);
                    rxData.Nwk.FrameSn = (byte)(rxBuf[index++] >> 4);

                    if (true == rxData.Nwk.CtrlWord.RouteFlag)
                    {
                        int allAddrLen = 0, relayInfo;
                        relayInfo = (rxBuf[index] + (rxBuf[index + 1] << 8) + (rxBuf[index + 2] << 16));
                        rxData.Nwk.Route.RelayCount = (byte)(relayInfo & 0x01F);        // bit4-0        
                        rxData.Nwk.Route.RelayIndex = (byte)(relayInfo >> 5 & 0x1F);    // bit9-5 
                        rxData.Nwk.Route.AddrMode = (UInt16)(relayInfo >> 10);          // bit23-10
                        index += 3;

                        for (int i = 0; i < rxData.Nwk.Route.RelayCount; i++)
                        {
                            allAddrLen += (byte)(rxData.Nwk.Route.AddrMode >> 2 * i & 0x03) == 0x02 ? 2 : 6;
                        }

                        if (rxBuf.Length < index + allAddrLen + 3) throw new Exception("Nwk层无效");

                        rxData.Nwk.Route.RelayList = new byte[allAddrLen];
                        Array.Copy(rxBuf, index, rxData.Nwk.Route.RelayList, 0, allAddrLen);
                        index += allAddrLen;
                    }


                    if (rxData.Nwk.CtrlWord.FrameType > NwkFrameType.Cmd)   //无法识别的Nwk帧类型
                    {
                        throw new Exception("Nwk层无效");
                    }
                    else if (rxData.Nwk.CtrlWord.FrameType == NwkFrameType.Cmd)
                    {
                        int PayloadLen = rxBuf.Length - index - 2;
                        if (PayloadLen <= 0)
                        {
                            throw new Exception("Nwk层无效");
                        }
                        rxData.Nwk.Payload = new byte[PayloadLen];
                        Array.Copy(rxBuf, index, rxData.Nwk.Payload, 0, PayloadLen);

                    }
                    else    // 0x00 Nwk数据帧
                    {
                        // Aps层提取

                        if (rxBuf.Length < index + 5) throw new Exception("Aps层无效");

                        rxData.Aps.CtrlWord.All = rxBuf[index];
                        rxData.Aps.CtrlWord.FrameType = (ApsFrameType)(rxBuf[index] & 0x07);
                        rxData.Aps.CtrlWord.ExtendFlag = (rxBuf[index] & 0x08) > 0 ? true : false;
                        index++;

                        rxData.Aps.FrameSn = rxBuf[index++];

                        if (true == rxData.Aps.CtrlWord.ExtendFlag)
                        {
                            rxData.Aps.Extend.Length = rxBuf[index++];

                            if (rxBuf.Length < index + rxData.Aps.Extend.Length + 3) throw new Exception("Aps层无效");

                            if (rxData.Aps.Extend.Length >= 2)
                            {
                                rxData.Aps.Extend.OemInfo = (ushort)((rxBuf[index] << 8) + rxBuf[index + 1]); //ascii
                                rxData.Aps.Extend.Data = new byte[rxData.Aps.Extend.Length - 2];
                                Array.Copy(rxBuf, index, rxData.Aps.Extend.Data, 0, rxData.Aps.Extend.Length - 2);
                            }
                            index += rxData.Aps.Extend.Length;
                        }

                        if (rxData.Aps.CtrlWord.FrameType > ApsFrameType.Report)    //无法识别的Aps帧类型
                        {
                            throw new Exception("Aps层无效");
                        }
                        else    // 0x00~0x05 依次为Aps层 确认/否认帧、命令帧、数据转发帧、上报帧、广播业务帧、测试帧
                        {
                            int PayloadLen = rxBuf.Length - index - 2;
                            if (PayloadLen <= 0)
                            {
                                throw new Exception("Aps层无效");
                            }
                            rxData.Aps.Payload = new byte[PayloadLen];
                            Array.Copy(rxBuf, index, rxData.Aps.Payload, 0, PayloadLen);
                        }

                    } //Aps

                } //Nwk
            }
            catch (Exception ex)
            {
                switch (ex.Message)
                {
                    case "Mac层无效":
                        rxData.Mac.CtrlWord.FrameType = MacFrameType.Invalid;
                        break;

                    case "Nwk层无效":
                        rxData.Nwk.CtrlWord.FrameType = NwkFrameType.Invalid;
                        break;

                    case "Aps层无效":
                        rxData.Aps.CtrlWord.FrameType = ApsFrameType.Invalid;
                        break;

                    default:
                        rxData.Mac.CtrlWord.FrameType = MacFrameType.Invalid;
                        MessageBox.Show("数据解析异常:" + ex.Message + ex.StackTrace);
                        break;
                }
            }
            return rxData;
        }

        #endregion

        #region 协议帧解析
        // 解析顶层帧类型、颜色
        public static void GetFrameTypeAndColor(FrameFormat frame, out string frameType, out Color frameColor)
        {
            frameType = "";
            frameColor = Color.Black;

            // 无线升级帧
            if (frame.Mac.CtrlWord.All == 0x5253)
            {
                frameType = "无线升级帧：" + MacExplain.GetUpgradeCmdName(frame.Mac.Payload[0]);
                frameColor = MacExplain.GetUpgradeCmdColor(frame.Mac.Payload[0]);
                return;
            }

            // Mac层 -> Nwk层 -> Aps层
            MacFrameType macFrameType = frame.Mac.CtrlWord.FrameType;
            switch (macFrameType)
            {
                case MacFrameType.Beacon:
                    frameType = "Mac层：信标帧";
                    frameColor = Color.Red;
                    break;

                case MacFrameType.Data: // "Mac层：数据帧";
                    NwkFrameType nwkFrameType = frame.Nwk.CtrlWord.FrameType;
                    if (nwkFrameType == NwkFrameType.Cmd)       // "Nwk层：命令帧"
                    {
                        if (frame.Nwk.Payload == null)
                        {
                            frameType = "Nwk层：无法识别";
                            frameColor = Color.Gray;
                        }
                        else
                        {
                            frameType = "Nwk层：";
                            byte cmdId = frame.Nwk.Payload[0];
                            frameType += NwkExplain.GetCmdName(cmdId);
                            frameColor = NwkExplain.GetCmdColor(cmdId);
                        }
                    }
                    else if (nwkFrameType == NwkFrameType.Data) // "Nwk层：数据帧"
                    {
                        ApsFrameType apsFrameType = frame.Aps.CtrlWord.FrameType;
                        frameType = "Aps层：";
                        frameColor = Color.Olive;

                        if (frame.Aps.Payload == null)
                        {
                            frameType = "Aps层：无法识别";
                            frameColor = Color.Gray;
                        }
                        else if (apsFrameType == ApsFrameType.Ack)
                        {
                            frameType += "确认/否认帧-" + (frame.Aps.Payload[0] == 0x00 ? "否认" : "确认");
                        }
                        else if (apsFrameType == ApsFrameType.Cmd)  // APS层：命令帧
                        {
                            frameType += ApsExplain.GetCmdName(frame.Aps.Payload);
                            frameColor = ApsExplain.GetCmdColor(frame.Aps.Payload[0]);
                        }
                        else if (apsFrameType == ApsFrameType.DataTransfer)
                        {
                            frameType += "数据转发帧";
                        }
                        else if (apsFrameType == ApsFrameType.Report)
                        {
                            frameType += "数据上报帧";
                        }
                        else
                        {
                            frameType = "Aps层：其他帧";
                            frameColor = Color.Gray;
                        }
                    }
                    else
                    {
                        frameType = "Nwk层：其他帧";
                        frameColor = Color.Gray;
                    }
                    break;

                case MacFrameType.Ack:
                    frameType = "Mac层：确认帧";
                    frameColor = Color.Blue;
                    break;
                case MacFrameType.Cmd:
                    if (frame.Mac.Payload == null)
                    {
                        frameType = "Mac层：无法识别";
                        frameColor = Color.Gray;
                    }
                    else
                    {
                        frameType = "Mac层：" + MacExplain.GetCmdName(frame.Mac.Payload[0]);
                        frameColor = MacExplain.GetCmdColor(frame.Mac.Payload[0]);
                    }
                    break;
                case MacFrameType.WaterGasMeterScan:
                    frameType = "Mac层：水气表查找信标帧";
                    frameColor = Color.Red;
                    break;
                case MacFrameType.NetworkScan:
                    frameType = "Mac层：全网感知广播帧";
                    frameColor = Color.Red;
                    break;
                case MacFrameType.Reserve:
                    frameType = "Mac层：无法识别的帧类型";
                    frameColor = Color.Gray;
                    break;
                case MacFrameType.LowPwrMeterCmd:
                    if (frame.Mac.Payload == null)
                    {
                        frameType = "Mac层：无法识别";
                        frameColor = Color.Gray;
                    }
                    else
                    {
                        frameType = "Mac层：" + MacExplain.GetLowPowerMeterCmdName(frame.Mac.Payload[0]);
                        frameColor = MacExplain.GetLowPowerMeterCmdColor(frame.Mac.Payload[0]);
                    }
                    break;
                default:
                    frameType = "Mac层：无法识别";
                    frameColor = Color.Gray;
                    break;
            }
        }

        // 解析协议树
        public static TreeNode GetProtoTree(byte[] databuf)
        {
            FrameFormat RxFrame = ExplainRxPacket(databuf);
            TreeNode parentNode = new TreeNode("无线协议帧");

            // parentNode--物理层
            TreeNode PhyNode = new TreeNode("物理层");
            {
                PhyNode.ForeColor = Color.Black;
#if     ProtoVer_North
                PhyNode.Nodes.Add("帧长度 ：" + RxFrame.Length);
                PhyNode.Nodes.Add("信道组-频点 ：" + RxFrame.Chanel / 2 + "-" + RxFrame.Chanel % 2);
                PhyNode.Nodes.Add("协议标准：" + RxFrame.Version);
                PhyNode.Nodes.Add("帧头校验：" + RxFrame.PhrCrc);
#elif   ProtoVer_NiBoEr
                PhyNode.Nodes.Add("帧长度 ：" + RxFrame.Length);
                PhyNode.Nodes.Add("信道组-频点 ：" + RxFrame.Chanel / 2 + "-" + RxFrame.Chanel % 2);
                PhyNode.Nodes.Add("选项字(Rssi)：" + RxFrame.Option);
#elif   ProtoVer_BaXi
                PhyNode.Nodes.Add("帧长度：" + RxFrame.Length);
                PhyNode.Nodes.Add("信道号：" + RxFrame.Chanel);
                PhyNode.Nodes.Add("帧头校验：" + RxFrame.PhrCrc);
#endif
            }
            parentNode.Nodes.Add(PhyNode);

            // 无线升级帧
            if (RxFrame.Mac.CtrlWord.All == 0x5253)
            {
                TreeNode WirelessUpgradeNode = MacExplain.GetUpgradeTreeNode(RxFrame, Color.Red);
                WirelessUpgradeNode.Expand();
                parentNode.Nodes.Add(WirelessUpgradeNode);
                return parentNode;
            }

            // parentNode--Mac层
            TreeNode MacNode = MacExplain.GetTreeNode(RxFrame, Color.Red);
            MacNode.Expand();
            parentNode.Nodes.Add(MacNode);

            if (RxFrame.Mac.CtrlWord.FrameType == MacFrameType.Data)
            {
                // parentNode--Nwk层
                TreeNode NwkNode = NwkExplain.GetTreeNode(RxFrame, Color.Orange);
                NwkNode.Expand();
                parentNode.Nodes.Add(NwkNode);

                if (RxFrame.Nwk.CtrlWord.FrameType == NwkFrameType.Data)
                {
                    // parentNode--Aps层
                    TreeNode ApsNode = ApsExplain.GetTreeNode(RxFrame, Color.Green);
                    ApsNode.Expand();
                    parentNode.Nodes.Add(ApsNode);
                }
            }
            parentNode.Expand();

            return parentNode;
        }
        #endregion

        #region Mac层解析
        public class MacExplain             //Mac层解析
        {
            private static readonly CmdExplain[] MacCmdTbl = new CmdExplain[]
            {
                // 国网命令
                new CmdExplain( 0x01, "网络维护请求",         Color.Blue, new ExplainCallback(ExplainNwkMaintenanceRequest)),
                new CmdExplain( 0x02, "网络维护响应",         Color.Blue, new ExplainCallback(ExplainNwkMaintenanceResponse)),
                
                // 北网扩展命令
                new CmdExplain( 0x10, "报警箱数据请求",        Color.Blue, new ExplainCallback(ExplainAlarmBoxDataRequest)),
                new CmdExplain( 0x11, "报警箱数据应答",        Color.Blue, new ExplainCallback(ExplainAlarmBoxDataResponse)),
                new CmdExplain( 0x12, "报警箱事件上报",        Color.Blue, new ExplainCallback(ExplainAlarmBoxEventReport)),
                new CmdExplain( 0x13, "报警箱事件上报应答",    Color.Blue, new ExplainCallback(ExplainAlarmBoxEventReportResponse)),
                new CmdExplain( 0x50, "水表主动上报",          Color.Blue, new ExplainCallback(ExplainWaterAmeterAutoReport)),
                new CmdExplain( 0x51, "水表主动上报应答",      Color.Blue, new ExplainCallback(ExplainWaterAmeterAutoReportResponse)),
            };

            private static readonly CmdExplain[] WirelessUpgradeCmdTbl = new CmdExplain[]
            {
                // 三星国外-无线升级命令
                new CmdExplain( 0x83, "广播发送升级数据包",      Color.BlueViolet, new ExplainCallback(ExplainBroadcastSendUpgradeData)),
                new CmdExplain( 0x84, "广播查询升级状态",        Color.BlueViolet, new ExplainCallback(ExplainBroadcastQueryUpgradeStatus)),
                new CmdExplain( 0x85, "广播查询升级状态应答",    Color.BlueViolet, new ExplainCallback(ExplainBroadcastQueryUpgradeStatusResponse)),
                new CmdExplain( 0x87, "广播查询节点状态",        Color.BlueViolet, new ExplainCallback(ExplainBroadcastQueryNodeStatus)),
                new CmdExplain( 0x88, "广播查询节点状态应答",    Color.BlueViolet, new ExplainCallback(ExplainBroadcastQueryNodeStatusResponse)),
            };

            private static readonly CmdExplain[] LowPowerMeterCmdTbl = new CmdExplain[]
            {
                // 北网扩展-低功耗表命令帧
                new CmdExplain( 0x40, "低功耗表透抄",             Color.Blue, new ExplainCallback(ExplainLowPowerMeterCmdFrame_LowPowerMeterRead)),
                new CmdExplain( 0x41, "低功耗表透抄应答",         Color.Blue, new ExplainCallback(ExplainLowPowerMeterCmdFrame_LowPowerMeterReadResponse)),
                new CmdExplain( 0x42, "广播时隙转发",             Color.Blue, new ExplainCallback(ExplainLowPowerMeterCmdFrame_BroadcastSlotTransfer)),
                new CmdExplain( 0x43, "广播时隙识别",             Color.Blue, new ExplainCallback(ExplainLowPowerMeterCmdFrame_BroadcastSlotTransferResponse)),
                new CmdExplain( 0x44, "低功耗表配置",             Color.Blue, new ExplainCallback(ExplainLowPowerMeterCmdFrame_LowPowerMeterConfig)),
                new CmdExplain( 0x45, "低功耗表配置应答",         Color.Blue, new ExplainCallback(ExplainLowPowerMeterCmdFrame_LowPowerMeterConfigResponse)),
                new CmdExplain( 0x46, "低功耗表上报",             Color.Blue, new ExplainCallback(ExplainLowPowerMeterCmdFrame_LowPowerMeterReport)),
                new CmdExplain( 0x47, "低功耗表上报应答",         Color.Blue, new ExplainCallback(ExplainLowPowerMeterCmdFrame_LowPowerMeterReportResponse)),
                new CmdExplain( 0x4A, "低功耗表维护",             Color.Blue, new ExplainCallback(ExplainLowPowerMeterCmdFrame_LowPowerMeterMaintain)),
                new CmdExplain( 0x4B, "低功耗表维护应答",         Color.Blue, new ExplainCallback(ExplainLowPowerMeterCmdFrame_LowPowerMeterMaintainResponse)),
                
                // 桑锐水表指令
                new CmdExplain( 0xAA, "桑锐水表命令",         Color.Blue, new ExplainCallback(ExplainLowPowerMeterCmdFrame_SangReiWaterMeterCmd)),
            };

            private static string[] SangReiWaterCmdNameTbl = new string[]
            {
                "切换成单向模式",     // 0x00 
                "切换成双向模式",     // 0x01 
                "设置水表时钟",       // 0x02 
                "读取水表时钟",       // 0x03 
            };

            // 获取命令名
            public static String GetCmdName(byte cmdId)
            {
                string strName = "无法识别的命令";

                foreach (CmdExplain cmd in MacCmdTbl)
                {
                    if (cmd.CmdId == cmdId) { strName = cmd.CmdName; }
                }
                return strName;
            }
            public static String GetUpgradeCmdName(byte cmdId)
            {
                string strName = "无法识别的命令";

                foreach (CmdExplain cmd in WirelessUpgradeCmdTbl)
                {
                    if (cmd.CmdId == cmdId) { strName = cmd.CmdName; }
                }
                return strName;
            }
            public static String GetLowPowerMeterCmdName(byte cmdId)
            {
                string strName = "低功耗表其他命令";

                foreach (CmdExplain cmd in LowPowerMeterCmdTbl)
                {
                    if (cmd.CmdId == cmdId) { strName = cmd.CmdName; }
                }

                return strName;
            }

            // 获取命令颜色
            public static Color GetCmdColor(byte cmdId)
            {
                Color cmdColor = Color.Black;

                foreach (CmdExplain cmd in MacCmdTbl)
                {
                    if (cmd.CmdId == cmdId) { cmdColor = cmd.CmdColor; }
                }
                return cmdColor;
            }
            public static Color GetUpgradeCmdColor(byte cmdId)
            {
                Color cmdColor = Color.Black;

                foreach (CmdExplain cmd in WirelessUpgradeCmdTbl)
                {
                    if (cmd.CmdId == cmdId) { cmdColor = cmd.CmdColor; }
                }
                return cmdColor;
            }
            public static Color GetLowPowerMeterCmdColor(byte cmdId)
            {
                Color cmdColor = Color.Black;

                foreach (CmdExplain cmd in LowPowerMeterCmdTbl)
                {
                    if (cmd.CmdId == cmdId) { cmdColor = cmd.CmdColor; }
                }
                return cmdColor;
            }

            // 解析命令帧
            private static TreeNode ExplainCmdFrame(byte cmdId, byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载 ：无法识别");

                foreach (CmdExplain cmd in MacCmdTbl)
                {
                    if (cmd.CmdId == cmdId)
                    {
                        payloadNode = cmd.CmdExplainFunc(buf);
                    }
                }

                return payloadNode;
            }
            private static TreeNode ExplainUpgradeCmdFrame(byte cmdId, byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载 ：无法识别");

                foreach (CmdExplain cmd in WirelessUpgradeCmdTbl)
                {
                    if (cmd.CmdId == cmdId)
                    {
                        payloadNode = cmd.CmdExplainFunc(buf);
                    }
                }

                return payloadNode;
            }

            private static TreeNode ExplainLowPowerMeterCmdFrame(byte cmdId, byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载 ：无法识别");

                foreach (CmdExplain cmd in LowPowerMeterCmdTbl)
                {
                    if (cmd.CmdId == cmdId)
                    {
                        payloadNode = cmd.CmdExplainFunc(buf);
                    }
                }

                return payloadNode;
            }
            public static TreeNode GetTreeNode(FrameFormat rxFrame, Color fgColor)
            {
                TreeNode macNode = new TreeNode("Mac层");
                macNode.ForeColor = fgColor;

                TreeNode node = null;
                TreeNode payloadNode = null;
                String strTmp = "";
                int dstAddrLen;
                int srcAddrLen;

                if (rxFrame.Mac.CtrlWord.FrameType == MacFrameType.Invalid)  //无效帧
                {
                    macNode.Nodes.Add(new TreeNode("无法识别的MAC帧"));
                    return macNode;
                }

                node = new TreeNode("帧控制域：0x" + rxFrame.Mac.CtrlWord.All.ToString("X4"));
                {
                    switch (rxFrame.Mac.CtrlWord.FrameType)
                    {
                        case MacFrameType.Beacon:
                            strTmp = "帧类型  ：信标帧(" + rxFrame.Mac.CtrlWord.FrameType.ToString("D") + ")";
                            payloadNode = ExplainBeaconFrame(rxFrame.Mac.Payload);
                            break;
                        case MacFrameType.Data:
                            strTmp = "帧类型  ：数据帧(" + rxFrame.Mac.CtrlWord.FrameType.ToString("D") + ")";
                            payloadNode = new TreeNode("Mac层负载：Nwk层");
                            break;
                        case MacFrameType.Ack:
                            strTmp = "帧类型  ：确认帧(" + rxFrame.Mac.CtrlWord.FrameType.ToString("D") + ")";
                            payloadNode = new TreeNode("Mac层负载：确认帧");
                            break;
                        case MacFrameType.Cmd:
                            strTmp = "帧类型  ：命令帧(" + rxFrame.Mac.CtrlWord.FrameType.ToString("D") + ")";
                            payloadNode = ExplainCmdFrame(rxFrame.Mac.Payload[0], rxFrame.Mac.Payload);
                            break;
                        case MacFrameType.WaterGasMeterScan:
                            strTmp = "帧类型  ：水气表查找信标帧(" + rxFrame.Mac.CtrlWord.FrameType.ToString("D") + ")";
                            payloadNode = ExplainWaterGasScanFrame(rxFrame.Mac.Payload);
                            break;
                        case MacFrameType.NetworkScan:
                            strTmp = "帧类型  ：全网感知广播帧(" + rxFrame.Mac.CtrlWord.FrameType.ToString("D") + ")";
                            payloadNode = ExplainNetworkScanFrame(rxFrame.Mac.Payload);
                            break;
                        case MacFrameType.LowPwrMeterCmd:
                            strTmp = "帧类型  ：低功耗表命令帧(" + rxFrame.Mac.CtrlWord.FrameType.ToString("D") + ")";
                            payloadNode = ExplainLowPowerMeterCmdFrame(rxFrame.Mac.Payload[0], rxFrame.Mac.Payload);
                            break;

                        default:
                            payloadNode = new TreeNode("Mac层负载：无法识别");
                            break;
                    }
                    node.Nodes.Add(strTmp);

                    strTmp = "安全使能：" + (rxFrame.Mac.CtrlWord.SecFlag == true ? "有" : "无");
                    node.Nodes.Add(strTmp);

                    strTmp = "确认请求：" + (rxFrame.Mac.CtrlWord.AckFlag == true ? "有" : "无");
                    node.Nodes.Add(strTmp);

                    strTmp = "扩展信息：" + (rxFrame.Mac.CtrlWord.ExtendFlag == true ? "有" : "无");
                    node.Nodes.Add(strTmp);

                    strTmp = "帧版本  ：" + (rxFrame.Mac.CtrlWord.FrameVer.ToString("D2"));
                    node.Nodes.Add(strTmp);

                    // 控制域其他字段不需展示
                    dstAddrLen = rxFrame.Mac.CtrlWord.DstAddrMode == 0x02 ? 2 : 6;
                    srcAddrLen = rxFrame.Mac.CtrlWord.SrcAddrMode == 0x02 ? 2 : 6;

                }
                macNode.Nodes.Add(node);

                strTmp = "帧序号  ：" + rxFrame.Mac.FrameSn.ToString("X2");
                macNode.Nodes.Add(strTmp);

                strTmp = "PanID   ：" + rxFrame.Mac.PanID.ToString("X4");
                macNode.Nodes.Add(strTmp);

                strTmp = "目的地址：" + Util.GetStringHexFromBytes(rxFrame.Mac.DstAddr, 0, dstAddrLen, "", true);
                macNode.Nodes.Add(strTmp);

                strTmp = "源地址  ：" + Util.GetStringHexFromBytes(rxFrame.Mac.SrcAddr, 0, srcAddrLen, "", true);
                macNode.Nodes.Add(strTmp);

                if (true == rxFrame.Mac.CtrlWord.ExtendFlag)
                {
                    node = new TreeNode("扩展域  ：");

                    strTmp = "扩展域长度：" + rxFrame.Mac.Extend.Length.ToString();
                    node.Nodes.Add(strTmp);

                    if (rxFrame.Mac.Extend.Length >= 2)
                    {
                        strTmp = "厂家标识  ：" + rxFrame.Mac.Extend.OemInfo.ToString("X4");
                        string strOem = "(" + Convert.ToChar(rxFrame.Mac.Extend.OemInfo >> 8)
                                    + Convert.ToChar(rxFrame.Mac.Extend.OemInfo & 0x00FF) + ")";
                        node.Nodes.Add(strTmp + " " + strOem);
                        node.Text = node.Text + "厂家" + strOem;

                        strTmp = "扩展域数据：" + "(" + (rxFrame.Mac.Extend.Length - 2) + "byte)";
                        node.Nodes.Add(strTmp);
                    }
                    macNode.Nodes.Add(node);
                }

                payloadNode.Expand();
                macNode.Nodes.Add(payloadNode);

                return macNode;
            }
            public static TreeNode GetUpgradeTreeNode(FrameFormat rxFrame, Color fgColor)
            {
                TreeNode macNode = new TreeNode("无线升级帧");
                macNode.ForeColor = fgColor;

                TreeNode payloadNode = null;
                String strTmp = "";

                strTmp = "升级标识：" + Convert.ToChar(rxFrame.Mac.CtrlWord.All & 0xFF) + Convert.ToChar(rxFrame.Mac.CtrlWord.All >> 8);
                macNode.Nodes.Add(strTmp);
                strTmp = "帧序号  ：" + rxFrame.Mac.FrameSn.ToString("X2");
                macNode.Nodes.Add(strTmp);
                strTmp = "PanID   ：" + rxFrame.Mac.PanID.ToString("X4");
                macNode.Nodes.Add(strTmp);
                strTmp = "目的地址：" + Util.GetStringHexFromBytes(rxFrame.Mac.DstAddr, 0, LongAddrSize, "", true);
                macNode.Nodes.Add(strTmp);
                strTmp = "源地址  ：" + Util.GetStringHexFromBytes(rxFrame.Mac.SrcAddr, 0, LongAddrSize, "", true);
                macNode.Nodes.Add(strTmp);

                payloadNode = ExplainUpgradeCmdFrame(rxFrame.Mac.Payload[0], rxFrame.Mac.Payload);
                payloadNode.Expand();
                macNode.Nodes.Add(payloadNode);

                return macNode;
            }


            // 信标帧
            private static TreeNode ExplainBeaconFrame(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载：信标帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < 16) return payloadNode;

                strTmp = "发射随机延时：" + buf[index++].ToString();
                payloadNode.Nodes.Add(strTmp);

                strTmp = "信标轮次：" + buf[index++].ToString();
                payloadNode.Nodes.Add(strTmp);

                strTmp = "层次号  ：" + ((buf[index] + (buf[index + 1] << 8)) >> 10);
                payloadNode.Nodes.Add(strTmp);

                strTmp = "时隙号  ：" + ((buf[index] + (buf[index + 1] << 8)) & 0x3FF);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "信标标识：" + buf[index++].ToString("X2");
                payloadNode.Nodes.Add(strTmp);

                strTmp = "网络规模：" + (buf[index] + (buf[index + 1] << 8));
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "场强门限：" + buf[index++].ToString();
                payloadNode.Nodes.Add(strTmp);

                strTmp = "中心节点PanID：" + buf[index + 1].ToString("X2") + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "中心节点地址 ：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }

            // 查找水气表广播帧
            private static TreeNode ExplainWaterGasScanFrame(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载：水气表查找信标帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < index + 9) return payloadNode;

                strTmp = "发射随机延时：" + buf[index++];
                payloadNode.Nodes.Add(strTmp);

                strTmp = "时隙号      ：" + (buf[index] + (buf[index + 1] << 8));
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "广播标识    ：" + buf[index++].ToString("X2");
                payloadNode.Nodes.Add(strTmp);

                strTmp = "电表网络规模：" + (buf[index] + (buf[index + 1] << 8));
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "水气表网络规模：" + (buf[index] + (buf[index + 1] << 8));
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "水气表场强门限：" + buf[index++];
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }

            // 全网感知帧
            private static TreeNode ExplainNetworkScanFrame(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载：全网感知广播帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < index + 7) return payloadNode;

                if (buf[index] == 0x01)
                {
                    // 全网感知--请求
                    strTmp = "命令标识：" + "全网感知--请求 (0x" + buf[index++].ToString("X2") + ")";
                    payloadNode.Nodes.Add(strTmp);
                    strTmp = "命令序号：" + buf[index++].ToString();
                    payloadNode.Nodes.Add(strTmp);
                    strTmp = "广播标识：" + buf[index++].ToString("X2");
                    payloadNode.Nodes.Add(strTmp);
                    strTmp = "当前时隙：" + (buf[index] + (buf[index + 1] << 8)).ToString();
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;
                    strTmp = "总时隙  ：" + ((buf[index] + (buf[index + 1] << 8)) & 0x3FF).ToString();
                    payloadNode.Nodes.Add(strTmp);
                    strTmp = "竞争时隙数：" + ((buf[index] + (buf[index + 1] << 8)) >> 10).ToString();
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;
                }
                else if (buf[index] == 0x02)
                {
                    // 全网感知--响应
                    strTmp = "命令标识：" + "全网感知--响应 (0x" + buf[index++].ToString("X2") + ")";
                    payloadNode.Nodes.Add(strTmp);
                    strTmp = "命令序号：" + buf[index++].ToString();
                    payloadNode.Nodes.Add(strTmp);
                    strTmp = "广播标识：" + buf[index++].ToString("X2");
                    payloadNode.Nodes.Add(strTmp);
                    strTmp = "当前时隙：" + (buf[index] + (buf[index + 1] << 8)).ToString();
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;
                    strTmp = "总时隙  ：" + ((buf[index] + (buf[index + 1] << 8)) & 0x3FF).ToString();
                    payloadNode.Nodes.Add(strTmp);
                    strTmp = "竞争时隙数：" + ((buf[index] + (buf[index + 1] << 8)) >> 10).ToString();
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;

                    if (buf.Length < index + 50) return payloadNode;

                    int slotCnt = 0;
                    for (int i = 0; i < 50; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            slotCnt += buf[index] & 0x01;
                        }
                        index++;
                    }
                    strTmp = "扫描到时隙数：" + slotCnt;
                    payloadNode.Nodes.Add(strTmp);
                }
                else if (buf[index] == 0x03)
                {
                    // 全网感知--时隙同步
                    strTmp = "命令标识：" + "全网感知--时隙同步 (0x" + buf[index++].ToString("X2") + ")";
                    payloadNode.Nodes.Add(strTmp);
                    strTmp = "命令序号：" + buf[index++].ToString();
                    payloadNode.Nodes.Add(strTmp);
                    strTmp = "广播标识：" + buf[index++].ToString("X2");
                    payloadNode.Nodes.Add(strTmp);
                    strTmp = "当前时隙：" + (buf[index] + (buf[index + 1] << 8)).ToString();
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;
                    strTmp = "总时隙  ：" + ((buf[index] + (buf[index + 1] << 8)) & 0x3FF).ToString();
                    payloadNode.Nodes.Add(strTmp);
                    strTmp = "竞争时隙数：" + ((buf[index] + (buf[index + 1] << 8)) >> 10).ToString();
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;

                    if (buf.Length < index + 50) return payloadNode;

                    int slotCnt = 0;
                    for (int i = 0; i < 50; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            slotCnt += buf[index] & 0x01;
                        }
                        index++;
                    }
                    strTmp = "扫描到时隙数：" + slotCnt;
                    payloadNode.Nodes.Add(strTmp);
                }
                else if (buf[index] == 0x04)
                {
                    // 全网感知--掉电上报
                    strTmp = "命令标识：" + "全网感知--掉电上报 (0x" + buf[index++].ToString("X2") + ")";
                    payloadNode.Nodes.Add(strTmp);
                    strTmp = "命令序号：" + buf[index++].ToString();
                    payloadNode.Nodes.Add(strTmp);
                    strTmp = "广播标识：" + buf[index++].ToString("X2");
                    payloadNode.Nodes.Add(strTmp);
                    strTmp = "当前时隙：" + (buf[index] + (buf[index + 1] << 8)).ToString();
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;
                    strTmp = "总时隙  ：" + ((buf[index] + (buf[index + 1] << 8)) & 0x3FF).ToString();
                    payloadNode.Nodes.Add(strTmp);
                    strTmp = "竞争时隙数：" + ((buf[index] + (buf[index + 1] << 8)) >> 10).ToString();
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;

                    if (buf.Length < index + 50) return payloadNode;

                    int slotCnt = 0;
                    for (int i = 0; i < 50; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            slotCnt += buf[index] & 0x01;
                        }
                        index++;
                    }
                    strTmp = "扫描到时隙数：" + slotCnt;
                    payloadNode.Nodes.Add(strTmp);
                }

                return payloadNode;
            }

            //--------------------  Mac层：低功耗表命令帧 07 -----------------------------------------
            // 低功耗表透抄
            private static TreeNode ExplainLowPowerMeterCmdFrame_LowPowerMeterRead(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载：低功耗表命令帧");

                String strTmp = "";
                int index = 0;

                // 命令
                if (buf.Length < index + 4) return payloadNode;

                strTmp = "命令标识：" + GetLowPowerMeterCmdName(buf[index]) + " (0x" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "波特率  ：" + (buf[index] < BaudRateTbl.Length ? BaudRateTbl[buf[index]] : (buf[index] * 1200).ToString());
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "超时时间：" + buf[index] * 10 + "ms";
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "报文长度：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (buf.Length < index + buf[index - 1]) return payloadNode;

                TreeNode node = new TreeNode("188-04报文");
                payloadNode.Nodes.Add(node);
                for (; index < buf.Length; index++)
                {
                    if (buf[index] == 0x68 && buf[buf.Length - 1] == 0x16)
                    {
                        index += 1;

                        if (buf.Length < index + 12) return payloadNode;

                        strTmp = "仪表类型：" + "0x" + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 1;
                        strTmp = "仪表地址：" + Util.GetStringHexFromBytes(buf, index, 7, "", true);
                        node.Nodes.Add(strTmp);
                        index += 7;
                        strTmp = "控制字  ：" + "0x" + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 1;
                        strTmp = "数据长度：" + buf[index];
                        node.Nodes.Add(strTmp);
                        index += 1;
                        strTmp = "数据标识：" + (buf[index] * 256 + buf[index + 1]).ToString("X4");
                        node.Nodes.Add(strTmp);
                        index += 2;
                        strTmp = "帧序号  ：" + buf[index];
                        node.Nodes.Add(strTmp);
                        index += 1;

                        break;
                    }
                }
                node.Expand();

                return payloadNode;
            }
            private static TreeNode ExplainLowPowerMeterCmdFrame_LowPowerMeterReadResponse(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载：低功耗表命令帧");

                String strTmp = "";
                int index = 0;

                // 应答
                if (buf.Length < index + 2) return payloadNode;

                strTmp = "命令标识：" + GetLowPowerMeterCmdName(buf[index]) + " (0x" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "报文长度：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (buf.Length < index + buf[index - 1]) return payloadNode;

                TreeNode node = new TreeNode("188-04报文");
                payloadNode.Nodes.Add(node);
                for (; index < buf.Length; index++)
                {
                    if (buf[index] == 0x68 && buf[buf.Length - 1] == 0x16)
                    {
                        index += 1;

                        if (buf.Length < index + 12) return payloadNode;

                        strTmp = "仪表类型：" + "0x" + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 1;
                        strTmp = "仪表地址：" + Util.GetStringHexFromBytes(buf, index, 7, "", true);
                        node.Nodes.Add(strTmp);
                        index += 7;
                        strTmp = "控制字  ：" + "0x" + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 1;
                        strTmp = "数据长度：" + buf[index];
                        node.Nodes.Add(strTmp);
                        index += 1;
                        strTmp = "数据标识：" + (buf[index] * 256 + buf[index + 1]).ToString("X4");
                        node.Nodes.Add(strTmp);
                        index += 2;
                        strTmp = "帧序号  ：" + buf[index];
                        node.Nodes.Add(strTmp);
                        index += 1;

                        if (buf.Length < index + 19) return payloadNode;

                        strTmp = "累计用量：" + BitConverter.ToUInt32(buf, index).ToString("X8");
                        node.Nodes.Add(strTmp);
                        index += 4;
                        strTmp = "流量单位：" + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 1;
                        strTmp = "上月累计：" + BitConverter.ToUInt32(buf, index).ToString("X8");
                        node.Nodes.Add(strTmp);
                        index += 4;
                        strTmp = "流量单位：" + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 1;
                        strTmp = "水表时间：" + buf[index + 6].ToString("X2")
                                + buf[index + 5].ToString("X2") + "-"
                                + buf[index + 4].ToString("X2") + "-"
                                + buf[index + 3].ToString("X2") + " "
                                + buf[index + 2].ToString("X2") + ":"
                                + buf[index + 1].ToString("X2") + ":"
                                + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 7;
                        strTmp = "状态字  ：" + BitConverter.ToUInt16(buf, index).ToString("X4");
                        node.Nodes.Add(strTmp);
                        index += 2;

                        break;
                    }
                }
                node.Expand();

                return payloadNode;
            }

            // 广播时隙转发
            private static TreeNode ExplainLowPowerMeterCmdFrame_BroadcastSlotTransfer(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载：低功耗表命令帧");

                String strTmp = "";
                int index = 0;

                // 时隙转发命令
                if (buf.Length < index + 8) return payloadNode;

                strTmp = "命令标识：" + GetLowPowerMeterCmdName(buf[index]) + " (0x" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "广播序号：" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "电表网络规模：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "电表当前时隙：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "低功耗表网络规模：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                return payloadNode;
            }
            private static TreeNode ExplainLowPowerMeterCmdFrame_BroadcastSlotTransferResponse(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载：低功耗表命令帧");

                String strTmp = "";
                int index = 0;

                // 时隙转发识别
                if (buf.Length < index + 10) return payloadNode;

                strTmp = "命令标识：" + GetLowPowerMeterCmdName(buf[index]) + " (0x" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "广播序号：" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "电表网络规模：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "电表当前时隙：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "低功耗表网络规模：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "低功耗表当前时隙：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                return payloadNode;
            }

            // 低功耗表配置
            private static TreeNode ExplainLowPowerMeterCmdFrame_LowPowerMeterConfig(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载：低功耗表命令帧");

                String strTmp = "";
                int index = 0;

                // 命令
                if (buf.Length < index + 23) return payloadNode;

                strTmp = "命令标识：" + GetLowPowerMeterCmdName(buf[index]) + " (0x" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "工作信道组：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "归属电表地址：" + Util.GetStringHexFromBytes(buf, index, 6, "", true);
                payloadNode.Nodes.Add(strTmp);
                index += 6;
                strTmp = "网络号：" + (buf[index] + buf[index + 1] * 256).ToString("X4");
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "时隙号：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "实时时钟：" + DateTime.Now.Year / 100
                        + buf[index + 6].ToString("X2") + "-"
                        + buf[index + 5].ToString("X2") + "-"
                        + buf[index + 4].ToString("X2") + " "
                        + buf[index + 2].ToString("X2") + ":"
                        + buf[index + 1].ToString("X2") + ":"
                        + buf[index].ToString("X2") + " ";
                switch (buf[index + 3])
                {
                    case 0: strTmp += "星期日"; break;
                    case 1: strTmp += "星期一"; break;
                    case 2: strTmp += "星期二"; break;
                    case 3: strTmp += "星期三"; break;
                    case 4: strTmp += "星期四"; break;
                    case 5: strTmp += "星期五"; break;
                    case 6: strTmp += "星期六"; break;
                    default: strTmp += "星期X"; break;
                }
                payloadNode.Nodes.Add(strTmp);
                index += 7;
                strTmp = "工作模式：" + buf[index] + " (" + (buf[index] == 0 ? "持续工作--忽略工作时段" : "特定时段工作") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "工作时段：" + buf[index + 2].ToString("X2") + buf[index + 1].ToString("X2")
                        + buf[index].ToString("X2") + " ( 24bit代表0~23点，置1时工作)";
                payloadNode.Nodes.Add(strTmp);
                index += 3;

                return payloadNode;
            }
            private static TreeNode ExplainLowPowerMeterCmdFrame_LowPowerMeterConfigResponse(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载：低功耗表命令帧");

                String strTmp = "";
                int index = 0;

                // 响应
                if (buf.Length < index + 2) return payloadNode;

                strTmp = "命令标识：" + GetLowPowerMeterCmdName(buf[index]) + " (0x" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "工作信道组：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                // 扩展的5字节
                if (buf.Length < index + 5) return payloadNode;

                strTmp = "软件版本：" + Encoding.ASCII.GetString(buf, index, 5);
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }

            // 低功耗表主动上报
            private static TreeNode ExplainLowPowerMeterCmdFrame_LowPowerMeterReport(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载：低功耗表命令帧");

                String strTmp = "";
                int index = 0;

                // 命令
                if (buf.Length < index + 2) return payloadNode;

                strTmp = "命令标识：" + GetLowPowerMeterCmdName(buf[index]) + " (0x" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "报文长度：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (buf.Length < index + buf[index - 1]) return payloadNode;

                TreeNode node = new TreeNode("188-04报文");
                payloadNode.Nodes.Add(node);
                for (; index < buf.Length; index++)
                {
                    if (buf[index] == 0x68 && buf[buf.Length - 1] == 0x16)
                    {
                        index += 1;

                        if (buf.Length < index + 10) return payloadNode;

                        strTmp = "仪表类型：" + "0x" + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 1;
                        strTmp = "仪表地址：" + Util.GetStringHexFromBytes(buf, index, 7, "", true);
                        node.Nodes.Add(strTmp);
                        index += 7;
                        strTmp = "控制字  ：" + "0x" + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 1;
                        strTmp = "数据长度：" + buf[index];
                        node.Nodes.Add(strTmp);
                        index += 1;
                        strTmp = "帧序号  ：" + "0x" + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 1;
                        strTmp = "其他信息：" + "暂不解析";
                        node.Nodes.Add(strTmp);
                        break;
                    }
                }
                node.Expand();

                return payloadNode;
            }
            private static TreeNode ExplainLowPowerMeterCmdFrame_LowPowerMeterReportResponse(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载：低功耗表命令帧");

                String strTmp = "";
                int index = 0;

                // 响应
                if (buf.Length < index + 2) return payloadNode;

                strTmp = "命令标识：" + GetLowPowerMeterCmdName(buf[index]) + " (0x" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "报文长度：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (buf.Length < index + buf[index - 1]) return payloadNode;

                TreeNode node = new TreeNode("188-04报文");
                payloadNode.Nodes.Add(node);
                for (; index < buf.Length; index++)
                {
                    if (buf[index] == 0x68 && buf[buf.Length - 1] == 0x16)
                    {
                        index += 1;

                        if (buf.Length < index + 10) return payloadNode;

                        strTmp = "仪表类型：" + "0x" + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 1;
                        strTmp = "仪表地址：" + Util.GetStringHexFromBytes(buf, index, 7, "", true);
                        node.Nodes.Add(strTmp);
                        index += 7;
                        strTmp = "控制字  ：" + "0x" + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 1;
                        strTmp = "数据长度：" + buf[index];
                        node.Nodes.Add(strTmp);
                        index += 1;
                        strTmp = "帧序号  ：" + "0x" + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 1;
                        strTmp = "其他信息：" + "暂不解析";
                        node.Nodes.Add(strTmp);
                        break;
                    }
                }
                node.Expand();

                return payloadNode;
            }

            // 低功耗表维护
            private static TreeNode ExplainLowPowerMeterCmdFrame_LowPowerMeterMaintain(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载：低功耗表命令帧");

                String strTmp = "";
                int index = 0;

                // 命令
                if (buf.Length < index + 4) return payloadNode;

                strTmp = "命令标识：" + GetLowPowerMeterCmdName(buf[index]) + " (0x" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                return payloadNode;
            }
            private static TreeNode ExplainLowPowerMeterCmdFrame_LowPowerMeterMaintainResponse(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载：低功耗表命令帧");

                String strTmp = "";
                int index = 0;

                // 响应
                if (buf.Length < index + 8) return payloadNode;

                strTmp = "命令标识：" + GetLowPowerMeterCmdName(buf[index]) + " (0x" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "工作信道组：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "时隙号：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "硬件标识：" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "软件版本：" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "场强门限：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "接收场强：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                return payloadNode;
            }

            // 桑锐水表命令
            private static TreeNode ExplainLowPowerMeterCmdFrame_SangReiWaterMeterCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载：低功耗表命令帧");

                String strTmp = "";
                int index = 0;

                // 响应
                if (buf.Length < index + 5) return payloadNode;

                strTmp = "命令标识：" + GetLowPowerMeterCmdName(buf[index]) + " (0x" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "报文长度：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "特征字  ：" + (buf[index] + buf[index + 1] * 256).ToString("X4");
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                string strCmd = (buf[index] < SangReiWaterCmdNameTbl.Length ? SangReiWaterCmdNameTbl[buf[index]] : "其他指令");
                strTmp = "命令字  ：" + buf[index].ToString("X2") + " " + strCmd;
                TreeNode node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                index += 1;

                switch (strCmd)
                {
                    case "切换成单向模式":
                    case "切换成双向模式":
                        if (buf.Length == index + 1)
                        {
                            node.Text += "-应答";
                            strTmp = "执行结果：" + (buf[index] == 0xAA ? "成功" : "失败");
                            payloadNode.Nodes.Add(strTmp);
                            index += 1;
                        }
                        break;

                    case "设置水表时钟":
                        if (buf.Length == index + 6)
                        {
                            strTmp = "水表时钟：" + DateTime.Now.Year / 100
                                    + buf[index + 5].ToString("X2") + "-"
                                    + buf[index + 4].ToString("X2") + "-"
                                    + buf[index + 3].ToString("X2") + " "
                                    + buf[index + 2].ToString("X2") + ":"
                                    + buf[index + 1].ToString("X2") + ":"
                                    + buf[index].ToString("X2");
                            payloadNode.Nodes.Add(strTmp);
                            index += 6;
                        }
                        else if (buf.Length == index + 7)
                        {
                            node.Text += "-应答";
                            strTmp = "执行结果：" + (buf[index] == 0xAA ? "成功" : "失败");
                            payloadNode.Nodes.Add(strTmp);
                            index += 1;
                            strTmp = "水表时钟：" + DateTime.Now.Year / 100
                                    + buf[index + 5].ToString("X2") + "-"
                                    + buf[index + 4].ToString("X2") + "-"
                                    + buf[index + 3].ToString("X2") + " "
                                    + buf[index + 2].ToString("X2") + ":"
                                    + buf[index + 1].ToString("X2") + ":"
                                    + buf[index].ToString("X2");
                            payloadNode.Nodes.Add(strTmp);
                            index += 6;
                        }
                        break;

                    case "读取水表时钟":
                        if (buf.Length == index + 6)
                        {
                            node.Text += "-应答";
                            strTmp = "水表时钟：" + DateTime.Now.Year / 100
                                    + buf[index + 5].ToString("X2") + "-"
                                    + buf[index + 4].ToString("X2") + "-"
                                    + buf[index + 3].ToString("X2") + " "
                                    + buf[index + 2].ToString("X2") + ":"
                                    + buf[index + 1].ToString("X2") + ":"
                                    + buf[index].ToString("X2");
                            payloadNode.Nodes.Add(strTmp);
                            index += 6;
                        }
                        break;
                }

                return payloadNode;
            }


            //---------------------------  Mac层 命令帧 03  ------------------------------------
            //网络维护请求
            private static TreeNode ExplainNwkMaintenanceRequest(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载：命令帧");

                TreeNode node = null;
                String strTmp = "";
                int index = 0;
                int relayCount;

                if (buf.Length < 2) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf[index]) + "(0x" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index++;

                node = new TreeNode("路由域");
                payloadNode.Nodes.Add(node);
                {
                    relayCount = (buf[index] & 0x0F);
                    strTmp = "路径节点总数：" + relayCount.ToString();
                    node.Nodes.Add(strTmp);

                    strTmp = "路径索引：" + (buf[index] >> 4).ToString();
                    node.Nodes.Add(strTmp);
                    index++;

                    if (buf.Length < index + relayCount * LongAddrSize) return payloadNode;

                    // 路径节点列表  
                    for (int i = 0; i < relayCount; i++)
                    {
                        strTmp = "节点" + (i + 1) + "：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                        node.Nodes.Add(strTmp);
                        index += LongAddrSize;
                    }
                }
                node.Expand();

                if (buf.Length < index + (relayCount - 1) * 1) return payloadNode;

                node = new TreeNode("下行场强列表：");
                {
                    for (int i = 0; i < relayCount - 1; i++)
                    {
                        strTmp = "第" + (i + 1) + "跳：" + buf[index++];
                        node.Nodes.Add(strTmp);
                    }
                }
                node.Expand();
                payloadNode.Nodes.Add(node);

                return payloadNode;
            }
            //网络维护响应
            private static TreeNode ExplainNwkMaintenanceResponse(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载 ：命令帧");

                TreeNode node = null;
                String strTmp = "";
                int index = 0;
                int relayCount;

                if (buf.Length < index + 2) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf[index]) + "(0x" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index++;

                node = new TreeNode("路由域");
                payloadNode.Nodes.Add(node);
                {
                    relayCount = (buf[index] & 0x0F);
                    strTmp = "路径节点总数：" + relayCount.ToString();
                    node.Nodes.Add(strTmp);

                    strTmp = "路径索引：" + (buf[index] >> 4).ToString();
                    node.Nodes.Add(strTmp);
                    index++;
                    node.Expand();

                    if (buf.Length < index + relayCount * LongAddrSize) return payloadNode;

                    // 路径节点列表  
                    for (int i = 0; i < relayCount; i++)
                    {
                        strTmp = "节点" + (i + 1) + "：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                        node.Nodes.Add(strTmp);
                        index += LongAddrSize;
                    }
                }
                node.Expand();


                if (buf.Length < index + (relayCount - 1) * 2) return payloadNode;

                node = new TreeNode("下行场强列表");
                {
                    for (int i = 0; i < relayCount - 1; i++)
                    {
                        strTmp = "第" + (i + 1) + "跳：" + buf[index++];
                        node.Nodes.Add(strTmp);
                    }
                }
                node.Expand();
                payloadNode.Nodes.Add(node);

                node = new TreeNode("上行场强列表");
                {
                    for (int i = 0; i < relayCount - 1; i++)
                    {
                        strTmp = "第" + (i + 1) + "跳：" + buf[index++];
                        node.Nodes.Add(strTmp);
                    }
                }
                node.Expand();
                payloadNode.Nodes.Add(node);

                return payloadNode;
            }

            //报警箱数据请求
            private static TreeNode ExplainAlarmBoxDataRequest(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载：命令帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < 3) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf[index]) + "(0x" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index++;
                strTmp = "频道    ：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index++;
                strTmp = "数据长度：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index++;
                strTmp = "数据内容：" + "( 略 )";
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }
            //报警箱数据应答
            private static TreeNode ExplainAlarmBoxDataResponse(byte[] buf)
            {
                return ExplainAlarmBoxDataRequest(buf);     // 应答与请求格式相同
            }

            //报警箱事件上报
            private static TreeNode ExplainAlarmBoxEventReport(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载：命令帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < 2) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf[index]) + "(0x" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index++;
                strTmp = "事件序号：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index++;
                strTmp = "数据长度：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index++;
                strTmp = "数据内容：" + "( 略 )";
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }
            //报警箱事件上报应答
            private static TreeNode ExplainAlarmBoxEventReportResponse(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载：命令帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < 3) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf[index]) + "(0x" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index++;
                strTmp = "频道    ：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index++;
                strTmp = "事件序号：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index++;

                return payloadNode;
            }
            //水表主动上报
            private static TreeNode ExplainWaterAmeterAutoReport(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载：命令帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < 10) return payloadNode;
                // T188协议帧：0x68 + 0x10 + 地址7 + 0x84 + 长度 + 数据区20 + CS + 0x16

                strTmp = "命令标识：" + GetCmdName(buf[index]) + "(0x" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index++;

                index += 2;     // 跳过 0x68 + 0x10

                strTmp = "水表地址：" + Util.GetStringHexFromBytes(buf, index, 7, "", true);
                payloadNode.Nodes.Add(strTmp);
                index += 7;

                index += 3;     // 跳过 0x84 + 长度 + 数据区的命令字0xC6

                if (buf.Length < index + 20) return payloadNode;

                strTmp = "上报时间：" + DateTime.Now.Year / 100
                            + buf[index + 5].ToString("X2") + "-"
                            + buf[index + 4].ToString("X2") + "-"
                            + buf[index + 3].ToString("X2") + " "
                            + buf[index + 2].ToString("X2") + ":"
                            + buf[index + 1].ToString("X2") + ":"
                            + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 6;
                strTmp = "表分类  ：" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "累计用量："
                        + buf[index + 3].ToString("X2")
                        + buf[index + 2].ToString("X2")
                        + buf[index + 1].ToString("X2") + "."
                        + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 4;
                strTmp = "统计日期：" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "上月累计："
                        + buf[index + 3].ToString("X2")
                        + buf[index + 2].ToString("X2")
                        + buf[index + 1].ToString("X2") + "."
                        + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 4;
                strTmp = "欠压状态：" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "出厂年份：" + DateTime.Now.Year / 100 + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                return payloadNode;
            }
            //水表主动上报应答
            private static TreeNode ExplainWaterAmeterAutoReportResponse(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载：命令帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < 1) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf[index]) + "(0x" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index++;

                if (buf.Length < 2) return payloadNode;

                strTmp = "水表模式：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index++;

                return payloadNode;
            }

            //广播发送升级数据包
            private static TreeNode ExplainBroadcastSendUpgradeData(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载：无线升级帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < 4) return payloadNode;

                strTmp = "命令字  ：" + GetUpgradeCmdName(buf[index]) + "(0x" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index++;
                strTmp = "命令选项：" + buf[index].ToString("X2") + "(" + (buf[index] == 0x11 ? "4E88升级" : "无法识别") + ")";
                payloadNode.Nodes.Add(strTmp);
                index++;
                strTmp = "升级模式：" + buf[index].ToString("X2") + "(" + (buf[index] == 0 ? "延时广播" : "时隙广播") + ")";
                payloadNode.Nodes.Add(strTmp);
                index++;
                strTmp = "广播序号：" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index++;

                if (buf.Length < index + 10) return payloadNode;

                strTmp = "总时隙  ：" + (buf[index + 1] * 256 + buf[index]);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "当前时隙：" + (buf[index + 1] * 256 + buf[index]);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "升级程序CRC16 ：" + (buf[index + 1] * 256 + buf[index]).ToString("X4");
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "升级数据总包数：" + (buf[index + 1] * 256 + buf[index]);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "升级数据包序号：" + (buf[index + 1] * 256 + buf[index]);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "升级数据：" + (buf.Length - index) + " byte";
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }
            //广播查询升级状态
            private static TreeNode ExplainBroadcastQueryUpgradeStatus(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载：无线升级帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < 4) return payloadNode;

                strTmp = "命令字  ：" + GetUpgradeCmdName(buf[index]) + "(0x" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index++;
                strTmp = "命令选项：" + buf[index].ToString("X2") + "(" + (buf[index] == 0x11 ? "4E88升级" : "无法识别") + ")";
                payloadNode.Nodes.Add(strTmp);
                index++;
                strTmp = "升级模式：" + buf[index].ToString("X2") + "(" + (buf[index] == 0 ? "延时广播" : "时隙广播") + ")";
                payloadNode.Nodes.Add(strTmp);
                index++;
                strTmp = "广播序号：" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index++;

                if (buf.Length < index + 10) return payloadNode;

                strTmp = "总时隙  ：" + (buf[index + 1] * 256 + buf[index]);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "当前时隙：" + (buf[index + 1] * 256 + buf[index]);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "升级程序CRC16 ：" + (buf[index + 1] * 256 + buf[index]).ToString("X4");
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                int pktCnt = (buf[index + 1] * 256 + buf[index]);
                int recvCnt = 0;
                strTmp = "升级数据总包数：" + (buf[index + 1] * 256 + buf[index]);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                if (buf.Length < index + (pktCnt + 7) / 8) return payloadNode;

                strTmp = "升级标志：" + "接收率 ";
                for (int i = 0, cnt = 0; i < (pktCnt + 7) / 8; i++)
                {
                    for (int k = 0; k < 8; k++)
                    {
                        if ((buf[index] & (1 << k)) > 0)
                        {
                            recvCnt++;
                        }

                        if (++cnt >= pktCnt)
                        {
                            break;
                        }
                    }
                    index++;
                }
                strTmp += ((float)recvCnt * 100 / pktCnt).ToString("F2") + "%" + " ( " + recvCnt + "/" + pktCnt + " )";
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }
            //广播查询升级状态应答
            private static TreeNode ExplainBroadcastQueryUpgradeStatusResponse(byte[] buf)
            {
                return ExplainBroadcastQueryUpgradeStatus(buf);     // 应答与请求数据格式相同
            }
            //广播查询节点状态
            private static TreeNode ExplainBroadcastQueryNodeStatus(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Mac层负载：无线升级帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < 4) return payloadNode;

                strTmp = "命令字  ：" + GetUpgradeCmdName(buf[index]) + "(0x" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index++;
                strTmp = "命令选项：" + buf[index].ToString("X2") + "(" + (buf[index] == 0x11 ? "4E88升级" : "无法识别") + ")";
                payloadNode.Nodes.Add(strTmp);
                index++;
                strTmp = "升级模式：" + buf[index].ToString("X2") + "(" + (buf[index] == 0 ? "延时广播" : "时隙广播") + ")";
                payloadNode.Nodes.Add(strTmp);
                index++;
                strTmp = "广播序号：" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index++;

                if (buf.Length < index + 10) return payloadNode;

                int timeSlotCnt = (buf[index + 1] * 256 + buf[index]);
                int recvFinishCnt = 0;
                strTmp = "总时隙  ：" + (buf[index + 1] * 256 + buf[index]);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "当前时隙：" + (buf[index + 1] * 256 + buf[index]);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "升级程序CRC16 ：" + (buf[index + 1] * 256 + buf[index]).ToString("X4");
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "升级数据总包数：" + (buf[index + 1] * 256 + buf[index]);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                if (buf.Length < index + (timeSlotCnt + 7) / 8) return payloadNode;

                strTmp = "升级成功标志  ：" + "成功率 ";
                for (int i = 0, cnt = 0; i < (timeSlotCnt + 7) / 8; i++)
                {
                    for (int k = 0; k < 8; k++)
                    {
                        if ((buf[index] & (1 << k)) > 0)
                        {
                            recvFinishCnt++;
                        }

                        if (++cnt >= timeSlotCnt)
                        {
                            break;
                        }
                    }
                    index++;
                }
                strTmp += ((float)recvFinishCnt * 100 / (timeSlotCnt - 3)).ToString("F2") + "%" + " ( " + recvFinishCnt + "/" + (timeSlotCnt - 3) + " )";
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }
            //广播查询节点状态应答
            private static TreeNode ExplainBroadcastQueryNodeStatusResponse(byte[] buf)
            {
                return ExplainBroadcastQueryNodeStatus(buf);     // 应答与请求数据格式相同
            }

        }
        #endregion

        #region Nwk层解析
        public class NwkExplain             //Nwk层解析
        {
            private static readonly CmdExplain[] NwkCmdTbl = new CmdExplain[]
            {
                // 国网标准命令
                new CmdExplain( 0x01, "入网申请请求",         Color.Orchid, new ExplainCallback(ExplainJoinNwkRequest)),
                new CmdExplain( 0x02, "入网申请响应",         Color.Orchid, new ExplainCallback(ExplainJoinNwkResponse)),
                new CmdExplain( 0x16, "游离节点就绪",         Color.Orchid, new ExplainCallback(ExplainOffLineNodeReadyCmd)),
                new CmdExplain( 0x03, "路由错误",             Color.Crimson, new ExplainCallback(ExplainRouteErrorCmd)),
                new CmdExplain( 0x10, "场强收集",             Color.DarkOrange, new ExplainCallback(ExplainGatherRssiRequest)),
                new CmdExplain( 0x11, "场强收集应答",         Color.DarkOrange, new ExplainCallback(ExplainGatherRssiResponse)),
                new CmdExplain( 0x12, "配置子节点",           Color.DarkGreen, new ExplainCallback(ExplainConfigSubNodeRequest)),
                new CmdExplain( 0x13, "配置子节点应答",       Color.DarkGreen, new ExplainCallback(ExplainConfigSubNodeResponse)),

                // 北网扩展 -- 水气表相关命令
                new CmdExplain( 0x0A, "收集水表上报数据",       Color.Magenta, new ExplainCallback(ExplainGatherWaterAmeterReportData)),
                new CmdExplain( 0x0B, "收集水表上报数据应答",   Color.Magenta, new ExplainCallback(ExplainGatherWaterAmeterReportDataResponse)),
                new CmdExplain( 0x0C, "水气表场强收集",         Color.Orange, new ExplainCallback(ExplainGatherWaterGasAmeterRssi)),
                new CmdExplain( 0x0D, "水气表场强收集应答",     Color.Orange, new ExplainCallback(ExplainGatherWaterGasAmeterRssiResponse)),
            };

            public static String GetCmdName(byte cmdId)
            {
                string strName = "无法识别的命令";

                foreach (CmdExplain cmd in NwkCmdTbl)
                {
                    if (cmd.CmdId == cmdId)
                    {
                        strName = cmd.CmdName;
                    }
                }
                return strName;
            }
            public static Color GetCmdColor(byte cmdId)
            {
                Color cmdColor = Color.Black;

                foreach (CmdExplain cmd in NwkCmdTbl)
                {
                    if (cmd.CmdId == cmdId)
                    {
                        cmdColor = cmd.CmdColor;
                    }
                }
                return cmdColor;
            }
            private static TreeNode ExplainCmdFrame(byte cmdId, byte[] buf)
            {
                ExplainCallback callback = null;
                TreeNode payloadNode = null;

                foreach (CmdExplain cmd in NwkCmdTbl)
                {
                    if (cmd.CmdId == cmdId)
                    {
                        callback = cmd.CmdExplainFunc;
                    }
                }

                if (callback != null)
                {
                    payloadNode = callback(buf);
                }
                else
                {
                    payloadNode = new TreeNode("Nwk层负载 ：无法识别");
                }

                return payloadNode;
            }

            public static TreeNode GetTreeNode(FrameFormat rxFrame, Color fgColor)
            {
                TreeNode nwkNode = new TreeNode("Nwk层");
                nwkNode.ForeColor = fgColor;

                TreeNode node = null;
                TreeNode payloadNode = null;
                String strTmp = "";
                int dstAddrLen;
                int srcAddrLen;

                if (rxFrame.Nwk.CtrlWord.FrameType == NwkFrameType.Invalid)
                {
                    nwkNode.Nodes.Add(new TreeNode("无法识别的NWK帧"));
                    return nwkNode;
                }

                node = new TreeNode("帧控制域：0x" + rxFrame.Nwk.CtrlWord.All.ToString("X2"));
                {
                    switch (rxFrame.Nwk.CtrlWord.FrameType)
                    {
                        case NwkFrameType.Data:
                            strTmp = "帧类型      ：数据帧(" + rxFrame.Nwk.CtrlWord.FrameType.ToString("D") + ")";
                            payloadNode = new TreeNode("Nwk层负载 ：Aps层");
                            break;

                        case NwkFrameType.Cmd:
                            strTmp = "帧类型      ：命令帧(" + rxFrame.Nwk.CtrlWord.FrameType.ToString("D") + ")";
                            payloadNode = ExplainCmdFrame(rxFrame.Nwk.Payload[0], rxFrame.Nwk.Payload);
                            break;

                        default:
                            payloadNode = new TreeNode("Nwk层负载 ：无法识别");
                            break;
                    }
                    node.Nodes.Add(strTmp);

                    dstAddrLen = rxFrame.Nwk.CtrlWord.DstAddrMode == 0x02 ? 2 : 6;
                    strTmp = "目的地址模式：" + (rxFrame.Nwk.CtrlWord.DstAddrMode == 0x02 ? "2字节" : "6字节");
                    node.Nodes.Add(strTmp);

                    srcAddrLen = rxFrame.Nwk.CtrlWord.SrcAddrMode == 0x02 ? 2 : 6;
                    strTmp = "源地址模式  ：" + (rxFrame.Nwk.CtrlWord.SrcAddrMode == 0x02 ? "2字节" : "6字节");
                    node.Nodes.Add(strTmp);

                    strTmp = "路由信息指示：" + (rxFrame.Nwk.CtrlWord.RouteFlag == true ? "有" : "无");
                    node.Nodes.Add(strTmp);
                }
                nwkNode.Nodes.Add(node);

                strTmp = "目的地址：" + Util.GetStringHexFromBytes(rxFrame.Nwk.DstAddr, 0, dstAddrLen, "", true);
                nwkNode.Nodes.Add(strTmp);
                strTmp = "源地址  ：" + Util.GetStringHexFromBytes(rxFrame.Nwk.SrcAddr, 0, srcAddrLen, "", true);
                nwkNode.Nodes.Add(strTmp);
                strTmp = "网络半径：" + rxFrame.Nwk.Radius.ToString("X");
                nwkNode.Nodes.Add(strTmp);
                strTmp = "帧序号  ：" + rxFrame.Nwk.FrameSn.ToString("X");
                nwkNode.Nodes.Add(strTmp);

                if (true == rxFrame.Nwk.CtrlWord.RouteFlag)
                {
                    node = new TreeNode("路由信息域");

                    strTmp = "中继总数：" + rxFrame.Nwk.Route.RelayCount.ToString();
                    node.Nodes.Add(strTmp);
                    strTmp = "中继索引：" + rxFrame.Nwk.Route.RelayIndex.ToString();
                    node.Nodes.Add(strTmp);
                    strTmp = "地址模式：" + rxFrame.Nwk.Route.AddrMode.ToString("X");
                    node.Nodes.Add(strTmp);
                    // 中继列表  
                    for (int i = 0, index = 0; i < rxFrame.Nwk.Route.RelayCount; i++)
                    {
                        dstAddrLen = (rxFrame.Nwk.Route.AddrMode >> i * 2 & 0x03) == 0x02 ? 2 : 6;
                        strTmp = "中继" + (i + 1).ToString() + "：";
                        strTmp += Util.GetStringHexFromBytes(rxFrame.Nwk.Route.RelayList, index, dstAddrLen, "", true);
                        node.Nodes.Add(strTmp);
                        index += dstAddrLen;
                    }
                    node.Expand();
                    nwkNode.Nodes.Add(node);
                }

                payloadNode.Expand();
                nwkNode.Nodes.Add(payloadNode);

                return nwkNode;
            }

            //入网申请请求
            private static TreeNode ExplainJoinNwkRequest(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Nwk层负载 ：命令帧");

                String strTmp = "";

                if (buf.Length < 2) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf[0]) + "(0x" + buf[0].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                strTmp = "命令选项：0x" + buf[1].ToString("X2");
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }
            //入网申请响应
            private static TreeNode ExplainJoinNwkResponse(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Nwk层负载 ：命令帧");

                TreeNode node = null;
                String strTmp = "";
                int index = 0;
                int relayCount;

                if (buf.Length < 14) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf[index]) + "(0x" + buf[index++].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                strTmp = "命令选项：0x" + buf[index++].ToString("X2");
                payloadNode.Nodes.Add(strTmp);

                strTmp = "PanID   ：" + buf[index + 1].ToString("X2") + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "中心节点地址：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                payloadNode.Nodes.Add(strTmp);
                index += LongAddrSize;

                strTmp = "层次号  ：" + ((buf[index] + (buf[index + 1] << 8)) >> 10);
                payloadNode.Nodes.Add(strTmp);

                strTmp = "时隙号  ：" + ((buf[index] + (buf[index + 1] << 8)) & 0x3FF);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "RSSI    ：" + buf[index++].ToString();
                payloadNode.Nodes.Add(strTmp);

                relayCount = buf[index];
                strTmp = "中继节点数：" + buf[index++].ToString();
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                {
                    // 中继列表
                    if (buf.Length < index + relayCount * 2) return payloadNode;

                    for (int i = 0; i < relayCount; i++)
                    {
                        strTmp = "中继" + (i + 1) + "：" + Util.GetStringHexFromBytes(buf, index, 2, "", true);
                        node.Nodes.Add(strTmp);
                        index += 2;
                    }
                }
                node.Expand();

                return payloadNode;
            }
            //游离节点就绪命令
            private static TreeNode ExplainOffLineNodeReadyCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Nwk层负载 ：命令帧");

                String strTmp = "";

                if (buf.Length < 3) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf[0]) + "(0x" + buf[0].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                strTmp = "层次号：" + ((buf[1] + (buf[2] << 8)) >> 10);
                payloadNode.Nodes.Add(strTmp);

                strTmp = "时隙号：" + ((buf[1] + (buf[2] << 8)) & 0x3FF);
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }
            //路由错误命令
            private static TreeNode ExplainRouteErrorCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Nwk层负载 ：命令帧");

                String strTmp = "";

                if (buf.Length < 2) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf[0]) + "(0x" + buf[0].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                strTmp = "错误代码：" + (buf[1] == 0x01 ? "目标无响应" : "其他错误");
                payloadNode.Nodes.Add(strTmp);

                int addrLen = buf.Length < 8 ? 2 : 6;

                if (buf.Length < 2 + addrLen) return payloadNode;

                strTmp = "失败帧目标地址：" + Util.GetStringHexFromBytes(buf, 2, addrLen, "", true);
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }
            //收集场强命令
            private static TreeNode ExplainGatherRssiRequest(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Nwk层负载 ：命令帧");

                String strTmp = "";

                if (buf.Length < 2) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf[0]) + "(0x" + buf[0].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                strTmp = "页序号  ：" + (buf[1] & 0x0F);
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }
            //收集场强应答
            private static TreeNode ExplainGatherRssiResponse(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Nwk层负载 ：命令帧");

                TreeNode node = null;
                String strTmp = "";
                int index = 0;

                if (buf.Length < index + 1) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf[index]) + "(0x" + buf[index++].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                strTmp = "总页数  ：" + (buf[index] >> 4).ToString();
                payloadNode.Nodes.Add(strTmp);

                strTmp = "页序号  ：" + (buf[index] & 0x0F).ToString();
                payloadNode.Nodes.Add(strTmp);
                index++;

                int neighborCnt = buf[index];
                strTmp = "邻居节点数：" + buf[index++].ToString();
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                {
                    // 邻居场强列表
                    if (buf.Length < index + neighborCnt * (LongAddrSize + 1)) return payloadNode;

                    for (int i = 0; i < neighborCnt; i++)
                    {
                        strTmp = "邻居" + (i + 1) + "：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                        index += LongAddrSize;
                        strTmp += " (" + buf[index++].ToString() + ")";
                        node.Nodes.Add(strTmp);
                    }
                }
                node.Expand();

                return payloadNode;
            }
            //配置子节点命令
            private static TreeNode ExplainConfigSubNodeRequest(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Nwk层负载 ：命令帧");

                TreeNode node = null;
                String strTmp = "";
                int index = 0;
                byte cmdOption;

                if (buf.Length < 9) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf[index]) + "(0x" + buf[index++].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                strTmp = "命令选项：0x" + buf[index].ToString("X2");
                {
                    node = new TreeNode(strTmp);
                    cmdOption = buf[index++];

                    strTmp = "时隙号  ：" + ((cmdOption & 0x01) > 0 ? "有" : "无");
                    node.Nodes.Add(strTmp);
                    strTmp = "层次号  ：" + ((cmdOption & 0x02) > 0 ? "有" : "无");
                    node.Nodes.Add(strTmp);
                    strTmp = "信道组号：" + ((cmdOption & 0x04) > 0 ? "有" : "无");
                    node.Nodes.Add(strTmp);
                    strTmp = "短地址  ：" + ((cmdOption & 0x08) > 0 ? "有" : "无");
                    node.Nodes.Add(strTmp);
                    strTmp = "PanID   ：" + ((cmdOption & 0x10) > 0 ? "有" : "无");
                    node.Nodes.Add(strTmp);
                    strTmp = "中继列表：" + ((cmdOption & 0x20) > 0 ? "有" : "无");
                    node.Nodes.Add(strTmp);
                    strTmp = "在网标识：" + ((cmdOption & 0x80) > 0 ? "离网" : "在网");
                    node.Nodes.Add(strTmp);
                }
                payloadNode.Nodes.Add(node);

                if ((cmdOption & 0x01) > 0)
                {
                    strTmp = "信道组：" + buf[index++].ToString();
                    payloadNode.Nodes.Add(strTmp);
                }

                strTmp = "层次号：" + ((buf[index] + (buf[index + 1] << 8)) >> 10);
                payloadNode.Nodes.Add(strTmp);
                strTmp = "时隙号：" + ((buf[index] + (buf[index + 1] << 8)) & 0x3FF);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                if ((cmdOption & 0x08) > 0)
                {
                    strTmp = "短地址：" + buf[index + 1].ToString("X2") + buf[index].ToString("X2");
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;
                }

                if ((cmdOption & 0x10) > 0)
                {
                    strTmp = "PanID ：" + buf[index + 1].ToString("X2") + buf[index].ToString("X2");
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;
                }

                if ((cmdOption & 0x20) > 0)
                {
                    if (buf.Length < index + 1) return payloadNode;

                    strTmp = "路径数：" + buf[index].ToString();
                    payloadNode.Nodes.Add(strTmp);
                    // 路径列表
                    int routeCount = buf[index++];
                    int relayCount;
                    for (int i = 0; i < routeCount; i++)
                    {
                        if (buf.Length < index + 1) return payloadNode;

                        relayCount = buf[index++];
                        strTmp = "路径" + (i + 1) + "中继数：" + relayCount;
                        node = new TreeNode(strTmp);
                        payloadNode.Nodes.Add(node);

                        if (buf.Length < index + relayCount * 2) return payloadNode;

                        for (int j = 0; j < relayCount; j++)
                        {
                            strTmp = "中继" + (j + 1) + "：";
                            strTmp += Util.GetStringHexFromBytes(buf, index, 2, "", true);
                            node.Nodes.Add(strTmp);
                            index += 2;
                        }
                    }
                }

                return payloadNode;
            }
            //配置子节点应答
            private static TreeNode ExplainConfigSubNodeResponse(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Nwk层负载 ：命令帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < index + 7) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf[index]) + "(0x" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "命令选项：0x" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "硬件版本：" + buf[index + 1].ToString("X") + "." + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "软件版本：" + buf[index + 1].ToString("X") + "." + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "Boot版本：" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                return payloadNode;
            }

            //收集水表上报数据
            private static TreeNode ExplainGatherWaterAmeterReportData(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Nwk层负载 ：命令帧");

                String strTmp = "";

                if (buf.Length < 2) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf[0]) + "(0x" + buf[0].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                strTmp = "页序号  ：" + (buf[1] & 0x0F);
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }
            //收集水表上报数据应答
            private static TreeNode ExplainGatherWaterAmeterReportDataResponse(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Nwk层负载 ：命令帧");

                String strTmp = "";
                int index = 0;
                TreeNode node = null;

                if (buf.Length < 3) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf[index]) + "(0x" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index++;

                strTmp = "总页数  ：" + (buf[index] >> 4);
                payloadNode.Nodes.Add(strTmp);
                strTmp = "页序号  ：" + (buf[index] & 0x0F);
                payloadNode.Nodes.Add(strTmp);
                index++;

                int nodeCnt = buf[index];
                strTmp = "水表数量：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index++;

                if (buf.Length < index + nodeCnt * 8) return payloadNode;

                for (int i = 0; i < nodeCnt; i++)
                {
                    strTmp = "水表" + (i + 1) + "：" + Util.GetStringHexFromBytes(buf, index + 1, 7, "", true)
                            + " (" + buf[index] + ")";
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                    index += 8;
                    {
                        strTmp = "上报时间：" + DateTime.Now.Year / 100
                                 + buf[index + 5].ToString("X2") + "-"
                                 + buf[index + 4].ToString("X2") + "-"
                                 + buf[index + 3].ToString("X2") + " "
                                 + buf[index + 2].ToString("X2") + ":"
                                 + buf[index + 1].ToString("X2") + ":"
                                 + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 6;
                        strTmp = "表分类  ：" + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 1;
                        strTmp = "累计用量："
                                + buf[index + 3].ToString("X2")
                                + buf[index + 2].ToString("X2")
                                + buf[index + 1].ToString("X2") + "."
                                + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 4;
                        strTmp = "统计日期：" + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 1;
                        strTmp = "上月累计："
                                + buf[index + 3].ToString("X2")
                                + buf[index + 2].ToString("X2")
                                + buf[index + 1].ToString("X2") + "."
                                + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 4;
                        strTmp = "欠压状态：" + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 1;
                        strTmp = "出厂年份：" + DateTime.Now.Year / 100 + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 1;
                    }

                }
                return payloadNode;
            }
            //水气表场强收集
            private static TreeNode ExplainGatherWaterGasAmeterRssi(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Nwk层负载 ：命令帧");

                String strTmp = "";

                if (buf.Length < 2) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf[0]) + "(0x" + buf[0].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                strTmp = "页序号  ：" + (buf[1] & 0x0F);
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }
            //水气表场强收集应答
            private static TreeNode ExplainGatherWaterGasAmeterRssiResponse(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Nwk层负载 ：命令帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < 3) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf[index]) + "(0x" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index++;

                strTmp = "总页数  ：" + (buf[index] >> 4);
                payloadNode.Nodes.Add(strTmp);
                strTmp = "页序号  ：" + (buf[index] & 0x0F);
                payloadNode.Nodes.Add(strTmp);
                index++;

                int nodeCnt = buf[index];
                strTmp = "节点个数：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index++;

                if (buf.Length < index + nodeCnt * 8) return payloadNode;

                for (int i = 0; i < nodeCnt; i++)
                {
                    strTmp = "节点" + (i + 1) + "：" + Util.GetStringHexFromBytes(buf, index, 7, "", true)
                            + " (" + buf[index + 7] + ")";
                    payloadNode.Nodes.Add(strTmp);
                    index += 8;
                }

                return payloadNode;
            }
        }
        #endregion

        #region Aps层解析

        public static readonly String[] ApsFrameTypeTbl = new String[]
        {
            "确认/否认帧",
            "命令帧",
            "数据转发帧",
            "上报帧",
        };
        private static readonly String[] ProtolTbl = new String[]
        {
            "双向水表",
            "645-97电表",
            "645-07电表",
            "单向水表",
            "保留",
            "燃气表",
            "热表",
            "698电表"
        };
        private static readonly String[] BaudRateTbl = new String[]
        {
            "自适应",
            "1200",
            "2400",
            "4800",
            "9600",
            "19200"
        };
        private static readonly String[] ParityTbl = new String[]
        {
            "无校验",
            "奇校验",
            "偶校验"
        };
        private static readonly String[] RfSendPowerTbl = new String[]
        {
            "最高（17dBm）",
            "次高（10dBm）",
            "次低（4dBm）",
            "最低（-2dBm）",
            "全功率（20dBm）"
        };
        private static readonly String[] DeviceTypeTbl = new String[]
        {
            "中心节点",
            "I型采集器节点",
            "II型采集器节点",
            "电表节点"
        };

        private static readonly string[] NodeStatusTbl = new string[]
        {
            // 节点组网状态
            "等待组网",    
            "组网完成",    
            "信标阶段",    
            "点名阶段",    
            "配置阶段",    
            "网络维护阶段",    
            "未知阶段"   
        };

        public class ApsExplain             //Aps层解析
        {
            //命令帧
            private static readonly CmdExplain[] ApsCmdTbl = new CmdExplain[]
            {
                // 国网标准命令
                new CmdExplain( 0x00, "串口配置",       Color.Brown, new ExplainCallback(ExplainSerialComConfigCmd)),
                new CmdExplain( 0x01, "设置信道组",     Color.Brown, new ExplainCallback(ExplainSetChanelGrpCmd)),
                new CmdExplain( 0x02, "设置RSSI门限",   Color.Brown, new ExplainCallback(ExplainSetRssiThresholdCmd)),
                new CmdExplain( 0x03, "设置发射功率",   Color.Brown, new ExplainCallback(ExplainSetSendPowerCmd)),
                new CmdExplain( 0x04, "读取配置",       Color.Brown, new ExplainCallback(ExplainReadConfigCmd), 9),
                new CmdExplain( 0x05, "设备重启命令",   Color.Brown, new ExplainCallback(ExplainDeviceRebootCmd)),
                new CmdExplain( 0x06, "软件升级",       Color.Brown, new ExplainCallback(ExplainSoftwareUpgradeCmd)),
                new CmdExplain( 0x07, "广播校时",       Color.Brown, new ExplainCallback(ExplainBroadcastTimeCmd)),

                // 北网扩展命令
                new CmdExplain( 0x0D, "透传数据",           Color.Brown, new ExplainCallback(ExplainPassthroughData)),
                new CmdExplain( 0x10, "转发报警箱下行",      Color.Brown, new ExplainCallback(ExplainTransferAlarmBoxCmd)),
                new CmdExplain( 0x11, "转发报警箱应答",      Color.Brown, new ExplainCallback(ExplainTransferAlarmBoxResponse)),
                new CmdExplain( 0x12, "转发报警箱上报",      Color.Brown, new ExplainCallback(ExplainTransferAlarmBoxReport)),
                new CmdExplain( 0x13, "转发报警箱上报应答",  Color.Brown, new ExplainCallback(ExplainTransferAlarmBoxReportResponse)),

                // 手持机命令
                new CmdExplain( 0x88, "读取全部档案",           Color.Brown, new ExplainCallback(ExplainReadDocumentCmd), 11),
                new CmdExplain( 0x89, "读取全部入网档案",       Color.Brown, new ExplainCallback(ExplainReadDocumentCmd), 11),
                new CmdExplain( 0x8A, "读取全部离网档案",       Color.Brown, new ExplainCallback(ExplainReadDocumentCmd), 11),
                new CmdExplain( 0x8B, "读取升级成功档案",       Color.Brown, new ExplainCallback(ExplainReadDocumentCmd), 11),
                new CmdExplain( 0x8C, "读取升级失败档案",       Color.Brown, new ExplainCallback(ExplainReadDocumentCmd), 11),
                new CmdExplain( 0x9B, "读取全部问题档案",       Color.Brown, new ExplainCallback(ExplainReadDocumentCmd), 11),

                new CmdExplain( 0x90, "参数区初始化",         Color.Brown, new ExplainCallback(ExplainParamsInitCmd)),
                new CmdExplain( 0x91, "启动组网",             Color.Brown, new ExplainCallback(ExplainStartBuildNetworkCmd)),
                new CmdExplain( 0x92, "读取从节点路由",       Color.Brown, new ExplainCallback(ExplainReadNodeRouteCmd), 8),
                new CmdExplain( 0x93, "读取网络状态",         Color.Brown, new ExplainCallback(ExplainReadNetworkStateCmd), 7),
                new CmdExplain( 0x94, "读取发射功率",         Color.Brown, new ExplainCallback(ExplainReadSendPowerCmd), 2),
                new CmdExplain( 0x95, "读取软件版本",         Color.Brown, new ExplainCallback(ExplainReadSoftwareVerCmd), 4),
                new CmdExplain( 0x96, "更改二采地址",         Color.Brown, new ExplainCallback(ExplainModifyCollect2AddrCmd)),
                new CmdExplain( 0x97, "广播清邻居表",         Color.Brown, new ExplainCallback(ExplainBroadcastClearNeighborCmd), 3),
                new CmdExplain( 0x98, "广播设置发射功率",     Color.Brown, new ExplainCallback(ExplainBroadcastSetSendPowerCmd), 3),
                new CmdExplain( 0x99, "强制入网",             Color.Brown, new ExplainCallback(ExplainForceJoinNetworkCmd)),
                new CmdExplain( 0x9A, "启动网络维护",         Color.Brown, new ExplainCallback(ExplainStartNetworkMaintainCmd)),

                new CmdExplain( 0x9C, "模拟集中器抄表-停止",     Color.Brown, new ExplainCallback(ExplainSimulateConcReadAmeterStop)),
                new CmdExplain( 0x9D, "模拟集中器抄表-启动",     Color.Brown, new ExplainCallback(ExplainSimulateConcReadAmeterStart)),

                new CmdExplain( 0x9E, "广播复位节点",         Color.Brown, new ExplainCallback(ExplainBroadcastResetCmd)),
                new CmdExplain( 0xA0, "读取节点邻居表",       Color.Brown, new ExplainCallback(ExplainReadNodeNeighborCmd), 3),
            };

            public static String GetCmdName(byte[] buf)
            {
                string strName = "无法识别的命令";

                foreach (CmdExplain cmd in ApsCmdTbl)
                {
                    if (cmd.CmdId == buf[0])
                    {
                        strName = cmd.CmdName;

                        if (strName == "透传数据" && buf.Length > 20 && buf[1] == 0x47 && buf[2] == 0xCD)
                        {
                            strName += "-" + MacExplain.GetLowPowerMeterCmdName(buf[20]);
                        }
                        else
                        {
                            if (cmd.PayloadMinLen != 0 && buf.Length >= cmd.PayloadMinLen)
                            {
                                strName += "-应答";
                            }
                        }
                    }
                }

                return strName;
            }
            public static Color GetCmdColor(byte cmdId)
            {
                Color cmdColor = Color.Black;

                foreach (CmdExplain cmd in ApsCmdTbl)
                {
                    if (cmd.CmdId == cmdId)
                    {
                        cmdColor = cmd.CmdColor;
                    }
                }
                return cmdColor;
            }
            private static TreeNode ExplainCmdFrame(byte[] buf)
            {
                ExplainCallback callback = null;
                TreeNode payloadNode = null;

                foreach (CmdExplain cmd in ApsCmdTbl)
                {
                    if (cmd.CmdId == buf[0])
                    {
                        callback = cmd.CmdExplainFunc;
                    }
                }

                if (callback != null)
                {
                    payloadNode = callback(buf);
                }
                else
                {
                    payloadNode = new TreeNode("Aps层负载 ：无法识别");
                }

                return payloadNode;
            }

            public static TreeNode GetTreeNode(FrameFormat rxFrame, Color fgColor)
            {
                TreeNode apsNode = new TreeNode("Aps层");
                apsNode.ForeColor = fgColor;

                TreeNode node = null;
                TreeNode payloadNode = null;
                String strTmp = "";

                if (rxFrame.Aps.CtrlWord.FrameType == ApsFrameType.Invalid)
                {
                    apsNode.Nodes.Add(new TreeNode("无法识别的APS帧"));
                    return apsNode;
                }

                node = new TreeNode("帧控制域：0x" + rxFrame.Aps.CtrlWord.All.ToString("X2"));
                {
                    switch (rxFrame.Aps.CtrlWord.FrameType)
                    {
                        case ApsFrameType.Ack:
                            strTmp = "帧类型    ：确认/否认帧(" + rxFrame.Aps.CtrlWord.FrameType.ToString("D") + ")";
                            payloadNode = ExplainAckFrame(rxFrame.Aps.Payload);
                            break;
                        case ApsFrameType.Cmd:
                            strTmp = "帧类型    ：命令帧(" + rxFrame.Aps.CtrlWord.FrameType.ToString("D") + ")";
                            payloadNode = ExplainCmdFrame(rxFrame.Aps.Payload);
                            break;
                        case ApsFrameType.DataTransfer:
                            strTmp = "帧类型    ：数据转发帧(" + rxFrame.Aps.CtrlWord.FrameType.ToString("D") + ")";
                            payloadNode = ExplainDataTransferFrame(rxFrame.Aps.Payload);
                            break;
                        case ApsFrameType.Report:
                            strTmp = "帧类型    ：上报帧(" + rxFrame.Aps.CtrlWord.FrameType.ToString("D") + ")";
                            payloadNode = ExplainReportFrame(rxFrame.Aps.Payload);
                            break;
                        default:
                            payloadNode = new TreeNode("Aps层负载 ：无法识别");
                            break;
                    }
                    node.Nodes.Add(strTmp);

                    strTmp = "扩展域标识：";
                    strTmp += rxFrame.Aps.CtrlWord.ExtendFlag == true ? "有" : "无";
                    node.Nodes.Add(strTmp);
                }
                apsNode.Nodes.Add(node);

                node = new TreeNode("帧序号  ：" + rxFrame.Aps.FrameSn.ToString("X2"));
                apsNode.Nodes.Add(node);

                if (true == rxFrame.Aps.CtrlWord.ExtendFlag)
                {
                    node = new TreeNode("扩展域  ：");

                    strTmp = "扩展域长度：" + rxFrame.Aps.Extend.Length.ToString();
                    node.Nodes.Add(strTmp);

                    if (rxFrame.Aps.Extend.Length >= 2)
                    {
                        strTmp = "厂家标识  ：" + rxFrame.Aps.Extend.OemInfo.ToString("X4");
                        string strOem = "(" + Convert.ToChar(rxFrame.Aps.Extend.OemInfo >> 8)
                                    + Convert.ToChar(rxFrame.Aps.Extend.OemInfo & 0x00FF) + ")";
                        node.Nodes.Add(strTmp + " " + strOem);
                        node.Text = node.Text + "厂家" + strOem;

                        strTmp = "扩展域数据：" + "(" + (rxFrame.Aps.Extend.Length - 2) + "byte)";
                        node.Nodes.Add(strTmp);
                    }
                    node.Expand();
                    apsNode.Nodes.Add(node);
                }

                payloadNode.Expand();
                apsNode.Nodes.Add(payloadNode);

                return apsNode;
            }

            #region 确认/否认帧、数据转发帧、数据上报帧

            // 确认/否认帧
            private static TreeNode ExplainAckFrame(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载：确认/否认帧");

                String strTmp = "";

                if (buf.Length < 1) return payloadNode;

                strTmp = "确认/否认标识：" + (buf[0] == 0x00 ? "否认" : "确认");
                payloadNode.Nodes.Add(strTmp);

                if (buf.Length < 2) return payloadNode;

                strTmp = "数据单元：" + buf[1];
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }
            // 数据转发帧
            private static TreeNode ExplainDataTransferFrame(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载：数据转发帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < 13) return payloadNode;

                strTmp = "转发标识  ：" + "波特率-" + (buf[index] < BaudRateTbl.Length ? BaudRateTbl[buf[index]] : "");
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                string strDataType = "";
                for (; index + 7 < buf.Length; index++)
                {
                    if (buf[index] == 0x68 && buf[index + 7] == 0x68)
                    {
                        strDataType = "645-07报文";
                        break;
                    }
                }

                if (strDataType == "645-07报文")
                {
                    // 解析645-07报文
                    byte[] data_645 = new byte[buf.Length - index];
                    Array.Copy(buf, index, data_645, 0, data_645.Length);
                    TreeNode node = ProtoDLT645_07.GetProtoTree(data_645);
                    if (node != null)
                    {
                        node.Expand();
                        payloadNode.Nodes.Add(node);
                    }
                }

                return payloadNode;
            }
            // 上报帧
            private static TreeNode ExplainReportFrame(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载：上报帧");

                String strTmp = "";

                if (buf.Length < 2) return payloadNode;

                strTmp = "上报标识：" + (buf[0] == 0x00 ? "事件上报" : "未知");
                payloadNode.Nodes.Add(strTmp);

                if (buf[0] == 0x00)
                {
                    strTmp = "事件上报类型：" + (buf[1] == 0x00 ? "电能表事件" : "从节点事件");
                    payloadNode.Nodes.Add(strTmp);
                }

                if (buf.Length < 4) return payloadNode;

                strTmp = "事件序号：" + buf[2];
                payloadNode.Nodes.Add(strTmp);

                strTmp = "事件数据长度：" + buf[3] + " byte";
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }
            #endregion

            #region 命令帧 -- 国网标准命令

            // 串口配置
            private static TreeNode ExplainSerialComConfigCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";

                if (buf.Length < 3) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[0].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                strTmp = "波特率  ：" + (buf[1] < BaudRateTbl.Length ? BaudRateTbl[buf[1]] : "未知");
                payloadNode.Nodes.Add(strTmp);

                strTmp = "校验方式：" + (buf[2] < ParityTbl.Length ? ParityTbl[buf[2]] : "未知");
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }
            // 设置信道组
            private static TreeNode ExplainSetChanelGrpCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";

                if (buf.Length < 2) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[0].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                strTmp = "信道组号：" + buf[1];
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }
            // 设置RSSI门限
            private static TreeNode ExplainSetRssiThresholdCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";

                if (buf.Length < 2) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[0].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                strTmp = "场强门限：" + buf[1];
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }
            // 设置发射功率
            private static TreeNode ExplainSetSendPowerCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";

                if (buf.Length < 2) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[0].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                strTmp = "发射功率：" + (buf[1] < RfSendPowerTbl.Length ? RfSendPowerTbl[buf[1]] : "未知");
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }

            //读取配置
            private static TreeNode ExplainReadConfigCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                TreeNode node = null;
                String strTmp = "";
                int index = 0;

                //读取配置--请求
                if (buf.Length < 9)
                {
                    if (buf.Length < 1) return payloadNode;

                    strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                    payloadNode.Nodes.Add(strTmp);
                    return payloadNode;
                }

                //读取配置--应答
                if (buf.Length < index + 27) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "设备出厂地址：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                payloadNode.Nodes.Add(strTmp);
                index += 6;
                strTmp = "节点类型：" + (buf[index] < DeviceTypeTbl.Length ? DeviceTypeTbl[buf[index]] : "未知");
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "PanID   ：" + buf[index + 1].ToString("X2") + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "短地址  ：" + buf[index + 1].ToString("X2") + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "厂家标识：" + Convert.ToChar(buf[index]) + Convert.ToChar(buf[index + 1]);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "硬件版本：" + buf[index + 1].ToString("X2") + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "软件版本：" + buf[index + 2].ToString("X2") + "." + buf[index + 1].ToString("X2") + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 3;

                strTmp = "发射功率：" + (buf[index] < RfSendPowerTbl.Length ? RfSendPowerTbl[buf[index]] : "未知");
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "场强门限：" + buf[index].ToString();
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "信道组  ：" + buf[index].ToString();
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "层次号  ：" + ((buf[index] + (buf[index + 1] << 8)) >> 10);
                payloadNode.Nodes.Add(strTmp);
                strTmp = "时隙号  ：" + ((buf[index] + (buf[index + 1] << 8)) & 0x3FF);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "网络规模：" + (buf[index] + buf[index] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                // 路径列表
                int routeCount = buf[index];
                strTmp = "路径数  ：" + buf[index].ToString();
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                int relayCount;
                for (int i = 0; i < routeCount; i++)
                {
                    if (buf.Length < index + 1) return payloadNode;

                    relayCount = buf[index++];
                    strTmp = "路径" + (i + 1) + "中继数：" + relayCount;
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);

                    if (buf.Length < index + relayCount * 2) return payloadNode;

                    for (int j = 0; j < relayCount; j++)
                    {
                        strTmp = "中继" + (j + 1) + "：" + Util.GetStringHexFromBytes(buf, index, 2, "", true);
                        node.Nodes.Add(strTmp);
                        index += 2;
                    }

                }

                return payloadNode;
            }

            //设备重启
            private static TreeNode ExplainDeviceRebootCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";

                if (buf.Length < 1) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[0].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }
            // 软件升级
            private static TreeNode ExplainSoftwareUpgradeCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < 8) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                strTmp = "厂商标识：" + buf[index].ToString("X2") + buf[index + 1].ToString("X2")
                                        + "(" + Convert.ToChar(buf[index]) + Convert.ToChar(buf[index + 1]) + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "设备类型：" + DeviceTypeTbl[buf[index++]];
                payloadNode.Nodes.Add(strTmp);

                strTmp = "总包数  ：" + (buf[index] + (buf[index + 1] << 8));
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "当前包序号：" + (buf[index] + (buf[index + 1] << 8));
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "当前包数据：" + (buf.Length - index) + " byte";
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }
            // 广播校时
            private static TreeNode ExplainBroadcastTimeCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < 9) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                strTmp = "广播帧序号：" + buf[index++];
                payloadNode.Nodes.Add(strTmp);

                strTmp = "层次号  ：" + ((buf[index] + (buf[index + 1] << 8)) >> 10);
                payloadNode.Nodes.Add(strTmp);

                strTmp = "时隙号  ：" + ((buf[index] + (buf[index + 1] << 8)) & 0x3FF);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "全网最大延时：" + (buf[index] + (buf[index + 1] << 8) + (buf[index + 2] << 16) + (buf[index + 3] << 24)) + " ms";
                payloadNode.Nodes.Add(strTmp);
                index += 4;

                // 解析645-07报文
                byte[] data_645 = new byte[buf.Length - index];
                Array.Copy(buf, index, data_645, 0, data_645.Length);
                TreeNode node = ProtoDLT645_07.GetProtoTree(data_645);
                if (node != null)
                {
                    node.Expand();
                    payloadNode.Nodes.Add(node);
                }

                return payloadNode;
            }
            #endregion

            #region 命令帧 -- 北网扩展命令
            // 转发报警箱下行
            private static TreeNode ExplainTransferAlarmBoxCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < 3) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                strTmp = "数据长度：" + buf[index++];
                payloadNode.Nodes.Add(strTmp);

                strTmp = "数据内容：" + "（ 略 ）";
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }
            // 转发报警箱应答
            private static TreeNode ExplainTransferAlarmBoxResponse(byte[] buf)
            {
                return ExplainTransferAlarmBoxCmd(buf);     // 应答与请求的格式相同
            }
            // 转发报警箱上报
            private static TreeNode ExplainTransferAlarmBoxReport(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < 4) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                strTmp = "事件序号：" + buf[index++];
                payloadNode.Nodes.Add(strTmp);

                strTmp = "数据长度：" + buf[index++];
                payloadNode.Nodes.Add(strTmp);

                strTmp = "数据内容：" + "（ 略 ）";
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }
            // 转发报警箱上报应答
            private static TreeNode ExplainTransferAlarmBoxReportResponse(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < 2) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                strTmp = "事件序号：" + buf[index++];
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }
            //透传数据
            private static TreeNode ExplainPassthroughData(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < 1) return payloadNode;

                strTmp = "命令标识：" + "透传数据" + "(0x" + buf[index++].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                // 解析透传数据
                byte[] pkt = new byte[buf.Length - 1 + 6];
                Array.Copy(buf, 1, pkt, 4, buf.Length - 1);     // 构造帧，phy层头部4字节 + 尾部2字节 忽略
                FrameFormat frame = ExplainRxPacket(pkt);
                TreeNode node = MacExplain.GetTreeNode(frame, Color.Blue);
                node.Expand();
                payloadNode.Nodes.Add(node);

                return payloadNode;
            }
            #endregion

            #region 命令帧 -- 扩展的手持机指令

            //读取档案（全部档案 / 全部入网档案 / 全部离网档案 / 全部问题档案）
            private static TreeNode ExplainReadDocumentCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                TreeNode node = null;
                String strTmp = "";
                int index = 0;

                if (buf.Length < 4) return payloadNode;

                //读取档案--请求
                if (buf.Length < 11 && buf[3] != 0)
                {
                    strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                    payloadNode.Nodes.Add(strTmp);

                    strTmp = "起始序号：" + (buf[index] + (buf[index + 1] << 8)).ToString();
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;

                    strTmp = "读取数量：" + buf[index++].ToString();
                    payloadNode.Nodes.Add(strTmp);

                    return payloadNode;
                }

                //读取档案--响应

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                strTmp = "起始序号：" + (buf[index] + (buf[index + 1] << 8)).ToString();
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "应答数量：" + buf[index].ToString();
                int nodeCnt = buf[index++];
                node = new TreeNode(strTmp);
                for (int i = 0; i < nodeCnt; i++)
                {
                    if (buf.Length < index + LongAddrSize + 1) break;

                    strTmp = "地址" + (i + 1) + "：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                    index += LongAddrSize;

                    if ((buf[index] & 0x10) > 0)
                    {
                        strTmp += " " + (buf[index] & 0x0F) + "级";
                    }
                    else
                    {
                        strTmp += " 离网";
                    }
                    index++;
                    node.Nodes.Add(strTmp);
                }
                node.Expand();
                payloadNode.Nodes.Add(node);

                return payloadNode;
            }

            //参数初始化
            private static TreeNode ExplainParamsInitCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";

                if (buf.Length < 1) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[0].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }

            //启动组网
            private static TreeNode ExplainStartBuildNetworkCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";

                if (buf.Length < 1) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[0].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }

            //读取从节点路由
            private static TreeNode ExplainReadNodeRouteCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";
                TreeNode node = null;
                int index = 0;

                //读取从节点路由--请求
                if (buf.Length == 7)
                {
                    strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                    payloadNode.Nodes.Add(strTmp);

                    strTmp = "从节点地址：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                    payloadNode.Nodes.Add(strTmp);
                    index += LongAddrSize;

                    return payloadNode;
                }

                //读取从节点路由--响应
                if (buf.Length < 2) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                strTmp = "中继个数：" + buf[index].ToString();
                int relayCnt = buf[index++];
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                {
                    if (buf.Length < index + relayCnt * LongAddrSize) return payloadNode;

                    // 中继列表
                    for (int i = 0; i < relayCnt; i++)
                    {
                        strTmp = "中继" + (i + 1) + "" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                        index += LongAddrSize;
                        node.Nodes.Add(strTmp);
                    }
                }

                return payloadNode;
            }

            //读取网络状态
            private static TreeNode ExplainReadNetworkStateCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";
                int index = 0;

                //读取网络状态--请求
                if (buf.Length == 1)
                {
                    strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                    payloadNode.Nodes.Add(strTmp);

                    return payloadNode;
                }

                //读取网络状态--响应

                if (buf.Length == 7)
                {
                    //子节点状态--响应  （测试代码，后期需变更）
                    strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                    payloadNode.Nodes.Add(strTmp);

                    strTmp = "节点状态：" + (buf[index++] == 0x01 ? "在网" : "离网");
                    payloadNode.Nodes.Add(strTmp);

                    strTmp = "保留字节：" + buf[index++]; // 跳过

                    int cnt, ngCnt;
                    cnt = ((buf[index + 1] << 8) + buf[index]);
                    strTmp = "接收帧总数：" + cnt;
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;

                    ngCnt = ((buf[index + 1] << 8) + buf[index]);
                    strTmp = "CRC错误总数：" + ngCnt;
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;

                    strTmp = "接收正确率：" + ((float)(cnt - ngCnt) * 100 / cnt).ToString("F2") + "%";
                    payloadNode.Nodes.Add(strTmp);

                }
                else
                {
                    //主节点状态--响应
                    if (buf.Length < 9) return payloadNode;

                    strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index].ToString("X2") + ")";
                    payloadNode.Nodes.Add(strTmp);
                    index++;
                    strTmp = "组网状态：" + (buf[index] < NodeStatusTbl.Length ? NodeStatusTbl[buf[index]] : "未知");
                    payloadNode.Nodes.Add(strTmp);
                    index++;
                    strTmp = "组网时间：" + buf[index] + "分钟";
                    payloadNode.Nodes.Add(strTmp);
                    index++;
                    strTmp = "下载总数：" + ((buf[index + 1] << 8) + buf[index]);
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;
                    strTmp = "在网数量：" + ((buf[index + 1] << 8) + buf[index]);
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;
                    strTmp = "离网数量：" + ((buf[index + 1] << 8) + buf[index]);
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;
                }

                return payloadNode;
            }

            //读取发射功率
            private static TreeNode ExplainReadSendPowerCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";
                int index = 0;

                //读取发射功率--请求
                if (buf.Length == 1)
                {
                    strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                    payloadNode.Nodes.Add(strTmp);

                    return payloadNode;
                }

                //读取发射功率--响应
                if (buf.Length < 2) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                strTmp = "发射功率：" + buf[index] + " dBm";
                payloadNode.Nodes.Add(strTmp);
                index++;

                return payloadNode;
            }

            //读取软件版本
            private static TreeNode ExplainReadSoftwareVerCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";
                int index = 0;

                //读取软件版本--请求
                if (buf.Length == 1)
                {
                    strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                    payloadNode.Nodes.Add(strTmp);

                    return payloadNode;
                }

                //读取软件版本--响应
                if (buf.Length < 2) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                strTmp = "版本长度：" + buf[index];
                int verLen = buf[index++];
                payloadNode.Nodes.Add(strTmp);

                if (buf.Length < index + verLen) return payloadNode;

                strTmp = "版本信息：" + Encoding.ASCII.GetString(buf, index, verLen);
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }

            //更改二采地址
            private static TreeNode ExplainModifyCollect2AddrCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < 7) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                strTmp = "二采地址：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }

            //广播清邻居表
            private static TreeNode ExplainBroadcastClearNeighborCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";
                int index = 0;

                //--请求
                if (buf.Length == 1)
                {
                    strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                    payloadNode.Nodes.Add(strTmp);

                    return payloadNode;
                }

                //--应答
                if (buf.Length < 3) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                strTmp = "子节点个数：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                return payloadNode;
            }

            //广播设置发射功率
            private static TreeNode ExplainBroadcastSetSendPowerCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < 3)
                {
                    if (buf.Length < 2) return payloadNode;

                    strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                    payloadNode.Nodes.Add(strTmp);

                    strTmp = "发射功率：" + (buf[index] < RfSendPowerTbl.Length ? RfSendPowerTbl[buf[index]] : "未知");
                    payloadNode.Nodes.Add(strTmp);
                    index++;
                    return payloadNode;
                }

                if (buf.Length < 3) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                strTmp = "子节点个数：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                return payloadNode;
            }

            //强制入网
            private static TreeNode ExplainForceJoinNetworkCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < 2) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                strTmp = "入网信道组：" + buf[index++].ToString();
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }

            //启动网络维护
            private static TreeNode ExplainStartNetworkMaintainCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";

                if (buf.Length < 1) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[0].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }

            //广播复位节点
            private static TreeNode ExplainBroadcastResetCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < 8) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                strTmp = "广播帧序号：" + buf[index++];
                payloadNode.Nodes.Add(strTmp);
                strTmp = "层次号  ：" + ((buf[index] + (buf[index + 1] << 8)) >> 10);
                payloadNode.Nodes.Add(strTmp);
                strTmp = "时隙号  ：" + ((buf[index] + (buf[index + 1] << 8)) & 0x3FF);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "全网最大延时：" + BitConverter.ToUInt32(buf, index) + " ms";
                payloadNode.Nodes.Add(strTmp);
                index += 4;

                return payloadNode;
            }

            //读取节点邻居表
            private static TreeNode ExplainReadNodeNeighborCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";
                TreeNode node = null;
                int index = 0;

                //读取节点邻居表--请求
                if (buf.Length == 2)
                {
                    strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                    payloadNode.Nodes.Add(strTmp);

                    strTmp = "页序号  ：" + (buf[index] & 0x0F);
                    payloadNode.Nodes.Add(strTmp);

                    return payloadNode;
                }

                //读取节点邻居表--响应
                if (buf.Length < 3) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                strTmp = "总页数：" + (buf[index] >> 4);
                payloadNode.Nodes.Add(strTmp);
                strTmp = "页序号  ：" + (buf[index] & 0x0F);
                payloadNode.Nodes.Add(strTmp);
                index++;

                strTmp = "邻居节点数：" + (buf[index]);
                int neighborCnt = buf[index++];
                node = new TreeNode(strTmp);
                // 邻居场强列表
                for (int i = 0; i < neighborCnt; i++)
                {
                    if (buf.Length < index + LongAddrSize + 2) break;

                    strTmp = "邻居" + (i + 1) + "：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                    index += LongAddrSize;
                    strTmp += " (" + buf[index] + "|" + buf[index + 1] + "h)";
                    index += 2;

                    node.Nodes.Add(strTmp);
                }
                node.Expand();
                payloadNode.Nodes.Add(node);

                return payloadNode;
            }

            //模拟集中器抄表-停止
            private static TreeNode ExplainSimulateConcReadAmeterStop(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < 1) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);

                return payloadNode;
            }
            //模拟集中器抄表-启动
            private static TreeNode ExplainSimulateConcReadAmeterStart(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("Aps层负载 ：命令帧");

                String strTmp = "";
                int index = 0;

                if (buf.Length < 8) return payloadNode;

                strTmp = "命令标识：" + GetCmdName(buf) + "(0x" + buf[index++].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                strTmp = "节点地址：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                payloadNode.Nodes.Add(strTmp);
                index += 6;

                int dataLen = buf[index];
                strTmp = "报文长度：" + buf[index++];
                payloadNode.Nodes.Add(strTmp);

                if (buf.Length < index + dataLen) return payloadNode;

                string strDataType = "";
                for (; index + 7 < buf.Length; index++)
                {
                    if (buf[index] == 0x68 && buf[index + 7] == 0x68)
                    {
                        strDataType = "645-07报文";
                        break;
                    }
                }

                if (strDataType == "645-07报文")
                {
                    // 解析645-07报文
                    byte[] data_645 = new byte[buf.Length - index];
                    Array.Copy(buf, index, data_645, 0, data_645.Length);
                    TreeNode node = ProtoDLT645_07.GetProtoTree(data_645);
                    if (node != null)
                    {
                        node.Expand();
                        payloadNode.Nodes.Add(node);
                    }
                }

                return payloadNode;
            }

            #endregion

        }
        #endregion


        #region CRC16计算
        public static UInt16 GenCRC16(byte[] Buf, int iStart, int iLen, UInt16 uiSeed = 0x8408)
        {
            UInt16 uiCRC = 0xffff;

            for (int iLoop = 0; iLoop < iLen; iLoop++)
            {
                uiCRC ^= (UInt16)Buf[iStart + iLoop];
                for (int iLoop1 = 0; iLoop1 < 8; iLoop1++)
                {
                    if ((uiCRC & 1) == 1)
                    {
                        uiCRC >>= 1;
                        uiCRC ^= uiSeed;
                    }
                    else
                    {
                        uiCRC >>= 1;
                    }
                }
            }
            uiCRC ^= 0xffff;
            return uiCRC;
        }
        #endregion
    }
}
