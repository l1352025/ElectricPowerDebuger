using ElectricPowerDebuger.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ElectricPowerDebuger.Protocol
{
    class ProtoCJT188_04
    {
        public const byte FrameHeader = 0x68;           // 帧头
        public const byte FrameTail = 0x16;             // 帧尾
        public const byte FrameFixedLen = 13;           // 0x68, 仪表类型(1), 地址(7), 控制域(1), 长度(1), 数据域（N），校验和(1), 0x16
        public const byte LongAddrSize = 7;             // 地址的长度
        public const string BroadcastAddr = "999999999999"; // 广播地址
        public const string CommonUseAddr = "AAAAAAAAAAAA"; // 通配地址

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
            new DataExplain(0x81, 0x06, "读密钥版本号",       ExplainReadSecretKeyVersion),  

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

        public struct MeterStatus
        {
            byte ValveStatus;       // 阀门状态     bit1-0:  00 - 开, 01 - 关， 11 - 异常
            byte VBatStatus;        // 电池电压状态   bit2:  0 - 正常, 1 - 欠压
            ushort reserved;        // 保留或厂家定义 bit15-3

            ushort All;
        }

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
                        if (rxBuf.Length >= (index + FrameFixedLen + rxBuf[index + 11]))
                        {
                            break;
                        }
                    }
                }

                if (startIdx == -1) throw new Exception("帧头错误");
                if (rxBuf.Length < index + FrameFixedLen + rxBuf[index + 11]) throw new Exception("长度错误");

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
                        rxData.ErrorInfo = "数据异常" + ex.Message;

                        break;
                }
            }

            return rxData;
        }
        #endregion

        #region 协议帧解析

        // 解析 数据项类型
        public static string GetDataType(FrameFormat frame)
        {
            string dataType = "无法识别";

            foreach (DataExplain dat in DataExplainTbl)
            {
                if (dat.DI0 == frame.DataBuf[0]
                    && dat.DI1 == frame.DataBuf[1]
                   )
                {
                    dataType = dat.DataName;
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
            TreeNode parentNode = new TreeNode("188-04报文");
            TreeNode node = null;
            string strTmp = "";

            if (frame.CtrlWord.CmdType == CommandType.Invalid)
            {
                parentNode.Nodes.Add("无效帧-" + frame.ErrorInfo);
                return parentNode;
            }

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
                strTmp = "错误标志：" + (frame.DataBuf[1] + (frame.DataBuf[2] << 8)).ToString("X4") + " ("
                    + "阀门-" + ((frame.DataBuf[1] & 0x03) == 0 ? "开启" : ((frame.DataBuf[1] & 0x03) == 1? "关闭" : "异常")) 
                    + "电池-" + ((frame.DataBuf[1] & 0x04) == 0 ? "正常" : "欠压") + ")";
                parentNode.Nodes.Add(strTmp);
            }
            else
            {
                // 正常应答
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
            }
            else
            {
                // 应答
                if (buf.Length < index + 6) return payloadNode;

                strTmp = "通信地址："
                            + buf[index + 5].ToString("X2")
                            + buf[index + 4].ToString("X2")
                            + buf[index + 3].ToString("X2")
                            + buf[index + 2].ToString("X2")
                            + buf[index + 1].ToString("X2")
                            + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 6;

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
            }
            else
            {
                // 应答
                if (buf.Length < index + 6) return payloadNode;

                strTmp = "通信地址："
                            + buf[index + 5].ToString("X2")
                            + buf[index + 4].ToString("X2")
                            + buf[index + 3].ToString("X2")
                            + buf[index + 2].ToString("X2")
                            + buf[index + 1].ToString("X2")
                            + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 6;

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
            }
            else
            {
                // 应答
                if (buf.Length < index + 6) return payloadNode;

                strTmp = "通信地址："
                            + buf[index + 5].ToString("X2")
                            + buf[index + 4].ToString("X2")
                            + buf[index + 3].ToString("X2")
                            + buf[index + 2].ToString("X2")
                            + buf[index + 1].ToString("X2")
                            + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 6;

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
                if (buf.Length < index + 6) return payloadNode;

                strTmp = "通信地址："
                            + buf[index + 5].ToString("X2")
                            + buf[index + 4].ToString("X2")
                            + buf[index + 3].ToString("X2")
                            + buf[index + 2].ToString("X2")
                            + buf[index + 1].ToString("X2")
                            + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 6;

            }
            else
            {
                // 应答
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
                if (buf.Length < index + 6) return payloadNode;

                strTmp = "通信地址："
                            + buf[index + 5].ToString("X2")
                            + buf[index + 4].ToString("X2")
                            + buf[index + 3].ToString("X2")
                            + buf[index + 2].ToString("X2")
                            + buf[index + 1].ToString("X2")
                            + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 6;

            }
            else
            {
                // 应答
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
                if (buf.Length < index + 6) return payloadNode;

                strTmp = "通信地址："
                            + buf[index + 5].ToString("X2")
                            + buf[index + 4].ToString("X2")
                            + buf[index + 3].ToString("X2")
                            + buf[index + 2].ToString("X2")
                            + buf[index + 1].ToString("X2")
                            + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 6;

            }
            else
            {
                // 应答
            }

            return payloadNode;
        }
        #endregion

        #region 有【数据标识】--读数据、读后续数据、写数据、安全认证
        private static TreeNode ExplainElectricEnergyData(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 4) return payloadNode;

                string strDataType = GetDataType(frame);
                strTmp = "数据标识：" + strDataType + " (" + BitConverter.ToUInt32(buf, index).ToString("X8") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;
            }
            else
            {
                // 应答
                if (buf.Length < index + 8) return payloadNode;

                string strDataType = GetDataType(frame);
                strTmp = "数据标识：" + strDataType + " (" + BitConverter.ToUInt32(buf, index).ToString("X8") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;

                strTmp = "";
                if (strDataType.Contains("电能"))  // 组合有功/正向有功/反向有功总电能
                {
                    strTmp = "总电量  ：" + ((buf[index + 3] << 16) + (buf[index + 2] << 8) + buf[index + 1]).ToString("X")
                                + "." + buf[index].ToString("X2") + "kWh";
                }
                else if (strDataType.Contains("剩余电量"))
                {
                    strTmp = "剩余电量：" + ((buf[index + 3] << 16) + (buf[index + 2] << 8) + buf[index + 1]).ToString("X")
                                + "." + buf[index].ToString("X2") + "kWh";
                }
                else if (strDataType.Contains("透支电量"))
                {
                    strTmp = "透支电量：" + ((buf[index + 3] << 16) + (buf[index + 2] << 8) + buf[index + 1]).ToString("X")
                                + "." + buf[index].ToString("X2") + "kWh";
                }
                else if (strDataType.Contains("剩余金额"))
                {
                    strTmp = "剩余金额：" + ((buf[index + 3] << 16) + (buf[index + 2] << 8) + buf[index + 1]).ToString("X")
                                + "." + buf[index].ToString("X2") + "元";
                }
                else if (strDataType.Contains("透支金额"))
                {
                    strTmp = "透支金额：" + ((buf[index + 3] << 16) + (buf[index + 2] << 8) + buf[index + 1]).ToString("X")
                                + "." + buf[index].ToString("X2") + "元";
                }

                payloadNode.Nodes.Add(strTmp);
                index += 4;
            }

            return payloadNode;
        }

        private static TreeNode ExplainMaxValueAndTime(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 4) return payloadNode;

                string strDataType = GetDataType(frame);
                strTmp = "数据标识：" + strDataType + " (" + BitConverter.ToUInt32(buf, index).ToString("X8") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;
            }
            else
            {
                // 应答
                if (buf.Length < index + 12) return payloadNode;

                string strDataType = GetDataType(frame);
                strTmp = "数据标识：" + strDataType + " (" + BitConverter.ToUInt32(buf, index).ToString("X8") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;
                strTmp = "最大需量：" + buf[index + 2].ToString("X") + "." + (buf[index + 1] * 256 + buf[index]).ToString("X4") + "kW";
                payloadNode.Nodes.Add(strTmp);
                index += 3;
                strTmp = "发生时间：" + DateTime.Now.Year / 100
                            + buf[index + 4].ToString("X2") + "-"
                            + buf[index + 3].ToString("X2") + "-"
                            + buf[index + 2].ToString("X2") + " "
                            + buf[index + 1].ToString("X2") + ":"
                            + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 5;

            }

            return payloadNode;
        }

        private static TreeNode ExplainVariable(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 4) return payloadNode;

                string strDataType = GetDataType(frame);
                strTmp = "数据标识：" + strDataType + " (" + BitConverter.ToUInt32(buf, index).ToString("X8") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;
            }
            else
            {
                // 应答
                if (buf.Length < index + 10) return payloadNode;

                string strDataType = GetDataType(frame);
                strTmp = "数据标识：" + strDataType + " (" + BitConverter.ToUInt32(buf, index).ToString("X8") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;
                if (strDataType.Contains("电压数据块"))
                {
                    strTmp = "A相电压 ：" + buf[index + 1].ToString("X") + (buf[index] >> 4).ToString("X1") + "." + (buf[index] & 0x0F).ToString("X1") + "V";
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;
                    strTmp = "B相电压 ：" + buf[index + 1].ToString("X") + (buf[index] >> 4).ToString("X1") + "." + (buf[index] & 0x0F).ToString("X1") + "V";
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;
                    strTmp = "C相电压 ：" + buf[index + 1].ToString("X") + (buf[index] >> 4).ToString("X1") + "." + (buf[index] & 0x0F).ToString("X1") + "V";
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;
                }
            }

            return payloadNode;
        }

        private static TreeNode ExplainEventRecord(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 4) return payloadNode;

                string strDataType = GetDataType(frame);
                strTmp = "数据标识：" + strDataType + " (" + BitConverter.ToUInt32(buf, index).ToString("X8") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;
            }
            else
            {
                // 应答
                if (buf.Length < index + 4) return payloadNode;

                string strDataType = GetDataType(frame);
                strTmp = "数据标识：" + strDataType + " (" + BitConverter.ToUInt32(buf, index).ToString("X8") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;
                if (strDataType.Contains("掉电总次数"))
                {
                    if (buf.Length < index + 3) return payloadNode;
                    strTmp = "掉电总次数：" + ((buf[index + 2] << 16) + (buf[index + 1] << 8) + buf[index]).ToString("X");
                    payloadNode.Nodes.Add(strTmp);
                    index += 3;
                }
                else if (strDataType.Contains("掉电时间"))
                {
                    if (buf.Length < index + 6) return payloadNode;
                    strTmp = "掉电时间：" + DateTime.Now.Year / 100
                            + buf[index + 5].ToString("X2") + "-"
                            + buf[index + 4].ToString("X2") + "-"
                            + buf[index + 3].ToString("X2") + " "
                            + buf[index + 2].ToString("X2") + ":"
                            + buf[index + 1].ToString("X2") + ":"
                            + buf[index].ToString("X2");
                    payloadNode.Nodes.Add(strTmp);
                    index += 6;
                }
                else if (strDataType.Contains("购电总次数"))
                {
                    if (buf.Length < index + 2) return payloadNode;
                    strTmp = "购电总次数：" + ((buf[index + 1] << 8) + buf[index]).ToString("X");
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;
                }

            }

            return payloadNode;
        }

        private static TreeNode ExplainParameter(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            // 请求 或 应答
            if (buf.Length < index + 4) return payloadNode;

            string strDataType = GetDataType(frame);
            strTmp = "数据标识：" + strDataType + " (" + BitConverter.ToUInt32(buf, index).ToString("X8") + ")";
            payloadNode.Nodes.Add(strTmp);
            index += 4;

            if (strDataType.Contains("日期及星期"))
            {
                if (buf.Length < index + 4) return payloadNode;
                strTmp = "日期    ：" + DateTime.Now.Year / 100
                        + buf[index + 3].ToString("X2") + "-"
                        + buf[index + 2].ToString("X2") + "-"
                        + buf[index + 1].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                strTmp = "星期    ：";
                switch (buf[index])
                {
                    case 0: strTmp += "星期日"; break;
                    case 1: strTmp += "星期一"; break;
                    case 2: strTmp += "星期二"; break;
                    case 3: strTmp += "星期三"; break;
                    case 4: strTmp += "星期四"; break;
                    case 5: strTmp += "星期五"; break;
                    case 6: strTmp += "星期六"; break;
                    default: strTmp += "未知"; break;
                }
                payloadNode.Nodes.Add(strTmp);
                index += 4;
            }
            else if (strDataType.Contains("时间"))
            {
                if (buf.Length < index + 3) return payloadNode;
                strTmp = "时间    ："
                        + buf[index + 2].ToString("X2") + ":"
                        + buf[index + 1].ToString("X2") + ":"
                        + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 3;
            }
            else if (strDataType.Contains("通信地址"))
            {
                if (buf.Length < index + 6) return payloadNode;
                strTmp = "通信地址：" + Util.GetStringHexFromBytes(buf, index, 6, "", true);
                payloadNode.Nodes.Add(strTmp);
                index += 6;
            }
            else if (strDataType.Contains("表号"))
            {
                if (buf.Length < index + 6) return payloadNode;
                strTmp = "表号    ：" + Util.GetStringHexFromBytes(buf, index, 6, "", true);
                payloadNode.Nodes.Add(strTmp);
                index += 6;
            }
            else if (strDataType.Contains("资产管理编号"))
            {
                if (buf.Length < index + 32) return payloadNode;
                strTmp = "资产管理编号：" + Util.GetStringHexFromBytes(buf, index, 32, "");
                payloadNode.Nodes.Add(strTmp);
                index += 32;
            }
            else if (strDataType.Contains("电表运行状态字"))
            {
                if (buf.Length < index + 14) return payloadNode;
                strTmp = "状态字1 ：" + ((buf[index + 1] << 8) + buf[index]).ToString("X4");
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "状态字2 ：" + ((buf[index + 1] << 8) + buf[index]).ToString("X4");
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "状态字3 ：" + ((buf[index + 1] << 8) + buf[index]).ToString("X4") + " （操作类）";
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "状态字4 ：" + ((buf[index + 1] << 8) + buf[index]).ToString("X4") + " （A相故障状态）";
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "状态字5 ：" + ((buf[index + 1] << 8) + buf[index]).ToString("X4") + " （B相故障状态）";
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "状态字6 ：" + ((buf[index + 1] << 8) + buf[index]).ToString("X4") + " （C相故障状态）";
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "状态字7 ：" + ((buf[index + 1] << 8) + buf[index]).ToString("X4") + " （合相故障状态）";
                payloadNode.Nodes.Add(strTmp);
                index += 2;
            }


            return payloadNode;
        }

        private static TreeNode ExplainFreezeData(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 4) return payloadNode;

                string strDataType = GetDataType(frame);
                strTmp = "数据标识：" + strDataType + " (" + BitConverter.ToUInt32(buf, index).ToString("X8") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;
            }
            else
            {
                // 应答
                if (buf.Length < index + 4) return payloadNode;

                string strDataType = GetDataType(frame);
                strTmp = "数据标识：" + strDataType + " (" + BitConverter.ToUInt32(buf, index).ToString("X8") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;
                if (strDataType.Contains("日冻结时间"))
                {
                    if (buf.Length < index + 5) return payloadNode;
                    strTmp = "冻结时间：" + DateTime.Now.Year / 100
                            + buf[index + 4].ToString("X2") + "-"
                            + buf[index + 3].ToString("X2") + "-"
                            + buf[index + 2].ToString("X2") + " "
                            + buf[index + 1].ToString("X2") + ":"
                            + buf[index].ToString("X2") + ":00";
                    payloadNode.Nodes.Add(strTmp);
                    index += 5;
                }
                else if (strDataType.Contains("日冻结正向电能"))
                {
                    if (buf.Length < index + 4) return payloadNode;
                    strTmp = "正向电能：" + ((buf[index + 3] << 16) + (buf[index + 2] << 8) + buf[index + 1]).ToString("X")
                            + "." + buf[index].ToString("X2");
                    payloadNode.Nodes.Add(strTmp);
                    index += 4;
                }
                else if (strDataType.Contains("日冻结反向电能"))
                {
                    if (buf.Length < index + 4) return payloadNode;
                    strTmp = "反向电能：" + ((buf[index + 3] << 16) + (buf[index + 2] << 8) + buf[index + 1]).ToString("X")
                            + "." + buf[index].ToString("X2");
                    payloadNode.Nodes.Add(strTmp);
                    index += 4;
                }

            }

            return payloadNode;
        }

        private static TreeNode ExplainLoadRecord(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 4) return payloadNode;

                string strDataType = GetDataType(frame);
                strTmp = "数据标识：" + strDataType + " (" + BitConverter.ToUInt32(buf, index).ToString("X8") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;
            }
            else
            {
                // 应答
                if (buf.Length < index + 4) return payloadNode;

                string strDataType = GetDataType(frame);
                strTmp = "数据标识：" + strDataType + " (" + BitConverter.ToUInt32(buf, index).ToString("X8") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;
                if (strDataType.Contains("最早记录块"))
                {
                    // 暂不解析
                }
                else if (strDataType.Contains("给定时间记录块"))
                {
                    // 暂不解析
                }
                else if (strDataType.Contains("最近记录块"))
                {
                    // 暂不解析
                }

            }

            return payloadNode;
        }

        #region 安全认证-指令
        private static TreeNode ExplainDataReadBack(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 12) return payloadNode;

                strTmp = "数据标识：" + GetDataType(frame) + " (" + BitConverter.ToUInt32(buf, index).ToString("X8") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;
                strTmp = "读取的数据长度：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "读取的起始地址：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "文件标识：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "目录标识：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

            }
            else
            {
                // 应答
                if (buf.Length < index + 16) return payloadNode;

                strTmp = "数据标识：" + GetDataType(frame) + " (" + BitConverter.ToUInt32(buf, index).ToString("X8") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;
                strTmp = "读取的数据长度：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "读取的起始地址：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "文件标识：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;
                strTmp = "目录标识：" + (buf[index] + buf[index + 1] * 256);
                payloadNode.Nodes.Add(strTmp);
                index += 2;

                strTmp = "回抄的数据(" + (buf.Length - 16) + " byte)：" + Util.GetStringHexFromBytes(buf, index, buf.Length - 16);
                payloadNode.Nodes.Add(strTmp);
                index += (buf.Length - 16);

                strTmp = "MAC     ：" + BitConverter.ToUInt32(buf, index).ToString("X8");
                payloadNode.Nodes.Add(strTmp);
                index += 4;
            }

            return payloadNode;
        }

        private static TreeNode ExplainQueryStatus(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 12) return payloadNode;

                strTmp = "数据标识：" + GetDataType(frame) + " (" + BitConverter.ToUInt32(buf, index).ToString("X8") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;
            }
            else
            {
                // 应答
                if (buf.Length < index + 30) return payloadNode;

                strTmp = "数据标识：" + GetDataType(frame) + " (" + BitConverter.ToUInt32(buf, index).ToString("X8") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;
                strTmp = "剩余金额：" + BitConverter.ToUInt32(buf, index);
                payloadNode.Nodes.Add(strTmp);
                index += 4;
                strTmp = "MAC     ：" + BitConverter.ToUInt32(buf, index).ToString("X8");
                payloadNode.Nodes.Add(strTmp);
                index += 4;
                strTmp = "购电次数：" + BitConverter.ToUInt32(buf, index);
                payloadNode.Nodes.Add(strTmp);
                index += 4;
                strTmp = "MAC     ：" + BitConverter.ToUInt32(buf, index).ToString("X8");
                payloadNode.Nodes.Add(strTmp);
                index += 4;
                strTmp = "客户编号："
                        + buf[index + 5].ToString("X2") + buf[index + 4].ToString("X2")
                        + buf[index + 3].ToString("X2") + buf[index + 2].ToString("X2")
                        + buf[index + 1].ToString("X2") + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 6;
                strTmp = "密钥信息：" + BitConverter.ToUInt32(buf, index).ToString("X8");
                payloadNode.Nodes.Add(strTmp);
                index += 4;
            }

            return payloadNode;
        }

        private static TreeNode ExplainIdAuthCmd(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 4) return payloadNode;

                strTmp = "数据标识：" + GetDataType(frame) + " (" + BitConverter.ToUInt32(buf, index).ToString("X8") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;

                if (buf[0] == 0x01)
                {
                    if (buf.Length < index + 8) return payloadNode;

                    strTmp = "密文    ：" + Util.GetStringHexFromBytes(buf, index, 8);
                    payloadNode.Nodes.Add(strTmp);
                    index += 8;
                }
                else if (buf[0] == 0x02)
                {
                    if (buf.Length < index + 8) return payloadNode;

                    strTmp = "随机数1 ：" + Util.GetStringHexFromBytes(buf, index, 8);
                    payloadNode.Nodes.Add(strTmp);
                    index += 8;
                }
                else if (buf[0] == 0x03)
                {
                    if (buf.Length < index + 8) return payloadNode;

                    strTmp = "分散因子：" + Util.GetStringHexFromBytes(buf, index, 8);
                    payloadNode.Nodes.Add(strTmp);
                    index += 8;
                }
                else if (buf[0] == 0xFF)
                {
                    if (buf.Length < index + 24) return payloadNode;

                    strTmp = "密文    ：" + Util.GetStringHexFromBytes(buf, index, 8);
                    payloadNode.Nodes.Add(strTmp);
                    index += 8;
                    strTmp = "随机数1 ：" + Util.GetStringHexFromBytes(buf, index, 8);
                    payloadNode.Nodes.Add(strTmp);
                    index += 8;
                    strTmp = "分散因子：" + Util.GetStringHexFromBytes(buf, index, 8);
                    payloadNode.Nodes.Add(strTmp);
                    index += 8;
                }
            }
            else
            {
                // 应答
                if (buf.Length < index + 8) return payloadNode;

                strTmp = "数据标识：" + GetDataType(frame) + " (" + BitConverter.ToUInt32(buf, index).ToString("X8") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;
                strTmp = "随机数2 ：" + Util.GetStringHexFromBytes(buf, index, 4);
                payloadNode.Nodes.Add(strTmp);
                index += 4;

                if (buf.Length < index + 8) return payloadNode;

                strTmp = "ESAM序列号：" + Util.GetStringHexFromBytes(buf, index, 8);
                payloadNode.Nodes.Add(strTmp);
                index += 8;
            }

            return payloadNode;
        }

        private static TreeNode ExplainIdAuthTimeSet(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 4) return payloadNode;

                strTmp = "数据标识：" + GetDataType(frame) + " (" + BitConverter.ToUInt32(buf, index).ToString("X8") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;

                if (buf[0] == 0x01)
                {
                    if (buf.Length < index + 2) return payloadNode;

                    strTmp = "有效时长：" + (buf[index + 1] * 256 + buf[index]).ToString("X") + "分钟";
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;
                }
                else if (buf[0] == 0x02)
                {
                    if (buf.Length < index + 4) return payloadNode;

                    strTmp = "MAC     ：" + BitConverter.ToUInt32(buf, index).ToString("X8");
                    payloadNode.Nodes.Add(strTmp);
                    index += 4;
                }
                else if (buf[0] == 0xFF)
                {
                    if (buf.Length < index + 12) return payloadNode;

                    strTmp = "有效时长：" + (buf[index + 1] * 256 + buf[index]).ToString("X") + "分钟";
                    payloadNode.Nodes.Add(strTmp);
                    index += 2;
                    strTmp = "MAC     ：" + BitConverter.ToUInt32(buf, index).ToString("X8");
                    payloadNode.Nodes.Add(strTmp);
                    index += 4;
                }
            }
            else
            {
                // 应答
            }

            return payloadNode;
        }

        private static TreeNode ExplainIdAuthFail(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 22) return payloadNode;

                strTmp = "数据标识：" + GetDataType(frame) + " (" + BitConverter.ToUInt32(buf, index).ToString("X8") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;
            }
            else
            {
                // 应答
                if (buf.Length < index + 22) return payloadNode;

                strTmp = "数据标识：" + GetDataType(frame) + " (" + BitConverter.ToUInt32(buf, index).ToString("X8") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;
                strTmp = "客户编号："
                        + buf[index + 5].ToString("X2") + buf[index + 4].ToString("X2")
                        + buf[index + 3].ToString("X2") + buf[index + 2].ToString("X2")
                        + buf[index + 1].ToString("X2") + buf[index].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 6;
                strTmp = "剩余金额：" + BitConverter.ToUInt32(buf, index);
                payloadNode.Nodes.Add(strTmp);
                index += 4;
                strTmp = "购电次数：" + BitConverter.ToUInt32(buf, index);
                payloadNode.Nodes.Add(strTmp);
                index += 4;
                strTmp = "密钥信息：" + BitConverter.ToUInt32(buf, index).ToString("X8");
                payloadNode.Nodes.Add(strTmp);
                index += 4;
            }

            return payloadNode;
        }

        private static TreeNode ExplainRegisterOrRecharge(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 4) return payloadNode;

                strTmp = "数据标识：" + GetDataType(frame) + " (" + BitConverter.ToUInt32(buf, index).ToString("X8") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;

                if (buf[0] == 0x01)
                {
                    if (buf.Length < index + 4) return payloadNode;

                    strTmp = "购电金额：" + BitConverter.ToUInt32(buf, index);
                    payloadNode.Nodes.Add(strTmp);
                    index += 4;
                }
                else if (buf[0] == 0x02)
                {
                    if (buf.Length < index + 4) return payloadNode;

                    strTmp = "购电次数：" + BitConverter.ToUInt32(buf, index);
                    payloadNode.Nodes.Add(strTmp);
                    index += 4;
                }
                else if (buf[0] == 0x03)
                {
                    if (buf.Length < index + 4) return payloadNode;

                    strTmp = "MAC1    ：" + BitConverter.ToUInt32(buf, index).ToString("X8");
                    payloadNode.Nodes.Add(strTmp);
                    index += 4;
                }
                else if (buf[0] == 0x04)
                {
                    if (buf.Length < index + 6) return payloadNode;

                    strTmp = "客户编号："
                            + buf[index + 5].ToString("X2") + buf[index + 4].ToString("X2")
                            + buf[index + 3].ToString("X2") + buf[index + 2].ToString("X2")
                            + buf[index + 1].ToString("X2") + buf[index].ToString("X2");
                    payloadNode.Nodes.Add(strTmp);
                    index += 6;
                }
                else if (buf[0] == 0x05)
                {
                    if (buf.Length < index + 4) return payloadNode;

                    strTmp = "MAC2    ：" + BitConverter.ToUInt32(buf, index).ToString("X8");
                    payloadNode.Nodes.Add(strTmp);
                    index += 4;
                }
                else if (buf[0] == 0xFF)
                {
                    if (buf.Length < index + 22) return payloadNode;

                    strTmp = "剩余金额：" + BitConverter.ToUInt32(buf, index);
                    payloadNode.Nodes.Add(strTmp);
                    index += 4;
                    strTmp = "购电次数：" + BitConverter.ToUInt32(buf, index);
                    payloadNode.Nodes.Add(strTmp);
                    index += 4;
                    strTmp = "MAC1    ：" + BitConverter.ToUInt32(buf, index).ToString("X8");
                    payloadNode.Nodes.Add(strTmp);
                    index += 4;
                    strTmp = "客户编号："
                            + buf[index + 5].ToString("X2") + buf[index + 4].ToString("X2")
                            + buf[index + 3].ToString("X2") + buf[index + 2].ToString("X2")
                            + buf[index + 1].ToString("X2") + buf[index].ToString("X2");
                    payloadNode.Nodes.Add(strTmp);
                    index += 6;
                    strTmp = "MAC2    ：" + BitConverter.ToUInt32(buf, index).ToString("X8");
                    payloadNode.Nodes.Add(strTmp);
                    index += 4;
                }
            }
            else
            {
                // 应答
            }

            return payloadNode;
        }

        private static TreeNode ExplainKeyUpdate(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 4) return payloadNode;

                strTmp = "数据标识：" + GetDataType(frame) + " (" + BitConverter.ToUInt32(buf, index).ToString("X8") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;

                if (buf[0] == 0x01)
                {
                    if (buf.Length < index + 8) return payloadNode;

                    strTmp = "密钥信息+MAC：" + Util.GetStringHexFromBytes(buf, index, 8, " ");
                    payloadNode.Nodes.Add(strTmp);
                    index += 8;
                }
                else if (buf[0] == 0x02)
                {
                    if (buf.Length < index + 32) return payloadNode;

                    strTmp = "密钥：" + Util.GetStringHexFromBytes(buf, index, 32, " ");
                    payloadNode.Nodes.Add(strTmp);
                    index += 32;
                }
                else if (buf[0] == 0xFF)
                {
                    if (buf.Length < index + 40) return payloadNode;

                    strTmp = "密钥信息+MAC：" + Util.GetStringHexFromBytes(buf, index, 8, " ");
                    payloadNode.Nodes.Add(strTmp);
                    index += 8;
                    strTmp = "密钥：" + Util.GetStringHexFromBytes(buf, index, 32, " ");
                    payloadNode.Nodes.Add(strTmp);
                    index += 32;
                }
            }
            else
            {
                // 应答
            }

            return payloadNode;
        }

        #endregion

        #endregion
    }
}
