using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ElectricPowerDebuger.Function
{
    public partial class ConcSimulator_North : UserControl
    {
        Control usrCtrl;

        public ConcSimulator_North()
        {
            InitializeComponent();

            FrmMain.ProtocolVerChanged += OnProtoVerChanged;
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
        private void cbxPortNum_Click(object sender, EventArgs e)
        {

        }

        private void btPortCtrl_Click(object sender, EventArgs e)
        {

        }
        #endregion 

        #region 循环抄表
        private void btLoopCtrl_Click(object sender, EventArgs e)
        {

        }
        #endregion

        #region 命令处理

        private void AllCmdButton_Click(object sender, EventArgs e)
        {
            AllCmdItem_Clicked(sender, null);
        }
        private void AllCmdItem_Clicked(object sender, ToolStripItemClickedEventArgs e)
        {
            string cmdText = "";

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

            switch(cmdText)
            {
                case "":

                    break;

                case "1":

                    break;

                default:

                    break;
            }

        }
        #endregion

        #region 有参数的命令处理


        #endregion
    }
}
