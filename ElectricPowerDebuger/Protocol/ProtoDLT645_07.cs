using ElectricPowerDebuger.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ElectricPowerDebuger.Protocol
{
    class ProtoDLT645_07
    {
        public const byte FrameHeader = 0x68;           // 帧头
        public const byte FrameTail = 0x16;             // 帧尾
        public const byte FrameFixedLen = 12;           // 0x68, 地址(6), 0x68, 控制域(1), 长度(1), 数据域（N），校验和(1), 0x16
        public const byte LongAddrSize = 6;             // 地址的长度
        public const string BroadcastAddr = "999999999999"; // 广播地址
        public const string CommonUseAddr = "AAAAAAAAAAAA"; // 通配地址

        #region 帧格式定义
        // 通信报文格式
        public struct FrameFormat
        {
            public byte Header;                         // 帧头：  0x68
            public byte[] DevAddr;                      // 设备地址
            public byte Header2;                        // 帧头2： 0x68
            public CtrlField CtrlWord;                  // 控制字
            public int DataLen;                         // 数据域长度
            public byte[] DataBuf;                      // 数据域： （发送前 + 0x33）
            public byte Crc8;                           // Crc8校验： 帧头->CRC8之前的累加和
            public byte Tail;                           // 帧尾 ： 0x16
        };

        // 控制域
        public struct CtrlField
        {
            public CommandType CmdType;         // 命令类型 bit4-0
            public bool IsDataRemain;           // 后续数据标志 bit5
            public bool IsErrorAck;             // 错误应答标志 bit6
            public bool IsAckFrame;             // 应答帧标志 bit7 

            public byte All;
        }

        // 功能码
        public enum CommandType
        {
            BroadcastTime   = 0x08,       // 广播校时
            ReadData        = 0x11,       // 读数据
            ReadRemainData  = 0x12,       // 读后续数据
            ReadDevAddr     = 0x13,       // 读通信地址
            WriteData       = 0x14,       // 写数据
            WriteDevAddr    = 0x15,       // 写通信地址
            FreezeCmd       = 0x16,       // 冻结命令
            ChangeDataRate  = 0x17,       // 更改通信速率
            ChangePswd      = 0x18,       // 修改密码
            MaxValueClear   = 0x19,       // 最大需量清零
            AmeterDataClear = 0x1A,       // 电表清零
            EventClear      = 0x1B,       // 事件清零
            SecurityCtrl    = 0x1C,       // 安全控制：跳闸、报警、保电
            PortOutputCtrl  = 0x1D,       // 端子输出控制
            SecurityCert    = 0x03,       // 安全认证

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
            new CmdExplain(0x03, "安全认证",         Color.Black, ExplainDataItem),
            new CmdExplain(0x08, "广播校时",         Color.Black, ExplainBroadcastTime),
            new CmdExplain(0x11, "读数据",           Color.Black, ExplainDataItem),
            new CmdExplain(0x12, "读后续数据",       Color.Black, ExplainDataItem),
            new CmdExplain(0x13, "读通信地址",       Color.Black, ExplainReadDevAddr),
            new CmdExplain(0x14, "写数据",           Color.Black, ExplainDataItem),
            new CmdExplain(0x15, "写通信地址",       Color.Black, ExplainWriteDevAddr),
            new CmdExplain(0x16, "冻结命令",            Color.Black, ExplainFreezeCmd),
            new CmdExplain(0x17, "更改通信速率",        Color.Black, ExplainChangeDataRate),
            new CmdExplain(0x18, "修改密码",            Color.Black, ExplainChangePassword),
            new CmdExplain(0x19, "最大需量清零",        Color.Black, ExplainAmeterClear),
            new CmdExplain(0x1A, "电表清零",            Color.Black, ExplainAmeterClear),
            new CmdExplain(0x1B, "事件清零",            Color.Black, ExplainEventClear),
            new CmdExplain(0x1C, "跳闸/报警/保电控制",  Color.Black, ExplainSecurityCtrl),
            new CmdExplain(0x1D, "端子输出控制",        Color.Black, ExplainPortOutputCtrl),
        };

        public struct DataExplain
        {
            public byte DI0, DI1, DI2, DI3;
            public String DataName;
            public ExplainCallback DataExplainFunc;

            public DataExplain(byte di3, byte di2, byte di1, byte di0, String dataName, ExplainCallback callback)
            {
                DI3 = di3;
                DI2 = di2;
                DI1 = di1;
                DI0 = di0;
                DataName = dataName;
                DataExplainFunc = callback;
            }
        }
        private static readonly DataExplain[] DataExplainTbl = new DataExplain[]
        {
            //  DI3 ~ DI1 表示数据项类型，DI0表示序号：00 -- 当前， 01 ~ N -- 上1次 ~上N次记录
            //------------  DI3   DI2   DI1   DI0    数据项名称            解析函数  -------------
            // 00 - 电能量
            new DataExplain(0x00, 0x00, 0x00, 0x00, "组合总电能",            ExplainElectricEnergyData),
            new DataExplain(0x00, 0x00, 0xFF, 0x00, "组合电能数据块",          ExplainElectricEnergyData),      // XXXXXX.XX  x N
            new DataExplain(0x00, 0x01, 0x00, 0x00, "正向总电能",            ExplainElectricEnergyData),
            new DataExplain(0x00, 0x01, 0xFF, 0x00, "正向电能数据块",          ExplainElectricEnergyData),      // XXXXXX.XX  x N
            new DataExplain(0x00, 0x02, 0x00, 0x00, "反向总电能",            ExplainElectricEnergyData),
            new DataExplain(0x00, 0x02, 0xFF, 0x00, "反向电能数据块",          ExplainElectricEnergyData),      // XXXXXX.XX  x N
            new DataExplain(0x00, 0x90, 0x01, 0x00, "剩余电量",             ExplainElectricEnergyData),    // XXXXXX.XX
            new DataExplain(0x00, 0x90, 0x01, 0x01, "透支电量",             ExplainElectricEnergyData),      // XXXXXX.XX
            new DataExplain(0x00, 0x90, 0x02, 0x00, "剩余金额",             ExplainElectricEnergyData),    // XXXXXX.XX
            new DataExplain(0x00, 0x90, 0x02, 0x01, "透支金额",             ExplainElectricEnergyData),      // XXXXXX.XX
            
            // 01 - 最大需量及发生时刻
            new DataExplain(0x01, 0x01, 0x00, 0x00, "最大需量及时间",      ExplainMaxValueAndTime),    

            // 02 - 变量
            new DataExplain(0x02, 0x01, 0xFF, 0x00, "电压数据块",            ExplainVariable),    // XXX.X

            // 03 - 事件记录
            new DataExplain(0x03, 0x11, 0x00, 0x00, "掉电总次数",            ExplainEventRecord),    // XXXXXX
            new DataExplain(0x03, 0x11, 0x00, 0x01, "上次掉电时间",           ExplainEventRecord),   // YYMMDDhhmmss -> YYMMDDhhmmss
            new DataExplain(0x03, 0x33, 0x02, 0x01, "购电总次数",            ExplainEventRecord),    // XXXX

            // 04 - 参变量
            new DataExplain(0x04, 0x00, 0x01, 0x01, "日期及星期",            ExplainParameter),    // YYMMDDWW  
            new DataExplain(0x04, 0x00, 0x01, 0x02, "时间",                  ExplainParameter),    // hhmmss
            new DataExplain(0x04, 0x00, 0x04, 0x01, "通信地址",              ExplainParameter),
            new DataExplain(0x04, 0x00, 0x04, 0x02, "表号",                  ExplainParameter),
            new DataExplain(0x04, 0x00, 0x04, 0x03, "资产管理编号",          ExplainParameter),
            new DataExplain(0x04, 0x00, 0x05, 0xFF, "电表运行状态字",        ExplainParameter),    // XXXX x N

            // 05 - 冻结数据
            new DataExplain(0x05, 0x06, 0x00, 0x01, "日冻结时间",           ExplainFreezeData),   // YYMMDDhhmm
            new DataExplain(0x05, 0x06, 0x01, 0x01, "日冻结正向电能",       ExplainFreezeData),   // XXXXXX.XX  x N
            new DataExplain(0x05, 0x06, 0x02, 0x01, "日冻结反向电能",       ExplainFreezeData),  // XXXXXX.XX  x N

            // 06 - 负荷记录
            new DataExplain(0x06, 0x00, 0x00, 0x00, "最早记录块",           ExplainLoadRecord),
            new DataExplain(0x06, 0x00, 0x00, 0x01, "给定时间记录块",       ExplainLoadRecord),
            new DataExplain(0x06, 0x00, 0x00, 0x02, "最近记录块",           ExplainLoadRecord),

            // 07 - 安全认证
            new DataExplain(0x07, 0x80, 0x01, 0x01, "数据回抄",           ExplainDataReadBack),
            new DataExplain(0x07, 0x81, 0x02, 0x01, "状态查询",           ExplainQueryStatus),

            new DataExplain(0x07, 0x00, 0x00, 0x01, "身份认证指令",           ExplainIdAuthCmd),    //-->密文1 8byte  
            new DataExplain(0x07, 0x00, 0x00, 0x02, "身份认证指令",           ExplainIdAuthCmd),    //-->随机数1 8byte 
            new DataExplain(0x07, 0x00, 0x00, 0x03, "身份认证指令",           ExplainIdAuthCmd),    //-->分散因子 8byte
            new DataExplain(0x07, 0x00, 0x00, 0xFF, "身份认证指令",           ExplainIdAuthCmd),    //-->密文1+随机数1+分散因子  <--随机数2+ESAM序列号

            new DataExplain(0x07, 0x00, 0x01, 0x01, "身份认证时效设置",           ExplainIdAuthTimeSet),    //-->有效时长 2byte  
            new DataExplain(0x07, 0x00, 0x01, 0x02, "身份认证时效设置",           ExplainIdAuthTimeSet),    //-->MAC 4byte 
            new DataExplain(0x07, 0x00, 0x01, 0xFF, "身份认证时效设置",           ExplainIdAuthTimeSet),    //-->有效时长+MAC 6byte

            new DataExplain(0x07, 0x00, 0x02, 0x01, "身份认证失效",           ExplainIdAuthFail),    //-->
            new DataExplain(0x07, 0x00, 0x02, 0xFF, "身份认证失效",           ExplainIdAuthFail),    //<--状态信息 18byte

            new DataExplain(0x07, 0x01, 0x01, 0xFF, "开户",                   ExplainRegisterOrRecharge),    //-->购电金额+购电次数+MAC1+用户编号+MAC2 22byte
            new DataExplain(0x07, 0x01, 0x02, 0xFF, "充值",                   ExplainRegisterOrRecharge),    //-->购电金额+购电次数+MAC1+用户编号+MAC2 22byte

            new DataExplain(0x07, 0x02, 0x01, 0x01, "控制命令密钥更新",           ExplainKeyUpdate),    //-->密钥信息+MAC 8byte  
            new DataExplain(0x07, 0x02, 0x01, 0x02, "控制命令密钥更新",           ExplainKeyUpdate),    //-->控制命令文件线路保护密钥 32byte 
            new DataExplain(0x07, 0x02, 0x01, 0xFF, "控制命令密钥更新",           ExplainKeyUpdate),    //--> 40byte
            new DataExplain(0x07, 0x02, 0x02, 0x01, "参数密钥更新",               ExplainKeyUpdate),    //-->密钥信息+MAC 8byte  
            new DataExplain(0x07, 0x02, 0x02, 0x02, "参数密钥更新",               ExplainKeyUpdate),    //-->参数更新文件线路保护密钥 32byte 
            new DataExplain(0x07, 0x02, 0x02, 0xFF, "参数密钥更新",               ExplainKeyUpdate),    //--> 40byte
            new DataExplain(0x07, 0x02, 0x03, 0x01, "远程主控密钥更新",           ExplainKeyUpdate),    //-->密钥信息+MAC 8byte  
            new DataExplain(0x07, 0x02, 0x03, 0x02, "远程主控密钥更新",           ExplainKeyUpdate),    //-->远程主控密钥 32byte 
            new DataExplain(0x07, 0x02, 0x03, 0xFF, "远程主控密钥更新",           ExplainKeyUpdate),    //--> 40byte
        };



        #region 协议帧提取

        // 协议帧提取
        public static FrameFormat ExplainRxPacket(byte[] rxBuf)
        {
            FrameFormat rxData = new FrameFormat();

            try
            {
                int index = 0;

                for (index = 0; index < rxBuf.Length; index++)
                {
                    if (rxBuf[index] == 0x68 && rxBuf[index + 7] == 0x68) break;    // 跳过唤醒字FEFEFEFE
                }

                if (rxBuf.Length < index + FrameFixedLen) throw new Exception("无效帧");

                rxData.Header = rxBuf[index++];         //帧头

                rxData.DevAddr = new byte[LongAddrSize];//通信地址     
                Array.Copy(rxBuf, index, rxData.DevAddr, 0, rxData.DevAddr.Length);
                index += rxData.DevAddr.Length;

                rxData.Header2 = rxBuf[index++];        //帧头2

                rxData.CtrlWord.All = rxBuf[index];     //控制域
                rxData.CtrlWord.IsAckFrame = (rxBuf[index] & 0x80) > 0 ? true : false;
                rxData.CtrlWord.IsErrorAck = (rxBuf[index] & 0x40) > 0 ? true : false;
                rxData.CtrlWord.IsDataRemain = (rxBuf[index] & 0x20) > 0 ? true : false;
                rxData.CtrlWord.CmdType = (CommandType)(rxBuf[index] & 0x1F);
                index++;

                rxData.DataLen = rxBuf[index++];    // 数据域长度

                if (rxData.DataLen > 0)             //数据区
                {
                    rxData.DataBuf = new byte[rxData.DataLen];
                    for (int i = 0; i < rxData.DataLen; i++ )
                    {
                        rxData.DataBuf[i] = (byte)(rxBuf[index++] - 0x33);
                    }
                }
                rxData.Crc8 = rxBuf[index++];       //校验和
                rxData.Tail = rxBuf[index++];       //帧尾

            }
            catch (Exception ex)
            {
                rxData.CtrlWord.CmdType = CommandType.Invalid;
                MessageBox.Show("数据解析异常:" + ex.Message + ex.StackTrace);
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
                    && dat.DI2 == frame.DataBuf[2]
                    && dat.DI3 == frame.DataBuf[3])
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
                        || frame.CtrlWord.CmdType == CommandType.ReadRemainData
                        || frame.CtrlWord.CmdType == CommandType.WriteData
                        || frame.CtrlWord.CmdType == CommandType.SecurityCert)
                    {
                        type += "[" + GetDataType(frame) + "]";
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
                    && dat.DI2 == frame.DataBuf[2]
                    && dat.DI3 == frame.DataBuf[3])
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
            TreeNode parentNode = new TreeNode("645-07报文");
            TreeNode node = null;
            string strTmp = "";

            if(frame.CtrlWord.CmdType == CommandType.Invalid)
            {
                parentNode.Nodes.Add("无效帧");
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
                strTmp = "后续数据：" + (frame.CtrlWord.IsDataRemain ? "有" : "无");
                node.Nodes.Add(strTmp);
            }
            node.Expand();
            parentNode.Nodes.Add(node);

            // parentNode--帧长
            strTmp = "载荷长度：" + frame.DataLen;
            parentNode.Nodes.Add(strTmp);

            // parentNode--数据区
            node = ExplainFrameData(frame);
            if (node != null)
            {
                node.Expand();
                foreach(TreeNode subnode in node.Nodes)
                {
                    parentNode.Nodes.Add(subnode);
                }
            }

            return parentNode;
        }
        
        #endregion

        #region 无【数据标识】--其他命令
        private static TreeNode ExplainBroadcastTime(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 6) return payloadNode;

                strTmp = "校时时间：" + DateTime.Now.Year / 100
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
                // 应答 -- 无
            }

            return payloadNode;
        }

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

        private static TreeNode ExplainFreezeCmd(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 6) return payloadNode;

                strTmp = "冻结时间："
                        + (buf[index + 3] == 0x99 ? "" : buf[index + 3].ToString("X2") + "-")
                        + (buf[index + 2] == 0x99 ? "" : buf[index + 2].ToString("X2")) + " "
                        + (buf[index + 1] == 0x99 ? "" : buf[index + 1].ToString("X2")) + ":"
                        + (buf[index] == 0x99 ? "" : buf[index].ToString("X2")) + ":00";
                payloadNode.Nodes.Add(strTmp);
                index += 6;

            }
            else
            {
                // 应答
            }

            return payloadNode;
        }

        private static string GetCommSpeed(byte speedWord)
        {
            string speed = "未知";

            for(int i = 0; i < 6; i++)
            {
                if((speedWord & 0x40) > 0)
                {
                    speed = Math.Pow(2, 5-i).ToString() + "bps";
                    break;
                }

                speedWord = (byte)(speedWord << 1);
            }

            return speed ;
        }
        private static TreeNode ExplainChangeDataRate(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 1) return payloadNode;

                strTmp = "通信速率：" + GetCommSpeed(frame.DataBuf[0]);
                payloadNode.Nodes.Add(strTmp);
                index += 1;

            }
            else
            {
                // 应答
                if (buf.Length < index + 1) return payloadNode;

                strTmp = "通信速率：" + GetCommSpeed(frame.DataBuf[0]);
                payloadNode.Nodes.Add(strTmp);
                index += 1;
            }

            return payloadNode;
        }

        private static TreeNode ExplainChangePassword(FrameFormat frame)
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
                strTmp = "旧密码  ：" + buf[index + 1].ToString("X2") + buf[index + 2].ToString("X2") + buf[index + 3].ToString("X2") 
                        + " (权限" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;
                strTmp = "新密码  ：" + buf[index + 1].ToString("X2") + buf[index + 2].ToString("X2") + buf[index + 3].ToString("X2")
                        + " (权限" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;
            }
            else
            {
                // 应答
                if (buf.Length < index + 4) return payloadNode;

                strTmp = "新密码  ：" + buf[index + 1].ToString("X2") + buf[index + 2].ToString("X2") + buf[index + 3].ToString("X2")
                        + " (权限" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;

            }

            return payloadNode;
        }

        private static TreeNode ExplainAmeterClear(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 8) return payloadNode;

                strTmp = "表密码：" + buf[index + 1].ToString("X2") + buf[index + 2].ToString("X2") + buf[index + 3].ToString("X2")
                        + " (权限" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;
                strTmp = "操作员：" + buf[index].ToString("X2") + buf[index + 1].ToString("X2") + buf[index + 2].ToString("X2")
                        + buf[index + 3].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 4;
            }
            else
            {
                // 应答
            }

            return payloadNode;
        }

        private static TreeNode ExplainEventClear(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 12) return payloadNode;

                strTmp = "表密码：" + buf[index + 1].ToString("X2") + buf[index + 2].ToString("X2") + buf[index + 3].ToString("X2")
                        + " (权限" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;
                strTmp = "操作员：" + buf[index].ToString("X2") + buf[index + 1].ToString("X2") + buf[index + 2].ToString("X2")
                        + buf[index + 3].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 4;
                strTmp = "清除项：" + BitConverter.ToUInt32(buf, index).ToString("X8");
                payloadNode.Nodes.Add(strTmp);
                index += 4;
            }
            else
            {
                // 应答
            }

            return payloadNode;
        }

        private static TreeNode ExplainSecurityCtrl(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 16) return payloadNode;

                strTmp = "表密码：" + buf[index + 1].ToString("X2") + buf[index + 2].ToString("X2") + buf[index + 3].ToString("X2")
                        + " (权限" + buf[index].ToString("X2") + ")";
                payloadNode.Nodes.Add(strTmp);
                index += 4;
                strTmp = "操作员：" + buf[index].ToString("X2") + buf[index + 1].ToString("X2") + buf[index + 2].ToString("X2")
                        + buf[index + 3].ToString("X2");
                payloadNode.Nodes.Add(strTmp);
                index += 4;
                strTmp = "控制类型：";
                switch(buf[index])
                {
                    case 0x1A: strTmp += "跳闸";  break;
                    case 0x1B: strTmp += "合闸允许"; break;
                    case 0x2A: strTmp += "报警"; break;
                    case 0x2B: strTmp += "报警解除"; break;
                    case 0x3A: strTmp += "保电"; break;
                    case 0x3B: strTmp += "保电解除"; break;
                    default: strTmp += "未知"; break;
                }
                payloadNode.Nodes.Add(strTmp);
                index += 2; // 跳过保留的1字节
                strTmp = "命令截止时间：" + DateTime.Now.Year / 100
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
                // 应答
            }

            return payloadNode;
        }

        private static TreeNode ExplainPortOutputCtrl(FrameFormat frame)
        {
            TreeNode payloadNode = new TreeNode("数据载荷");
            byte[] buf = frame.DataBuf;
            string strTmp = "";
            int index = 0;

            if (!frame.CtrlWord.IsAckFrame)
            {
                // 请求
                if (buf.Length < index + 1) return payloadNode;

                strTmp = "输出控制字：";
                switch (buf[index])
                {
                    case 0x00: strTmp += "时钟秒脉冲"; break;
                    case 0x01: strTmp += "需量周期"; break;
                    case 0x02: strTmp += "时段投切"; break;
                    default: strTmp += "未知"; break;
                }
                payloadNode.Nodes.Add(strTmp);
                index += 1;
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
                if(strDataType.Contains("电能"))  // 组合有功/正向有功/反向有功总电能
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

                strTmp = "回抄的数据(" + (buf.Length - 16) + " byte)：" + Util.GetStringHexFromBytes(buf, index , buf.Length - 16);
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

                if(buf[0] == 0x01)
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
