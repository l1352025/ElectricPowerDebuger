using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using ElectricPowerLib.Common;

namespace ElectricPowerDebuger.Function
{
    public partial class LogManager : UserControl
    {
        private SerialCom scom;
        private ConcurrentQueue<byte[]> recvQueue;
        public LogManager()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;

            scom = new SerialCom();
            scom.DataReceivedEvent += serialPort_DataReceived;
            recvQueue = new ConcurrentQueue<byte[]>();
        }

       
        #region 串口通信
        
        //串口选择
        private void combPortNum_Click(object sender, EventArgs e)
        {
            combPortNum.Items.Clear();
            combPortNum.Items.AddRange(SerialPort.GetPortNames());
        }
        
        //串口打开/关闭
        private void btPortCtrl_Click(object sender, EventArgs e)
        {
            if(combPortNum.Text == "" || combPortBaud.Text == "" || combPortChk.Text == "")
            {
                MessageBox.Show("请在端口设置完后打开");
                return;
            }

            if (btPortCtrl.Text == "打开" && true == PortCtrl(true))
            {
                btPortCtrl.Text = "关闭";
                btPortCtrl.BackColor = Color.GreenYellow;
                combPortNum.Enabled = false;
                combPortBaud.Enabled = false;
                combPortChk.Enabled = false;

                XmlHelper.SetNodeValue(FrmMain.SystemConfigPath, "/Config/LogManager", "PortName", combPortNum.Text);
                XmlHelper.SetNodeValue(FrmMain.SystemConfigPath, "/Config/LogManager", "Baudrate", combPortBaud.Text);
                XmlHelper.SetNodeValue(FrmMain.SystemConfigPath, "/Config/LogManager", "BitAndCheck", combPortChk.Text);
            }
            else
            {
                PortCtrl(false);
                btPortCtrl.Text = "打开";
                btPortCtrl.BackColor = Color.Silver;
                combPortNum.Enabled = true;
                combPortBaud.Enabled = true;
                combPortChk.Enabled = true;
            }
        }

        private bool PortCtrl(bool ctrl)
        {
            if (true == ctrl)
            {
                if (scom.IsOpen == false)
                {
                    try
                    {
                        scom.Config(combPortNum.Text, Convert.ToInt32(combPortBaud.Text), combPortChk.Text);
                        scom.Open();
                    }
                    catch (Exception ex)
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
                    scom.Close();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("关闭通信端口失败" + "," + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return true;
        }

        //串口发送
        private void serialPort_SendData(byte[] buf, int index, int len)
        {
            if (true == scom.IsOpen)
            {
                try
                {
                    scom.WritePort(buf, index, len);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("发送错误:" + ex.Message);
                }
            }
        }

        //串口接收
        private void serialPort_DataReceived(byte[] buf)
        {
            recvQueue.Enqueue(buf);
        }

        #endregion

        #region 日志管理

        //读取规则变化
        private void rbReadByHour_CheckedChanged(object sender, EventArgs e)
        {
            if(false == rbReadByHour.Checked)
            {
                foreach (int i in chkListHour.CheckedIndices)
                {
                    chkListHour.SetItemChecked(i, false);
                }
            }
            chkListHour.Enabled = rbReadByHour.Checked;
        }

        //读取日志
        private void btLogRead_Click(object sender, EventArgs e)
        {

        }

        //保存日志
        private void btLogSave_Click(object sender, EventArgs e)
        {

        }

        //载入日志
        private void btLogLoad_Click(object sender, EventArgs e)
        {

        }
        #endregion

        #region 组网统计
        //组网统计树--更新
        private void TreeNetCntUpdate()
        {

        }

        #endregion 

        #region 日志显示

        //日志列表
        private void Log2List()
        {

        }

        //日志文本
        private void Log2Text()
        {

        }

        //日志协议树
        private void Log2Tree()
        {

        }

        #endregion 

    }
}
