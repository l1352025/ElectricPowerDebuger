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
using System.Reflection;
using ElectricPowerDebuger.Common;
using ElectricPowerDebuger.Protocol;

namespace ElectricPowerDebuger.Function
{
    public partial class DataMonitor : UserControl
    {
        private int PortBufRdPos = 0;
        private int PortBufWrPos = 0;
        private byte[] PortRxBuf = new Byte[2000];

        private string strCenterAddr = "888888888888";
        private int InvalidFrameNum = 0;
        private bool IsScrollToEnd = true;

        private int xMsTimer = 0;

        public delegate void CmdSendHandler();
        public delegate void CmdRecvHandler(byte[] rxBuf);
        public enum CmdType
        {
            空闲          = 0x00,
            设置信道组,
            模块版本检测
        };
        public struct Command
        {
            public CmdType Type;
            public byte[] Params;
            public int WaitTime;
            public int RetryTimes;
            public bool IsEnable;
            public CmdSendHandler SendFunc;
            public CmdRecvHandler RecvFunc; 
        };

        public Command Cmd;

        public delegate void SerialDataRecievedEventHandler(object sender, SerialDataReceivedEventArgs e); 

        public DataMonitor()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            lvDataList.DoubleBuffered(true);
            treeVwrProtol.DoubleBuffered(true);
            rtbRxdata.DoubleBuffered(true);

            string strPortName = XmlHelper.GetNodeDefValue(FrmMain.SystemConfigPath, "/Config/DataMonitor/PortName", "");
            cmbPort.Items.AddRange(new object[] { strPortName });
            cmbPort.Text = strPortName;
            cmbBaudrate.Text = XmlHelper.GetNodeDefValue(FrmMain.SystemConfigPath, "/Config/DataMonitor/Baudrate", "19200");
            cmbChanel.Text = XmlHelper.GetNodeDefValue(FrmMain.SystemConfigPath, "/Config/DataMonitor/ChanelGrp", "30");
        }

        #region 串口通信
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

            if (btOpenPort.Text == "打开串口")
            {
                if (true == port_Ctrl(true))
                {
                    btOpenPort.Text = "关闭串口"; ;
                    btOpenPort.BackColor = Color.GreenYellow;
                    cmbPort.Enabled = false;
                    cmbBaudrate.Enabled = false;

                    XmlHelper.SetNodeValue(FrmMain.SystemConfigPath, "/Config/DataMonitor", "PortName", cmbPort.Text);
                    XmlHelper.SetNodeValue(FrmMain.SystemConfigPath, "/Config/DataMonitor", "Baudrate", cmbBaudrate.Text);

                    CmdSetChanelGrp(Convert.ToByte(cmbChanel.Text));
                }
                else
                {
                    btOpenPort.Text = "打开串口";
                    btOpenPort.BackColor = Color.Silver;
                    cmbPort.Enabled = true;
                    cmbBaudrate.Enabled = true;
                }
            }
            else
            {
                port_Ctrl(false);
                btOpenPort.Text = "打开串口";
                btOpenPort.BackColor = Color.Silver;
                cmbPort.Enabled = true;
                cmbBaudrate.Enabled = true;
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

        private void serialPort_SendData(byte[] buf, int index, int len)
        {
            if (true == serialPort.IsOpen)
            {
                try
                {
                    serialPort.Write(buf, index, len);
                }
                catch (Exception ex)
                {
                    throw (new Exception("发送错误，" + ex.Message));
                }
            }
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
        private void timerDataMonitor_Tick(object sender, EventArgs e)
        {
            int len, sum;

            try
            {  //test

                if (Cmd.IsEnable)
                {
                    if (Cmd.WaitTime > 0)
                    {
                        Cmd.WaitTime--;
                    }
                    else if (Cmd.Type != CmdType.空闲)
                    {
                        try
                        {
                            if (Cmd.RetryTimes > 0)
                            {
                                Cmd.SendFunc();
                                Cmd.RetryTimes--;
                                Cmd.WaitTime = 30;
                            }
                            else
                            {
                                throw (new Exception("无应答"));
                            }
                        }
                        catch (Exception ex)
                        {
                            StatusUpdate(Cmd.Type.ToString() + "失败：", Color.Red, ex.Message);
                            Cmd.Type = CmdType.空闲;
                            Cmd.IsEnable = false;
                        }
                    }
                }

                if (xMsTimer++ > 10 / timerDataMonitor.Interval)
                {
                    xMsTimer = 0;
                    while (true)
                    {
                        len = (PortBufWrPos >= PortBufRdPos) ? (PortBufWrPos - PortBufRdPos) : (PortRxBuf.Length - PortBufRdPos + PortBufWrPos);

                        // 接收无线帧起始字符 55AA
                        if (len < ProtoWireless_South.FrameFixedLen)
                        {
                            break;
                        }
                        if (PortRxBuf[PortBufRdPos % PortRxBuf.Length] != (ProtoWireless_South.FrameHeader & 0xFF)
                            || PortRxBuf[(PortBufRdPos + 1) % PortRxBuf.Length] != (ProtoWireless_South.FrameHeader >> 8))
                        {
                            PortBufRdPos = (UInt16)((PortBufRdPos + 1) % PortRxBuf.Length);
                            continue;
                        }

                        // 串口命令应答
                        if (Cmd.Type != CmdType.空闲 && len > 8)
                        {
                            byte[] cmdResponse = new byte[len];
                            if (PortBufRdPos + len > PortRxBuf.Length)
                            {
                                Array.Copy(PortRxBuf, PortBufRdPos, cmdResponse, 0, PortRxBuf.Length - PortBufRdPos);
                                Array.Copy(PortRxBuf, 0, cmdResponse, (PortRxBuf.Length - PortBufRdPos), len - (PortRxBuf.Length - PortBufRdPos));
                            }
                            else
                            {
                                Array.Copy(PortRxBuf, PortBufRdPos, cmdResponse, 0, len);
                            }

                            Cmd.RecvFunc(cmdResponse);
                            break;
                        }

                        string protoVer = Common.XmlHelper.GetNodeValue(FrmMain.SystemConfigPath, "Config/Global/ProtocolVer");

                        if (protoVer == "南网-版本" || protoVer == "北网-版本")
                        {
                            sum = PortRxBuf[(PortBufRdPos + 3) % PortRxBuf.Length] + 6;
                        }
                        else //     if (protoVer == "尼泊尔-版本")
                        {
                            sum = (PortRxBuf[(PortBufRdPos + 3) % PortRxBuf.Length] + PortRxBuf[(PortBufRdPos + 4) % PortRxBuf.Length] * 256) + 7;
                        }
                        

                        if (sum > len)
                        {
                            break;
                        }

                        byte[] rxBuf = new byte[sum];
                        if (PortBufRdPos + sum > PortRxBuf.Length)
                        {
                            Array.Copy(PortRxBuf, PortBufRdPos, rxBuf, 0, PortRxBuf.Length - PortBufRdPos);
                            Array.Copy(PortRxBuf, 0, rxBuf, (PortRxBuf.Length - PortBufRdPos), sum - (PortRxBuf.Length - PortBufRdPos));
                        }
                        else
                        {
                            Array.Copy(PortRxBuf, PortBufRdPos, rxBuf, 0, sum);
                        }

                        PortBufRdPos = (UInt16)((PortBufRdPos + 6) % PortRxBuf.Length); // 可能该包不完整，跳过6byte , 查找下一包

                        dataTableAppend(rxBuf);
                    }
                }

            }catch( Exception ex)
            {
                MessageBox.Show("Timer_Tick 异常：" + ex.Message + "\n exStack:" + ex.StackTrace + "\n exSource:" + ex.Source + "\n exTarget:" + ex.TargetSite);
            }
        }

        #endregion

        #region 中心地址设置
        private void txtCenterAddr_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ("0123456789\b\r\x03\x16\x18".IndexOf(e.KeyChar) < 0)
            {
                e.Handled = true;
                return;
            }
            if (e.KeyChar == '\r' || (txtCenterAddr.Text.Length == ProtoLocal_South.LongAddrSize * 2) && e.KeyChar != '\b')
            {
                txtCenterAddr.Text = txtCenterAddr.Text.PadLeft(ProtoLocal_South.LongAddrSize * 2, '0');
                if (txtCenterAddr.SelectionLength == 0)
                {
                    e.Handled = true;
                }

                strCenterAddr = txtCenterAddr.Text;
            }
        }
        #endregion

        #region 串口命令处理
        /// <summary>
        /// 命令发送
        /// </summary>
        private void TransmitCmd()
        {
            if (Cmd.Params != null && Cmd.Params.Length > 0)
            {
                serialPort_SendData(Cmd.Params, 0, Cmd.Params.Length);
            }
            else
            {
                MessageBox.Show("Cmd.Params Error");
            }
        }

        private void combChanel_SelectedIndexChanged(object sender, EventArgs e)
        {
            XmlHelper.SetNodeValue(FrmMain.SystemConfigPath, "/Config/DataMonitor", "ChanelGrp", cmbChanel.Text);

            CmdSetChanelGrp(Convert.ToByte(cmbChanel.Text));
        }

        private void CmdSetChanelGrp(byte chGrp)
        {
            if(serialPort.IsOpen == false || Cmd.Type != CmdType.空闲)
            {
                return;
            }
            Cmd.Type = CmdType.设置信道组;
            Cmd.Params = new byte[9] { 0x55, 0xAA, 0x55, 0xAA, 0x55, 0xAA, 0x55, 0xAA, chGrp };
            Cmd.WaitTime = 30;
            Cmd.RetryTimes = 3;
            Cmd.SendFunc = TransmitCmd;
            Cmd.RecvFunc = RecieveCmdSetChanelGrp;
            Cmd.IsEnable = true;
        }

        private void RecieveCmdSetChanelGrp(byte[] rxBuf)
        {
            if (rxBuf.Length > 8
                && rxBuf[0] == 0x55 && rxBuf[1] == 0xAA && rxBuf[2] == 0x55 && rxBuf[3] == 0xAA
                && rxBuf[4] == 0x55 && rxBuf[5] == 0xAA && rxBuf[6] == 0x55 && rxBuf[7] == 0xAA)
            {
                StatusUpdate("信道组设置成功：", Color.Red, rxBuf[8].ToString());
                PortBufRdPos = (UInt16)((PortBufRdPos + 9) % PortRxBuf.Length);

                Cmd.Type = CmdType.空闲;
                Cmd.IsEnable = false;
            }
        }

        private void btModuleChk_Click(object sender, EventArgs e)
        {
            CmdModuleCheck();
        }

        private void CmdModuleCheck()
        {
            if (serialPort.IsOpen == false || Cmd.Type != CmdType.空闲)
            {
                return;
            }
            Cmd.Type = CmdType.模块版本检测;
            Cmd.Params = new byte[]{ 0x68,0x01,0x02,0x03,0x04,0x05,0x06,0x68,0x68,0x68,0x01,0x02,0x16 };
            Cmd.WaitTime = 30;
            Cmd.RetryTimes = 3;
            Cmd.SendFunc = TransmitCmd;
            Cmd.RecvFunc = RecieveCmdModuleCheck;
            Cmd.IsEnable = true;
        }

        private void RecieveCmdModuleCheck(byte[] rxBuf)
        {
            // 55 AA 00 00 00 2F 55 AA 88 00 00 00 00 00 53 52 57 46 ... 00 CRC16
            if (rxBuf.Length > 10
                && rxBuf[0] == 0x55 && rxBuf[1] == 0xAA && rxBuf[6] == 0x55 && rxBuf[7] == 0xAA
                && rxBuf[5] <= (rxBuf.Length - 5)
                )
            {
                string verInfo = "";
                for (int i = 14; rxBuf[i] != 0x00; i++ )
                {
                    verInfo += Convert.ToChar(rxBuf[i]);
                }

                StatusUpdate("监控模块版本信息：", Color.Red, verInfo);
                PortBufRdPos = (UInt16)((PortBufRdPos + rxBuf[6] + 10) % PortRxBuf.Length);

                Cmd.Type = CmdType.空闲;
                Cmd.IsEnable = false;
            }
        }
        #endregion

        #region 监控日志 - 保存、导入、清除、追加
        private void btSaveLog_Click(object sender, EventArgs e)
        {
            string strDirectory;
            string strFileName;

            if (0 == tbLog.Rows.Count)
            {
                MessageBox.Show("没有可保存的日志!");
                return;
            }

            strDirectory = XmlHelper.GetNodeDefValue(FrmMain.SystemConfigPath, "/Config/DataMonitor/LogPath", Application.StartupPath);
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
                XmlHelper.SetNodeValue(FrmMain.SystemConfigPath, "/Config/DataMonitor", "LogPath", strDirectory);
            }
            StreamWriter sw = new StreamWriter(strFileName, false);

            if (lvDataList.SelectedItems.Count > 1)     // save selected
            {
                for (int i = 0, index = 0; i < lvDataList.SelectedItems.Count; i++)
                {
                    index = lvDataList.SelectedIndices[i];

                    string strLine = tbLog.Rows[index]["序号"].ToString() + " "
                                    + tbLog.Rows[index]["日期"].ToString() + " "
                                     + tbLog.Rows[index]["时间"].ToString() + " ";
                    byte[] databuf = (byte[])tbLog.Rows[index]["原始报文"];

                    foreach (byte data in databuf)
                    {
                        strLine += data.ToString("X2") + " ";
                    }

                    sw.WriteLine(strLine);
                }
            }
            else    // save all
            {
                for (int i = 0; i < tbLog.Rows.Count; i++)
                {
                    string strLine = tbLog.Rows[i]["序号"].ToString() + " "
                                    + tbLog.Rows[i]["日期"].ToString() + " "
                                     + tbLog.Rows[i]["时间"].ToString() + " ";
                    byte[] databuf = (byte[])tbLog.Rows[i]["原始报文"];

                    foreach(byte data in databuf)
                    {
                        strLine += data.ToString("X2") + " ";
                    }

                    sw.WriteLine(strLine);
                }
            }
            sw.Close();
            MessageBox.Show("保存档案成功！");
        }

        private void btLoadLog_Click(object sender, EventArgs e)
        {
            string strDirectory, strFileName, strRead;

            strDirectory = XmlHelper.GetNodeDefValue(FrmMain.SystemConfigPath, "/Config/DataMonitor/LogPath", Application.StartupPath);
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
                MessageBox.Show("导入失败！\n", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (strDirectory != Path.GetDirectoryName(strFileName))
            {
                strDirectory = Path.GetDirectoryName(strFileName);
                XmlHelper.SetNodeValue(FrmMain.SystemConfigPath, "/Config/DataMonitor", "LogPath", strDirectory);
            }
            StreamReader sr = new StreamReader(strFileName, Encoding.GetEncoding("GB2312"));

            String strNo, strDate, strTime;
            string[] strSplit;
            byte[] byteArray;

            while ((strRead = sr.ReadLine()) != null)
            {
                try
                {
                    strSplit = strRead.Trim().Split(' ');
                    if (strRead.StartsWith("@"))
                    {
                        strNo = strSplit[1];
                        strDate = strSplit[0].Substring(1);
                        strTime = strSplit[2];
                    }
                    else
                    {
                        strNo = strSplit[0];
                        strDate = strSplit[1];
                        strTime = strSplit[2];
                    }

                    DateTime timeCheck = DateTime.ParseExact(strDate + " " + strTime, "yyyy-MM-dd HH:mm:ss.fff", null);

                    byteArray = new byte[strSplit.Length - 3];
                    for (int iLoop = 3; iLoop < strSplit.Length; iLoop++)
                    {
                        byteArray[iLoop - 3] = Convert.ToByte(strSplit[iLoop], 16);
                    }

                    dataTableAppend(byteArray, strNo, strDate, strTime);
                }
                catch(System.FormatException ex)
                {
                    rtbRxdata.Text = "无法识别的日志格式:" + strRead;
                    MessageBox.Show("第" + tbLog.Rows.Count.ToString() + "行: 日志格式错误！" + ex.Message + "\r\n");
                    break;
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("在读取第" + tbLog.Rows.Count.ToString() + "行时出现错误，" + ex.Message + "！\r\n", "错误信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                }
            }
            sr.Close();
        }

        private void btClearLog_Click(object sender, EventArgs e)
        {
            tbLog.Rows.Clear();
            lvDataList.Items.Clear();
            rtbRxdata.Clear();
            treeVwrProtol.Nodes.Clear();
            InvalidFrameNum = 0;
        }

        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btSaveLog_Click(null, null);
        }
        private void 载入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btLoadLog_Click(null, null);
        }
        private void 清空ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btClearLog_Click(null, null);
        }
        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(lvDataList.SelectedItems.Count == 0)
            {
                return;
            }

            for (int i = lvDataList.SelectedItems.Count -1, index = 0; i >= 0; i--)
            {
                index = lvDataList.SelectedIndices[i];
                tbLog.Rows[index].Delete();
                lvDataList.Items[index].Remove();
            }
        }

        private void dataTableAppend(byte[] data, String strNo = null, String strDate = null, String strTime = null)
        {
            DataRow newRow = tbLog.NewRow();

            if (strNo == null || strDate == null || strTime == null)
            {
                strNo = (tbLog.Rows.Count + 1).ToString("D5");
                strDate = DateTime.Now.ToString("yyyy-MM-dd");
                strTime = DateTime.Now.ToString("HH:mm:ss.fff");
            }
            newRow["序号"] = strNo;
            newRow["日期"] = strDate;
            newRow["时间"] = strTime;
            newRow["原始报文"] = data;
            tbLog.Rows.Add(newRow);
            try
            {
                dataListUpdate();
            }
            catch(Exception ex)
            {
                StatusUpdate("更新报文异常:" + ex.Message + "\n" + ex.StackTrace, Color.Red, 
                    "\n" + Util.GetStringHexFromByte(data, 0, data.Length, " "));
            }
        }
        #endregion

        #region 监控日志 - 列表
        private void dataListUpdate()
        {
            string strTmp = "", strChgrp = "", strChnl = "", strRssi = "", strLen = "";
            string strDstAddr = "", strSrcAddr = "", strPanId = "", strFsn = "", strFrameType = "", strComment = "";
            Color cmdColor = Color.Black;
            bool IsInvalidFrame = false;

            DataRow curRow = tbLog.Rows[tbLog.Rows.Count - 1];

            lvDataList.BeginUpdate();
            ListViewItem lvi = lvDataList.Items.Add(Convert.ToInt32(curRow["序号"]).ToString());

            lvi.SubItems.Add(curRow["日期"].ToString());
            lvi.SubItems.Add(curRow["时间"].ToString());

            byte[] databuf = (byte[])curRow["原始报文"];

            string protoVer = Common.XmlHelper.GetNodeValue(FrmMain.SystemConfigPath, "Config/Global/ProtocolVer");

            if (protoVer == "南网-版本")
            {
                ProtoWireless_South.FrameFormat RxFrame = ProtoWireless_South.ExplainRxPacket(databuf);

                strChgrp = (RxFrame.Chanel / 2) == 0 ? "公共信道" : (RxFrame.Chanel / 2).ToString();
                strChnl = (RxFrame.Chanel % 2).ToString();        
                strRssi = RxFrame.Rssi.ToString();
                strLen = RxFrame.Length.ToString();

                if (RxFrame.Mac.CtrlWord.FrameType == ProtoWireless_South.MacFrameType.Invalid)  //无效帧统计
                {
                    InvalidFrameNum++;
                    IsInvalidFrame = true;

                    strSrcAddr = "无效帧 " + InvalidFrameNum.ToString();                           //源地址   -- 无效帧总数
                    strDstAddr = ((float)InvalidFrameNum / tbLog.Rows.Count).ToString("00.00%");    //目的地址 -- 无效帧百分比
                    strPanId = "";          //PanID  -- 空
                    strFsn = "";            //帧序号 -- 空
                }
                else
                {
                    strSrcAddr = RxFrame.Mac.SrcAddr == null ?
                                        "" : Util.GetStringHexFromByte(RxFrame.Mac.SrcAddr, 0, RxFrame.Mac.SrcAddr.Length, "", true);
                    strDstAddr = RxFrame.Mac.DstAddr == null ?
                                "" : Util.GetStringHexFromByte(RxFrame.Mac.DstAddr, 0, RxFrame.Mac.DstAddr.Length, "", true);
                    strPanId = (RxFrame.Mac.PanID >> 8).ToString("X2") + (RxFrame.Mac.PanID & 0x00FF).ToString("X2");
                    strFsn = RxFrame.Mac.FrameSn.ToString("X2");
                }

                strComment = RxFrame.PhrCrc != (databuf[3] ^ databuf[4]) ? "PHR校验错误、" : "";
                ushort crc16 = ProtoWireless_South.GenCRC16(databuf, 6, databuf.Length - 8);
                if (RxFrame.Crc16 != crc16)
                {
                    strComment += " CRC错误：" + RxFrame.Crc16.ToString("X4") + "-->" + crc16.ToString("X4");
                }

                ProtoWireless_South.GetTopFrameTypeAndColor(RxFrame, out strFrameType, out cmdColor);
            }
            else if (protoVer == "北网-版本")
            {
                ProtoWireless_North.FrameFormat RxFrame = ProtoWireless_North.ExplainRxPacket(databuf);

                strChgrp = (RxFrame.Chanel / 2) == 0 ? "公共信道" : (RxFrame.Chanel / 2).ToString();
                strChnl = (RxFrame.Chanel % 2).ToString();
                strRssi = RxFrame.Rssi.ToString();
                strLen = RxFrame.Length.ToString();

                if (RxFrame.Mac.CtrlWord.FrameType == ProtoWireless_North.MacFrameType.Invalid)  //无效帧统计
                {
                    InvalidFrameNum++;
                    IsInvalidFrame = true;

                    strSrcAddr = "无效帧 " + InvalidFrameNum.ToString();                           //源地址   -- 无效帧总数
                    strDstAddr = ((float)InvalidFrameNum / tbLog.Rows.Count).ToString("00.00%");    //目的地址 -- 无效帧百分比
                    strPanId = "";          //PanID  -- 空
                    strFsn = "";            //帧序号 -- 空
                }
                else
                {
                    strSrcAddr = RxFrame.Mac.SrcAddr == null ?
                                        "" : Util.GetStringHexFromByte(RxFrame.Mac.SrcAddr, 0, RxFrame.Mac.SrcAddr.Length, "", true);
                    strDstAddr = RxFrame.Mac.DstAddr == null ?
                                "" : Util.GetStringHexFromByte(RxFrame.Mac.DstAddr, 0, RxFrame.Mac.DstAddr.Length, "", true);
                    strPanId = (RxFrame.Mac.PanID >> 8).ToString("X2") + (RxFrame.Mac.PanID & 0x00FF).ToString("X2");
                    strFsn = RxFrame.Mac.FrameSn.ToString("X2");
                }

                strComment = RxFrame.PhrCrc != (databuf[3] ^ databuf[4] ^ databuf[5]) ? "PHR校验错误、" : "";

                ushort crc16 = ProtoWireless_North.GenCRC16(databuf, 3, databuf.Length - 5);
                if (RxFrame.Crc16 != crc16)
                {
                    strComment += " CRC错误：" + RxFrame.Crc16.ToString("X4") + "-->" + crc16.ToString("X4");
                }

                ProtoWireless_North.GetFrameTypeAndColor(RxFrame, out strFrameType, out cmdColor);
            }
            else if (protoVer == "尼泊尔-版本")
            {
                ProtoWireless_NiBoEr.FrameFormat RxFrame = ProtoWireless_NiBoEr.ExplainRxPacket(databuf);

                strChgrp = (RxFrame.Chanel / 2) == 0 ? "公共信道" : (RxFrame.Chanel / 2).ToString();
                strChnl = (RxFrame.Chanel % 2).ToString();
                strRssi = RxFrame.Rssi.ToString();
                strLen = RxFrame.Length.ToString();

                if (RxFrame.Mac.CtrlWord.FrameType == ProtoWireless_NiBoEr.MacFrameType.Invalid)  //无效帧统计
                {
                    InvalidFrameNum++;
                    IsInvalidFrame = true;

                    strSrcAddr = "无效帧 " + InvalidFrameNum.ToString();                           //源地址   -- 无效帧总数
                    strDstAddr = ((float)InvalidFrameNum / tbLog.Rows.Count).ToString("00.00%");    //目的地址 -- 无效帧百分比
                    strPanId = "";          //PanID  -- 空
                    strFsn = "";            //帧序号 -- 空
                }
                else
                {
                    strSrcAddr = RxFrame.Mac.SrcAddr == null ?
                                        "" : Util.GetStringHexFromByte(RxFrame.Mac.SrcAddr, 0, RxFrame.Mac.SrcAddr.Length, "", true);
                    strDstAddr = RxFrame.Mac.DstAddr == null ?
                                "" : Util.GetStringHexFromByte(RxFrame.Mac.DstAddr, 0, RxFrame.Mac.DstAddr.Length, "", true);
                    strPanId = (RxFrame.Mac.PanID >> 8).ToString("X2") + (RxFrame.Mac.PanID & 0x00FF).ToString("X2");
                    strFsn = RxFrame.Mac.FrameSn.ToString("X2");
                }

                strComment = "";
                ushort crc16 = ProtoWireless_NiBoEr.GenCRC16(databuf, 3, databuf.Length - 5);
                if (RxFrame.Crc16 != crc16)
                {
                    strComment += " CRC错误：" + RxFrame.Crc16.ToString("X4") + "-->" + crc16.ToString("X4");
                }

                ProtoWireless_NiBoEr.GetFrameTypeAndColor(RxFrame, out strFrameType, out cmdColor);
            }
            else // if (protoVer == "巴西-版本")
            {
                ProtoWireless_BaXi.FrameFormat RxFrame = ProtoWireless_BaXi.ExplainRxPacket(databuf);

                strChgrp = (RxFrame.Chanel / 2) == 0 ? "公共信道" : (RxFrame.Chanel / 2).ToString();
                strChnl = (RxFrame.Chanel % 2).ToString();
                strRssi = RxFrame.Rssi.ToString();
                strLen = RxFrame.Length.ToString();

                if (RxFrame.Mac.CtrlWord.FrameType == ProtoWireless_BaXi.MacFrameType.Invalid)  //无效帧统计
                {
                    InvalidFrameNum++;
                    IsInvalidFrame = true;

                    strSrcAddr = "无效帧 " + InvalidFrameNum.ToString();                           //源地址   -- 无效帧总数
                    strDstAddr = ((float)InvalidFrameNum / tbLog.Rows.Count).ToString("00.00%");    //目的地址 -- 无效帧百分比
                    strPanId = "";          //PanID  -- 空
                    strFsn = "";            //帧序号 -- 空
                }
                else
                {
                    strSrcAddr = RxFrame.Mac.SrcAddr == null ?
                                        "" : Util.GetStringHexFromByte(RxFrame.Mac.SrcAddr, 0, RxFrame.Mac.SrcAddr.Length, "", true);
                    strDstAddr = RxFrame.Mac.DstAddr == null ?
                                "" : Util.GetStringHexFromByte(RxFrame.Mac.DstAddr, 0, RxFrame.Mac.DstAddr.Length, "", true);
                    strPanId = (RxFrame.Mac.PanID >> 8).ToString("X2") + (RxFrame.Mac.PanID & 0x00FF).ToString("X2");
                    strFsn = RxFrame.Mac.FrameSn.ToString("X2");
                }

                strComment = "";
                ushort crc16 = ProtoWireless_BaXi.GenCRC16(databuf, 3, databuf.Length - 5);
                if (RxFrame.Crc16 != crc16)
                {
                    strComment += " CRC错误：" + RxFrame.Crc16.ToString("X4") + "-->" + crc16.ToString("X4");
                }

                ProtoWireless_BaXi.GetFrameTypeAndColor(RxFrame, out strFrameType, out cmdColor);
            }


            lvi.SubItems.Add(strChgrp);                         //信道组
            lvi.SubItems.Add(strChnl);                          //频点
            lvi.SubItems.Add(strRssi);                          //Rssi
            lvi.SubItems.Add(strLen);                           //包长
            lvi.SubItems.Add(strSrcAddr);                       //源地址
            lvi.SubItems.Add(strDstAddr);                       //目的地址
            lvi.SubItems.Add(strPanId);                           //PanID
            lvi.SubItems.Add(strFsn);                           //帧序号

            lvi.SubItems.Add(strFrameType);                     //帧类型
            strTmp = (strSrcAddr == "FFFFFFFFFFFF") ? "广播" : ((strSrcAddr == strCenterAddr) ? "中心" : "");
            lvi.SubItems.Add(strTmp);                           //源名称
            strTmp = (strDstAddr == "FFFFFFFFFFFF") ? "广播" : ((strDstAddr == strCenterAddr) ? "中心" : "");
            lvi.SubItems.Add(strTmp);                           //目的名称
            lvi.SubItems.Add(strComment);                       //备注

            lvi.ForeColor = (IsInvalidFrame == true) ? Color.Gray : cmdColor;
            IsInvalidFrame = false;

            if( IsScrollToEnd == false)
            {
                lvDataList.TopItem.EnsureVisible();     //选择某行时，停止滚动
            }
            else
            {
                lvi.EnsureVisible();
            }

            lvDataList.EndUpdate();
        }

        private void btScroll_Click(object sender, EventArgs e)
        {
            if (btScroll.Text == "停止滚动")
            {
                IsScrollToEnd = false;
                btScroll.Text = "开始滚动";
                btScroll.ForeColor = Color.Red;
            }
            else
            {
                IsScrollToEnd = true;
                btScroll.Text = "停止滚动";
                btScroll.ForeColor = Color.Green;

                lvDataList.EnsureVisible(lvDataList.Items.Count-1);
            }
        }
        #endregion

        #region 监控日志 - 协议树、原始报文
        private void lvDataList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(lvDataList.SelectedItems.Count == 0)
            {
                return;
            }

            if (lvDataList.SelectedItems.Count == 1 && lvDataList.SelectedIndices[0] == lvDataList.Items.Count - 1)
            {
                btScroll.Text = "开始滚动";
                btScroll_Click(null, null);
            }
            else
            {
                btScroll.Text = "停止滚动";
                btScroll_Click(null, null);
            }

            int rowIndex = lvDataList.SelectedIndices[0];

            dataTreeUpdate(rowIndex);
            dataRichTextUpdate(rowIndex);
        }

        private void dataTreeUpdate(int rowIndex)
        {
            DataRow curRow = tbLog.Rows[rowIndex];
            byte[] databuf = (byte[])curRow["原始报文"];

            treeVwrProtol.BeginUpdate();
            treeVwrProtol.Nodes.Clear();

            // root--数据包
            TreeNode PacketNode = new TreeNode("数据包");
            treeVwrProtol.Nodes.Add(PacketNode);

            // root--数据包--属性
            TreeNode AttrNode = new TreeNode("属性");
            {
                AttrNode.ForeColor = Color.Black;
                AttrNode.Nodes.Add("接收时间：" + curRow["时间"].ToString());
                AttrNode.Nodes.Add("报文长度：" + databuf.Length);
            }
            AttrNode.Expand();
            PacketNode.Nodes.Add(AttrNode);

            // root--数据包--协议帧
            string protoVer = Common.XmlHelper.GetNodeValue(FrmMain.SystemConfigPath, "Config/Global/ProtocolVer");

            TreeNode protoTree;
            if (protoVer == "南网-版本")
            {
                protoTree = ProtoWireless_South.GetProtoTree(databuf);
            }
            else if (protoVer == "北网-版本")
            {
                protoTree = ProtoWireless_North.GetProtoTree(databuf);
            }
            else //  if (protoVer == "尼泊尔-版本")
            {
                protoTree = ProtoWireless_NiBoEr.GetProtoTree(databuf);
            }

            foreach (TreeNode node in protoTree.Nodes)
            {
                node.Expand();
                PacketNode.Nodes.Add(node);
            }

            PacketNode.Expand();
            treeVwrProtol.EndUpdate();
        }

        private void dataRichTextUpdate(int rowIndex)
        {
            DataRow curRow = tbLog.Rows[rowIndex];
            byte[] databuf = (byte[])curRow["原始报文"];
            ProtoWireless_South.FrameFormat RxFrame = ProtoWireless_South.ExplainRxPacket(databuf);

            int index = 0, len, frameType;
            string strData = null;

            rtbRxdata.Clear();

            if ((byte)RxFrame.Mac.CtrlWord.FrameType == 0xFF)  //无效帧
            {
                strData = Util.GetStringHexFromByte(databuf, index, databuf.Length, " ");
                rtbRxdataAdd(strData, Color.Black);
                return;
            }

            // 包头 55AA + Rssi
            len = 3;
            strData = Util.GetStringHexFromByte(databuf, index, len, " ");
            rtbRxdataAdd(strData, Color.LimeGreen);
            index += len;
            // Phy层帧头
            len = 3;
            strData = Util.GetStringHexFromByte(databuf, index, len, " ");
            rtbRxdataAdd(strData, Color.Purple);
            index += len;
            // Mac层帧头
            frameType = (databuf[index] & 0x07);
            len = (databuf[index + 1] >> 2 & 0x03) == 0x02 ? 9 : 17;
            strData = Util.GetStringHexFromByte(databuf, index, len, " ");
            rtbRxdataAdd(strData, Color.Red);
            index += len;
            if (frameType == (int)ProtoWireless_South.MacFrameType.Data)
            {
                // Nwk层帧头
                frameType = (databuf[index] & 0x03);
                len = (databuf[index + 1] >> 2 & 0x03) == 0x02 ? 7 : 15;
                if ((databuf[index] & 0x80) > 0) //有路由域
                {
                    int count = databuf[index + len] & 0x0F;
                    len += (databuf[index + len + 1] & 0x03) == 0x02 ? (2 + count * 2) : (2 + count * 6);
                }
                if ((databuf[index] & 0x04) > 0) //有扩展域
                {
                    len += databuf[index + len] + 1;
                }
                strData = Util.GetStringHexFromByte(databuf, index, len, " ");
                rtbRxdataAdd(strData, Color.Orange);
                index += len;

                if (frameType == (int)ProtoWireless_South.NwkFrameType.Data)
                {
                    // Aps层帧头
                    len = 2;
                    if ((databuf[index] & 0x08) > 0) //有扩展域
                    {
                        len += databuf[index + len] + 1;
                    }
                    strData = Util.GetStringHexFromByte(databuf, index, len, " ");
                    rtbRxdataAdd(strData, Color.Green);
                    index += len;
                }
            }
            // 载荷
            len = databuf.Length - index - 2;
            strData = Util.GetStringHexFromByte(databuf, index, len, " ");
            rtbRxdataAdd(strData, Color.Blue);
            index += len;

            // 帧尾FCS
            len = 2;
            strData = Util.GetStringHexFromByte(databuf, index, len, " ");
            rtbRxdataAdd(strData, Color.Purple);
        }

        private void rtbRxdataAdd(string strInfo, Color colFore)
        {
            if (rtbRxdata.Text.Length > rtbRxdata.MaxLength - 100)
            {
                rtbRxdata.Clear();
            }
            int iStart = rtbRxdata.Text.Length;
            rtbRxdata.AppendText(strInfo);
            rtbRxdata.Select(iStart, rtbRxdata.Text.Length);
            rtbRxdata.SelectionColor = colFore;
        }

        private void StatusUpdate(string str1, Color color1, string str2 = null)
        {
            rtbRxdata.Clear();
            rtbRxdataAdd(str1, color1);
            if(str2 != null)
            {
                rtbRxdataAdd(str2, Color.Blue);
            }
        }

        #endregion

    }

    public static class DoubleBufferExpand
    {
        /// <summary>
        /// 双缓冲，解决闪烁问题
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="flag"></param>
        public static void DoubleBuffered(this  object obj, bool flag)
        {
            Type tp = obj.GetType();
            PropertyInfo pi = tp.GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(obj, flag, null);
        }
    }
}
