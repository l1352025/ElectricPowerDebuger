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

namespace ElectricPowerLib.Dialog
{
    public partial class InputTwoParamDlg : Form
    {
        private int lenTxtParam1;
        private int lenTxtParam2;

        public delegate void Callback(String strParam1, String strParam2);
        public Callback ParamsOutputProc;
        public string Text1 { get { return txtParam1.Text; } set { txtParam1.Text = value; } }
        public string Text2 { get { return txtParam2.Text; } set { txtParam2.Text = value; } }

        public InputTwoParamDlg(string title, string strLabel1, string strLabel2, int lenTxt1 = 12, int lenTxt2 = 3)
        {
            InitializeComponent();

            Text = title;
            lbParam1.Text = strLabel1;
            lbParam2.Text = strLabel2;
            lbParam1.TextAlign = ContentAlignment.MiddleRight;
            lbParam2.TextAlign = ContentAlignment.MiddleRight;
            lenTxtParam1 = lenTxt1;
            lenTxtParam2 = lenTxt2;
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
                    txtParam1.Text = txtParam1.Text.PadLeft(lenTxtParam1, '0');
                    txtParam2.Focus();
                    e.Handled = true;
                }
                if (txtParam1.Text.Length >= lenTxtParam1 && e.KeyChar != '\b')
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
                if (txtParam1.Text.Length >= lenTxtParam1 && e.KeyChar != '\b')
                {
                    if (txtParam1.SelectionLength == 0)
                    {
                        e.Handled = true;
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
                if (txtParam2.Text.Length >= lenTxtParam2 && e.KeyChar != '\b')
                {
                    if (txtParam1.SelectionLength == 0)
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            if (txtParam1.Text == "" || txtParam2.Text == "")
            {
                MessageBox.Show("请输入 [" + lbParam1.Text + "]  和  [" + lbParam2.Text + "] ！");
                return;
            }

            // 若 Param1 输入的是地址
            if (lbParam1.Text.IndexOf("地址") > 0)
            {
                txtParam1.Text = txtParam1.Text.PadLeft(lenTxtParam1, '0');
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
                txtParam2.Text = txtParam2.Text.Trim();
            }

            if (ParamsOutputProc != null)
            {
                ParamsOutputProc(txtParam1.Text, txtParam2.Text);
            }
            this.Close();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            txtParam1.Text = "";
            txtParam2.Text = "";
            this.Close();
        }

    }
}
