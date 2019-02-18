using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.Concurrent;
using System.Threading;
using ElectricPowerLib.Common;
using System.IO.Ports;
using System.IO;
using ElectricPowerLib.Protocol;

namespace ElectricPowerDebuger.Function
{
    public partial class ConcSimulator_North : UserControl
    {
        private string _configPath;
        private SerialCom _scom;
        private ConcurrentQueue<byte[]> _recvQueue;
        private ConcurrentQueue<Command> _sendQueue;
        private Thread _thrTransceiver;
        private bool _IsSendNewCmd;
        private LogHelper _errorLog;
        private bool _isAutoSaveLog;
        private FrmMain.FormEventNotify _evtLogAutoSaveChanged;
        private LogHelper _conCenterLog;

        delegate void CallbackFunc(params object[] args);
        private CallbackFunc _cmdEndCallback;
        private object[] _argsEndCallback;
        private Command CurrentCmd { get; set; }
        private Command PreparedCmd { get; set; }

        private string _strMsgBuf = "";
        private string _strMsgMain = "";
        private string _strMsgSub = "";

        private static byte[] TempBuf = new byte[4096];
        private static int _docCnt = 0;
        private static string _strCenterAddr = "201901020304";
        private static byte _fsn = 0;
        private static string _currProtol;

        public ConcSimulator_North()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            dgvDoc.DoubleBuffered(true);
            rtbMsg.DoubleBuffered(true);

            _configPath = FrmMain.SystemConfigPath;
            _scom = new SerialCom();
            _scom.DataReceivedEvent += SerialPort_DataReceived;
            _scom.UnexpectedClosedEvent += SerialPort_UnexpectedClosed;
            _recvQueue = new ConcurrentQueue<byte[]>();
            _sendQueue = new ConcurrentQueue<Command>();
            _thrTransceiver = new Thread(TransceiverHandle);
            _thrTransceiver.IsBackground = true;
            _thrTransceiver.Start();

            _errorLog = new LogHelper(_configPath + "/error.log");

            _currProtol = XmlHelper.GetNodeDefValue(_configPath, "Config/ProtocolVer", "");
            _evtLogAutoSaveChanged = new FrmMain.FormEventNotify(msg =>
            {
                if (msg == "true")
                {
                    _isAutoSaveLog = true;
                    if (!Directory.Exists("集中器本地报文"))
                    {
                        Directory.CreateDirectory("集中器本地报文");
                    }
                    string logPath = "集中器本地报文/" + DateTime.Now.ToString("yyyy-MM-dd") + "_ConCenter.log";
                    _conCenterLog = new LogHelper(logPath);
                }
                else
                {
                    _isAutoSaveLog = false;
                    _conCenterLog.Close();
                }
            });
            FrmMain.LogAutoSaveStateChanged += _evtLogAutoSaveChanged;

            LoadRecentCmdName();
            UpdateCenterAddr(_strCenterAddr);
        }

        #region 串口处理

        //串口选择
        private void cbxPortNum_Click(object sender, EventArgs e)
        {
            cbxPortNum.Items.Clear();
            cbxPortNum.Items.AddRange(SerialPort.GetPortNames());
        }

        //串口打开/关闭
        private void btPortCtrl_Click(object sender, EventArgs e)
        {
            if (cbxPortNum.Text == "")
            {
                return;
            }

            if (btPortCtrl.Text == "打开串口" && true == PortCtrl(true))
            {
                btPortCtrl.Text = "关闭串口";
                btPortCtrl.BackColor = Color.GreenYellow;
                cbxPortNum.Enabled = false;

                XmlHelper.SetNodeValue(_configPath, "/Config", "ConcSimulator_North_PortName", cbxPortNum.Text);

                SendToCmdQueue("查询主节点地址");
            }
            else
            {
                PortCtrl(false);
                btPortCtrl.Text = "打开串口";
                btPortCtrl.BackColor = Color.Silver;
                cbxPortNum.Enabled = true;

                UiOperateDisable();
                if (CurrentCmd != null)
                {
                    CurrentCmd.RetryTimes = 0;
                    CurrentCmd.TimeWaitMS = 0;
                }

            }
        }

        private bool PortCtrl(bool ctrl)
        {
            if (true == ctrl)
            {
                if (_scom.IsOpen == false)
                {
                    try
                    {
                        _scom.Config(cbxPortNum.Text, Convert.ToInt32("9600"), "8E1");
                        _scom.Open();
                    }
                    catch (Exception ex)
                    {
                        ShowMsg("打开串口失败：" + ex.Message + "\r\n", Color.Red);
                        return false;
                    }
                }
            }
            else
            {
                try
                {
                    _scom.Close();
                }
                catch (System.Exception ex)
                {
                    ShowMsg("关闭串口失败：" + ex.Message + "\r\n", Color.Red);
                    return false;
                }
            }
            return true;
        }

        //串口发送
        private bool SerialPort_SendData(byte[] buf)
        {
            bool ret = false;

            if (true == _scom.IsOpen)
            {
                try
                {
                    _scom.WritePort(buf, 0, buf.Length);
                    ret = true;
                }
                catch (Exception ex)
                {
                    ShowMsg("SerialPort_SendData error :" + ex.Message, Color.Red);
                }
            }

            return ret;
        }

        //串口接收
        private void SerialPort_DataReceived(byte[] buf)
        {
            _recvQueue.Enqueue(buf);
        }

        //串口异常断开
        private void SerialPort_UnexpectedClosed(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                Invoke(new EventHandler(SerialPort_UnexpectedClosed), new object[] { sender, e });
                return;
            }

            btPortCtrl_Click(null, null);

            ShowMsg("[ 串口连接已断开 ]" + " \r\n\r\n", Color.Red);
        }
        #endregion

        #region 命令处理--发送、接收线程

        // 发送、接收处理
        private void TransceiverHandle()
        {
            Command cmd = null;
            TimeSpan timeWait = TimeSpan.MaxValue;
            DateTime lastSendTime = DateTime.Now;
            int sendTimes = 0;
            string msg = "";
            bool enableUI = true;

            while (Thread.CurrentThread.IsAlive)
            {
                // send a new command
                if (_IsSendNewCmd && _sendQueue.Count > 0 && _sendQueue.TryDequeue(out cmd))
                {
                    cmd.IsEnable = true;
                    timeWait = TimeSpan.MaxValue;
                    sendTimes = 0;
                    CurrentCmd = cmd;
                    UiOperateDisable("show executing");
                }

                // send and retry
                if (cmd != null && cmd.IsEnable && timeWait.TotalMilliseconds > cmd.TimeWaitMS)
                {
                    if (cmd.RetryTimes > 0)
                    {
                        cmd.SendFunc(cmd);
                        sendTimes++;
                        cmd.RetryTimes--;
                        lastSendTime = DateTime.Now;
                        _IsSendNewCmd = false;
                    }
                    else
                    {
                        cmd.IsEnable = false;
                        _IsSendNewCmd = true;
                        enableUI = true;

                        if (cmd.GrpName != "")
                        {
                            msg = cmd.GrpName + "-失败 [@_@']";
                            ShowMsg(msg + "\r\n\r\n", Color.Red);
                        }
                        else
                        {
                            msg = cmd.Name + "-失败 [@_@']";
                            ShowMsg(msg + "\r\n\r\n", Color.Red);
                        }


                        while (_sendQueue.Count > 0)
                        {
                            Command ignoredCmd;
                            _sendQueue.TryDequeue(out ignoredCmd);
                        }

                        _strMsgBuf = "";
                        _strMsgMain = "";
                        _strMsgSub = "";
                        cmd.Name = "Idle状态";

                        if (enableUI)
                        {
                            UiOperateEnable();
                        }
                        else
                        {
                            UiOperateDisable();
                        }

                        if (_cmdEndCallback != null)
                        {
                            _cmdEndCallback(_argsEndCallback);
                            _cmdEndCallback = null;
                        }
                    }
                }

                // receive
                if (cmd != null && _recvQueue.Count > 0 && _recvQueue.TryDequeue(out cmd.RxBuf))
                {
                    cmd.RecvFunc(cmd);

                    if (cmd.IsEnable == false)
                    {
                        if (_sendQueue.Count == 0)
                        {
                            _strMsgBuf = "";
                            _strMsgMain = "";
                            _strMsgSub = "";
                            cmd.Name = "Idle状态";

                            UiOperateEnable();

                            if (_cmdEndCallback != null)
                            {
                                _cmdEndCallback(_argsEndCallback);
                                _cmdEndCallback = null;
                            }
                        }
                    }
                }

                // wait
                Thread.Sleep(50);

                timeWait = DateTime.Now - lastSendTime;
            }
        }
        #endregion

        #region 循环抄表
        private void btLoopCtrl_Click(object sender, EventArgs e)
        {

        }
        #endregion

        #region 命令按钮/菜单点击处理

        private void AllCmdButton_Click(object sender, EventArgs e)
        {
            AllCmdItem_Clicked(sender, null);
        }

        private void cbxRecentUseCmd_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxRecentUseCmd.SelectedIndex < 0) return;

            AllCmdItem_Clicked(sender, null);
        }
        private void AllCmdItem_Clicked(object sender, ToolStripItemClickedEventArgs e)
        {
            string cmdText = "";
            bool isParamCmd = true;

            if (sender is Button)
            {
                cmdText = ((Button)sender).Text;
            }
            else if (sender is ToolStripItem)
            {
                if (e != null)
                {
                    cmdText = e.ClickedItem.Text;
                }
                else
                {
                    cmdText = ((ToolStripItem)sender).Text;
                }
            }

            if(cmdText.Trim().Contains(" "))
            {
                cmdText = cmdText.Trim().Split(' ').Last();
            }

            HideParamCmdGrp();

            #region 带参数的命令-界面设置
            switch (cmdText)
            {
                case "数据转发":
                case "路由数据转发":
                case "读取计量箱相关事件":
                    lbParam1.Text = "协议类型";
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    cbxParam1.Items.Clear();
                    cbxParam1.Items.Add("0 透明传输");
                    cbxParam1.Items.Add("1 DL/T645-97");
                    cbxParam1.Items.Add("2 DL/T645-07");
                    cbxParam1.SelectedIndex = 0;
                    cbxParam1.Location = new Point(78, 27);
                    cbxParam1.Width = 111;
                    cbxParam1.Visible = true;
                    lbParam2.Text = "目标地址";
                    lbParam2.Location = new Point(22, 60);
                    lbParam2.Visible = true;
                    txtParam1.Location = new Point(78, 57);
                    txtParam1.Width = 111;
                    txtParam1.Visible = true;
                    lbParam3.Text = "报文内容：";
                    lbParam3.Location = new Point(22, 90);
                    lbParam3.Visible = true;
                    if(cmdText == "路由数据转发")
                    {
                        chkParam1.Text = "通信延时相关";
                        chkParam1.Location = new Point(95, 90);
                        chkParam1.Visible = true;
                    }
                    txtParam3.Location = new Point(24, 113);
                    txtParam3.Visible = true;
                    btParamConfirm.Location = new Point(24, 160);
                    break;

                case "查询厂商代码和版本信息":
                    lbParam1.Text = "节点类型";
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    rbtParam1.Text = "主节点";
                    rbtParam1.Location = new Point(78, 27);
                    rbtParam1.Width = 52;
                    rbtParam1.Visible = true;
                    rbtParam2.Text = "子节点";
                    rbtParam2.Location = new Point(132, 27);
                    rbtParam2.Width = 52;
                    rbtParam2.Visible = true;
                    lbParam2.Text = "目标地址";
                    lbParam2.Location = new Point(22, 60);
                    txtParam1.Location = new Point(78, 57);
                    txtParam1.Width = 111;
                    if (rbtParam2.Checked)
                    {
                        lbParam2.Visible = true;
                        txtParam1.Visible = true;
                    }
                    btParamConfirm.Location = new Point(24, 90);
                    break;

                case "查询从节点侦听信息":
                    lbParam1.Text = "起始序号(0-255)";
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    txtParam1.Text = "0";
                    txtParam1.Location = new Point(118, 27);
                    txtParam1.Width = 71;
                    txtParam1.Visible = true;
                    lbParam2.Text = "节点数量(1-16)";
                    lbParam2.Location = new Point(22, 60);
                    lbParam2.Visible = true;
                    txtParam2.Text = "16";
                    txtParam2.Location = new Point(118, 57);
                    txtParam2.Width = 71;
                    txtParam2.Visible = true;
                    btParamConfirm.Location = new Point(24, 90);
                    break;

                case "查询通信延时相关的广播时长":
                    lbParam1.Text = "协议类型";
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    cbxParam1.Items.Clear();
                    cbxParam1.Items.Add("0 透明传输");
                    cbxParam1.Items.Add("1 DL/T645-97");
                    cbxParam1.Items.Add("2 DL/T645-07");
                    cbxParam1.SelectedIndex = 0;
                    cbxParam1.Location = new Point(78, 27);
                    cbxParam1.Width = 111;
                    cbxParam1.Visible = true;
                    lbParam3.Text = "报文内容：";
                    lbParam3.Location = new Point(22, 60);
                    lbParam3.Visible = true;
                    txtParam3.Location = new Point(24, 57);
                    txtParam3.Visible = true;
                    btParamConfirm.Location = new Point(24, 110);
                    break;

                case "查询本地通信模块AFN索引":
                    lbParam1.Text = "AFN功能码：";
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    cbxParam1.Items.Clear();
                    cbxParam1.Items.Add("00H 确认/否认");
                    cbxParam1.Items.Add("01H 初始化");
                    cbxParam1.Items.Add("02H 数据转发");
                    cbxParam1.Items.Add("03H 查询数据");
                    cbxParam1.Items.Add("04H 链路接口测试");
                    cbxParam1.Items.Add("05H 控制命令");
                    cbxParam1.Items.Add("06H 主动上报");
                    cbxParam1.Items.Add("10H 路由查询");
                    cbxParam1.Items.Add("11H 路由设置");
                    cbxParam1.Items.Add("12H 路由控制");
                    cbxParam1.Items.Add("13H 路由数据转发");
                    cbxParam1.Items.Add("14H 路由数据抄读");
                    cbxParam1.Items.Add("15H 文件传输");
                    cbxParam1.Items.Add("20H 水表上报");
                    cbxParam1.Items.Add("F0H 内部调试");
                    cbxParam1.SelectedIndex = 0;
                    cbxParam1.Location = new Point(24, 57);
                    cbxParam1.Width = 165;
                    cbxParam1.Visible = true;
                    btParamConfirm.Location = new Point(24, 90);
                    break;

                case "发送测试":
                    lbParam1.Text = "持续时间(0-255秒)";
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    txtParam1.Text = "60";
                    txtParam1.Location = new Point(128, 27);
                    txtParam1.Width = 61;
                    txtParam1.Visible = true;
                    btParamConfirm.Location = new Point(24, 60);
                    break;

                case "本地通信模块报文通信测试":
                    lbParam1.Text = "通信速率";
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    cbxParam1.Items.Clear();
                    cbxParam1.Items.Add("0 自适应");
                    cbxParam1.Items.Add("1 1200");
                    cbxParam1.Items.Add("2 2400");
                    cbxParam1.Items.Add("3 4800");
                    cbxParam1.Items.Add("4 9600");
                    cbxParam1.Items.Add("5 19200");
                    cbxParam1.SelectedIndex = 0;
                    cbxParam1.Location = new Point(78, 27);
                    cbxParam1.Width = 111;
                    cbxParam1.Visible = true;
                    lbParam2.Text = "协议类型";
                    lbParam2.Location = new Point(22, 60);
                    lbParam2.Visible = true;
                    cbxParam2.Items.Clear();
                    cbxParam2.Items.Add("0 透明传输");
                    cbxParam2.Items.Add("1 DL/T645-97");
                    cbxParam2.Items.Add("2 DL/T645-07");
                    cbxParam2.SelectedIndex = 0;
                    cbxParam2.Location = new Point(78, 57);
                    cbxParam2.Width = 111;
                    cbxParam2.Visible = true;
                    lbParam4.Text = "目标地址";
                    lbParam4.Location = new Point(22, 90);
                    lbParam4.Visible = true;
                    txtParam1.Location = new Point(78, 87);
                    txtParam1.Width = 111;
                    txtParam1.Visible = true;
                    lbParam3.Text = "报文内容：";
                    lbParam3.Location = new Point(22, 117);
                    lbParam3.Visible = true;
                    txtParam3.Location = new Point(24, 140);
                    txtParam3.Visible = true;
                    btParamConfirm.Location = new Point(24, 190);
                    break;

                case "发射功率测试":
                    string chanel;
                    if (_currProtol.Contains("北网"))
                    {
                        chanel = "(0-65)";
                    }
                    else // 尼泊尔
                    {
                        chanel = "(0-23)";
                    }
                    lbParam1.Text = "信道索引" + chanel;
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    txtParam1.Text = "0";
                    txtParam1.Location = new Point(128, 27);
                    txtParam1.Width = 61;
                    txtParam1.Visible = true;
                    lbParam2.Text = "测试码流";
                    lbParam2.Location = new Point(22, 60);
                    lbParam2.Visible = true;
                    cbxParam2.Items.Clear();
                    cbxParam2.Items.Add("0 bit0无穷序列");
                    cbxParam2.Items.Add("1 bit1无穷序列");
                    cbxParam2.Items.Add("4 bit0/1交替序列");
                    cbxParam2.SelectedIndex = 2;
                    cbxParam2.Location = new Point(78, 57);
                    cbxParam2.Width = 111;
                    cbxParam2.Visible = true;
                    lbParam3.Text = "持续时间(0-255秒)";
                    lbParam3.Location = new Point(22, 90);
                    lbParam3.Visible = true;
                    txtParam2.Text = "16";
                    txtParam2.Location = new Point(128, 87);
                    txtParam2.Width = 61;
                    txtParam2.Visible = true;
                    btParamConfirm.Location = new Point(24, 120);
                    break;

                case "设置主节点地址":
                    lbParam1.Text = "节点地址";
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    txtParam1.Text = "";
                    txtParam1.Location = new Point(78, 27);
                    txtParam1.Width = 111;
                    txtParam1.Visible = true;
                    btParamConfirm.Location = new Point(24, 60);
                    break;

                case "允许/禁止从节点上报":
                    lbParam1.Text = "事件上报";
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    rbtParam1.Text = "允许";
                    rbtParam1.Location = new Point(88, 27);
                    rbtParam1.Width = 47;
                    rbtParam1.Visible = true;
                    rbtParam2.Text = "禁止";
                    rbtParam2.Location = new Point(142, 27);
                    rbtParam2.Width = 47;
                    rbtParam2.Visible = true;
                    btParamConfirm.Location = new Point(24, 60);
                    break;

                case "启动广播":
                    lbParam1.Text = "协议类型";
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    cbxParam1.Items.Clear();
                    cbxParam1.Items.Add("0 透明传输");
                    cbxParam1.Items.Add("1 DL/T645-97");
                    cbxParam1.Items.Add("2 DL/T645-07");
                    cbxParam1.SelectedIndex = 0;
                    cbxParam1.Location = new Point(78, 27);
                    cbxParam1.Width = 111;
                    cbxParam1.Visible = true;
                    lbParam3.Text = "报文内容：";
                    lbParam3.Location = new Point(22, 60);
                    lbParam3.Visible = true;
                    txtParam3.Location = new Point(24, 87);
                    txtParam3.Visible = true;
                    btParamConfirm.Location = new Point(24, 150);
                    break;

                case "设置从节点监控最大超时时间":
                    lbParam1.Text = "超时时间(0-255秒)";
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    txtParam1.Text = "60";
                    txtParam1.Location = new Point(128, 27);
                    txtParam1.Width = 61;
                    txtParam1.Visible = true;
                    btParamConfirm.Location = new Point(24, 60);
                    break;

                case "设置无线通信参数":
                    string[] chanels;
                    if (_currProtol.Contains("北网"))
                    {
                        chanels = new string[32];
                        for(int i = 0; i < 32; i++)
                        {
                            chanels[i] = (i + 1).ToString();
                        }
                    }
                    else // 尼泊尔
                    {
                        chanels = new string[11];
                        for (int i = 0; i < 11; i++)
                        {
                            chanels[i] = (i + 1).ToString();
                        }
                    }
                    lbParam1.Text = "信道组号";
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    cbxParam1.Items.Clear();
                    cbxParam1.Items.AddRange(chanels);
                    cbxParam1.SelectedIndex = 0;
                    cbxParam1.Location = new Point(78, 27);
                    cbxParam1.Width = 111;
                    cbxParam1.Visible = true;
                    lbParam2.Text = "发射功率";
                    lbParam2.Location = new Point(22, 60);
                    lbParam2.Visible = true;
                    cbxParam2.Items.Clear();
                    cbxParam2.Items.Add("0 最高");
                    cbxParam2.Items.Add("1 次高");
                    cbxParam2.Items.Add("2 次低");
                    cbxParam2.Items.Add("3 最低");
                    cbxParam2.Items.Add("4 全功率");
                    cbxParam2.SelectedIndex = 0;
                    cbxParam2.Location = new Point(78, 57);
                    cbxParam2.Width = 111;
                    cbxParam2.Visible = true;
                    btParamConfirm.Location = new Point(24, 90);
                    break;

                case "设置场强门限":
                    lbParam1.Text = "场强门限(50-120)";
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    txtParam1.Text = "96";
                    txtParam1.Location = new Point(128, 27);
                    txtParam1.Width = 61;
                    txtParam1.Visible = true;
                    btParamConfirm.Location = new Point(24, 60);
                    break;

                case "查询从节点信息":
                case "查询未抄读成功的从节点信息":
                case "查询主动注册的从节点信息":
                    lbParam1.Text = "起始序号(0-512)";
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    txtParam1.Text = "0";
                    txtParam1.Location = new Point(128, 27);
                    txtParam1.Width = 61;
                    txtParam1.Visible = true;
                    lbParam2.Text = "节点数量(0-30)";
                    lbParam2.Location = new Point(22, 60);
                    lbParam2.Visible = true;
                    txtParam2.Text = "20";
                    txtParam2.Location = new Point(128, 57);
                    txtParam2.Width = 61;
                    txtParam2.Visible = true;
                    btParamConfirm.Location = new Point(24, 90);
                    break;

                case "查询从节点的上一级路由信息":
                case "查询无线从节点的中继路由信息":
                    lbParam1.Text = "从节点地址";
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    txtParam1.Text = "";
                    txtParam1.Location = new Point(88, 27);
                    txtParam1.Width = 101;
                    txtParam1.Visible = true;
                    btParamConfirm.Location = new Point(24, 60);
                    break;

                case "查询在网状态更新信息":
                    lbParam1.Text = "起始序号(0-512)";
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    txtParam1.Text = "0";
                    txtParam1.Location = new Point(128, 27);
                    txtParam1.Width = 61;
                    txtParam1.Visible = true;
                    lbParam2.Text = "节点数量(0-30)";
                    lbParam2.Location = new Point(22, 60);
                    lbParam2.Visible = true;
                    txtParam2.Text = "20";
                    txtParam2.Location = new Point(128, 57);
                    txtParam2.Width = 61;
                    txtParam2.Visible = true;
                    lbParam3.Text = "节点类型";
                    lbParam3.Location = new Point(22, 90);
                    lbParam3.Visible = true;
                    cbxParam1.Items.Clear();
                    cbxParam1.Items.Add("0 全网");
                    cbxParam1.Items.Add("1 在网");
                    cbxParam1.Items.Add("2 离网");
                    cbxParam1.SelectedIndex = 0;
                    cbxParam1.Location = new Point(128, 87);
                    cbxParam1.Width = 61;
                    cbxParam1.Visible = true;
                    btParamConfirm.Location = new Point(24, 120);
                    break;

                case "添加从节点":
                    lbParam1.Text = "协议类型";
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    cbxParam1.Items.Clear();
                    cbxParam1.Items.Add("0 透明传输");
                    cbxParam1.Items.Add("1 DL/T645-97");
                    cbxParam1.Items.Add("2 DL/T645-07");
                    cbxParam1.Items.Add("3 单向水表");
                    cbxParam1.Items.Add("7 DL/T698");
                    cbxParam1.SelectedIndex = 0;
                    cbxParam1.Location = new Point(78, 27);
                    cbxParam1.Width = 111;
                    cbxParam1.Visible = true;
                    lbParam2.Text = "起始地址";
                    lbParam2.Location = new Point(22, 60);
                    lbParam2.Visible = true;
                    txtParam2.Location = new Point(78, 57);
                    txtParam2.Width = 111;
                    txtParam2.Visible = true;
                    lbParam3.Text = "添加数量";
                    lbParam3.Location = new Point(22, 90);
                    lbParam3.Visible = true;
                    txtParam1.Location = new Point(78, 87);
                    txtParam1.Width = 111;
                    txtParam1.Visible = true;
                    btParamConfirm.Location = new Point(24, 120);
                    break;

                case "设置从节点固定中继路径":
                    lbParam1.Text = "从节点地址";
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    txtParam1.Location = new Point(88, 27);
                    txtParam1.Width = 101;
                    txtParam1.Visible = true;
                    lbParam3.Text = "1-N级中继地址(空格分隔)：";
                    lbParam3.Location = new Point(22, 60);
                    lbParam3.Visible = true;
                    txtParam3.Location = new Point(24, 87);
                    txtParam3.Visible = true;
                    btParamConfirm.Location = new Point(24, 150);
                    break;

                case "设置路由工作模式":
                    lbParam1.Text = "路由工作模式";
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    cbxParam1.Items.Clear();
                    cbxParam1.Items.Add("0 学习");
                    cbxParam1.Items.Add("1 抄表");
                    cbxParam1.SelectedIndex = 1;
                    cbxParam1.Location = new Point(118, 27);
                    cbxParam1.Width = 71;
                    cbxParam1.Visible = true;
                    lbParam2.Text = "注册允许状态";
                    lbParam2.Location = new Point(22, 60);
                    lbParam2.Visible = true;
                    cbxParam2.Items.Clear();
                    cbxParam2.Items.Add("0 禁止");
                    cbxParam2.Items.Add("1 允许");
                    cbxParam2.SelectedIndex = 0;
                    cbxParam2.Location = new Point(118, 57);
                    cbxParam2.Width = 71;
                    cbxParam2.Visible = true;
                    lbParam3.Text = "通信速率(bit/s)";
                    lbParam3.Location = new Point(22, 90);
                    lbParam3.Visible = true;
                    txtParam1.Text = "9600";
                    txtParam1.Location = new Point(118, 87);
                    txtParam1.Width = 71;
                    txtParam1.Visible = true;
                    btParamConfirm.Location = new Point(24, 120);
                    break;

                case "激活从节点主动注册":
                    lbParam4.Text = "开始时间(分钟，发送时设置)";
                    lbParam4.Location = new Point(22, 30);
                    lbParam4.Visible = true;
                    lbParam1.Text = "持续时间(分钟)";
                    lbParam1.Location = new Point(22, 60);
                    lbParam1.Visible = true;
                    txtParam1.Text = "20";
                    txtParam1.Location = new Point(118, 57);
                    txtParam1.Width = 71;
                    txtParam1.Visible = true;
                    lbParam2.Text = "从节点重发次数";
                    lbParam2.Location = new Point(22, 90);
                    lbParam2.Visible = true;
                    cbxParam1.Items.Clear();
                    cbxParam1.Items.Add("1");
                    cbxParam1.Items.Add("2");
                    cbxParam1.Items.Add("3");
                    cbxParam1.Items.Add("4");
                    cbxParam1.Items.Add("5");
                    cbxParam1.SelectedIndex = 0;
                    cbxParam1.Location = new Point(118, 87);
                    cbxParam1.Width = 71;
                    cbxParam1.Visible = true;
                    lbParam3.Text = "等待时间片个数";
                    lbParam3.Location = new Point(22, 120);
                    lbParam3.Visible = true;
                    txtParam2.Text = "5";
                    txtParam2.Location = new Point(118, 117);
                    txtParam2.Width = 71;
                    txtParam2.Visible = true;
                    btParamConfirm.Location = new Point(24, 150);
                    break;

                case "设置网络规模":
                    lbParam1.Text = "网络规模(2-512)";
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    txtParam1.Text = "255";
                    txtParam1.Location = new Point(118, 27);
                    txtParam1.Width = 71;
                    txtParam1.Visible = true;
                    btParamConfirm.Location = new Point(24, 60);
                    break;

                case "文件传输":
                    lbParam1.Text = "文件标识：";
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    cbxParam1.Items.Clear();
                    cbxParam1.Items.Add("0 清除下装文件");
                    cbxParam1.Items.Add("3 主节点模块升级");
                    cbxParam1.Items.Add("7 主节点和子节点模块升级");
                    cbxParam1.Items.Add("8 子节点模块升级");
                    cbxParam1.SelectedIndex = 1;
                    cbxParam1.Location = new Point(24, 57);
                    cbxParam1.Width = 165;
                    cbxParam1.Visible = true;
                    btParamConfirm.Location = new Point(24, 90);
                    break;

                case "按类型读取日志":
                    lbParam1.Text = "读取类型";
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    cbxParam1.Items.Clear();
                    cbxParam1.Items.Add("0 时-日志");
                    cbxParam1.Items.Add("1 日-日志");
                    cbxParam1.Items.Add("2 月-日志");
                    cbxParam1.SelectedIndex = 0;
                    cbxParam1.Location = new Point(78, 27);
                    cbxParam1.Width = 111;
                    cbxParam1.Visible = true;
                    lbParam2.Text = "读取时间(月/日 时)";
                    lbParam2.Location = new Point(22, 60);
                    lbParam2.Visible = true;
                    txtParam2.Text = DateTime.Now.ToString("MM/dd HH");
                    txtParam2.Location = new Point(133, 57);
                    txtParam2.Width = 56;
                    txtParam2.Visible = true;
                    btParamConfirm.Location = new Point(24, 90);
                    break;

                case "设置广播维护开关":
                    lbParam1.Text = "维护开关";
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    rbtParam1.Text = "开启";
                    rbtParam1.Location = new Point(88, 27);
                    rbtParam1.Width = 47;
                    rbtParam1.Visible = true;
                    rbtParam2.Text = "关闭";
                    rbtParam2.Location = new Point(142, 27);
                    rbtParam2.Width = 47;
                    rbtParam2.Visible = true;
                    lbParam2.Text = "维护时间(时:分)";
                    lbParam2.Location = new Point(22, 60);
                    lbParam2.Visible = true;
                    txtParam2.Text = DateTime.Now.AddHours(1).ToString("HH:mm");
                    txtParam2.Location = new Point(130, 57);
                    txtParam2.Width = 59;
                    txtParam2.Visible = true;
                    btParamConfirm.Location = new Point(24, 90);
                    break;

                case "读取子节点参数信息":
                    lbParam1.Text = "读取类型";
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    cbxParam1.Items.Clear();
                    cbxParam1.Items.Add("0 档案信息");
                    cbxParam1.Items.Add("1 邻居表");
                    cbxParam1.Items.Add("2 路径表");
                    cbxParam1.SelectedIndex = 0;
                    cbxParam1.Location = new Point(78, 27);
                    cbxParam1.Width = 111;
                    cbxParam1.Visible = true;
                    lbParam2.Text = "节点地址";
                    lbParam2.Location = new Point(22, 60);
                    lbParam2.Visible = true;
                    txtParam1.Location = new Point(78, 57);
                    txtParam1.Width = 111;
                    txtParam1.Visible = true;
                    btParamConfirm.Location = new Point(24, 90);
                    break;

                default:
                    SendToCmdQueue(cmdText);
                    isParamCmd = false;
                    break;
            }
            #endregion

            if (isParamCmd)
            {
                grpParamCmd.Text = cmdText;
                grpParamCmd.Visible = true;
            }

            UpdateRecentCmdName(cmdText);

        }

        private void LoadRecentCmdName()
        {
            string recentCmds = XmlHelper.GetNodeValue(_configPath, "/Config/ConcSimulator_North_RecentCmds");
            if (recentCmds == "")
            {
                return;
            }

            string[] strs = recentCmds.Split(new string[]{";"}, StringSplitOptions.RemoveEmptyEntries);
            cbxRecentUseCmd.Items.Clear();
            cbxRecentUseCmd.Items.AddRange(strs);

            XmlHelper.SetNodeValue(_configPath, "/Config", "ConcSimulator_North_RecentCmds", recentCmds);
        }
        private void UpdateRecentCmdName(string cmdText = "")
        {
            int index = cbxRecentUseCmd.Items.IndexOf(cmdText);
            string recentCmds = "";

            if (index >= 0)
            {
                cbxRecentUseCmd.Items.RemoveAt(index);
            }

            if (cmdText != "")
            {
                cbxRecentUseCmd.Items.Insert(0, cmdText);
            }
            else
            {
                recentCmds = XmlHelper.GetNodeValue(_configPath, "/Config/ConcSimulator_North_RecentCmds");
                string[] strs = recentCmds.Split(';');
                cbxRecentUseCmd.Items.Clear();
                cbxRecentUseCmd.Items.AddRange(strs);
            }

            for (int i = cbxRecentUseCmd.Items.Count; cbxRecentUseCmd.Items.Count > 15; i--)
            {
                cbxRecentUseCmd.Items.RemoveAt(i - 1);
            }

            foreach (object str in cbxRecentUseCmd.Items)
            {
                recentCmds += str + ";";
            }

            XmlHelper.SetNodeValue(_configPath, "/Config", "ConcSimulator_North_RecentCmds", recentCmds);
        }
        #endregion

        #region 带参数命令控件处理 - 隐藏、输入过滤、确认提交
        private void HideParamCmdGrp()
        {
            lbParam1.Visible = false;
            lbParam2.Visible = false;
            lbParam3.Visible = false;
            lbParam4.Visible = false;
            txtParam1.Visible = false;
            txtParam2.Visible = false;
            txtParam3.Visible = false;
            rbtParam1.Visible = false;
            rbtParam2.Visible = false;
            cbxParam1.Visible = false;
            cbxParam2.Visible = false;
            chkParam1.Visible = false;
            grpParamCmd.Visible = false;

            txtParam1.Text = "";
            txtParam2.Text = "";
            txtParam3.Text = "";
            cbxParam1.Text = "";
            cbxParam2.Text = "";

            this.Refresh();
        }
        
        private void txtParam1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ("0123456789\b\r\x03\x16\x18".IndexOf(e.KeyChar) < 0)
            {
                e.Handled = true;
                return;
            }

            if (e.KeyChar == '\r')
            {
                txtParam1.Text = txtParam1.Text.PadLeft(12, '0');
                e.Handled = true;
            }
            if (txtParam1.Text.Length >= 12 && e.KeyChar != '\b')
            {
                if (txtParam1.SelectionLength == 0)
                {
                    e.Handled = true;
                }
            }
        }

        private void txtParam2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ("0123456789\b\r\x03\x16\x18".IndexOf(e.KeyChar) < 0)
            {
                e.Handled = true;
                return;
            }

            if (e.KeyChar == '\r')
            {
                txtParam2.Text = txtParam2.Text.PadLeft(12, '0');
                e.Handled = true;
            }
            if (txtParam2.Text.Length >= 12 && e.KeyChar != '\b')
            {
                if (txtParam2.SelectionLength == 0)
                {
                    e.Handled = true;
                }
            }
        }

        private void txtParam3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ("0123456789abcdef\b\r\x03\x16\x18\x20".IndexOf(e.KeyChar) < 0)
            {
                e.Handled = true;
                return;
            }
        }

        private void rbtParam_CheckedChanged(object sender, EventArgs e)
        {
            if(((RadioButton)sender).Text == "主节点")
            {
                lbParam2.Visible = false;
                txtParam1.Visible = false;
            }
            else if (((RadioButton)sender).Text == "子节点")
            {
                lbParam2.Visible = true;
                txtParam1.Visible = true;
            }
        }

        private void btParamConfirm_Click(object sender, EventArgs e)
        {
            string cmdText = grpParamCmd.Text;

            try
            {
                SendToCmdQueue(cmdText);
            }
            catch (Exception ex) 
            {
                ShowMsg("SendToCmdQueue() error : "  + ex.Message, Color.Red);
            }
            
        }
        #endregion

        #region 命令发送
        private void SendToCmdQueue(string cmdName)
        {
            int txLen = 0, intTemp;
            string strTemp = "";
            Command cmd = new Command(cmdName);

            cmd.SendFunc = SendCmd;
            cmd.RecvFunc = RecvCmd;
            cmd.TimeWaitMS = 1000;
            cmd.RetryTimes = 1;

            TempBuf[txLen++] = 0x68;    // start
            TempBuf[txLen++] = 0;       // length skip
            TempBuf[txLen++] = 0;
            TempBuf[txLen++] = 0x4A;    // ctrl
            TempBuf[txLen++] = 0;       // info-1~5
            TempBuf[txLen++] = 0;
            TempBuf[txLen++] = 0;
            TempBuf[txLen++] = 0;
            TempBuf[txLen++] = 0;
            TempBuf[txLen++] = _fsn++;  // info-6 

            // 参数检查
            if ((txtParam1.Visible && string.IsNullOrWhiteSpace(txtParam1.Text))
                || (txtParam2.Visible && string.IsNullOrWhiteSpace(txtParam2.Text))
                || (txtParam3.Visible && string.IsNullOrWhiteSpace(txtParam3.Text)))
            {
                ShowMsg("请输入所需的参数后，再按确认键\r\n", Color.Red);
                return;
            }

            // addr
            if(cmd.Name == "数据转发" 
                || cmd.Name == "读取计量箱相关事件" 
                || cmd.Name == "路由数据转发"
                || (cmd.Name == "查询厂商代码和版本信息" && rbtParam2.Checked))
            {
                TempBuf[4] = 0x04;      // info-1 
                txtParam1.Text = txtParam1.Text.PadLeft(12, '0');
                Util.GetBytesFromStringHex(_strCenterAddr, TempBuf, txLen, true); // 源地址
                txLen += 6;
                Util.GetBytesFromStringHex(txtParam1.Text, TempBuf, txLen, true); // 目的地址
                txLen += 6;
            }

            // AFN - Fn
            byte afn;
            UInt16 fnId;
            ProtoLocal_North.GetAfnFnByName(cmd.Name, out afn, out fnId);
            TempBuf[txLen++] = afn;                         // AFN
            TempBuf[txLen++] = (byte)(fnId & 0xFF);         // Fn
            TempBuf[txLen++] = (byte)(fnId >> 8);

            #region 不带参数命令处理
            switch(cmd.Name)
            {
                case "查询微功率无线从节点信息":
                    _docCnt = 0;
                    tbDoc.Rows.Clear();
                    break;

                default:
                    break;
            }
            #endregion

            #region 带参数的命令处理--填充数据
            switch (cmd.Name)
            {
                case "数据转发":
                case "路由数据转发":
                case "读取计量箱相关事件":
                    TempBuf[txLen++] = Convert.ToByte(cbxParam1.Text.Split(' ')[0]);    // 协议类型
                    if (cmd.Name == "路由数据转发")
                    {
                        TempBuf[txLen++] = (byte)(chkParam1.Checked ? 1 : 0);           // 延时相关
                    }
                    TempBuf[txLen++] = 0x00;                                            // 报文长度
                    intTemp = Util.GetBytesFromStringHex(txtParam3.Text.Trim(), TempBuf, txLen);  // 报文内容
                    TempBuf[txLen -1] = (byte)intTemp;
                    txLen += intTemp;
                    break;

                case "查询从节点侦听信息":
                    TempBuf[txLen++] = Convert.ToByte(txtParam1.Text.Trim());       // "起始序号(0-255)";
                    TempBuf[txLen++] = Convert.ToByte(txtParam2.Text.Trim());       // "节点数量(1-16)";
                    break;

                case "查询通信延时相关的广播时长":
                    TempBuf[txLen++] = Convert.ToByte(cbxParam1.Text.Split(' ')[0]);    // 协议类型
                    TempBuf[txLen++] = 0x00;                                            // 报文长度
                    intTemp = Util.GetBytesFromStringHex(txtParam3.Text.Trim(), TempBuf, txLen);  // 报文内容
                    TempBuf[txLen -1] = (byte)intTemp;
                    txLen += intTemp;
                    break;

                case "查询本地通信模块AFN索引":
                    TempBuf[txLen++] = Convert.ToByte(cbxParam1.Text.Substring(0,2), 16);   // "AFN功能码：";
                    break;

                case "发送测试":
                    TempBuf[txLen++] = Convert.ToByte(txtParam1.Text.Trim());       // "持续时间(0-255秒)";
                    break;

                case "本地通信模块报文通信测试":
                    TempBuf[txLen++] = Convert.ToByte(cbxParam1.Text.Split(' ')[0]);    // "通信速率";
                    txtParam1.Text = txtParam1.Text.PadLeft(12, '0');
                    txLen += Util.GetBytesFromStringHex(txtParam1.Text, TempBuf, txLen, true);  // 目的地址
                    TempBuf[txLen++] = Convert.ToByte(cbxParam2.Text.Split(' ')[0]);    // "协议类型";
                    TempBuf[txLen++] = 0x00;                                            // 报文长度
                    intTemp = Util.GetBytesFromStringHex(txtParam3.Text.Trim(), TempBuf, txLen);  // 报文内容
                    TempBuf[txLen -1] = (byte)intTemp;
                    txLen += intTemp;
                    break;

                case "发射功率测试":
                    TempBuf[txLen++] = Convert.ToByte(txtParam1.Text.Trim());           // "信道索引" 
                    TempBuf[txLen++] = Convert.ToByte(cbxParam2.Text.Split(' ')[0]);    // "测试码流";
                    TempBuf[txLen++] = Convert.ToByte(txtParam2.Text.Trim());       // "持续时间(0-255秒)";
                    break;

                case "设置主节点地址":
                    txtParam1.Text = txtParam1.Text.PadLeft(12, '0');
                    txLen += Util.GetBytesFromStringHex(txtParam1.Text, TempBuf, txLen, true);   // 节点地址
                    break;

                case "允许/禁止从节点上报":
                    TempBuf[txLen++] = Convert.ToByte(rbtParam1.Checked ? 1 : 0);       //"事件上报"-允许/禁止;
                    break;

                case "启动广播":
                    TempBuf[txLen++] = Convert.ToByte(cbxParam1.Text.Split(' ')[0]);    // "协议类型";
                    TempBuf[txLen++] = 0x00;                                            // 报文长度
                    intTemp = Util.GetBytesFromStringHex(txtParam3.Text.Trim(), TempBuf, txLen);  // 报文内容
                    TempBuf[txLen -1] = (byte)intTemp;
                    txLen += intTemp;
                    break;

                case "设置从节点监控最大超时时间":
                    TempBuf[txLen++] = Convert.ToByte(txtParam1.Text.Trim());           // "超时时间(0-255秒)";
                    break;

                case "设置无线通信参数":
                    TempBuf[txLen++] = Convert.ToByte(cbxParam1.Text.Split(' ')[0]);    // "信道组号";
                    TempBuf[txLen++] = Convert.ToByte(cbxParam2.Text.Split(' ')[0]);    // "发射功率";
                    break;

                case "设置场强门限":
                    TempBuf[txLen++] = Convert.ToByte(txtParam1.Text.Trim());           // "场强门限(50-120)";
                    break;

                case "设置中心节点时间":
                    TempBuf[txLen++] = Util.DecToBcd((byte)DateTime.Now.Second);        // "用当前时间设置";
                    TempBuf[txLen++] = Util.DecToBcd((byte)DateTime.Now.Minute);
                    TempBuf[txLen++] = Util.DecToBcd((byte)DateTime.Now.Hour);
                    TempBuf[txLen++] = Util.DecToBcd((byte)DateTime.Now.Day);
                    TempBuf[txLen++] = Util.DecToBcd((byte)DateTime.Now.Month);
                    TempBuf[txLen++] = Util.DecToBcd((byte)(DateTime.Now.Year % 100));
                    break;

                case "查询从节点信息":
                case "查询未抄读成功的从节点信息":
                case "查询主动注册的从节点信息":
                    intTemp = Convert.ToInt32(txtParam1.Text.Trim());                    // "起始序号(0-512)";
                    TempBuf[txLen++] = (byte)(intTemp & 0xFF);
                    TempBuf[txLen++] = (byte)((intTemp >> 8) & 0xFF);
                    TempBuf[txLen++] = Convert.ToByte(txtParam2.Text.Trim());           // "节点数量(0-30)";
                    break;

                case "查询从节点的上一级路由信息":
                case "查询无线从节点的中继路由信息":
                    txtParam1.Text = txtParam1.Text.PadLeft(12, '0');
                    txLen += Util.GetBytesFromStringHex(txtParam1.Text, TempBuf, txLen, true);   // 从节点地址
                    break;

                case "查询在网状态更新信息":
                    intTemp = Convert.ToInt32(txtParam1.Text.Trim());                    // "起始序号(0-512)";
                    TempBuf[txLen++] = (byte)(intTemp & 0xFF);
                    TempBuf[txLen++] = (byte)((intTemp >> 8) & 0xFF);
                    TempBuf[txLen++] = Convert.ToByte(txtParam2.Text.Trim());           // "节点数量(0-30)";
                    TempBuf[txLen++] = Convert.ToByte(cbxParam1.Text.Split(' ')[0]);    // "节点类型";
                    break;

                case "添加从节点":
                    byte proto = Convert.ToByte(cbxParam1.Text.Split(' ')[0]);          // "协议类型";
                    ulong addrBegin = Convert.ToUInt64(txtParam2.Text.Trim());          // "起始地址";
                    ushort cnt = Convert.ToUInt16(txtParam1.Text.Trim());               // "添加数量";

                    txtParam2.Text = txtParam2.Text.Trim().PadLeft(12, '0');

                    TempBuf[txLen++] = (byte)(cnt > 20 ? 20 : cnt);     //节点数量
                    for (int i = 0; i < cnt && i < 20; i++ )
                    {
                        strTemp = addrBegin.ToString().PadLeft(12, '0');
                        txLen += Util.GetBytesFromStringHex(strTemp, TempBuf, txLen, true);   // 节点地址
                        TempBuf[txLen++] = proto;                       //节点协议类型
                        addrBegin++;
                    }
                    break;

                case "删除从节点":
                    intTemp = 0;        // todo 设置要删除的地址数
                    TempBuf[txLen++] = (byte)(intTemp > 20 ? 20 : intTemp);     //节点数量
                    for (int i = 0; i < intTemp && i < 20; i++)
                    {
                        strTemp = "";   // todo 取出要删除的地址
                        txLen += Util.GetBytesFromStringHex(strTemp, TempBuf, txLen, true);   // 节点地址
                    }
                    break;

                case "设置从节点固定中继路径":
                    if (string.IsNullOrWhiteSpace(txtParam1.Text)
                        || string.IsNullOrWhiteSpace(txtParam3.Text))
                    {
                        ShowMsg("请输入所需的参数后，再按确认键\r\n", Color.Red);
                        return;
                    }
                    txtParam1.Text = txtParam1.Text.PadLeft(12, '0');
                    txLen += Util.GetBytesFromStringHex(txtParam1.Text, TempBuf, txLen, true);   // 从节点地址

                    string[] strs = txtParam3.Text.Trim().Split(' ');                   // "1-N级中继地址(空格分隔)：";
                    TempBuf[txLen++] = Convert.ToByte(strs.Length);                     // "中继级别";
                    for (int i = 0; i < strs.Length; i++)
                    {
                        strs[i] = strs[i].PadLeft(12, '0');
                        txLen += Util.GetBytesFromStringHex(strs[i], TempBuf, txLen, true);   // 中继地址
                    }
                    break;

                case "设置路由工作模式":
                    byte bit0 = (byte)(cbxParam1.Text.Split(' ')[0] == "1" ? 0x01 : 0x00);  // "路由工作模式";
                    byte bit1 = (byte)(cbxParam2.Text.Split(' ')[0] == "1" ? 0x02 : 0x00);  // "注册允许状态";
                    TempBuf[txLen++] = (byte)(bit0 | bit1);
                    ushort baud = Convert.ToUInt16(txtParam1.Text.Trim());                  // "通信速率(bit/s)";
                    TempBuf[txLen++] = (byte)(baud & 0xFF);
                    TempBuf[txLen++] = (byte)(baud >> 8);
                    break;

                case "激活从节点主动注册":
                    TempBuf[txLen++] = Util.DecToBcd((byte)DateTime.Now.Second);            // "开始时间(分钟，发送时设置)";
                    TempBuf[txLen++] = Util.DecToBcd((byte)DateTime.Now.Minute);
                    TempBuf[txLen++] = Util.DecToBcd((byte)DateTime.Now.Hour);
                    TempBuf[txLen++] = Util.DecToBcd((byte)DateTime.Now.Day);
                    TempBuf[txLen++] = Util.DecToBcd((byte)DateTime.Now.Month);
                    TempBuf[txLen++] = Util.DecToBcd((byte)(DateTime.Now.Year % 100));
                    TempBuf[txLen++] = Convert.ToByte(txtParam1.Text.Trim());               // "持续时间(分钟)";
                    TempBuf[txLen++] = Convert.ToByte(cbxParam1.Text.Split(' ')[0]);        // "从节点重发次数";
                    TempBuf[txLen++] = Convert.ToByte(txtParam2.Text.Trim());               // "等待时间片个数";
                    break;

                case "设置网络规模":
                    intTemp = Convert.ToInt32(txtParam1.Text.Trim());                       // "网络规模(2-512)";
                    TempBuf[txLen++] = (byte)(intTemp & 0xFF);
                    TempBuf[txLen++] = (byte)((intTemp >> 8) & 0xFF);
                    break;

                case "文件传输":
                    byte fileFlg = Convert.ToByte(cbxParam1.Text.Split(' ')[0]);            // "文件标识：";

                    switch (fileFlg)
                    {
                        case 0:     // ("0 清除下装文件");
                            TempBuf[txLen++] = fileFlg;
                            break;

                        case 3:     // ("3 主节点模块升级");
                        case 7:     // ("7 主节点和子节点模块升级");
                        case 8:     // ("8 子节点模块升级");
                            string info = "";
                            openFileDlg.Filter = "Bin文件|*.bin";
                            openFileDlg.DefaultExt = ".bin";
                            if(openFileDlg.ShowDialog() != DialogResult.OK)
                            {
                                return;
                            }

                            _upgradeFile = new FilePacket(openFileDlg.FileName, 128);

                            if (fileFlg == 3)
                            {
                                info = _upgradeFile.GetStringFromDataBuffer("SRWF-", "", FilePacket.FindMode.End, 3072);
                            }
                            else if (fileFlg == 8)
                            {
                                info = _upgradeFile.GetStringFromDataBuffer("SRWF-", "", FilePacket.FindMode.Begin, 1024);
                            }
                            else
                            {
                                info = _upgradeFile.GetStringFromDataBuffer("SRWF-", "", FilePacket.FindMode.Begin, _upgradeFile.FileSize);
                            }

                            if (info != "")
                            {
                                ShowMsg("文件版本信息：" + info + "\r\n", Color.Green);
                            }

                            break;

                        default:

                            break;
                    }
                    break;

                case "按类型读取日志":
                    TempBuf[txLen++] = Convert.ToByte(cbxParam1.Text.Split(' ')[0]);    // "读取类型";
                    string strTime = txtParam2.Text.Trim();                             // "读取时间(月/日 时)";
                    DateTime time;
                    try
                    {
                        time = DateTime.ParseExact(strTime, "MM/dd HH", Thread.CurrentThread.CurrentCulture);
                    }
                    catch(Exception)
                    {
                        ShowMsg("输入的时间格式错误！如5月20日21时，则输入 05/20 21 \r\n", Color.Red);
                        return;
                    }
                    TempBuf[txLen++] = Convert.ToByte(time.Month);
                    TempBuf[txLen++] = Convert.ToByte(time.Day);
                    TempBuf[txLen++] = Convert.ToByte(time.Hour);
                    break;

                case "设置广播维护开关":
                    TempBuf[txLen++] = (byte)(rbtParam1.Checked ? 1 : 0);               // "维护开关";
                    TempBuf[txLen++] = Util.DecToBcd((byte)DateTime.Now.Second);        // "开始时间(分钟，发送时设置)";
                    TempBuf[txLen++] = Util.DecToBcd((byte)DateTime.Now.Minute);
                    TempBuf[txLen++] = Util.DecToBcd((byte)DateTime.Now.Hour);
                    TempBuf[txLen++] = Util.DecToBcd((byte)DateTime.Now.Day);
                    TempBuf[txLen++] = Util.DecToBcd((byte)DateTime.Now.Month);
                    TempBuf[txLen++] = Util.DecToBcd((byte)(DateTime.Now.Year % 100));
                    string strTime2 = txtParam2.Text.Trim();                            // "维护时间(时:分)";
                    DateTime time2;
                    try
                    {
                        time2 = DateTime.ParseExact(strTime2, "HH:mm", Thread.CurrentThread.CurrentCulture);
                    }
                    catch (Exception)
                    {
                        ShowMsg("输入的时间格式错误！如22时30分，则输入 22;30 \r\n", Color.Red);
                        return;
                    }
                    TempBuf[txLen++] = Convert.ToByte(time2.Hour);
                    TempBuf[txLen++] = Convert.ToByte(time2.Minute);
                    break;

                case "读取子节点参数信息":
                    TempBuf[txLen++] = Convert.ToByte(cbxParam1.Text.Split(' ')[0]);    // "读取类型";
                    txtParam1.Text = txtParam1.Text.PadLeft(12, '0');
                    txLen += Util.GetBytesFromStringHex(txtParam1.Text, TempBuf, txLen, true);   // 节点地址
                    break;

                default:// 无法识别的命令
                    //cmd = null;
                    break;
            }
            #endregion

            if (cmd == null) return;

            byte sum = 0;
            for (int i = 3; i < txLen; i++ )
            {
                sum += TempBuf[i];
            }

            TempBuf[txLen++] = sum;                 // checksum
            TempBuf[txLen++] = 0x16;                // end

            TempBuf[1] = (byte)(txLen & 0xFF);      // length set
            TempBuf[2] = (byte)(txLen >> 8);   

            cmd.TxBuf = new byte[txLen];
            Array.Copy(TempBuf, 0, cmd.TxBuf, 0, txLen);
            _sendQueue.Enqueue(cmd);

            _IsSendNewCmd = true;
        }

        #endregion

        FilePacket _upgradeFile;

        #region 读取升级文件的版本信息
        /// <summary>
        /// 读取升级文件的版本信息
        /// </summary>
        /// <param name="filename">升级文件名</param>
        /// <param name="fileFlg">文件标识：3 - 主模块升级文件， 8 - 子模块升级文件， 7 - 主模块或子模块升级文件</param>
        /// <returns>string 文件版本信息</returns>

        /*
    private string ReadVersionInfoFromUpgradeFile(string filename, byte fileFlg = 7)
    {
        byte[] first1k = new byte[1024];
        byte[] last3k = new byte[3072];
        byte[] infoHeader = new byte[] { 0x53, 0x52, 0x57, 0x46, 0x2d }; // "SRWF-"
        string info = "";
        int indexStart = -1, indexEnd = -1;

        if (fileFlg == 3)   // 主模块bin文件 尾部3k查找版本信息
        {
            FileStream fs = File.OpenRead(openFileDlg.FileName);
            fs.Seek(-last3k.Length, SeekOrigin.End);
            fs.Read(last3k, 0, last3k.Length);
            fs.Close();

            indexStart = Util.IndexOf(last3k, infoHeader);

            if(indexStart >= 0)
                indexEnd = Util.IndexOf(last3k, 0x00, indexStart);

            if(indexEnd >= 0)
                info = Encoding.ASCII.GetString(last3k, indexStart, (indexEnd - indexStart));
        }
        else if (fileFlg == 8) // 子模块bin文件 头部1k查找版本信息
        {
            FileStream fs = File.OpenRead(openFileDlg.FileName);
            fs.Read(first1k, 0, first1k.Length);
            fs.Close();

            indexStart = Util.IndexOf(first1k, infoHeader);

            if (indexStart >= 0)
                indexEnd = Util.IndexOf(first1k, 0x00, indexStart);

            if (indexEnd >= 0)
                info = Encoding.ASCII.GetString(first1k, indexStart, (indexEnd - indexStart));
        }
        else if (fileFlg == 7) // 主模块 或 子模块bin文件
        {
            FileStream fs = File.OpenRead(openFileDlg.FileName);
            fs.Read(first1k, 0, first1k.Length);
            fs.Seek(-last3k.Length, SeekOrigin.End);
            fs.Read(last3k, 0, last3k.Length);
            fs.Close();

            indexStart = Util.IndexOf(first1k, infoHeader);

            if (indexStart >= 0)
                indexEnd = Util.IndexOf(first1k, 0x00, indexStart);

            if (indexEnd >= 0)
                info = Encoding.ASCII.GetString(first1k, indexStart, (indexEnd - indexStart));

            if (info == "")
            {
                indexStart = Util.IndexOf(last3k, infoHeader);

                if (indexStart >= 0)
                    indexEnd = Util.IndexOf(last3k, 0x00, indexStart);

                if (indexEnd >= 0)
                    info = Encoding.ASCII.GetString(last3k, indexStart, (indexEnd - indexStart));
            }
        }

        return info;
    }
    */
        #endregion


        #region 报文生成、解析
        // 发送报文
        private void SendCmd(Command cmd)
        {
            string msg;

            if (cmd.TxBuf == null || cmd.TxBuf.Length == 0) return;

            

            byte[] buf = cmd.TxBuf;

            if (false == SerialPort_SendData(buf))
            {
                UiOperateEnable();
                cmd.RetryTimes = 0;
                return;
            }

            msg = cmd.Name + " （下行）\r\n"
                + "    Tx：" + Util.GetStringHexFromBytes(buf, 0, buf.Length, " ") + "\r\n";
            ShowMsg(msg, Color.Blue);

        }

        // 接收报文
        private void RecvCmd(Command cmd)
        {
            string cmdName = "", strVal = "", strTmp;
            //int index = 0, strLen = 0;

            if (cmd.RxBuf == null || cmd.RxBuf.Length == 0) return;

            byte[] buf = cmd.RxBuf;

            // 协议解析
            ProtoLocal_North.FrameFormat frame = ProtoLocal_North.ExplainRxPacket(buf);

            TreeNode tree = ProtoLocal_North.GetProtoTree(buf);

            if(tree != null && tree.Nodes.Count > 3)
            {
                for(int i = 3; i < tree.Nodes.Count; i++)
                {
                    strVal += tree.Nodes[i].Text + "\r\n";

                    if(tree.Nodes[i].Text.Contains("具体项Fn  ："))
                    {
                        cmdName = tree.Nodes[i].Text.Trim().Split(' ').Last();
                        strVal += "----------\r\n";
                        break;
                    }
                }

                if (tree.Nodes[tree.Nodes.Count -1].Text.Contains("数据载荷"))
                {
                    foreach(TreeNode nodeL1 in tree.Nodes[tree.Nodes.Count - 1].Nodes)
                    {
                        if (cmdName == "查询主节点地址-应答")
                        {
                            strVal += "" + nodeL1.Text + "\r\n";
                            if (nodeL1.Text.Contains("主节点地址："))
                            {
                                strTmp = nodeL1.Text.Substring(nodeL1.Text.IndexOf("：") + 1);
                                UpdateCenterAddr(strTmp);
                            }
                        }
                        else if (cmdName == "查询微功率无线从节点信息-应答")
                        {
                            if (nodeL1.Text.StartsWith("从节点总数量："))
                            {
                                strVal += "" + nodeL1.Text + "\r\n";
                            }
                            else if (nodeL1.Text.StartsWith("本次应答数量："))
                            {
                                strVal += "" + nodeL1.Text + "\r\n\r\n";
                                if (nodeL1.Text.Contains("：0")) break;

                                strVal += "序号   节点地址    中继级别   通信协议   升级标志  软件版本  boot版本\r\n";
                                strVal += "-----  ----------  --------  ----------  --------  --------  --------\r\n";
                            }
                            else if (nodeL1.Text.StartsWith("节点"))
                            {
                                _docCnt++;
                                strTmp = _docCnt.ToString().PadRight(6);
                                strTmp += nodeL1.Text.Substring(nodeL1.Text.IndexOf("：") + 1).PadRight(17);
                                foreach (TreeNode nodeL2 in nodeL1.Nodes)
                                {
                                    if (nodeL2.Text.StartsWith("中继级别："))
                                    {
                                        strTmp += nodeL2.Text.Substring(nodeL2.Text.IndexOf("：") + 1).PadRight(6);
                                    }
                                    else if (nodeL2.Text.StartsWith("通信协议："))
                                    {
                                        strTmp += nodeL2.Text.Substring(nodeL2.Text.IndexOf("：") + 1).PadRight(12);
                                    }
                                    else if (nodeL2.Text.StartsWith("升级标志："))
                                    {
                                        strTmp += nodeL2.Text.Substring(nodeL2.Text.IndexOf("：") + 1).PadRight(8);
                                    }
                                    else if (nodeL2.Text.StartsWith("软件版本："))
                                    {
                                        strTmp += nodeL2.Text.Substring(nodeL2.Text.IndexOf("：") + 1).PadRight(10);
                                    }
                                    else if (nodeL2.Text.StartsWith("Boot版本："))
                                    {
                                        strTmp += nodeL2.Text.Substring(nodeL2.Text.IndexOf("：") + 1).PadRight(6);
                                    }
                                }
                                strVal += strTmp + "\r\n";

                                UpdateDocCnt(_docCnt.ToString());
                                AddDocToTbl(strTmp);
                            }
                        }
                        else
                        {
                            strVal += "" + nodeL1.Text + "\r\n";

                            foreach (TreeNode nodeL2 in nodeL1.Nodes)
                            {
                                strVal += "  " + nodeL2.Text + "\r\n";

                                foreach (TreeNode nodeL3 in nodeL2.Nodes)
                                {
                                    strVal += "    " + nodeL3.Text + "\r\n";
                                }

                            }
                        }
                    }
                }
            }

            // 显示 命令名、接收数据
            strTmp = cmdName + " （上行）\r\n"
                    + "    Rx：" + Util.GetStringHexFromBytes(buf, 0, buf.Length, " ") + "\r\n";
            ShowMsg(strTmp, Color.DarkRed);

            // 显示 解析结果
            if (cmdName != "")
            {
                cmd.IsEnable = false;
                cmd.RetryTimes = 0;

                strTmp = strVal + "\r\n";
                ShowMsg(strTmp, Color.Green, false);
            }
        }
        #endregion

        #region 添加档案
        private void AddDocToTbl(string docInfo)
        {
            dgvDoc.BeginInvoke(new EventHandler(delegate 
            {
                string[] strs = docInfo.Trim().Split(new string[] { " " }, 7, StringSplitOptions.RemoveEmptyEntries);

                DataRow row = tbDoc.NewRow();
                row.BeginEdit();
                row[序号] = tbDoc.Rows.Count + 1;
                row[模块地址] = strs[1];
                row[表地址] = strs[1];
                row[协议类型] = strs[3];
                row[版本] = strs[6] + "_" + strs[5];
                row[中继级别] = strs[2];
                row[升级状态] = strs[4];
                row[发送] = 0;
                row[接收] = 0;
                row[读数] = "";
                row.EndEdit();
                tbDoc.Rows.Add(row);
                dgvDoc.FirstDisplayedScrollingRowIndex = dgvDoc.RowCount - 1;
            }));
            
        }
        #endregion

        #region 通信记录-显示/清空/保存
        private void ShowMsg(string msg, Color fgColor, bool showTime = true)
        {
            if (msg == "") return;

            rtbMsg.BeginInvoke(new EventHandler(delegate
            {
                msg = (showTime ? "[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + "]  " : "") + msg;

                RichTextBoxAppand(rtbMsg, msg, fgColor);

                if (_isAutoSaveLog)
                {
                    _conCenterLog.WriteLine(msg);
                }
            }));
        }

        private delegate void UpdateRichTextCallback(RichTextBox rtb, string msg, Color fgColor);
        private void RichTextBoxAppand(RichTextBox rtb, string str, Color foreColor)
        {
            if (this.InvokeRequired)
            {
                Invoke(new UpdateRichTextCallback(RichTextBoxAppand), new object[] { rtb, str, foreColor });
                return;
            }

            int iStart = rtb.Text.Length;
            rtb.AppendText(str);
            rtb.Select(iStart, rtb.Text.Length);
            rtb.SelectionColor = foreColor;
            rtb.Select(rtb.Text.Length, 0);
            rtb.ScrollToCaret();
        }

        private void btClear_Click(object sender, EventArgs e)
        {
            rtbMsg.Clear();
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            string strDirectory;
            string strFileName;
            SaveFileDialog saveFileDlg = new SaveFileDialog();

            if (rtbMsg.Text == "")
            {
                return;
            }

            strDirectory = XmlHelper.GetNodeDefValue(_configPath, "/Config/LogPath", Application.StartupPath);
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
                XmlHelper.SetNodeValue(_configPath, "/Config", "LogPath", strDirectory);
            }

            using (StreamWriter sw = new StreamWriter(strFileName, false, Encoding.UTF8))
            {
                sw.WriteLine(rtbMsg.Text.Replace("\n", "\r\n"));
            }

            ShowMsg("保存成功" + " ！\r\n\r\n", Color.Green);
        }
        #endregion

        #region UI更新

        delegate void UpdateUi(string msg);
        private void UiOperateEnable(string msg = "")
        {
            if (this.InvokeRequired)
            {
                Invoke(new UpdateUi(UiOperateEnable), msg);
                return;
            }

            lbCmdStatus.Visible = false;

            grpRecentCmd.Enabled = true;
            grpRecentUse.Enabled = true;
            grpCmdMenu.Enabled = true;
            grpParamCmd.Enabled = true;
            grpLoopReadAmeter.Enabled = true;
        }
        private void UiOperateDisable(string msg = "")
        {
            if (this.InvokeRequired)
            {
                Invoke(new UpdateUi(UiOperateDisable), msg);
                return;
            }

            lbCmdStatus.Visible = (msg != "" ? true : false);

            grpRecentCmd.Enabled = false;
            grpRecentUse.Enabled = true;
            grpCmdMenu.Enabled = false;
            grpParamCmd.Enabled = true;
            grpLoopReadAmeter.Enabled = false;
        }

        private void UpdateDocCnt(string msg)
        {
            if (this.InvokeRequired)
            {
                Invoke(new UpdateUi(UpdateDocCnt), msg);
                return;
            }

            lbDocCnt.Text = "档案列表[ " + msg + " ]";

        }
        private void UpdateCenterAddr(string msg)
        {
            if (this.InvokeRequired)
            {
                Invoke(new UpdateUi(UpdateCenterAddr), msg);
                return;
            }

            lbCenterAddr.Text = "集中器地址：" + msg;
        }
        #endregion

        #region 窗口关闭处理

        private void Close()
        {
            _thrTransceiver.Abort();
            if (_scom.IsOpen)
            {
                _scom.Close();
            }

            FrmMain.LogAutoSaveStateChanged -= _evtLogAutoSaveChanged;

            if (_conCenterLog != null)
            {
                _conCenterLog.Close();
                _conCenterLog = null;
            }
        }

        #endregion

    }
}
