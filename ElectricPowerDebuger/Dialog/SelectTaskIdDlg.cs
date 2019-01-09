using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ElectricPowerDebuger.Common;

namespace ElectricPowerDebuger.Dialog
{
    public partial class SelectTaskIdDlg : Form
    {
        public delegate void strSelectTaskId(string strTaskId);
        public strSelectTaskId SelectTaskIdProcess;

        public SelectTaskIdDlg(byte[] TaskIdArray, int TaskIdCount)
        {
            InitializeComponent();
            cmbTaskIdList.Items.Clear();
            cmbTaskIdList.AutoCompleteCustomSource.Clear();
            for (int iLoop = 0; iLoop < TaskIdCount / 2; iLoop++)
            {
                string strItem = Util.GetStringHexFromBytes(TaskIdArray, iLoop * 2, 2, "", true);
                cmbTaskIdList.Items.Add(strItem);
                cmbTaskIdList.AutoCompleteCustomSource.Add(strItem);
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SelectTaskIdDlg_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Dispose();
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            try
            {
                cmbTaskIdList.Text = cmbTaskIdList.Text.PadLeft(4, '0');
                SelectTaskIdProcess(cmbTaskIdList.Text);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("任务ID格式不正确，" + ex.Message + "！");
            }
        }

        private void cmbTaskIdList_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (cmbTaskIdList.Text.Length >= 4 && e.KeyChar != '\b')
            {
                e.Handled = true;
                return;
            }
            if ("0123456789abcdefABCDEF\b".IndexOf(e.KeyChar) < 0)
            {
                e.Handled = true;
                return;
            }
        }
    }
}
