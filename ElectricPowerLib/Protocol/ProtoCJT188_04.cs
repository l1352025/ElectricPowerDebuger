using ElectricPowerLib.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ElectricPowerLib.Protocol
{
#pragma warning disable
    public class ProtoCJT188_04
    {
        public const byte FrameHeader = 0x68;           // 帧头
        public const byte FrameTail = 0x16;             // 帧尾
        public const byte FrameFixedLen = 13;           // 0x68, 仪表类型(1), 地址(7), 控制域(1), 长度(1), 数据域（N），校验和(1), 0x16
        public const byte LongAddrSize = 7;             // 地址的长度
        public const string BroadcastAddr = "99999999999999"; // 广播地址
        public const string CommonUseAddr = "AAAAAAAAAAAAAA"; // 通配地址

        #region 帧格式定义
        // 通信报文格式
        public struct FrameFormat
        {
            public byte Header;                         // 帧头：  0x68
            public byte MeterType;                      // 仪表类型
            public byte[] DevAddr;                      // 设备地址
            public CtrlField CtrlWord;                  // 控制字
            public int DataLen;                         // 数据域长度
            public byte[] DataBuf;                      // 数据域： （发送前 + 0x33）
            public byte Crc8;                           // Crc8校验： 帧头->CRC8之前的累加和
            public byte Tail;                           // 帧尾 ： 0x16

            public string ErrorInfo;                    // 帧错误信息
        };

        // 仪表类型
        public enum MeterType
        {
            WaterMeter_Cold     = 0x10,     // 冷水表
            WaterMeter_Hot      = 0x11,     // 热水表
            WaterMeter_Drink    = 0x12,     // 直饮水水表
            WaterMeter_Middle   = 0x13,     // 中水水表

            HeatMeter_Hot       = 0x20,     // 热量表（计热量）
            HeatMeter_Cold      = 0x21,     // 热量表（计冷量）

            GasMeter            = 0x30,     // 燃气表           

            WattHourMeter       = 0x40,     // 电度表
        }

        // 控制域
        public struct CtrlField
        {
            public CommandType CmdType;         // 命令类型 bit5-0
            public bool IsErrorAck;             // 错误应答标志 bit6
            public bool IsAckFrame;             // 应答帧标志 bit7 

            public byte All;
        }

        // 功能码
        public enum CommandType
        {
            ReadData            = 0x01,       // 读数据
            WriteData           = 0x04,       // 写数据
            ReadSecretKeyVer    = 0x09,       // 读密钥版本号
            ReadDevAddr         = 0x03,       // 读地址（表号）
            WriteDevAddr        = 0x15,       // 写地址（表号）
            WriteMeterSyncData  = 0x16,       // 写机电同步数（置表底数）

            Invalid = 0xFF
        }
        #endregion

        // 帧解析表配置
        public delegate TreeNode ExplainCallback(FrameFormat frame);
        public struct CmdExplain
        {
            public byte Cmd;
            public String CmdName;
            public Color CmdColor;
            public ExplainCallback CmdExplainFunc;

            public CmdExplain(byte cmd, String cmdName, Color color, ExplainCallback callback)
            {
                Cmd = cmd;
                CmdName = cmdName;
                CmdColor = color;
                CmdExplainFunc = callback;
            }
        }

        private static readonly CmdExplain[] FrameExplainTbl = new CmdExplain[]
        {
            new CmdExplain(0x01, "读数据",             Color.Black, ExplainDataItem),
            new CmdExplain(0x04, "写数据",             Color.Black, ExplainDataItem),
            new CmdExplain(0x03, "读地址",             Color.Black, ExplainDataItem),
            new CmdExplain(0x15, "写地址",             Color.Black, ExplainDataItem),
            new CmdExplain(0x09, "读密钥版本号",       Color.Black, ExplainDataItem),
            new CmdExplain(0x16, "写机电同步数",       Color.Black, ExplainDataItem),
        };

        public struct DataExplain
        {
            public byte DI0, DI1;
            public String DataName;
            public ExplainCallback DataExplainFunc;

            public DataExplain(byte di0, byte di1, String dataName, ExplainCallback callback)
            {
                DI0 = di0;
                DI1 = di1;
                DataName = dataName;
                DataExplainFunc = callback;
            }
        }
        private static readonly DataExplain[] DataExplainTbl = new DataExplain[]
        {
            //------------  DI0   DI1    数据项名称            解析函数  -------------

            // 901F - 读计量数据       T= 10H一19H 和 T=30H^49H , 应答16H byte,  T= 20 H-29 H, 应答2EH byte
            new DataExplain(0x90, 0x1F, "读计量数据",       ExplainReadData),     
            
            // D12x - 读历史计量数据 DI1(20 ~ 2B) 表示上 1~12个月的历史计量数据 
            new DataExplain(0xD1, 0x20, "读历史计量数据",   ExplainReadData),      // 8 byte ：数据标识DI，序号SER，上1月结算日累积流量
            
            // 810x - 读其他数据
            new DataExplain(0x81, 0x02, "读价格表",         ExplainReadData),
            new DataExplain(0x81, 0x03, "读结算日",         ExplainReadData),      
            new DataExplain(0x81, 0x04, "读抄表日",         ExplainReadData),
            new DataExplain(0x81, 0x05, "读购入金额",       ExplainReadData),      
            
            // 8106 - 读密钥版本号
            new DataExplain(0x81, 0x06, "读密钥版本号",   ExplainReadSecretKeyVersion),  

            // 810A - 读地址
            new DataExplain(0x81, 0x0A, "读地址",          ExplainReadDevAddr),  


            // A01x - 写其他数据
            new DataExplain(0xA0, 0x10, "写价格表",         ExplainWriteData), 
            new DataExplain(0xA0, 0x11, "写结算日",         ExplainWriteData), 
            new DataExplain(0xA0, 0x12, "写抄表日",         ExplainWriteData), 
            new DataExplain(0xA0, 0x13, "写购入金额",       ExplainWriteData), 
            new DataExplain(0xA0, 0x14, "写新密钥",         ExplainWriteData), 
            new DataExplain(0xA0, 0x15, "写标准时间",       ExplainWriteData), 
            new DataExplain(0xA0, 0x17, "写阀门控制",       ExplainWriteData), 
            new DataExplain(0xA0, 0x19, "出厂启用",         ExplainWriteData), 

            // A016 - 写机电同步数据
            new DataExplain(0xA0, 0x16, "写机电同步数据",  ExplainWriteMeterSyncData), 
            
            // A018 - 写地址
            new DataExplain(0xA0, 0x18, "写地址",          ExplainWriteDevAddr), 
            
        };


        #region 协议帧提取

        // 协议帧提取
        public static FrameFormat ExplainRxPacket(byte[] rxBuf)
        {
            FrameFormat rxData = new FrameFormat();

            try
            {
                int index = 0, startIdx = -1;

                for (index = 0; index < rxBuf.Length && rxBuf.Length > index + 11; index++)
                {
                    // 跳过唤醒字FEFEFEFE， 帧头和长度判断
                    if (rxBuf[index] == 0x68)   
                    {
                        startIdx = index;
                        if (rxBuf.Length >= (index + FrameFixedLen + rxBuf[index + 10]))
                        {
                            break;
                        }
                    }
                }

                if (startIdx == -1) throw new Exception("帧头错误");
                if (rxBuf.Length < index + FrameFixedLen + rxBuf[index + 10]) throw new Exception("长度错误");

                rxData.Header = rxBuf[index++];         // 帧头
                rxData.MeterType = rxBuf[index++];      // 表类型

                rxData.DevAddr = new byte[LongAddrSize];// 通信地址     
                Array.Copy(rxBuf, index, rxData.DevAddr, 0, rxData.DevAddr.Length);
                index += rxData.DevAddr.Length;

                rxData.CtrlWord.All = rxBuf[index];     // 控制域
                rxData.CtrlWord.IsAckFrame = (rxBuf[index] & 0x80) > 0 ? true : false;
                rxData.CtrlWord.IsErrorAck = (rxBuf[index] & 0x40) > 0 ? true : false;
                rxData.CtrlWord.CmdType = (CommandType)(rxBuf[index] & 0x3F);
                index++;

                rxData.DataLen = rxBuf[index++];    // 数据域长度

                if (rxData.DataLen > 0)             //数据区
                {
                    rxData.DataBuf = new byte[rxData.DataLen];
                    for (int i = 0; i < rxData.DataLen; i++)
                    {
                        rxData.DataBuf[i] = rxBuf[index++];
                    }
                }
                rxData.Crc8 = rxBuf[index++];       //校验和

                byte chksum = 0;
                for (int i = startIdx; i < index - 1; i++)
                {
                    chksum += rxBuf[i];
                }
                if (rxData.Crc8 != chksum) throw new Exception("校验错误");

                rxData.Tail = rxBuf[index++];       //帧尾

                rxData.ErrorInfo = "";
            }
            catch (Exception ex)
            {
                switch (ex.Message)
                {
                    case "帧头错误":
                    case "长度错误":
                        rxData.CtrlWord.CmdType = CommandType.Invalid;
                        rxData.ErrorInfo = ex.Message;
                        break;

                    case "校验错误":
                        rxData.ErrorInfo = ex.Message;
                        break;

                    default:
                        rxData.CtrlWord.CmdType = CommandType.Invalid;
                        rxData.ErrorInfo = "数据异常";
                        string msg = "ProtoCJT188_04.ExplainRxPacket() Error: " + ex.Message + "\r\n  " + Util.GetStringHexFromBytes(rxBuf, 0, rxBuf.Length, " ");
                        LogHelper.WriteLine("error.log", msg);
                        break;
                }
            }

            return rxData;
        }
        #endregion

        #region 协议帧解析

        // 解析 仪表类型名
        public static string GetMeterTypeName(byte typeCode)
        {
            string typeName = "";

            switch(typeCode)
            {
                case 0x10: typeName = "冷水水表"; break;
                case 0x11: typeName = "热水水表"; break;
                case 0x12: typeName = "直饮水水表"; break;
                case 0x13: typeName = "中水水表"; break;
                case 0x19: typeName = "电子水表"; break;
                case 0x20: typeName = "热量表(计热)"; break;
                case 0x21: typeName = "热量表(计冷)"; break;
                case 0x30: typeName = "燃气表"; break;
                case 0x40: typeName = "其他仪表"; break;
                default: 
                    typeName = "未知仪表"; 
                    break;
            }

            return typeName;
        }

        // 解析 数据项类型
        public static string GetDataType(FrameFormat frame)
        {
            string dataType = "无法识别";

            foreach (DataExplain dat in DataExplainTbl)
            {
                if (dat.DI0 == frame.DataBuf[0] && dat.DI1 == frame.DataBuf[1])
                {
                    dataType = dat.DataName;
                }
                else if (0xD1 == frame.DataBuf[0] && dat.DI1 == (frame.DataBuf[1] & 0xF0))
                {
                    dataType = dat.DataName + ((frame.DataBuf[1] & 0x0F) + 1);  // 读历史计量数据1~12
                }
            }

            return dataType;
        }

        // 解析 帧类型
        public static string GetFrameType(FrameFormat frame)
        {
            string frameType = "无法识别";

            foreach (CmdExplain cmd in FrameExplainTbl)
            {
                if (cmd.Cmd == (byte)frame.CtrlWord.CmdType)
                {
                    frameType = cmd.CmdName;

                    if (frame.CtrlWord.IsAckFrame)
                    {
                        frameType += "-应答";
                    }
                }
            }

            return frameType;
        }

        // 解析 帧类型和数据项类型
        public static string GetFrameTypeAndDataType(FrameFormat frame)
        {
            string type = "无法识别";

            foreach (CmdExplain cmd in FrameExplainTbl)
            {
                if (cmd.Cmd == (byte)frame.CtrlWord.CmdType)
                {
                    type = cmd.CmdName;
                    if (frame.CtrlWord.IsAckFrame)
                    {
                        type += "-应答";
                    }

                    if (frame.CtrlWord.CmdType == CommandType.ReadData
                        || frame.CtrlWord.CmdType == CommandType.WriteData
                        )
                    {
                        type += " [" + GetDataType(frame) + "]";
                    }
                }
            }

            return type;
        }

        // 解析 帧颜色
        public static Color GetFrameColor(FrameFormat frame)
        {
            Color frameColor = Color.Black;

            foreach (CmdExplain cmd in FrameExplainTbl)
            {
                if (cmd.Cmd == (byte)frame.CtrlWord.CmdType)
                {
                    frameColor = cmd.CmdColor;
                }
            }

            return frameColor;
        }

        // 解析 帧数据部分
        public static TreeNode ExplainFrameData(FrameFormat frame)
        {
            TreeNode node = null;

            foreach (CmdExplain cmd in FrameExplainTbl)
            {
                if (cmd.Cmd == (byte)frame.CtrlWord.CmdType)
                {
                    node = cmd.CmdExplainFunc(frame);
                }
            }

            return node;
        }

        // 解析 帧数据项
        public static TreeNode ExplainDataItem(FrameFormat frame)
        {
            TreeNode node = null;

            foreach (DataExplain dat in DataExplainTbl)
            {
                if (dat.DI0 == frame.DataBuf[0]
                    && dat.DI1 == frame.DataBuf[1]
                    )
                {
                    node = dat.DataExplainFunc(frame);
                }
            }

            return node;
        }

        // 解析 协议帧
        public static TreeNode GetProtoTree(byte[] databuf)
        {
            FrameFormat frame = ExplainRxPacket(databuf);
            TreeNode node = null;
            string strTmp = (frame.ErrorInfo != "" ? (" (" + frame.ErrorInfo + " )") : "");
            TreeNode parentNode = new TreeNode("188-04报文" + strTmp);

            if (frame.CtrlWord.CmdType == CommandType.Invalid)
            {
                return parentNode;
            }

            // parentNode--仪表类型
            strTmp = "仪表类型：" + frame.MeterType.ToString("X2") + " " + GetMeterTypeName(frame.MeterType);
            parentNode.Nodes.Add(strTmp);

            // parentNode--通信地址
            strTmp = "通信地址：" + Util.GetStringHexFromBytes(frame.DevAddr, 0, LongAddrSize, "", true);
            parentNode.Nodes.Add(strTmp);

            // parentNode--控制域
            strTmp = "控制域  ：" + frame.CtrlWord.All.ToString("X2");
            node = new TreeNode(strTmp);
            {
                strTmp = "命令类型：" + GetFrameType(frame);
                node.Nodes.Add(strTmp);
                strTmp = "传输方向：" + (frame.CtrlWord.IsAckFrame ? "应答" : "请求");
                node.Nodes.Add(strTmp);
                strTmp = "应答标识：" + (frame.CtrlWord.IsErrorAck ? "异常" : "正常");
                node.Nodes.Add(strTmp);
            }
            node.Expand();
            parentNode.Nodes.Add(node);

            // parentNode--帧长
            strTmp = "载荷长度：" + frame.DataLen;
            parentNode.Nodes.Add(strTmp);

            // parentNode--数据区
            if (frame.CtrlWord.IsErrorAck)
            {
                // 异常应答
                if (frame.DataBuf.Length < 3) return parentNode;

                strTmp = "帧序号  ：" + frame.DataBuf[0];
                parentNode.Nodes.Add(strTmp);
                strTmp = "错误标志：" + (frame.DataBuf[1] + (frame.DataBuf[2] << 8)).ToString("X4")
                                + " (" + GetMeterStatusInfo(frame.DataBuf[1], frame.DataBuf[2]) + ")";
                parentNode.Nodes.Add(strTmp);
            }
            else
            {
                // 正常应答 或 请求
                node = ExplainFrameData(frame);
                if (node != null)
                {
                    node.Expand();
                    foreach (TreeNode subnode in node.Nodes)
                    {
                        parentNode.Nodes.Add(subnode);
                    }
                }
            }

            return parentNode;
        }

        #endregion

        #region 读数据

        // 解析单位代号
        private static string GetUnitName(byte unitCode)
        {
            string unitName;

            switch(unitCode)
            {
                case 0x02: unitName = "Wh";     break;
                case 0x05: unitName = "kWh";    break;
                case 0x08: unitName = "MWh";    break;
                case 0x0A: unitName = "MWh x100"; break;
                case 0x01: unitName = "J";      break;
                case 0x0B: unitName = "kJ";     break;
                case 0x0E: unitName = "MJ";     break;
                case 0x11: unitName = "GJ";     break;
                case 0x13: unitName = "GJ x100"; break;
                case 0x14: unitName = "W";      break;
                case 0x17: unitName = "kW";     break;
                case 0x1A: unitName = "MW";     break;
                case 0x29: unitName = "L";      break;
                case 0x2C: unitName = "m³"; break;
                case 0x32: unitName = "L/h";    break;
                case 0x35: unitName = "m³/h"; break;
                default: unitName = "(未知单位)"; break;
            }

            return unitName;
        }

        // 解析状态ST
        private static string GetMeterStatusInfo(byte statusB1, byte statusB2)
        {
            string status  = "阀门-" + ((statusB1 & 0x03) == 0 ? "开启" : ((statusB1 & 0x03) == 1 ? "关闭" : "异常"))
                            + " | 电池-" + ((statusB1 & 0x04) == 0 ? "正常" : "欠压");
            return status;
        }

        // 读数据
        private static TreeNode ExplainReadData(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 3) return payloadNode;

                string strDataType = GetDataType(frame);
                strTmp = "数据标识：" + strDataType + " (" + buf[index].ToString("X2") + buf[index + 1].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "帧序号  ：" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }
            else
            {
                // 应答
                if (buf.Length < index + 3) return payloadNode;

                string strDataType = GetDataType(frame);
                strTmp = "数据标识：" + strDataType + " (" + buf[index].ToString("X2") + buf[index + 1].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "帧序号  ：" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (strDataType.Contains("读计量数据")) 
                {
                    if ((frame.MeterType >= 0x10 && frame.MeterType <= 0x19)    // 水表
                        || (frame.MeterType >= 0x30 && frame.MeterType <= 0x49) // 燃气表、其他仪表
                        )
                    {
                        if (buf.Length < index + 19) return payloadNode;

                        strTmp = "当前流量：" 
                            + buf[index + 3].ToString("X2") + buf[index + 2].ToString("X2") 
                            + buf[index + 1].ToString("X2") + "." + buf[index].ToString("X2")
                            + " " + GetUnitName(buf[index + 4]); // 单位
                        payloadNode.Nodes.Add(strTmp);
                        index += 5;
                        strTmp = "上月流量："
                            + buf[index + 3].ToString("X2") + buf[index + 2].ToString("X2")
                            + buf[index + 1].ToString("X2") + "." + buf[index].ToString("X2")
                            + " " + GetUnitName(buf[index + 4]); // 单位
                        payloadNode.Nodes.Add(strTmp);
                        index += 5;
                        strTmp = "实时时间："
                            + buf[index + 6].ToString("X2") + buf[index + 5].ToString("X2") + "-"
                            + buf[index + 4].ToString("X2") + "-"
                            + buf[index + 3].ToString("X2") + " "
                            + buf[index + 2].ToString("X2") + ":"
                            + buf[index + 1].ToString("X2") + ":"
                            + buf[index + 0].ToString("X2");
                        payloadNode.Nodes.Add(strTmp);
                        index += 7;
                        strTmp = "状态ST  ：" + GetMeterStatusInfo(buf[index], buf[index + 1]);
                        payloadNode.Nodes.Add(strTmp);
                        index += 2;
                    }
                    else if ((frame.MeterType >= 0x20 && frame.MeterType <= 0x29))  // 热量表
                    {
                        if (buf.Length < index + 43) return payloadNode;

                        strTmp = "上月热量："
                            + buf[index + 3].ToString("X2") + buf[index + 2].ToString("X2")
                            + buf[index + 1].ToString("X2") + "." + buf[index].ToString("X2")
                            + " " + GetUnitName(buf[index + 4]); // 单位
                        payloadNode.Nodes.Add(strTmp);
                        index += 5;
                        strTmp = "当前热量："
                            + buf[index + 3].ToString("X2") + buf[index + 2].ToString("X2")
                            + buf[index + 1].ToString("X2") + "." + buf[index].ToString("X2")
                            + " " + GetUnitName(buf[index + 4]); // 单位
                        payloadNode.Nodes.Add(strTmp);
                        index += 5;
                        strTmp = "热功率  ："
                            + buf[index + 3].ToString("X2") + buf[index + 2].ToString("X2")
                            + buf[index + 1].ToString("X2") + "." + buf[index].ToString("X2")
                            + " " + GetUnitName(buf[index + 4]); // 单位
                        payloadNode.Nodes.Add(strTmp);
                        index += 5;
                        strTmp = "流量    ："
                            + buf[index + 3].ToString("X2") + buf[index + 2].ToString("X2")
                            + "." + buf[index + 1].ToString("X2") + buf[index].ToString("X2")
                            + " " + GetUnitName(buf[index + 4]); // 单位
                        payloadNode.Nodes.Add(strTmp);
                        index += 5;
                        strTmp = "累计流量："
                            + buf[index + 3].ToString("X2") + buf[index + 2].ToString("X2")
                            + buf[index + 1].ToString("X2") + "." + buf[index].ToString("X2")
                            + " " + GetUnitName(buf[index + 4]); // 单位
                        payloadNode.Nodes.Add(strTmp);
                        index += 5;
                        strTmp = "供水温度："
                            + buf[index + 2].ToString("X2")
                            + buf[index + 1].ToString("X2") + "."
                            + buf[index + 0].ToString("X2") + " ℃";
                        payloadNode.Nodes.Add(strTmp);
                        index += 3;
                        strTmp = "回水温度："
                            + buf[index + 2].ToString("X2")
                            + buf[index + 1].ToString("X2") + "."
                            + buf[index + 0].ToString("X2") + " ℃";
                        payloadNode.Nodes.Add(strTmp);
                        index += 3;
                        strTmp = "工作时间："
                            + buf[index + 2].ToString("X2")
                            + buf[index + 1].ToString("X2")
                            + buf[index + 0].ToString("X2") + " 小时";
                        payloadNode.Nodes.Add(strTmp);
                        index += 3;
                        strTmp = "实时时间："
                            + buf[index + 6].ToString("X2") + buf[index + 5].ToString("X2") + "-"
                            + buf[index + 4].ToString("X2") + "-"
                            + buf[index + 3].ToString("X2") + " "
                            + buf[index + 2].ToString("X2") + ":"
                            + buf[index + 1].ToString("X2") + ":"
                            + buf[index + 0].ToString("X2");
                        payloadNode.Nodes.Add(strTmp);
                        index += 7;
                        strTmp = "状态ST  ：" + GetMeterStatusInfo(buf[index], buf[index + 1]);
                        payloadNode.Nodes.Add(strTmp);
                        index += 2;
                    }
                }
                else if (strDataType.Contains("读历史计量数据"))
                {
                    if ((frame.MeterType >= 0x10 && frame.MeterType <= 0x19)    // 水表
                        || (frame.MeterType >= 0x30 && frame.MeterType <= 0x49) // 燃气表、其他仪表
                        )
                    {
                        if (buf.Length < index + 5) return payloadNode;

                        strTmp = "上" + ((frame.DataBuf[1] & 0x0F) + 1) + "月流量："
                            + buf[index + 3].ToString("X2") + buf[index + 2].ToString("X2")
                            + buf[index + 1].ToString("X2") + "." + buf[index].ToString("X2")
                            + " " + GetUnitName(buf[index + 4]); // 单位
                        payloadNode.Nodes.Add(strTmp);
                        index += 5;
                    }
                    else if ((frame.MeterType >= 0x20 && frame.MeterType <= 0x29))  // 热量表
                    {
                        if (buf.Length < index + 5) return payloadNode;

                        strTmp = "上" + ((frame.DataBuf[1] & 0x0F) + 1) + "月热量："
                            + buf[index + 3].ToString("X2") + buf[index + 2].ToString("X2")
                            + buf[index + 1].ToString("X2") + "." + buf[index].ToString("X2")
                            + " " + GetUnitName(buf[index + 4]); // 单位
                        payloadNode.Nodes.Add(strTmp);
                        index += 5;
                    }
                }
                else if (strDataType.Contains("读价格表"))
                {
                    if (buf.Length < index + 15) return payloadNode;

                    strTmp = "价格一：" + ((buf[index + 2] << 8) + buf[index + 1]).ToString("X") 
                        + "." + (buf[index + 0]).ToString("X2")
                        + " 元/m³";
                    payloadNode.Nodes.Add(strTmp);
                    index += 3;
                    strTmp = "用量一：" + ((buf[index + 2] << 16) + (buf[index + 1] << 8) + buf[index]).ToString("X")
                        + " m³";
                    payloadNode.Nodes.Add(strTmp);
                    index += 3;
                    strTmp = "价格二：" + ((buf[index + 2] << 8) + buf[index + 1]).ToString("X")
                        + "." + (buf[index + 0]).ToString("X2")
                        + " 元/m³";
                    payloadNode.Nodes.Add(strTmp);
                    index += 3;
                    strTmp = "用量二：" + ((buf[index + 2] << 16) + (buf[index + 1] << 8) + buf[index]).ToString("X")
                        + " m³";
                    payloadNode.Nodes.Add(strTmp);
                    index += 3;
                    strTmp = "价格三：" + ((buf[index + 2] << 8) + buf[index + 1]).ToString("X")
                        + "." + (buf[index + 0]).ToString("X2")
                        + " 元/m³";
                    payloadNode.Nodes.Add(strTmp);
                    index += 3;
                }
                else if (strDataType.Contains("读结算日"))
                {
                    if (buf.Length < index + 1) return payloadNode;

                    strTmp = "结算日：" + buf[index].ToString("X2");
                    payloadNode.Nodes.Add(strTmp);
                    index += 1;
                }
                else if (strDataType.Contains("读抄表日"))
                {
                    if (buf.Length < index + 1) return payloadNode;

                    strTmp = "抄表日：" + buf[index].ToString("X2");
                    payloadNode.Nodes.Add(strTmp);
                    index += 1;
                }
                else if (strDataType.Contains("读购入金额"))
                {
                    if (buf.Length < index + 15) return payloadNode;

                    strTmp = "本次购买序号：" + buf[index];
                    payloadNode.Nodes.Add(strTmp);
                    index += 1;
                    strTmp = "本次购入金额：" + ((buf[index + 3] << 16) + (buf[index + 2] << 8) + buf[index + 1]).ToString("X")
                        + "." + (buf[index + 0]).ToString("X2")
                        + " 元";
                    payloadNode.Nodes.Add(strTmp);
                    index += 4;
                    strTmp = "累计购入金额：" + ((buf[index + 3] << 16) + (buf[index + 2] << 8) + buf[index + 1]).ToString("X")
                        + "." + (buf[index + 0]).ToString("X2")
                        + " 元";
                    payloadNode.Nodes.Add(strTmp);
                    index += 4;
                    strTmp = "剩余金额：" + ((buf[index + 3] << 16) + (buf[index + 2] << 8) + buf[index + 1]).ToString("X")
                        + "." + (buf[index + 0]).ToString("X2")
                        + " 元";
                    payloadNode.Nodes.Add(strTmp);
                    index += 4;
                    strTmp = "状态ST  ：" + GetMeterStatusInfo(buf[index], buf[index + 1]);
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;
                }
            }

            return payloadNode;
        }
        #endregion

        #region 写数据
        // 写数据
        private static TreeNode ExplainWriteData(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 3) return payloadNode;

                string strDataType = GetDataType(frame);
                strTmp = "数据标识：" + strDataType + " (" + buf[index].ToString("X2") + buf[index + 1].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "帧序号  ：" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (strDataType.Contains("写价格表"))
                {
                    if (buf.Length < index + 16) return payloadNode;

                    strTmp = "价格一：" + ((buf[index + 2] << 8) + buf[index + 1]).ToString("X")
                        + "." + (buf[index + 0]).ToString("X2")
                        + " 元/m³";
                    payloadNode.Nodes.Add(strTmp);
                    index += 3;
                    strTmp = "用量一：" + ((buf[index + 2] << 16) + (buf[index + 1] << 8) + buf[index]).ToString("X")
                        + " m³";
                    payloadNode.Nodes.Add(strTmp);
                    index += 3;
                    strTmp = "价格二：" + ((buf[index + 2] << 8) + buf[index + 1]).ToString("X")
                        + "." + (buf[index + 0]).ToString("X2")
                        + " 元/m³";
                    payloadNode.Nodes.Add(strTmp);
                    index += 3;
                    strTmp = "用量二：" + ((buf[index + 2] << 16) + (buf[index + 1] << 8) + buf[index]).ToString("X")
                        + " m³";
                    payloadNode.Nodes.Add(strTmp);
                    index += 3;
                    strTmp = "价格三：" + ((buf[index + 2] << 8) + buf[index + 1]).ToString("X")
                        + "." + (buf[index + 0]).ToString("X2")
                        + " 元/m³";
                    payloadNode.Nodes.Add(strTmp);
                    index += 3;
                    strTmp = "启用日期：" + buf[index].ToString("X2");
                    payloadNode.Nodes.Add(strTmp);
                    index += 1;
                }
                else if (strDataType.Contains("写结算日"))
                {
                    if (buf.Length < index + 1) return payloadNode;

                    strTmp = "结算日：" + buf[index].ToString("X2");
                    payloadNode.Nodes.Add(strTmp);
                    index += 1;
                }
                else if (strDataType.Contains("写抄表日"))
                {
                    if (buf.Length < index + 1) return payloadNode;

                    strTmp = "抄表日：" + buf[index].ToString("X2");
                    payloadNode.Nodes.Add(strTmp);
                    index += 1;
                }
                else if (strDataType.Contains("写购入金额"))
                {
                    if (buf.Length < index + 5) return payloadNode;

                    strTmp = "本次购买序号：" + buf[index];
                    payloadNode.Nodes.Add(strTmp);
                    index += 1;
                    strTmp = "本次购入金额：" + ((buf[index + 3] << 16) + (buf[index + 2] << 8) + buf[index + 1]).ToString("X")
                        + "." + (buf[index + 0]).ToString("X2")
                        + " 元";
                    payloadNode.Nodes.Add(strTmp);
                    index += 4;
                }
                else if (strDataType.Contains("写新密钥"))
                {
                    if (buf.Length < index + 9) return payloadNode;

                    strTmp = "新密钥版本：" + buf[index].ToString("X2");
                    payloadNode.Nodes.Add(strTmp);
                    index += 1;
                    strTmp = "新密钥数据：" + Util.GetStringHexFromBytes(buf, index, 8, "", true);
                    payloadNode.Nodes.Add(strTmp);
                    index += 8;
                }
                else if (strDataType.Contains("写标准时间"))
                {
                    if (buf.Length < index + 7) return payloadNode;

                    strTmp = "实时时间："
                            + buf[index + 6].ToString("X2") + buf[index + 5].ToString("X2") + "-"
                            + buf[index + 4].ToString("X2") + "-"
                            + buf[index + 3].ToString("X2") + " "
                            + buf[index + 2].ToString("X2") + ":"
                            + buf[index + 1].ToString("X2") + ":"
                            + buf[index + 0].ToString("X2");
                    payloadNode.Nodes.Add(strTmp);
                    index += 7;
                }
                else if (strDataType.Contains("写阀门控制"))
                {
                    if (buf.Length < index + 1) return payloadNode;

                    strTmp = "阀门操作：" + (buf[index] == 0x55 ? "开阀" : (buf[index] == 0x99 ? "关阀" : "未知"));
                    payloadNode.Nodes.Add(strTmp);
                    index += 1;
                }
            }
            else
            {
                // 应答
                if (buf.Length < index + 3) return payloadNode;

                string strDataType = GetDataType(frame);
                strTmp = "数据标识：" + strDataType + " (" + buf[index].ToString("X2") + buf[index + 1].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "帧序号  ：" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (strDataType.Contains("写价格表"))
                {
                    if (buf.Length < index + 2) return payloadNode;

                    strTmp = "状态ST  ：" + GetMeterStatusInfo(buf[index], buf[index + 1]);
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;
                }
                else if (strDataType.Contains("写购入金额"))
                {
                    if (buf.Length < index + 5) return payloadNode;

                    strTmp = "本次购买序号：" + buf[index];
                    payloadNode.Nodes.Add(strTmp);
                    index += 1;
                    strTmp = "本次购入金额：" + ((buf[index + 3] << 16) + (buf[index + 2] << 8) + buf[index + 1]).ToString("X")
                        + "." + (buf[index + 0]).ToString("X2")
                        + " 元";
                    payloadNode.Nodes.Add(strTmp);
                    index += 4;
                }
                else if (strDataType.Contains("写新密钥"))
                {
                    if (buf.Length < index + 1) return payloadNode;

                    strTmp = "密钥版本号：" + buf[index].ToString("X2");
                    payloadNode.Nodes.Add(strTmp);
                    index += 1;
                }
                else if (strDataType.Contains("写阀门控制"))
                {
                    if (buf.Length < index + 2) return payloadNode;

                    strTmp = "状态ST  ：" + GetMeterStatusInfo(buf[index], buf[index + 1]);
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;
                }

            }

            return payloadNode;
        }
        #endregion

        #region 读地址 / 写地址

        // 读地址
        private static TreeNode ExplainReadDevAddr(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 3) return payloadNode;

                string strDataType = GetDataType(frame);
                strTmp = "数据标识：" + strDataType + " (" + buf[index].ToString("X2") + buf[index + 1].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "帧序号  ：" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }
            else
            {
                // 应答
                if (buf.Length < index + 3) return payloadNode;

                string strDataType = GetDataType(frame);
                strTmp = "数据标识：" + strDataType + " (" + buf[index].ToString("X2") + buf[index + 1].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "帧序号  ：" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }

            return payloadNode;
        }

        // 写地址
        private static TreeNode ExplainWriteDevAddr(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 3) return payloadNode;

                string strDataType = GetDataType(frame);
                strTmp = "数据标识：" + strDataType + " (" + buf[index].ToString("X2") + buf[index + 1].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "帧序号  ：" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (buf.Length < index + 7) return payloadNode;

                strTmp = "新地址  ：" + Util.GetStringHexFromBytes(buf, index, 7, "", true);
                payloadNode.Nodes.Add(strTmp);
                index += 7;
            }
            else
            {
                // 应答
                if (buf.Length < index + 3) return payloadNode;

                string strDataType = GetDataType(frame);
                strTmp = "数据标识：" + strDataType + " (" + buf[index].ToString("X2") + buf[index + 1].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "帧序号  ：" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }

            return payloadNode;
        }
        #endregion 
        
        #region 读密钥版本号
        // 读密钥版本号
        private static TreeNode ExplainReadSecretKeyVersion(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 3) return payloadNode;

                string strDataType = GetDataType(frame);
                strTmp = "数据标识：" + strDataType + " (" + buf[index].ToString("X2") + buf[index + 1].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "帧序号  ：" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }
            else
            {
                // 应答
                if (buf.Length < index + 4) return payloadNode;

                string strDataType = GetDataType(frame);
                strTmp = "数据标识：" + strDataType + " (" + buf[index].ToString("X2") + buf[index + 1].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "帧序号  ：" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                strTmp = "密钥版本号：" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }

            return payloadNode;
        }
        #endregion

        #region 写机电同步数据
        // 写地址
        private static TreeNode ExplainWriteMeterSyncData(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 3) return payloadNode;

                string strDataType = GetDataType(frame);
                strTmp = "数据标识：" + strDataType + " (" + buf[index].ToString("X2") + buf[index + 1].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "帧序号  ：" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (buf.Length < index + 5) return payloadNode;

                strTmp = "当前流量："
                            + buf[index + 3].ToString("X2") + buf[index + 2].ToString("X2")
                            + buf[index + 1].ToString("X2") + "." + buf[index].ToString("X2")
                            + " " + GetUnitName(buf[index + 4]); // 单位
                payloadNode.Nodes.Add(strTmp);
                index += 5;
            }
            else
            {
                // 应答
                if (buf.Length < index + 3) return payloadNode;

                string strDataType = GetDataType(frame);
                strTmp = "数据标识：" + strDataType + " (" + buf[index].ToString("X2") + buf[index + 1].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "帧序号  ：" + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 1;

                if (buf.Length < index + 2) return payloadNode;

                strTmp = "状态ST  ：" + GetMeterStatusInfo(buf[index], buf[index + 1]);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
            }

            return payloadNode;
        }
        #endregion

    }

#pragma warning restore
}
