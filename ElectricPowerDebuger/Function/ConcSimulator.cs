using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using ElectricPowerLib.Common;
using ElectricPowerDebuger.Dialog;
using ElectricPowerLib.Protocol;
using System.Threading;


namespace ElectricPowerDebuger.Function
{
    public partial class ConcSimulator : UserControl
    {
        private string strConcAddr = "";
        protected bool bPortOpened = false;
        private int PortBufRdPos = 0;
        private int PortBufWrPos = 0;
        private byte[] PortRxBuf = new Byte[2000];

        private int _50MsTimer = 0;

        private byte SequenceNo = (byte)(DateTime.Now.Millisecond);
        private byte CmdSn;
        private Command Cmd = Command.任务空闲;
        private Command SubCmd = Command.任务空闲;
        private byte[] CmdParams = null;
        private int CmdRetryTimes;
        private int CmdWaitTime;
        private bool bTransceiveEnable;

        private byte NodeCountOnePacket = 20;
        private int TaskIdSequence = 0;
        private byte[] UndoTaskTab = new byte[400];
        private int UndoTaskTabCount = 0;

        public delegate void SerialDataRecievedEventHandler(object sender, SerialDataReceivedEventArgs e); 
  
        private enum Command
        {
            复位硬件 = 0,
            初始化档案,
            初始化任务,

            添加任务,
            删除任务,
            查询未完成任务数,
            查询未完成任务列表,
            查询未完成任务详细信息,
            查询剩余可分配任务数,
            添加多播任务,
            启动任务,
            暂停任务,

            查询厂商代码和版本信息,
            查询本地通信模块运行模式信息,
            查询主节点地址,
            查询通信延时时长,
            查询从节点数量,
            查询从节点信息,
            查询从节点主动注册进度,
            查询从节点的父节点,

            设置主节点地址,
            添加从节点,
            删除从节点,
            允许_禁止上报从节点事件,
            激活从节点主动注册,
            终止从节点主动注册,

            从设备导入档案,
            导出档案到设备,

            任务空闲
        }
        public ConcSimulator()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            strConcAddr = XmlHelper.GetNodeDefValue(FrmMain.SystemConfigPath, "/Config/ConcSimulator_ConcAddr", "");
            if (strConcAddr == "")
            {
                strConcAddr = "000020160618";
                XmlHelper.SetNodeValue(FrmMain.SystemConfigPath, "/Config", "ConcSimulator_ConcAddr", strConcAddr);
            }
            string strPortName = XmlHelper.GetNodeDefValue(FrmMain.SystemConfigPath, "/Config/ConcSimulator_PortName", "");
            cmbPort.Items.AddRange(new object[] {strPortName});
            cmbPort.Text = strPortName;
            cmbBaudrate.Text = XmlHelper.GetNodeDefValue(FrmMain.SystemConfigPath, "/Config/ConcSimulator_Baudrate", "");
            if ("0" == XmlHelper.GetNodeDefValue(FrmMain.SystemConfigPath, "/Config/ConcSimulator_CommonMsgAutoScroll", "1"))
            {
                tsmiAutoScrollCommMsg.Checked = false;
            }
            else
            {
                tsmiAutoScrollCommMsg.Checked = true;
            }

        }

        #region 串口通信控制
        private void cmbPort_Click(object sender, EventArgs e)
        {
            cmbPort.Items.Clear();
            foreach (string portName in SerialPort.GetPortNames())
            {
                cmbPort.Items.Add(portName);
            }
        }
        private void btOpenPort_Click(object sender, EventArgs e)
        {
            if (cmbPort.SelectedIndex < 0 || cmbPort.Text == "")
            {
                MessageBox.Show("请先选择通讯串口!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cmbBaudrate.SelectedIndex < 0 || cmbBaudrate.Text == "")
            {
                MessageBox.Show("请先选择通讯的波特率!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (btOpenPort.Text == "打开端口")
            {
                if (true == port_Ctrl(true))
                {
                    btOpenPort.Text = "关闭端口";
                    btOpenPort.BackColor = Color.GreenYellow;
                    cmbPort.Enabled = false;
                    cmbBaudrate.Enabled = false;
                    bPortOpened = true;
                    XmlHelper.SetNodeValue(FrmMain.SystemConfigPath, "/Config", "ConcSimulator_PortName", cmbPort.Text);
                    XmlHelper.SetNodeValue(FrmMain.SystemConfigPath, "/Config", "ConcSimulator_Baudrate", cmbBaudrate.Text);
                }
                else
                {
                    btOpenPort.Text = "打开端口";
                    btOpenPort.BackColor = Color.Silver;
                    cmbPort.Enabled = true;
                    cmbBaudrate.Enabled = true;
                    bPortOpened = false;
                }
            }
            else
            {
                port_Ctrl(false);
                btOpenPort.Text = "打开端口";
                btOpenPort.BackColor = Color.Silver;
                cmbPort.Enabled = true;
                cmbBaudrate.Enabled = true;
                bPortOpened = false;
            }
        }
        private bool port_Ctrl(bool ctrl)
        {
            if (true == ctrl)
            {
                if (serialPort.IsOpen == false ||
                    serialPort.BaudRate != Convert.ToInt32(cmbBaudrate.Text) ||
                    serialPort.PortName != cmbPort.Text)
                {
                    try
                    {
                        serialPort.Close();
                        serialPort.BaudRate = Convert.ToInt32(cmbBaudrate.Text);
                        serialPort.PortName = cmbPort.Text;
                        serialPort.Open();
                        return true;
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("打开通信端口失败" + "," + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            else
            {
                try
                {
                    serialPort.DiscardInBuffer();
                    serialPort.DiscardOutBuffer();
                    serialPort.Close();
                    return true;
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("关闭通信端口失败" + "," + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return true;
        }
        private bool ComPortStatus()
        {
            if (bPortOpened == false)
            {
                MessageBox.Show("请先打开通信串口！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int iRead;

            if (InvokeRequired)
            {
                try
                {
                    Invoke(new SerialDataRecievedEventHandler(serialPort_DataReceived), new object[] { sender, e });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            else
            {
                try
                {
                    while (serialPort.BytesToRead > 0)
                    {
                        iRead = serialPort.ReadByte();
                        PortRxBuf[PortBufWrPos] = (byte)iRead;
                        PortBufWrPos = (PortBufWrPos + 1) % PortRxBuf.Length;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
        private void timerConcSim_Tick(object sender, EventArgs e)
        {
            int len, sum;

            if (true == bTransceiveEnable)
            {
                if (CmdWaitTime > 0)
                {
                    CmdWaitTime--;
                }
                else
                {
                    TransmitSwitch();
                }
            }

            if (_50MsTimer++ > 50 / timerConcSim.Interval)
            {
                _50MsTimer = 0;
                while (true)
                {
                    len = (PortBufWrPos >= PortBufRdPos) ? (PortBufWrPos - PortBufRdPos) : (PortRxBuf.Length - PortBufRdPos + PortBufWrPos);
                    if (len < ProtoLocal_South.FrameFixedLen)
                    {
                        break;
                    }
                    if (PortRxBuf[PortBufRdPos % PortRxBuf.Length] != ProtoLocal_South.FrameHeader)
                    {
                        PortBufRdPos = (UInt16)((PortBufRdPos + 1) % PortRxBuf.Length);
                        continue;
                    }
                    sum = PortRxBuf[(PortBufRdPos + 1) % PortRxBuf.Length] + PortRxBuf[(PortBufRdPos + 2) % PortRxBuf.Length];
                    if (sum > len)
                    {
                        break;
                    }
                    if (PortRxBuf[(PortBufRdPos + sum - 1) % PortRxBuf.Length] != ProtoLocal_South.FrameTail)
                    {
                        PortBufRdPos = (UInt16)((PortBufRdPos + 1) % PortRxBuf.Length);
                        continue;
                    }
                    byte[] rxBuf = new byte[sum];
                    for (int i = 0; i < sum; i++)
                    {
                        rxBuf[i] = PortRxBuf[(PortBufRdPos + i) % PortRxBuf.Length];
                    }
                    if (ProtoLocal_South.CalCRC8(rxBuf, 3, sum - 5) != rxBuf[sum - 2])
                    {
                        PortBufRdPos = (UInt16)((PortBufRdPos + 1) % PortRxBuf.Length);
                        continue;
                    }
                    PortBufRdPos = (UInt16)((PortBufRdPos + sum) % PortRxBuf.Length);

                    ProtoLocal_South.PacketFormat rxData = ProtoLocal_South.ExplainRxPacket(rxBuf);
                    AddToCommMsg(true, rxBuf, rxData);
                    ExplainPacket(rxData);
                    ReceiveSwitch(rxData);
                }
            }
        }
        private void TransmitSwitch()
        {
            switch (Cmd)
            {
                case Command.复位硬件: TransmitResetDevice(); break;
                case Command.初始化档案: TransmitInitialDocument(); break;
                case Command.初始化任务: TransmitInitialTask(); break;

                case Command.添加任务: TransmitAddTask(); break;
                case Command.删除任务: TransmitDelTask(); break;
                case Command.查询未完成任务数: TransmitReadUndoTaskNum(); break;
                case Command.查询未完成任务列表: TransmitReadUndoTaskTab(); break;
                case Command.查询未完成任务详细信息: TransmitReadUndoTaskInfo(); break;
                case Command.查询剩余可分配任务数: TransmitReadRemainTask(); break;
                case Command.添加多播任务: TransmitAddMultiTask(); break;
                case Command.启动任务: TransmitStartTask(); break;
                case Command.暂停任务: TransmitPauseTask(); break;

                case Command.查询厂商代码和版本信息: TransmitVersionInfo(); break;
                case Command.查询本地通信模块运行模式信息: TransmitModuleWorkMode(); break;
                case Command.查询主节点地址: TransmitReadAddress(); break;
                case Command.查询通信延时时长: TransmitReadComDelayTime(); break;
                case Command.查询从节点数量: TransmitReadSubnodeCount(); break;
                case Command.查询从节点信息: TransmitReadSubnodeInfo(); break;
                case Command.查询从节点主动注册进度: TransmitReadSubnodeRegisterProgress(); break;
                case Command.查询从节点的父节点: TransmitReadFatherNode(); break;

                case Command.设置主节点地址: TransmitSetConcAddr(); break;
                case Command.添加从节点: TransmitAddSubNode(); break;
                case Command.删除从节点: TransmitDelSubNode(); break;
                case Command.允许_禁止上报从节点事件: TransmitControlEventReport(); break;
                case Command.激活从节点主动注册: TransmitStartRegister(); break;
                case Command.终止从节点主动注册: TransmitStopRegister(); break;

                case Command.从设备导入档案: TransmitImportFromDevice(); break;
                case Command.导出档案到设备: TransmitExportToDevice(); break;

                default: break;
            }
        }
        private void ReceiveSwitch(ProtoLocal_South.PacketFormat RxData)
        {
            // 设备主动发起的
            if ((RxData.CtrlWord & 0x40) == 0x40)
            {
                switch (RxData.Afn)
                {
                    case ProtoLocal_South.Afn.Afn5_ReportData:
                        switch (RxData.DataId[0])
                        {
                            case 0x01: ReceiveReportTaskData(RxData); break; 
                            case 0x02: ReceiveReportSubNodeEvent(RxData); break;
                            case 0x03: ReceiveReportSubNodeInfo(RxData); break;
                            case 0x04: ReceiveReportSubNodeRegisterEnd(RxData); break;
                            case 0x05: ReceiveReportTaskStatus(RxData); break;
                            default: break;
                        }
                        break;

                    case ProtoLocal_South.Afn.Afn6_RequestInfo:
                        switch (RxData.DataId[0])
                        {
                            case 0x01: ReceiveRequestRtc(RxData); break;
                            default: break;
                        }
                        break;
                    default: break;
                }
            }
            else
            {
                switch (Cmd)
                {
                    case Command.复位硬件: ReceiveResetDevice(RxData); break;
                    case Command.初始化档案: ReceiveInitialDocument(RxData); break;
                    case Command.初始化任务: ReceiveInitialTask(RxData); break;

                    case Command.添加任务: ReceiveAddTask(RxData); break;
                    case Command.删除任务: ReceiveDelTask(RxData); break;
                    case Command.查询未完成任务数: ReceiveReadUndoTaskNum(RxData); break;
                    case Command.查询未完成任务列表: ReceiveReadUndoTaskTab(RxData); break;
                    case Command.查询未完成任务详细信息: ReceiveReadUndoTaskInfo(RxData); break;
                    case Command.查询剩余可分配任务数: ReceiveReadRemainTask(RxData); break;
                    case Command.添加多播任务: ReceiveAddMultiTask(RxData); break;
                    case Command.启动任务: ReceiveStartTask(RxData); break;
                    case Command.暂停任务: ReceivePauseTask(RxData); break;

                    case Command.查询厂商代码和版本信息: ReceiveVersionInfo(RxData); break;
                    case Command.查询本地通信模块运行模式信息: ReceiveModuleWorkMode(RxData); break;
                    case Command.查询主节点地址: ReceiveReadAddress(RxData); break;
                    case Command.查询通信延时时长: ReceiveReadComDelayTime(RxData); break;
                    case Command.查询从节点数量: ReceiveReadSubnodeCount(RxData); break;
                    case Command.查询从节点信息: ReceiveReadSubnodeInfo(RxData); break;
                    case Command.查询从节点主动注册进度: ReceiveReadSubnodeRegisterProgress(RxData); break;
                    case Command.查询从节点的父节点: ReceiveReadFatherNode(RxData); break;

                    case Command.设置主节点地址: ReceiveSetConcAddr(RxData); break;
                    case Command.添加从节点: ReceiveAddSubNode(RxData); break;
                    case Command.删除从节点: ReceiveDelSubNode(RxData); break;
                    case Command.允许_禁止上报从节点事件: ReceiveControlEventReport(RxData); break;
                    case Command.激活从节点主动注册: ReceiveStartRegister(RxData); break;
                    case Command.终止从节点主动注册: ReceiveStopRegister(RxData); break;

                    case Command.从设备导入档案: ReceiveImportFromDevice(RxData); break;
                    case Command.导出档案到设备: ReceiveExportToDevice(RxData); break;

                    default: break;
                }
            }
        }
        private void DataTransmit(ProtoLocal_South.PacketFormat TxData)
        {
            try
            {
                int len = ProtoLocal_South.FrameFixedLen;
                if ((TxData.CtrlWord & 0x20) == 0x20)
                {
                    len += ProtoLocal_South.FrameAddrLen;
                }
                if (TxData.DataBuf != null)
                {
                    len += TxData.DataBuf.Length;
                }
                byte[] txBuf = new byte[len];
                len = 0;
                txBuf[len++] = ProtoLocal_South.FrameHeader;
                txBuf[len++] = (byte)(txBuf.Length);
                txBuf[len++] = (byte)(txBuf.Length >> 8);
                txBuf[len++] = TxData.CtrlWord;
                if ((TxData.CtrlWord & 0x20) == 0x20)
                {
                    Array.Copy(TxData.SrcAddr, 0, txBuf, len, TxData.SrcAddr.Length);
                    len += TxData.SrcAddr.Length;
                    Array.Copy(TxData.DstAddr, 0, txBuf, len, TxData.DstAddr.Length);
                    len += TxData.DstAddr.Length;
                }
                txBuf[len++] = (byte)(TxData.Afn);
                txBuf[len++] = TxData.SerialNo;
                Array.Copy(TxData.DataId, 0, txBuf, len, TxData.DataId.Length);
                len += TxData.DataId.Length;
                if (TxData.DataBuf != null)
                {
                    Array.Copy(TxData.DataBuf, 0, txBuf, len, TxData.DataBuf.Length);
                    len += TxData.DataBuf.Length;
                }
                txBuf[len++] = ProtoLocal_South.CalCRC8(txBuf, 3, len - 3);
                txBuf[len++] = ProtoLocal_South.FrameTail;

                try
                {
                    serialPort.Write(txBuf, 0, txBuf.Length);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("发送数据失败," + ex.Message, "错误");
                }
                AddToCommMsg(false, txBuf, TxData);
                ExplainPacket(TxData);
            }
            catch (System.Exception ex)
            {
                Cmd = Command.任务空闲;
                MessageBox.Show("串口通信出现异常，" + ex.Message, "通信出现错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public bool CmdStatus()
        {
            if (Cmd != Command.任务空闲)
            {
                MessageBox.Show("通信任务执行中，请稍后再试！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            return true;
        }
        #endregion

        #region 解释信息包
        private void ExplainPacket(ProtoLocal_South.PacketFormat CommData)
        {
            switch ((ProtoLocal_South.Afn)(CommData.Afn))
            {
                case ProtoLocal_South.Afn.Afn0_Ack: 
                    Explain_Afn0Ack(CommData); 
                    break;
                case ProtoLocal_South.Afn.Afn1_Initial: 
                    Explain_Afn1Initial(CommData); 
                    break;
                case ProtoLocal_South.Afn.Afn2_TaskManage:
                    Explain_Afn2TaskManage(CommData);
                    break;
                case ProtoLocal_South.Afn.Afn3_ReadParams:
                    Explain_Afn3ReadParams(CommData);
                    break;
                case ProtoLocal_South.Afn.Afn4_WriteParams:
                    Explain_Afn4WriteParams(CommData);
                    break;

                case ProtoLocal_South.Afn.Afn5_ReportData:
                    Explain_Afn5ReportData(CommData);
                    break;

                case ProtoLocal_South.Afn.Afn6_RequestInfo:
                    Explain_Afn6RequestInfo(CommData);
                    break;
                    
                case ProtoLocal_South.Afn.Afn7_FileTransfer:
                    break;
            }
        }
        private void Explain_Afn0Ack(ProtoLocal_South.PacketFormat RxData)
        {
            switch (RxData.DataId[0])
            {
                case 0x01:
                    rtbCommMsgAdd("\n确认应答", Color.Green);
                    rtbCommMsgAdd(" 等待时间:" + (RxData.DataBuf[0] + RxData.DataBuf[1] * 256).ToString("D") + "秒\n", Color.Green);
                    break;
                
                case 0x02:
                    rtbCommMsgAdd("\n否认应答 错误状态字", Color.Green);
                    switch (RxData.DataBuf[0])
                    {
                        case 0: rtbCommMsgAdd("(0):通信超时\n", Color.Green); break;
                        case 1: rtbCommMsgAdd("(1):无效数据标识内容\n", Color.Green); break;
                        case 2: rtbCommMsgAdd("(2):长度错误\n", Color.Green); break;
                        case 3: rtbCommMsgAdd("(3):校验错误\n", Color.Green); break;
                        case 4: rtbCommMsgAdd("(4):数据标识编码不存在\n", Color.Green); break;
                        case 5: rtbCommMsgAdd("(5):格式错误\n", Color.Green); break;
                        case 6: rtbCommMsgAdd("(6):表号重复\n", Color.Green); break;
                        case 7: rtbCommMsgAdd("(7):表号不存在\n", Color.Green); break;
                        case 8: rtbCommMsgAdd("(8):电表应用层无应答\n", Color.Green); break;
                        case 9: rtbCommMsgAdd("(9):主节点忙\n", Color.Green); break;
                        case 10: rtbCommMsgAdd("(10):主节点不支持此命令\n", Color.Green); break;
                        case 11: rtbCommMsgAdd("(11):从节点不应答\n", Color.Green); break;
                        case 12: rtbCommMsgAdd("(12):从节点不在网内\n", Color.Green); break;
                        case 13: rtbCommMsgAdd("(13):添加任务时剩余可分配任务数不足\n", Color.Green); break;
                        case 14: rtbCommMsgAdd("(14):上报任务数据时任务不存在\n", Color.Green); break;
                        case 15: rtbCommMsgAdd("(15):任务ID重复\n", Color.Green); break;
                        case 16: rtbCommMsgAdd("(16):查询任务时模块没有此任务\n", Color.Green); break;
                        default: rtbCommMsgAdd("未知错误\n", Color.Green); break;
                    }
                    break;
                default:
                    break;
            }
        }
        private void Explain_Afn1Initial(ProtoLocal_South.PacketFormat RxData)
        {
            switch (RxData.DataId[0])
            {
                case 0x01:
                case 0x02:
                case 0x03: 
                    rtbCommMsgAdd(" 无数据域\n", Color.Green);
                    break;
                default:
                    break;
            }
        }
        private void Explain_Afn2TaskManage(ProtoLocal_South.PacketFormat RxData)
        {
            switch (RxData.DataId[0])
            {
                case 0x01:      // 添加任务
                    if ((RxData.CtrlWord & 0x80) == 0x00)   // 下行报文
                    {
                        rtbCommMsgAdd("\n任务ID：" + (RxData.DataBuf[0] + RxData.DataBuf[1] * 256).ToString("X4"), Color.Green);
                        rtbCommMsgAdd("  响应标识：" + ((RxData.DataBuf[2] & 0x80) == 0x80 ? "需要" : "不需要"), Color.Green);
                        rtbCommMsgAdd("  优先级：" + (RxData.DataBuf[2] & 0x0F).ToString("D") + "级", Color.Green);
                        rtbCommMsgAdd("\n超时时间：" + (RxData.DataBuf[3] + RxData.DataBuf[4] * 256).ToString("D") + "秒", Color.Green);
                        rtbCommMsgAdd("\n报文长度：" + RxData.DataBuf[5].ToString("D"), Color.Green);
                        rtbCommMsgAdd("  报文内容：" + Util.GetStringHexFromBytes(RxData.DataBuf, 6, RxData.DataBuf[5], " ", false) + "\n", Color.Green);
                    }
                    break;
                case 0x02:      // 删除任务
                    if ((RxData.CtrlWord & 0x80) == 0x00)   // 下行报文
                    {
                        rtbCommMsgAdd("\n删除任务 ID:" + (RxData.DataBuf[0] + RxData.DataBuf[1] * 256).ToString("X4") + "\n", Color.Green);
                    }
                    break;
                case 0x03:      // 查询未完成任务数
                    if ((RxData.CtrlWord & 0x80) == 0x00)   // 下行报文
                    {
                        rtbCommMsgAdd(" 无数据域\n", Color.Green);
                    }
                    else                                    // 上行报文
                    {
                        rtbCommMsgAdd("\n未完成任务数:" + (RxData.DataBuf[0] + RxData.DataBuf[1] * 256).ToString("D") + "\n", Color.Green);
                    }
                    break;
                case 0x04:      // 查询未完成任务列表
                    if ((RxData.CtrlWord & 0x80) == 0x00)   // 下行报文
                    {
                        rtbCommMsgAdd("\n起始任务序号:" + (RxData.DataBuf[0] + RxData.DataBuf[1] * 256).ToString("D"), Color.Green);
                        rtbCommMsgAdd("  查询任务数量：" + RxData.DataBuf[2].ToString("D") + "\n", Color.Green);
                    }
                    else                                    // 上行报文
                    {
                        int count, len = 0;
                        count = RxData.DataBuf[len++] + RxData.DataBuf[len++] * 256;
                        rtbCommMsgAdd("\n本次上报未完成任务数量:" + count.ToString("D"), Color.Green);
                        for (int i = 0; i < count; i++)
                        {
                            rtbCommMsgAdd(((i % 10) == 0 ? "\n" : "\t"), Color.Green);
                            rtbCommMsgAdd(Util.GetStringHexFromBytes(RxData.DataBuf, len, 2, "", true), Color.Green);
                            len += 2;
                        }
                        rtbCommMsgAdd("\n", Color.Green);
                    }
                    break;
                case 0x05:      // 查询未完成任务详细信息
                    if ((RxData.CtrlWord & 0x80) == 0x00)   // 下行报文
                    {
                        rtbCommMsgAdd("\n任务ID:" + (RxData.DataBuf[0] + RxData.DataBuf[1] * 256).ToString("D5") + "\n", Color.Green);
                    }
                    else                                    // 上行报文
                    {
                        int count, len = 0;
                        rtbCommMsgAdd("\n任务ID:" + (RxData.DataBuf[len++] + RxData.DataBuf[len++] * 256).ToString("X5"), Color.Green);
                        rtbCommMsgAdd("   响应标识:" + ((RxData.DataBuf[len] & 0x80) == 0x80 ? "需要返回数据" : "不需要返回数据"), Color.Green);
                        rtbCommMsgAdd("   任务优先级:" + (RxData.DataBuf[len] & 0x0F).ToString("D") + "级\n", Color.Green);
                        len += 1;
                        count = RxData.DataBuf[len++] + RxData.DataBuf[len++] * 256;
                        rtbCommMsgAdd("任务目的地址个数:" + count.ToString("D") + "\n", Color.Green);
                        for (int iLoop = 0; iLoop < count; iLoop++)
                        {
                            if (iLoop > 0)
                            {
                                rtbCommMsgAdd((iLoop % 5 == 0 ? "\n" : "\t"), Color.Green);
                            }
                            rtbCommMsgAdd(Util.GetStringHexFromBytes(RxData.DataBuf, len, ProtoLocal_South.LongAddrSize, "", true), Color.Green);
                            len += ProtoLocal_South.LongAddrSize;
                        }
                        if (count > 0)
                        {
                            rtbCommMsgAdd("\n", Color.Green);
                        }
                        int length = RxData.DataBuf[len++];
                        rtbCommMsgAdd("报文长度:" + length.ToString("D") + "\n", Color.Green);
                        rtbCommMsgAdd("报文内容:" + Util.GetStringHexFromBytes(RxData.DataBuf, len, length, " "), Color.Green);
                        rtbCommMsgAdd("\n", Color.Green);
                    }
                    break;
                case 0x06:
                    if ((RxData.CtrlWord & 0x80) == 0x00)   // 下行报文
                    {
                        rtbCommMsgAdd("  无数据域\n", Color.Green);
                    }
                    else                                    // 上行报文
                    {
                        rtbCommMsgAdd("\n剩余可分配任务数量:" + (RxData.DataBuf[0] + RxData.DataBuf[1] * 256).ToString("D"), Color.Green);
                        rtbCommMsgAdd("\n", Color.Green);
                    }
                    break;
                case 0x07:      // 添加多播任务
                    if ((RxData.CtrlWord & 0x80) == 0x00)   // 下行报文
                    {
                        rtbCommMsgAdd("\n任务ID:" + (RxData.DataBuf[0] + RxData.DataBuf[1] * 256).ToString("X4"), Color.Green);
                        rtbCommMsgAdd("  响应标识：" + ((RxData.DataBuf[2] & 0x80) == 0x80 ? "需要" : "不需要"), Color.Green);
                        rtbCommMsgAdd("  优先级：" + (RxData.DataBuf[2] & 0x0F).ToString("D") + "级", Color.Green);
                        int nodeCount = (RxData.DataBuf[3] + RxData.DataBuf[4] * 256);
                        int index = 5;
                        byte multiTaskFlag = CmdParams[0];    // 单播/多播标识
                        if (0x02 == multiTaskFlag)  // 多播所有
                        {
                            rtbCommMsgAdd("\n从节点数量：所有（已下装的档案）\n", Color.Green);
                        }
                        else    // 多播已选择
                        {
                            rtbCommMsgAdd("\n从节点数量：" + nodeCount.ToString("D") + "\n", Color.Green);
                            for (int iLoop = 0; iLoop < nodeCount; iLoop++)
                            {
                                if (iLoop > 0)
                                {
                                    rtbCommMsgAdd((iLoop % 5 == 0 ? "\n" : "\t"), Color.Green);
                                }
                                rtbCommMsgAdd(Util.GetStringHexFromBytes(RxData.DataBuf, index, ProtoLocal_South.LongAddrSize, "", true), Color.Green);
                                index += ProtoLocal_South.LongAddrSize;
                            }
                        }
                        rtbCommMsgAdd("\n超时时间：" + (RxData.DataBuf[index] + RxData.DataBuf[index + 1] * 256).ToString("D") + "秒", Color.Green);
                        index += 2;
                        rtbCommMsgAdd("\n报文长度：" + RxData.DataBuf[index].ToString("D"), Color.Green);
                        rtbCommMsgAdd("  报文内容：" + Util.GetStringHexFromBytes(RxData.DataBuf, index + 1, RxData.DataBuf[index], " ", false) + "\n", Color.Green);
                    }
                    break;
                case 0x08:      // 启动任务
                case 0x09:      // 暂停任务
                    if ((RxData.CtrlWord & 0x80) == 0x00)   // 下行报文
                    {
                        rtbCommMsgAdd("  无数据域\n", Color.Green);
                    }
                    break;
                default:
                    break;
            }
        }
        private void Explain_Afn3ReadParams(ProtoLocal_South.PacketFormat RxData)
        {
            string strMsg = "";

            switch (RxData.DataId[0])
            {
                case 0x01:                  // 查询厂商代码和版本信息
                    if ((RxData.CtrlWord & 0x80) == 0x00)   // 下行报文
                    {
                        rtbCommMsgAdd("  无数据域\n", Color.Green);
                    }
                    else                                    // 上行报文
                    {
                        rtbCommMsgAdd("\n厂商代码:" + (char)(RxData.DataBuf[1]) + (char)(RxData.DataBuf[0]), Color.Green);
                        rtbCommMsgAdd("\t\t芯片代码:" + (char)(RxData.DataBuf[3]) + (char)RxData.DataBuf[2], Color.Green);
                        rtbCommMsgAdd("\n版本时间:" + RxData.DataBuf[4].ToString("X2") + "/"
                                                          + RxData.DataBuf[5].ToString("X2") + "/"
                                                          + RxData.DataBuf[6].ToString("X2"), Color.Green);
                        rtbCommMsgAdd("\t程序版本:" + RxData.DataBuf[8].ToString("X") + "." + RxData.DataBuf[7].ToString("X2") + "\n", Color.Green);
                    }
                    break;
                case 0x02:                  // 查询本地通信模块运行模式信息
                    if ((RxData.CtrlWord & 0x80) == 0x00)   // 下行报文
                    {
                        rtbCommMsgAdd(" 无数据域\n", Color.Green);
                    }
                    else                                    // 上行报文
                    {
                        strMsg = "\n通信方式:";
                        switch (RxData.DataBuf[0] & 0x0F)
                        {
                            case 0x01: strMsg += "窄带电力线载波通信"; break;
                            case 0x02: strMsg += "宽带电力线载波通信"; break;
                            case 0x03: strMsg += "微功率无线通信"; break;
                            case 0x04: strMsg += "窄带双模"; break;
                            case 0x05: strMsg += "宽带双模"; break;
                            default: strMsg += "保留"; break;
                        }
                        rtbCommMsgAdd(strMsg, Color.Green);
                        rtbCommMsgAdd("\t\t\t最大支持的协议报文长度:" + (RxData.DataBuf[1] + RxData.DataBuf[2] * 256).ToString("D"), Color.Green);
                        rtbCommMsgAdd("\n文件传输支持的最大单包长度:" + (RxData.DataBuf[3] + RxData.DataBuf[4] * 256).ToString("D"), Color.Green);
                        rtbCommMsgAdd("\t\t升级操作等待时间:" + RxData.DataBuf[5].ToString("D") + "分钟", Color.Green);
                        rtbCommMsgAdd("\n主节点地址:" + Util.GetStringHexFromBytes(RxData.DataBuf, 6, ProtoLocal_South.LongAddrSize, "", true), Color.Green);
                        lbConcAddr.Text = "主节点地址:" + Util.GetStringHexFromBytes(RxData.DataBuf, 6, ProtoLocal_South.LongAddrSize, "", true);
                        rtbCommMsgAdd("\t\t\t支持的最大从节点数量:" + (RxData.DataBuf[12] + RxData.DataBuf[13] * 256).ToString("D"), Color.Green);
                        rtbCommMsgAdd("\n当前从节点数量:" + (RxData.DataBuf[14] + RxData.DataBuf[15] * 256).ToString("D"), Color.Green);
                        NodeCountOnePacket = (byte)(RxData.DataBuf[16] + RxData.DataBuf[17] * 256);
                        rtbCommMsgAdd("\t\t\t支持单次读写从节点信息的最大数量:" + (RxData.DataBuf[16] + RxData.DataBuf[17] * 256).ToString("D"), Color.Green);
                        rtbCommMsgAdd("\n通信模块使用的协议发布日期:" + RxData.DataBuf[18].ToString("X2") + "/"
                                                                      + RxData.DataBuf[19].ToString("X2") + "/"
                                                                      + RxData.DataBuf[20].ToString("X2"), Color.Green);
                        rtbCommMsgAdd("\t厂商代码:" + (char)(RxData.DataBuf[22]) + (char)(RxData.DataBuf[21]), Color.Green);
                        rtbCommMsgAdd("\n芯片代码:" + (char)(RxData.DataBuf[24]) + (char)RxData.DataBuf[23], Color.Green);
                        rtbCommMsgAdd("\t\t\t\t版本时间:" + RxData.DataBuf[25].ToString("X2") + "/"
                                                          + RxData.DataBuf[26].ToString("X2") + "/"
                                                          + RxData.DataBuf[27].ToString("X2"), Color.Green);
                        rtbCommMsgAdd("\n程序版本:" + RxData.DataBuf[29].ToString("X") + "." + RxData.DataBuf[28].ToString("X2") + "\n", Color.Green);
                    }
                    break;
                case 0x03:                  // 查询主节点地址
                    if ((RxData.CtrlWord & 0x80) == 0x00)   // 下行报文
                    {
                        rtbCommMsgAdd(" 无数据域\n", Color.Green);
                    }
                    else                                    // 上行报文
                    {
                        rtbCommMsgAdd("\n主节点地址:" + Util.GetStringHexFromBytes(RxData.DataBuf, 0, ProtoLocal_South.LongAddrSize, "", true) + "\n", Color.Green);
                    }
                    break;
                case 0x04:                  // 查询通信延时时长
                    if ((RxData.CtrlWord & 0x80) == 0x00)   // 下行报文
                    {
                        rtbCommMsgAdd("\n通信目的地址：" + Util.GetStringHexFromBytes(RxData.DataBuf, 0, ProtoLocal_South.LongAddrSize, "", true), Color.Green);
                        rtbCommMsgAdd("\n报文长度：" + RxData.DataBuf[6] + "\n", Color.Green);
                    }
                    else                                    // 上行报文
                    {
                        rtbCommMsgAdd("\n通信目的地址：" + Util.GetStringHexFromBytes(RxData.DataBuf, 0, ProtoLocal_South.LongAddrSize, "", true), Color.Green);
                        rtbCommMsgAdd("\n通信延时时长：" + (RxData.DataBuf[6] + RxData.DataBuf[7]*256) + "s", Color.Green);
                        rtbCommMsgAdd("\n报文长度：" + RxData.DataBuf[8] + "\n", Color.Green);
                    }
                    break;
                case 0x05:                  // 查询从节点数量
                    if ((RxData.CtrlWord & 0x80) == 0x00)   // 下行报文
                    {
                        rtbCommMsgAdd(" 无数据域\n", Color.Green);
                    }
                    else                                    // 上行报文
                    {
                        rtbCommMsgAdd("\n从节点数量:" + (RxData.DataBuf[0] + RxData.DataBuf[1] * 256).ToString("D") + "\n", Color.Green);
                    }
                    break;
                case 0x06:                  // 查询从节点信息
                    if ((RxData.CtrlWord & 0x80) == 0x00)   // 下行报文
                    {
                        rtbCommMsgAdd("\n起始序号:" + (RxData.DataBuf[0] + RxData.DataBuf[1] * 256).ToString("D"), Color.Green);
                        rtbCommMsgAdd("  读取数量：" + RxData.DataBuf[2].ToString("D") + "\n", Color.Green);
                    }
                    else                                    // 上行报文
                    {
                        int len = 0;
                        rtbCommMsgAdd("\n从节点总数量:" + (RxData.DataBuf[len++] + RxData.DataBuf[len++] * 256).ToString("D"), Color.Green);
                        rtbCommMsgAdd("    本次应答数量:" + RxData.DataBuf[len++].ToString("D") + "\n", Color.Green);
                        for (int iLoop = 0; iLoop < RxData.DataBuf[2]; iLoop++)
                        {
                            if (iLoop > 0)
                            {
                                rtbCommMsgAdd((iLoop % 5 == 0 ? "\n" : "\t"), Color.Green);
                            }
                            rtbCommMsgAdd(Util.GetStringHexFromBytes(RxData.DataBuf, len, ProtoLocal_South.LongAddrSize, "", true), Color.Green);
                            len += ProtoLocal_South.LongAddrSize;
                        }
                        if (RxData.DataBuf[2] > 0)
                        {
                            rtbCommMsgAdd("\n", Color.Green);
                        }
                    }
                    break;
                case 0x07:                  // 查询从节点主动注册进度
                    if ((RxData.CtrlWord & 0x80) == 0x00)   // 下行报文
                    {
                        rtbCommMsgAdd(" 无数据域\n", Color.Green);
                    }
                    else                                    // 上行报文
                    {
                        string strFlag = RxData.DataBuf[0] == 0x00 ? "(0)停止主动注册" : "(1)正在主动注册";
                        rtbCommMsgAdd("\n从节点主动注册标识:" + strFlag + "\n", Color.Green);
                    }
                    break;
                case 0x08:                  // 查询从节点的父节点
                    if ((RxData.CtrlWord & 0x80) == 0x00)   // 下行报文
                    {
                        rtbCommMsgAdd("\n从节点地址：" + Util.GetStringHexFromBytes(RxData.DataBuf, 0, ProtoLocal_South.LongAddrSize, "", true) + "\n", Color.Green);
                    }
                    else                                    // 上行报文
                    {
                        rtbCommMsgAdd("\n从节点地址：" + Util.GetStringHexFromBytes(RxData.DataBuf, 0, ProtoLocal_South.LongAddrSize, "", true) , Color.Green);
                        rtbCommMsgAdd("  父节点地址：" + Util.GetStringHexFromBytes(RxData.DataBuf, 6, ProtoLocal_South.LongAddrSize, "", true), Color.Green);
                        rtbCommMsgAdd("  链路质量：" + RxData.DataBuf[12]+ "\n", Color.Green);
                    }
                    break;
                default:
                    break;
            }
        }
        private void Explain_Afn4WriteParams(ProtoLocal_South.PacketFormat RxData)
        {
            int iLoop;

            switch (RxData.DataId[0])
            {
                case 0x01:                  // 设置主节点地址
                    if ((RxData.CtrlWord & 0x80) == 0x00)   // 下行报文
                    {
                        rtbCommMsgAdd("\n新主节点地址：" + Util.GetStringHexFromBytes(RxData.DataBuf, 0, ProtoLocal_South.LongAddrSize, "", true) + "\n", Color.Green);
                    }
                    break;
                case 0x02:                  // 添加从节点
                    if ((RxData.CtrlWord & 0x80) == 0x00)   // 下行报文
                    {
                        int index = 0;
                        rtbCommMsgAdd("\n本次添加数量：" + RxData.DataBuf[index++] + "\n", Color.Green);
                        for (iLoop = 0; iLoop < RxData.DataBuf[0]; iLoop++)
                        {
                            if (iLoop > 0)
                            {
                                rtbCommMsgAdd((iLoop % 5 == 0 ? "\n" : "\t"), Color.Green);
                            }
                            rtbCommMsgAdd(Util.GetStringHexFromBytes(RxData.DataBuf, index, ProtoLocal_South.LongAddrSize, "", true), Color.Green);
                            index += ProtoLocal_South.LongAddrSize;
                        }
                        rtbCommMsgAdd("\n", Color.Green);
                    }
                    break;
                case 0x03:                  // 删除从节点
                    if ((RxData.CtrlWord & 0x80) == 0x00)   // 下行报文
                    {
                        int index = 0;
                        rtbCommMsgAdd("\n本次删除数量：" + RxData.DataBuf[index++] + "\n", Color.Green);
                        for (iLoop = 0; iLoop < RxData.DataBuf[0]; iLoop++)
                        {
                            if (iLoop > 0)
                            {
                                rtbCommMsgAdd((iLoop % 5 == 0 ? "\n" : "\t"), Color.Green);
                            }
                            rtbCommMsgAdd(Util.GetStringHexFromBytes(RxData.DataBuf, index, ProtoLocal_South.LongAddrSize, "", true), Color.Green);
                            index += ProtoLocal_South.LongAddrSize;
                        }
                        rtbCommMsgAdd("\n", Color.Green);
                    }
                    break;
                default:                  // 允许_禁止上报从节点事件
                    if ((RxData.CtrlWord & 0x80) == 0x00)   // 下行报文
                    {
                        rtbCommMsgAdd("  无数据域\n", Color.Green);
                    }
                    break;
            }
        }
        private void Explain_Afn5ReportData(ProtoLocal_South.PacketFormat RxData)
        {
            switch (RxData.DataId[0])
            {
                case 0x01:                  // 上报任务数据
                    if ((RxData.CtrlWord & 0x80) == 0x80)   // 上行报文
                    {
                        rtbCommMsgAdd("\n任务ID：" + RxData.DataBuf[1].ToString("X2") + RxData.DataBuf[0].ToString("X2") + "\n", Color.Green);
                        rtbCommMsgAdd("报文长度：" + RxData.DataBuf[2] + "\n", Color.Green);
                        rtbCommMsgAdd("报文内容：" + Util.GetStringHexFromBytes(RxData.DataBuf, 3, RxData.DataBuf[2], " ") + "\n", Color.Green);
                    }
                    break;

                case 0x02:                  // 上报从节点事件
                    if ((RxData.CtrlWord & 0x80) == 0x80)   // 上行报文
                    {
                        rtbCommMsgAdd("\n报文长度：" + RxData.DataBuf[0] + "\n", Color.Green);
                        rtbCommMsgAdd("报文内容：" + Util.GetStringHexFromBytes(RxData.DataBuf, 1, RxData.DataBuf[0], " ") + "\n", Color.Green);
                    }
                    break;

                case 0x03:                  // 上报从节点信息
                    if ((RxData.CtrlWord & 0x80) == 0x80)   // 上行报文
                    {
                        int index = 0;
                        rtbCommMsgAdd("\n本次上报从节点数量：" + RxData.DataBuf[index++] + "\n", Color.Green);
                        for (int iLoop = 0; iLoop < RxData.DataBuf[0]; iLoop++)
                        {
                            if (iLoop > 0)
                            {
                                rtbCommMsgAdd((iLoop % 5 == 0 ? "\n" : "\t"), Color.Green);
                            }
                            rtbCommMsgAdd(Util.GetStringHexFromBytes(RxData.DataBuf, index, ProtoLocal_South.LongAddrSize, "", true), Color.Green);
                            index += ProtoLocal_South.LongAddrSize;
                        }
                        rtbCommMsgAdd("\n", Color.Green);
                    }
                    break;

                case 0x04:                  // 上报从节点主动注册结束
                    if ((RxData.CtrlWord & 0x80) == 0x80)   // 上行报文
                    {
                        rtbCommMsgAdd("  无数据域\n", Color.Green);
                    }
                    break;

                case 0x05:                  // 上报任务状态
                    if ((RxData.CtrlWord & 0x80) == 0x80)   // 上行报文
                    {
                        string strTskStatus = "";
                        rtbCommMsgAdd("\n任务ID：" + RxData.DataBuf[1].ToString("X2") + RxData.DataBuf[0].ToString("X2") + "\n", Color.Green);
                        rtbCommMsgAdd("从节点地址：" + Util.GetStringHexFromBytes(RxData.DataBuf, 2, ProtoLocal_South.LongAddrSize, "", true) + "\n", Color.Green);
                        rtbCommMsgAdd("任务状态：", Color.Green);
                        switch (RxData.DataBuf[8])
                        {
                            case 0x00:
                                strTskStatus = "(0) - 成功";
                                break;

                            case 0x01:
                                strTskStatus = "(1) - 从节点无响应";
                                break;

                            case 0x02:
                                strTskStatus = "(2) - 数据不合法";
                                break;

                            case 0xFF:
                            default:
                                strTskStatus = "其他错误";
                                break;
                        }
                        rtbCommMsgAdd(strTskStatus + "\n", Color.Green);
                    }
                    break; 
                default:
                    break;
            }
        }
        private void Explain_Afn6RequestInfo(ProtoLocal_South.PacketFormat RxData)
        {
            string strMsg = "";

            switch (RxData.DataId[0])
            {
                case 0x01:                  // 请求集中器时间
                    if ((RxData.CtrlWord & 0x80) == 0x80)   // 上行报文
                    {
                        rtbCommMsgAdd("  无数据域\n", Color.Green);
                    }
                    else                                    // 下行报文
                    {
                        strMsg = "\n集中器应答时间:";
                        strMsg += RxData.DataBuf[5].ToString("X2") + "/" + RxData.DataBuf[4].ToString("X2") + "/" + RxData.DataBuf[3].ToString("X2") + " ";
                        strMsg += RxData.DataBuf[2].ToString("X2") + ":" + RxData.DataBuf[1].ToString("X2") + ":" + RxData.DataBuf[0].ToString("X2") + "\n";
                        rtbCommMsgAdd(strMsg, Color.Green);
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region 通讯信息显示
        private void AddToCommMsg(bool Dir, byte[] Buf, ProtoLocal_South.PacketFormat rxData)
        {
            string strInfo = "";

            // 时间+收发数据
            strInfo = "\n-----------------------------------------------------------------------------------\n";
            strInfo += DateTime.Now.ToString("【yy/MM/dd HH:mm:ss.fff】");
            strInfo += "   " + GetStringExplain(rxData) + "\n";
            rtbCommMsgAdd(strInfo, (true == Dir) ? Color.Red : Color.Blue);
            rtbCommMsgAdd(Util.GetStringHexFromBytes(Buf, 0, Buf.Length, " "), (true == Dir) ? Color.Red : Color.Blue);
            rtbCommMsg.AppendText("\n");
            // AFN
            rtbCommMsgAdd("AFN:" + ((byte)(rxData.Afn)).ToString("X2"), Color.Indigo);
            // 序号
            rtbCommMsgAdd("  序号:" + rxData.SerialNo.ToString("X2"), Color.Indigo);
            // 上/下行
            strInfo = "  方向:";
            strInfo += (rxData.CtrlWord & 0x80) == 0x80 ? "上行" : "下行";
            strInfo += (rxData.CtrlWord & 0x40) == 0x40 ? "(启动站)" : "(从动站)";
            rtbCommMsgAdd(strInfo, Color.Indigo);
            // 地址域
            if ((rxData.CtrlWord & 0x20) == 0x20)
            {
                rtbCommMsgAdd("  源地址:", Color.Indigo);
                rtbCommMsgAdd(Util.GetStringHexFromBytes(rxData.SrcAddr, 0, ProtoLocal_South.LongAddrSize, "", true), Color.Indigo);
                rtbCommMsgAdd("  目的地址:", Color.Indigo);
                rtbCommMsgAdd(Util.GetStringHexFromBytes(rxData.DstAddr, 0, ProtoLocal_South.LongAddrSize, "", true), Color.Indigo);
            }
            else
            {
                rtbCommMsgAdd("  无地址域", Color.Indigo);
            }
        }
        private void rtbCommMsgAdd(string strInfo, Color colFore)
        {
            if (rtbCommMsg.Text.Length > rtbCommMsg.MaxLength - 100)
            {
                rtbCommMsg.Clear();
            }
            int iStart = rtbCommMsg.Text.Length;
            rtbCommMsg.AppendText(strInfo);
            rtbCommMsg.Select(iStart, rtbCommMsg.Text.Length);
            rtbCommMsg.SelectionColor = colFore;
            if (tsmiAutoScrollCommMsg.Checked == true)
            {
                rtbCommMsg.ScrollToCaret();
            }
            pgbCapactity.Minimum = 0;
            pgbCapactity.Maximum = 500;
            pgbCapactity.Value = (rtbCommMsg.Text.Length * pgbCapactity.Maximum) / rtbCommMsg.MaxLength;
        }
        private void tsmiClearAllCommMsg_Click(object sender, EventArgs e)
        {
            rtbCommMsg.Clear();
        }
        private void cnMenuComm_Opening(object sender, CancelEventArgs e)
        {
            tsmiAutoScrollCommMsg.Enabled = true;
            if (rtbCommMsg.Text.Length == 0)
            {
                tsmiClearAllCommMsg.Enabled = false;
                tsmiSaveCommMsgToFile.Enabled = false;
            }
            else
            {
                tsmiClearAllCommMsg.Enabled = true;
                tsmiSaveCommMsgToFile.Enabled = true;
            }
        }
        private void tsmiAutoScrollCommMsg_Click(object sender, EventArgs e)
        {
            XmlHelper.SetNodeValue(FrmMain.SystemConfigPath, "/Config", "ConcSimulator_CommonMsgAutoScroll", (tsmiAutoScrollCommMsg.Checked == true ? "1" : "0"));
        }
        private void tsmiSaveCommMsgToFile_Click(object sender, EventArgs e)
        {
            string strDirectory = "";
            string strFileName;

            if (rtbCommMsg.Text.Length == 0)
            {
                MessageBox.Show("没有通讯数据可以保存！", "信息", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Information);
                return;
            }
            strDirectory = XmlHelper.GetNodeDefValue(FrmMain.SystemConfigPath, "/Config/ConcSimulator_CommMsgPath", System.Windows.Forms.Application.StartupPath);
            saveFileDlg.Filter = "*.txt(文本文件)|*.txt";
            saveFileDlg.DefaultExt = "txt";
            saveFileDlg.FileName = "通讯记录";
            if (saveFileDlg.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            strFileName = saveFileDlg.FileName;
            if (strFileName.Length == 0)
            {
                return;
            }

            if (strDirectory != Path.GetDirectoryName(strFileName))
            {
                strDirectory = Path.GetDirectoryName(strFileName);
                XmlHelper.SetNodeValue(FrmMain.SystemConfigPath, "/Config", "ConcSimulator_CommMsgPath", strDirectory);
            }
            try
            {
                StreamWriter sw = new StreamWriter(strFileName, true, System.Text.Encoding.UTF8);
                sw.WriteLine("\n******以下记录保存时间是" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "******");
                string strTemp = rtbCommMsg.Text.Replace("\n", "\r\n");
                sw.Write(strTemp);
                sw.Close();
            }
            catch (SystemException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private string GetStringExplain(ProtoLocal_South.PacketFormat RxData)
        {
            string strInfo = "";

            switch ((ProtoLocal_South.Afn)(RxData.Afn))
            {
                case ProtoLocal_South.Afn.Afn0_Ack:
                    switch (RxData.DataId[0])
                    {
                        case 0x01: return "确认应答";
                        case 0x02: return "否认应答";
                        default: return "未定义";
                    }

                case ProtoLocal_South.Afn.Afn1_Initial:
                    switch (RxData.DataId[0])
                    {
                        case 0x01: return "复位硬件";
                        case 0x02: return "初始化档案";
                        case 0x03: return "初始化任务";
                        default: return "未定义";
                    }
                
                case ProtoLocal_South.Afn.Afn2_TaskManage:
                    switch (RxData.DataId[0])
                    {
                        case 0x01: return "添加任务";
                        case 0x02: return "删除任务";
                        case 0x03: return "查询未完成任务";
                        case 0x04: return "查询未完成任务列表";
                        case 0x05:
                            strInfo = (RxData.DataId[2] == 0x03) ? "查询未完成任务详细信息" : "返回查询未完成任务详细信息";
                            return strInfo;
                        case 0x06: return "查询剩余可分配任务数";
                        case 0x07: return "添加多播任务";
                        case 0x08: return "启动任务";
                        case 0x09: return "暂停任务";
                        default: return "未定义";
                    }

                case ProtoLocal_South.Afn.Afn3_ReadParams:
                    switch (RxData.DataId[0])
                    {
                        case 0x01: return "查询厂商代码和版本信息";
                        case 0x02: return "查询本地通信模块运行模式信息";
                        case 0x03: return "查询主节点地址";
                        case 0x04:
                            strInfo = (RxData.DataId[2] == 0x03) ? "查询通信延时时长" : "返回查询通信延时时长";
                            return strInfo;
                        case 0x05: return "查询从节点数量";
                        case 0x06:
                            strInfo = (RxData.DataId[2] == 0x03) ? "查询从节点信息" : "返回查询从节点信息";
                            return strInfo;
                        case 0x07: return "查询从节点主动注册进度";
                        case 0x08:
                            strInfo = (RxData.DataId[2] == 0x03) ? "查询从节点的父节点" : "返回查询从节点的父节点";
                            return strInfo;
                        default:   return "未定义";
                    }

                case ProtoLocal_South.Afn.Afn4_WriteParams:
                    switch (RxData.DataId[0])
                    {
                        case 0x01: return "设置主节点";
                        case 0x02: return "添加从节点";
                        case 0x03: return "删除从节点";
                        case 0x04: return "允许/禁止上报从节点事件";
                        case 0x05: return "激活从节点主动注册";
                        case 0x06: return "终止从节点主动注册";
                        default: return "未定义";
                    }

                case ProtoLocal_South.Afn.Afn5_ReportData:
                    switch (RxData.DataId[0])
                    {
                        case 0x01: return "上报任务数据";
                        case 0x02: return "上报从节点事件";
                        case 0x03: return "上报从节点信息";
                        case 0x04: return "上报从节点注册结束";
                        case 0x05: return "上报任务状态";
                        default: return "未定义";
                    }

                case ProtoLocal_South.Afn.Afn6_RequestInfo:
                    switch (RxData.DataId[0])
                    {
                        case 0x01: return "请求集中器时间";
                        default: return "未定义";
                    }

                case ProtoLocal_South.Afn.Afn7_FileTransfer:
                    switch (RxData.DataId[0])
                    {
                        case 0x01: return "启动文件传输";
                        case 0x02: return "传输文件内容";
                        case 0x03: return "查询文件信息";
                        case 0x04:
                            if (RxData.DataId[2] == 0x00)
                            {
                                return "查询文件处理进度";
                            }
                            else if (RxData.DataId[2] == 0x03)
                            {
                                return "查询文件传输失败节点";
                            }
                            else
                            {
                                return "返回查询文件传输失败节点";
                            }
                        default: return "未定义";
                    }

                default:
                    return "未定义";
            }
        }
        #endregion

        #region 复位硬件
        private void btResetDevice_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            if (DialogResult.Cancel == MessageBox.Show("确定要重新启动集中器模块吗？", "确认信息", MessageBoxButtons.OKCancel, MessageBoxIcon.Question))
            {
                return;
            }
            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.复位硬件;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            btResetDevice.Enabled = false;
        }
        private void TransmitResetDevice()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btResetDevice.Enabled = true;
                MessageBox.Show("重新启动集中器模块失败,模块无应答", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn1_Initial;
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x01, 0x01, 0x02, 0xE8 };
            txData.DataBuf = null;
            DataTransmit(txData);
            CmdWaitTime = 150;
            CmdRetryTimes--;
        }
        private void ReceiveResetDevice(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn != ProtoLocal_South.Afn.Afn0_Ack || RxData.SerialNo != CmdSn)
            {
                return;
            }
            bTransceiveEnable = false;
            Cmd = Command.任务空闲;
            btResetDevice.Enabled = true;
        }
        #endregion

        #region 初始化档案
        private void btInitialDocument_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            if (DialogResult.Cancel == MessageBox.Show("确定要清除模块中的档案信息吗？", "确认信息", MessageBoxButtons.OKCancel, MessageBoxIcon.Question))
            {
                return;
            }
            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.初始化档案;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            btInitialDocument.Enabled = false;
        }
        private void TransmitInitialDocument()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btInitialDocument.Enabled = true;
                MessageBox.Show("初始化档案失败,模块无应答", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn1_Initial;
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x02, 0x01, 0x02, 0xE8 };
            txData.DataBuf = null;
            DataTransmit(txData);
            CmdWaitTime = 150;
            CmdRetryTimes--;
        }
        private void ReceiveInitialDocument(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn != ProtoLocal_South.Afn.Afn0_Ack || RxData.SerialNo != CmdSn)
            {
                return;
            }
            bTransceiveEnable = false;
            Cmd = Command.任务空闲;
            btInitialDocument.Enabled = true;
        }
        #endregion

        #region 初始化任务
        private void btInitialTask_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            if (DialogResult.Cancel == MessageBox.Show("确定要清除模块中的任务信息吗？", "确认信息", MessageBoxButtons.OKCancel, MessageBoxIcon.Question))
            {
                return;
            }
            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.初始化任务;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            btInitialTask.Enabled = false;
        }
        private void TransmitInitialTask()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btInitialTask.Enabled = true;
                MessageBox.Show("初始化任务失败,模块无应答", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn1_Initial;
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x03, 0x01, 0x02, 0xE8 };
            txData.DataBuf = null;
            DataTransmit(txData);
            CmdWaitTime = 150;
            CmdRetryTimes--;
        }
        private void ReceiveInitialTask(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn != ProtoLocal_South.Afn.Afn0_Ack || RxData.SerialNo != CmdSn)
            {
                return;
            }
            bTransceiveEnable = false;
            Cmd = Command.任务空闲;
            btInitialTask.Enabled = true;
        }
#endregion

        #region 查询未完成任务数
        private void btReadUndoTaskNum_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.查询未完成任务数;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            btReadUndoTaskNum.Enabled = false;
        }
        private void TransmitReadUndoTaskNum()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btReadUndoTaskNum.Enabled = true;
                MessageBox.Show("查询未完成任务数失败,模块无应答或应答错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn2_TaskManage;
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x03, 0x02, 0x00, 0xE8 };
            txData.DataBuf = null;
            DataTransmit(txData);
            CmdWaitTime = 150;
            CmdRetryTimes--;
        }
        private void ReceiveReadUndoTaskNum(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn != ProtoLocal_South.Afn.Afn2_TaskManage || RxData.DataId[0] != 0x03 || RxData.SerialNo != CmdSn)
            {
                return;
            }
            bTransceiveEnable = false;
            Cmd = Command.任务空闲;
            btReadUndoTaskNum.Enabled = true;
        }
        #endregion

        #region 查询剩余可分配任务数
        private void btReadRemainTask_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.查询剩余可分配任务数;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            btReadRemainTask.Enabled = false;
        }
        private void TransmitReadRemainTask()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btReadRemainTask.Enabled = true;
                MessageBox.Show("查询剩余可分配任务数失败,模块无应答或应答错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn2_TaskManage;
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x06, 0x02, 0x00, 0xE8 };
            txData.DataBuf = null;
            DataTransmit(txData);
            CmdWaitTime = 150;
            CmdRetryTimes--;
        }
        private void ReceiveReadRemainTask(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn != ProtoLocal_South.Afn.Afn2_TaskManage || RxData.DataId[0] != 0x06 || RxData.SerialNo != CmdSn)
            {
                return;
            }
            bTransceiveEnable = false;
            Cmd = Command.任务空闲;
            btReadRemainTask.Enabled = true;
        }
        #endregion

        #region 查询未完成任务列表
        int currentReadUndoTaskTab = 0;
        private void btReadUndoTaskTab_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.查询未完成任务列表;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            btReadUndoTaskTab.Enabled = false;
            currentReadUndoTaskTab = 0;
        }
        private void TransmitReadUndoTaskTab()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btReadUndoTaskTab.Enabled = true;
                MessageBox.Show("查询未完成任务列表失败,模块无应答或应答错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn2_TaskManage;
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x04, 0x02, 0x03, 0xE8 };
            txData.DataBuf = new byte[3];
            txData.DataBuf[0] = (byte)currentReadUndoTaskTab;
            txData.DataBuf[1] = (byte)(currentReadUndoTaskTab >> 8);
            txData.DataBuf[2] = 20;
            DataTransmit(txData);
            CmdWaitTime = 200;
            CmdRetryTimes--;
        }
        private void ReceiveReadUndoTaskTab(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn == ProtoLocal_South.Afn.Afn2_TaskManage && RxData.DataId[0] == 0x04 && RxData.SerialNo == CmdSn)
            {
                int iPos = 0;
                int count = RxData.DataBuf[iPos++] + RxData.DataBuf[iPos++] * 256;
                if (currentReadUndoTaskTab == 0)
                {
                    Array.Copy(RxData.DataBuf, 2, UndoTaskTab, 0, count * 2);
                    UndoTaskTabCount = count * 2;
                }
                else
                {
                    Array.Copy(RxData.DataBuf, 2, UndoTaskTab, UndoTaskTabCount, count * 2);
                    UndoTaskTabCount += count * 2;
                }
                currentReadUndoTaskTab += count;
                if (count < 20)
                {
                    bTransceiveEnable = false;
                    Cmd = Command.任务空闲;
                    btReadUndoTaskTab.Enabled = true;
                }
                else
                {
                    CmdSn++;
                    CmdWaitTime = 2;
                    CmdRetryTimes = 3;
                }
            }
        }
        #endregion    

        #region 查询未完成任务详细信息
        byte[] CurrentReadTaskId = new byte[2];
        SelectTaskIdDlg SelTaskIdDlg = null;
        private void btReadUndoTaskInfo_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            if (SelTaskIdDlg == null || SelTaskIdDlg.IsDisposed)
            {
                SelTaskIdDlg = new SelectTaskIdDlg(UndoTaskTab, UndoTaskTabCount);
                SelTaskIdDlg.SelectTaskIdProcess = ReadUndoTaskInfo;
            }
            SelTaskIdDlg.Show();
            SelTaskIdDlg.Focus();
        }
        private void ReadUndoTaskInfo(string strTaskId)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.查询未完成任务详细信息;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            btReadUndoTaskInfo.Enabled = false;
            Util.GetBytesFromStringHex(strTaskId, CurrentReadTaskId, 0, true);
        }
        private void TransmitReadUndoTaskInfo()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btReadUndoTaskInfo.Enabled = true;
                MessageBox.Show("查询节点任务信息时失败,模块无应答或应答错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn2_TaskManage;
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x05, 0x02, 0x03, 0xE8 };
            txData.DataBuf = new byte[2];
            Array.Copy(CurrentReadTaskId, txData.DataBuf, txData.DataBuf.Length);
            DataTransmit(txData);
            CmdWaitTime = 200;
            CmdRetryTimes--;
        }
        private void ReceiveReadUndoTaskInfo(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn == ProtoLocal_South.Afn.Afn2_TaskManage && RxData.DataId[0] == 0x05 && RxData.SerialNo == CmdSn &&
                RxData.DataBuf[0] == CurrentReadTaskId[0] && RxData.DataBuf[1] == CurrentReadTaskId[1])
            {
                byte taskLevel = (byte)(RxData.DataBuf[2] & 0x0F);
                int nodeCount = RxData.DataBuf[3] + RxData.DataBuf[4] * 256;
                for (int iLoop = 0; iLoop < nodeCount; iLoop++)
                {
                    string strSubNodeAddr = Util.GetStringHexFromBytes(RxData.DataBuf, 5 + iLoop * ProtoLocal_South.LongAddrSize, ProtoLocal_South.LongAddrSize, "", true);
                    for (int iRow = 0; iRow < tbDocument.Rows.Count; iRow++)
                    {
                        if (tbDocument.Rows[iRow]["表具地址"].ToString() == strSubNodeAddr)
                        {
                            tbDocument.Rows[iRow]["任务ID"] = (RxData.DataBuf[0] + RxData.DataBuf[1] * 256).ToString("X4");
                            tbDocument.Rows[iRow]["优先级"] = taskLevel.ToString("D") + "级";
                            tbDocument.Rows[iRow]["状态"] = "未完成";
                            tbDocument.Rows[iRow]["结果"] = "未知";
                            break;
                        }
                    }
                }
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btReadUndoTaskInfo.Enabled = true;
                return;
            }
        }
        #endregion

        #region 启动任务
        private void btStartTask_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.启动任务;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            btStartTask.Enabled = false;
        }
        private void TransmitStartTask()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btStartTask.Enabled = true;
                MessageBox.Show("启动任务失败,模块无应答或应答错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn2_TaskManage;
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x08, 0x02, 0x02, 0xE8 };
            txData.DataBuf = null;
            DataTransmit(txData);
            CmdWaitTime = 150;
            CmdRetryTimes--;
        }
        private void ReceiveStartTask(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn != ProtoLocal_South.Afn.Afn0_Ack || RxData.DataId[0] != 0x01 || RxData.SerialNo != CmdSn)
            {
                return;
            }
            bTransceiveEnable = false;
            Cmd = Command.任务空闲;
            btStartTask.Enabled = true;
        }
        #endregion

        #region 暂停任务
        private void btPauseTask_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.暂停任务;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            btPauseTask.Enabled = false;
        }
        private void TransmitPauseTask()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btPauseTask.Enabled = true;
                MessageBox.Show("暂停任务失败,模块无应答或应答错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn2_TaskManage;
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x09, 0x02, 0x02, 0xE8 };
            txData.DataBuf = null;
            DataTransmit(txData);
            CmdWaitTime = 150;
            CmdRetryTimes--;
        }
        private void ReceivePauseTask(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn != ProtoLocal_South.Afn.Afn0_Ack || RxData.DataId[0] != 0x01 || RxData.SerialNo != CmdSn)
            {
                return;
            }
            bTransceiveEnable = false;
            Cmd = Command.任务空闲;
            btPauseTask.Enabled = true;
        }
        #endregion

        #region 查询厂商代码和版本信息
        private void btVersionInfo_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.查询厂商代码和版本信息;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            btVersionInfo.Enabled = false;
        }
        private void TransmitVersionInfo()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btVersionInfo.Enabled = true;
                MessageBox.Show("查询厂商代码和版本信息失败,模块无应答", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn3_ReadParams;
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x01, 0x03, 0x00, 0xE8 };
            txData.DataBuf = null;
            DataTransmit(txData);
            CmdWaitTime = 150;
            CmdRetryTimes--;
        }
        private void ReceiveVersionInfo(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn != ProtoLocal_South.Afn.Afn3_ReadParams || RxData.DataId[0] != 0x01 || RxData.SerialNo != CmdSn)
            {
                return;
            }
            bTransceiveEnable = false;
            Cmd = Command.任务空闲;
            btVersionInfo.Enabled = true;
        }
        #endregion

        #region 查询本地通信模块运行模式信息
        private void btModuleWorkMode_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.查询本地通信模块运行模式信息;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            btModuleWorkMode.Enabled = false;
        }
        private void TransmitModuleWorkMode()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btModuleWorkMode.Enabled = true;
                MessageBox.Show("查询本地通信模块运行模式信息失败,模块无应答", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn3_ReadParams;
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x02, 0x03, 0x00, 0xE8 };
            txData.DataBuf = null;
            DataTransmit(txData);
            CmdWaitTime = 150;
            CmdRetryTimes--;
        }
        private void ReceiveModuleWorkMode(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn != ProtoLocal_South.Afn.Afn3_ReadParams || RxData.DataId[0] != 0x02 || RxData.SerialNo != CmdSn)
            {
                return;
            }
            bTransceiveEnable = false;
            Cmd = Command.任务空闲;
            btModuleWorkMode.Enabled = true;
        }
        #endregion

        #region 查询主节点地址
        private void btReadAddress_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.查询主节点地址;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            btReadAddress.Enabled = false;
        }
        private void TransmitReadAddress()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btReadAddress.Enabled = true;
                MessageBox.Show("查询主节点地址失败,模块无应答或应答错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn3_ReadParams;
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x03, 0x03, 0x00, 0xE8 };
            txData.DataBuf = null;
            DataTransmit(txData);
            CmdWaitTime = 150;
            CmdRetryTimes--;
        }
        private void ReceiveReadAddress(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn == ProtoLocal_South.Afn.Afn3_ReadParams && RxData.DataId[0] == 0x03 && RxData.SerialNo == CmdSn)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btReadAddress.Enabled = true;
                lbConcAddr.Text = "主节点地址:" + Util.GetStringHexFromBytes(RxData.DataBuf, 0, ProtoLocal_South.LongAddrSize, "", true);
                return;
            }
        }
        #endregion

        #region 查询通信延时时长
        InputTwoParamDlg InputComAddrAndLenDlg = null;
        private void btReadComDelayTime_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }

            if (InputComAddrAndLenDlg == null || InputComAddrAndLenDlg.IsDisposed)
            {
                InputComAddrAndLenDlg = new InputTwoParamDlg("通信目的地址", "报文长度");
                InputComAddrAndLenDlg.ParamsOutputProc = SetComAddrAndLength;
            }
            InputComAddrAndLenDlg.Show();
            InputComAddrAndLenDlg.Focus();
        }

        private void SetComAddrAndLength(String strAddr, String strLength)
        {
            CmdParams = new byte[ProtoLocal_South.LongAddrSize + 1];
            Util.GetBytesFromStringHex(strAddr, CmdParams, 0, true);
            try
            {
                CmdParams[6] = Convert.ToByte(strLength);
            }
            catch(Exception ex)
            {
                MessageBox.Show("报文长度超出范围（0~255）：" + ex.Message );
                return;
            }

            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.查询通信延时时长;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            btReadComDelayTime.Enabled = false;
        }
        private void TransmitReadComDelayTime()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btReadComDelayTime.Enabled = true;
                MessageBox.Show("查询通信延时时长失败,模块无应答或应答错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn3_ReadParams;
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x04, 0x03, 0x03, 0xE8 };
            txData.DataBuf = CmdParams;
            DataTransmit(txData);
            CmdWaitTime = 150;
            CmdRetryTimes--;
        }
        private void ReceiveReadComDelayTime(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn == ProtoLocal_South.Afn.Afn3_ReadParams && RxData.DataId[0] == 0x04 && RxData.SerialNo == CmdSn)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btReadComDelayTime.Enabled = true;
            }
        }
        #endregion

        #region 查询从节点数量
        private void btReadSubnodeCount_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.查询从节点数量;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            btReadSubnodeCount.Enabled = false;
        }
        private void TransmitReadSubnodeCount()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btReadSubnodeCount.Enabled = true;
                MessageBox.Show("查询从节点数量失败,模块无应答或应答错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn3_ReadParams;
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x05, 0x03, 0x00, 0xE8 };
            txData.DataBuf = null;
            DataTransmit(txData);
            CmdWaitTime = 150;
            CmdRetryTimes--;
        }
        private void ReceiveReadSubnodeCount(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn == ProtoLocal_South.Afn.Afn3_ReadParams && RxData.DataId[0] == 0x05 && RxData.SerialNo == CmdSn)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btReadSubnodeCount.Enabled = true;
                if (tbDocument.Rows.Count != RxData.DataBuf[0] + RxData.DataBuf[1] * 256)
                {
                    MessageBox.Show("主节点中的节点数量和档案列表中的节点数量不同！", "特别注意");
                }
            }
        }
        #endregion

        #region 查询从节点信息
        InputTwoParamDlg InputStartIndexAndCountDlg = null;
        private void btReadSubnodeInfo_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }

            if (InputStartIndexAndCountDlg == null || InputStartIndexAndCountDlg.IsDisposed)
            {
                InputStartIndexAndCountDlg = new InputTwoParamDlg("从节点起始序号", "从节点数量");
                InputStartIndexAndCountDlg.ParamsOutputProc = SetStartIndexAndCount;
            }
            InputStartIndexAndCountDlg.Show();
            InputStartIndexAndCountDlg.Focus();
        }

        private void SetStartIndexAndCount(String strIndex, String strCount)
        {
            CmdParams = new byte[3];
            CmdParams[0] = (byte)(Convert.ToUInt16(strIndex) & 0x00FF);
            CmdParams[1] = (byte)(Convert.ToUInt16(strIndex) >> 8);
            try
            {
                CmdParams[2] = Convert.ToByte(strCount);
                if(CmdParams[2] == 0)
                {
                    throw(new Exception("0"));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("从节点数量超出范围（1~255）：" + ex.Message );
                return;
            }

            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.查询从节点信息;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            btReadSubnodeInfo.Enabled = false;
        }
        private void TransmitReadSubnodeInfo()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btReadSubnodeInfo.Enabled = true;
                MessageBox.Show("查询从节点信息失败,模块无应答或应答错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn3_ReadParams;
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x06, 0x03, 0x03, 0xE8 };
            txData.DataBuf = CmdParams;
            DataTransmit(txData);
            CmdWaitTime = 150;
            CmdRetryTimes--;
        }
        private void ReceiveReadSubnodeInfo(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn == ProtoLocal_South.Afn.Afn3_ReadParams && RxData.DataId[0] == 0x06 && RxData.SerialNo == CmdSn)
            {
                int subNodeCount = RxData.DataBuf[0] + RxData.DataBuf[1] * 256;
                byte readCount = RxData.DataBuf[2];

                int curStartIndex = CmdParams[0] + CmdParams[1] * 256 + readCount;
                if (curStartIndex < subNodeCount && readCount < CmdParams[2])
                {
                    CmdParams[0] = (byte)(curStartIndex & 0x00FF);
                    CmdParams[1] = (byte)(curStartIndex >> 8 & 0x00FF);
                    CmdParams[2] = (byte)(CmdParams[2] - readCount);

                    CmdWaitTime = 2;
                    CmdRetryTimes = 3;
                    CmdSn = SequenceNo++;
                }
                else
                {
                    bTransceiveEnable = false;
                    Cmd = Command.任务空闲;
                    btReadSubnodeInfo.Enabled = true;
                }
            }
        }
        #endregion

        #region 查询从节点主动注册进度
        private void btReadSubnodeRegProgress_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.查询从节点主动注册进度;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            btReadSubnodeCount.Enabled = false;
        }
        private void TransmitReadSubnodeRegisterProgress()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btReadSubnodeCount.Enabled = true;
                MessageBox.Show("查询从节点主动注册进度失败,模块无应答或应答错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn3_ReadParams;
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x07, 0x03, 0x00, 0xE8 };
            txData.DataBuf = null;
            DataTransmit(txData);
            CmdWaitTime = 150;
            CmdRetryTimes--;
        }
        private void ReceiveReadSubnodeRegisterProgress(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn == ProtoLocal_South.Afn.Afn3_ReadParams && RxData.DataId[0] == 0x07 && RxData.SerialNo == CmdSn)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btReadSubnodeCount.Enabled = true;
            }
        }
        #endregion

        #region 查询从节点的父节点
        int CurrentProcIndex = 0;
        private void btReadFatherNode_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            if (dgvDocument.SelectedRows.Count == 0)
            {
                MessageBox.Show("请至少选择一个要查询的从节点！", "错误");
                return;
            }
            CurrentProcIndex = 0;

            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.查询从节点的父节点;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            btReadSubnodeCount.Enabled = false;
        }
        private void TransmitReadFatherNode()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btReadSubnodeCount.Enabled = true;
                MessageBox.Show("查询从节点的父节点失败,模块无应答或应答错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn3_ReadParams;
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x08, 0x03, 0x03, 0xE8 };
            txData.DataBuf = new byte[ProtoLocal_South.LongAddrSize];
            Util.GetBytesFromStringHex(dgvDocument.SelectedRows[dgvDocument.SelectedRows.Count - CurrentProcIndex - 1].Cells[1].Value.ToString(), txData.DataBuf, 0, true);
            DataTransmit(txData);
            CmdWaitTime = 150;
            CmdRetryTimes--;
        }
        private void ReceiveReadFatherNode(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn != ProtoLocal_South.Afn.Afn3_ReadParams || RxData.DataId[0] != 0x08 || RxData.SerialNo != CmdSn)
            {
                return;
            }

            CurrentProcIndex++;
            CmdSn++;
            if (CurrentProcIndex >= dgvDocument.SelectedRows.Count)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btReadSubnodeCount.Enabled = true;
            }
            else
            {
                CmdRetryTimes = 3;
                CmdWaitTime = 2;
            }
        }
        #endregion
        
        #region 设置主节点地址
        InputAddrDlg SetConcAddrDlg = null;
        string strNewConcAddr = "";
        private void btSetConcAddr_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            if (SetConcAddrDlg == null || SetConcAddrDlg.IsDisposed)
            {
                SetConcAddrDlg = new InputAddrDlg("主节点地址", strNewConcAddr);
                SetConcAddrDlg.NewAddress = SetConcAddr;
            }
            SetConcAddrDlg.Show();
            SetConcAddrDlg.Focus();
        }
        private void SetConcAddr(string strAddr)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.设置主节点地址;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            btSetConcAddr.Enabled = false;
            strNewConcAddr = strAddr;
        }
        private void TransmitSetConcAddr()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btSetConcAddr.Enabled = true;
                MessageBox.Show("设置主节点地址失败,模块无应答或应答错误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn4_WriteParams;
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x01, 0x04, 0x02, 0xE8 };
            txData.DataBuf = new byte[ProtoLocal_South.LongAddrSize];
            Util.GetBytesFromStringHex(strNewConcAddr, txData.DataBuf, 0, true);
            DataTransmit(txData);
            CmdWaitTime = 200;
            CmdRetryTimes--;
        }
        private void ReceiveSetConcAddr(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn == ProtoLocal_South.Afn.Afn0_Ack && RxData.DataId[0] == 0x01 && RxData.SerialNo == CmdSn)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btSetConcAddr.Enabled = true;
                lbConcAddr.Text = "主节点地址:" + strNewConcAddr;
                MessageBox.Show("设置主节点地址成功！");
                return;
            }
        }
        #endregion

        #region 允许_禁止上报从节点事件
        private void btEnableEventReport_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.允许_禁止上报从节点事件;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            btEnableEventReport.Enabled = false;

            CmdParams = new byte[1] { 0x01 };   // 允许
        }
        private void btDisableEventReport_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.允许_禁止上报从节点事件;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            btDisableEventReport.Enabled = false;

            CmdParams = new byte[1] { 0x00 };   // 禁止
        }
        private void TransmitControlEventReport()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btEnableEventReport.Enabled = true;
                btDisableEventReport.Enabled = true;
                MessageBox.Show("允许//禁止节点事件上报失败,模块无应答或应答错误!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn4_WriteParams;
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x04, 0x04, 0x02, 0xE8 };
            txData.DataBuf = CmdParams;   // 0禁止,1允许
            DataTransmit(txData);
            CmdWaitTime = 150;
            CmdRetryTimes--;
        }
        private void ReceiveControlEventReport(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn != ProtoLocal_South.Afn.Afn0_Ack || RxData.DataId[0] != 0x01 || RxData.SerialNo != CmdSn)
            {
                return;
            }
            bTransceiveEnable = false;
            Cmd = Command.任务空闲;
            btEnableEventReport.Enabled = true;
            btDisableEventReport.Enabled = true;
        }
        #endregion

        #region 激活从节点主动注册
        private void btStartRegister_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.激活从节点主动注册;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            btStartRegister.Enabled = false;
        }
        private void TransmitStartRegister()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btStartRegister.Enabled = true;
                MessageBox.Show("激活从节点主动注册失败,模块无应答或应答错误!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn4_WriteParams;
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x05, 0x04, 0x02, 0xE8 };
            txData.DataBuf = new byte[] { 
                0x0A, 0x00,                 // 持续时间
                0x05,                       // 从节点重发次数
                0x05,                       // 随机等待时间片个数
            };
            DataTransmit(txData);
            CmdWaitTime = 150;
            CmdRetryTimes--;
        }
        private void ReceiveStartRegister(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn != ProtoLocal_South.Afn.Afn0_Ack || RxData.DataId[0] != 0x01 || RxData.SerialNo != CmdSn)
            {
                return;
            }
            bTransceiveEnable = false;
            Cmd = Command.任务空闲;
            btStartRegister.Enabled = true;
        }
        #endregion

        #region 终止从节点主动注册
        private void btStopRegister_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.终止从节点主动注册;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            btStopRegister.Enabled = false;
        }
        private void TransmitStopRegister()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                btStopRegister.Enabled = true;
                MessageBox.Show("终止从节点主动注册失败,模块无应答或应答错误!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn4_WriteParams;
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x06, 0x04, 0x02, 0xE8 };
            txData.DataBuf = null;
            DataTransmit(txData);
            CmdWaitTime = 150;
            CmdRetryTimes--;
        }
        private void ReceiveStopRegister(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn != ProtoLocal_South.Afn.Afn0_Ack || RxData.DataId[0] != 0x01 || RxData.SerialNo != CmdSn)
            {
                return;
            }
            bTransceiveEnable = false;
            Cmd = Command.任务空闲;
            btStopRegister.Enabled = true;
        }
        #endregion

        #region 上报任务数据
        private void ReceiveReportTaskData(ProtoLocal_South.PacketFormat RxData)
        {
            byte iStart;
            string strResult = "";

            for (iStart = 0; iStart < RxData.DataBuf.Length; iStart++)
            {
                if (0x68 == RxData.DataBuf[iStart] && 0x68 == RxData.DataBuf[iStart + 7])
                {
                    break;
                }
            }

            if (0x91 == RxData.DataBuf[iStart + 8])
            {
                strResult += (RxData.DataBuf[iStart + 17] - 0x33).ToString("X2");
                strResult += (RxData.DataBuf[iStart + 16] - 0x33).ToString("X2");
                strResult += (RxData.DataBuf[iStart + 15] - 0x33).ToString("X2") + ".";
                strResult += (RxData.DataBuf[iStart + 14] - 0x33).ToString("X2") + "kWh";

                for (int iRow = 0; iRow < tbDocument.Rows.Count; iRow++)
                {
                    if (tbDocument.Rows[iRow]["表具地址"].ToString() == Util.GetStringHexFromBytes( RxData.SrcAddr, 0, 6, "", true))
                    {
                        tbDocument.Rows[iRow]["状态"] = "Success";
                        tbDocument.Rows[iRow]["结果"] = strResult;
                        break;
                    }
                }
            
                // 回复确认报文
                ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
                txData.Afn = ProtoLocal_South.Afn.Afn0_Ack;
                txData.CtrlWord = 0x00;
                txData.SerialNo = RxData.SerialNo;
                txData.DataId = new byte[] { 0x01, 0x00, 0x01, 0xE8 };
                txData.DataBuf = new byte[2];
                txData.DataBuf[0] = 0x00;
                txData.DataBuf[1] = 0x00;
                DataTransmit(txData);
            }
        }
        #endregion

        #region 上报子节点事件
        private void ReceiveReportSubNodeEvent(ProtoLocal_South.PacketFormat RxData)
        {
            // 回复确认报文
            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn0_Ack;
            txData.CtrlWord = 0x00;
            txData.SerialNo = RxData.SerialNo;
            txData.DataId = new byte[] { 0x01, 0x00, 0x01, 0xE8 };
            txData.DataBuf = new byte[2];
            txData.DataBuf[0] = 0x00;
            txData.DataBuf[1] = 0x00;
            DataTransmit(txData);
        }
        #endregion

        #region 上报子节点信息
        private void ReceiveReportSubNodeInfo(ProtoLocal_South.PacketFormat RxData)
        {
            // 回复确认报文
            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn0_Ack;
            txData.CtrlWord = 0x00;
            txData.SerialNo = RxData.SerialNo;
            txData.DataId = new byte[] { 0x01, 0x00, 0x01, 0xE8 };
            txData.DataBuf = new byte[2];
            txData.DataBuf[0] = 0x00;
            txData.DataBuf[1] = 0x00;
            DataTransmit(txData);
        }
        #endregion

        #region 上报子节点主动注册结束
        private void ReceiveReportSubNodeRegisterEnd(ProtoLocal_South.PacketFormat RxData)
        {
            // 回复确认报文
            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn0_Ack;
            txData.CtrlWord = 0x00;
            txData.SerialNo = RxData.SerialNo;
            txData.DataId = new byte[] { 0x01, 0x00, 0x01, 0xE8 };
            txData.DataBuf = new byte[2];
            txData.DataBuf[0] = 0x00;
            txData.DataBuf[1] = 0x00;
            DataTransmit(txData);
        }
        #endregion

        #region 上报任务状态
        private void ReceiveReportTaskStatus(ProtoLocal_South.PacketFormat RxData)
        {
            for (int iRow = 0; iRow < tbDocument.Rows.Count; iRow++)
            {
                if (tbDocument.Rows[iRow]["表具地址"].ToString() == Util.GetStringHexFromBytes(RxData.DataBuf, 2, 6, "", true))
                {
                    string strStatus = "Failed";
                    string strResult = "";

                    switch (RxData.DataBuf[8])
                    {
                        case 0x00:
                            strStatus = "Success";
                            strResult = "成功";
                            break;

                        case 0x01:
                            strResult = "从节点无响应";
                            break;

                        case 0x02:
                            strResult = "数据不合法";
                            break;

                        case 0xFF:
                        default:
                            strResult = "其他错误";
                            break;
                    }

                    tbDocument.Rows[iRow]["状态"] = strStatus;
                    tbDocument.Rows[iRow]["结果"] = strResult;
                    break;
                }
            }

            // 回复确认报文
            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn0_Ack;
            txData.CtrlWord = 0x00;
            txData.SerialNo = RxData.SerialNo;
            txData.DataId = new byte[] { 0x01, 0x00, 0x01, 0xE8 };
            txData.DataBuf = new byte[2];
            txData.DataBuf[0] = 0x00;
            txData.DataBuf[1] = 0x00;
            DataTransmit(txData);
        }
        #endregion

        #region 请求集中器时间
        private void ReceiveRequestRtc(ProtoLocal_South.PacketFormat RxData)
        {
            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn6_RequestInfo;
            txData.CtrlWord = 0x00;
            txData.SerialNo = RxData.SerialNo;
            txData.DataId = new byte[] { 0x01, 0x06, 0x06, 0xE8 };
            txData.DataBuf = new byte[6];
            string strRtc = DateTime.Now.ToString("ssmmHHddMMyy");
            txData.DataBuf[0] = Convert.ToByte(strRtc.Substring(0, 2), 16);
            txData.DataBuf[1] = Convert.ToByte(strRtc.Substring(2, 2), 16);
            txData.DataBuf[2] = Convert.ToByte(strRtc.Substring(4, 2), 16);
            txData.DataBuf[3] = Convert.ToByte(strRtc.Substring(6, 2), 16);
            txData.DataBuf[4] = Convert.ToByte(strRtc.Substring(8, 2), 16);
            txData.DataBuf[5] = Convert.ToByte(strRtc.Substring(10, 2), 16);
            DataTransmit(txData);
        }
        #endregion

        #region 打开下拉菜单
        private void cnMenuDocument_Opening(object sender, CancelEventArgs e)
        {
            tsmiAddTask.Enabled = true;
            tsmiDelTask.Enabled = true;
            tsmiClearDocument.Enabled = true;
            tsmiSelectAll.Enabled = true;
            tsmiImportFromDevice.Enabled = true;
            tsmiExportToDevice.Enabled = true;
            tsmiLoadDocument.Enabled = true;
            tsmiSaveDocument.Enabled = true;
            tsmiAddSubNode.Enabled = true;
            tsmiDelSubNode.Enabled = true;
            if (tbDocument.Rows.Count == 0)
            {
                tsmiAddTask.Enabled = false;
                tsmiDelTask.Enabled = false;
                tsmiClearDocument.Enabled = false;
                tsmiSelectAll.Enabled = false;
                tsmiExportToDevice.Enabled = false;
                tsmiSaveDocument.Enabled = false;
                tsmiDelSubNode.Enabled = false;
            }
            if (dgvDocument.SelectedRows.Count == 0)
            {
                tsmiAddTask.Enabled = false;
                tsmiDelTask.Enabled = false;
                tsmiDelSubNode.Enabled = false;
            }
        }
        #endregion

        #region 添加任务
        AddTaskDlg AddNodeTaskDlg = null;
        byte[] TaskContent = null;
        int CurrentAddTaskNodeNo = 0;
        private void tsmiAddTask_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            if (dgvDocument.SelectedRows.Count == 0)
            {
                MessageBox.Show("请至少选择一个要添加任务的节点！", "错误");
                return;
            }
            if (AddNodeTaskDlg == null || AddNodeTaskDlg.IsDisposed)
            {
                AddNodeTaskDlg = new AddTaskDlg();
                AddNodeTaskDlg.addTaskProc = AddTask;
            }
            AddNodeTaskDlg.Show();
            AddNodeTaskDlg.Focus();
        }
        private void AddTask(byte[] taskArray)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            if (dgvDocument.SelectedRows.Count == 0)
            {
                MessageBox.Show("请至少为一个节点添加任务！", "错误");
                return;
            }

            dgvDocument.Enabled = false;

            TaskContent = new byte[taskArray.Length + 14];

            byte index = 0;
            Array.Copy(taskArray, 0, TaskContent, 0, 5);             // 任务ID、任务模式字、超时时间
            index += 5;

            byte DltCmdLen = taskArray[index];
            byte[] DltCmd = new byte[DltCmdLen];
            Array.Copy(taskArray, index + 1, DltCmd, 0, DltCmdLen);

            TaskContent[index++] = (byte)(DltCmdLen + 14);          // 报文长度

            /* DLT645报文 - 初始化打包 */
            TaskContent[index++] = 0xFE;                            // 报文内容： DLT645指令  
            TaskContent[index++] = 0xFE;
            TaskContent[index++] = 0xFE;
            TaskContent[index++] = 0xFE;
            byte[] DltAddr = new byte[ProtoLocal_South.LongAddrSize];       // 目的地址，发送时填充
            byte[] DltFrame = Util.GetDlt645Frame(DltAddr, DltCmd);
            Array.Copy(DltFrame, 0, TaskContent, index, DltFrame.Length);

            byte multiTaskFlag = TaskContent[0];    // 单播/多播标识
            CmdParams = new byte[1]{ multiTaskFlag };

            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = multiTaskFlag == 0 ? Command.添加任务 : Command.添加多播任务;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            CurrentAddTaskNodeNo = 0;
        }
        private void TransmitAddTask()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                dgvDocument.Enabled = true;
                MessageBox.Show("向节点" + dgvDocument.SelectedRows[dgvDocument.SelectedRows.Count - CurrentAddTaskNodeNo - 1].Cells[1].Value.ToString()  + "添加任务失败,模块无应答或应答错误！\n添加任务中断！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn2_TaskManage;
            txData.CtrlWord = 0x60;
            txData.SrcAddr = new byte[ProtoLocal_South.LongAddrSize];
            Util.GetBytesFromStringHex(strConcAddr, txData.SrcAddr, 0, true);
            txData.DstAddr = new byte[ProtoLocal_South.LongAddrSize];
            Util.GetBytesFromStringHex(dgvDocument.SelectedRows[dgvDocument.SelectedRows.Count - CurrentAddTaskNodeNo - 1].Cells[1].Value.ToString(), txData.DstAddr, 0, true);
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x01, 0x02, 0x02, 0xE8 };
            txData.DataBuf = new byte[TaskContent.Length];

            TaskContent[0] = (byte)TaskIdSequence;
            TaskContent[1] = (byte)(TaskIdSequence >> 8);

            /* DLT645报文 - 修改地址，重新打包 */
            byte iStart = 10;                       // 起始字符索引，跳过 任务ID、任务模式字、超时时间、报文长度、4字节0xFE
            byte DltCmdLen = (byte)(TaskContent.Length - iStart - 10);
            byte[] DltCmd = new byte[DltCmdLen];
            Array.Copy(TaskContent, iStart + 8, DltCmd, 0, DltCmdLen);
            byte[] DltAddr = null;
            if( (TaskContent[2] & 0x80 ) == 0)  // 不需要应答，广播地址
            {
                DltAddr = new byte[ProtoLocal_South.LongAddrSize] { 0x99, 0x99, 0x99, 0x99, 0x99, 0x99 };
            }
            else    // 需要应答，电表地址
            {
                DltAddr = new byte[ProtoLocal_South.LongAddrSize];
                txData.DstAddr.CopyTo(DltAddr, 0);
            }
            byte[] frame = Util.GetDlt645Frame(DltAddr, DltCmd);
            Array.Copy(frame, 0, TaskContent, iStart, frame.Length);

            Array.Copy(TaskContent, txData.DataBuf, TaskContent.Length);
            DataTransmit(txData);
            CmdWaitTime = 200;
            CmdRetryTimes--;
        }
        private void ReceiveAddTask(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn == ProtoLocal_South.Afn.Afn0_Ack && RxData.DataId[0] == 0x01 && RxData.SerialNo == CmdSn)
            {
                string strSubNodeAddr = dgvDocument.SelectedRows[dgvDocument.SelectedRows.Count - CurrentAddTaskNodeNo - 1].Cells[1].Value.ToString();
                for (int iRow = 0; iRow < tbDocument.Rows.Count; iRow++ )
                {
                    if (tbDocument.Rows[iRow]["表具地址"].ToString() == strSubNodeAddr)
                    {
                        tbDocument.Rows[iRow]["任务ID"] = (TaskContent[0] + TaskContent[1] * 256).ToString("X4");
                        tbDocument.Rows[iRow]["优先级"] = (TaskContent[2] & 0x0F).ToString("D") + "级";
                        tbDocument.Rows[iRow]["状态"] = (TaskContent[3] + TaskContent[4] * 256).ToString("D");
                        tbDocument.Rows[iRow]["结果"] = "未知";
                        break;
                    }
                }
                TaskIdSequence++;
                CurrentAddTaskNodeNo++;
                CmdSn++;
                if (CurrentAddTaskNodeNo >= dgvDocument.SelectedRows.Count)
                {
                    bTransceiveEnable = false;
                    Cmd = Command.任务空闲;
                    dgvDocument.Enabled = true;
                    MessageBox.Show("成功为" + dgvDocument.SelectedRows.Count.ToString("D") + "个节点添加了任务！");
                }
                else
                {
                    CmdRetryTimes = 3;
                    CmdWaitTime = 2;
                }
            }
        }
        #endregion

        #region 添加多播任务
        private void TransmitAddMultiTask()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                dgvDocument.Enabled = true;
                MessageBox.Show("添加多播任务失败,模块无应答或应答错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            byte multiTaskFlag = CmdParams[0];    // 单播/多播标识

            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn2_TaskManage;
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x07, 0x02, 0x02, 0xE8 };

            TaskContent[0] = (byte)TaskIdSequence;
            TaskContent[1] = (byte)(TaskIdSequence >> 8);

            int payloadLen = TaskContent.Length;
            payloadLen += (0x02 == multiTaskFlag) ? (2) : (dgvDocument.SelectedRows.Count * 6 + 2);
            txData.DataBuf = new byte[payloadLen];

            int index = 0;
            Array.Copy(TaskContent, 0, txData.DataBuf, index, 3);
            index += 3;

            //多播地址数、地址列表
            if( 0x02 == multiTaskFlag)      //多播所有
            {
                txData.DataBuf[index++] = 0xFF;
                txData.DataBuf[index++] = 0xFF;
            }
            else if (0x01 == multiTaskFlag) //多播已选择
            {
                txData.DataBuf[index++] = (byte)(dgvDocument.SelectedRows.Count & 0x00FF);
                txData.DataBuf[index++] = (byte)(dgvDocument.SelectedRows.Count >> 8);
                foreach (DataGridViewRow row in dgvDocument.SelectedRows)
                {
                    Util.GetBytesFromStringHex(row.Cells[1].Value.ToString(), txData.DataBuf, index, true);
                    index += ProtoLocal_South.LongAddrSize;
                }
            }

            Array.Copy(TaskContent, 3, txData.DataBuf, index, 7);
            index += 7;

            /* DLT645报文 - 修改地址，重新打包 */
            byte iStart = 10;                       // 起始字符索引: 跳过 任务ID、任务模式字、超时时间、报文长度、4字节0xFE
            byte DltCmdLen = (byte)(TaskContent.Length - iStart - 10);
            byte[] DltCmd = new byte[DltCmdLen];
            Array.Copy(TaskContent, iStart + 8, DltCmd, 0, DltCmdLen);
            byte[] DltAddr = new byte[ProtoLocal_South.LongAddrSize] { 0x99, 0x99, 0x99, 0x99, 0x99, 0x99 };
            //byte[] DltAddr = new byte[ProtoLocal_South.LongAddrSize] { 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA};

            byte[] frame = Util.GetDlt645Frame(DltAddr, DltCmd);
            Array.Copy(frame, 0, TaskContent, iStart, frame.Length);

            Array.Copy(TaskContent, 10, txData.DataBuf, index, TaskContent.Length - 10);
            DataTransmit(txData);
            CmdWaitTime = 200;
            CmdRetryTimes--;
        }
        private void ReceiveAddMultiTask(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn == ProtoLocal_South.Afn.Afn0_Ack && RxData.DataId[0] == 0x01 && RxData.SerialNo == CmdSn)
            {
                for (int iRow = 0, index = 0; iRow < dgvDocument.SelectedRows.Count; iRow++)
                {
                    index = dgvDocument.SelectedRows[dgvDocument.SelectedRows.Count - iRow -1].Index;
                    tbDocument.Rows[index]["任务ID"] = (TaskIdSequence++).ToString("X4");
                    tbDocument.Rows[index]["优先级"] = (TaskContent[2] & 0x0F).ToString("D") + "级";
                    tbDocument.Rows[index]["状态"] = (TaskContent[3] + TaskContent[4] * 256).ToString("D");
                    tbDocument.Rows[index]["结果"] = "未知";
                }

                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                dgvDocument.Enabled = true;
                MessageBox.Show("成功为" + dgvDocument.SelectedRows.Count.ToString("D") + "个节点添加了任务！");
            }
        }
        #endregion

        #region 删除任务
        int CurrentDelTaskNodeNo = 0;
        private void tsmiDelTask_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            if (dgvDocument.SelectedRows.Count == 0)
            {
                MessageBox.Show("请至少选择一个要删除任务的节点！", "错误");
                return;
            }
            dgvDocument.Enabled = false;
            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.删除任务;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            CurrentDelTaskNodeNo = 0;
        }
        private void TransmitDelTask()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                dgvDocument.Enabled = true;
                MessageBox.Show("节点" + dgvDocument.SelectedRows[dgvDocument.SelectedRows.Count - CurrentDelTaskNodeNo - 1].Cells[1].Value.ToString() + "删除任务失败,模块无应答或应答错误！\n添加任务中断！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn2_TaskManage;
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x02, 0x02, 0x02, 0xE8 };
            string strTaskId = dgvDocument.SelectedRows[dgvDocument.SelectedRows.Count - CurrentDelTaskNodeNo - 1].Cells[2].Value.ToString();
            txData.DataBuf = new byte[2];
            try
            {
                Util.GetBytesFromStringHex(strTaskId, txData.DataBuf, 0, true);
                DataTransmit(txData);
                CmdWaitTime = 150;
                CmdRetryTimes--;
            }
            catch
            {            
                string strSubNodeAddr = dgvDocument.SelectedRows[dgvDocument.SelectedRows.Count - CurrentDelTaskNodeNo - 1].Cells[1].Value.ToString();
                for (int iRow = 0; iRow < tbDocument.Rows.Count; iRow++)
                {
                    if (tbDocument.Rows[iRow]["表具地址"].ToString() == strSubNodeAddr)
                    {
                        tbDocument.Rows[iRow]["任务ID"] = "无";
                        tbDocument.Rows[iRow]["优先级"] = "无";
                        tbDocument.Rows[iRow]["状态"] = "未知";
                        tbDocument.Rows[iRow]["结果"] = "未知";
                        break;
                    }
                }
                CurrentDelTaskNodeNo++;
                if (CurrentDelTaskNodeNo >= dgvDocument.SelectedRows.Count)
                {
                    bTransceiveEnable = false;
                    Cmd = Command.任务空闲;
                    dgvDocument.Enabled = true;
                    MessageBox.Show("成功删除" + dgvDocument.SelectedRows.Count.ToString("D") + "个节点的任务！");
                }
                CmdWaitTime = 2;
                CmdRetryTimes = 3;
                CmdSn++;
            }
        }
        private void ReceiveDelTask(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn == ProtoLocal_South.Afn.Afn0_Ack && RxData.DataId[0] == 0x01 && RxData.SerialNo == CmdSn)
            {
                string strSubNodeAddr = dgvDocument.SelectedRows[dgvDocument.SelectedRows.Count - CurrentDelTaskNodeNo - 1].Cells[1].Value.ToString();
                for (int iRow = 0; iRow < tbDocument.Rows.Count; iRow++)
                {
                    if (tbDocument.Rows[iRow]["表具地址"].ToString() == strSubNodeAddr)
                    {
                        tbDocument.Rows[iRow]["任务ID"] = "无";
                        tbDocument.Rows[iRow]["优先级"] = "无";
                        tbDocument.Rows[iRow]["状态"] = "已删除";
                        tbDocument.Rows[iRow]["结果"] = "未知";
                        break;
                    }
                }
                CurrentDelTaskNodeNo++;
                CmdSn++;
                if (CurrentDelTaskNodeNo >= dgvDocument.SelectedRows.Count)
                {
                    bTransceiveEnable = false;
                    Cmd = Command.任务空闲;
                    dgvDocument.Enabled = true;
                    MessageBox.Show("成功删除" + dgvDocument.SelectedRows.Count.ToString("D") + "个节点的任务！");
                }
                else
                {
                    CmdRetryTimes = 3;
                    CmdWaitTime = 2;
                }
            }
        }
        #endregion

        #region 清空档案
        private void tsmiClearDocument_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            tbDocument.Clear();
            lbDocument.Text = "档案列表〖" + tbDocument.Rows.Count.ToString("D") + "〗";
        }
        #endregion

        #region 全部选择
        private void tsmiSelectAll_Click(object sender, EventArgs e)
        {
            dgvDocument.SelectAll();
        }
        #endregion

        #region 从设备导入档案
        int currentSubNodeImportFromDevice = 0;
        private void tsmiImportFromDevice_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            if (tbDocument.Rows.Count > 0)
            {
                if (DialogResult.Cancel == MessageBox.Show("新导入的档案会清除现有列表档案，继续吗？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning))
                {
                    return;
                }
            }
            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.从设备导入档案;
            SubCmd = Command.查询从节点信息;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            tbDocument.Clear();
            currentSubNodeImportFromDevice = 0;
        }
        private void TransmitImportFromDevice()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                MessageBox.Show("从设备导入档案失败，模块没有应答或者应答错误！");
                return;
            }
            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            if (SubCmd == Command.查询从节点信息)
            {
                txData.Afn = ProtoLocal_South.Afn.Afn3_ReadParams;
                txData.DataId = new byte[] { 0x06, 0x03, 0x03, 0xE8 };
                txData.DataBuf = new byte[3];
                txData.DataBuf[0] = (byte)currentSubNodeImportFromDevice;
                txData.DataBuf[1] = (byte)(currentSubNodeImportFromDevice >> 8);
                txData.DataBuf[2] = 30;
            }
            DataTransmit(txData);
            CmdWaitTime = 300;
            CmdRetryTimes--;
        }
        private void ReceiveImportFromDevice(ProtoLocal_South.PacketFormat RxData)
        {
            int totalSubNodeCount;

            if (SubCmd == Command.查询从节点信息)
            {
                if (RxData.Afn == ProtoLocal_South.Afn.Afn3_ReadParams && RxData.DataId[0] == 0x06 && RxData.SerialNo == CmdSn)
                {
                    totalSubNodeCount = RxData.DataBuf[0] + RxData.DataBuf[1] * 256;
                    byte readCount = RxData.DataBuf[2];
                    for (int iLoop = 0; iLoop < readCount; iLoop++ )
                    {
                        DataRow newRow = tbDocument.NewRow();
                        newRow["SN"] = tbDocument.Rows.Count.ToString("D3");
                        newRow["表具地址"] = Util.GetStringHexFromBytes(RxData.DataBuf, iLoop * ProtoLocal_South.LongAddrSize + 3, ProtoLocal_South.LongAddrSize, "", true);
                        newRow["任务ID"] = "无";
                        newRow["优先级"] = "无";
                        newRow["状态"] = "未知";
                        newRow["结果"] = "未知";
                        tbDocument.Rows.Add(newRow);
                    }

                    lbDocument.Text = "档案列表〖" + tbDocument.Rows.Count.ToString("D") + "〗";
                    currentSubNodeImportFromDevice += readCount; 
                    if (currentSubNodeImportFromDevice < totalSubNodeCount)
                    {
                        CmdWaitTime = 2;
                        CmdRetryTimes = 3;
                        CmdSn = SequenceNo++;
                    }
                    else
                    {
                        bTransceiveEnable = false;
                        Cmd = Command.任务空闲;
                        MessageBox.Show("已成功导入" + tbDocument.Rows.Count.ToString("D") + "个档案信息！");
                    }
                }
            }
        }
        #endregion

        #region 导出档案到设备
        int totalSubNodesExportToDevice = 0;
        byte currentSubNodeExportToDevice = 0;
        private void tsmiExportToDevice_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            if (tbDocument.Rows.Count == 0)
            {
                MessageBox.Show("没有档案可以导出，请先添加或导入从节点！");
                return;
            }
            if (DialogResult.No == MessageBox.Show("本操作会清除模块中原有的档案数据，是否继续？", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
            {
                return;
            }
            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.导出档案到设备;
            SubCmd = Command.初始化档案;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
        }
        private void TransmitExportToDevice()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                MessageBox.Show("导出档案到设备失败，模块没有应答或者应答错误，请注意模块中原有的档案信息可能遭到破坏。");
                return;
            }
            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            if (SubCmd == Command.初始化档案)
            {
                txData.Afn = ProtoLocal_South.Afn.Afn1_Initial;
                txData.DataId = new byte[] { 0x02, 0x01, 0x02, 0xE8 };
                txData.DataBuf = null;
            }
            else if (SubCmd == Command.添加从节点)
            {
                txData.Afn = ProtoLocal_South.Afn.Afn4_WriteParams;
                txData.DataId = new byte[] { 0x02, 0x04, 0x02, 0xE8 };
                byte[] buf = new byte[300];
                for (currentSubNodeExportToDevice = 0; currentSubNodeExportToDevice + totalSubNodesExportToDevice < tbDocument.Rows.Count && currentSubNodeExportToDevice < 30; currentSubNodeExportToDevice++)
                {
                    Util.GetBytesFromStringHex(tbDocument.Rows[currentSubNodeExportToDevice + totalSubNodesExportToDevice]["表具地址"].ToString(), buf, currentSubNodeExportToDevice * 6, true);
                }
                txData.DataBuf = new byte[currentSubNodeExportToDevice * ProtoLocal_South.LongAddrSize + 1];
                txData.DataBuf[0] = currentSubNodeExportToDevice;
                Array.Copy(buf, 0, txData.DataBuf, 1, currentSubNodeExportToDevice * ProtoLocal_South.LongAddrSize);
            }
            else if (SubCmd == Command.初始化任务)
            {
                txData.Afn = ProtoLocal_South.Afn.Afn1_Initial;
                txData.DataId = new byte[] { 0x03, 0x01, 0x02, 0xE8 };
                txData.DataBuf = null;
            }
            else
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                MessageBox.Show("出现了未知的错误！");
                return;
            }

            DataTransmit(txData);
            CmdWaitTime = 500;
            CmdRetryTimes--;

        }
        private void ReceiveExportToDevice(ProtoLocal_South.PacketFormat RxData)
        {
            if (SubCmd == Command.初始化档案)
            {
                if (RxData.Afn == ProtoLocal_South.Afn.Afn0_Ack && RxData.SerialNo == CmdSn)
                {
                    SubCmd = Command.添加从节点;
                    totalSubNodesExportToDevice = 0;
                    CmdWaitTime = 2;
                    CmdRetryTimes = 3;
                    CmdSn = SequenceNo++;
                }
            }
            else if (SubCmd == Command.添加从节点)
            {
                if (RxData.Afn == ProtoLocal_South.Afn.Afn0_Ack && RxData.SerialNo == CmdSn)
                {
                    totalSubNodesExportToDevice += currentSubNodeExportToDevice;
                    if (totalSubNodesExportToDevice >= tbDocument.Rows.Count)
                    {
                        SubCmd = Command.初始化任务;
                        CmdWaitTime = 2;
                        CmdRetryTimes = 3;
                        CmdSn = SequenceNo++;
                    }
                    else
                    {
                        CmdWaitTime = 2;
                        CmdRetryTimes = 3;
                        CmdSn = SequenceNo++;
                    }
                }
            }
            else if (SubCmd == Command.初始化任务)
            {
                if (RxData.Afn == ProtoLocal_South.Afn.Afn0_Ack && RxData.SerialNo == CmdSn)
                {
                    for (int iRow = 0; iRow < tbDocument.Rows.Count; iRow++)
                    {
                        tbDocument.Rows[iRow]["任务ID"] = "无";
                        tbDocument.Rows[iRow]["优先级"] = "无";
                        tbDocument.Rows[iRow]["状态"] = "未知";
                        tbDocument.Rows[iRow]["结果"] = "未知";
                    }
                    bTransceiveEnable = false;
                    Cmd = Command.任务空闲;
                    MessageBox.Show("档案信息已经成功导入模块中！");
                }
            }
        }
        #endregion

        #region 从文件读取档案
        private void tsmiLoadDocument_Click(object sender, EventArgs e)
        {
            string strDirectory, strFileName, strRead, strLine;

            if (tbDocument.Rows.Count > 0)
            {
                if (DialogResult.Cancel == MessageBox.Show("导入的档案将会清除当前的档案，是否继续?", "导入档案", MessageBoxButtons.OKCancel, MessageBoxIcon.Question))
                {
                    return;
                }
            }

            strDirectory = XmlHelper.GetNodeDefValue(FrmMain.SystemConfigPath, "/Config/ConcSimulator_DocumentPath", Application.StartupPath);
            openFileDlg.InitialDirectory = strDirectory;
            openFileDlg.Filter = "*.TXT(文本文件)|*.TXT";
            openFileDlg.DefaultExt = "TXT";
            openFileDlg.FileName = "";
            if (DialogResult.OK != openFileDlg.ShowDialog())
            {
                return;
            }

            strFileName = openFileDlg.FileName;
            if (strFileName.Length == 0)
            {
                MessageBox.Show("档案文件导入失败！\n", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (strDirectory != Path.GetDirectoryName(strFileName))
            {
                strDirectory = Path.GetDirectoryName(strFileName);
                XmlHelper.SetNodeValue(FrmMain.SystemConfigPath, "/Config", "ConcSimulator_DocumentPath", strDirectory);
            }
            StreamReader sr = new StreamReader(strFileName, Encoding.GetEncoding("GB2312"));

            lbDocument.Text = "档案列表";
            tbDocument.Clear();
            while ((strRead = sr.ReadLine()) != null)
            {
                try
                {
                    DataRow newRow = tbDocument.NewRow();
                    newRow["SN"] = tbDocument.Rows.Count.ToString("D3");
                    strLine = strRead.Substring(0, 12);
                    Convert.ToUInt64(strLine);
                    newRow["表具地址"] = strLine;
                    newRow["任务ID"] = "无";
                    newRow["优先级"] = "无";
                    newRow["状态"] = "未知";
                    newRow["结果"] = "未知";
                    tbDocument.Rows.Add(newRow);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("在读取第" + tbDocument.Rows.Count.ToString() + "行时出现错误，" + ex.Message + "！\r\n", "错误信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                }
            }
            sr.Close();
            lbDocument.Text = "档案列表〖" + tbDocument.Rows.Count.ToString("D") + "〗";
            MessageBox.Show("成功从文件中读取" + tbDocument.Rows.Count.ToString("D") + "个档案！");
        }
        #endregion

        #region 保存档案到文件
        private void tsmiSaveDocument_Click(object sender, EventArgs e)
        {
            string strDirectory;
            string strFileName;

            if (0 == tbDocument.Rows.Count)
            {
                MessageBox.Show("没有可保存的档案!");
                return;
            }

            strDirectory = XmlHelper.GetNodeDefValue(FrmMain.SystemConfigPath, "/Config/ConcSimulator_DocumentPath", Application.StartupPath);
            saveFileDlg.Filter = "*.txt(文本文件)|*.txt";
            saveFileDlg.DefaultExt = "txt";
            saveFileDlg.FileName = "";
            saveFileDlg.ShowDialog();

            strFileName = saveFileDlg.FileName;
            if (strFileName.Length == 0)
            {
                return;
            }

            if (strDirectory != Path.GetDirectoryName(strFileName))
            {
                strDirectory = Path.GetDirectoryName(strFileName);
                XmlHelper.SetNodeValue(FrmMain.SystemConfigPath, "/Config", "ConcSimulator_DocumentPath", strDirectory);
            }
            StreamWriter sw = new StreamWriter(strFileName, false);
            for (int i = 0; i < tbDocument.Rows.Count; i++)
            {
                sw.WriteLine(tbDocument.Rows[i]["表具地址"].ToString());
            }
            sw.Close();
            MessageBox.Show("保存档案成功！");
        }
        #endregion

        #region 添加从节点
        InputAddrDlg AddSubNodeDlg = null;
        string strNewSubNodeAddr = "";
        private void tsmiAddSubNode_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            if (AddSubNodeDlg == null || AddSubNodeDlg.IsDisposed)
            {
                AddSubNodeDlg = new InputAddrDlg("节点地址", strNewSubNodeAddr);
                AddSubNodeDlg.NewAddress = AddSubNode;
            }
            AddSubNodeDlg.Show();
            AddSubNodeDlg.Focus();
        }
        private void AddSubNode(string strAddr)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.添加从节点;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
            strNewSubNodeAddr = strAddr;
        }
        private void TransmitAddSubNode()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                MessageBox.Show("添加从节点失败,模块无应答或应答错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn4_WriteParams;
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x02, 0x04, 0x02, 0xE8 };
            txData.DataBuf = new byte[7];
            txData.DataBuf[0] = 1;
            Util.GetBytesFromStringHex(strNewSubNodeAddr, txData.DataBuf, 1, true);
            DataTransmit(txData);
            CmdWaitTime = 300;
            CmdRetryTimes--;
        }
        private void ReceiveAddSubNode(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn == ProtoLocal_South.Afn.Afn0_Ack && RxData.DataId[0] == 0x01 && RxData.SerialNo == CmdSn)
            {
                int iRow;
                for (iRow = 0; iRow < tbDocument.Rows.Count; iRow++ )
                {
                    if (strNewSubNodeAddr == tbDocument.Rows[iRow]["表具地址"].ToString())
                    {
                        break;
                    }
                }
                if (iRow >= tbDocument.Rows.Count)
                {
                    DataRow newRow = tbDocument.NewRow();
                    newRow["SN"] = tbDocument.Rows.Count.ToString("D3");
                    newRow["表具地址"] = strNewSubNodeAddr;
                    newRow["任务ID"] = "无";
                    newRow["优先级"] = "无";
                    newRow["状态"] = "未知";
                    newRow["结果"] = "未知";
                    tbDocument.Rows.Add(newRow);
                    lbDocument.Text = "档案列表〖" + tbDocument.Rows.Count.ToString("D") + "〗";
                }
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                MessageBox.Show("成功添加一个从节点！");
            }
        }
        #endregion

        #region 删除从节点
        byte[] delSubNodeArray = new byte[200];
        private void tsmiDelSubNode_Click(object sender, EventArgs e)
        {
            if (false == ComPortStatus() || false == CmdStatus())
            {
                return;
            }
            if (dgvDocument.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要删除的节点档案！");
                return;
            }
            if (dgvDocument.SelectedRows.Count > 20)
            {
                MessageBox.Show("每次最多可以删除20个节点，少选点！");
                return;
            }
            if (DialogResult.Cancel == MessageBox.Show("确定要删除选定的" + dgvDocument.SelectedRows.Count.ToString("D") + "个节点吗？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning))
            {
                return;
            }
            CmdRetryTimes = 3;
            CmdWaitTime = 2;
            Cmd = Command.删除从节点;
            CmdSn = SequenceNo++;
            bTransceiveEnable = true;
        }
        private void TransmitDelSubNode()
        {
            if (CmdRetryTimes == 0)
            {
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                MessageBox.Show("删除节点失败,模块无应答或应答错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProtoLocal_South.PacketFormat txData = new ProtoLocal_South.PacketFormat();
            txData.Afn = ProtoLocal_South.Afn.Afn4_WriteParams;
            txData.CtrlWord = 0x40;
            txData.SerialNo = CmdSn;
            txData.DataId = new byte[] { 0x03, 0x04, 0x02, 0xE8 };
            delSubNodeArray[0] = (byte)dgvDocument.SelectedRows.Count;
            for (int iLoop = 0; iLoop < delSubNodeArray[0]; iLoop++ )
            {
                Util.GetBytesFromStringHex(dgvDocument.SelectedRows[iLoop].Cells[1].Value.ToString(), delSubNodeArray, iLoop * ProtoLocal_South.LongAddrSize + 1, true);
            }
            txData.DataBuf = new byte[delSubNodeArray[0] * ProtoLocal_South.LongAddrSize + 1];
            Array.Copy(delSubNodeArray, txData.DataBuf, txData.DataBuf.Length);
            DataTransmit(txData);
            CmdWaitTime = 300;
            CmdRetryTimes--;
        }
        private void ReceiveDelSubNode(ProtoLocal_South.PacketFormat RxData)
        {
            if (RxData.Afn == ProtoLocal_South.Afn.Afn0_Ack && RxData.DataId[0] == 0x01 && RxData.SerialNo == CmdSn)
            {
                for (int iLoop = 0; iLoop < delSubNodeArray[0]; iLoop++ )
                {
                    string strAddr = Util.GetStringHexFromBytes(delSubNodeArray, iLoop * ProtoLocal_South.LongAddrSize + 1, ProtoLocal_South.LongAddrSize, "", true);
                    for (int iRow = 0; iRow < tbDocument.Rows.Count; iRow++)
                    {
                        if (tbDocument.Rows[iRow]["表具地址"].ToString() == strAddr)
                        {
                            tbDocument.Rows[iRow].Delete();
                            break;
                        }
                    }
                }
                for (int iRow = 0; iRow < tbDocument.Rows.Count; iRow++ )
                {
                    tbDocument.Rows[iRow]["SN"] = iRow.ToString("D3");
                }
                lbDocument.Text = "档案列表〖" + tbDocument.Rows.Count.ToString("D") + "〗";
                bTransceiveEnable = false;
                Cmd = Command.任务空闲;
                MessageBox.Show("已经成功删除了" + delSubNodeArray[0].ToString("D") + "个节点！");
            }
        }
        #endregion        

    }
}
