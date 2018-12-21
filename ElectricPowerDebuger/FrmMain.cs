using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ElectricPowerDebuger.Function;

namespace ElectricPowerDebuger
{
    public partial class FrmMain : Form
    {
        public static string SystemConfigPath = Application.StartupPath +  @"\Config.xml";  // 配置文件路径

        Control concSimulator = new ConcSimulator();
        Control dataMonitor = new DataMonitor();
        Control logManager = new LogManager();

        public delegate void FormEventNotify(string msg);
        public static event FormEventNotify ProtocolVerChanged;
        public static event FormEventNotify LogAutoSaveStateChanged;

        public FrmMain()
        {
            InitializeComponent();
            this.Text = Application.ProductName + "_Ver" + Application.ProductVersion + "   " + Application.CompanyName;
            this.tabPage1.Controls.Add(concSimulator);
            this.tabPage2.Controls.Add(dataMonitor);
            this.tabPage3.Controls.Add(logManager);

            Common.XmlHelper.CheckXmlFile(SystemConfigPath);
        }

        private void combProtoVer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(combProtoVer.Text))
            {
                return;
            }

            Common.XmlHelper.SetNodeValue(SystemConfigPath, "Config/Global", "ProtocolVer", combProtoVer.Text);

            this.tabPage1.Controls.Remove(concSimulator);

            concSimulator.Dispose();

            switch (combProtoVer.Text)
            {
                case "南网-版本":
                    concSimulator = new ConcSimulator();
                    break;

                case "北网-版本":
                case "尼泊尔-版本":
                default:
                    concSimulator = new ConcSimulator_North();
                    break;
            }

            this.tabPage1.Controls.Add(concSimulator);
            
            if (ProtocolVerChanged != null)
            {
            //    ProtocolVerChanged(combProtoVer.Text);
            }
        }

        private void chkAutoSave_CheckedChanged(object sender, EventArgs e)
        {
            string isAutoSave = chkAutoSave.Checked ? "true" : "false";
            Common.XmlHelper.SetNodeValue(SystemConfigPath, "Config/Global", "AutoSaveLog", isAutoSave);

            if(LogAutoSaveStateChanged != null)
            {
                LogAutoSaveStateChanged(isAutoSave);
            }
        }
    }
}
