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
using ElectricPowerDebuger.Common;
using System.IO.Ports;
using System.IO;
using ElectricPowerDebuger.Protocol;

namespace ElectricPowerDebuger.Function
{
    public partial class ConcSimulator_North : UserControl
    {
        private Control usrCtrl;
        private string _configPath;
        private SerialCom _scom;
        private ConcurrentQueue<byte[]> _recvQueue;
        private ConcurrentQueue<Command> _sendQueue;
        private Thread _thrTransceiver;
        private bool _IsSendNewCmd;

        delegate void CallbackFunc(params object[] args);
        private CallbackFunc _cmdEndCallback;
        private object[] _argsEndCallback;
        private Command CurrentCmd { get; set; }
        private Command PreparedCmd { get; set; }

        private string _strMsgBuf = "";
        private string _strMsgMain = "";
        private string _strMsgSub = "";

        private static byte[] TempBuf = new byte[512];
        private static string _strDocCnt = "0";
        private static string _strCenterAddr = "201900008888";
        private static byte _fsn = 0;

        public ConcSimulator_North()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;

            FrmMain.ProtocolVerChanged += OnProtoVerChanged;
            _configPath = FrmMain.SystemConfigPath;

            DataRow row = dtbDoc.NewRow();
            row[序号] = 100;
            row[模块地址] = "112233445566";
            row[表地址] = "112233445566";
            row[协议类型] = "(2) 07电表";
            row[版本] = "000032";
            row[升级状态] = "未升级";
            row[发送] = 123456;
            row[接收] = 123456;
            row[读数] = "123456.78 kWh";
            dtbDoc.Rows.Add(row);

            _scom = new SerialCom();
            _scom.DataReceivedEvent += SerialPort_DataReceived;
            _scom.UnexpectedClosedEvent += SerialPort_UnexpectedClosed;
            _recvQueue = new ConcurrentQueue<byte[]>();
            _sendQueue = new ConcurrentQueue<Command>();
            _thrTransceiver = new Thread(TransceiverHandle);
            _thrTransceiver.IsBackground = true;
            _thrTransceiver.Start();

            UpdateRecentCmdName();

        }

        private void OnProtoVerChanged(string msg)
        {
            switch (msg)
            {
                case "国网-版本":
                    usrCtrl = new ConcSimulator_North();
                    break;

                case "南网-版本":
                    usrCtrl = new ConcSimulator();
                    break;

                default:
                    usrCtrl = new ConcSimulator();
                    break;
            }
            FrmMain.ProtocolVerChanged -= OnProtoVerChanged;

            Control tabPage = this.Parent;
            tabPage.Controls.Remove(this);
            tabPage.Controls.Add(usrCtrl);

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

                XmlHelper.SetNodeValue(_configPath, "/Config", "PortName", cbxPortNum.Text);

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

        #region 命令处理--发送、接收

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
                            msg = cmd.GrpName + "失败";
                            ShowMsg(msg + "\r\n\r\n", Color.Red);
                        }
                        else
                        {
                            msg = cmd.Name + "失败";
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

        #region 命令发送（所有的）

        private void AllCmdButton_Click(object sender, EventArgs e)
        {
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

            if(cmdText.StartsWith("0"))
            {
                cmdText = cmdText.Trim().Split(' ').Last();
            }

            HideParamCmdGrp();

            switch (cmdText)
            {
                case "数据转发":
                    lbParam1.Text = "协议类型";
                    lbParam1.Location = new Point(22, 30);
                    lbParam1.Visible = true;
                    cbxParam1.Items.Clear();
                    cbxParam1.Items.Add("0 透明传输");
                    cbxParam1.Items.Add("1 DL/T645-97");
                    cbxParam1.Items.Add("2 DL/T645-07");
                    cbxParam1.Location = new Point(78, 28);
                    cbxParam1.Visible = true;
                    lbParam2.Text = "报文内容：";
                    lbParam2.Location = new Point(22, 60);
                    lbParam2.Visible = true;
                    txtParam3.Location = new Point(24, 83);
                    txtParam3.Visible = true;
                    btParamConfirm.Location = new Point(24, 146);
                    break;

                case "1":
                    
                    break;

                case "2":

                    break;

                case "3":

                    break;

                default:
                    SendToCmdQueue(cmdText);
                    isParamCmd = false;
                    break;
            }

            if(isParamCmd)
            {
                grpParamCmd.Text = cmdText;
                grpParamCmd.Visible = true;
            }

            UpdateRecentCmdName(cmdText);

        }

        private void HideParamCmdGrp()
        {
            lbParam1.Visible = false;
            lbParam2.Visible = false;
            txtParam1.Visible = false;
            txtParam2.Visible = false;
            txtParam3.Visible = false;
            rbtParam1.Visible = false;
            rbtParam2.Visible = false;
            cbxParam1.Visible = false;
            chkParam1.Visible = false;
            grpParamCmd.Visible = false;
        }

        private void UpdateRecentCmdName(string cmdText = "")
        {
            string recentFixedCmds = "读出全部档案/重下全部档案/启动组网/查询路由运行状态";

            string recentCmds = XmlHelper.GetNodeValue(_configPath, "/Config/ConcSimulator_North/RecentCmds");
            if(recentCmds == "")
            {
                recentCmds = "添加主节点/查询主节点地址/查询厂商代码和版本/查询无线通信参数";
            }

            if( !recentCmds.Contains(cmdText) && !recentFixedCmds.Contains(cmdText) && cmdText != "")
            {
                if (cmdText != "")
                {
                    recentCmds = recentCmds.Substring(recentCmds.IndexOf("/") + 1) + "/" + cmdText;
                }

                string[] strs = recentCmds.Split('/');
                btRecentUse1.Text = strs[0];
                btRecentUse2.Text = strs[1];
                btRecentUse3.Text = strs[2];
                btRecentUse4.Text = strs[3];
                XmlHelper.SetNodeValue(_configPath, "/Config/ConcSimulator_North", "RecentCmds", recentCmds);
            }

        }

        private void btParamConfirm_Click(object sender, EventArgs e)
        {
            string cmdText = grpParamCmd.Text;

            SendToCmdQueue(cmdText);
        }

        private void SendToCmdQueue(string cmdName)
        {
            Command cmd = new Command(cmdName);
            int txLen = 0;

            cmd.SendFunc = SendCmd;
            cmd.RecvFunc = RecvCmd;
            cmd.TimeWaitMS = 1000;
            cmd.RetryTimes = 1;

            TempBuf[txLen++] = 0x68;
            TempBuf[txLen++] = 0;       // length
            TempBuf[txLen++] = 0;
            TempBuf[txLen++] = 0x4A;    // ctrl
            TempBuf[txLen++] = 0;       // info-1~5
            TempBuf[txLen++] = 0;
            TempBuf[txLen++] = 0;
            TempBuf[txLen++] = 0;
            TempBuf[txLen++] = 0;
            TempBuf[txLen++] = _fsn++;   // info-6

            if(cmd.Name == "" || cmd.Name == "")
            {
                Util.GetByteAddrFromString(_strCenterAddr, TempBuf, txLen, true);
            }

            switch (cmd.Name)
            {
                case "":
                    
                    break;

                case "1":

                    break;

                default:
                    cmd = null;
                    break;
            }

            if (cmd == null) return;

            cmd.TxBuf = new byte[txLen];
            Array.Copy(TempBuf, 0, cmd.TxBuf, 0, txLen);
            _sendQueue.Enqueue(cmd);

            _IsSendNewCmd = true;
        }

        #endregion


        #region 报文生成、解析
        // 发送报文
        private void SendCmd(Command cmd)
        {
            string msg;

            if (cmd.TxBuf == null || cmd.TxBuf.Length == 0) return;

            while (cmd.Params.Count < 3)    //Params0 - cmdName, Params1 - cmdParam, Params2 - cmdRecvMsg
            {
                cmd.Params.Add("");
            }

            byte[] buf = cmd.TxBuf;

            if (false == SerialPort_SendData(buf))
            {
                UiOperateEnable();
                cmd.RetryTimes = 0;
                return;
            }

            msg = "  " + cmd.Name + "\r\n"
                + "    Tx：" + Util.GetStringHexFromByte(buf, 0, buf.Length, " ") + "\r\n";
            ShowMsg(msg, Color.Blue);

        }

        // 接收报文
        private void RecvCmd(Command cmd)
        {
            string msg = "", cmdName = "", strVal = "", strTmp;
            int index = 0, strLen = 0;

            if (cmd.RxBuf == null || cmd.RxBuf.Length == 0) return;

            byte[] buf = cmd.RxBuf;

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
                    }
                }

                if (tree.Nodes[tree.Nodes.Count -1].Text.Contains("数据载荷"))
                {
                    foreach(TreeNode nodeL1 in tree.Nodes[tree.Nodes.Count - 1].Nodes)
                    {
                        strVal += "\t" + nodeL1.Text + "\r\n";

                        foreach (TreeNode nodeL2 in nodeL1.Nodes)
                        {
                            strVal += "\t\t" + nodeL2.Text + "\r\n";

                            foreach (TreeNode nodeL3 in nodeL2.Nodes)
                            {
                                strVal += "\t\t" + nodeL3.Text + "\r\n";
                            }
                        }
                    }
                }
            }

            msg = "  " + cmdName + "\r\n"
                + "    Rx：" + Util.GetStringHexFromByte(buf, 0, buf.Length, " ") + "\r\n"
                + strVal;
            ShowMsg(msg, Color.DarkRed);

        }
        #endregion

        #region 通信记录-显示/清空/保存
        private void ShowMsg(string msg, Color fgColor)
        {
            if (msg == "") return;

            msg = "[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + "] " + msg;

            RichTextBoxAppand(rtbMsg, msg, fgColor);
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
            grpCmdMenu.Enabled = true;
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
            grpCmdMenu.Enabled = false;
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

        #region 窗口关闭
        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            _thrTransceiver.Abort();
            if (_scom.IsOpen)
            {
                _scom.Close();
            }
        }
        #endregion

    }
}
