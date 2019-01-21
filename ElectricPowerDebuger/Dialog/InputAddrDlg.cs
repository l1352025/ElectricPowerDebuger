using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ElectricPowerLib.Common;
using ElectricPowerLib.Protocol;

namespace ElectricPowerDebuger.Dialog
{
    public partial class InputAddrDlg : Form
    {
        private string strDlgType = "";
        public delegate void strResult(string strAddr);
        public strResult NewAddress;
        public InputAddrDlg(string strType, string strNewAddr)
        {
            strDlgType = strType;
            InitializeComponent();
            this.Text = "请输入" + strType;
            tbAddress.Text = strNewAddr;
        }

        private void tbAddress_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ("0123456789\b\r\x03\x16\x18".IndexOf(e.KeyChar) < 0)
            {
                e.Handled = true;
                return;
            }
            if (e.KeyChar == '\r')
            {
                tbAddress.Text = tbAddress.Text.PadLeft(ProtoLocal_South.LongAddrSize * 2, '0');
                btOk.Focus();
                e.Handled = true;
            }
            if (tbAddress.Text.Length >= ProtoLocal_South.LongAddrSize * 2 && e.KeyChar != '\b')
            {
                if (tbAddress.SelectionLength == 0)
                {
                    e.Handled = true;
                }
            }
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            tbAddress.Text = tbAddress.Text.PadLeft(ProtoLocal_South.LongAddrSize * 2, '0');
            try
            {
                Convert.ToInt64(tbAddress.Text);
            }
            catch
            {
                tbAddress.Focus();
                tbAddress.Select(0, tbAddress.Text.Length);
                MessageBox.Show("您输入的地址格式有误，请重新输入！");
                return;
            }
            NewAddress(tbAddress.Text);
            this.Close();
        }

        private void InputAddrDlg_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Dispose();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
