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
    /// <summary>
    /// 表示有两个文本输入框的对话框
    /// </summary>
    public partial class InputTwoParamDlg : Form
    {
        private int lenTxtParam1;
        private int lenTxtParam2;

        /// <summary>
        /// 参数输出回调函数的方法
        /// </summary>
        /// <param name="strParam1">输入框1的字符串</param>
        /// <param name="strParam2">输入框2的字符串</param>
        public delegate void Callback(String strParam1, String strParam2);

        /// <summary>
        /// 输入完成并确定后，参数输出时调用
        /// </summary>
        public Callback ParamsOutputProc;

        /// <summary>
        /// 获取或设置 输入框1 的字符串
        /// </summary>
        public string Text1 { get { return txtParam1.Text; } set { txtParam1.Text = value; } }

        /// <summary>
        /// 获取或设置 输入框2 的字符串
        /// </summary>
        public string Text2 { get { return txtParam2.Text; } set { txtParam2.Text = value; } }

        /// <summary>
        /// 初始化 InputTwoParamDlg 类的新实例
        /// </summary>
        /// <param name="title">对话框标题</param>
        /// <param name="strLabel1">输入框1标签</param>
        /// <param name="strLabel2">输入框2标签</param>
        /// <param name="lenTxt1">输入框1可输入字符长度</param>
        /// <param name="lenTxt2">输入框2可输入字符长度</param>
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
                    if (txtParam2.SelectionLength == 0)
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

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            txtParam1.Text = "";
            txtParam2.Text = "";

            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

    }
}
