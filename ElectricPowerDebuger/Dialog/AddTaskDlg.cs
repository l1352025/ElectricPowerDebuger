using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ElectricPowerLib.Common;

namespace ElectricPowerDebuger.Dialog
{
    public partial class AddTaskDlg : Form
    {
        public delegate void AddTask(byte[] taskArray);
        public AddTask addTaskProc;
        private string strTaskParam = "";
        private string strCustomDefineTask = "";
        // 抄读正向有功数据
        private string strReadMeterData0 = "11 04 33 34 33 33";
        public AddTaskDlg()
        {
            InitializeComponent();
        }

        private void AddTaskDlg_Load(object sender, EventArgs e)
        {
            strTaskParam = XmlHelper.GetNodeDefValue(FrmMain.SystemConfigPath, "/Config/ConcSimulator_TaskParam", "2,1,7200,0,0");
            strCustomDefineTask = XmlHelper.GetNodeDefValue(FrmMain.SystemConfigPath, "/Config/ConcSimulator_CustomDefineTask", "请在此输入自定义抄表数据，并以空格分隔。");
            string[] strSplit = strTaskParam.Split(',');
            // 优先级
            if (strSplit[0] == "0")
            {
                rbLevel0.Checked = true;
            }
            else if (strSplit[0] == "1")
            {
                rbLevel1.Checked = true;
            }
            else if (strSplit[0] == "2")
            {
                rbLevel2.Checked = true;
            }
            else
            {
                rbLevel3.Checked = true;
            }
            // 是否应答
            if (strSplit[1] == "0")
            {
                rbNoAck.Checked = true;
            }
            else
            {
                rbNeedAck.Checked = true;
            }
            // 超时时间
            nudOutTime.Value = int.Parse(strSplit[2]);
            // 任务类型
            if (strSplit[3] == "0")
            {
                cbTaskType.Text = cbTaskType.Items[0].ToString();
                rtbTaskContent.Enabled = false;
                lbNotice.Visible = false;
                rtbTaskContent.Text = strReadMeterData0;
            }
            else if (strSplit[3] == "FF")
            {
                cbTaskType.Text = cbTaskType.Items[cbTaskType.Items.Count - 1].ToString();
                rtbTaskContent.Enabled = true;
                lbNotice.Visible = true;
                rtbTaskContent.Text = strCustomDefineTask;
            }
            //单播/多播任务
            if (strSplit[4] == "0")
            {
                rbMultiTaskOne.Checked = true;
            }
            else if (strSplit[4] == "1")
            {
                rbMultiTaskSelected.Checked = true;
            }
            else if (strSplit[4] == "2")
            {
                rbMultiTaskAll.Checked = true;
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void AddTaskDlg_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Dispose();
        }

        private void cbTaskType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbTaskType.SelectedIndex == 0)          // 正向有功
            {
                rtbTaskContent.Enabled = false;
                lbNotice.Visible = false;
                rtbTaskContent.Text = strReadMeterData0;
            }
            else if (cbTaskType.SelectedIndex == cbTaskType.Items.Count - 1)        // 自定义抄表
            {
                rtbTaskContent.Enabled = true;
                lbNotice.Visible = true;
                rtbTaskContent.Text = strCustomDefineTask;
            }
        }

        private void rtbTaskContent_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (rtbTaskContent.Text.Length > 200)
            {
                e.Handled = true;
                MessageBox.Show("输入数据超出范围，请重新输入!");
                return;
            }
            if ("0123456789abcdefABCDEF\b\x20".IndexOf(e.KeyChar) < 0)
            {
                e.Handled = true;
                return;
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            int iLoop, iLen;
            string strTaskParamNew = "";
            string strCustomDefineTaskNew = "";
            byte[] bTaskParamArray = new byte[300];

            iLen = 0;
            try
            {
                bTaskParamArray[iLen] = 0;
                if (rbLevel0.Checked == true)
                {
                    strTaskParamNew = "0,";
                    bTaskParamArray[iLen] |= 0;
                }
                else if (rbLevel1.Checked == true)
                {
                    strTaskParamNew = "1,";
                    bTaskParamArray[iLen] |= 1;
                }
                else if (rbLevel2.Checked == true)
                {
                    strTaskParamNew = "2,";
                    bTaskParamArray[iLen] |= 2;
                }
                else
                {
                    strTaskParamNew = "3,";
                    bTaskParamArray[iLen] |= 3;
                }

                if (rbNoAck.Checked == true)
                {
                    strTaskParamNew += "0,";
                    bTaskParamArray[iLen] |= 0;
                }
                else
                {
                    strTaskParamNew += "1,";
                    bTaskParamArray[iLen] |= 0x80;
                }
                iLen += 1;
                int outTime = Convert.ToUInt16(nudOutTime.Value);
                strTaskParamNew += outTime.ToString("D") + ",";
                bTaskParamArray[iLen++] = (byte)outTime;
                bTaskParamArray[iLen++] = (byte)(outTime >> 8);

                rtbTaskContent.Text = rtbTaskContent.Text.TrimStart(' ');
                rtbTaskContent.Text = rtbTaskContent.Text.TrimEnd(' ');
                string[] strSplit = rtbTaskContent.Text.Split(' ');
                bTaskParamArray[iLen++] = (byte)strSplit.Length;
                for (iLoop = 0; iLoop < strSplit.Length; iLoop++)
                {
                    bTaskParamArray[iLen] = Convert.ToByte(strSplit[iLoop], 16);
                    strCustomDefineTaskNew += bTaskParamArray[iLen].ToString("X2") + " ";
                    iLen += 1;
                }
                strCustomDefineTaskNew = strCustomDefineTaskNew.TrimEnd(' ');
                if (cbTaskType.SelectedIndex == cbTaskType.Items.Count - 1)
                {
                    strTaskParamNew += "FF,";
                    if (strCustomDefineTask != strCustomDefineTaskNew)
                    {
                        XmlHelper.SetNodeValue(FrmMain.SystemConfigPath, "/Config", "ConcSimulator_CustomDefineTask", strCustomDefineTaskNew);
                    }
                }
                else
                {
                    strTaskParamNew += cbTaskType.SelectedIndex.ToString("D") + ",";
                }

                byte multiTaskFlag = 0;
                if (rbMultiTaskOne.Checked == true)
                {
                    strTaskParamNew += "0,";
                    multiTaskFlag = 0;
                }
                else if (rbMultiTaskSelected.Checked == true)
                {
                    strTaskParamNew += "1,";
                    multiTaskFlag = 1;
                }
                else if (rbMultiTaskAll.Checked == true)
                {
                    strTaskParamNew += "2,";
                    multiTaskFlag = 2;
                }
                
                if (strTaskParam != strTaskParamNew)
                {
                    XmlHelper.SetNodeValue(FrmMain.SystemConfigPath, "/Config", "ConcSimulator_TaskParam", strTaskParamNew);
                }
                
                byte[] taskContent = new byte[iLen + 2];
                Array.Copy(bTaskParamArray, 0, taskContent, 2, iLen);
                taskContent[0] = multiTaskFlag;             // 单播/多播标志

                addTaskProc(taskContent);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("报文格式不正确，" + ex.Message + "！");
            }
        }
    }
}
