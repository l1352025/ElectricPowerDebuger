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
    }
}
