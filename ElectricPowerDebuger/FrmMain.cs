using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ElectricPowerDebuger.Function;
using ElectricPowerDebuger.Common;

namespace ElectricPowerDebuger
{
    public partial class FrmMain : Form
    {
        public static string SystemConfigPath = Application.ExecutablePath.Remove(Application.ExecutablePath.Length - 4) + @".cfg";  // 配置文件路径

        private static Control concSimulatorCurrent = new ConcSimulator_North();
        private static Control dataMonitor = new DataMonitor();
        private static Control logManager = new LogManager();

        public delegate void FormEventNotify(string msg);
        //public static event FormEventNotify ProtocolVerChanged;
        public static event FormEventNotify LogAutoSaveStateChanged;

        public FrmMain()
        {
            //在InitializeComponent()之前调用
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            InitializeComponent();
            this.Text = Application.ProductName + "_Ver" + Application.ProductVersion + "   " + Application.CompanyName;
            this.tabPage1.Controls.Add(concSimulatorCurrent);
            this.tabPage2.Controls.Add(dataMonitor);
            this.tabPage3.Controls.Add(logManager);

            XmlHelper.CheckXmlFile(SystemConfigPath);

            combProtoVer.Text = XmlHelper.GetNodeDefValue(SystemConfigPath, "Config/Global/ProtocolVer", "北网-版本");
        }

        private void combProtoVer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(combProtoVer.Text))
            {
                return;
            }

            XmlHelper.SetNodeValue(SystemConfigPath, "Config/Global", "ProtocolVer", combProtoVer.Text);

            this.tabPage1.Controls.Remove(concSimulatorCurrent);

            concSimulatorCurrent.Dispose();

            switch (combProtoVer.Text)
            {
                case "南网-版本":
                    concSimulatorCurrent = new ConcSimulator();
                    break;

                case "北网-版本":
                case "尼泊尔-版本":
                default:
                    concSimulatorCurrent = new ConcSimulator_North();
                    break;
            }

            this.tabPage1.Controls.Add(concSimulatorCurrent);
            
        }

        private void chkAutoSave_CheckedChanged(object sender, EventArgs e)
        {
            string isAutoSave = chkAutoSave.Checked ? "true" : "false";
            XmlHelper.SetNodeValue(SystemConfigPath, "Config/Global", "AutoSaveLog", isAutoSave);

            if(LogAutoSaveStateChanged != null)
            {
                LogAutoSaveStateChanged(isAutoSave);
            }
        }

        System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllName = args.Name.Contains(",") ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name.Replace(".dll", "");
            dllName = dllName.Replace(".", "_");
            
            if (dllName.EndsWith("_resources")) return null;

            System.Resources.ResourceManager rm = new System.Resources.ResourceManager(GetType().Namespace + ".Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());
            byte[] bytes = (byte[])rm.GetObject(dllName);
            return System.Reflection.Assembly.Load(bytes);
        }

    }
}
