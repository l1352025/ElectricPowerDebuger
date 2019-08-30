using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ElectricPowerLib.Dialog
{
    /// <summary>
    /// 倒计时关闭的消息提示框
    /// </summary>
    public partial class TimeoutMsgBox : Form
    {
        Timer _timer;
        uint _timerCnt;
        uint _timeoutSec;

        /// <summary>
        /// TimeoutMsgBox 实例化
        /// </summary>
        /// <param name="msg">提示消息</param>
        /// <param name="timeoutSec">倒计时关闭时间，单位s</param>
        /// <param name="bgcolor">提示框背景色</param>
        private TimeoutMsgBox(string msg, uint timeoutSec, Color bgcolor)
            :this(msg, timeoutSec)
        {
            this.BackColor = bgcolor;
        }

        /// <summary>
        /// TimeoutMsgBox 实例化
        /// </summary>
        /// <param name="msg">提示消息</param>
        /// <param name="timeoutSec">倒计时关闭时间，单位s</param>
        private TimeoutMsgBox(string msg, uint timeoutSec)
        {
            InitializeComponent();

            lbMsg.Text = msg;
            lbTimeout.Text = timeoutSec.ToString();
            _timeoutSec = timeoutSec;

            this.StartPosition = FormStartPosition.CenterParent;

            Point p = new Point();
            p.X = (lbMsg.Parent.Width - lbMsg.Width) / 2;
            p.Y = (lbMsg.Parent.Height - lbMsg.Height) / 2;
            lbMsg.Location = p;

            _timer = new Timer();
            _timer.Interval = 100;
            _timer.Tick += OnTick;
            _timer.Start();
            _timerCnt = 0;
        }

        /// <summary>
        /// 弹框显示提示消息，倒计时完成后关闭
        /// </summary>
        /// <param name="msg">提示消息</param>
        /// <param name="timeoutSec">倒计时关闭时间，单位s</param>
        public static void Show(string msg, uint timeoutSec)
        {
            Show(msg, timeoutSec, Color.Black);
        }

        /// <summary>
        /// 弹框显示提示消息，倒计时完成后关闭
        /// </summary>
        /// <param name="msg">提示消息</param>
        /// <param name="timeoutSec">倒计时关闭时间，单位s</param>
        /// <param name="bgcolor">提示框背景色</param>
        public static void Show(string msg, uint timeoutSec, Color bgcolor)
        {
            TimeoutMsgBox msgbox = new TimeoutMsgBox(msg, timeoutSec, bgcolor);
            msgbox.ShowDialog();
            msgbox.Focus();
        }

        delegate void InvokeUpdateUi(string msg);
        private void TimeoutValUpdate(string msg)
        {
            if(InvokeRequired)
            {
                Invoke(new InvokeUpdateUi(TimeoutValUpdate), msg);
                return;
            }

            lbTimeout.Text = msg;
        }

        private void OnTick(object sender, EventArgs e)
        {
            _timerCnt++;

            if (_timeoutSec > 0 && _timerCnt % 10 == 0)  // 1s
            {
                _timeoutSec--;
                TimeoutValUpdate(_timeoutSec.ToString());
            }

            if (_timeoutSec == 0)
            {
                _timer.Stop();
                this.Close();
            }
        }

    }
}
