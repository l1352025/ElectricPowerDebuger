 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace ElectricPowerDebuger.Protocol
{
    using ElectricPowerDebuger.Common;
    class ProtoLocal_North
    {
        public const byte FrameHeader = 0x68;           // 帧头
        public const byte FrameTail = 0x16;             // 帧尾
        public const byte FrameFixedLen = 15;           // 起始字符,长度(2),控制域,信息域(6)、AFN、Fn(2)、校验和,结束字符
        public const byte LongAddrSize = 6;             // 地址的长度

        #region 帧格式定义
        // 通信报文格式
        public struct FrameFormat
        {
            public byte Header;                         // 帧头
            public int Length;                          // 长度
            public CtrlField CtrlWord;                  // 控制字
            public InfoFieldDown InfoDown;              // 信息域-下行 6 byte
            public InfoFieldUp InfoUp;                      // 信息域-上行 6 byte
            public byte[] SrcAddr;                      // 源地址
            public byte[] RouteAddrs;                       // 中继地址列表 N * 6 byte
            public byte[] DstAddr;                      // 目标地址
            public byte Afn;                             // 功能码
            public UInt16 DataId;                       // 数据标识 DT2DT1 = (Fn/8 << 8) + (1 << Fn%8)
            public byte[] DataBuf;                      // 数据域
            public byte Crc8;                           // Crc8校验
            public byte Tail;                           // 帧尾

            public string ErrorInfo;                    // 帧错误信息
        };

        // 控制域
        public struct CtrlField
        {
            public bool DirFlag;         // 传输方向 bit7
            public bool StartFlag;       // 启动标识 bit6
            public byte CommType;        // 通信方式 bit5-0

            public byte All;
        }

        // 信息域下行
        public struct InfoFieldDown
        {
            public bool B1_RouteFlag;       // 路由标识 bit0
            public bool B1_SubNodeFlag;     // 附属节点标识 bit1
            public bool B1_CommModuleFlag;  // 通信模块标识 bit2
            public bool B1_ConflictFlag;    // 冲突检测标识 bit3
            public byte B1_RelayLevel;      // 中继级别 bit7-4
            public byte B1_All;
            public byte B2_Chanel;          // 信道标识 bit3-0
            public byte B2_ErrCrcCode;      // 纠错编码 bit7-4
            public byte B2_All;
            public byte B3_ExpectAckLen;    // 预计应答字节数 
            public UInt16 B4B5_DataRate;    // 通信速率 bit14-0
            public bool B4B5_RateUnitFlag;  // 速率单位标识 bit15  0 - bit/s  1 - Kbit/s
            public UInt16 B4B5_All;
            public byte B6_Fsn;             // 帧序号 
        }

        // 信息域上行
        public struct InfoFieldUp
        {
            public bool B1_RouteFlag;       // 路由标识 bit0
            public bool B1_SubNodeFlag;     // 附属节点标识 bit1
            public bool B1_CommModuleFlag;  // 通信模块标识 bit2
            public bool B1_ConflictFlag;    // 冲突检测标识 bit3
            public byte B1_RelayLevel;      // 中继级别 bit7-4
            public byte B1_All;
            public byte B2_Chanel;          // 信道标识 bit3-0
            public byte B2_ErrCrcCode;      // 纠错编码 bit7-4
            public byte B2_All;
            public byte B3_ActualPhase;     // 实际相线标识 bit3-0
            public byte B3_AmmeterFuture;   // 电表通用特征 bit7-4
            public byte B3_All;
            public byte B4_EndCmdSignalQlty;    // 末级命令信号品质 bit3-0
            public byte B4_EndAckSignalQlty;    // 末级应答信号品质 bit7-4
            public byte B4_All;
            public bool B5_EventReportFlag; // 事件上报标识 bit0 
            public byte B6_Fsn;             // 帧序号 
        }
        #endregion

        // 帧解析表配置
        public delegate TreeNode ExplainCallback(FrameFormat frame);
        public struct CmdExplain
        {
            public byte Afn;
            public String AfnName;
            public byte Fn;
            public String FnName;
            public Color CmdColor;
            public ExplainCallback CmdExplainFunc;

            public CmdExplain(byte afn, String afnName, byte fn, String fnName, Color color, ExplainCallback callback)
            {
                Afn = afn;
                AfnName = afnName;
                Fn = fn;
                FnName = fnName;
                CmdColor = color;
                CmdExplainFunc = callback;
            }
        }

        private static readonly CmdExplain[] FrameExplainTbl = new CmdExplain[]
        {
            new CmdExplain(0x00, "确认/否认",   1, "确认",                        Color.DarkGreen, ExplainAckNack_Ack),
            new CmdExplain(0x00, "确认/否认",   2, "否认",                        Color.Red, ExplainAckNack_Nack),

            new CmdExplain(0x01, "初始化",     1, "硬件初始化",                  Color.Black, ExplainInit_Hardware),
            new CmdExplain(0x01, "初始化",     2, "参数区初始化",                Color.Black, ExplainInit_ParamFiled),
            new CmdExplain(0x01, "初始化",     3, "数据区初始化",                Color.Black, ExplainInit_DataFiled),

            new CmdExplain(0x02, "数据转发",    1, "数据转发",                   Color.Brown, ExplainDataTransfer_Transfer),
            new CmdExplain(0x02, "数据转发",    2, "读取计量箱相关事件",         Color.Brown, ExplainDataTransfer_ReadAmeterBoxOpenEvent),  /* 北网扩展 */

            new CmdExplain(0x03, "查询数据",    1, "查询厂商代码和版本信息",      Color.Magenta, ExplainDataQuery_OemAndVer),
            new CmdExplain(0x03, "查询数据",    2, "查询噪声值",                  Color.Magenta, ExplainDataQuery_NoiseValue),
            new CmdExplain(0x03, "查询数据",    3, "查询从节点侦听信息",          Color.Magenta, ExplainDataQuery_SubNodeListenInfo),
            new CmdExplain(0x03, "查询数据",    4, "查询主节点地址",              Color.Magenta, ExplainDataQuery_MainNodeAddr),
            new CmdExplain(0x03, "查询数据",    5, "查询主节点状态字和通信速率",  Color.Magenta, ExplainDataQuery_MainNodeStatusAndDataRate),
            new CmdExplain(0x03, "查询数据",    6, "查询主节点干扰状态",          Color.Magenta, ExplainDataQuery_MainNodeDisturbedState),
            new CmdExplain(0x03, "查询数据",    7, "查询从节点监控最大超时时间",  Color.Magenta, ExplainDataQuery_SubNodeListenMaxTimeoutTime),
            new CmdExplain(0x03, "查询数据",    8, "查询无线通信参数",            Color.Magenta, ExplainDataQuery_WirelessCommParam),
            new CmdExplain(0x03, "查询数据",    9, "查询通信延时相关的广播时长",  Color.Magenta, ExplainDataQuery_BroadcastDelayTime),
            new CmdExplain(0x03, "查询数据",    10, "查询本地通信模块运行模式信息",   Color.Magenta, ExplainDataQuery_LocalCommModuleRunningMode),
            new CmdExplain(0x03, "查询数据",    11, "查询本地通信模块AFN索引",    Color.Magenta, ExplainDataQuery_LocalCommModuleAfnIndex),
            new CmdExplain(0x03, "查询数据",    100, "查询场强门限",              Color.Magenta, ExplainDataQuery_RssiThreshold),

            new CmdExplain(0x04, "链路接口测试",  1, "发送测试",                   Color.Black, ExplainDataLinkTest_SendTest),
            new CmdExplain(0x04, "链路接口测试",  2, "从节点点名",                 Color.Black, ExplainDataLinkTest_SubNodeCheckName),
            new CmdExplain(0x04, "链路接口测试",  3, "本地通信模块报文通信测试",   Color.Black, ExplainDataLinkTest_DatagramCommTest),
            new CmdExplain(0x04, "链路接口测试",  4, "发射功率测试",               Color.Black, ExplainDataLinkTest_SendPowerTest),  /* 北网扩展 */

            new CmdExplain(0x05, "控制命令",    1, "设置主节点地址",              Color.Black, ExplainCtrlCmd_SetMainNodeAddr),
            new CmdExplain(0x05, "控制命令",    2, "允许/禁止从节点上报",         Color.Black, ExplainCtrlCmd_EnableOrDisableSubNodeReport),
            new CmdExplain(0x05, "控制命令",    3, "启动广播",                    Color.Red, ExplainCtrlCmd_StartBroadcast),
            new CmdExplain(0x05, "控制命令",    4, "设置从节点监控最大超时时间",  Color.Black, ExplainCtrlCmd_SetSubNodeMaxTimeoutTime),
            new CmdExplain(0x05, "控制命令",    5, "设置无线通信参数",            Color.Black, ExplainCtrlCmd_SetWirelessCommParam),
            new CmdExplain(0x05, "控制命令",    51, "启动全网感知",               Color.Red, ExplainCtrlCmd_StartWholeNetworkSense),  /* 北网扩展 */
            new CmdExplain(0x05, "控制命令",    100, "设置场强门限",              Color.Black, ExplainCtrlCmd_SetRssiThreshold),
            new CmdExplain(0x05, "控制命令",    101, "设置中心节点时间",          Color.Black, ExplainCtrlCmd_SetMainNodeClock),

            new CmdExplain(0x06, "主动上报",    1, "上报从节点信息",                Color.Blue, ExplainAutoReport_SubNodeInfo),
            new CmdExplain(0x06, "主动上报",    2, "上报抄读数据",                  Color.Blue, ExplainAutoReport_ReadData),
            new CmdExplain(0x06, "主动上报",    3, "上报路由工况变动信息",          Color.Blue, ExplainAutoReport_RouteWorkStateChanges),
            new CmdExplain(0x06, "主动上报",    4, "上报从节点信息及设备类型",      Color.Blue, ExplainAutoReport_SubNodeInfoAndDevType),
            new CmdExplain(0x06, "主动上报",    5, "上报从节点事件",                Color.Blue, ExplainAutoReport_SubNodeEvent),
            new CmdExplain(0x06, "主动上报",    20, "上报计量箱相关事件",           Color.Blue, ExplainAutoReport_MeteringBoxOpenEvent),  /* 北网扩展 */

            new CmdExplain(0x10, "路由查询",    1, "查询从节点数量",                 Color.Magenta, ExplainRouteQuery_SubNodeCount),
            new CmdExplain(0x10, "路由查询",    2, "查询从节点信息",                 Color.Magenta, ExplainRouteQuery_SubNodeInfo),
            new CmdExplain(0x10, "路由查询",    3, "查询从节点的上一级路由信息",       Color.Magenta, ExplainRouteQuery_SubNodeRelayInfo),
            new CmdExplain(0x10, "路由查询",    4, "查询路由运行状态",               Color.Magenta, ExplainRouteQuery_RouteRunningState),
            new CmdExplain(0x10, "路由查询",    5, "查询未抄读成功的从节点信息",     Color.Magenta, ExplainRouteQuery_ReadFailedSubNodeInfo),
            new CmdExplain(0x10, "路由查询",    6, "查询主动注册的从节点信息",       Color.Magenta, ExplainRouteQuery_AutoRegSubNodeInfo),
            new CmdExplain(0x10, "路由查询",    7, "查询无线从节点的中继路由信息",   Color.Magenta, ExplainRouteQuery_WirelessSubNodeRouteInfo),  /* 北网扩展 */
            new CmdExplain(0x10, "路由查询",    50, "查询全网感知状态",              Color.Magenta, ExplainRouteQuery_WholeNetworkSenseStatus),  /* 北网扩展 */
            new CmdExplain(0x10, "路由查询",    51, "查询在网状态更新信息",          Color.Magenta, ExplainRouteQuery_OnlineStatusUpdateInfo),  /* 北网扩展 */
            new CmdExplain(0x10, "路由查询",    100, "查询网络规模",                 Color.Magenta, ExplainRouteQuery_NetworkScale),
            new CmdExplain(0x10, "路由查询",    101, "查询微功率无线从节点信息",     Color.Magenta, ExplainRouteQuery_MicroWirelessSubNodeInfo),
              
            new CmdExplain(0x11, "路由设置",    1, "添加从节点",                    Color.Black, ExplainRouteSet_AddSubNode),
            new CmdExplain(0x11, "路由设置",    2, "删除从节点",                    Color.Black, ExplainRouteSet_DelSubNode),
            new CmdExplain(0x11, "路由设置",    3, "设置从节点固定中继路径",        Color.Black, ExplainRouteSet_SetSubNodeRelay),
            new CmdExplain(0x11, "路由设置",    4, "设置路由工作模式",              Color.Black, ExplainRouteSet_SetRouteWorkMode),
            new CmdExplain(0x11, "路由设置",    5, "激活从节点主动注册",            Color.Black, ExplainRouteSet_ActiveSubNodeAutoRegister),
            new CmdExplain(0x11, "路由设置",    6, "禁止从节点主动注册",            Color.Black, ExplainRouteSet_DisableSubNodeAutoRegister),
            new CmdExplain(0x11, "路由设置",    100, "设置网络规模",                Color.Black, ExplainRouteSet_SetNetworkScale),
            new CmdExplain(0x11, "路由设置",    101, "启动网络维护进程",            Color.Red, ExplainRouteSet_StartNetworkMaintain),
            new CmdExplain(0x11, "路由设置",    102, "启动组网",                    Color.Red, ExplainRouteSet_StartBuildingNetwork),

            new CmdExplain(0x12, "路由控制",    1, "路由重启",                      Color.Black, ExplainRouteCtrl_Reboot),
            new CmdExplain(0x12, "路由控制",    2, "路由暂停",                      Color.Black, ExplainRouteCtrl_Pause),
            new CmdExplain(0x12, "路由控制",    3, "路由恢复",                      Color.Black, ExplainRouteCtrl_Resume),

            new CmdExplain(0x13, "路由数据转发",    1, "路由数据转发",               Color.Brown, ExplainRouteDataTransfer_Transfer),

            new CmdExplain(0x14, "路由数据抄读",    1, "路由请求抄读内容",             Color.Black, ExplainRouteDataRead_RequestReadContent),
            new CmdExplain(0x14, "路由数据抄读",    2, "路由请求集中器时钟",           Color.Black, ExplainRouteDataRead_RequestConcentratorClock),
            new CmdExplain(0x14, "路由数据抄读",    3, "请求依通信延时修正通信数据",   Color.Black, ExplainRouteDataRead_RequstAdjustCommData),

            new CmdExplain(0x15, "文件传输",    1, "文件传输",                      Color.Black, ExplainFileTransfer_TransferModeOne),

            new CmdExplain(0x20, "水表上报",    1, "启动抄收水表数据",              Color.Red, ExplainWaterMeterReport_ReadWaterMeterDataStart),        /* 北网扩展 */
            new CmdExplain(0x20, "水表上报",    2, "停止抄收水表数据",              Color.Red, ExplainWaterMeterReport_ReadWaterMeterDataStop),         /* 北网扩展 */
            new CmdExplain(0x20, "水表上报",    3, "主节点上报水表数据",            Color.Blue, ExplainWaterMeterReport_MainNodeReportWaterMeterData),  /* 北网扩展 */
            new CmdExplain(0x20, "水表上报",    4, "主节点上报水表数据完成",        Color.Blue, ExplainWaterMeterReport_MainNodeReportWaterMeterDataComplete),  /* 北网扩展 */

            // 桑锐内部指令
            new CmdExplain(0xF0, "内部调试",    1, "按类型读取日志",                Color.Black, ExplainInnerTest_ReadLogByType),
            new CmdExplain(0xF0, "内部调试",    20, "设置广播维护开关",             Color.Black, ExplainInnerTest_SetBroadcastMaintain),
            new CmdExplain(0xF0, "内部调试",    30, "读取子节点参数信息",           Color.Black, ExplainInnerTest_ReadSubNodeParamsInfo),
            new CmdExplain(0xF0, "内部调试",    40, "读取子节点概要信息",           Color.Black, ExplainInnerTest_ReadSubNodeSummaryInfo),
            new CmdExplain(0xF0, "内部调试",    50, "读取中心节点邻居表",           Color.Black, ExplainInnerTest_ReadMainNodeNeighborTbl),
        };

        #region 协议帧提取

        // 协议帧提取
        public static FrameFormat ExplainRxPacket(byte[] rxBuf)
        {
            FrameFormat rxData = new FrameFormat();
        
            try
            {
                int index = 0, relayCnt;

                if (rxBuf.Length < index + FrameFixedLen) throw new Exception("长度错误");

                //帧头
                rxData.Header = rxBuf[index++]; 
                //长度        
                rxData.Length = rxBuf[index] + rxBuf[index + 1] * 256;
                index += 2;

                if (rxData.Header != 0x68) throw new Exception("帧头错误");
                if (rxData.Length > rxBuf.Length) throw new Exception("长度错误");

                //控制域
                rxData.CtrlWord.All = rxBuf[index];
                rxData.CtrlWord.DirFlag = (rxBuf[index] & 0x80) > 0 ? true : false;
                rxData.CtrlWord.StartFlag = (rxBuf[index] & 0x40) > 0 ? true : false;
                rxData.CtrlWord.CommType = (byte)(rxBuf[index] & 0x1F);
                index++;
                //信息域
                if(rxData.CtrlWord.DirFlag == false)
                {
                    rxData.InfoDown.B1_All = rxBuf[index];
                    rxData.InfoDown.B1_RouteFlag = (rxBuf[index] & 0x01) > 0 ? true : false;
                    rxData.InfoDown.B1_SubNodeFlag = (rxBuf[index] & 0x02) > 0 ? true : false;
                    rxData.InfoDown.B1_CommModuleFlag = (rxBuf[index] & 0x04) > 0 ? true : false;
                    rxData.InfoDown.B1_ConflictFlag = (rxBuf[index] & 0x08) > 0 ? true : false;
                    rxData.InfoDown.B1_RelayLevel = (byte)(rxBuf[index] >> 4);
                    rxData.InfoDown.B2_All = rxBuf[index + 1];
                    rxData.InfoDown.B2_Chanel = (byte)(rxBuf[index + 1] & 0x0F);
                    rxData.InfoDown.B2_ErrCrcCode = (byte)(rxBuf[index + 1] >> 4);
                    rxData.InfoDown.B3_ExpectAckLen = rxBuf[index + 2]; 
                    rxData.InfoDown.B4B5_All = (UInt16)(rxBuf[index + 3] + rxBuf[index + 4] * 256);
                    rxData.InfoDown.B4B5_DataRate = (UInt16)((rxBuf[index + 3] + rxBuf[index + 4] * 256) & 0x7FFF);
                    rxData.InfoDown.B4B5_RateUnitFlag = (rxBuf[index + 4] & 0x80) > 0 ? true : false;
                    rxData.InfoDown.B6_Fsn = rxBuf[index + 5];

                    relayCnt = rxData.InfoDown.B1_RelayLevel;
                }
                else
                {
                    rxData.InfoUp.B1_All = rxBuf[index];
                    rxData.InfoUp.B1_RouteFlag = (rxBuf[index] & 0x01) > 0 ? true : false;
                    rxData.InfoUp.B1_SubNodeFlag = (rxBuf[index] & 0x02) > 0 ? true : false;
                    rxData.InfoUp.B1_CommModuleFlag = (rxBuf[index] & 0x04) > 0 ? true : false;
                    rxData.InfoUp.B1_ConflictFlag = (rxBuf[index] & 0x08) > 0 ? true : false;
                    rxData.InfoUp.B1_RelayLevel = (byte)(rxBuf[index] >> 4);
                    rxData.InfoUp.B2_All = rxBuf[index + 1];
                    rxData.InfoUp.B2_Chanel = (byte)(rxBuf[index + 1] & 0x0F);
                    rxData.InfoUp.B2_ErrCrcCode = (byte)(rxBuf[index + 1] >> 4);
                    rxData.InfoUp.B3_All = rxBuf[index + 2];
                    rxData.InfoUp.B3_ActualPhase = (byte)(rxBuf[index + 2] & 0x0F);
                    rxData.InfoUp.B3_AmmeterFuture = (byte)(rxBuf[index + 2] >> 4);
                    rxData.InfoUp.B4_All = rxBuf[index + 1];
                    rxData.InfoUp.B4_EndCmdSignalQlty = (byte)(rxBuf[index + 3] & 0x0F);
                    rxData.InfoUp.B4_EndAckSignalQlty = (byte)(rxBuf[index + 3] >> 4);
                    rxData.InfoUp.B5_EventReportFlag = (rxBuf[index + 4] & 0x01) > 0 ? true : false;
                    rxData.InfoUp.B6_Fsn = rxBuf[index + 5];

                    relayCnt = rxData.InfoUp.B1_RelayLevel;
                }
                index += 6;

                //地址域
                if (rxData.InfoUp.B1_CommModuleFlag == true || rxData.InfoDown.B1_CommModuleFlag == true)
                {
                    if (rxBuf.Length < index + (relayCnt + 2) * LongAddrSize + 5) throw new Exception("长度错误");

                    rxData.SrcAddr = new byte[LongAddrSize];
                    Array.Copy(rxBuf, index, rxData.SrcAddr, 0, LongAddrSize);
                    index += LongAddrSize;

                    if (relayCnt > 0)
                    {
                        rxData.RouteAddrs = new byte[LongAddrSize * relayCnt];
                        for (int i = 0; i < relayCnt; i++)
                        {
                            Array.Copy(rxBuf, index, rxData.RouteAddrs, i * LongAddrSize, LongAddrSize);
                            index += LongAddrSize;
                        }
                    }

                    rxData.DstAddr = new byte[LongAddrSize];
                    Array.Copy(rxBuf, index, rxData.DstAddr, 0, LongAddrSize);
                    index += LongAddrSize;
                }
                //AFN
                rxData.Afn = rxBuf[index++];
                //Fn
                rxData.DataId = (UInt16)(rxBuf[index] + rxBuf[index + 1] * 256);
                index += 2;
                //数据区
                rxData.DataBuf = new byte[rxData.Length - index - 2];
                Array.Copy(rxBuf, index, rxData.DataBuf, 0, rxData.DataBuf.Length);
                index += rxData.DataBuf.Length;

                //校验和
                rxData.Crc8 = rxBuf[index++];

                byte chksum = 0;
                for (int i = 3; i < index - 1; i++ )
                {
                    chksum += rxBuf[i];
                }
                if (rxData.Crc8 != chksum) throw new Exception("校验错误");

                //帧尾
                rxData.Tail = rxBuf[index++];
            }
            catch (Exception ex)
            {
                switch (ex.Message)
                {
                    case "帧头错误":
                    case "长度错误":
                        rxData.Length = 0x00;
                        rxData.ErrorInfo = ex.Message;
                        break;

                    case "校验错误":
                        rxData.ErrorInfo = ex.Message;
                        break;

                    default:
                        rxData.Length = 0x00;
                        rxData.ErrorInfo = "数据异常";
                        string msg = "ProtoLocal_North.ExplainRxPacket() Error: " + ex.Message + "\r\n  " + Util.GetStringHexFromBytes(rxBuf, 0, rxBuf.Length, " ");
                        LogHelper.WriteLine("error.log", msg);
                        break;
                }
            }
            return rxData;
        }
        #endregion

        #region 协议帧解析
        // DataId --> Fn
        public static byte DataIdToFn(UInt16 DataId)
        {
            byte Fn = 0;

            for (byte i = 0; i < 8; i++ )
            {
                if (((1 << i) & DataId) > 0)
                {
                    Fn = (byte)(DataId / 256 * 8 + i + 1);
                    break;
                }
            }

            return Fn;
        }
        // Fn --> DataId
        public static UInt16 FnToDataId(byte Fn)
        {
            return (UInt16)( (((Fn - 1) / 8) << 8)  +  (1 << ((Fn - 1) % 8)) );
        }
        // 解析 AFN
        public static string ExplainAFN(byte AFN)
        {
            string strAFN = "无法识别";

            foreach (CmdExplain cmd in FrameExplainTbl)
            {
                if (cmd.Afn == AFN)
                {
                    strAFN = cmd.AfnName;
                }
            }

            return strAFN;
        }

        // 解析 Fn
        public static string ExplainFn(byte AFN, byte Fn)
        {
            string strFn = "无法识别";

            foreach (CmdExplain cmd in FrameExplainTbl)
            {
                if (cmd.Afn == AFN && cmd.Fn == Fn)
                {
                    strFn = cmd.FnName;
                }
            }

            return strFn;
        }
        public static string ExplainFn(FrameFormat frame)
        {
            string strFn = ExplainFn(frame.Afn, DataIdToFn(frame.DataId));

            if (frame.CtrlWord.StartFlag == false && frame.Afn != 0x00 && strFn != "无法识别")
            {
                strFn += "-应答";
            }

            return strFn;
        }

        // 根据Fn名称获取AFN、Fn
        public static void GetAfnFnByName(string fnName, out byte afn, out ushort fnDataId)
        {
            afn = 0xFF;
            fnDataId = 0xFFFF;

            foreach (CmdExplain cmd in FrameExplainTbl)
            {
                if (cmd.FnName == fnName)
                {
                    afn = cmd.Afn;
                    fnDataId = FnToDataId(cmd.Fn);
                }
            }
        }

        // 解析 帧类型、颜色
        public static void GetFrameTypeAndColor(FrameFormat frame, out string frameType, out Color frameColor)
        {
            frameType = "无法识别";
            frameColor = Color.Black;

            foreach(CmdExplain cmd in FrameExplainTbl)
            {
                if(cmd.Afn == frame.Afn 
                    && cmd.Fn == DataIdToFn(frame.DataId)
                  )
                {
                    frameType = ExplainFn(frame);
                    frameColor = cmd.CmdColor;
                }
            }
        }

        // 解析 帧数据部分
        public static TreeNode ExplainFrameData(FrameFormat frame)
        {
            TreeNode node = null;

            foreach (CmdExplain cmd in FrameExplainTbl)
            {
                if (cmd.Afn == frame.Afn
                    && cmd.Fn == DataIdToFn(frame.DataId) 
                  )
                {
                    node = cmd.CmdExplainFunc(frame);
                }
            }

            return node;
        }

        // 解析 协议帧
        public static TreeNode GetProtoTree(byte[] databuf)
        {
            FrameFormat frame = ExplainRxPacket(databuf);
            TreeNode parentNode = new TreeNode("1376.2报文");
            TreeNode node = null;
            string strTmp = "";
            string strCommType;
            byte relayCnt;

            if (frame.Length == 0) return null;

            // parentNode--帧长
            strTmp = "帧长  ：" + frame.Length;
            parentNode.Nodes.Add(strTmp);

            // parentNode--控制域
            strTmp = "控制域：" + frame.CtrlWord.All.ToString("X2");
            node = new TreeNode(strTmp);
            {
                strTmp = "通信方式：" + frame.CtrlWord.CommType + " ";
                switch (frame.CtrlWord.CommType)
                {
                    case 1: strTmp += "集中式窄带载波"; break;
                    case 2: strTmp += "分布式窄带载波"; break;
                    case 3: strTmp += "宽带载波"; break;
                    case 10: strTmp += "微功率无线"; break;
                    case 20: strTmp += "以太网"; break;
                    default: strTmp += "无法识别"; break;
                }
                node.Nodes.Add(strTmp);
                strTmp = "传输方向：" + (frame.CtrlWord.DirFlag ? "上行" : "下行");
                node.Nodes.Add(strTmp);
                strTmp = "启动标识：" + (frame.CtrlWord.StartFlag ? "主动" : "从动");
                node.Nodes.Add(strTmp);
            }
            parentNode.Nodes.Add(node);
            node.Expand();

            // parentNode--信息域
            strTmp = "信息域(" + (frame.InfoDown.B1_CommModuleFlag ? "子节点通信" : "主节点通信") + ")";
            node = new TreeNode(strTmp);
            if (frame.CtrlWord.DirFlag == false)
            {
                strTmp = "路由标识    ：" + (frame.InfoDown.B1_RouteFlag ? "有" : "无");
                node.Nodes.Add(strTmp);
                strTmp = "附属节点标识：" + (frame.InfoDown.B1_SubNodeFlag ? "有" : "无");
                node.Nodes.Add(strTmp);
                strTmp = "通信模块标识：" + (frame.InfoDown.B1_CommModuleFlag ? "子节点通信" : "主节点通信");
                node.Nodes.Add(strTmp);
                strTmp = "冲突检测标识：" + (frame.InfoDown.B1_RouteFlag ? "有" : "无");
                node.Nodes.Add(strTmp);
                strTmp = "中继级别    ：" + frame.InfoDown.B1_RelayLevel;
                node.Nodes.Add(strTmp);
                strTmp = "信道标识    ：" + frame.InfoDown.B2_Chanel;
                node.Nodes.Add(strTmp);
                strTmp = "纠错编码标识：" + (frame.InfoDown.B2_ErrCrcCode == 0 ? "无" : "按RS编码");
                node.Nodes.Add(strTmp);
                strTmp = "预计应答数  ：" + frame.InfoDown.B3_ExpectAckLen + " byte";
                node.Nodes.Add(strTmp);
                strTmp = "通信速率    ：" + frame.InfoDown.B4B5_DataRate + (frame.InfoDown.B4B5_RateUnitFlag ? " Kbps" : " bps");
                node.Nodes.Add(strTmp);
                strTmp = "报文序号    ：" + frame.InfoDown.B6_Fsn;
                node.Nodes.Add(strTmp);

                strCommType = (frame.InfoDown.B1_CommModuleFlag ? "子节点通信" : "主节点通信");
                relayCnt = frame.InfoDown.B1_RelayLevel;
            }
            else
            {
                strTmp = "路由标识    ：" + (frame.InfoUp.B1_RouteFlag ? "有" : "无");
                node.Nodes.Add(strTmp);
                strTmp = "附属节点标识：" + (frame.InfoUp.B1_SubNodeFlag ? "有" : "无");
                node.Nodes.Add(strTmp);
                strTmp = "通信模块标识：" + (frame.InfoUp.B1_CommModuleFlag ? "子节点通信" : "主节点通信");
                node.Nodes.Add(strTmp);
                strTmp = "冲突检测标识：" + (frame.InfoUp.B1_RouteFlag ? "有" : "无");
                node.Nodes.Add(strTmp);
                strTmp = "中继级别    ：" + frame.InfoUp.B1_RelayLevel;
                node.Nodes.Add(strTmp);
                strTmp = "信道标识    ：" + frame.InfoUp.B2_Chanel;
                node.Nodes.Add(strTmp);
                strTmp = "纠错编码标识：" + (frame.InfoUp.B2_ErrCrcCode == 0 ? "无" : "按RS编码");
                node.Nodes.Add(strTmp);
                strTmp = "实际相线标识：" + frame.InfoUp.B3_ActualPhase;
                node.Nodes.Add(strTmp);
                strTmp = "电表通用特征：" + frame.InfoUp.B3_AmmeterFuture;
                node.Nodes.Add(strTmp);
                strTmp = "命令信号品质：" + frame.InfoUp.B4_EndCmdSignalQlty;
                node.Nodes.Add(strTmp);
                strTmp = "应答信号品质：" + frame.InfoUp.B4_EndAckSignalQlty;
                node.Nodes.Add(strTmp);
                strTmp = "事件上报标识：" + (frame.InfoUp.B5_EventReportFlag ? "有" : "无");
                node.Nodes.Add(strTmp);
                strTmp = "报文序号    ：" + frame.InfoUp.B6_Fsn;
                node.Nodes.Add(strTmp);

                strCommType = (frame.InfoUp.B1_CommModuleFlag ? "子节点通信" : "主节点通信");
                relayCnt = frame.InfoUp.B1_RelayLevel;
            }
            parentNode.Nodes.Add(node);

            //地址域
            if ("子节点通信" == strCommType)
            {
                // parentNode--源地址
                strTmp = "源地址  ：" + Util.GetStringHexFromBytes(frame.SrcAddr, 0, LongAddrSize, "", true);
                parentNode.Nodes.Add(strTmp);
                
                // parentNode--中继地址
                for (int i = 0; i < relayCnt; i++)
                {
                    strTmp = "中继" + (i + 1) + "：" + Util.GetStringHexFromBytes(frame.RouteAddrs, i * LongAddrSize, LongAddrSize, "", true);
                    parentNode.Nodes.Add(strTmp );
                }
                
                // parentNode--目的地址
                strTmp = "目的地址：" + Util.GetStringHexFromBytes(frame.DstAddr, 0, LongAddrSize, "", true);
                parentNode.Nodes.Add(strTmp);
            }

            // parentNode--AFN
            strTmp = "功能码AFN ：" + (frame.Afn.ToString("X2") + "H").PadRight(4) + " " + ExplainAFN(frame.Afn);
            parentNode.Nodes.Add(strTmp);
            
            // parentNode--Fn
            strTmp = "具体项Fn  ：" + ("F" + DataIdToFn(frame.DataId)).PadRight(4) + " " + ExplainFn(frame);
            parentNode.Nodes.Add(strTmp);

            // parentNode--数据区
            node = ExplainFrameData(frame);
            if (node != null)
            {
                parentNode.Nodes.Add(node);
                node.Expand();
            }

            return parentNode;
        }
        #endregion

        #region 详细命令解析

        #region 00 确认/否认
        private static TreeNode ExplainAckNack_Ack(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷"); 
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (buf == null) return null; 

            if (buf.Length < 6) return payloadNode;

            UInt32 tmp = BitConverter.ToUInt32(buf, index);
            strTmp = "命令状态：" + ((tmp & 0x01) > 0 ? "已处理" : "未处理");
            payloadNode.Nodes.Add(strTmp);

            strTmp = "信道状态：" + ((tmp >> 1) == 0x7FFFFFFF ? "空闲" : (tmp >> 1).ToString("X8"));
            payloadNode.Nodes.Add(strTmp);
            index += 4;

            strTmp = "等待时间：" + (buf[index] + buf[index + 1] * 256);
            payloadNode.Nodes.Add(strTmp);
            index += 2;

            return payloadNode;
        }
        private static TreeNode ExplainAckNack_Nack(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (buf == null) return null;

            if (buf.Length < 1) return payloadNode;

            strTmp = "错误状态字：" 
                    + (buf[index] >= ErrorStatusTbl.Length ? "其他错误" : ErrorStatusTbl[buf[index]]);
            payloadNode.Nodes.Add(strTmp);
            index += 1;

            return payloadNode;
        }
        private static readonly string[] ErrorStatusTbl = new string[]
        {
            "通信超时",
            "无效数据单元",
            "长度错误",
            "校验错误",
            "信息类不存在",
            "格式错误",
            "表号重复",
            "表号不存在",
            "应用层无应答",
            "主节点忙",
            "主节点不支持此命令",
            "从节点无应答",
            "从节点不在网内",
        };
        #endregion

        #region 01 初始化
        private static TreeNode ExplainInit_Hardware(FrameFormat frame)
        {
            // 无数据载荷
            return null;
        }
        private static TreeNode ExplainInit_ParamFiled(FrameFormat frame)
        {
            // 无数据载荷
            return null;
        }
        private static TreeNode ExplainInit_DataFiled(FrameFormat frame)
        {
            // 无数据载荷
            return null;
        }
        #endregion

        #region 02 数据转发
        private static TreeNode ExplainDataTransfer_Transfer(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (buf == null) return null;

            if (buf.Length < 2) return payloadNode;

            strTmp = "通信协议：" + (buf[index] >= CommProtoTbl.Length ? "无法识别" : CommProtoTbl[buf[index]]);
            payloadNode.Nodes.Add(strTmp);
            index += 1;

            if (buf.Length < index + buf[index] + 1) return payloadNode;

            strTmp = "报文长度：" + buf[index];
            payloadNode.Nodes.Add(strTmp);
            index += 1;

            string strDataType = "";
            for (; index < buf.Length; index++)
            {
                if (buf[index] == 0x68 && buf.Length > 10 && buf.Length == (12 + buf[index + 10]))
                {
                    strDataType = "645-07报文";
                    break;
                }
                else if (buf[index] == 0x68 && buf.Length > 11 && buf.Length == (13 + buf[index + 11]))
                {
                    strDataType = "188-04报文";
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

        private static readonly string[] CommProtoTbl = new string[]
        {
            "双向水表",       // 0x00
            "645-97电表",
            "645-07电表",
            "单向水表",
            "未知",           // 0x04 保留
            "燃气表",
            "热表",
            "698电表",
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

        private static TreeNode ExplainDataTransfer_ReadAmeterBoxOpenEvent(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (buf == null) return null;

            if (buf.Length < 2) return payloadNode;

            strTmp = "通信协议：" + (buf[index] >= CommProtoTbl.Length ? "无法识别" : CommProtoTbl[buf[index]]);
            payloadNode.Nodes.Add(strTmp);
            index += 1;

            if (buf.Length < index + buf[index] + 1) return payloadNode;

            strTmp = "报文长度：" + buf[index];
            payloadNode.Nodes.Add(strTmp);
            index += 1;

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

        #region 03 数据查询
        private static TreeNode ExplainDataQuery_OemAndVer(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                return null;
            }
            else
            {
                // 应答
                if (buf == null) return null;

                if (buf.Length < 9) return payloadNode;

                strTmp = "厂商代码：" + Convert.ToChar(buf[index + 1]) + Convert.ToChar(buf[index]);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "芯片代码：" + Convert.ToChar(buf[index + 1]) + Convert.ToChar(buf[index]);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "版本日期：" 
                        + DateTime.Now.Year/100 + buf[index + 2].ToString("X2") + "-"
                        + buf[index + 1].ToString("X2") + "-"
                        + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 3;

                strTmp = "版本号  ：" + buf[index + 1].ToString("X") + "." + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 2;
            }

            return payloadNode;
        }
        private static TreeNode ExplainDataQuery_NoiseValue(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                return null;
            }
            else
            {
                // 应答
                if (buf == null) return null;

                if (buf.Length < 1) return payloadNode;

                strTmp = "噪声值：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }

            return payloadNode;
        }
        private static TreeNode ExplainDataQuery_SubNodeListenInfo(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 2) return payloadNode;

                strTmp = "开始节点序号：" + buf[index++];
                payloadNode.Nodes.Add(strTmp);

                strTmp = "读取节点数  ：" + buf[index++];
                payloadNode.Nodes.Add(strTmp);
            }
            else
            {
                // 应答
                if (buf == null) return null;

                if (buf.Length < 2) return payloadNode;

                strTmp = "侦听到的从节点总数：" + buf[index++];
                payloadNode.Nodes.Add(strTmp);

                int nodeCnt = buf[index];
                strTmp = "本帧传输的从节点数：" + buf[index++];
                payloadNode.Nodes.Add(strTmp);

                if (buf.Length < index + nodeCnt * 8) return payloadNode;

                TreeNode node = null;
                for(int i = 0; i < nodeCnt; i++)
                {
                    strTmp = "节点" + (i + 1) + "：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                    index += LongAddrSize;
                    {
                        strTmp = "中继级别：" + (buf[index] & 0x0F);
                        node.Nodes.Add(strTmp);
                        strTmp = "信号品质：" + (buf[index] >> 4);
                        node.Nodes.Add(strTmp);
                        index++;

                        strTmp = "侦听次数：" + buf[index++];
                        node.Nodes.Add(strTmp);
                    }
                }
            }

            return payloadNode;
        }
        private static TreeNode ExplainDataQuery_MainNodeAddr(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                return null;
            }
            else
            {
                // 应答
                if (buf == null) return null;

                if (buf.Length < 6) return payloadNode;

                strTmp = "主节点地址：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                payloadNode.Nodes.Add(strTmp);
                index += LongAddrSize;
            }

            return payloadNode;
        }
        private static TreeNode ExplainDataQuery_MainNodeStatusAndDataRate(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                return null;
            }
            else
            {
                // 应答
                if (buf == null) return null;

                if (buf.Length < 2) return payloadNode;

                strTmp = "信道数量      ：" + (buf[index + 1] & 0x1F);
                payloadNode.Nodes.Add(strTmp);

                strTmp = "周期抄表模式  ：";
                switch((buf[index] >> 6 & 0x03))
                {
                    case 0: strTmp += "由集中器和路由主导的"; break;
                    case 1: strTmp += "由集中器主导的"; break;
                    case 2: strTmp += "由路由主导的"; break;
                    default: strTmp += "未知"; break;
                }
                payloadNode.Nodes.Add(strTmp);

                strTmp = "主节点信道特征：";
                switch ((buf[index] >> 4 & 0x03))
                {
                    case 0: strTmp += "微功率无线传输"; break;
                    case 1: strTmp += "单相供电单相传输"; break;
                    case 2: strTmp += "单相供电三相传输"; break;
                    case 3: strTmp += "三相供电三相传输"; break;
                    default: strTmp += "未知"; break;
                }
                payloadNode.Nodes.Add(strTmp);

                int rateCnt = (buf[index] & 0x0F);
                strTmp = "支持的速率数量：" + (buf[index] & 0x0F);
                payloadNode.Nodes.Add(strTmp);

                index += 2;

                if (buf.Length < index + rateCnt * 2) return payloadNode;

                UInt16 u16Temp, baud;
                for (int i = 0; i < rateCnt; i++)
                {
                    u16Temp = (UInt16)(buf[index] + buf[index + 1] * 256);
                    baud = (UInt16)(u16Temp & 0x7FFF);
                    strTmp = ("通信速率" + (i + 1)).PadRight(10) + "：" 
                            + (baud == 0 ? "9600 bps" : baud.ToString() + ((u16Temp >> 15) > 0 ? " Kbps" : " bps"));
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;
                }
            }

            return payloadNode;
        }
        private static TreeNode ExplainDataQuery_MainNodeDisturbedState(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 1) return payloadNode;

                strTmp = "持续时间：" + buf[index] + " 分钟";
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }
            else
            {
                // 应答
                if (buf == null) return null;

                if (buf.Length < 1) return payloadNode;

                strTmp = "干扰状态：" + (buf[index] == 0 ? "无" : "有");
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }

            return payloadNode;
        }
        private static TreeNode ExplainDataQuery_SubNodeListenMaxTimeoutTime(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                return null;
            }
            else
            {
                // 应答
                if (buf == null) return null;

                if (buf.Length < 1) return payloadNode;

                strTmp = "超时时间：" + buf[index] + " 秒";
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }

            return payloadNode;
        }
        private static TreeNode ExplainDataQuery_WirelessCommParam(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                return null;
            }
            else
            {
                // 应答
                if (buf == null) return null;

                if (buf.Length < 2) return payloadNode;

                strTmp = "信道组号：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "发射功率：" + (buf[index] >= SendPowerTbl.Length ? "无法识别" : SendPowerTbl[buf[index]]);
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }

            return payloadNode;
        }

        private static readonly string[] SendPowerTbl = new string[]
        {
            "最高",
            "次高",
            "次低",
            "最低",
            "全功率",
        };
        private static TreeNode ExplainDataQuery_BroadcastDelayTime(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 2) return payloadNode;

                strTmp = "通信协议："
                        + (buf[index] >= CommProtoTbl.Length ? "无法识别" : CommProtoTbl[buf[index]]);
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (buf.Length < index + buf[index] + 1) return payloadNode;

                strTmp = "报文长度：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }
            else
            {
                // 应答
                if (buf == null) return null;

                if (buf.Length < index + 2) return payloadNode;

                strTmp = "广播延时：" + (buf[index] + buf[index + 1] * 256 ) + " 秒";
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                if (buf.Length < index + 2) return payloadNode;

                strTmp = "通信协议："
                        + (buf[index] >= CommProtoTbl.Length ? "无法识别" : CommProtoTbl[buf[index]]);
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (buf.Length < index + buf[index] + 1) return payloadNode;

                strTmp = "报文长度：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }

            return payloadNode;
        }
        private static TreeNode ExplainDataQuery_LocalCommModuleRunningMode(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                return null;
            }
            else
            {
                // 应答
                if (buf == null) return null;

                if (buf.Length < 39) return payloadNode;

                strTmp = "通信方式        ：";
                switch(buf[index] & 0x0F)
                {
                    case 1: strTmp += "窄带电力线载波"; break;
                    case 2: strTmp += "宽带电力线载波"; break;
                    case 3: strTmp += "微功率无线载波"; break;
                    default: strTmp += "无法识别"; break;
                }
                payloadNode.Nodes.Add(strTmp);
                strTmp = "路由管理方式    ：" + ((buf[index] & 0x10) > 0 ? "支持" : "不支持") + "路由";
                payloadNode.Nodes.Add(strTmp);
                strTmp = "从节点信息模式  ：" + ((buf[index] & 0x20) > 0 ? "下发" : "不下发") + "从节点信息";
                payloadNode.Nodes.Add(strTmp);
                strTmp = "周期抄表模式    ：";
                switch (buf[index] >> 6)
                {
                    case 1: strTmp += "由集中器主导的"; break;
                    case 2: strTmp += "由路由主动的"; break;
                    case 3: strTmp += "由集中器和路由主导的"; break;
                    default: strTmp += "无法识别"; break;
                }
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "传输延时参数支持："
                        + "路由主动抄表-" + ((buf[index] & 0x01) > 0 ? "支持" : "不支持")
                        + " | 从节点监控-" + ((buf[index] & 0x02) > 0 ? "支持" : "不支持")
                        + " | 广播-" + ((buf[index] & 0x04) > 0 ? "支持" : "不支持");
                payloadNode.Nodes.Add(strTmp);
                strTmp = "失败节点切换方式："
                        + "通信模块主动切换-" + ((buf[index] & 0x08) > 0 ? "支持" : "不支持") 
                        + " | 集中器通知通信模块切换-" + ((buf[index] & 0x10) > 0 ? "支持" : "不支持");
                payloadNode.Nodes.Add(strTmp);
                strTmp = "广播命令确认方式：" + ((buf[index] & 0x20) > 0 ? "广播执行前" : "广播执行后") + "返回确认报文";
                payloadNode.Nodes.Add(strTmp);
                strTmp = "广播命令信道执行方式：" + ((buf[index] & 0x40) > 0 ? "按信道标识逐个发送" : "不需要信道标识");
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "支持的信道数量  ：" + (buf[index] & 0x1F);
                payloadNode.Nodes.Add(strTmp);
                strTmp = "低压电网掉电信息："
                        + "A相-" + ((buf[index] & 0x20) > 0 ? "掉电" : "未掉电") + " | "
                        + "B相-" + ((buf[index] & 0x40) > 0 ? "掉电" : "未掉电") + " | "
                        + "C相-" + ((buf[index] & 0x80) > 0 ? "掉电" : "未掉电");
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                int rateCnt = (buf[index] & 0x0F);
                strTmp = "支持的速率数量  ：" + (buf[index] & 0x0F);
                payloadNode.Nodes.Add(strTmp);
                index += 3;  // 2字节保留

                strTmp = "从节点监控最大超时时间：" + buf[index] + " 秒";
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "广播命令最大超时时间：" + (buf[index] + buf[index + 1] * 256) + " 秒";
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "最大支持的报文长度  ：" + (buf[index] + buf[index + 1] * 256) + " byte";
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "文件传输单包最大长度：" + (buf[index] + buf[index + 1] * 256) + " byte";
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "升级操作等待时间    ：" + buf[index] + " 秒";
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "主节点地址          ：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                payloadNode.Nodes.Add(strTmp);
                index += LongAddrSize;

                strTmp = "最大支持的从节点数量：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "当前从节点数量      ：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "通信协议发布日期    ：" 
                        + DateTime.Now.Year/100 + buf[index + 2].ToString("X2") + "/"
                        + buf[index + 1].ToString("X2") + "/"
                        + buf[index + 0].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 3;

                strTmp = "通信协议最后备案日期："
                        + DateTime.Now.Year/100 + buf[index + 2].ToString("X2") + "/"
                        + buf[index + 1].ToString("X2") + "/"
                        + buf[index + 0].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 3;

                strTmp = "厂商代码  ：" + Convert.ToChar(buf[index]) + Convert.ToChar(buf[index + 1]);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "芯片代码  ：" + Convert.ToChar(buf[index]) + Convert.ToChar(buf[index + 1]);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "版本日期  ："
                        + DateTime.Now.Year/100 + buf[index + 2].ToString("X2") + "/"
                        + buf[index + 1].ToString("X2") + "/"
                        + buf[index + 0].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 3;

                strTmp = "版本号    ：" + "v" + buf[index + 1].ToString("X") + "." + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                if (buf.Length < index + rateCnt * 2) return payloadNode;

                UInt16 u16Temp, baud;
                for (int i = 0; i < rateCnt; i++)
                {
                    u16Temp = (UInt16)(buf[index] + buf[index + 1] * 256);
                    baud = (UInt16)(u16Temp & 0x7FFF);
                    strTmp = ("通信速率" + (i + 1)).PadRight(6) + "："
                            + (baud == 0 ? "9600 bps" : baud.ToString() + ((u16Temp >> 15) > 0 ? " Kbps" : " bps"));
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;
                }
            }

            return payloadNode;
        }
        private static TreeNode ExplainDataQuery_LocalCommModuleAfnIndex(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 1) return payloadNode;

                strTmp = "AFN功能码：" + ExplainAFN(buf[index]);
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }
            else
            {
                // 应答
                if (buf == null) return null;

                if (buf.Length < 33) return payloadNode;

                byte u8Temp, u8AFN, u8Fn = 0;

                u8AFN = buf[index];
                strTmp = "AFN功能码：" + u8AFN.ToString("X2") + "H " + ExplainAFN(u8AFN);
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "支持的数据单元(F1 ~ F255)";
                TreeNode node = new TreeNode(strTmp);
                payloadNode.Nodes.Add(node);
                for(int i = 0; i < 32; i++)
                {
                    u8Temp = buf[index++];
                    for(int j = 0; j < 8; j++)
                    {
                        u8Fn++;
                        if((u8Temp & 0x01) > 0)
                        {
                            strTmp = ("F" + u8Fn +  "：").PadRight(4) + ExplainFn(u8AFN, u8Fn);
                            node.Nodes.Add(strTmp);
                        }
                        u8Temp >>= 1;
                    }
                }

                if(node.Nodes.Count == 0)
                {
                    node.Nodes.Add("无");
                }
                node.Expand();
            }

            return payloadNode;
        }
        private static TreeNode ExplainDataQuery_RssiThreshold(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                return null;
            }
            else
            {
                // 应答
                if (buf == null) return null;

                if (buf.Length < 1) return payloadNode;

                strTmp = "场强门限：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }

            return payloadNode;
        }

        #endregion

        #region 04 链路接口测试
        private static TreeNode ExplainDataLinkTest_SendTest(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 1) return payloadNode;

                strTmp = "持续时间：" + buf[index] + " 秒";
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainDataLinkTest_SubNodeCheckName(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                return null;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainDataLinkTest_DatagramCommTest(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 9) return payloadNode;

                strTmp = "通信速率序号：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "目标地址：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                payloadNode.Nodes.Add(strTmp);
                index += LongAddrSize;

                strTmp = "通信协议：" + (buf[index] >= CommProtoTbl.Length ? "无法识别" : CommProtoTbl[buf[index]]);
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (buf.Length < index + buf[index] + 1) return payloadNode;

                strTmp = "报文长度：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainDataLinkTest_SendPowerTest(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 3) return payloadNode;

                strTmp = "信道索引：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "测试码流：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "持续时间：" + buf[index] + " 秒";
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        #endregion

        #region 05 控制命令
        private static TreeNode ExplainCtrlCmd_SetMainNodeAddr(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 6) return payloadNode;

                strTmp = "主节点地址：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                payloadNode.Nodes.Add(strTmp);
                index += LongAddrSize;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainCtrlCmd_EnableOrDisableSubNodeReport(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 1) return payloadNode;

                strTmp = "事件上报标识：" + (buf[index] == 0 ? "禁止" : "允许");
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainCtrlCmd_StartBroadcast(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 2) return payloadNode;

                strTmp = "通信协议：" + (buf[index] >= CommProtoTbl.Length ? "无法识别" : CommProtoTbl[buf[index]]);
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (buf.Length < index + buf[index] + 1) return payloadNode;

                strTmp = "报文长度：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainCtrlCmd_SetSubNodeMaxTimeoutTime(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 1) return payloadNode;

                strTmp = "最大超时时间：" + buf[index] + " 秒";
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainCtrlCmd_SetWirelessCommParam(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 2) return payloadNode;

                strTmp = "信道组号：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "发射功率：" + (buf[index] >= SendPowerTbl.Length ? "无法识别" : SendPowerTbl[buf[index]]);
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainCtrlCmd_StartWholeNetworkSense(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                return null;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainCtrlCmd_SetRssiThreshold(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 1) return payloadNode;

                strTmp = "场强门限：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainCtrlCmd_SetMainNodeClock(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 6) return payloadNode;

                strTmp = "当前时间：20" 
                        + buf[index + 5].ToString("X2") + "-"
                        + buf[index + 4].ToString("X2") + "-" 
                        + buf[index + 3].ToString("X2") + " "
                        + buf[index + 2].ToString("X2") + ":"
                        + buf[index + 1].ToString("X2") + ":" 
                        + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 6;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        #endregion

        #region 06 主动上报
        private static TreeNode ExplainAutoReport_SubNodeInfo(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 1) return payloadNode;

                int nodeCnt = buf[index];
                strTmp = "从节点数量：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (buf.Length < index + nodeCnt * 9) return payloadNode;

                TreeNode node = null;
                for (int i = 0; i < nodeCnt; i++)
                {
                    strTmp = "节点" + (i + 1) + "：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);

                    index += LongAddrSize;
                    {
                        strTmp = "通信协议：" + (buf[index] >= CommProtoTbl.Length ? "无法识别" : CommProtoTbl[buf[index]]);
                        node.Nodes.Add(strTmp);
                        index += 1;

                        strTmp = "节点序号：" + (buf[index] + buf[index + 1] * 256);
                        node.Nodes.Add(strTmp);
                        index += 2;
                    }
                    
                }

            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainAutoReport_ReadData(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 2) return payloadNode;

                strTmp = "节点序号：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "通信协议：" + (buf[index] >= CommProtoTbl.Length ? "无法识别" : CommProtoTbl[buf[index]]);
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "上行耗时：" + +(buf[index] + buf[index + 1] * 256) + " 秒";
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (buf.Length < index + buf[index] + 1) return payloadNode;

                strTmp = "报文长度：" + buf[index];
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
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainAutoReport_RouteWorkStateChanges(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 1) return payloadNode;

                strTmp = "路由工作变动类型：" ;
                switch(buf[index])
                {
                    case 1: strTmp += "抄表任务结束"; break;
                    case 2: strTmp += "搜表任务结束"; break;
                    default: strTmp += "无法识别";  break;
                }
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainAutoReport_SubNodeInfoAndDevType(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 1) return payloadNode;

                int nodeCnt = buf[index];
                strTmp = "从节点数量：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                TreeNode node = null;
                for (int i = 0; i < nodeCnt; i++)
                {
                    if (buf.Length < index + 11) return payloadNode;

                    strTmp = "从节点" + (i + 1) + "：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                    index += LongAddrSize;

                    {
                        strTmp = "通信协议：" + (buf[index] >= CommProtoTbl.Length ? "无法识别" : CommProtoTbl[buf[index]]);
                        node.Nodes.Add(strTmp);
                        index += 1;

                        strTmp = "节点序号：" + (buf[index] + buf[index + 1] * 256);
                        node.Nodes.Add(strTmp);
                        index += 2;

                        strTmp = "节点类型：" ;
                        switch (buf[index])
                        {
                            case 0: strTmp += "采集器"; break;
                            case 1: strTmp += "电表"; break;
                            default: strTmp += "无法识别"; break;
                        }
                        node.Nodes.Add(strTmp);
                        index += 1;

                        // 从节点的附属节点信息
                        int subNodeCnt = buf[index];
                        strTmp = "附属节点数量：" + buf[index];
                        node.Nodes.Add(strTmp);
                        index += 1;

                        TreeNode subNode = null;
                        for (int j = 0; j < subNodeCnt; j++)
                        {
                            if (buf.Length < index + 7) return payloadNode;

                            strTmp = "附属节点" + (j + 1) + "：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                            subNode = new TreeNode(strTmp);
                            node.Nodes.Add(subNode);
                            index += LongAddrSize;

                            {
                                strTmp = "通信协议：" + (buf[index] >= CommProtoTbl.Length ? "无法识别" : CommProtoTbl[buf[index]]);
                                subNode.Nodes.Add(strTmp);
                                index += 1;
                            }
                        }
                    }  
                }
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainAutoReport_SubNodeEvent(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 3) return payloadNode;

                strTmp = "节点类型：";
                switch (buf[index])
                {
                    case 0: strTmp += "采集器"; break;
                    case 1: strTmp += "电表"; break;
                    default: strTmp += "无法识别"; break;
                }
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "通信协议：" + (buf[index] >= CommProtoTbl.Length ? "无法识别" : CommProtoTbl[buf[index]]);
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (buf.Length < index + buf[index] + 1) return payloadNode;

                strTmp = "报文长度：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainAutoReport_MeteringBoxOpenEvent(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 8) return payloadNode;

                strTmp = "通信协议：" + (buf[index] >= CommProtoTbl.Length ? "无法识别" : CommProtoTbl[buf[index]]);
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "表端模块地址：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                payloadNode.Nodes.Add(strTmp);
                index += LongAddrSize;

                if (buf.Length < index + buf[index] + 1) return payloadNode;

                strTmp = "报文长度：" + buf[index];
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

            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        #endregion

        #region 10 路由查询
        private static TreeNode ExplainRouteQuery_SubNodeCount(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                return null;
            }
            else
            {
                // 应答 
                if (buf == null) return null;

                if (buf.Length < 4) return payloadNode;

                strTmp = "从节点总数量：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "最大支持的从节点数量：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
            }

            return payloadNode;
        }
        private static TreeNode ExplainRouteQuery_SubNodeInfo(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 3) return payloadNode;

                strTmp = "从节点起始序号：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "从节点数量：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }
            else
            {
                // 应答 
                if (buf == null) return null;

                if (buf.Length < 3) return payloadNode;

                strTmp = "从节点总数量：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                int nodeCnt = buf[index];
                strTmp = "本次应答数量：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (buf.Length < index + nodeCnt * 8) return payloadNode;

                TreeNode node = null;
                int temp;
                for (int i = 0; i < nodeCnt; i++)
                {
                    strTmp = "节点" + (i + 1) + "：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                    index += LongAddrSize;
                    {
                        strTmp = "中继级别：" + (buf[index] & 0x0F);
                        node.Nodes.Add(strTmp);
                        strTmp = "信号品质：" + (buf[index] >> 4);
                        node.Nodes.Add(strTmp);
                        index += 1;

                        strTmp = "相位："
                                + ((buf[index] & 0x01) > 0 ? "1相|" : "")
                                + ((buf[index] & 0x02) > 0 ? "2相|" : "")
                                + ((buf[index] & 0x04) > 0 ? "3相|" : "");
                        node.Nodes.Add(strTmp);
                        temp = (buf[index] >> 3) & 0x03;
                        strTmp = "通信协议：" + ( temp >= CommProtoTbl.Length ? "无法识别" : CommProtoTbl[temp]);
                        node.Nodes.Add(strTmp);
                        index += 1;
                    }
                }
            }

            return payloadNode;
        }
        private static TreeNode ExplainRouteQuery_SubNodeRelayInfo(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 6) return payloadNode;

                strTmp = "从节点地址：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                payloadNode.Nodes.Add(strTmp);
                index += LongAddrSize;
            }
            else
            {
                // 应答 
                if (buf == null) return null;

                if (buf.Length < 1) return payloadNode;

                int nodeCnt = buf[index];
                strTmp = "中继节点总数：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (buf.Length < index + nodeCnt * 8) return payloadNode;

                TreeNode node = null;
                int temp;
                for (int i = 0; i < nodeCnt; i++)
                {
                    strTmp = "节点" + (i + 1) + "：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                    index += LongAddrSize;
                    {
                        strTmp = "中继级别：" + (buf[index] & 0x0F);
                        node.Nodes.Add(strTmp);
                        strTmp = "信号品质：" + (buf[index] >> 4);
                        node.Nodes.Add(strTmp);
                        index += 1;

                        strTmp = "相位："
                                + ((buf[index] & 0x01) > 0 ? "1相|" : "")
                                + ((buf[index] & 0x02) > 0 ? "2相|" : "")
                                + ((buf[index] & 0x04) > 0 ? "3相|" : "");
                        node.Nodes.Add(strTmp);
                        temp = (buf[index] >> 3) & 0x03;
                        strTmp = "通信协议：" + (temp >= CommProtoTbl.Length ? "无法识别" : CommProtoTbl[temp]);
                        node.Nodes.Add(strTmp);
                        index += 1;
                    }
                }
            }

            return payloadNode;
        }
        private static TreeNode ExplainRouteQuery_RouteRunningState(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                return null;
            }
            else
            {
                // 应答 
                if (buf == null) return null;

                if (buf.Length < 2) return payloadNode;

                strTmp = "路由学习标志：" + ((buf[index] & 0x01) > 0 ? "已完成" : "未完成");
                payloadNode.Nodes.Add(strTmp);
                strTmp = "路由工作标志：" + ((buf[index] & 0x02) > 0 ? "正在工作" : "停止工作");
                payloadNode.Nodes.Add(strTmp);
                strTmp = "事件上报标志：" + ((buf[index] & 0x04) > 0 ? "有事件上报" : "无事件上报");
                payloadNode.Nodes.Add(strTmp);
                strTmp = "纠错编码    ：" + (buf[index] >> 4);
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "从节点总数量：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "入网节点数量：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                List<string> aliasList = new List<string>();

                strTmp = "中继抄到节点数量：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                strTmp = "-----------------------重定义-->[CRC总包数]   ：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "路由工作状态：" + ((buf[index] & 0x01) > 0 ? "学习" : "抄表");
                payloadNode.Nodes.Add(strTmp);
                strTmp = "注册允许状态：" + ((buf[index] & 0x02) > 0 ? "允许" : "不允许");
                payloadNode.Nodes.Add(strTmp);
                strTmp = "-----------------------重定义-->[接收信标个数]：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                UInt16 u16Temp = (UInt16)(buf[index] + buf[index + 1] * 256);
                strTmp = "通信速率    ：" + (u16Temp & 0x7FFF) + ((u16Temp >> 15) > 0 ? " Kbps" : " bps");
                payloadNode.Nodes.Add(strTmp);
                strTmp = "-----------------------重定义-->[CRC错误包数] ：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "第1相中继级别：" + (buf[index] & 0x0F);
                payloadNode.Nodes.Add(strTmp);
                strTmp = "-----------------------重定义-->[路径优先级错误]  ：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "第2相中继级别：" + (buf[index] & 0x0F);
                payloadNode.Nodes.Add(strTmp);
                strTmp = "-----------------------重定义-->[强制组网结束标志]：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "第3相中继级别：" + (buf[index] & 0x0F);
                payloadNode.Nodes.Add(strTmp);
                strTmp = "-----------------------重定义-->[保存路径错误]    ：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "第1相工作步骤：" + (buf[index] < RouteWorkStateTbl.Length ? RouteWorkStateTbl[buf[index]] : "无法识别");
                payloadNode.Nodes.Add(strTmp);
                strTmp = "-----------------------重定义-->[取路径优先级错误]：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "第2相工作步骤：" + (buf[index] < RouteWorkStateTbl.Length ? RouteWorkStateTbl[buf[index]] : "无法识别" );
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "第3相工作步骤：" + (buf[index] < RouteWorkStateTbl.Length ? RouteWorkStateTbl[buf[index]] : "无法识别");
                payloadNode.Nodes.Add(strTmp);
                strTmp = "-----------------------重定义-->[维护个数]        ：" + (buf[index - 1] + buf[index] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }

            return payloadNode;
        }

        private static readonly string[] RouteWorkStateTbl = new string[]
        {
            "无法识别",     /* 0 */
            "初始状态",     /* 1 */ 
            "直抄状态",     
            "中继状态",     
            "监控状态",
            "广播状态",
            "广播召读电表",
            "读侦听信息",
            "空闲状态",     /* 8 */
        };

        private static TreeNode ExplainRouteQuery_ReadFailedSubNodeInfo(FrameFormat frame)
        {
            return ExplainRouteQuery_SubNodeInfo(frame);
        }
        private static TreeNode ExplainRouteQuery_AutoRegSubNodeInfo(FrameFormat frame)
        {
            return ExplainRouteQuery_SubNodeInfo(frame);
        }
        private static TreeNode ExplainRouteQuery_WirelessSubNodeRouteInfo(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 6) return payloadNode;

                strTmp = "从节点地址：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                payloadNode.Nodes.Add(strTmp);
                index += LongAddrSize;
            }
            else
            {
                // 应答 
                if (buf == null) return null;

                if (buf.Length < 7) return payloadNode;

                strTmp = "从节点地址：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                payloadNode.Nodes.Add(strTmp);
                index += LongAddrSize;

                int RouteCnt = buf[index];
                strTmp = "路由个数：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                TreeNode node = null;
                int relayCnt;
                for (int i = 0; i < RouteCnt; i++)
                {
                    if (buf.Length < index + 1) return payloadNode;

                    relayCnt = buf[index++];

                    if (buf.Length < index + relayCnt * 6 + 1) return payloadNode;

                    strTmp = "路由" + (i + 1) + "中继级别：" + relayCnt;
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                    {
                        for (int j = 0; j < relayCnt; j++)
                        {
                            strTmp = "中继节点" + (j + 1) + "：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                            node.Nodes.Add(strTmp);
                            index += LongAddrSize;
                        }

                        strTmp = "路由信任度：" + buf[index];
                        node.Nodes.Add(strTmp);
                        index += 1;
                    }
                }
            }

            return payloadNode;
        }
        private static TreeNode ExplainRouteQuery_WholeNetworkSenseStatus(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                return null;
            }
            else
            {
                // 应答 
                if (buf == null) return null;

                if (buf.Length < 9) return payloadNode;

                strTmp = "更新完成标志：" + ((buf[index] & 0x01) > 0 ? "已完成" : "未完成");
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "剩余时间  ：" + (buf[index] + buf[index + 1] * 256) + " 秒";
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "下载总数：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "在网总数：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "离网总数：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
            }

            return payloadNode;
        }
        private static TreeNode ExplainRouteQuery_OnlineStatusUpdateInfo(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 3) return payloadNode;

                strTmp = "从节点起始序号：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "从节点数量：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "从节点类型：" ;
                switch(buf[index])
                {
                    case 0: strTmp += "全网"; break;
                    case 1: strTmp += "在网"; break;
                    case 2: strTmp += "离网"; break;
                    default: strTmp += "未知"; break;
                }
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }
            else
            {
                // 应答 
                if (buf == null) return null;

                if (buf.Length < 4) return payloadNode;

                strTmp = "从节点总数量：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                int nodeCnt = buf[index];
                strTmp = "本次应答数量：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "从节点类型：";
                switch (buf[index])
                {
                    case 0: strTmp += "全网"; break;
                    case 1: strTmp += "在网"; break;
                    case 2: strTmp += "离网"; break;
                    default: strTmp += "未知"; break;
                }
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (buf.Length < index + nodeCnt * 7) return payloadNode;

                for (int i = 0; i < nodeCnt; i++)
                {
                    strTmp = "节点" + (i + 1) + "：" 
                            + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true)
                            + "(" + ((buf[index + 6] & 0x01) > 0 ? "在网" : "离网") + ")";
                    payloadNode.Nodes.Add(strTmp);
                    index += 7;
                }
            }

            return payloadNode;
        }
        private static TreeNode ExplainRouteQuery_NetworkScale(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                return null;
            }
            else
            {
                // 应答 
                if (buf == null) return null;

                if (buf.Length < 2) return payloadNode;

                strTmp = "网络规模：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
            }

            return payloadNode;
        }
        private static TreeNode ExplainRouteQuery_MicroWirelessSubNodeInfo(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                return null;
            }
            else
            {
                // 应答 
                if (buf == null) return null;

                if (buf.Length < 3) return payloadNode;

                strTmp = "从节点总数量：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                int nodeCnt = buf[index];
                strTmp = "本次应答数量：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (buf.Length < index + nodeCnt * 11) return payloadNode;

                TreeNode node = null;
                int temp;
                for (int i = 0; i < nodeCnt; i++)
                {
                    strTmp = "节点" + (i + 1) + "：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                    index += LongAddrSize;
                    {
                        strTmp = "中继级别：" + (buf[index] & 0x0F);
                        node.Nodes.Add(strTmp);
                        strTmp = "信号品质：" + (buf[index] >> 4);
                        node.Nodes.Add(strTmp);
                        index += 1;

                        strTmp = "相位："
                                + ((buf[index] & 0x01) > 0 ? "1相|" : "")
                                + ((buf[index] & 0x02) > 0 ? "2相|" : "")
                                + ((buf[index] & 0x04) > 0 ? "3相|" : "");
                        node.Nodes.Add(strTmp);
                        temp = (buf[index] >> 3) & 0x03;
                        strTmp = "通信协议：" + (temp >= CommProtoTbl.Length ? "无法识别" : CommProtoTbl[temp]);
                        node.Nodes.Add(strTmp);
                        strTmp = "升级标志：" + ((buf[index] & 0x40) > 0 ? "已升级" : "未升级");
                        node.Nodes.Add(strTmp);
                        index += 1;

                        strTmp = "软件版本：" + buf[index + 1].ToString("X") + "." + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 2;

                        strTmp = "Boot版本：" + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 1;
                    }
                }
            }

            return payloadNode;
        }
        #endregion

        #region 11 路由设置
        private static TreeNode ExplainRouteSet_AddSubNode(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 1) return payloadNode;

                int nodeCnt = buf[index];
                strTmp = "从节点总数量：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (buf.Length < index + nodeCnt * 7) return payloadNode;

                TreeNode node = null;
                for (int i = 0; i < nodeCnt; i++)
                {
                    strTmp = "节点" + (i + 1) + "：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                    index += LongAddrSize;
                    {
                        strTmp = "通信协议：" + (buf[index] >= CommProtoTbl.Length ? "无法识别" : CommProtoTbl[buf[index]]);
                        node.Nodes.Add(strTmp);
                        index += 1;
                    }
                    node.Collapse();
                }
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainRouteSet_DelSubNode(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 1) return payloadNode;

                int nodeCnt = buf[index];
                strTmp = "从节点总数量：" + nodeCnt;
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (buf.Length < index + nodeCnt * 6) return payloadNode;

                for (int i = 0; i < nodeCnt; i++)
                {
                    strTmp = "节点" + (i + 1) + "：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                    payloadNode.Nodes.Add(strTmp);
                    index += LongAddrSize;
                }
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainRouteSet_SetSubNodeRelay(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 7) return payloadNode;

                strTmp = "从节点地址：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                payloadNode.Nodes.Add(strTmp);
                index += LongAddrSize;

                int nodeCnt = buf[index];
                strTmp = "中继级别  ：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (buf.Length < index + nodeCnt * 6) return payloadNode;

                for (int i = 0; i < nodeCnt; i++)
                {
                    strTmp = "中继节点" + (i + 1) + "：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                    payloadNode.Nodes.Add(strTmp);
                    index += LongAddrSize;
                }
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainRouteSet_SetRouteWorkMode(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 3) return payloadNode;

                strTmp = "路由工作状态：" + ((buf[index] & 0x01) > 0 ? "学习" : "抄表");
                payloadNode.Nodes.Add(strTmp);
                strTmp = "注册允许状态：" + ((buf[index] & 0x02) > 0 ? "允许" : "不允许");
                payloadNode.Nodes.Add(strTmp);
                strTmp = "纠错编码    ：" + (buf[index] >> 4);
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                UInt16 u16Temp = (UInt16)(buf[index] + buf[index + 1] * 256);
                strTmp = "通信速率    ：" + (u16Temp & 0x7FFF) + ((u16Temp >> 15) > 0 ? " Kbps" : " bps");
                payloadNode.Nodes.Add(strTmp);
                index += 2;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainRouteSet_ActiveSubNodeAutoRegister(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 10) return payloadNode;

                strTmp = "开始时间：20"
                        + buf[index + 5].ToString("X2") + "-"
                        + buf[index + 4].ToString("X2") + "-"
                        + buf[index + 3].ToString("X2")
                        + buf[index + 2].ToString("X2") + ":"
                        + buf[index + 1].ToString("X2") + ":"
                        + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 6;

                strTmp = "持续时间：" + (buf[index] + buf[index + 1] * 256) + " 分钟";
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "从节点重发次数：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "随机等待时间片个数：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainRouteSet_DisableSubNodeAutoRegister(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                return null;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainRouteSet_SetNetworkScale(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 2) return payloadNode;

                strTmp = "网络规模：" + (buf[index]  + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainRouteSet_StartNetworkMaintain(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                return null;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainRouteSet_StartBuildingNetwork(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                return null;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        #endregion

        #region 12 路由控制
        private static TreeNode ExplainRouteCtrl_Reboot(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                return null;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainRouteCtrl_Pause(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                return null;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainRouteCtrl_Resume(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                return null;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        #endregion

        #region 13 路由数据转发
        private static TreeNode ExplainRouteDataTransfer_Transfer(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 3) return payloadNode;

                strTmp = "通信协议：" + (buf[index] >= CommProtoTbl.Length ? "无法识别" : CommProtoTbl[buf[index]]);
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "通信延时相关性标志：" + "通信数据与延时" + ((buf[index] & 0x01) > 0 ? "有关" : "无关");
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                int nodeCnt = buf[index];
                strTmp = "从节点附属节点数量：" + nodeCnt;
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (buf.Length < index + nodeCnt * 6 + 1) return payloadNode;

                for (int i = 0; i < nodeCnt; i++)
                {
                    strTmp = "附属节点" + (i + 1) + "：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                    payloadNode.Nodes.Add(strTmp);
                    index += LongAddrSize;
                }

                if (buf.Length < index + buf[index] + 1) return payloadNode;

                strTmp = "报文长度：" + buf[index];
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

            }
            else
            {
                // 应答 
                if (buf == null) return null;

                if (buf.Length < 4) return payloadNode;

                strTmp = "本地通信上行时间：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "通信协议：" + (buf[index] >= CommProtoTbl.Length ? "无法识别" : CommProtoTbl[buf[index]]);
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (buf.Length < index + buf[index] + 1) return payloadNode;

                strTmp = "报文长度：" + buf[index];
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
            }

            return payloadNode;
        }
        #endregion

        #region 14 路由数据抄读
        private static TreeNode ExplainRouteDataRead_RequestReadContent(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 9) return payloadNode;

                strTmp = "通信相位  ：" + ((buf[index] == 0 || buf[index] > 3) ? "未知相位" : ("第" + buf[index] + "相"));
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "从节点地址：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                payloadNode.Nodes.Add(strTmp);
                index += LongAddrSize;
                
                strTmp = "从节点序号：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
            }
            else
            {
                // 应答 
                if (buf == null) return null;

                if (buf.Length < 4) return payloadNode;

                int readFlg = buf[index];
                strTmp = "抄读标志：";
                switch(readFlg)
                {
                    case 0: strTmp += "抄读失败";   break;
                    case 1: strTmp += "抄读成功"; break;
                    case 2: strTmp += "可以抄读"; break;
                    default: strTmp += "无法识别"; break;
                }
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "通信延时相关性标志：" + "通信数据与延时" + ((buf[index] & 0x01) > 0 ? "有关" : "无关");
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (readFlg == 2) // 可以继续抄读
                {
                    if (buf.Length < index + buf[index] + 2) return payloadNode;

                    strTmp = "报文长度：" + buf[index];
                    payloadNode.Nodes.Add(strTmp);
                    if(buf[index] > 12)
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
                    index += buf[index] + 1;

                    int nodeCnt = buf[index];
                    strTmp = "从节点附属节点数量：" + nodeCnt;
                    payloadNode.Nodes.Add(strTmp);
                    index += 1;

                    if (buf.Length < index + nodeCnt * 6) return payloadNode;

                    for (int i = 0; i < nodeCnt; i++)
                    {
                        strTmp = "附属节点" + (i + 1) + "：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                        payloadNode.Nodes.Add(strTmp);
                        index += LongAddrSize;
                    }
                }
            }

            return payloadNode;
        }
        private static TreeNode ExplainRouteDataRead_RequestConcentratorClock(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                
            }
            else
            {
                // 应答
                if (buf == null) return null;

                if (buf.Length < 6) return payloadNode;

                strTmp = "当前时间：" + DateTime.Now.Year/100
                        + buf[index + 5].ToString("X2") + "-"
                        + buf[index + 4].ToString("X2") + "-"
                        + buf[index + 3].ToString("X2") + " "
                        + buf[index + 2].ToString("X2") + ":"
                        + buf[index + 1].ToString("X2") + ":"
                        + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 6;
            }

            return payloadNode;
        }
        private static TreeNode ExplainRouteDataRead_RequstAdjustCommData(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 9) return payloadNode;

                strTmp = "从节点地址  ：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true);
                payloadNode.Nodes.Add(strTmp);
                index += LongAddrSize;

                strTmp = "预计延时时间：" + (buf[index] + buf[index + 1] * 256) + " 秒";
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                if (buf.Length < index + buf[index] + 1) return payloadNode;

                strTmp = "报文长度：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

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
            else
            {
                // 应答 
                if (buf == null) return null;

                if (buf.Length < 1) return payloadNode;

                strTmp = "报文长度：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

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

        #region 15 文件传输
        private static TreeNode ExplainFileTransfer_TransferModeOne(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 9) return payloadNode;

                strTmp = "文件标识：";
                switch (buf[index])
                {
                    case 0: strTmp += "清除下装文件"; break;
                    case 3: strTmp += "本地通信模块升级文件"; break;
                    case 7: strTmp += "主节点和子节点模块升级"; break;
                    case 8: strTmp += "子节点模块升级"; break;
                    default: strTmp += "无法识别"; break;
                }
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "文件属性：" + ((buf[index] & 0x01) == 0 ? "起始帧/中间帧" : "结束帧");
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "文件指令：" + (buf[index] == 0 ? "报文方式下装" : "无法识别");
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "文件总段数：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "当前段序号：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "当前段长度：" + (buf[index] + buf[index + 1] * 256) + " byte";
                payloadNode.Nodes.Add(strTmp);
                index += 2;
            }
            else
            {
                // 应答 
                if (buf == null) return null;

                if (buf.Length < 2) return payloadNode;

                strTmp = "当前段序号：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
            }

            return payloadNode;
        }
        #endregion

        #region 20 水表上报
        private static TreeNode ExplainWaterMeterReport_ReadWaterMeterDataStart(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                return null;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainWaterMeterReport_ReadWaterMeterDataStop(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                return null;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainWaterMeterReport_MainNodeReportWaterMeterData(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 1) return payloadNode;

                strTmp = "上报帧序号：" + buf[index] + " (" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                int nodeCnt = buf[index];
                strTmp = "水表数量  ：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (buf.Length < index + nodeCnt * 25) return payloadNode;

                TreeNode node = null;
                for (int i = 0; i < nodeCnt; i++)
                {
                    strTmp = "水表" + (i + 1) + "：" + Util.GetStringHexFromBytes(buf, index, 7, "", true);
                    node = new TreeNode(strTmp);
                    payloadNode.Nodes.Add(node);
                    index += 7;
                    {
                        strTmp = "上报时间：" + DateTime.Now.Year/100
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
                                + buf[index + 1].ToString("X2") 
                                + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 4;
                        strTmp = "统计日期：" + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 1;
                        strTmp = "上月累计："
                                + buf[index + 3].ToString("X2")
                                + buf[index + 2].ToString("X2")
                                + buf[index + 1].ToString("X2")
                                + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 4;
                        strTmp = "水表状态：" + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 1;
                        strTmp = "出厂年份：" + DateTime.Now.Year / 100 + buf[index].ToString("X2");
                        node.Nodes.Add(strTmp);
                        index += 1;
                    }
                }
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        private static TreeNode ExplainWaterMeterReport_MainNodeReportWaterMeterDataComplete(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                return null;
            }
            else
            {
                // 应答 - 确认/否认帧
            }

            return payloadNode;
        }
        #endregion

        #region F0 内部调试
        private static TreeNode ExplainInnerTest_ReadLogByType(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 8) return payloadNode;

                strTmp = "读取规则：";
                switch (buf[index])
                {
                    case 0x00: strTmp += "时-日志"; break;
                    case 0x01: strTmp += "日-日志"; break;
                    case 0x02: strTmp += "月-日志"; break;
                    default: strTmp += "无法识别"; break;
                }
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "读取时间：" + buf[index] + "/" + buf[index + 1] + " " + buf[index + 2] + "时";
                payloadNode.Nodes.Add(strTmp);
                index += 3;
                strTmp = "记录包号：" + (buf[index] + (buf[index + 1] << 8) + (buf[index + 1] << 16) + (buf[index + 1] << 24));
                payloadNode.Nodes.Add(strTmp);
                index += 4;
            }
            else
            {
                // 应答 
                if (buf == null) return null;

                if (buf.Length < 5) return payloadNode;

                strTmp = "回复标志：";
                switch (buf[index])
                {
                    case 0x00: strTmp += "记录未结束"; break;
                    case 0x01: strTmp += "当前时段记录正常结束"; break;
                    case 0x02: strTmp += "当前时段记录异常结束"; break;
                    case 0x03: strTmp += "当前时段记录不存在"; break;
                    case 0x04: strTmp += "当前包序号错误"; break;
                    default: strTmp += "无法识别"; break;
                }
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "记录包号：" + (buf[index] + (buf[index + 1] << 8) + (buf[index + 1] << 16) + (buf[index + 1] << 24));
                payloadNode.Nodes.Add(strTmp);
                index += 4;
                strTmp = "记录数据：" + (buf.Length - index) + " byte";
                payloadNode.Nodes.Add(strTmp);
            }

            return payloadNode;
        }
        private static TreeNode ExplainInnerTest_SetBroadcastMaintain(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 9) return payloadNode;

                strTmp = "广播维护开关：";
                switch (buf[index])
                {
                    case 0x00: strTmp += "关闭"; break;
                    case 0x01: strTmp += "开启"; break;
                    default: strTmp += "无法识别"; break;
                }
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "当前时间：" + (DateTime.Now.Year/100)
                        + buf[index + 5].ToString("X2") + "/"
                        + buf[index + 4].ToString("X2") + "/"
                        + buf[index + 3].ToString("X2") + " "
                        + buf[index + 2].ToString("X2") + ":"
                        + buf[index + 1].ToString("X2") + ":"
                        + buf[index + 0].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 6;
                strTmp = "维护时间：" + buf[index + 1].ToString("X2") + ":" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 2;
            }
            else
            {
                // 应答 - 确认/否认
                
            }

            return payloadNode;
        }
        private static TreeNode ExplainInnerTest_ReadSubNodeParamsInfo(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 
                if (buf == null) return null;

                if (buf.Length < 7) return payloadNode;

                strTmp = "读取类型：";
                switch(buf[index])
                {
                    case 0x00: strTmp += "档案信息"; break;
                    case 0x01: strTmp += "邻居表"; break;
                    case 0x02: strTmp += "路径表"; break;
                    default: strTmp += "无法识别"; break;
                }
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "节点地址：" + Util.GetStringHexFromBytes(buf, index, 6, "", true);
                payloadNode.Nodes.Add(strTmp);
                index += 6;
            }
            else
            {
                // 应答 
                if (buf == null) return null;

                if (buf.Length < 7) return payloadNode;

                string strReadType = "读取类型：";
                switch (buf[index])
                {
                    case 0x00: strReadType += "档案信息"; break;
                    case 0x01: strReadType += "邻居表"; break;
                    case 0x02: strReadType += "路径表"; break;
                    default: strReadType += "无法识别"; break;
                }
                payloadNode.Nodes.Add(strReadType);
                index += 1;

                strTmp = "节点地址：" + Util.GetStringHexFromBytes(buf, index, 6, "", true);
                payloadNode.Nodes.Add(strTmp);
                index += 6;

                if (strReadType.Contains("档案信息"))
                {
                    if (buf.Length < index + 22) return payloadNode;

                    TreeNode node = new TreeNode("档案信息");
                    payloadNode.Nodes.Add(node);
                    {
                        index += 7;     // 7字节地址不用显示（一般最高字节为0，与6字节地址一样）

                        strTmp = "节点标记：";
                        {
                            strTmp += "已删除" + ((buf[index] & 0x01) > 0 ? "\u26AB" : "\u26AA");  // 显示实心圆或空心圆
                            strTmp += " 已点名" + ((buf[index] & 0x02) > 0 ? "\u26AB" : "\u26AA");
                            strTmp += " 已配置" + ((buf[index] & 0x04) > 0 ? "\u26AB" : "\u26AA");
                            strTmp += " 已发现" + ((buf[index] & 0x08) > 0 ? "\u26AB" : "\u26AA");
                            strTmp += " 事件上报" + ((buf[index] & 0x10) > 0 ? "\u26AB" : "\u26AA");
                            strTmp += " 存储失败" + ((buf[index] & 0x20) > 0 ? "\u26AB" : "\u26AA");
                            strTmp += " 已维护" + ((buf[index] & 0x40) > 0 ? "\u26AB" : "\u26AA");
                        }
                        node.Nodes.Add(strTmp);
                        index += 1;

                        UInt16 u16Tmp = (UInt16)(buf[index] + buf[index + 1] * 256);
                        string strNodeTp = "";
                        strTmp = "节点类型：";
                        switch ((u16Tmp & 0x07))
                        {
                            case 0x00: strNodeTp += "水气表"; break;
                            case 0x01: strNodeTp += "97-电表"; break;
                            case 0x02: strNodeTp += "07-电表"; break;
                            case 0x03: strNodeTp += "主动上报水表"; break;
                            case 0x07: strNodeTp += "698-电表"; break;
                            default: strNodeTp += "无法识别"; break;
                        }
                        node.Nodes.Add(strTmp + strNodeTp);
                        node.Text = node.Text + "(" + strNodeTp + ")";

                        strTmp = "路径数量：" + ((u16Tmp >> 3) & 0x07);
                        node.Nodes.Add(strTmp);
                        strTmp = "抄表路径优先级：" + ((u16Tmp >> 6) & 0x07);
                        node.Nodes.Add(strTmp);
                        strTmp = "当前路径优先级：" + ((u16Tmp >> 9) & 0x07);
                        node.Nodes.Add(strTmp);
                        strTmp = "点名/配置计数器：" + ((u16Tmp >> 12) & 0x0F);
                        node.Nodes.Add(strTmp);
                        index += 2;
                        strTmp = "App更新标志 ：" + (buf[index] & 0x0F);
                        node.Nodes.Add(strTmp);
                        strTmp = "子节点类属性：" + ((buf[index] >> 4) & 0x0F);
                        node.Nodes.Add(strTmp);
                        index += 1;
                        strTmp = "广播维护标记：" + (buf[index] & 0x01);
                        node.Nodes.Add(strTmp);
                        strTmp = "邻居数量    ：" + ((buf[index] >> 1) & 0x7F);
                        node.Nodes.Add(strTmp);
                        index += 1;
                        u16Tmp = (UInt16)(buf[index] + buf[index + 1] * 256);
                        strTmp = "当前路径成本：" + (u16Tmp & 0x07FF);
                        node.Nodes.Add(strTmp);
                        strTmp = "层次号      ：" + ((u16Tmp >> 11) & 0x0F);
                        node.Nodes.Add(strTmp);
                        strTmp = "水气表已点名：" + (((u16Tmp >> 15) & 0x01) > 0 ? "是" : "否");
                        node.Nodes.Add(strTmp);
                        index += 2;
                        u16Tmp = (UInt16)(buf[index] + buf[index + 1] * 256);
                        strTmp = "时隙号      ：" + (u16Tmp & 0x03FF);
                        node.Nodes.Add(strTmp);
                        strTmp = "在线标记    ：" + (((u16Tmp >> 10) & 0x01) > 0 ? "在网" : "离网");
                        node.Nodes.Add(strTmp);
                        strTmp = "电表已点名  ：" + (((u16Tmp >> 11) & 0x01) > 0 ? "是" : "否");
                        node.Nodes.Add(strTmp);
                        strTmp = "重新计算路径：" + (((u16Tmp >> 12) & 0x01) > 0 ? "是" : "否");
                        node.Nodes.Add(strTmp);
                        strTmp = "组网抄表正常：" + (((u16Tmp >> 13) & 0x01) > 0 ? "是" : "否");
                        node.Nodes.Add(strTmp);
                        strTmp = "抄表信道组  ：" + (((u16Tmp >> 14) & 0x01) > 0 ? "公共信道" : "工作信道");
                        node.Nodes.Add(strTmp);
                        strTmp = "已抄主动上报水气表：" + (((u16Tmp >> 15) & 0x01) > 0 ? "是" : "否");
                        node.Nodes.Add(strTmp);
                        index += 2;
                        strTmp = "抄表成功率    ：" + ( buf[index] == 255 ? "无" : buf[index] + "%");
                        node.Nodes.Add(strTmp);
                        index += 1;
                        strTmp = "抄表信道组备份：" + (buf[index] & 0x0F);
                        node.Nodes.Add(strTmp);
                        strTmp = "抄表优先级备份：" + ((buf[index] >> 4) & 0x0F);
                        node.Nodes.Add(strTmp);
                        index += 1;
                        strTmp = "抄表成功次数  ：" + (buf[index] + buf[index + 1] * 256);
                        node.Nodes.Add(strTmp);
                        index += 2;
                        strTmp = "抄表总次数    ：" + (buf[index] + buf[index + 1] * 256);
                        node.Nodes.Add(strTmp);
                        index += 2;
                    }
                }
                else if (strReadType.Contains("邻居表"))
                {
                    int Cnt, docNo;
                    Cnt = buf[index];
                    TreeNode node = new TreeNode("邻居个数" + "(" + Cnt + ")");
                    payloadNode.Nodes.Add(node);
                    index += 1;

                    if (buf.Length < index + Cnt * 4) return payloadNode;

                    for(int i = 0; i < Cnt; i++)
                    {
                        docNo = ((buf[index] + buf[index + 1] * 256) & 0xFFF);
                        index += 2;
                        strTmp = "邻居" + (i + 1).ToString("D2") + "："
                                + " 索引 " + docNo
                                + " (上 " + (buf[index] == 255 ? "无" : buf[index].ToString()) + " , 下 " 
                                + (buf[index + 1] == 255 ? "无" : buf[index + 1].ToString()) + ")";
                        node.Nodes.Add(strTmp);
                        index += 2;
                    }
                }
                else if (strReadType.Contains("路径表"))
                {
                    int Cnt = buf[index], docNo;
                    TreeNode node = new TreeNode("路径条数" + "(" + Cnt + ")");
                    payloadNode.Nodes.Add(node);
                    index += 1;

                    if (buf.Length < index + Cnt * 20) return payloadNode;

                    for (int i = 0; i < Cnt; i++)
                    {
                        int jumps = ((buf[index + 14] >> 4) & 0x0F);

                        strTmp = "路径" + (i + 1).ToString("D2") + " [" + jumps + "跳]：-> ";
                        for (int k = 0; k < jumps -1; k++ )
                        {
                            docNo = (buf[index + k * 2] + buf[index + k * 2 + 1] * 256);
                            strTmp += "[" + docNo + "]" + "-> ";
                        }
                        strTmp += "中心";
                        index += 12;
                        TreeNode subNode = new TreeNode(strTmp);
                        node.Nodes.Add(subNode);
                        {
                            strTmp = "相关性  ：" + (buf[index] & 0x1F);
                            subNode.Nodes.Add(strTmp);
                            strTmp = "路径成本：" + ((buf[index] + buf[index + 1] * 256) >> 5);
                            subNode.Nodes.Add(strTmp);
                            index += 2;
                            strTmp = "优先级  ：" + (buf[index] & 0x0F);
                            subNode.Nodes.Add(strTmp);
                            strTmp = "跳数    ：" + (buf[index] >> 4);
                            subNode.Nodes.Add(strTmp);
                            index += 1;
                            strTmp = "成功次数：" + (buf[index] & 0x7F);
                            subNode.Nodes.Add(strTmp);
                            strTmp = "失败次数：" + (buf[index + 1] & 0x7F);
                            subNode.Nodes.Add(strTmp);
                            strTmp = "路径场强：" + ((buf[index] & 0x80) > 0 ? "已超标" : "未超标");
                            subNode.Nodes.Add(strTmp);
                            strTmp = "邻居路径：" + ((buf[index + 1] & 0x80) > 0 ? "已验证" : "未验证");
                            subNode.Nodes.Add(strTmp);
                            index += 2;
                            strTmp = "路径1-6相关性：" + (buf[index] & 0x0F) + "-" + (buf[index] >> 4)
                                    + "-" + (buf[index + 1] & 0x0F) + "-" + (buf[index + 1] >> 4)
                                    + "-" + (buf[index + 2] & 0x0F) + "-" + (buf[index + 2] >> 4);
                            subNode.Nodes.Add(strTmp);
                            index += 3;
                        }
                    }
                }
                else
                {
                    // do nothing
                }

            }

            return payloadNode;
        }
        private static TreeNode ExplainInnerTest_ReadSubNodeSummaryInfo(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 - 无数据载荷
                return null;
            }
            else
            {
                // 应答 
                if (buf == null) return null;

                if (buf.Length < 17) return payloadNode;

                strTmp = "总数量  ：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "发现数量：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "点名数量：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "配置数量：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "维护数量：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "中心邻居数量：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "组网时间：" + buf[index] + " 分钟";
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "工作信道组：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "广播维护开关：" + (buf[index] == 0 ?  "关" : "开");
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "广播维护时间：" + buf[index].ToString("X2") + ":" + buf[index + 1].ToString("X2") + ":00";
                payloadNode.Nodes.Add(strTmp);
                index += 2;
            }

            return payloadNode;
        }
        private static TreeNode ExplainInnerTest_ReadMainNodeNeighborTbl(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            string strTmp = "";
            byte[] buf = frame.DataBuf;
            int index = 0;

            if (frame.CtrlWord.StartFlag)
            {
                // 请求 - 无数据载荷
                if (buf == null) return null;

                if (buf.Length < index + 1) return payloadNode;

                strTmp = "当前页号：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }
            else
            {
                // 应答 
                if (buf == null) return null;

                if (buf.Length < index + 3) return payloadNode;

                strTmp = "总页数  ：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;
                strTmp = "当前页号：" + buf[index];
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                int cnt = buf[index];
                TreeNode node = new TreeNode("邻居个数：" + buf[index]);
                payloadNode.Nodes.Add(node);
                index += 1;

                if (buf.Length < index + cnt * 8) return payloadNode;

                for(int i = 0; i < cnt; i++)
                {
                    strTmp = "邻居" + (i + 1) + "：" + Util.GetStringHexFromBytes(buf, index, LongAddrSize, "", true)
                            + " (上 " + buf[index + 6] + " , 下 " + buf[index + 7] + ")";
                    node.Nodes.Add(strTmp);
                    index += 8;
                }
            }

            return payloadNode;
        }
        #endregion

        #endregion /* 详细命令解析 */

        #region CRC8 计算
        public static byte CalCRC8(byte[] dataBuf, int startIndex, int length)
        {
            byte crc = 0;

            for (int i = 0; i < length; i++)
            {
                crc += dataBuf[startIndex + i];
            }

            return crc;
        }
        #endregion
    }
}
