namespace ElectricPowerDebuger.Function
{
    partial class DataMonitor
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);

            this.Close();
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.btModuleChk = new System.Windows.Forms.Button();
            this.btClearLog = new System.Windows.Forms.Button();
            this.btLoadLog = new System.Windows.Forms.Button();
            this.btSaveLog = new System.Windows.Forms.Button();
            this.btOpenPort2 = new System.Windows.Forms.Button();
            this.btOpenPort = new System.Windows.Forms.Button();
            this.combChanel2 = new System.Windows.Forms.ComboBox();
            this.combSpeed = new System.Windows.Forms.ComboBox();
            this.cmbChanel = new System.Windows.Forms.ComboBox();
            this.combPort2 = new System.Windows.Forms.ComboBox();
            this.lbChanel = new System.Windows.Forms.Label();
            this.cmbPort = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.lbPort = new System.Windows.Forms.Label();
            this.lvDataList = new System.Windows.Forms.ListView();
            this.序号 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.日期 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.时间 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.信道组 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.频点 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.场强值 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.包长 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.源地址 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.目的地址 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.PanID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.帧序号 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.帧类型 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.源名称 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.目的名称 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.备注 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cnMenuDataList = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.保存ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.载入ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.删除ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.清空ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btScroll = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.rtbRxdata = new System.Windows.Forms.RichTextBox();
            this.treeVwrProtol = new System.Windows.Forms.TreeView();
            this.lbProtolParse = new System.Windows.Forms.Label();
            this.serialPort = new System.IO.Ports.SerialPort(this.components);
            this.openFileDlg = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDlg = new System.Windows.Forms.SaveFileDialog();
            this.timerDataMonitor = new System.Windows.Forms.Timer(this.components);
            this.dataSet = new System.Data.DataSet();
            this.tbLog = new System.Data.DataTable();
            this.colSN = new System.Data.DataColumn();
            this.colDate = new System.Data.DataColumn();
            this.colTime = new System.Data.DataColumn();
            this.colPacket = new System.Data.DataColumn();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.cnMenuDataList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbLog)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.SystemColors.ControlLight;
            this.splitContainer1.Panel2.Controls.Add(this.treeVwrProtol);
            this.splitContainer1.Panel2.Controls.Add(this.lbProtolParse);
            this.splitContainer1.Size = new System.Drawing.Size(1363, 765);
            this.splitContainer1.SplitterDistance = 1081;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.splitContainer2.Panel1.Controls.Add(this.btModuleChk);
            this.splitContainer2.Panel1.Controls.Add(this.btClearLog);
            this.splitContainer2.Panel1.Controls.Add(this.btLoadLog);
            this.splitContainer2.Panel1.Controls.Add(this.btSaveLog);
            this.splitContainer2.Panel1.Controls.Add(this.btOpenPort2);
            this.splitContainer2.Panel1.Controls.Add(this.btOpenPort);
            this.splitContainer2.Panel1.Controls.Add(this.combChanel2);
            this.splitContainer2.Panel1.Controls.Add(this.combSpeed);
            this.splitContainer2.Panel1.Controls.Add(this.cmbChanel);
            this.splitContainer2.Panel1.Controls.Add(this.combPort2);
            this.splitContainer2.Panel1.Controls.Add(this.lbChanel);
            this.splitContainer2.Panel1.Controls.Add(this.cmbPort);
            this.splitContainer2.Panel1.Controls.Add(this.label12);
            this.splitContainer2.Panel1.Controls.Add(this.label9);
            this.splitContainer2.Panel1.Controls.Add(this.label11);
            this.splitContainer2.Panel1.Controls.Add(this.label13);
            this.splitContainer2.Panel1.Controls.Add(this.label10);
            this.splitContainer2.Panel1.Controls.Add(this.lbPort);
            this.splitContainer2.Panel1.Controls.Add(this.lvDataList);
            this.splitContainer2.Panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.BackColor = System.Drawing.SystemColors.ControlLight;
            this.splitContainer2.Panel2.Controls.Add(this.btScroll);
            this.splitContainer2.Panel2.Controls.Add(this.label6);
            this.splitContainer2.Panel2.Controls.Add(this.label5);
            this.splitContainer2.Panel2.Controls.Add(this.label7);
            this.splitContainer2.Panel2.Controls.Add(this.label4);
            this.splitContainer2.Panel2.Controls.Add(this.label3);
            this.splitContainer2.Panel2.Controls.Add(this.label8);
            this.splitContainer2.Panel2.Controls.Add(this.label2);
            this.splitContainer2.Panel2.Controls.Add(this.label1);
            this.splitContainer2.Panel2.Controls.Add(this.rtbRxdata);
            this.splitContainer2.Panel2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.splitContainer2.Size = new System.Drawing.Size(1079, 763);
            this.splitContainer2.SplitterDistance = 619;
            this.splitContainer2.TabIndex = 0;
            // 
            // btModuleChk
            // 
            this.btModuleChk.Location = new System.Drawing.Point(142, 31);
            this.btModuleChk.Name = "btModuleChk";
            this.btModuleChk.Size = new System.Drawing.Size(70, 22);
            this.btModuleChk.TabIndex = 5;
            this.btModuleChk.Text = "模块检测";
            this.btModuleChk.UseVisualStyleBackColor = true;
            this.btModuleChk.Click += new System.EventHandler(this.btModuleChk_Click);
            // 
            // btClearLog
            // 
            this.btClearLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btClearLog.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.btClearLog.Location = new System.Drawing.Point(1014, 7);
            this.btClearLog.Name = "btClearLog";
            this.btClearLog.Size = new System.Drawing.Size(62, 23);
            this.btClearLog.TabIndex = 3;
            this.btClearLog.Text = "清除日志";
            this.btClearLog.UseVisualStyleBackColor = false;
            this.btClearLog.Click += new System.EventHandler(this.btClearLog_Click);
            // 
            // btLoadLog
            // 
            this.btLoadLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btLoadLog.BackColor = System.Drawing.Color.SeaShell;
            this.btLoadLog.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btLoadLog.Location = new System.Drawing.Point(944, 7);
            this.btLoadLog.Name = "btLoadLog";
            this.btLoadLog.Size = new System.Drawing.Size(64, 23);
            this.btLoadLog.TabIndex = 3;
            this.btLoadLog.Text = "导入日志";
            this.btLoadLog.UseVisualStyleBackColor = false;
            this.btLoadLog.Click += new System.EventHandler(this.btLoadLog_Click);
            // 
            // btSaveLog
            // 
            this.btSaveLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btSaveLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btSaveLog.Location = new System.Drawing.Point(875, 7);
            this.btSaveLog.Name = "btSaveLog";
            this.btSaveLog.Size = new System.Drawing.Size(63, 23);
            this.btSaveLog.TabIndex = 3;
            this.btSaveLog.Text = "保存日志";
            this.btSaveLog.UseVisualStyleBackColor = false;
            this.btSaveLog.Click += new System.EventHandler(this.btSaveLog_Click);
            // 
            // btOpenPort2
            // 
            this.btOpenPort2.BackColor = System.Drawing.Color.Silver;
            this.btOpenPort2.Location = new System.Drawing.Point(625, 5);
            this.btOpenPort2.Name = "btOpenPort2";
            this.btOpenPort2.Size = new System.Drawing.Size(72, 23);
            this.btOpenPort2.TabIndex = 3;
            this.btOpenPort2.Text = "打开串口";
            this.btOpenPort2.UseVisualStyleBackColor = false;
            this.btOpenPort2.Click += new System.EventHandler(this.btOpenPort2_Click);
            // 
            // btOpenPort
            // 
            this.btOpenPort.BackColor = System.Drawing.Color.Silver;
            this.btOpenPort.Location = new System.Drawing.Point(229, 7);
            this.btOpenPort.Name = "btOpenPort";
            this.btOpenPort.Size = new System.Drawing.Size(72, 23);
            this.btOpenPort.TabIndex = 3;
            this.btOpenPort.Text = "打开串口";
            this.btOpenPort.UseVisualStyleBackColor = false;
            this.btOpenPort.Click += new System.EventHandler(this.btOpenPort_Click);
            // 
            // combChanel2
            // 
            this.combChanel2.FormattingEnabled = true;
            this.combChanel2.Items.AddRange(new object[] {
            "484.7 (公共)",
            "489.7 (App时Rx)",
            "486.9 (Boot时Rx)",
            "489.2 (Boot时Tx)"});
            this.combChanel2.Location = new System.Drawing.Point(461, 33);
            this.combChanel2.Name = "combChanel2";
            this.combChanel2.Size = new System.Drawing.Size(102, 20);
            this.combChanel2.TabIndex = 2;
            this.combChanel2.SelectedIndexChanged += new System.EventHandler(this.combChanel2_SelectedIndexChanged);
            // 
            // combSpeed
            // 
            this.combSpeed.FormattingEnabled = true;
            this.combSpeed.Items.AddRange(new object[] {
            "10K",
            "25K"});
            this.combSpeed.Location = new System.Drawing.Point(606, 32);
            this.combSpeed.Name = "combSpeed";
            this.combSpeed.Size = new System.Drawing.Size(51, 20);
            this.combSpeed.TabIndex = 2;
            this.combSpeed.SelectedIndexChanged += new System.EventHandler(this.combSpeed_SelectedIndexChanged);
            // 
            // cmbChanel
            // 
            this.cmbChanel.FormattingEnabled = true;
            this.cmbChanel.Items.AddRange(new object[] {
            "轮询",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28",
            "29",
            "30",
            "31",
            "32"});
            this.cmbChanel.Location = new System.Drawing.Point(68, 33);
            this.cmbChanel.Name = "cmbChanel";
            this.cmbChanel.Size = new System.Drawing.Size(60, 20);
            this.cmbChanel.TabIndex = 2;
            this.cmbChanel.SelectedIndexChanged += new System.EventHandler(this.combChanel_SelectedIndexChanged);
            // 
            // combPort2
            // 
            this.combPort2.FormattingEnabled = true;
            this.combPort2.Location = new System.Drawing.Point(461, 5);
            this.combPort2.Name = "combPort2";
            this.combPort2.Size = new System.Drawing.Size(60, 20);
            this.combPort2.TabIndex = 2;
            this.combPort2.Click += new System.EventHandler(this.combPort2_Click);
            // 
            // lbChanel
            // 
            this.lbChanel.AutoSize = true;
            this.lbChanel.Location = new System.Drawing.Point(17, 36);
            this.lbChanel.Name = "lbChanel";
            this.lbChanel.Size = new System.Drawing.Size(53, 12);
            this.lbChanel.TabIndex = 1;
            this.lbChanel.Text = "信道组：";
            // 
            // cmbPort
            // 
            this.cmbPort.FormattingEnabled = true;
            this.cmbPort.Location = new System.Drawing.Point(68, 8);
            this.cmbPort.Name = "cmbPort";
            this.cmbPort.Size = new System.Drawing.Size(60, 20);
            this.cmbPort.TabIndex = 2;
            this.cmbPort.Click += new System.EventHandler(this.cmbPort_Click);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(569, 36);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(41, 12);
            this.label12.TabIndex = 1;
            this.label12.Text = "速率：";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(533, 10);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(77, 12);
            this.label9.TabIndex = 1;
            this.label9.Text = "19200bps 8E1";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(423, 35);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(41, 12);
            this.label11.TabIndex = 1;
            this.label11.Text = "信道：";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(140, 13);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(77, 12);
            this.label13.TabIndex = 1;
            this.label13.Text = "19200bps 8E1";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(399, 10);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(65, 12);
            this.label10.TabIndex = 1;
            this.label10.Text = "水表监控：";
            // 
            // lbPort
            // 
            this.lbPort.AutoSize = true;
            this.lbPort.Location = new System.Drawing.Point(5, 13);
            this.lbPort.Name = "lbPort";
            this.lbPort.Size = new System.Drawing.Size(65, 12);
            this.lbPort.TabIndex = 1;
            this.lbPort.Text = "电表监控：";
            // 
            // lvDataList
            // 
            this.lvDataList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvDataList.BackColor = System.Drawing.SystemColors.Control;
            this.lvDataList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.序号,
            this.日期,
            this.时间,
            this.信道组,
            this.频点,
            this.场强值,
            this.包长,
            this.源地址,
            this.目的地址,
            this.PanID,
            this.帧序号,
            this.帧类型,
            this.源名称,
            this.目的名称,
            this.备注});
            this.lvDataList.ContextMenuStrip = this.cnMenuDataList;
            this.lvDataList.FullRowSelect = true;
            this.lvDataList.GridLines = true;
            this.lvDataList.Location = new System.Drawing.Point(0, 58);
            this.lvDataList.Name = "lvDataList";
            this.lvDataList.Size = new System.Drawing.Size(1079, 558);
            this.lvDataList.TabIndex = 0;
            this.lvDataList.UseCompatibleStateImageBehavior = false;
            this.lvDataList.View = System.Windows.Forms.View.Details;
            this.lvDataList.SelectedIndexChanged += new System.EventHandler(this.lvDataList_SelectedIndexChanged);
            // 
            // 序号
            // 
            this.序号.Text = "序号";
            this.序号.Width = 47;
            // 
            // 日期
            // 
            this.日期.Text = "日期";
            this.日期.Width = 72;
            // 
            // 时间
            // 
            this.时间.Text = "时间";
            this.时间.Width = 86;
            // 
            // 信道组
            // 
            this.信道组.Text = "信道组";
            // 
            // 频点
            // 
            this.频点.Text = "频点";
            this.频点.Width = 37;
            // 
            // 场强值
            // 
            this.场强值.Text = "场强";
            this.场强值.Width = 37;
            // 
            // 包长
            // 
            this.包长.Text = "包长";
            this.包长.Width = 37;
            // 
            // 源地址
            // 
            this.源地址.Text = "源地址";
            this.源地址.Width = 100;
            // 
            // 目的地址
            // 
            this.目的地址.Text = "目的地址";
            this.目的地址.Width = 100;
            // 
            // PanID
            // 
            this.PanID.Text = "PanID";
            this.PanID.Width = 50;
            // 
            // 帧序号
            // 
            this.帧序号.Text = "帧序号";
            this.帧序号.Width = 50;
            // 
            // 帧类型
            // 
            this.帧类型.Text = "帧类型";
            this.帧类型.Width = 220;
            // 
            // 源名称
            // 
            this.源名称.Text = "源";
            this.源名称.Width = 40;
            // 
            // 目的名称
            // 
            this.目的名称.Text = "目的";
            this.目的名称.Width = 40;
            // 
            // 备注
            // 
            this.备注.Text = "备注";
            this.备注.Width = 210;
            // 
            // cnMenuDataList
            // 
            this.cnMenuDataList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.保存ToolStripMenuItem,
            this.载入ToolStripMenuItem,
            this.删除ToolStripMenuItem,
            this.清空ToolStripMenuItem});
            this.cnMenuDataList.Name = "contextMenuStrip1";
            this.cnMenuDataList.Size = new System.Drawing.Size(101, 92);
            // 
            // 保存ToolStripMenuItem
            // 
            this.保存ToolStripMenuItem.Name = "保存ToolStripMenuItem";
            this.保存ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.保存ToolStripMenuItem.Text = "保存";
            this.保存ToolStripMenuItem.Click += new System.EventHandler(this.保存ToolStripMenuItem_Click);
            // 
            // 载入ToolStripMenuItem
            // 
            this.载入ToolStripMenuItem.Name = "载入ToolStripMenuItem";
            this.载入ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.载入ToolStripMenuItem.Text = "载入";
            this.载入ToolStripMenuItem.Click += new System.EventHandler(this.载入ToolStripMenuItem_Click);
            // 
            // 删除ToolStripMenuItem
            // 
            this.删除ToolStripMenuItem.Name = "删除ToolStripMenuItem";
            this.删除ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.删除ToolStripMenuItem.Text = "删除";
            this.删除ToolStripMenuItem.Click += new System.EventHandler(this.删除ToolStripMenuItem_Click);
            // 
            // 清空ToolStripMenuItem
            // 
            this.清空ToolStripMenuItem.Name = "清空ToolStripMenuItem";
            this.清空ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.清空ToolStripMenuItem.Text = "清空";
            this.清空ToolStripMenuItem.Click += new System.EventHandler(this.清空ToolStripMenuItem_Click);
            // 
            // btScroll
            // 
            this.btScroll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btScroll.ForeColor = System.Drawing.Color.Green;
            this.btScroll.Location = new System.Drawing.Point(1014, -4);
            this.btScroll.Name = "btScroll";
            this.btScroll.Size = new System.Drawing.Size(67, 22);
            this.btScroll.TabIndex = 3;
            this.btScroll.Text = "停止滚动";
            this.btScroll.UseVisualStyleBackColor = true;
            this.btScroll.Click += new System.EventHandler(this.btScroll_Click);
            // 
            // label6
            // 
            this.label6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label6.ForeColor = System.Drawing.Color.Blue;
            this.label6.Location = new System.Drawing.Point(329, 5);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(49, 14);
            this.label6.TabIndex = 2;
            this.label6.Text = "载荷";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label5.ForeColor = System.Drawing.Color.Purple;
            this.label5.Location = new System.Drawing.Point(384, 5);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(37, 14);
            this.label5.TabIndex = 2;
            this.label5.Text = "CRC16";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label7.ForeColor = System.Drawing.Color.Green;
            this.label7.Location = new System.Drawing.Point(274, 5);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(49, 14);
            this.label7.TabIndex = 2;
            this.label7.Text = "Aps帧头";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label4.ForeColor = System.Drawing.Color.Orange;
            this.label4.Location = new System.Drawing.Point(219, 5);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 14);
            this.label4.TabIndex = 2;
            this.label4.Text = "Nwk帧头";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label3.ForeColor = System.Drawing.Color.Red;
            this.label3.Location = new System.Drawing.Point(164, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 14);
            this.label3.TabIndex = 2;
            this.label3.Text = "Mac帧头";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label8.ForeColor = System.Drawing.Color.LimeGreen;
            this.label8.Location = new System.Drawing.Point(72, 5);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(31, 14);
            this.label8.TabIndex = 2;
            this.label8.Text = "包头";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label2.ForeColor = System.Drawing.Color.Purple;
            this.label2.Location = new System.Drawing.Point(109, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 14);
            this.label2.TabIndex = 2;
            this.label2.Text = "Phy帧头";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "原始报文：";
            // 
            // rtbRxdata
            // 
            this.rtbRxdata.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbRxdata.BackColor = System.Drawing.SystemColors.Control;
            this.rtbRxdata.Location = new System.Drawing.Point(0, 20);
            this.rtbRxdata.Name = "rtbRxdata";
            this.rtbRxdata.Size = new System.Drawing.Size(1079, 117);
            this.rtbRxdata.TabIndex = 0;
            this.rtbRxdata.Text = "";
            // 
            // treeVwrProtol
            // 
            this.treeVwrProtol.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeVwrProtol.Location = new System.Drawing.Point(0, 38);
            this.treeVwrProtol.Name = "treeVwrProtol";
            this.treeVwrProtol.Size = new System.Drawing.Size(277, 725);
            this.treeVwrProtol.TabIndex = 2;
            // 
            // lbProtolParse
            // 
            this.lbProtolParse.AutoSize = true;
            this.lbProtolParse.Location = new System.Drawing.Point(12, 16);
            this.lbProtolParse.Name = "lbProtolParse";
            this.lbProtolParse.Size = new System.Drawing.Size(65, 12);
            this.lbProtolParse.TabIndex = 1;
            this.lbProtolParse.Text = "协议解析：";
            // 
            // serialPort
            // 
            this.serialPort.BaudRate = 19200;
            this.serialPort.Parity = System.IO.Ports.Parity.Even;
            this.serialPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.serialPort_DataReceived);
            // 
            // openFileDlg
            // 
            this.openFileDlg.FileName = "openFileDlg";
            // 
            // timerDataMonitor
            // 
            this.timerDataMonitor.Enabled = true;
            this.timerDataMonitor.Interval = 10;
            this.timerDataMonitor.Tick += new System.EventHandler(this.timerDataMonitor_Tick);
            // 
            // dataSet
            // 
            this.dataSet.DataSetName = "dsLog";
            this.dataSet.Tables.AddRange(new System.Data.DataTable[] {
            this.tbLog});
            // 
            // tbLog
            // 
            this.tbLog.Columns.AddRange(new System.Data.DataColumn[] {
            this.colSN,
            this.colDate,
            this.colTime,
            this.colPacket});
            this.tbLog.TableName = "tbLog";
            // 
            // colSN
            // 
            this.colSN.ColumnName = "序号";
            // 
            // colDate
            // 
            this.colDate.ColumnName = "日期";
            // 
            // colTime
            // 
            this.colTime.ColumnName = "时间";
            // 
            // colPacket
            // 
            this.colPacket.ColumnName = "原始报文";
            this.colPacket.DataType = typeof(byte[]);
            // 
            // DataMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.splitContainer1);
            this.Name = "DataMonitor";
            this.Size = new System.Drawing.Size(1366, 768);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.cnMenuDataList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbLog)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ListView lvDataList;
        private System.Windows.Forms.ColumnHeader 序号;
        private System.Windows.Forms.ColumnHeader 日期;
        private System.Windows.Forms.ColumnHeader 时间;
        private System.Windows.Forms.ColumnHeader 信道组;
        private System.Windows.Forms.ColumnHeader 频点;
        private System.Windows.Forms.ColumnHeader 场强值;
        private System.Windows.Forms.ColumnHeader 包长;
        private System.Windows.Forms.ColumnHeader 源地址;
        private System.Windows.Forms.ColumnHeader 目的地址;
        private System.Windows.Forms.ColumnHeader PanID;
        private System.Windows.Forms.ColumnHeader 帧序号;
        private System.Windows.Forms.ColumnHeader 帧类型;
        private System.Windows.Forms.ColumnHeader 备注;
        private System.Windows.Forms.ContextMenuStrip cnMenuDataList;
        private System.Windows.Forms.ToolStripMenuItem 保存ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 载入ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 删除ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 清空ToolStripMenuItem;
        private System.Windows.Forms.Label lbPort;
        private System.Windows.Forms.ComboBox cmbPort;
        private System.Windows.Forms.Button btClearLog;
        private System.Windows.Forms.Button btLoadLog;
        private System.Windows.Forms.Button btSaveLog;
        private System.Windows.Forms.Button btOpenPort;
        private System.Windows.Forms.ComboBox cmbChanel;
        private System.Windows.Forms.Label lbChanel;
        private System.Windows.Forms.TreeView treeVwrProtol;
        private System.Windows.Forms.Label lbProtolParse;
        private System.IO.Ports.SerialPort serialPort;
        private System.Windows.Forms.OpenFileDialog openFileDlg;
        private System.Windows.Forms.SaveFileDialog saveFileDlg;
        private System.Windows.Forms.Timer timerDataMonitor;
        private System.Data.DataSet dataSet;
        private System.Data.DataTable tbLog;
        private System.Data.DataColumn colSN;
        private System.Data.DataColumn colDate;
        private System.Data.DataColumn colTime;
        private System.Data.DataColumn colPacket;
        private System.Windows.Forms.RichTextBox rtbRxdata;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ColumnHeader 源名称;
        private System.Windows.Forms.ColumnHeader 目的名称;
        private System.Windows.Forms.Button btScroll;
        private System.Windows.Forms.Button btModuleChk;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btOpenPort2;
        private System.Windows.Forms.ComboBox combChanel2;
        private System.Windows.Forms.ComboBox combSpeed;
        private System.Windows.Forms.ComboBox combPort2;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label13;
    }
}
