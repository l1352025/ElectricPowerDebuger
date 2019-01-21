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
    public partial class InputTwoParamDlg : Form
    {
        public delegate void ParamsOutput(String strParam1, String strParam2);
        public ParamsOutput ParamsOutputProc;

        private string strLabelParam1;
        private string strLabelParam2;

        public InputTwoParamDlg(string strLabelParam1, string strLabelParam2)
        {
            InitializeComponent();
            lbParam1.Text = strLabelParam1 + "：";
            lbParam2.Text = strLabelParam2 + "：";
            lbParam1.TextAlign = ContentAlignment.MiddleRight;
            lbParam2.TextAlign = ContentAlignment.MiddleRight;

            this.strLabelParam1 = strLabelParam1;
            this.strLabelParam2 = strLabelParam2;
        }

        private void InputTwoParamDlg_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Dispose();
        }

        private void txtParam1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 若输入的是地址
            if (lbParam1.Text.IndexOf("地址") > 0)
            {
                if ("0123456789\b\r\x03\x16\x18".IndexOf(e.KeyChar) < 0)
                {
                    e.Handled = true;
                    return;
                }
                if (e.KeyChar == '\r')
                {
                    txtParam1.Text = txtParam1.Text.PadLeft(ProtoLocal_South.LongAddrSize * 2, '0');
                    txtParam2.Focus();
                    e.Handled = true;
                }
                if (txtParam1.Text.Length >= ProtoLocal_South.LongAddrSize * 2 && e.KeyChar != '\b')
                {
                    if (txtParam1.SelectionLength == 0)
                    {
                        e.Handled = true;
                    }
                }
            }
            // 若输入的是整形数
            else
            {
                if ("0123456789\b\r".IndexOf(e.KeyChar) < 0)
                {
                    e.Handled = true;
                    return;
                }
                if (e.KeyChar == '\r')
                {
                    txtParam2.Focus();
                    e.Handled = true;
                }
                if (txtParam1.Text.Length >= 5 && e.KeyChar != '\b')
                {
                    e.Handled = true;
                    if (Convert.ToUInt16(txtParam1.Text) > 0xFFFF)
                    {
                        MessageBox.Show("数字超出范围(0~65535)，请重新输入!");
                        return;
                    }
                }
            }
        }

        private void txtParam2_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 若输入的是报文内容
            if(lbParam2.Text.IndexOf("内容") > 0 )
            {
                if ("0123456789abcdefABCDEF\b\r\x20".IndexOf(e.KeyChar) < 0)
                {
                    e.Handled = true;
                    return;
                }
                if (e.KeyChar == '\r')
                {
                    btOk.Focus();
                    e.Handled = true;
                }
                if (txtParam2.Text.Length >= 200 && e.KeyChar != '\b')
                {
                    e.Handled = true;
                    MessageBox.Show("长度超出范围（0~200），请重新输入!");
                }
               
            }
            // 若输入的是整形数
            else
            {
                if ("0123456789\b\r".IndexOf(e.KeyChar) < 0)
                {
                    e.Handled = true;
                    return;
                }
                if (e.KeyChar == '\r')
                {
                    btOk.Focus();
                    e.Handled = true;
                }
                if (txtParam2.Text.Length >= 5 && e.KeyChar != '\b')
                {
                    e.Handled = true;
                    if (Convert.ToUInt16(txtParam1.Text) > 0xFFFF)
                    {
                        MessageBox.Show("数字超出范围(0~65535)，请重新输入!");
                        return;
                    }
                }
            }
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            if (txtParam1.Text == "" || txtParam2.Text == "")
            {
                MessageBox.Show("请输入的 [" + strLabelParam1 + "] 和 [" + strLabelParam2 + "] ！");
                return;
            }

            // 若 Param1 输入的是地址
            if (lbParam1.Text.IndexOf("地址") > 0)
            {
                txtParam1.Text = txtParam1.Text.PadLeft(ProtoLocal_South.LongAddrSize * 2, '0');
                try
                {
                    Convert.ToInt64(txtParam1.Text);
                }
                catch
                {
                    txtParam1.Focus();
                    txtParam1.Select(0, txtParam1.Text.Length);
                    MessageBox.Show("您输入的地址格式有误，请重新输入！");
                    return;
                }
            }

            // 若 Param2 输入的是报文内容
            if (lbParam2.Text.IndexOf("内容") > 0)
            {
                txtParam2.Text = txtParam2.Text.Trim(' ');
            }

            ParamsOutputProc(txtParam1.Text, txtParam2.Text);
            this.Close();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
