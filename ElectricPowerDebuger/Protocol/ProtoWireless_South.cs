using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using ElectricPowerDebuger.Common;

namespace ElectricPowerDebuger.Protocol
{
    class ProtoWireless_South
    {
        public const ushort FrameHeader = 0xAA55;       // 帧头
        public const byte FrameFixedLen = 9;           // 帧头， Rssi,长度,信道号,phy帧头校验,Mac帧(>18),phy载荷校验
        public const byte LongAddrSize = 6;             // 地址的长度

        public delegate TreeNode ExplainCallback(byte[] buf);
        public struct CmdExplain
        {
            public byte CmdId;
            public String CmdName;
            public Color CmdColor;
            public ExplainCallback CmdExplainFunc;

            public CmdExplain(byte id, String name, Color color, ExplainCallback callback)
            {
                CmdId = id;
                CmdName = name;
                CmdColor = color;
                CmdExplainFunc = callback;
            }
        }

        #region 协议帧格式
        // 通信报文格式
        public struct FrameFormat
        {
            public ushort Header;           // 帧头 固定值 AA55
            public byte Rssi;               // 接收场强           
            public byte Length;                 // 长度
            public byte Chanel;                 // 信道号 
            public byte PhrCrc;                 // 物理层帧头校验
            public MacFrame Mac;                // Mac帧
            public NwkFrame Nwk;                    // Nwk帧
            public ApsFrame Aps;                        // Aps帧
            public ushort Crc16;                // Crc16校验
        };

        // Mac层
        public struct MacFrame
        {
            public MacCtrl CtrlWord;            // 控制字
            public byte FrameSn;                // 帧序号
            public ushort PanID;                // PanID
            public byte[] DstAddr;              // 目标地址
            public byte[] SrcAddr;              // 源地址
            public byte[] Payload;              // 载荷
        };

        public struct MacCtrl
        {
            public MacFrameType FrameType;  // 帧类型  bit2-0
            public bool SecFlag;            // 安全使能标识 bit3
            public bool AckFlag;            // 确认应答标识 bit5
            public byte AddrMode;           // 地址模式 bit11-10

            public ushort All;              // 整个控制域，UInt16格式
        };

        public enum MacFrameType
        {
            Beacon = 0x00,
            Data,
            Ack,
            Cmd,
            Invalid = 0xFF
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
            public NwkExtend Extend;            // 扩展域
            public byte[] Payload;              // 载荷
        };

        public struct NwkCtrl
        {
            public NwkFrameType FrameType;  // 帧类型  bit1-0
            public bool ExtendFlag;         // 扩展域标识 bit2
            public byte ProtolVer;          // 协议版本  bit6-3
            public bool RouteFlag;          // 扩展域标识 bit7
            public byte AddrMode;           // 地址模式 bit11-10

            public ushort All;              // 整个控制域，UInt16格式
        };

        public enum NwkFrameType
        {
            Data = 0x00,
            Cmd,
            Invalid = 0xFF
        }

        public struct NwkRoute
        {
            public byte RelayCount;         // 中继节点数 bit3-0
            public byte RelayIndex;         // 中继索引 bit7-4
            public byte AddrMode;           // 地址模式 bit9-8
            public byte[] RelayList;        // 中继列表
        };

        public struct NwkExtend
        {
            public byte Length;             // 扩展域长度:(长度 = 厂家标识 + 扩展域数据区)
            public ushort OemInfo;          // 厂家标识
            public byte[] Data;             // 扩展域数据区
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
            public bool DirFlag;            // 传输方向标识 bit4

            public byte All;                // 整个控制域，byte格式
        };

        public enum ApsFrameType
        {
            Ack = 0x00,
            Cmd,
            DataTransfer,
            Report,
            BroadCast,
            Test,
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
        public static FrameFormat ExplainRxPacket(byte[] rxBuf)
        {
            FrameFormat rxData = new FrameFormat();

            try
            {
                int index = 0;
                int addrLen = 0;

                if (rxBuf.Length < index + 8) throw new Exception("Mac层无效");

                // 如果有 帧头、Rssi
                if (rxBuf[0] == 0x55 && rxBuf[1] == 0xAA)
                {
                    rxData.Header = (ushort)(rxBuf[index] + rxBuf[index + 1] * 256);
                    index += 2;
                    rxData.Rssi = rxBuf[index++];
                }
                // Phy层提取
                rxData.Length = rxBuf[index++];
                rxData.Chanel = rxBuf[index++];
                rxData.PhrCrc = rxBuf[index++];

                rxData.Crc16 = (ushort)(rxBuf[rxBuf.Length - 2] + rxBuf[rxBuf.Length - 1] * 256);

                // Mac层提取
                if (rxBuf.Length < index + 11) throw new Exception("Mac层无效");

                rxData.Mac.CtrlWord.All = (ushort)(rxBuf[index] + rxBuf[index + 1] * 256);
                rxData.Mac.CtrlWord.FrameType = (MacFrameType)(rxBuf[index] & 0x07);
                rxData.Mac.CtrlWord.SecFlag = (rxBuf[index] & 0x08) > 0 ? true : false;
                rxData.Mac.CtrlWord.AckFlag = (rxBuf[index++] & 0x20) > 0 ? true : false;
                rxData.Mac.CtrlWord.AddrMode = (byte)(rxBuf[index++] >> 2 & 0x03);
                rxData.Mac.FrameSn = rxBuf[index++];
                rxData.Mac.PanID = (ushort)(rxBuf[index++] + rxBuf[index++] * 256);

                if ((rxData.Mac.CtrlWord.FrameType > MacFrameType.Cmd)	//无效帧 
                    || (rxData.Mac.CtrlWord.AddrMode != 2 && rxData.Mac.CtrlWord.AddrMode != 3)
                    || ((rxData.Mac.CtrlWord.All | 0x0C2F) != 0x0C2F))
                {
                    throw new Exception("Mac层无效");
                }

                addrLen = rxData.Mac.CtrlWord.AddrMode == 0x02 ? 2 : 6;

                if (rxBuf.Length < index + addrLen * 2 + 2) throw new Exception("Mac层无效");

                rxData.Mac.DstAddr = new byte[addrLen];
                Array.Copy(rxBuf, index, rxData.Mac.DstAddr, 0, addrLen);
                index += addrLen;
                rxData.Mac.SrcAddr = new byte[addrLen];
                Array.Copy(rxBuf, index, rxData.Mac.SrcAddr, 0, addrLen);
                index += addrLen;

                if (rxData.Mac.CtrlWord.FrameType != MacFrameType.Data)     // 不是数据帧，mac层处理  
                {
                    int PayloadLen = rxBuf.Length - index - 2;
                    rxData.Mac.Payload = new byte[PayloadLen];
                    Array.Copy(rxBuf, index, rxData.Mac.Payload, 0, PayloadLen);
                }
                else                                            // 0x01 Mac数据帧
                {
                    //Nwk层提取
                    if (rxBuf.Length < index + 9) throw new Exception("Nwk层无效");

                    rxData.Nwk.CtrlWord.All = (ushort)(rxBuf[index] + rxBuf[index + 1] * 256);
                    rxData.Nwk.CtrlWord.FrameType = (NwkFrameType)(rxBuf[index] & 0x03);
                    rxData.Nwk.CtrlWord.ExtendFlag = (rxBuf[index] & 0x04) > 0 ? true : false;
                    rxData.Nwk.CtrlWord.ProtolVer = (byte)(rxBuf[index] >> 3 & 0x0F);
                    rxData.Nwk.CtrlWord.RouteFlag = (rxBuf[index++] & 0x80) > 0 ? true : false;
                    rxData.Nwk.CtrlWord.AddrMode = (byte)(rxBuf[index++] >> 2 & 0x03);

                    addrLen = rxData.Nwk.CtrlWord.AddrMode == 0x02 ? 2 : 6;

                    if (rxBuf.Length < index + addrLen * 2 + 3) throw new Exception("Nwk层无效");

                    rxData.Nwk.DstAddr = new byte[addrLen];
                    Array.Copy(rxBuf, index, rxData.Nwk.DstAddr, 0, addrLen);
                    index += addrLen;
                    rxData.Nwk.SrcAddr = new byte[addrLen];
                    Array.Copy(rxBuf, index, rxData.Nwk.SrcAddr, 0, addrLen);
                    index += addrLen;

                    rxData.Nwk.Radius = (byte)(rxBuf[index] & 0x0F);
                    rxData.Nwk.FrameSn = (byte)(rxBuf[index++] >> 4);

                    if (true == rxData.Nwk.CtrlWord.RouteFlag)
                    {
                        rxData.Nwk.Route.RelayCount = (byte)(rxBuf[index] & 0x0F);
                        rxData.Nwk.Route.RelayIndex = (byte)(rxBuf[index++] >> 4);
                        rxData.Nwk.Route.AddrMode = (byte)(rxBuf[index++] & 0x03);

                        addrLen = rxData.Nwk.Route.AddrMode == 0x02 ? 2 : 6;

                        if (rxBuf.Length < index + addrLen * rxData.Nwk.Route.RelayCount + 3) throw new Exception("Nwk层无效");

                        rxData.Nwk.Route.RelayList = new byte[addrLen * rxData.Nwk.Route.RelayCount];
                        Array.Copy(rxBuf, index, rxData.Nwk.Route.RelayList, 0, addrLen * rxData.Nwk.Route.RelayCount);
                        index += addrLen * rxData.Nwk.Route.RelayCount;
                    }

                    if (true == rxData.Nwk.CtrlWord.ExtendFlag)
                    {
                        rxData.Nwk.Extend.Length = rxBuf[index++];

                        if (rxBuf.Length < index + rxData.Nwk.Extend.Length + 2) throw new Exception("Nwk层无效");

                        if (rxData.Nwk.Extend.Length >= 2)
                        {
                            rxData.Nwk.Extend.OemInfo = (ushort)(rxBuf[index] * 256 + rxBuf[index + 1]);  // ascii格式
                            rxData.Nwk.Extend.Data = new byte[rxData.Nwk.Extend.Length - 2];
                            Array.Copy(rxBuf, index, rxData.Nwk.Extend.Data, 0, rxData.Nwk.Extend.Length - 2);
                        }

                        index += rxData.Nwk.Extend.Length;
                    }

                    if (rxData.Nwk.CtrlWord.FrameType > NwkFrameType.Cmd)      //无法识别的Nwk帧类型
                    {
                        rxData.Nwk.Payload = null;
                    }
                    else if (rxData.Nwk.CtrlWord.FrameType == NwkFrameType.Cmd)  // 0x01 Nwk命令帧
                    {
                        int PayloadLen = rxBuf.Length - index - 2;
                        rxData.Nwk.Payload = new byte[PayloadLen];
                        Array.Copy(rxBuf, index, rxData.Nwk.Payload, 0, PayloadLen);
                    }
                    else                                            // 0x00 Nwk数据帧
                    {
                        // Aps层提取

                        if (rxBuf.Length < index + 5) throw new Exception("Aps层无效");

                        rxData.Aps.CtrlWord.All = rxBuf[index];
                        rxData.Aps.CtrlWord.FrameType = (ApsFrameType)(rxBuf[index] & 0x07);
                        rxData.Aps.CtrlWord.ExtendFlag = (rxBuf[index] & 0x08) > 0 ? true : false;
                        rxData.Aps.CtrlWord.DirFlag = (rxBuf[index++] & 0x10) > 0 ? true : false;

                        rxData.Aps.FrameSn = rxBuf[index++];

                        if (true == rxData.Aps.CtrlWord.ExtendFlag)
                        {
                            rxData.Aps.Extend.Length = rxBuf[index++];

                            if (rxBuf.Length < index + rxData.Aps.Extend.Length + 3) throw new Exception("Aps层无效");

                            if (rxData.Aps.Extend.Length >= 2)
                            {
                                rxData.Aps.Extend.OemInfo = (ushort)(rxBuf[index] * 256 + rxBuf[index + 1]); //ascii
                                rxData.Aps.Extend.Data = new byte[rxData.Aps.Extend.Length - 2];
                                Array.Copy(rxBuf, index, rxData.Aps.Extend.Data, 0, rxData.Aps.Extend.Length - 2);
                            }
                            index += rxData.Aps.Extend.Length;
                        }

                        if (rxData.Aps.CtrlWord.FrameType > ApsFrameType.Test)      //无法识别的Aps帧类型
                        {
                            rxData.Aps.Payload = null;
                        }
                        else    // 0x00~0x05 依次为Aps层 确认/否认帧、命令帧、数据转发帧、上报帧、广播业务帧、测试帧
                        {
                            int PayloadLen = rxBuf.Length - index - 2;
                            rxData.Aps.Payload = new byte[PayloadLen];
                            Array.Copy(rxBuf, index, rxData.Aps.Payload, 0, PayloadLen);
                        }

                    } //Aps

                } //Nwk

                rxData.Crc16 = (ushort)(rxBuf[rxBuf.Length - 2] + rxBuf[rxBuf.Length - 1] * 256);
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
                        MessageBox.Show("数据解析异常:" + ex.Message + ex.StackTrace);
                        break;
                }
            }
            return rxData;
        }
        #endregion

        #region 协议帧解析
        // 解析顶层帧类型、颜色
        public static void GetTopFrameTypeAndColor(FrameFormat frame, out string frameType, out Color frameColor)
        {
            frameType = "";
            frameColor = Color.Black;

            // Mac层 -> Nwk层 -> Aps层
            MacFrameType macFrameType = frame.Mac.CtrlWord.FrameType;
            switch (macFrameType)
            {
                case MacFrameType.Beacon:
                    frameType = "MAC层：信标帧";
                    frameColor = Color.Red;
                    break;

                case MacFrameType.Data: // "Mac层：数据帧";
                    NwkFrameType nwkFrameType = frame.Nwk.CtrlWord.FrameType;
                    if (nwkFrameType == NwkFrameType.Cmd)       // "Nwk层：命令帧"
                    {
                        frameType = "网络层：";
                        byte cmdId = frame.Nwk.Payload[0];
                        frameType += NwkExplain.GetCmdName(cmdId);
                        frameColor = NwkExplain.GetCmdColor(cmdId);
                    }
                    else if (nwkFrameType == NwkFrameType.Data) // "Nwk层：数据帧"
                    {
                        ApsFrameType apsFrameType = frame.Aps.CtrlWord.FrameType;
                        frameType = "应用层：" + ApsFrameTypeTbl[(byte)apsFrameType];
                        frameColor = Color.Olive;

                        if (apsFrameType == ApsFrameType.Cmd)
                        {
                            byte cmdId = frame.Aps.Payload[0];
                            bool isDirUp = frame.Aps.CtrlWord.DirFlag;
                            frameType = frameType.Substring(0, 4) + ApsExplain.GetCmdName(isDirUp, cmdId);
                            frameColor = ProtoWireless_South.ApsExplain.GetCmdColor(cmdId);
                        }
                        else if (apsFrameType == ApsFrameType.DataTransfer)
                        {
                            if (frame.Aps.CtrlWord.DirFlag)
                            {
                                frameType += "应答";
                            }
                        }
                        else
                        {
                            frameType = "应用层：无法识别";
                        }
                    }
                    else
                    {
                        frameType = "网络层：无法识别的帧类型";
                        frameColor = Color.Gray;
                    }
                    break;

                case MacFrameType.Ack:
                    frameType = "MAC层：确认帧";
                    frameColor = Color.Red;
                    break;
                case MacFrameType.Cmd:
                    frameType = "MAC层：命令帧";
                    frameColor = Color.Red;
                    break;
                default:
                    frameType = "MAC层：无法识别的帧类型";
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
                PhyNode.Nodes.Add("帧长度 ：" + RxFrame.Length);
                PhyNode.Nodes.Add("信道组-频点 ：" + RxFrame.Chanel / 2 + "-" + RxFrame.Chanel % 2);
                PhyNode.Nodes.Add("帧头校验：" + RxFrame.PhrCrc);
            }
            parentNode.Nodes.Add(PhyNode);

            // parentNode--MAC层
            TreeNode MacNode = MacExplain.GetTreeNode(RxFrame, Color.Red);
            parentNode.Nodes.Add(MacNode);

            if (RxFrame.Mac.CtrlWord.FrameType == MacFrameType.Data)
            {
                // parentNode--网络层
                TreeNode NwkNode = NwkExplain.GetTreeNode(RxFrame, Color.Orange);
                parentNode.Nodes.Add(NwkNode);

                if (RxFrame.Nwk.CtrlWord.FrameType == NwkFrameType.Data)
                {
                    // parentNode--应用层
                    TreeNode ApsNode = ApsExplain.GetTreeNode(RxFrame, Color.Green);
                    parentNode.Nodes.Add(ApsNode);
                }
            }
            parentNode.ExpandAll();

            return parentNode;
        }
        #endregion

        #region Mac层解析
        public class MacExplain             //Mac层解析
        {
            public static TreeNode GetTreeNode(FrameFormat rxFrame, Color fgColor)
            {
                TreeNode macNode = new TreeNode("MAC层");
                macNode.ForeColor = fgColor;

                TreeNode node = null;
                TreeNode payloadNode = null;
                String strTmp = "";
                int addrLen;

                if (rxFrame.Mac.CtrlWord.FrameType == MacFrameType.Invalid)  //无效帧
                {
                    macNode.Nodes.Add(new TreeNode("无法识别的MAC帧"));
                    return macNode;
                }

                node = new TreeNode("帧控制域：0x" + rxFrame.Mac.CtrlWord.All.ToString("X4"));
                {
                    switch ((MacFrameType)rxFrame.Mac.CtrlWord.FrameType)
                    {
                        case MacFrameType.Beacon:
                            strTmp = "帧类型  ：信标帧(" + rxFrame.Mac.CtrlWord.FrameType.ToString("X2") + ")";
                            payloadNode = explainBeaconFrame(rxFrame.Mac.Payload);
                            break;
                        case MacFrameType.Data:
                            strTmp = "帧类型  ：数据帧(" + rxFrame.Mac.CtrlWord.FrameType.ToString("X2") + ")";
                            payloadNode = new TreeNode("MAC层负载 ：网络层");
                            break;
                        case MacFrameType.Ack:
                            strTmp = "帧类型  ：确认帧(" + rxFrame.Mac.CtrlWord.FrameType.ToString("X2") + ")";
                            payloadNode = new TreeNode("MAC层负载 ：确认帧");
                            payloadNode.Nodes.Add(new TreeNode("RSSI值 ：" + rxFrame.Mac.Payload[0].ToString()));
                            break;
                        case MacFrameType.Cmd:
                            strTmp = "帧类型  ：命令帧(" + rxFrame.Mac.CtrlWord.FrameType.ToString("X2") + ")";
                            payloadNode = new TreeNode("MAC层负载 ：命令帧");
                            payloadNode.Nodes.Add(new TreeNode("当前协议无法解析 ："));
                            break;

                        default:
                            payloadNode = new TreeNode("MAC层负载 ：无法识别");
                            break;
                    }
                    node.Nodes.Add(new TreeNode(strTmp));
                    strTmp = "安全使能：";
                    strTmp += rxFrame.Mac.CtrlWord.SecFlag == true ? "是" : "否";
                    node.Nodes.Add(new TreeNode(strTmp));
                    strTmp = "确认请求：";
                    strTmp += rxFrame.Mac.CtrlWord.AckFlag == true ? "是" : "否";
                    node.Nodes.Add(new TreeNode(strTmp));
                    strTmp = "地址模式：";
                    strTmp += rxFrame.Mac.CtrlWord.AddrMode == 0x02 ? "2字节" : "6字节";
                    addrLen = rxFrame.Mac.CtrlWord.AddrMode == 0x02 ? 2 : 6;
                    node.Nodes.Add(new TreeNode(strTmp));
                }
                macNode.Nodes.Add(node);

                node = new TreeNode("帧序号  ：" + rxFrame.Mac.FrameSn.ToString("X2"));
                macNode.Nodes.Add(node);

                node = new TreeNode("PanID   ：" + rxFrame.Mac.PanID.ToString("X4"));
                macNode.Nodes.Add(node);

                strTmp = "目的地址：";
                strTmp += Util.GetStringHexFromByte(rxFrame.Mac.DstAddr, 0, addrLen, "", true);
                node = new TreeNode(strTmp);
                macNode.Nodes.Add(node);

                strTmp = "源地址  ：";
                strTmp += Util.GetStringHexFromByte(rxFrame.Mac.SrcAddr, 0, addrLen, "", true);
                node = new TreeNode(strTmp);
                macNode.Nodes.Add(node);

                macNode.Nodes.Add(payloadNode);

                return macNode;
            }

            private static TreeNode explainBeaconFrame(byte[] payload)
            {
                TreeNode beaconNode = new TreeNode("MAC层负载：信标帧");

                TreeNode node = null;
                String strTmp = "";

                strTmp = "发射随机延时：" + payload[0].ToString();
                node = new TreeNode(strTmp);
                beaconNode.Nodes.Add(node);
                strTmp = "层次号      ：" + (payload[1] & 0x0F).ToString();
                node = new TreeNode(strTmp);
                beaconNode.Nodes.Add(node);
                strTmp = "信标轮次    ：" + (payload[1] >> 4).ToString();
                node = new TreeNode(strTmp);
                beaconNode.Nodes.Add(node);
                strTmp = "层次号限值  ：" + (payload[2] & 0x0F).ToString();
                node = new TreeNode(strTmp);
                beaconNode.Nodes.Add(node);
                strTmp = "信标轮次限值：" + (payload[2] >> 4).ToString();
                node = new TreeNode(strTmp);
                beaconNode.Nodes.Add(node);
                strTmp = "时隙号      ：" + (payload[3] + payload[4] * 256); 
                node = new TreeNode(strTmp);
                beaconNode.Nodes.Add(node);
                strTmp = "信标标识    ：" + payload[5].ToString("X2");
                node = new TreeNode(strTmp);
                beaconNode.Nodes.Add(node);
                strTmp = "网络规模    ：" + (payload[6] + payload[7]*256);
                node = new TreeNode(strTmp);
                beaconNode.Nodes.Add(node);
                strTmp = "场强门限    ：" + payload[8].ToString(); 
                node = new TreeNode(strTmp);
                beaconNode.Nodes.Add(node);
                strTmp = "中心节点信道组：" + payload[9].ToString() ; 
                node = new TreeNode(strTmp);
                beaconNode.Nodes.Add(node);
                strTmp = "中心节点PanID ：" + payload[11].ToString("X2") + payload[10].ToString("X2");
                node = new TreeNode(strTmp);
                beaconNode.Nodes.Add(node);
                strTmp = "中心节点地址  ：";
                strTmp += Util.GetStringHexFromByte(payload, 12, LongAddrSize, "", true);
                node = new TreeNode(strTmp);
                beaconNode.Nodes.Add(node);

                return beaconNode;
            }
        }
        #endregion

        #region Nwk层解析
        public class NwkExplain             //Nwk层解析
        {
            private static readonly CmdExplain[] NwkCmdTbl = new CmdExplain[11]
            {
                new CmdExplain( 0x01, "入网申请请求", Color.Orchid, new ExplainCallback(explainJoinNwkRequest)),
                new CmdExplain( 0x02, "入网申请响应", Color.Orchid, new ExplainCallback(explainJoinNwkResponse)),
                new CmdExplain( 0x03, "路由错误",     Color.Crimson, new ExplainCallback(explainRouteErrorCmd)),
                new CmdExplain( 0x10, "场强收集",     Color.Orange, new ExplainCallback(explainGatherRssiRequest)),
                new CmdExplain( 0x11, "场强收集应答", Color.Orange, new ExplainCallback(explainGatherRssiResponse)),
                new CmdExplain( 0x12, "配置子节点",     Color.Green, new ExplainCallback(explainConfigSubNodeRequest)),
                new CmdExplain( 0x13, "配置子节点应答", Color.Green, new ExplainCallback(explainConfigSubNodeResponse)),
                new CmdExplain( 0x14, "网络维护请求", Color.Blue, new ExplainCallback(explainNwkMaintenanceRequest)),
                new CmdExplain( 0x15, "网络维护响应", Color.Blue, new ExplainCallback(explainNwkMaintenanceResponse)),
                new CmdExplain( 0x16, "游离节点就绪", Color.Orchid, new ExplainCallback(explainOffLineNodeReadyCmd)),
                new CmdExplain( 0x17, "路径记录命令", Color.Brown, new ExplainCallback(explainRouteRecordCmd)),
            };

            public static String GetCmdName(byte cmdId)
            {
                string strName = "";

                foreach (CmdExplain cmd in NwkCmdTbl)
                {
                    if( cmd.CmdId == cmdId )
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
            public static TreeNode ExplainCmd(byte cmdId, byte[] buf)
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
                
                if( callback != null)
                {
                    payloadNode = callback(buf);
                }

                return payloadNode;
            }

            public static TreeNode GetTreeNode(FrameFormat rxFrame, Color fgColor)
            {
                TreeNode nwkNode = new TreeNode("网络层");
                nwkNode.ForeColor = fgColor;

                TreeNode node = null;
                TreeNode payloadNode = null;
                String strTmp = "";
                int addrLen;

                if (rxFrame.Nwk.CtrlWord.FrameType == NwkFrameType.Invalid)
                {
                    nwkNode.Nodes.Add(new TreeNode("无法识别的NWK帧"));
                    return nwkNode;
                }

                node = new TreeNode("帧控制域：0x" + rxFrame.Nwk.CtrlWord.All.ToString("X4"));
                {
                    switch ((NwkFrameType)rxFrame.Nwk.CtrlWord.FrameType)
                    {
                        case NwkFrameType.Data:
                            strTmp = "帧类型      ：数据帧(" + rxFrame.Nwk.CtrlWord.FrameType.ToString("X2") + ")";
                            payloadNode = new TreeNode("网络层负载：应用层");
                            break;

                        case NwkFrameType.Cmd:
                            strTmp = "帧类型      ：命令帧(" + rxFrame.Nwk.CtrlWord.FrameType.ToString("X2") + ")";
                            payloadNode = ExplainCmd(rxFrame.Nwk.Payload[0], rxFrame.Nwk.Payload);
                            break;

                        default:
                            payloadNode = new TreeNode("网络层负载：无法识别");
                            break;
                    }
                    node.Nodes.Add(new TreeNode(strTmp));

                    strTmp = "扩展信息指示：";
                    strTmp += rxFrame.Nwk.CtrlWord.ExtendFlag == true ? "有" : "无";
                    node.Nodes.Add(new TreeNode(strTmp));
                    strTmp = "协议版本号  ：";
                    strTmp += rxFrame.Nwk.CtrlWord.ProtolVer.ToString("X2");
                    node.Nodes.Add(new TreeNode(strTmp));
                    strTmp = "路由信息指示：";
                    strTmp += rxFrame.Nwk.CtrlWord.RouteFlag == true ? "有" : "无";
                    node.Nodes.Add(new TreeNode(strTmp));
                    strTmp = "地址模式    ：";
                    strTmp += rxFrame.Nwk.CtrlWord.AddrMode == 0x02 ? "2字节" : "6字节";
                    addrLen = rxFrame.Nwk.CtrlWord.AddrMode == 0x02 ? 2 : 6;
                    node.Nodes.Add(new TreeNode(strTmp));
                }
                nwkNode.Nodes.Add(node);

                strTmp = "目的地址：";
                strTmp += Util.GetStringHexFromByte(rxFrame.Nwk.DstAddr, 0, addrLen, "", true);
                node = new TreeNode(strTmp);
                nwkNode.Nodes.Add(node);

                strTmp = "源地址  ：";
                strTmp += Util.GetStringHexFromByte(rxFrame.Nwk.SrcAddr, 0, addrLen, "", true);
                node = new TreeNode(strTmp);
                nwkNode.Nodes.Add(node);

                node = new TreeNode("网络半径：" + rxFrame.Nwk.Radius.ToString("X"));
                nwkNode.Nodes.Add(node);

                node = new TreeNode("帧序号  ：" + rxFrame.Nwk.FrameSn.ToString("X"));
                nwkNode.Nodes.Add(node);

                if(true == rxFrame.Nwk.CtrlWord.RouteFlag)
                {
                    node = new TreeNode("路由信息域：" );
                    strTmp = "中继总数：" + rxFrame.Nwk.Route.RelayCount.ToString();
                    node.Nodes.Add(strTmp);
                    strTmp = "中继索引：" + rxFrame.Nwk.Route.RelayIndex.ToString();
                    node.Nodes.Add(strTmp);
                    strTmp = "地址模式：" + (rxFrame.Nwk.Route.AddrMode == 0x02 ? "2字节" : "6字节");
                    node.Nodes.Add(strTmp);
                    // 中继列表  
                    for (int i = 0; i < rxFrame.Nwk.Route.RelayCount; i++ )
                    {
                        strTmp = "中继" + (i+1).ToString() + "：";
                        strTmp += Util.GetStringHexFromByte(rxFrame.Nwk.Route.RelayList, i * addrLen, addrLen, "", true);
                        node.Nodes.Add(strTmp);
                    }
                    nwkNode.Nodes.Add(node);
                }

                if (true == rxFrame.Nwk.CtrlWord.ExtendFlag)
                {
                    node = new TreeNode("扩展信息域：");
                    strTmp = "扩展域长度：" + rxFrame.Nwk.Extend.Length.ToString();
                    node.Nodes.Add(strTmp);

                    if (rxFrame.Nwk.Extend.Length >= 2)
                    {
                        strTmp = "厂家标识  ：" + rxFrame.Nwk.Extend.OemInfo.ToString("X4");
                        strTmp += " (" + Convert.ToChar(rxFrame.Nwk.Extend.OemInfo >> 8)
                                    + Convert.ToChar(rxFrame.Nwk.Extend.OemInfo & 0x00FF) + ")";
                        node.Nodes.Add(strTmp);
                        strTmp = "扩展域数据：" + "(" + (rxFrame.Nwk.Extend.Length - 2) + "byte)";
                        node.Nodes.Add(strTmp);
                    }
                    nwkNode.Nodes.Add(node);
                }

                nwkNode.Nodes.Add(payloadNode);

                return nwkNode;
            }

            //入网申请请求
            private static TreeNode explainJoinNwkRequest(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("网络层负载：命令帧");

                TreeNode node = null;
                String strTmp = "";

                strTmp = "命令标识：" + GetCmdName(buf[0]) + "(0x" + buf[0].ToString("X2") + ")";
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "命令选项：0x" + buf[1].ToString("X2");
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
        
                return payloadNode;
            }
            //入网申请响应
            private static TreeNode explainJoinNwkResponse(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("网络层负载：命令帧");

                TreeNode node = null;
                String strTmp = "";

                strTmp = "命令标识：" + GetCmdName(buf[0]) + "(0x" + buf[0].ToString("X2") + ")";
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "命令选项：0x" + buf[1].ToString("X2");
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "PanID ：" + buf[3].ToString("X2") + buf[2].ToString("X2");
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "中心节点地址：";
                strTmp += Util.GetStringHexFromByte(buf, 4, LongAddrSize, "", true);
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "层次号：" + buf[10].ToString();
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "时隙号：" + (buf[11] + buf[12] * 256).ToString("X4") + " (" + (buf[11] + buf[12] * 256) + ")"; 
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "RSSI  ：" + buf[13].ToString();
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "中继节点数：" + buf[14].ToString();
                node = new TreeNode(strTmp);
                // 中继列表
                for (int i = 0, index = 14; i < buf[14]; i++)
                {
                    strTmp = "中继" + (i + 1) + "：";
                    strTmp += Util.GetStringHexFromByte(buf, index, 2, "", true);
                    node.Nodes.Add(new TreeNode(strTmp));
                    index += 2;  
                }
                payloadNode.Nodes.Add(node);

                return payloadNode;
            }
            //路由错误命令
            private static TreeNode explainRouteErrorCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("网络层负载：命令帧");

                TreeNode node = null;
                String strTmp = "";

                strTmp = "命令标识：" + GetCmdName(buf[0]) + "(0x" + buf[0].ToString("X2") + ")";
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "错误代码：" + (buf[1] == 0x01 ? "目标无响应" : "其他错误");
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "失败帧目标地址：";
                int addrLen = buf.Length < 8 ?  2 : 6;
                strTmp += Util.GetStringHexFromByte(buf, 2, addrLen, "", true);
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);

                return payloadNode;
            }
            //收集场强命令
            private static TreeNode explainGatherRssiRequest(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("网络层负载 ：命令帧");

                TreeNode node = null;
                String strTmp = "";

                strTmp = "命令标识：" + GetCmdName(buf[0]) + "(0x" + buf[0].ToString("X2") + ")";
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "页序号  ：" + buf[1].ToString();
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);

                return payloadNode;
            }
            //收集场强应答
            private static TreeNode explainGatherRssiResponse(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("网络层负载 ：命令帧");

                TreeNode node = null;
                String strTmp = "";

                strTmp = "命令标识：" + GetCmdName(buf[0]) + "(0x" + buf[0].ToString("X2") + ")";
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "页序号  ：" + (buf[1] & 0x0F).ToString();
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "总页数  ：" + (buf[1] >> 4).ToString();
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "邻居节点数：" + buf[2].ToString();
                node = new TreeNode(strTmp);
                // 邻居场强列表
                for (int i = 0, index = 3; i < buf[2]; i++)
                {
                    strTmp = "邻居" + (i + 1) + "：";
                    strTmp += Util.GetStringHexFromByte(buf, index, LongAddrSize, "", true);
                    index += 6;
                    strTmp += " (" + buf[index++].ToString() + ")";
                    node.Nodes.Add(new TreeNode(strTmp));
                }
                payloadNode.Nodes.Add(node);

                return payloadNode;
            }
            //配置子节点命令
            private static TreeNode explainConfigSubNodeRequest(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("网络层负载：命令帧");

                TreeNode node = null;
                String strTmp = "";

                strTmp = "命令标识：" + GetCmdName(buf[0]) + "(0x" + buf[0].ToString("X2") + ")";
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "命令选项：0x" + buf[1].ToString("X2");
                {
                    node = new TreeNode(strTmp);
                    strTmp = "网络属性域：" + ((buf[1] & 0x01) > 0 ? "有" : "无");
                    node.Nodes.Add(new TreeNode(strTmp));
                    strTmp = "中继列表域：" + ((buf[1] & 0x02) > 0 ? "有" : "无");
                    node.Nodes.Add(new TreeNode(strTmp));
                    strTmp = "在网标识  ：" + ((buf[1] & 0x80) > 0 ? "离网" : "在网");
                    node.Nodes.Add(new TreeNode(strTmp));
                    payloadNode.Nodes.Add(node);
                }
                strTmp = "信道组：" + buf[2].ToString();
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "层次号：" + buf[3].ToString();
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "时隙号：" + (buf[4] + buf[5] * 256).ToString("X4") + " (" + (buf[4] + buf[5] * 256) + ")"; 
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "短地址：" + buf[7].ToString("X2") + buf[6].ToString("X2");
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "PanID ：" + buf[9].ToString("X2") + buf[8].ToString("X2");
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "上传路径模式：" + (buf[10] == 0 ? "源路由" : (buf[10] == 1 ? "父节点" : "自主")) + "模式";
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "路径数：" + buf[11].ToString();
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                // 路径列表
                int routeCount = buf[11];
                int relayCount;
                for (int i = 0, index = 12; i < routeCount; i++)
                {
                    relayCount = buf[index++];
                    strTmp = "路径" + (i + 1) + "中继数：" + relayCount;
                    node = new TreeNode(strTmp);
                    for (int j = 0; j < relayCount; j++)
                    {
                        strTmp = "中继" + (j + 1) + "：" ;
                        strTmp += Util.GetStringHexFromByte(buf, index, 2, "", true);
                        node.Nodes.Add(new TreeNode(strTmp));
                        index += 2;
                    }
                    payloadNode.Nodes.Add(node);
                }

                return payloadNode;
            }
            //配置子节点应答
            private static TreeNode explainConfigSubNodeResponse(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("网络层负载：命令帧");

                TreeNode node = null;
                String strTmp = "";

                strTmp = "命令标识：" + GetCmdName(buf[0]) + "(0x" + buf[0].ToString("X2") + ")";
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "命令选项：0x" + buf[1].ToString("X2");
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "硬件版本：" + buf[3].ToString("X") + "." + buf[2].ToString("X2");
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "软件版本：" + buf[6].ToString("X") + "." + buf[5].ToString("X2") + buf[4].ToString("X2");
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "厂家标识：" + Convert.ToChar(buf[8]) + Convert.ToChar(buf[7]);
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "节点类型：";
                switch(buf[9])
                {
                    case 0x00:  strTmp += "中心节点";   break;
                    case 0x01:  strTmp += "I型采集器";  break;
                    case 0x02:  strTmp += "II型采集器"; break;
                    case 0x03:  strTmp += "电表";       break;
                    default:    strTmp += "未知";       break;
                }
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);

                return payloadNode;
            }
            //网络维护请求
            private static TreeNode explainNwkMaintenanceRequest(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("网络层负载：命令帧");

                TreeNode node = null;
                String strTmp = "";

                strTmp = "命令标识：" + GetCmdName(buf[0]) + "(0x" + buf[0].ToString("X2") + ")";
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "跳数 ：" + buf[1].ToString();
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                int index = 2;
                strTmp = "下行场强列表：";
                {
                    node = new TreeNode(strTmp);
                    for (int i = 0; i < buf[1]; i++)
                    {
                        strTmp = "第" + (i + 1) + "跳：" + buf[index++];
                        node.Nodes.Add(new TreeNode(strTmp));
                    }
                    payloadNode.Nodes.Add(node);
                }

                return payloadNode;
            }
            //网络维护响应
            private static TreeNode explainNwkMaintenanceResponse(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("网络层负载：命令帧");

                TreeNode node = null;
                String strTmp = "";

                strTmp = "命令标识：" + GetCmdName(buf[0]) + "(0x" + buf[0].ToString("X2") + ")";
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "跳数 ：" + buf[1].ToString();
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                int index = 2;
                strTmp = "下行场强列表：";
                {
                    node = new TreeNode(strTmp);
                    for (int i = 0; i < buf[1]; i++)
                    {
                        strTmp = "第" + (i + 1) + "跳：" + buf[index++];
                        node.Nodes.Add(new TreeNode(strTmp));
                    }
                    payloadNode.Nodes.Add(node);
                }
                strTmp = "上行场强列表：";
                {
                    node = new TreeNode(strTmp);
                    for (int i = 0; i < buf[1]; i++)
                    {
                        strTmp = "第" + (i + 1) + "跳：" + buf[index++];
                        node.Nodes.Add(new TreeNode(strTmp));
                    }
                    payloadNode.Nodes.Add(node);
                }

                return payloadNode;
            }
            //游离节点就绪命令
            private static TreeNode explainOffLineNodeReadyCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("网络层负载：命令帧");

                TreeNode node = null;
                String strTmp = "";

                strTmp = "命令标识：" + GetCmdName(buf[0]) + "(0x" + buf[0].ToString("X2") + ")";
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "配置请求：" + ((buf[1] & 0x01) > 0 ? "是" : "否");
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "层次号  ：" + buf[2].ToString();
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "时隙号  ：" + (buf[3] + buf[4] * 256).ToString("X4") + " (" + (buf[3] + buf[4] * 256) + ")"; 
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);

                return payloadNode;
            }
            //路径记录命令
            private static TreeNode explainRouteRecordCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("网络层负载：命令帧");

                TreeNode node = null;
                String strTmp = "";

                strTmp = "命令标识：" + GetCmdName(buf[0]) + "(0x" + buf[0].ToString("X2") + ")";
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "广播ID：" + buf[1].ToString("X2");
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "层次号：" + buf[2].ToString();
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "时隙号：" + (buf[3] + buf[4] * 256).ToString("X4") + " (" + (buf[3] + buf[4] * 256) + ")"; 
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "网络规模：" + (buf[5] + buf[6] * 256).ToString("X4") + " (" + (buf[5] + buf[6] * 256) + ")"; 
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                int dstCount = buf[7];
                int index = 8;
                strTmp = "目标地址数：" + dstCount.ToString();
                {
                    node = new TreeNode(strTmp);
                    // 目标地址列表
                    for (int i = 0; i < dstCount; i++)
                    {
                        strTmp = "地址" + (i + 1) + "：" + Util.GetStringHexFromByte(buf, index, LongAddrSize, "", true);
                        node.Nodes.Add(new TreeNode(strTmp));
                    }
                    index += LongAddrSize;
                    payloadNode.Nodes.Add(node);
                }
                int relayCount = buf[index++];
                strTmp = "中继深度：" + relayCount.ToString();
                {
                    node = new TreeNode(strTmp);
                    // 中继场强列表
                    for (int i = 0; i < relayCount; i++)
                    {
                        strTmp = "第" + (i + 1) + "跳：";
                        strTmp += Util.GetStringHexFromByte(buf, index, LongAddrSize, "", true);
                        index += LongAddrSize;
                        strTmp += "(" + buf[index] + ")";
                        index += 1;
                        node.Nodes.Add(new TreeNode(strTmp));
                        
                    }
                    payloadNode.Nodes.Add(node);
                }

                return payloadNode;
            }
        }
        #endregion

        #region Aps层解析
        
        public static readonly String[] ApsFrameTypeTbl = new String[6]
        {
            "确认/否认帧",
            "命令帧",
            "数据转发帧",
            "上报帧",
            "广播业务帧",
            "测试帧",
        };
        private static readonly String[] ProtolTbl = new String[3]
        {
            "透传",
            "97",
            "07"
        };
        private static readonly String[] BaudRateTbl = new String[5]
        {
            "自适应",
            "1200",
            "2400",
            "4800",
            "9600"
        };
        private static readonly String[] ErrorStatusTbl = new String[]
        {
            "未知错误",
            "无效帧类型",
            "无效帧数据内容",
            "无效命令标识",
            "校验错误",
            "电表无应答",
            "采集器无电表档案",
        };
        private static readonly String[] RfSendPowerTbl = new String[4]
        {
            "最高",
            "次高",
            "次低",
            "最低"
        };
        
        public class ApsExplain             //Aps层解析
        {
            //命令下行
            private static readonly CmdExplain[] NwkCmdTbl_0 = new CmdExplain[4]
            {
                new CmdExplain( 0x04, "读取配置", Color.Gray, new ExplainCallback(explainReadConfigCmd)),
                new CmdExplain( 0x05, "设备重启命令", Color.DarkRed, new ExplainCallback(explainDeviceRebootCmd)),
                new CmdExplain( 0x06, "文件传输-下行", Color.Violet, new ExplainCallback(explainFileTransferDownlink)),
                new CmdExplain( 0x08, "收集从节点附属节点", Color.SteelBlue, new ExplainCallback(explainGatherSubNodeCmd)),
            };
            //命令上行
            private static readonly CmdExplain[] NwkCmdTbl_1 = new CmdExplain[3]
            {
                new CmdExplain( 0x04, "读取配置应答", Color.Gray, new ExplainCallback(explainReadConfigResponse)),
                // 设备重启命令的应答帧为 确认/否认帧
                new CmdExplain( 0x06, "文件传输-上行", Color.Violet, new ExplainCallback(explainFileTransferUplink)),
                new CmdExplain( 0x08, "收集从节点附属节点应答", Color.SteelBlue, new ExplainCallback(explainGatherSubNodeResponse)),
            };

            private static TreeNode ExplainCmdFrame(bool isDirUp, byte cmdId, byte[] buf)
            {
                CmdExplain[] NwkCmdTbl = null;
                ExplainCallback callback = null;
                TreeNode payloadNode = null;

                NwkCmdTbl = (false == isDirUp) ? NwkCmdTbl_0 : NwkCmdTbl_1;
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

                return payloadNode;
            }
            public static String GetCmdName(bool isDirUp, byte cmdId)
            {
                string strName = "";
                CmdExplain[] NwkCmdTbl = null;

                NwkCmdTbl = (false == isDirUp) ? NwkCmdTbl_0 : NwkCmdTbl_1;
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

                foreach (CmdExplain cmd in NwkCmdTbl_0)
                {
                    if (cmd.CmdId == cmdId)
                    {
                        cmdColor = cmd.CmdColor;
                    }
                }
                return cmdColor;
            }

            public static TreeNode GetTreeNode(FrameFormat rxFrame, Color fgColor)
            {
                TreeNode apsNode = new TreeNode("应用层");
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
                    switch ((ApsFrameType)rxFrame.Aps.CtrlWord.FrameType)
                    {
                        case ApsFrameType.Ack:
                            strTmp = "帧类型      ：确认/否认帧(" + rxFrame.Aps.CtrlWord.FrameType.ToString("X2") + ")";
                            payloadNode = explainAckFrame(rxFrame.Aps.Payload);
                            break;
                        case ApsFrameType.Cmd:
                            strTmp = "帧类型      ：命令帧(" + rxFrame.Aps.CtrlWord.FrameType.ToString("X2") + ")";
                            payloadNode = ExplainCmdFrame(rxFrame.Aps.CtrlWord.DirFlag, rxFrame.Aps.Payload[0], rxFrame.Aps.Payload);
                            break;
                        case ApsFrameType.DataTransfer:
                            strTmp = "帧类型      ：数据转发帧(" + rxFrame.Aps.CtrlWord.FrameType.ToString("X2") + ")";
                            payloadNode = explainDataTransferFrame(rxFrame.Aps.Payload);
                            break;
                        case ApsFrameType.Report:
                            strTmp = "帧类型      ：上报帧(" + rxFrame.Aps.CtrlWord.FrameType.ToString("X2") + ")";
                            payloadNode = explainReportFrame(rxFrame.Aps.Payload);
                            break;
                        case ApsFrameType.BroadCast:
                            strTmp = "帧类型      ：广播业务帧(" + rxFrame.Aps.CtrlWord.FrameType.ToString("X2") + ")";
                            payloadNode = explainBroadCastFrame(rxFrame.Aps.Payload);
                            break;
                        case ApsFrameType.Test:
                            strTmp = "帧类型      ：测试帧(" + rxFrame.Aps.CtrlWord.FrameType.ToString("X2") + ")";
                            payloadNode = explainTestFrame(rxFrame.Aps.Payload);
                            break;
                        default:
                            payloadNode = new TreeNode("网络层负载：无法识别");
                            break;
                    }
                    node.Nodes.Add(new TreeNode(strTmp));

                    strTmp = "扩展信息标识：";
                    strTmp += rxFrame.Aps.CtrlWord.ExtendFlag == true ? "有" : "无";
                    node.Nodes.Add(new TreeNode(strTmp));
                    strTmp = "传输方向标识：";
                    strTmp += rxFrame.Aps.CtrlWord.DirFlag == true ? "子节点上行" : "中心节点下行";
                    node.Nodes.Add(new TreeNode(strTmp));
 
                }
                apsNode.Nodes.Add(node);

                node = new TreeNode("帧序号  ：" + rxFrame.Aps.FrameSn.ToString("X2"));
                apsNode.Nodes.Add(node);

                if (true == rxFrame.Aps.CtrlWord.ExtendFlag)
                {
                    node = new TreeNode("扩展信息域：");
                    strTmp = "扩展域长度：" + rxFrame.Aps.Extend.Length.ToString();
                    node.Nodes.Add(strTmp);
                    if (rxFrame.Aps.Extend.Length >= 2)
                    {
                        strTmp = "厂家标识  ：" + rxFrame.Aps.Extend.OemInfo.ToString("X4");
                        strTmp += " (" + Convert.ToChar(rxFrame.Aps.Extend.OemInfo >> 8)
                                    + Convert.ToChar(rxFrame.Aps.Extend.OemInfo & 0x00FF) + ")";
                        node.Nodes.Add(strTmp);
                        strTmp = "扩展域数据：" + "(" + (rxFrame.Aps.Extend.Length - 2) + "byte)";
                        node.Nodes.Add(strTmp);
                    }

                    apsNode.Nodes.Add(node);
                }

                apsNode.Nodes.Add(payloadNode);

                return apsNode;
            }

            // 确认/否认帧
            private static TreeNode explainAckFrame(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("应用层负载：确认/否认帧");

                TreeNode node = null;
                String strTmp = "";

                strTmp = "确认/否认标识：" + (buf[0] == 0x00 ? "否认" : "确认");
                strTmp +=  "(" + buf[0].ToString("X2") + ")";
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);

                if (0x00 == buf[0])
                {
                    strTmp = "应用层帧类型：" + ApsFrameTypeTbl[buf[1]];
                    strTmp += "(" + buf[1].ToString("X2") + ")";
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                    strTmp = "错误状态：" + ErrorStatusTbl[buf[2]];
                    strTmp += "(" + buf[2].ToString("X2") + ")";
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                }

                return payloadNode;
            }
            // 数据转发帧
            private static TreeNode explainDataTransferFrame(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("应用层负载：数据转发帧");

                TreeNode node = null;
                String strTmp = "";

                int iStart = 0;
                bool isFindStart = false;
                for (iStart = 0; iStart < buf.Length; iStart++)
                {
                    if (buf[iStart] == 0x68 && buf[iStart + 7] == 0x68)
                    {
                        isFindStart = true;
                        break;
                    }
                }
                if (true == isFindStart && buf[iStart + 8] == 0x11)
                {
                    strTmp = "原始报文：" + "抄表(" + buf.Length + "byte)";
                    node = new TreeNode(strTmp);
                    {
                        strTmp = "电表地址：" + Util.GetStringHexFromByte(buf, iStart + 1, 6, "", true);
                        node.Nodes.Add(new TreeNode(strTmp));
                    }
                }
                else
                {
                    strTmp = "原始报文：" + "抄表应答(" + buf.Length + "byte)";
                    node = new TreeNode(strTmp);
                    {
                        strTmp = "电表地址：" + Util.GetStringHexFromByte(buf, iStart + 1, 6, "", true);
                        node.Nodes.Add(new TreeNode(strTmp));
                        strTmp = "电表读数：";
                        strTmp += (buf[iStart + 17] - 0x33).ToString("X2");
                        strTmp += (buf[iStart + 16] - 0x33).ToString("X2");
                        strTmp += (buf[iStart + 15] - 0x33).ToString("X2") + ".";
                        strTmp += (buf[iStart + 14] - 0x33).ToString("X2") + "kWh";
                        node.Nodes.Add(new TreeNode(strTmp));
                    }
                }
                
                payloadNode.Nodes.Add(node);

                return payloadNode;
            }
            // 上报帧
            private static TreeNode explainReportFrame(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("应用层负载：上报帧");

                TreeNode node = null;
                String strTmp = "";

                strTmp = "上报标识：" + (buf[0] == 0x00 ? "事件上报" : "未知");
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                if(buf[0] == 0x00)
                {
                    strTmp = "事件上报类型：" + (buf[1] == 0x00 ? "电能表事件" : "从节点事件");
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                    strTmp = "事件序号    ：" + buf[2];
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                    strTmp = "事件载荷长度：" + buf[3];
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                    strTmp = "事件载荷";
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                }
                

                return payloadNode;
            }
            // 广播业务帧
            private static TreeNode explainBroadCastFrame(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("应用层负载：广播业务帧");

                TreeNode node = null;
                String strTmp = "";

                if (buf[0] == 0x00)
                {
                    strTmp = "广播标识：广播校时" + "(" + buf[0].ToString("X2") + ")";
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);

                    strTmp = "广播帧ID：" + buf[1].ToString("X2");
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                    strTmp = "层次号  ：" + buf[2].ToString();
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                    strTmp = "时隙号  ：" + (buf[3] + buf[4] * 256).ToString("X4") + " (" + (buf[3] + buf[4] * 256) + ")";
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                    strTmp = "全网最大延时：" + (buf[5] + buf[6] << 8
                            + buf[7] << 16 + buf[8] << 24).ToString() + "ms";
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                    strTmp = "校时报文：" + "(" + (buf.Length - 9) + "byte)";
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                }
                else if (buf[0] == 0x01)
                {
                    strTmp = "广播标识：命令搜表" + "(" + buf[0].ToString("X2") + ")";
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);

                    strTmp = "广播帧ID：" + buf[1].ToString("X2");
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                    strTmp = "电表地址数：" + buf[2].ToString();
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                    // 电表地址列表
                    for (int i = 0, index = 3; i < buf[2]; i++)
                    {
                        strTmp = "地址" + (i + 1).ToString() + "：" + Util.GetStringHexFromByte(buf, index, 6, "", true);
                        node = new TreeNode(strTmp);
                        index += 6;

                        payloadNode.Nodes.Add(node);
                    }
                }

                return payloadNode;
            }
            // 测试帧
            private static TreeNode explainTestFrame(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("应用层负载：测试帧");

                TreeNode node = null;
                String strTmp = "";

                if( buf[0] == 0x00 )
                {
                    strTmp = "测试命令标识：发送测试" + "(" + buf[0].ToString("X2") + ")";
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);

                    strTmp = "信道索引  ：" + buf[1].ToString();
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                    strTmp = "测试码流值：" + buf[2].ToString();
                    if(0x00 == buf[2])
                    {
                        strTmp = "比特0序列" + "(" + buf[2].ToString("X2") + ")"; 
                    }
                    else if (0x01 == buf[2])
                    {
                        strTmp = "比特1序列" + "(" + buf[2].ToString("X2") + ")"; 
                    }
                    else if (0x04 == buf[2])
                    {
                        strTmp = "比特0和1交替序列" + "(" + buf[2].ToString("X2") + ")"; 
                    }
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                    strTmp = "持续发射时间：" + buf[3].ToString() + "s";
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                }
                else if (buf[0] == 0x01)
                {
                    strTmp = "测试命令标识：接收测试" + "(" + buf[0].ToString("X2") + ")";
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);

                    strTmp = "波特率  ：" + BaudRateTbl[(buf[1])];
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                    strTmp = "测试报文：" + "(" + (buf.Length - 9) + "byte)";
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                }

                return payloadNode;
            }

            //-----------------------   Aps 命令帧     -------------------
            //读取配置
            private static TreeNode explainReadConfigCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("应用层负载 ：命令帧");

                TreeNode node = null;
                String strTmp = "";

                strTmp = "命令标识：" + GetCmdName(false, buf[0]) + "(0x" + buf[0].ToString("X2") + ")";
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);

                return payloadNode;
            }

            //读取配置应答
            private static TreeNode explainReadConfigResponse(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("应用层负载 ：命令帧");

                TreeNode node = null;
                String strTmp = "";
                int index = 0;

                strTmp = "命令标识：" + GetCmdName(true, buf[index]) + "(0x" + buf[index++].ToString("X2") + ")";
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "设备出厂地址：" + Util.GetStringHexFromByte(buf, index, LongAddrSize, "", true);
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                index += 6;
                strTmp = "节点类型：";
                switch (buf[index++])
                {
                    case 0x00: strTmp += "中心节点"; break;
                    case 0x01: strTmp += "I型采集器"; break;
                    case 0x02: strTmp += "II型采集器"; break;
                    case 0x03: strTmp += "电表"; break;
                    default: strTmp += "未知"; break;
                }
                node = new TreeNode(strTmp);
                strTmp = "PanID ：" + buf[index + 1].ToString("X2") + buf[index].ToString("X2");
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                index += 2;
                strTmp = "短地址：" + buf[index + 1].ToString("X2") + buf[index].ToString("X2");
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                index += 2;
                strTmp = "厂家标识：" + Convert.ToChar(buf[index + 1]) + Convert.ToChar(buf[index]);
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                index += 2;
                strTmp = "硬件版本：" + buf[index + 1].ToString() + "." + buf[index].ToString("X2");
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                index += 2;
                strTmp = "软件版本：" + buf[index + 2].ToString() + "." + buf[index + 1].ToString("X2") + buf[index].ToString("X2");
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                index += 3;
                strTmp = "发射功率：" + RfSendPowerTbl[buf[index++]];
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "场强门限：" + buf[index++].ToString();
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "信道组：" + buf[index++].ToString();
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "层次号：" + buf[index++].ToString();
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "时隙号：" + (buf[index] + buf[index + 1] << 8).ToString("X4") + " (" + (buf[index] + buf[index + 1] << 8) + ")";
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                index += 2;
                strTmp = "网络规模：" + (buf[index] + buf[index + 1] << 8).ToString("X4") + " (" + (buf[index] + buf[index + 1] << 8) + ")";
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                index += 2;
                strTmp = "路径数：" + buf[index].ToString();
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                // 路径列表
                int routeCount = buf[index];
                int relayCount;
                for (int i = 0; i < routeCount; i++)
                {
                    relayCount = buf[index++];
                    strTmp = "路径" + (i + 1) + "中继数：" + relayCount;
                    node = new TreeNode(strTmp);
                    for (int j = 0; j < relayCount; j++)
                    {
                        strTmp = "中继" + (j + 1) + "：";
                        strTmp += Util.GetStringHexFromByte(buf, index, 2, "", true);
                        node.Nodes.Add(new TreeNode(strTmp));
                        index += 2;
                    }
                    payloadNode.Nodes.Add(node);
                }

                return payloadNode;
            }

            //设备重启
            private static TreeNode explainDeviceRebootCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("应用层负载 ：命令帧");

                TreeNode node = null;
                String strTmp = "";

                strTmp = "命令标识：" + GetCmdName(false, buf[0]) + "(0x" + buf[0].ToString("X2") + ")";
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);

                return payloadNode;
            }
            //文件传输 - 下行
            private static TreeNode explainFileTransferDownlink(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("应用层负载 ：命令帧");

                TreeNode node = null;
                String strTmp = "";

                strTmp = "命令标识：" + GetCmdName(false, buf[0]) + "(0x" + buf[0].ToString("X2") + ")";
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);

                strTmp = "暂不解析";
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);

                return payloadNode;
            }
            //文件传输 - 上行
            private static TreeNode explainFileTransferUplink(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("应用层负载 ：命令帧");

                TreeNode node = null;
                String strTmp = "";

                strTmp = "命令标识：" + GetCmdName(true, buf[0]) + "(0x" + buf[0].ToString("X2") + ")";
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);

                strTmp = "暂不解析";
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);

                return payloadNode;
            }

            //收集从节点附属节点
            private static TreeNode explainGatherSubNodeCmd(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("应用层负载 ：命令帧");

                TreeNode node = null;
                String strTmp = "";

                strTmp = "命令标识  ：" + GetCmdName(false, buf[0]) + "(0x" + buf[0].ToString("X2") + ")";
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);

                strTmp = "页序号  ：" + buf[1].ToString() ;
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);

                return payloadNode;
            }

            //收集从节点附属节点应答
            private static TreeNode explainGatherSubNodeResponse(byte[] buf)
            {
                TreeNode payloadNode = new TreeNode("应用层负载 ：命令帧");

                TreeNode node = null;
                String strTmp = "";

                strTmp = "命令标识  ：" + GetCmdName(true, buf[0]) + "(0x" + buf[0].ToString("X2") + ")";
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "当前页序号：" + (buf[1] & 0x0F).ToString();
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "总页数    ：" + (buf[1] >> 4).ToString();
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                strTmp = "附属节点总数：" + buf[2].ToString();
                node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);

                int count = buf[2];
                int index = 3;
                for (int i = 0; i < count; i++)
                {
                    strTmp = "节点" + (i + 1).ToString("D2") + "：" + Util.GetStringHexFromByte(buf, index, 6, "", true);
                    index += 6;
                    strTmp += " (" + BaudRateTbl[(buf[index] & 0x0F)] + "| " + ProtolTbl[(buf[index] >> 4)] + ")"; 
                    index += 1;
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                }

                return payloadNode;
            }
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
